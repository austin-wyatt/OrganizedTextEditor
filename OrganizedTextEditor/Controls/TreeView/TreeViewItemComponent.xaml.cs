using OrganizedTextEditor.Classes;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Printing;
using System.Runtime.CompilerServices;
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

namespace OrganizedTextEditor.Controls
{
	/// <summary>
	/// Interaction logic for TreeViewItemComponent.xaml
	/// </summary>
	public partial class TreeViewItemComponent : UserControl
	{
		public PropertiesBase CurrentItem;
		public bool IsExpanded = false;
		public int IndentationLevel = 0;
		public int Size = 16;
		public bool IsSelected = false;

		private Image? _expandHandle = null;
		public bool DisplayDragHandle = false;

		public enum DragHandleType
		{
			None,
			Handle,
			FolderHandle
		}

		public TreeViewItemComponent(PropertiesBase item, int indentationLevel, bool expanded, int size)
		{
			InitializeComponent();
			CurrentItem = item;
			IsExpanded = expanded;
			IndentationLevel = indentationLevel;
			Size = size;
			Margin = new Thickness(0, -2, 0, 0);
			

			contentGrid.AllowDrop = true;
			//handleGrid.AllowDrop = true;

			contentBorder.BorderBrush = Brushes.Transparent;

			contentBorder.MouseLeftButtonDown += (sender, e) =>
			{
				e.Handled = true;

                if (e.ClickCount == 2)
                {
					//ToggleCategoryExpand();
					GlobalEventManager.OnPropertiesBaseItemInspected(this, CurrentItem, focus: false);
					GlobalEventManager.OnPropertiesBaseItemScroll(this, CurrentItem);
				}
				else if (!IsSelected)
				{
					ItemClicked?.Invoke(this);
				}
			};

			contentBorder.MouseLeftButtonUp += (sender, e) =>
			{
				e.Handled = true;

				if (e.ClickCount == 1 && IsSelected)
				{
					ItemClicked?.Invoke(this);
				}
			};

			contentBorder.MouseRightButtonDown += (sender, e) =>
			{
				e.Handled = true;

				ItemRightClicked?.Invoke(this);
			};

			contentBorder.MouseMove += (sender, e) =>
			{
				e.Handled = true;
				
				if (e.LeftButton == MouseButtonState.Pressed)
				{
					DragDrop.DoDragDrop(this, CurrentItem, DragDropEffects.Copy);
					DragEnded?.Invoke();
				}
			};

			contentGrid.Drop += (sender, e) =>
			{
				if (checkDataIsPropertiesBase(e))
				{
					e.Handled = true;
					SelectionDroppedOnItem?.Invoke(this, DragHandleType.None);
				}
			};


			BuildControl();

			GlobalEventManager.ItemExpansionChanged += GlobalEventManager_ItemExpansionChanged;
		}

		private void GlobalEventManager_ItemExpansionChanged(object? sender, PropertiesBaseChangedEventArgs e)
		{
			if(e.Item == CurrentItem)
			{
				var info = GlobalEventManager.ItemInfoMap[CurrentItem.Id];

				IsExpanded = info.IsExpanded;

				if (IsExpanded)
					_expandHandle.Source = IconBuilder.BuildIconBitmapImage("ScrollbarArrowDownRight.png");
				else
					_expandHandle.Source = IconBuilder.BuildIconBitmapImage("ScrollbarArrowCollapsed.png");
			}
		}

		public event Action<TreeViewItemComponent>? ItemClicked;
		public event Action<TreeViewItemComponent>? ItemRightClicked;
		public event Action<TreeViewItemComponent?, DragHandleType> SelectionDroppedOnItem;
		public event Action DragEnded;

		bool checkDataIsPropertiesBase(DragEventArgs e)
		{
			var obj = e.Data.GetData(e.Data.GetFormats()[0]);
			//double check that the object being dropped is a PropertiesBase object
			return typeof(PropertiesBase).IsAssignableFrom(obj.GetType());
		}

		public void BuildControl()
		{
			contentGrid.Children.Clear();
			contentGrid.Margin = new Thickness(IndentationLevel * Size, 0, 0, 0);

			handleGrid.Children.Clear();

			//Each item will consist of an icon, the name of the item, and a handle to expand or collapse the item (if it is a category).

			int gridColumn = 0;

			//Category icons
			if(CurrentItem is Category category)
			{
				Image expandHandle = new Image();
				if (IsExpanded)
					expandHandle.Source = IconBuilder.BuildIconBitmapImage("ScrollbarArrowDownRight.png");
				else
					expandHandle.Source = IconBuilder.BuildIconBitmapImage("ScrollbarArrowCollapsed.png");

				expandHandle.Width = Size;
				expandHandle.Height = Size;
				expandHandle.Cursor = Cursors.Hand;
				expandHandle.MouseLeftButtonDown += (sender, e) =>
				{
					e.Handled = true;

					ToggleCategoryExpand();	
				};

				_expandHandle = expandHandle;

				Grid.SetColumn(expandHandle, gridColumn);
				gridColumn++;

				contentGrid.Children.Add(expandHandle);

				Image typeIcon = IconBuilder.BuildIcon("FolderClosed.png");
				typeIcon.Width = Size;
				typeIcon.Height = Size;

				Grid.SetColumn(typeIcon, gridColumn);
				gridColumn++;
				contentGrid.Children.Add(typeIcon);
			}
			else
			{
				Image typeIcon = IconBuilder.BuildIcon("TextElement.png");
				typeIcon.Width = Size;
				typeIcon.Height = Size;

				Grid.SetColumn(typeIcon, gridColumn);
				gridColumn++;
				contentGrid.Children.Add(typeIcon);
			}
			
			//Item label
			TextBlock itemName = new TextBlock();
			itemName.Text = CurrentItem.Title;
			itemName.VerticalAlignment = VerticalAlignment.Center;
			itemName.Margin = new Thickness(5, 0, 0, 0);
			itemName.Padding = new Thickness(0, 0, 0, 2);
			itemName.FontSize = Size * 0.9;
			//itemName.FontWeight = FontWeights.Bold;

			Grid.SetColumn(itemName, gridColumn);
			gridColumn++;
			contentGrid.Children.Add(itemName);

			//Drag handle
			if (DisplayDragHandle)
			{
				bool isCategory = CurrentItem is Category;
				double width = Math.Max(outerGrid.ActualWidth, Editor.ActiveProject?.CategoryViewWidth ?? 0);
				width = width == 0 ? double.NaN : width;
				width = isCategory ? width / 2 : width;

				Image dragHandle = IconBuilder.BuildIcon("AddNoColor.png");
				dragHandle.Width = width;
				dragHandle.Height = Size;
				dragHandle.Cursor = Cursors.Hand;
				dragHandle.Margin = new Thickness(0, 0, 0, 4);

				Grid.SetRow(dragHandle, 0);

				Label dragLabel = new Label();
				dragLabel.Content = dragHandle;
				dragLabel.VerticalAlignment = VerticalAlignment.Center;
				dragLabel.HorizontalContentAlignment = HorizontalAlignment.Center;
				dragLabel.Width = width;
				dragLabel.Background = Brushes.Transparent;
				
				dragLabel.AllowDrop = true;

				dragLabel.Drop += (sender, e) =>
				{
					if (checkDataIsPropertiesBase(e))
					{
						e.Handled = true;
						SelectionDroppedOnItem?.Invoke(this, DragHandleType.Handle);
					}
				};
				

				if (isCategory)
				{
					Image folderDragHandle = IconBuilder.BuildIcon("PopOut.png");
					folderDragHandle.Width = Size;
					folderDragHandle.Height = Size;
					folderDragHandle.Cursor = Cursors.Hand;
					folderDragHandle.Margin = new Thickness(0, 0, 0, 4);

					Label folderLabel = new Label();
					folderLabel.Content = folderDragHandle;
					folderLabel.VerticalAlignment = VerticalAlignment.Center;
					folderLabel.HorizontalContentAlignment = HorizontalAlignment.Center;
					folderLabel.Width = width;
					folderLabel.Background = Brushes.Transparent;
					folderLabel.AllowDrop = true;

					folderLabel.Drop += (sender, e) =>
					{
						if (checkDataIsPropertiesBase(e))
						{
							e.Handled = true;
							SelectionDroppedOnItem?.Invoke(this, DragHandleType.FolderHandle);
						}
					};

					//move the icons closer to the center
					folderLabel.Padding = new Thickness(0, 0, width / 2, 0);
					dragLabel.Padding = new Thickness(width / 2, 0, 0, 0);

					Grid.SetColumn(folderLabel, 1);

					handleGrid.Children.Add(folderLabel);
				}

				handleGrid.Children.Add(dragLabel);
			}
		}

		public void ToggleCategoryExpand(bool? shouldExpand = null)
		{
			if (_expandHandle == null || shouldExpand == IsExpanded)
				return;

			bool expand = shouldExpand ?? !IsExpanded;

			GlobalEventManager.ChangeItemExpansion(CurrentItem, expand);
		}
	}
}
