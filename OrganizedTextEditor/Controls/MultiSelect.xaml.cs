using OrganizedTextEditor.Classes;
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
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace OrganizedTextEditor.Controls
{
	/// <summary>
	/// Interaction logic for MultiSelect.xaml
	/// </summary>
	public partial class MultiSelect : UserControl
	{
		public class MultiSelectItem
		{
			public string Name { get; set; }
			/// <summary>
			/// 
			/// </summary>
			public Id Id { get; set; }
			public bool IsSelected { get; set; }
		}

		private List<MultiSelectItem> Items { get; set; } = new List<MultiSelectItem>();

		public MultiSelect()
		{
			InitializeComponent();

			dropdownButton.Content = IconBuilder.BuildIcon("ExpandDown.png");
			dropdownButton.Width = 16;
			dropdownButton.Height = 16;

			dropdownButton.Click += DropdownButton_Click;

			borderComp.BorderBrush = new SolidColorBrush(Color.FromRgb(171, 173, 179));
			dropdownBorder.BorderBrush = new SolidColorBrush(Color.FromRgb(171, 173, 179));

			borderComp.Background = Brushes.Transparent;
			borderComp.MouseUp += DropdownButton_Click;
		}

		public event Action<Id, bool> SelectedChanged;

		public void LoadItems(List<MultiSelectItem> items)
		{
			Items = items;
			BuildItemsList();
		}

		public void BuildItemsList()
		{
			ContentPanel.Children.Clear();

			foreach (MultiSelectItem item in Items)
			{
				if(!item.IsSelected)
					continue;

				Grid grid = new Grid();
				grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });
				grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Auto) });

				grid.Margin = new Thickness(0, 0, 5, 5);

				Label label = new Label();
				label.Content = item.Name;
				label.FontSize = 12;
				label.VerticalAlignment = VerticalAlignment.Center;
				label.HorizontalAlignment = HorizontalAlignment.Left;

				grid.Children.Add(label);

				Button removeButton = new Button();
				removeButton.Content = IconBuilder.BuildIcon("Close.png");
				removeButton.Width = 12;
				removeButton.Height = 12;
				removeButton.Margin = new Thickness(0, 3, 0, 0);
				removeButton.Background = Brushes.Transparent;
				removeButton.BorderThickness = new Thickness(0);
				removeButton.Click += (s, e) => 
				{
					e.Handled = true;

					item.IsSelected = false;
					SelectedChanged?.Invoke(item.Id, false);
					BuildItemsList();
				};

				Grid.SetColumn(removeButton, 1);
				grid.Children.Add(removeButton);

				ContentPanel.Children.Add(grid);
			}
		}

		private void DropdownButton_Click(object sender, RoutedEventArgs e)
		{
			e.Handled = true;

			dropdownPopup.IsOpen = true;
			dropdownPopup.Width = ActualWidth;

			dropdownList.Items.Clear();


			void checkForEmpty()
			{
				if (dropdownList.Items.Count == 0)
				{
					Label label = new Label();
					label.Content = "No items available";
					label.FontSize = 12;
					label.VerticalAlignment = VerticalAlignment.Center;
					label.HorizontalAlignment = HorizontalAlignment.Center;

					dropdownList.Items.Add(label);
				}
			}

			foreach(MultiSelectItem item in Items)
			{
				if (item.IsSelected)
					continue;

				CheckBox checkBox = new CheckBox();
				checkBox.Content = item.Name;
				checkBox.Checked += (s, e) => 
				{
					item.IsSelected = true;
					SelectedChanged?.Invoke(item.Id, true);
					BuildItemsList();
					dropdownList.Items.Remove(checkBox);

					checkForEmpty();
				};

				dropdownList.Items.Add(checkBox);
			}

			checkForEmpty();
		}
	}
}
