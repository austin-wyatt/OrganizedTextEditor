using OrganizedTextEditor.Classes;
using System;
using System.Collections.Generic;
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
	/// Interaction logic for PropertiesPane.xaml
	/// </summary>
	public partial class PropertiesPane : UserControl
	{
		private Project _currentProject;
		private PropertiesBase? _selectedItem;

		const string PORTAL_NAME = "PropertiesPane";

		public PropertiesPane(Project project)
		{
			InitializeComponent();

			_currentProject = project;

			Width = _currentProject.PropertyPaneWidth;

			contentPanel.GotFocus += (sender, e) =>
			{
				PortalFocusManager.SetFocusedPortal(PORTAL_NAME);
			};

			titleTextBox.TextChanged += (sender, e) =>
			{
				if(_selectedItem == null)
					return;

				_selectedItem.Title = titleTextBox.Text;
				GlobalEventManager.OnPropertiesBaseItemChanged(this, _selectedItem);
			};

			descriptionTextBox.TextChanged += (sender, e) =>
			{
				if (_selectedItem == null)
					return;

				_selectedItem.Description = descriptionTextBox.Text;
				GlobalEventManager.OnPropertiesBaseItemChanged(this, _selectedItem);
			};

			tagsMultiselect.ContentPanel.Width = 150;
			tagsMultiselect.SelectedChanged += (id, selected) =>
			{
				if (_selectedItem == null)
					return;

				if (selected)
				{
					_selectedItem.TagIds.Add(id);
				}
				else
				{
					_selectedItem.TagIds.Remove(id);
				}

				GlobalEventManager.OnPropertiesBaseItemChanged(this, _selectedItem);
			};

			excludeFromExportCheckbox.Checked += ExcludeFromExportCheckbox_Checked;
			excludeFromExportCheckbox.Unchecked += ExcludeFromExportCheckbox_Checked;

			SelectItem(null);

			scrollComponent.PreviewMouseDown += (sender, e) =>
			{
				PortalFocusManager.SetFocusedPortal(PORTAL_NAME);
			};

			GlobalEventManager.ItemInspected += GlobalEventManager_PropertiesBaseItemInspected;
			GlobalEventManager.ItemInspectedNoFocus += GlobalEventManager_PropertiesBaseItemInspectedNoFocus;

			GlobalEventManager.ItemRemoved += GlobalEventManager_PropertiesBaseItemRemoved;
			PortalFocusManager.FocusChanged += PortalFocusManager_FocusChanged;

			Unloaded += (sender, e) =>
			{
				GlobalEventManager.ItemInspected -= GlobalEventManager_PropertiesBaseItemInspected;
				GlobalEventManager.ItemInspectedNoFocus -= GlobalEventManager_PropertiesBaseItemInspectedNoFocus;

				GlobalEventManager.ItemRemoved -= GlobalEventManager_PropertiesBaseItemRemoved;
				PortalFocusManager.FocusChanged -= PortalFocusManager_FocusChanged;
			};
		}

		private void ExcludeFromExportCheckbox_Checked(object sender, RoutedEventArgs e)
		{
			if(_selectedItem == null)
				return;

			_selectedItem.ExcludeFromExport = excludeFromExportCheckbox.IsChecked == true;
			GlobalEventManager.OnPropertiesBaseItemChanged(this, _selectedItem);
		}

		private void GlobalEventManager_PropertiesBaseItemRemoved(object? sender, PropertiesBaseChangedEventArgs e)
		{
			if(e.Item == _selectedItem)
			{
				SelectItem(null);
			}
		}

		private void PortalFocusManager_FocusChanged(string portal, string previous)
		{
			if(portal == PORTAL_NAME)
			{
				titleTextBox.Focus();
			}
		}

		private void GlobalEventManager_PropertiesBaseItemInspectedNoFocus(object? sender, PropertiesBaseChangedEventArgs e)
		{
			e.Handled = true;
			SelectItem(e.Item);
		}

		private void GlobalEventManager_PropertiesBaseItemInspected(object? sender, PropertiesBaseChangedEventArgs e)
		{
			e.Handled = true;
			SelectItem(e.Item);

			Task.Run(() =>
			{
				Thread.Sleep(50);
				Dispatcher.Invoke(() =>
				{
					PortalFocusManager.SetFocusedPortal(PORTAL_NAME);
					Keyboard.Focus(titleTextBox);
				});
			});
		}

		public void SelectItem(PropertiesBase? item)
		{
			_selectedItem = item;
			BuildProperties();
		}

		public void BuildProperties() 
		{
			if (_selectedItem == null)
			{
				contentPanel.Visibility = Visibility.Hidden;
				return;
			}
			else
			{
				contentPanel.Visibility = Visibility.Visible;
			}

			titleTextBox.Text = _selectedItem.Title;

			descriptionTextBox.Text = _selectedItem.Description;
			

			List<MultiSelect.MultiSelectItem> tagOptions = new List<MultiSelect.MultiSelectItem>();
			foreach (Tag tag in _currentProject.Settings.Tags)
			{
				tagOptions.Add(new MultiSelect.MultiSelectItem
				{
					Name = tag.Name,
					Id = tag.Id,
					IsSelected = _selectedItem.TagIds.Contains(tag.Id)
				});
			}

			tagsMultiselect.LoadItems(tagOptions);
		}
	}
}
