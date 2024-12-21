using OrganizedTextEditor.Classes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Section = OrganizedTextEditor.Classes.Section;

namespace OrganizedTextEditor.Controls
{
	/// <summary>
	/// Interaction logic for CategoryTreeView.xaml
	/// </summary>
	public partial class CategoryTreeView : UserControl
	{
		private Project _currentProject;


		

		const string PORTAL_NAME = "CategoryTreeView";

		/// <summary>
		/// The size of the font for the tree view.
		/// </summary>
		public int Size = 16;

		public CategoryTreeView(Project project)
		{
			InitializeComponent();

			_currentProject = project;

			Width = _currentProject.CategoryViewWidth;

			treeStackPanel.MouseDown += (sender, e) =>
			{
				//stop the event from propagating to the focus manager
				e.Handled = true;
			};


			treeStackPanel.PreviewMouseDown += (sender, e) =>
			{
				PortalFocusManager.SetFocusedPortal(PORTAL_NAME);
			};

			treeStackPanel.MouseWheel += (sender, e) =>
			{
				if(Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
				{
					if(e.Delta > 0)
					{
						Size++;
					}
					else
					{
						Size--;
					}

					Size = Math.Clamp(Size, 6, 32);

					GlobalEventManager.ScopeToCategory();
				}
			};

			treeStackPanel.Drop += (sender, e) =>
			{
				var obj = e.Data.GetData(e.Data.GetFormats()[0]);
				//double check that the object being dropped is a PropertiesBase object
				if (typeof(PropertiesBase).IsAssignableFrom(obj.GetType()))
				{
					if(!_checkDropIsValid(GlobalEventManager.ScopedRoot ?? _currentProject.Root))
					{
						return;
					}

					e.Handled = true;

					List<TreeViewItemComponent> itemsToMove = _getSelectedItemsToMove();

					Category parent = GlobalEventManager.ScopedRoot ?? _currentProject.Root;
					foreach (var item in itemsToMove) 
					{
						GlobalEventManager.MoveItem(item.CurrentItem, parent);
					}

					GlobalEventManager.SelectedItems.Clear();
					GlobalEventManager.ScopeToCategory();
				}
			};


			treeStackPanel.Background = Brushes.Transparent;
			treeStackPanel.AllowDrop = true;
			treeStackPanel.Focusable = true;

			treeStackPanel.GotFocus += (sender, e) =>
			{
				PortalFocusManager.SetFocusedPortal(PORTAL_NAME);
			};

			void portalFocused(string portal, string previous)
			{
				_disableDragHandle();

				if (portal == PORTAL_NAME)
				{
					foreach (var selectedItem in GlobalEventManager.SelectedItems)
						_selectItem(selectedItem);

					treeStackPanel.Focus();
					if (GlobalEventManager.SelectedItems.Count == 0 && treeStackPanel.Children.Count > 0)
					{
						GlobalEventManager.SelectedItems.Add(treeStackPanel.Children[0] as TreeViewItemComponent);
						_selectItem(GlobalEventManager.SelectedItems.First());
					}
				}
				else if(previous == PORTAL_NAME)
				{
					foreach (var selectedItem in GlobalEventManager.SelectedItems)
						_unfocusItem(selectedItem);
				}
			}

			PortalFocusManager.FocusChanged += portalFocused;
			PortalFocusManager.KeyDown += HandleKeyDown;
			
			Unloaded += (sender, e) =>
			{
				PortalFocusManager.FocusChanged -= portalFocused;
				PortalFocusManager.KeyDown -= HandleKeyDown;
			};

			ContextMenu contextMenu = new ContextMenu();

			
			MenuItem scopeItem = new MenuItem();
			scopeItem.Header = "Scope to root";
			scopeItem.Click += (sender, e) =>
			{
				GlobalEventManager.ScopeToCategory(_currentProject.Root);
			};
			contextMenu.Items.Add(scopeItem);

			//MenuItem addMenuItem = new MenuItem();
			//addMenuItem.Header = "Add";
			//contextMenu.Items.Add(addMenuItem);

			MenuItem newCategoryItem = new MenuItem();
			newCategoryItem.Icon = IconBuilder.BuildIcon("FolderClosed.png");
			newCategoryItem.Header = "New category";
			newCategoryItem.Click += (sender, e) =>
			{
				Category newCategory = new Category();
				GlobalEventManager.ScopedRoot?.Children.Add(newCategory);
				GlobalEventManager.ScopeToCategory();

				GlobalEventManager.OnPropertiesBaseItemAdded(this, newCategory);
			};
			//addMenuItem.Items.Add(newCategoryItem);
			contextMenu.Items.Add(newCategoryItem);

			MenuItem newSectionItem = new MenuItem();
			newSectionItem.Icon = IconBuilder.BuildIcon("TextElement.png");
			newSectionItem.Header = "New section";
			newSectionItem.Click += (sender, e) =>
			{
				Section newSection = new Section();
				GlobalEventManager.ScopedRoot?.Children.Add(newSection);
				GlobalEventManager.ScopeToCategory();

				GlobalEventManager.OnPropertiesBaseItemAdded(this, newSection);
			};
			//addMenuItem.Items.Add(newSectionItem);
			contextMenu.Items.Add(newSectionItem);

			MenuItem pasteItem = new MenuItem();
			pasteItem.Header = "Paste";
			pasteItem.Click += (sender, e) =>
			{
				PasteItemsFromClipboard(GlobalEventManager.ScopedRoot ?? _currentProject.Root);
			};
			contextMenu.Items.Add(pasteItem);

			treeStackPanel.ContextMenu = contextMenu;

			searchButton.Click += (sender, e) =>
			{
				searchBox.Visibility = searchBox.Visibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;

				if(searchBox.Visibility == Visibility.Collapsed)
				{
					GlobalEventManager.OnFilterItems(this, "");
				}
			};

			searchBox.TextChanged += (sender, e) =>
			{
				GlobalEventManager.OnFilterItems(this, searchBox.Text);
			};

			GlobalEventManager.ItemChanged += GlobalEventManager_PropertiesBaseItemChanged;
			GlobalEventManager.ItemAdded += GlobalEventManager_PropertiesBaseItemAdded;
			GlobalEventManager.ItemRemoved += GlobalEventManager_PropertiesBaseItemAdded;

			GlobalEventManager.ItemScopeChange += GlobalEventManager_ItemScopeChange;
			GlobalEventManager.ItemExpansionChanged += GlobalEventManager_ItemExpansionChanged;

			GlobalEventManager.ItemSelected += GlobalEventManager_ItemSelected;

			GlobalEventManager.FilterQueryChanged += GlobalEventManager_FilterQueryChanged;

			Unloaded += (sender, e) =>
			{
				GlobalEventManager.ItemChanged -= GlobalEventManager_PropertiesBaseItemChanged;
				GlobalEventManager.ItemAdded -= GlobalEventManager_PropertiesBaseItemAdded;
				GlobalEventManager.ItemRemoved -= GlobalEventManager_PropertiesBaseItemAdded;

				GlobalEventManager.ItemScopeChange -= GlobalEventManager_ItemScopeChange;
				GlobalEventManager.ItemExpansionChanged -= GlobalEventManager_ItemExpansionChanged;

				GlobalEventManager.ItemSelected -= GlobalEventManager_ItemSelected;

				GlobalEventManager.FilterQueryChanged -= GlobalEventManager_FilterQueryChanged;
			};

			GlobalEventManager.ScopeToCategory(_currentProject.Root);
		}

		private void GlobalEventManager_FilterQueryChanged(object? sender, string e)
		{
			BuildTreeView();
		}

		private void GlobalEventManager_ItemSelected(object? sender, PropertiesBaseChangedEventArgs e)
		{
			foreach(var component in treeStackPanel.Children)
			{
				if(component is TreeViewItemComponent item)
				{
					if(item.CurrentItem == e.Item)
					{
						TreeViewItemComponent_SelectedMouse(item);
						return;
					}
				}
			}
		}

		private void GlobalEventManager_ItemExpansionChanged(object? sender, PropertiesBaseChangedEventArgs e)
		{
			for (int i = 0; i < treeStackPanel.Children.Count; i++)
			{
				if (treeStackPanel.Children[i] is TreeViewItemComponent item)
				{
					if (item.CurrentItem.Id == e.Item.Id)
					{
						if (GlobalEventManager.ItemInfoMap[e.Item.Id].IsExpanded)
						{
							TreeViewItemComponent_Expanded(item);
						}
						else
						{
							TreeViewItemComponent_Collapsed(item);
						}
					}
				}
			}
		}

		private void GlobalEventManager_ItemScopeChange(object? sender, PropertiesBaseChangedEventArgs e)
		{
			BuildTreeView();

			if(e.Item is Category category)
			{
				if (category.Id == _currentProject.Root.Id)
				{
					scopeTextBlock.Text = _currentProject.Title;
				}
				else
				{
					scopeTextBlock.Text = "Category: " + category.Title;
				}
			}
		}

		private void GlobalEventManager_PropertiesBaseItemChanged(object? sender, PropertiesBaseChangedEventArgs e)
		{
			e.Handled = true;

			UpdateItem(e.Item);
		}

		private void GlobalEventManager_PropertiesBaseItemAdded(object? sender, PropertiesBaseChangedEventArgs e)
		{
			e.Handled = true;

			BuildTreeView();
		}

		private void HandleKeyDown(KeyEventArgs e, string portal)
		{
			if (portal != PORTAL_NAME)
				return;

			bool ctrlDown = Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl);
			bool actionTaken = false;

			if (e.Key == Key.Delete)
			{
				actionTaken = true;
				DeleteSelectedItems();
			}
			else if(ctrlDown && e.Key == Key.A)
			{
				actionTaken = true;
				foreach(var item in GlobalEventManager.SelectedItems)
				{
					_deselectItem(item);
				}

				GlobalEventManager.SelectedItems.Clear();

				foreach(var child in treeStackPanel.Children)
				{
					if(child is TreeViewItemComponent item)
					{
						_selectItem(item);
						GlobalEventManager.SelectedItems.Add(item);
					}
				}
			}
			else if(e.Key == Key.Escape)
			{
				actionTaken = true;
				foreach(var item in GlobalEventManager.SelectedItems)
				{
					_deselectItem(item);
				}
				GlobalEventManager.SelectedItems.Clear();
			}
			else if (ctrlDown && e.Key == Key.V)
			{
				actionTaken = true;

				PropertiesBase insertionPoint = GlobalEventManager.SelectedItems.Count > 0 ? GlobalEventManager.SelectedItems.First().CurrentItem : GlobalEventManager.ScopedRoot ?? _currentProject.Root;
				PasteItemsFromClipboard(insertionPoint);
			}

			if (GlobalEventManager.SelectedItems.Count == 0)
			{
				return;
			}

			if (e.Key == Key.Enter)
			{
				actionTaken = true;
				GlobalEventManager.OnPropertiesBaseItemInspected(this, GlobalEventManager.SelectedItems.First().CurrentItem, focus: false);
				GlobalEventManager.OnPropertiesBaseItemScroll(this, GlobalEventManager.SelectedItems.First().CurrentItem);
			}
			else if (e.Key == Key.Down)
			{
				actionTaken = true;
				//move down
				var selectedIndex = treeStackPanel.Children.IndexOf(GlobalEventManager.SelectedItems.First());
				if (selectedIndex < treeStackPanel.Children.Count - 1)
				{
					var nextItem = treeStackPanel.Children[selectedIndex + 1] as TreeViewItemComponent;
					TreeViewItemComponent_SelectedKeyboard(nextItem, Direction.Down);
				}
			}
			else if (e.Key == Key.Up)
			{
				actionTaken = true;
				//move up
				var selectedIndex = treeStackPanel.Children.IndexOf(GlobalEventManager.SelectedItems.First());
				if (selectedIndex > 0)
				{
					var previousItem = treeStackPanel.Children[selectedIndex - 1] as TreeViewItemComponent;
					TreeViewItemComponent_SelectedKeyboard(previousItem, Direction.Up);
				}
			}
			else if (e.Key == Key.Right)
			{
				actionTaken = true;
				//expand category
				foreach (var selectedItem in GlobalEventManager.SelectedItems)
				{
					if (selectedItem.CurrentItem is Category category)
					{
						if (!GlobalEventManager.ItemInfoMap[selectedItem.CurrentItem.Id].IsExpanded)
						{
							selectedItem.ToggleCategoryExpand(true);
						}
					}
				}
			}
			else if (e.Key == Key.Left)
			{
				actionTaken = true;
				//collapse category
				foreach (var selectedItem in GlobalEventManager.SelectedItems)
				{
					if (selectedItem.CurrentItem is Category category)
					{
						if (GlobalEventManager.ItemInfoMap[selectedItem.CurrentItem.Id].IsExpanded)
						{
							selectedItem.ToggleCategoryExpand(false);
						}
					}
				}
			}
			else if (ctrlDown && e.Key == Key.C)
			{
				actionTaken = true;

				List<PropertiesBase> items = GlobalEventManager.SelectedItems.Select(i => i.CurrentItem).ToList();

				CopyItemsToClipboard(PruneListOfChildren(items));
			}
			

			if(actionTaken)
			{
				e.Handled = true;
			}
		}

		private void BuildTreeView()
		{
			treeStackPanel.Children.Clear();

			List<Id> visibleItems = new List<Id>();

			if(GlobalEventManager.FilterQuery == "")
			{
				visibleItems = GlobalEventManager.CalculateVisibleChildrenOfCategory(GlobalEventManager.ScopedRoot ?? _currentProject.Root);
			}
			else
			{
				visibleItems = GlobalEventManager.CalculateFilteredChildrenOfCategory(GlobalEventManager.ScopedRoot ?? _currentProject.Root, GlobalEventManager.FilterQuery);
			}

			foreach (var itemId in visibleItems)
			{
				var itemInfo = GlobalEventManager.ItemInfoMap[itemId];

				var component = new TreeViewItemComponent(itemInfo.Item, itemInfo.IndentationLevel, itemInfo.IsExpanded, Size);

				AssignTreeViewItemComponentHandlers(component);

				treeStackPanel.Children.Add(component);
			}

			if(treeStackPanel.Children.Count > 0 && treeStackPanel.Children[0] is TreeViewItemComponent comp)
			{
				comp.Margin = new Thickness(0, 0, 0, 0);
			}
		}

		private void TreeViewItemComponent_Expanded(TreeViewItemComponent component)
		{
			PropertiesBase item = component.CurrentItem;

			if (item is Category category)
			{
				var children = GlobalEventManager.CalculateVisibleChildrenOfCategory(category);
				//find the index of the item in the stack panel
				int index = treeStackPanel.Children.IndexOf(component);

				foreach (var itemId in children)
				{
					var itemInfo = GlobalEventManager.ItemInfoMap[itemId];
					var newComponent = new TreeViewItemComponent(itemInfo.Item, itemInfo.IndentationLevel, itemInfo.IsExpanded, Size);

					AssignTreeViewItemComponentHandlers(newComponent);

					treeStackPanel.Children.Insert(index + 1, newComponent);
					index++;
				}
			}
		}

		private void TreeViewItemComponent_Collapsed(TreeViewItemComponent component)
		{
			PropertiesBase item = component.CurrentItem;

			if(item is Category category)
			{
				var children = GlobalEventManager.CalculateVisibleChildrenOfCategory(category).ToHashSet();

				int minIndex = int.MaxValue;
				int maxIndex = int.MinValue;

				for(int i = 0; i < treeStackPanel.Children.Count; i++)
				{
					var childComponent = treeStackPanel.Children[i];

					if(childComponent is TreeViewItemComponent child)
					{
						if(children.Contains(child.CurrentItem.Id))
						{
							minIndex = Math.Min(minIndex, i);
							maxIndex = Math.Max(maxIndex, i);
						}
					}
				}

				if(minIndex != int.MaxValue && maxIndex != int.MinValue)
				{
					treeStackPanel.Children.RemoveRange(minIndex, maxIndex - minIndex + 1);
				}
			}
		}


		private TreeViewItemComponent? _dragHoveredItem = null;
		private void _disableDragHandle()
		{
			if(_dragHoveredItem != null)
			{
				_dragHoveredItem.DisplayDragHandle = false;
				_dragHoveredItem.BuildControl();
				_dragHoveredItem = null;
			}
		}

		private bool _checkDropIsValid(PropertiesBase item)
		{
			HashSet<PropertiesBase> selectedItemsSet = GlobalEventManager.SelectedItems.Select(i => i.CurrentItem).ToHashSet();

			if (selectedItemsSet.Contains(item))
			{
				//don't allow an item to be dropped on itself
				return false;
			}

			//walk all parents of the component to check if any of the selected items are parents of the component
			PropertiesBase? checkParent = item;
			while (checkParent != null)
			{
				if (selectedItemsSet.Contains(checkParent))
				{
					//don't allow an item to be dropped on one of its children
					return false;
				}

				GlobalEventManager.ItemInfoMap.TryGetValue(GlobalEventManager.ItemInfoMap[checkParent.Id].ParentId, out var parentInfo);
				checkParent = parentInfo?.Item;
			}

			return true;
		}

		List<TreeViewItemComponent> _getSelectedItemsToMove()
		{
			HashSet<PropertiesBase> selectedItemsSet = GlobalEventManager.SelectedItems.Select(i => i.CurrentItem).ToHashSet();

			List<TreeViewItemComponent> itemsToMove = new List<TreeViewItemComponent>();
			foreach (var selectedItem in GlobalEventManager.SelectedItems)
			{
				GlobalEventManager.ItemInfoMap.TryGetValue(GlobalEventManager.ItemInfoMap[selectedItem.CurrentItem.Id].ParentId, out var oldParentInfo);
				if (selectedItemsSet.Contains(oldParentInfo.Item))
				{
					//don't allow an item to be dropped on one of its children
					continue;
				}

				itemsToMove.Add(selectedItem);
			}

			return itemsToMove;
		}

		private void AssignTreeViewItemComponentHandlers(TreeViewItemComponent item)
		{
			item.ItemClicked += (item) => TreeViewItemComponent_SelectedMouse(item);

			#region Item context menu handling
			item.ItemRightClicked += (component) =>
			{
				if (!GlobalEventManager.SelectedItems.Contains(component))
				{
					TreeViewItemComponent_SelectedMouse(component, isRightClick: true);
				}

				var contextMenu = new ContextMenu();
				
				if(GlobalEventManager.SelectedItems.Count > 1)
				{
					MenuItem expand = new MenuItem();
					expand.Header = "Expand all";
					expand.Click += (sender, e) =>
					{
						foreach(var selectedItem in GlobalEventManager.SelectedItems)
						{
							if(selectedItem.CurrentItem is Category category)
							{
								selectedItem.ToggleCategoryExpand(true);
							}
						}
					};
					contextMenu.Items.Add(expand);

					MenuItem collapse = new MenuItem();
					collapse.Header = "Collapse all";
					collapse.Click += (sender, e) =>
					{
						foreach(var selectedItem in GlobalEventManager.SelectedItems)
						{
							if(selectedItem.CurrentItem is Category category)
							{
								selectedItem.ToggleCategoryExpand(false);
							}
						}
					};
					contextMenu.Items.Add(collapse);

					MenuItem delete = new MenuItem();
					delete.Header = "Delete";
					delete.Click += (sender, e) =>
					{
						DeleteSelectedItems();
					};
					contextMenu.Items.Add(delete);
				}
				else
				{
					MenuItem copyItem = new MenuItem();
					copyItem.Header = "Copy";
					copyItem.Click += (sender, e) =>
					{
						List<PropertiesBase> items = GlobalEventManager.SelectedItems.Select(i => i.CurrentItem).ToList();

						CopyItemsToClipboard(PruneListOfChildren(items));
					};
					contextMenu.Items.Add(copyItem);

					MenuItem pasteItem = new MenuItem();
					pasteItem.Header = "Paste";
					pasteItem.Click += (sender, e) =>
					{
						PasteItemsFromClipboard(component.CurrentItem);
					};
					contextMenu.Items.Add(pasteItem);

					if (component.CurrentItem is Category category)
					{
						//category context menu
						MenuItem scopeItem = new MenuItem();
						scopeItem.Header = "Scope to category";
						scopeItem.Click += (sender, e) =>
						{
							GlobalEventManager.ScopeToCategory(category);
						};
						contextMenu.Items.Add(scopeItem);

						MenuItem expand = new MenuItem();
						expand.Header = "Expand";
						expand.Click += (sender, e) =>
						{
							component.ToggleCategoryExpand(true);
						};
						contextMenu.Items.Add(expand);

						MenuItem collapse = new MenuItem();
						collapse.Header = "Collapse";
						collapse.Click += (sender, e) =>
						{
							component.ToggleCategoryExpand(false);
						};
						contextMenu.Items.Add(collapse);

						MenuItem insertSection = new MenuItem();
						insertSection.Header = "Add section";
						insertSection.Click += (sender, e) =>
						{
							Section newSection = new Section();
							category.Children.Add(newSection);
							GlobalEventManager.ScopeToCategory();

							GlobalEventManager.OnPropertiesBaseItemAdded(this, newSection);
						};
						contextMenu.Items.Add(insertSection);

						MenuItem insertCategory = new MenuItem();
						insertCategory.Header = "Add category";
						insertCategory.Click += (sender, e) =>
						{
							Category newCategory = new Category();
							category.Children.Add(newCategory);
							GlobalEventManager.ScopeToCategory();

							GlobalEventManager.OnPropertiesBaseItemAdded(this, newCategory);
						};
						contextMenu.Items.Add(insertCategory);
					}
					else
					{
						//section context menu
						MenuItem insertSection = new MenuItem();
						insertSection.Header = "Insert section";

						insertSection.Click += (sender, e) =>
						{
							Section newSection = new Section();
							var parent = GlobalEventManager.ItemInfoMap[GlobalEventManager.ItemInfoMap[component.CurrentItem.Id].ParentId];
							if (parent.Item is Category category)
							{
								category.Children.Insert(category.Children.IndexOf(component.CurrentItem) + 1, newSection);
							}

							GlobalEventManager.ScopeToCategory();
						};
						contextMenu.Items.Add(insertSection);

						MenuItem insertCategory = new MenuItem();
						insertCategory.Header = "Insert category";

						insertCategory.Click += (sender, e) =>
						{
							Category newCategory = new Category();
							var parent = GlobalEventManager.ItemInfoMap[GlobalEventManager.ItemInfoMap[component.CurrentItem.Id].ParentId];
							if (parent.Item is Category category)
							{
								category.Children.Insert(category.Children.IndexOf(component.CurrentItem) + 1, newCategory);
							}

							GlobalEventManager.ScopeToCategory();
						};
						contextMenu.Items.Add(insertCategory);
					}

					MenuItem delete = new MenuItem();
					delete.Header = "Delete";
					delete.Click += (sender, e) =>
					{
						DeleteSelectedItems();
					};
					contextMenu.Items.Add(delete);

					MenuItem inspect = new MenuItem();
					inspect.Header = "Properties";
					inspect.Click += (sender, e) =>
					{
						GlobalEventManager.OnPropertiesBaseItemInspected(this, component.CurrentItem, focus: true);
					};
					contextMenu.Items.Add(inspect);
				}

				component.ContextMenu = contextMenu;
				contextMenu.IsOpen = true;

				component.ContextMenu = null;
			};
			#endregion

			item.SelectionDroppedOnItem += (component, handleType) =>
			{
				_disableDragHandle();

				if (component == null) return;

				if(!_checkDropIsValid(component.CurrentItem))
				{
					return;
				}


				Category parent;
				int insertionIndex;
				if(component.CurrentItem is Category category)
				{
					if(handleType == TreeViewItemComponent.DragHandleType.FolderHandle)
					{
						GlobalEventManager.ItemInfoMap.TryGetValue(GlobalEventManager.ItemInfoMap[category.Id].ParentId, out var parentInfo);
						if(parentInfo?.Item is Category categoryParent)
						{
							parent = categoryParent;
							insertionIndex = categoryParent.Children.IndexOf(category) + 1;
						}
						else
						{
							return;
						}
					}
					else
					{
						parent = category;
						insertionIndex = 0;
					}
				}
				else
				{
					GlobalEventManager.ItemInfoMap.TryGetValue(GlobalEventManager.ItemInfoMap[component.CurrentItem.Id].ParentId, out var parentInfo);
					if (parentInfo.Item is Category categoryParent)
					{
						parent = categoryParent;
						insertionIndex = categoryParent.Children.IndexOf(component.CurrentItem) + 1;
					}
					else
					{
						return;
					}
				}

				HashSet<PropertiesBase> selectedItemsSet = GlobalEventManager.SelectedItems.Select(i => i.CurrentItem).ToHashSet();

				List<TreeViewItemComponent> itemsToMove = _getSelectedItemsToMove();

				if (itemsToMove.Count == 1 && parent.Id == GlobalEventManager.ItemInfoMap[itemsToMove[0].CurrentItem.Id].ParentId && handleType == TreeViewItemComponent.DragHandleType.None)
				{
					//if the items are adjacent, swap them
					int currentIndex = parent.Children.IndexOf(itemsToMove[0].CurrentItem);
					if(insertionIndex == currentIndex)
					{
						insertionIndex = currentIndex - 1;
					}
				}

				foreach (var selectedItem in itemsToMove)
				{
					GlobalEventManager.ItemInfoMap.TryGetValue(GlobalEventManager.ItemInfoMap[selectedItem.CurrentItem.Id].ParentId, out var oldParentInfo);
					if (oldParentInfo.Item is Category oldParent)
					{
						oldParent.Children.Remove(selectedItem.CurrentItem);
					}

					GlobalEventManager.ItemInfoMap[selectedItem.CurrentItem.Id].ParentId = parent.Id;

					if(insertionIndex > parent.Children.Count)
					{
						insertionIndex = parent.Children.Count;
					}
					else if(insertionIndex < 0)
					{
						insertionIndex = 0;
					}

					parent.Children.Insert(insertionIndex++, selectedItem.CurrentItem);

					GlobalEventManager.OnPropertiesBaseItemChanged(this, selectedItem.CurrentItem);
				}

				GlobalEventManager.OnPropertiesBaseItemsMoved(this);

				GlobalEventManager.SelectedItems.Clear();
				GlobalEventManager.ScopeToCategory();
			};

			item.DragEnter += (sender, e) =>
			{
				_disableDragHandle();

				var obj = e.Data.GetData(e.Data.GetFormats()[0]);
				//double check that the object being dropped is a PropertiesBase object before invoking the event
				if (obj is PropertiesBase component)
				{
					if(component.Id != item.CurrentItem.Id)
					{
						_dragHoveredItem = item;

						item.DisplayDragHandle = true;
						item.BuildControl();
					}
				}
			};

			item.DragEnded += () =>
			{
				_disableDragHandle();
			};

			item.Unloaded += (sender, e) =>
			{
				if(GlobalEventManager.SelectedItems.Contains(item))
				{
					GlobalEventManager.SelectedItems.Remove(item);
				}
			};
		}

		#region Selection helpers

		enum Direction
		{
			Up,
			Down
		}

		Brush _selectedColor = new SolidColorBrush(Color.FromRgb(204, 232, 255));
		Brush _unfocusedColor = Brushes.LightGray;
		Brush _borderColor = new SolidColorBrush(Color.FromRgb(0, 122, 216));

		void _deselectItem(TreeViewItemComponent selectedItem)
		{
			selectedItem.contentBorder.Background = Brushes.Transparent;
			selectedItem.contentBorder.BorderBrush = Brushes.Transparent;
			selectedItem.IsSelected = false;
		}

		void _selectItem(TreeViewItemComponent selectedItem)
		{
			selectedItem.contentBorder.Background = Theme.SelectedColor;
			selectedItem.contentBorder.BorderBrush = Theme.CurrentTheme == Theme.ThemeType.Light ? _borderColor : Theme.DarkOutlineColor;
			selectedItem.IsSelected = true;
		}

		void _unfocusItem(TreeViewItemComponent selectedItem)
		{
			if(Theme.CurrentTheme == Theme.ThemeType.Light)
			{
				selectedItem.contentBorder.Background = _unfocusedColor;
				selectedItem.contentBorder.BorderBrush = Brushes.DarkGray;
			}
		}

		private void TreeViewItemComponent_SelectedMouse(TreeViewItemComponent item, bool isRightClick = false)
		{
			var shiftDown = Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift);
			var ctrlDown = Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl);

			if ((!shiftDown && !ctrlDown) || GlobalEventManager.SelectedItems.Count == 0 || isRightClick) 
			{
				foreach (var selectedItem in GlobalEventManager.SelectedItems)
				{
					_deselectItem(selectedItem);
				}

				GlobalEventManager.SelectedItems.Clear();

				_selectItem(item);

				GlobalEventManager.SelectedItems.Add(item);
			}
			else if (ctrlDown)
			{
				if (GlobalEventManager.SelectedItems.Contains(item))
				{
					_deselectItem(item);
					GlobalEventManager.SelectedItems.Remove(item);
				}
				else
				{
					_selectItem(item);
					GlobalEventManager.SelectedItems.Insert(0, item);
				}
			}
			else if (shiftDown)
			{
				int firstItemIndex = treeStackPanel.Children.IndexOf(GlobalEventManager.SelectedItems[0]);
				int currentItemIndex = treeStackPanel.Children.IndexOf(item);

				int minIndex = Math.Min(firstItemIndex, currentItemIndex);
				int maxIndex = Math.Max(firstItemIndex, currentItemIndex);

				foreach (var selectedItem in GlobalEventManager.SelectedItems)
				{
					_deselectItem(selectedItem);
				}

				var firstItem = GlobalEventManager.SelectedItems[0];

				GlobalEventManager.SelectedItems.Clear();

				GlobalEventManager.SelectedItems.Add(firstItem);

				for (int i = minIndex; i <= maxIndex; i++)
				{
					if (treeStackPanel.Children[i] is TreeViewItemComponent component)
					{
						_selectItem(component);

						if (i != firstItemIndex)
						{
							GlobalEventManager.SelectedItems.Add(component);
						}
					}
				}
			}
		}

		private void TreeViewItemComponent_SelectedKeyboard(TreeViewItemComponent item, Direction moveDirection)
		{
			var shiftDown = Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift);

			if ((!shiftDown) || GlobalEventManager.SelectedItems.Count == 0)
			{
				foreach (var selectedItem in GlobalEventManager.SelectedItems)
				{
					_deselectItem(selectedItem);
				}

				GlobalEventManager.SelectedItems.Clear();

				_selectItem(item);

				GlobalEventManager.SelectedItems.Add(item);
			}
			else if (shiftDown)
			{
				int firstItemIndex = treeStackPanel.Children.IndexOf(GlobalEventManager.SelectedItems[0]);
				int currentItemIndex = treeStackPanel.Children.IndexOf(item);

				int minIndex = int.MaxValue;
				int maxIndex = int.MinValue;

				HashSet<TreeViewItemComponent> items = GlobalEventManager.SelectedItems.ToHashSet();
				for(int i = 0; i < treeStackPanel.Children.Count; i++)
				{
					if(treeStackPanel.Children[i] is TreeViewItemComponent component)
					{
						if(items.Contains(component))
						{
							minIndex = Math.Min(minIndex, i);
							maxIndex = Math.Max(maxIndex, i);
						}
					}
				}

				foreach (var selectedItem in GlobalEventManager.SelectedItems)
				{
					_deselectItem(selectedItem);
				}

				bool oneOrZeroItems = GlobalEventManager.SelectedItems.Count <= 1;

				GlobalEventManager.SelectedItems.Clear();
				GlobalEventManager.SelectedItems.Add(item);

				void selectItems(int minBound, int maxBound)
				{
					for (int i = minBound; i <= maxBound; i++)
					{
						if (treeStackPanel.Children[i] is TreeViewItemComponent component)
						{
							_selectItem(component);

							if (i != currentItemIndex)
								GlobalEventManager.SelectedItems.Add(component);
						}
					}
				}

				if((moveDirection == Direction.Down && currentItemIndex == minIndex + 1 && !oneOrZeroItems) || (moveDirection == Direction.Up && currentItemIndex == minIndex - 1))
				{
					//deselect the topmost item or select the topmost item depending on direction
					selectItems(currentItemIndex, maxIndex);
				}
				else if((moveDirection == Direction.Up && currentItemIndex == maxIndex - 1 && !oneOrZeroItems) || (moveDirection == Direction.Down && currentItemIndex == maxIndex + 1))
				{
					//deselect the bottommost item or select the bottommost item depending on direction
					selectItems(minIndex, currentItemIndex);
				}
                else
                {
                    _selectItem(item);
                }
            }
		}
		#endregion

		public void DeleteSelectedItems()
		{
			string message = GlobalEventManager.SelectedItems.Count > 1 ? "Are you sure you want to delete the selected items?" : "Are you sure you want to delete the item " + GlobalEventManager.SelectedItems[0].CurrentItem.Title + "?";

			if (MessageBox.Show(message, $"Delete item{(GlobalEventManager.SelectedItems.Count > 1 ? "s" : "")}", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
			{
				foreach (var selectedItem in GlobalEventManager.SelectedItems)
				{
					GlobalEventManager.ItemInfoMap.TryGetValue(GlobalEventManager.ItemInfoMap[selectedItem.CurrentItem.Id].ParentId, out var parent);
					if (parent != null && parent.Item is Category category)
					{
						category.Children.Remove(selectedItem.CurrentItem);

						GlobalEventManager.OnPropertiesBaseItemRemoved(this, selectedItem.CurrentItem);
					}

					GlobalEventManager.ItemInfoMap.Remove(selectedItem.CurrentItem.Id);
				}

				GlobalEventManager.SelectedItems.Clear();

				GlobalEventManager.ScopeToCategory();
			}
		}


		public void UpdateItem(PropertiesBase item)
		{
			var component = treeStackPanel.Children.OfType<TreeViewItemComponent>().FirstOrDefault(c => c.CurrentItem.Id == item.Id);

			if(component != null)
			{
				component.BuildControl();
			}
		}

		public List<PropertiesBase> PruneListOfChildren(List<PropertiesBase> items)
		{
			//get a list of all selected items but exclude items that are children of other selected items
			HashSet<PropertiesBase> representedChildren = new HashSet<PropertiesBase>();

			foreach (var selectedItem in items)
			{
				if (representedChildren.Contains(selectedItem))
					continue;

				if (selectedItem is Category category)
				{
					Stack<PropertiesBase> itemStack = new Stack<PropertiesBase>();

					itemStack.Push(selectedItem);
					bool first = true;

					while (itemStack.Count > 0)
					{
						var item = itemStack.Pop();

						if (!first)
							representedChildren.Add(item);
						else 
							first = false;

						if (item is Category cat)
						{
							foreach (var child in cat.Children)
							{
								itemStack.Push(child);
							}
						}
					}
				}
			}

			List<PropertiesBase> itemsToCopy = new();
			foreach (var selectedItem in GlobalEventManager.SelectedItems)
			{
				if (!representedChildren.Contains(selectedItem.CurrentItem))
				{
					itemsToCopy.Add(selectedItem.CurrentItem);
				}
			}

			return itemsToCopy;
		}

		public void CopyItemsToClipboard(List<PropertiesBase> items)
		{
			string jsonStr = Serializer.Serialize(items);

			Clipboard.SetText(jsonStr);
		}

		public void PasteItemsFromClipboard(PropertiesBase insertionPoint)
		{
			string jsonStr = Clipboard.GetText();

			List<PropertiesBase> items = Serializer.Deserialize<List<PropertiesBase>>(jsonStr);

			if(items != null)
			{
				Stack<PropertiesBase> itemStack = new Stack<PropertiesBase>(items);

				//assign new ids to all items
				while (itemStack.Count > 0)
				{
					var item = itemStack.Pop();

					item.Id = new Id();

					if(item is Category cat)
					{
						foreach(var child in cat.Children)
						{
							itemStack.Push(child);
						}
					}
				}

				//insert the items into the tree
				if(insertionPoint is Category category)
				{
					foreach(var item in items)
					{
						category.Children.Add(item);
					}
				}
				else if(insertionPoint is Section section)
				{
					var parent = GlobalEventManager.GetParent(section);
					if (parent != null)
					{
						int index = parent.Children.IndexOf(section);
						foreach(var item in items)
						{
							parent.Children.Insert(index++, item);
						}
					}
				}

				GlobalEventManager.ScopeToCategory();
			}
		}
	}
}
