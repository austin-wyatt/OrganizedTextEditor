using OrganizedTextEditor.Classes;
using OrganizedTextEditor.Controls.Sections;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Section = OrganizedTextEditor.Classes.Section;

namespace OrganizedTextEditor.Controls
{
	/// <summary>
	/// Interaction logic for SectionDisplay.xaml
	/// </summary>
	public partial class SectionDisplay : UserControl
	{
		private Project _currentProject;

		const string PORTAL_NAME = "SectionDisplay";


		private Dictionary<Id, UIElement> _idToChildMap = new Dictionary<Id, UIElement>();
		private HashSet<PropertiesBase> _highlightedItems = new HashSet<PropertiesBase>();

		public SectionDisplay(Project project)
		{
			InitializeComponent();

			_currentProject = project;
			
			LoadSections();

			contentPanel.PreviewMouseDown += (sender, e) =>
			{
				PortalFocusManager.SetFocusedPortal(PORTAL_NAME);
			};

			contentPanel.GotFocus += (s, e) =>
			{
				PortalFocusManager.SetFocusedPortal(PORTAL_NAME);
			};

			scrollComponent.PreviewMouseWheel += (s, e) =>
			{
				bool ctrlDown = Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl);

				if (ctrlDown)
				{
					e.Handled = true;

					if(e.Delta < 0)
					{
						GlobalEventManager.OnSectionContentScaled(this, GlobalEventManager.SectionContentScale - 0.1);
					}
					else
					{
						GlobalEventManager.OnSectionContentScaled(this, GlobalEventManager.SectionContentScale + 0.1);
					}
				}
			};

			GlobalEventManager.ItemAdded += GlobalEventManager_PropertiesBaseItemAdded;
			GlobalEventManager.ItemRemoved += GlobalEventManager_PropertiesBaseItemAdded;
			GlobalEventManager.ItemsMoved += GlobalEventManager_ItemsMoved;

			GlobalEventManager.SelectedItems.CollectionChanged += SelectedItems_CollectionChanged;

			GlobalEventManager.ScrollToItem += GlobalEventManager_ScrollToItem;
			GlobalEventManager.SectionFocused += GlobalEventManager_SectionFocused;

			GlobalEventManager.ItemScopeChange += GlobalEventManager_ItemScopeChange;

			GlobalEventManager.ItemExpansionChanged += GlobalEventManager_ItemExpansionChanged; 

			Unloaded += (sender, e) =>
			{
				GlobalEventManager.ItemAdded -= GlobalEventManager_PropertiesBaseItemAdded;
				GlobalEventManager.ItemRemoved -= GlobalEventManager_PropertiesBaseItemAdded;
				GlobalEventManager.ItemsMoved -= GlobalEventManager_ItemsMoved;

				GlobalEventManager.SelectedItems.CollectionChanged -= SelectedItems_CollectionChanged;

				GlobalEventManager.ScrollToItem -= GlobalEventManager_ScrollToItem;
				GlobalEventManager.SectionFocused -= GlobalEventManager_SectionFocused;

				GlobalEventManager.ItemScopeChange -= GlobalEventManager_ItemScopeChange;

				GlobalEventManager.ItemExpansionChanged -= GlobalEventManager_ItemExpansionChanged;
			};
		}

		private void GlobalEventManager_ItemExpansionChanged(object? sender, PropertiesBaseChangedEventArgs e)
		{
			LoadSections();
		}

		private void GlobalEventManager_ItemScopeChange(object? sender, PropertiesBaseChangedEventArgs e)
		{
			LoadSections();
		}

		private void GlobalEventManager_SectionFocused(object? sender, PropertiesBaseChangedEventArgs e)
		{
			if (_idToChildMap.TryGetValue(e.Item.Id, out var element))
			{
				if (element is SectionComponent sectionComponent)
				{
					sectionComponent.sectionTextBox.Focus();
				}
			}
		}

		private void GlobalEventManager_ScrollToItem(object? sender, PropertiesBaseChangedEventArgs e)
		{
			if(_idToChildMap.TryGetValue(e.Item.Id, out var child))
			{
				GeneralTransform transform = child.TransformToAncestor(scrollComponent);
				
				Rect rect = transform.TransformBounds(new Rect(new Point(0, 0), child.RenderSize));


				double location = scrollComponent.VerticalOffset;
				double destination = rect.Top + scrollComponent.VerticalOffset;


				Task.Run(() =>
				{
					double t = 0;

					while (t <= 1)
					{
						double lerpedValue = Helpers.CubicLerp(location, destination, t);

						Dispatcher.Invoke(() =>
						{
							scrollComponent.ScrollToVerticalOffset(lerpedValue);
						});

						t += 0.01;
						Thread.Sleep(10);
					}
				});
				
			}
		}

		private void SelectedItems_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
		{
			void updateSelection(PropertiesBase item, bool status)
			{
				if(_idToChildMap.TryGetValue(item.Id, out var child))
				{
					if(child is SectionComponent sectionComponent)
					{
						sectionComponent.Select(status);
					}
					else if(child is CategoryComponent categoryComponent)
					{
						categoryComponent.Select(status);
					}

					if (status)
					{
						_highlightedItems.Add(item);
					}
					else
					{
						_highlightedItems.Remove(item);
					}
				}
			}


			if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Reset)
			{
				foreach (var item in _highlightedItems)
				{
					updateSelection(item, false);
				}
			}
			else if((e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Move ||
				e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Remove ||
				e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Replace)
				&& e.OldItems != null)
			{
				foreach(var item in e.OldItems)
				{
					if(item is TreeViewItemComponent comp)
					{
						updateSelection(comp.CurrentItem, false);
					}
				}
			}
			else if(e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add
				&& e.NewItems != null)
			{
				foreach (var item in e.NewItems)
				{
					if (item is TreeViewItemComponent comp)
					{
						updateSelection(comp.CurrentItem, true);
					}
				}
			}
		}

		private void GlobalEventManager_ItemsMoved(object? sender, EventArgs e)
		{
			LoadSections();
		}

		private void GlobalEventManager_PropertiesBaseItemAdded(object? sender, PropertiesBaseChangedEventArgs e)
		{
			LoadSections();
		}

		public void LoadSections()
		{
			var root = GlobalEventManager.ScopedRoot ?? _currentProject.Root;
			var visibleItems = GlobalEventManager.CalculateVisibleChildrenOfCategoryWithInfo(root);

			_idToChildMap.Clear();

			foreach (UIElement child in contentPanel.Children)
			{
				if (child is SectionComponent sectionComponent)
				{
					_idToChildMap.Add(sectionComponent.Section.Id, sectionComponent);
				}
				else if (child is CategoryComponent categoryComponent)
				{
					_idToChildMap.Add(categoryComponent.Category.Id, categoryComponent);
				}
			}

			contentPanel.Children.Clear();

			if(root == null)
			{
				return;
			}

			foreach (var info in visibleItems)
			{
				if (info.Type == VisibleItemDisplayInfo.RecordType.ExitCategory)
				{
					//TODO: add exit category component

					//temp, add a gray label with the category name and a thin line extending to the right
					Grid grid = new Grid();
					grid.Margin = new Thickness(10, 0, 0, 10);

					grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });
					grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Auto) });
					//grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });

					//Border borderLeft = new Border();
					//borderLeft.BorderBrush = Brushes.Gray;
					//borderLeft.BorderThickness = new Thickness(0, 0, 0, 1);
					//borderLeft.VerticalAlignment = VerticalAlignment.Center;
					//borderLeft.Margin = new Thickness(5, 0, 5, 0);
					//Grid.SetColumn(borderLeft, 0);
					//grid.Children.Add(borderLeft);

					TextBlock textBlock = new TextBlock();
					textBlock.Text = "^ " + info.Item.Title;
					textBlock.FontSize = 10;
					textBlock.Foreground = Brushes.Gray;
					textBlock.VerticalAlignment = VerticalAlignment.Center;
					textBlock.Margin = new Thickness(5, 0, 5, 0);
					Grid.SetColumn(textBlock, 1);
					grid.Children.Add(textBlock);

					//Border borderRight = new Border();
					//borderRight.BorderBrush = Brushes.Gray;
					//borderRight.BorderThickness = new Thickness(0, 0, 0, 1);
					//borderRight.VerticalAlignment = VerticalAlignment.Center;
					//borderRight.Margin = new Thickness(5, 0, 5, 0);
					//Grid.SetColumn(borderRight, 2);
					//grid.Children.Add(borderRight);

					contentPanel.Children.Add(grid);
				}
				else if (info.Type == VisibleItemDisplayInfo.RecordType.EnterCategory)
				{
					
				}
				else
				{
					var item = info.Item;
					if (_idToChildMap.TryGetValue(item.Id, out var child))
					{
						contentPanel.Children.Add(child);
					}
					else
					{
						if (item is Section section)
						{
							SectionComponent sectionComponent = new SectionComponent(section);
							sectionComponent.Margin = new Thickness(0, 0, 0, 10);
							contentPanel.Children.Add(sectionComponent);
							_idToChildMap.Add(section.Id, sectionComponent);
						}
						else if (item is Category category)
						{
							CategoryComponent categoryComponent = new CategoryComponent(category);
							categoryComponent.Margin = new Thickness(0, 0, 0, 10);
							contentPanel.Children.Add(categoryComponent);
							_idToChildMap.Add(category.Id, categoryComponent);
						}
					}
				}
			}
		}


		/// <summary>
		/// TODO: this should be moved to a separate class along with the handling within the CategoryTreeView to homogenize the handling of visible items
		/// </summary>
		/// <param name="parent"></param>
		/// <returns></returns>
		//public List<PropertiesBase> CalculateAllChildrenOfCategory(Category parent)
		//{
		//	List<PropertiesBase> visibleItems = new List<PropertiesBase>();

		//	Stack<PropertiesBase> itemStack = new Stack<PropertiesBase>();

		//	//the parent will not be shown in the tree view so the children must be added to the stack outside of the main handling loop
		//	for (int i = parent.Children.Count - 1; i >= 0; i--)
		//	{
		//		itemStack.Push(parent.Children[i]);
		//	}

		//	while (itemStack.Count > 0)
		//	{
		//		var item = itemStack.Pop();

		//		visibleItems.Add(item);

		//		if (item is Category category)
		//		{
		//			for (int i = category.Children.Count - 1; i >= 0; i--)
		//			{
		//				itemStack.Push(category.Children[i]);
		//			}
		//		}
		//	}

		//	return visibleItems;
		//}
	}
}
