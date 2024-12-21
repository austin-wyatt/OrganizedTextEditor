using OrganizedTextEditor.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static OrganizedTextEditor.Controls.CategoryTreeView;

namespace OrganizedTextEditor.Classes
{
	public class PropertiesBaseChangedEventArgs : EventArgs
	{
		public bool Handled { get; set; }
		public PropertiesBase Item { get; set; }
	}

	public class ItemInfo
	{
		public PropertiesBase Item;
		public Id ParentId;
		public int IndentationLevel;
		public bool IsExpanded;


		public ItemInfo(PropertiesBase item, int indentationLevel, Id parent)
		{
			Item = item;
			IndentationLevel = indentationLevel;
			IsExpanded = false;
			ParentId = parent;
		}
	}

	public class VisibleItemDisplayInfo 
	{
		public enum RecordType
		{
			PropertiesBase,
			EnterCategory,
			ExitCategory
		}

		public RecordType Type;
		public PropertiesBase Item;
	}

	public static class GlobalEventManager
	{
		//private static List<Id> VisibleItems = new(); 

		//public static void SetVisibleItems(List<Id> items)
		//{
		//	VisibleItems = items;
		//	VisibleItemsChanged?.Invoke(null, EventArgs.Empty);
		//}

		//public static IReadOnlyCollection<Id> GetVisibleItems()
		//{
		//	return VisibleItems.AsReadOnly();
		//}

		public static Category? ScopedRoot = null;
		public static Dictionary<Id, ItemInfo> ItemInfoMap = new Dictionary<Id, ItemInfo>();
		public static ObservableCollection<TreeViewItemComponent> SelectedItems = new ObservableCollection<TreeViewItemComponent>();

		public static double SectionContentScale = 1.0;

		public static string FilterQuery = "";

		public static List<Id> CalculateVisibleChildrenOfCategory(Category parent)
		{
			List<Id> visibleItems = new List<Id>();

			Stack<PropertiesBase> itemStack = new Stack<PropertiesBase>();

			//the parent will not be shown in the tree view so the children must be added to the stack outside of the main handling loop
			for (int i = parent.Children.Count - 1; i >= 0; i--)
			{
				itemStack.Push(parent.Children[i]);
			}

			while (itemStack.Count > 0)
			{
				var item = itemStack.Pop();

				visibleItems.Add(item.Id);

				if (item is Category category)
				{
					if (ItemInfoMap[item.Id].IsExpanded)
					{
						for (int i = category.Children.Count - 1; i >= 0; i--)
						{
							itemStack.Push(category.Children[i]);
						}
					}
				}
			}

			return visibleItems;
		}

		public static List<VisibleItemDisplayInfo> CalculateVisibleChildrenOfCategoryWithInfo(Category parent)
		{
			List<VisibleItemDisplayInfo> visibleItems = new List<VisibleItemDisplayInfo>();

			Stack<PropertiesBase> itemStack = new Stack<PropertiesBase>();

			//the parent will not be shown in the tree view so the children must be added to the stack outside of the main handling loop
			for (int i = parent.Children.Count - 1; i >= 0; i--)
			{
				itemStack.Push(parent.Children[i]);
			}

			while (itemStack.Count > 0)
			{
				var item = itemStack.Pop();


				//TODO, replace this _____Exit Category hack with a better solution
				//(use VisibleItemDisplayInfo for the itemStack as well)
				if(item.Title != "_____Exit Category")
				{
					visibleItems.Add(new()
					{
						Type = VisibleItemDisplayInfo.RecordType.PropertiesBase,
						Item = item
					});
				}

				if (item is Category category)
				{
					if(item.Title == "_____Exit Category")
					{
						visibleItems.Add(new()
						{
							Type = VisibleItemDisplayInfo.RecordType.ExitCategory,
							Item = (item as Category).Children[0]
						});
					}
					else if (ItemInfoMap[item.Id].IsExpanded)
					{
						itemStack.Push(new Category() { Title = "_____Exit Category", Children = { category } });

						for (int i = category.Children.Count - 1; i >= 0; i--)
						{
							itemStack.Push(category.Children[i]);
						}
					}
				}
			}

			return visibleItems;
		}

		public static List<Id> CalculateFilteredChildrenOfCategory(Category parent, string filter)
		{
			List<Id> visibleItems = new List<Id>();

			Stack<PropertiesBase> itemStack = new Stack<PropertiesBase>();

			//the parent will not be shown in the tree view so the children must be added to the stack outside of the main handling loop
			for (int i = parent.Children.Count - 1; i >= 0; i--)
			{
				itemStack.Push(parent.Children[i]);
			}

			while (itemStack.Count > 0)
			{
				var item = itemStack.Pop();

				bool found = false;

				found = found || item.Title.Contains(filter, StringComparison.OrdinalIgnoreCase);

				found = found || item.Description.Contains(filter, StringComparison.OrdinalIgnoreCase);

				if (!found && item is Section section)
				{
					found = section.Text.Contains(filter, StringComparison.OrdinalIgnoreCase);
				}

				if (found)
				{
					visibleItems.Add(item.Id);
				}

				if (item is Category category)
				{
					for (int i = category.Children.Count - 1; i >= 0; i--)
					{
						itemStack.Push(category.Children[i]);
					}
				}
			}

			return visibleItems;
		}

		public static void BuildScopedInfoMap(Category parent, int indentationLevel, Category? prevParent = null)
		{
			if (ItemInfoMap.TryGetValue(parent.Id, out var existingInfo))
			{
				existingInfo.IndentationLevel = indentationLevel;
			}
			else
			{
				var parentInfo = new ItemInfo(parent, indentationLevel, prevParent?.Id ?? Id.EMPTY_ID);
				ItemInfoMap.Add(parent.Id, parentInfo);
			}

			foreach (var child in parent.Children)
			{
				var childCategory = child as Category;

				if (childCategory != null)
				{
					BuildScopedInfoMap(childCategory, indentationLevel + 1, parent);
				}
				else
				{
					if (ItemInfoMap.TryGetValue(child.Id, out var existingChildInfo))
					{
						existingChildInfo.IndentationLevel = indentationLevel + 1;
					}
					else
					{
						var childInfo = new ItemInfo(child, indentationLevel + 1, parent.Id);
						ItemInfoMap.Add(child.Id, childInfo);
					}
				}
			}
		}

		public static void ScopeToCategory(Category? category = null)
		{
			if(category == null)
			{
				category = ScopedRoot ?? Editor.ActiveProject?.Root;

				if(category == null)
					return;
			}

			ScopedRoot = category;

			BuildScopedInfoMap(ScopedRoot, -1);
			SelectedItems.Clear();


			ItemScopeChange?.Invoke(null, new PropertiesBaseChangedEventArgs() { Item = category });
		}

		public static void MoveItem(PropertiesBase item, Category newParent)
		{
			//remove the item from its current parent
			ItemInfoMap.TryGetValue(ItemInfoMap[item.Id].ParentId, out var oldParentInfo);
			if (oldParentInfo.Item is Category oldParent)
			{
				oldParent.Children.Remove(item);
			}

			ItemInfoMap[item.Id].ParentId = newParent.Id;

			newParent.Children.Add(item);

			OnPropertiesBaseItemChanged(null, item);
		}

		public static void ChangeItemExpansion(PropertiesBase item, bool isExpanded)
		{
			if (ItemInfoMap.TryGetValue(item.Id, out var info))
			{
				if(info.IsExpanded != isExpanded)
				{
					info.IsExpanded = isExpanded;
					ItemExpansionChanged?.Invoke(null, new PropertiesBaseChangedEventArgs() { Item = item });
				}
			}
		}

		public static Category? GetParent(PropertiesBase item)
		{
			var itemInfo = ItemInfoMap[item.Id];

			if(itemInfo.ParentId == Id.EMPTY_ID)
				return Editor.ActiveProject?.Root;

			var parentInfo = ItemInfoMap[itemInfo.ParentId];
			if(parentInfo.Item is Category parent)
				return parent;

			return null;
		}
		


		//public static event EventHandler? VisibleItemsChanged;

		public static event EventHandler<PropertiesBaseChangedEventArgs>? ItemChanged;
		public static event EventHandler<PropertiesBaseChangedEventArgs>? ItemAdded;
		public static event EventHandler<PropertiesBaseChangedEventArgs>? ItemRemoved;
		public static event EventHandler<PropertiesBaseChangedEventArgs>? ItemExpansionChanged;

		public static event EventHandler<EventArgs>? ItemsMoved;

		public static event EventHandler<PropertiesBaseChangedEventArgs>? ItemInspected;
		public static event EventHandler<PropertiesBaseChangedEventArgs>? ItemInspectedNoFocus;

		public static event EventHandler<PropertiesBaseChangedEventArgs>? ItemScopeChange;

		public static event EventHandler<PropertiesBaseChangedEventArgs>? ScrollToItem;
		public static event EventHandler<PropertiesBaseChangedEventArgs>? SectionFocused;

		public static event EventHandler<PropertiesBaseChangedEventArgs>? ItemSelected;

		public static event EventHandler<string>? FilterQueryChanged;


		/// <summary>
		/// Scales the content in the section display by the given factor.
		/// </summary>
		public static event EventHandler<double>? SectionContentScaled;

		public static void OnPropertiesBaseItemChanged(object sender, PropertiesBase item)
		{
			if (ItemInfoMap.TryGetValue(item.Id, out var itemInfo))
			{
				itemInfo.Item = item;
			}

			PropertiesBaseChangedEventArgs args = new PropertiesBaseChangedEventArgs();
			args.Item = item;
			ItemChanged?.Invoke(sender, args);
		}

		public static void OnPropertiesBaseItemsMoved(object sender)
		{
			ItemsMoved?.Invoke(sender, EventArgs.Empty);
		}

		public static void OnPropertiesBaseItemAdded(object sender, PropertiesBase item)
		{
			PropertiesBaseChangedEventArgs args = new PropertiesBaseChangedEventArgs();
			args.Item = item;
			ItemAdded?.Invoke(sender, args);
		}

		public static void OnPropertiesBaseItemRemoved(object sender, PropertiesBase item)
		{
			PropertiesBaseChangedEventArgs args = new PropertiesBaseChangedEventArgs();
			args.Item = item;
			ItemRemoved?.Invoke(sender, args);
		}

		public static void OnPropertiesBaseItemInspected(object sender, PropertiesBase item, bool focus)
		{
			PropertiesBaseChangedEventArgs args = new PropertiesBaseChangedEventArgs();
			args.Item = item;

			if(focus)
				ItemInspected?.Invoke(sender, args);
			else
				ItemInspectedNoFocus?.Invoke(sender, args);
		}

		public static void OnPropertiesBaseItemScopeChange(object sender, PropertiesBase item)
		{
			PropertiesBaseChangedEventArgs args = new PropertiesBaseChangedEventArgs();
			args.Item = item;
			ItemScopeChange?.Invoke(sender, args);
		}

		public static void OnPropertiesBaseItemScroll(object sender, PropertiesBase item)
		{
			PropertiesBaseChangedEventArgs args = new PropertiesBaseChangedEventArgs();
			args.Item = item;
			ScrollToItem?.Invoke(sender, args);
		}

		public static void OnSectionContentScaled(object sender, double scale)
		{
			scale = Math.Clamp(scale, 0.1, 5);

			SectionContentScale = scale;

			SectionContentScaled?.Invoke(sender, scale);
		}

		public static void OnFocusSection(PropertiesBase item)
		{ 
			SectionFocused?.Invoke(null, new PropertiesBaseChangedEventArgs() { Item = item });
		}

		public static void OnSelectItem(object sender, PropertiesBase item)
		{
			PropertiesBaseChangedEventArgs args = new PropertiesBaseChangedEventArgs();
			args.Item = item;
			ItemSelected?.Invoke(sender, args);
		}

		public static void OnFilterItems(object sender, string query)
		{
			FilterQuery = query;
			FilterQueryChanged?.Invoke(sender, query);
		}
	}
}
