using OrganizedTextEditor.Classes;
using OrganizedTextEditor.Controls;
using System;
using System.Collections.Generic;
using System.Data.Common;
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

namespace OrganizedTextEditor.Pages
{
	/// <summary>
	/// Interaction logic for EditPage.xaml
	/// </summary>
	public partial class EditPage : Page
	{
		public EditPage()
		{
			InitializeComponent();

			if(Editor.ActiveProject != null)
			{
				LoadProject(Editor.ActiveProject);
			}

			Editor.ProjectOpened += LoadProject;

			Unloaded += (sender, e) =>
			{
				Editor.ProjectOpened -= LoadProject;
			};

			LoadProject(new Project());
		}

		public void LoadProject(Project project) 
		{
			contentGrid.ColumnDefinitions.Clear();
			ColumnDefinition catColumn = new ColumnDefinition();
			catColumn.Width = GridLength.Auto;
			contentGrid.ColumnDefinitions.Add(catColumn);

			ColumnDefinition secColumn = new ColumnDefinition();
			secColumn.Width = new GridLength(1, GridUnitType.Star);
			contentGrid.ColumnDefinitions.Add(secColumn);

			ColumnDefinition propColumn = new ColumnDefinition();
			propColumn.Width = GridLength.Auto;
			contentGrid.ColumnDefinitions.Add(propColumn);


			contentGrid.Children.Clear();

			Thickness margin = new Thickness(5, 5, 0, 5);

			CategoryTreeView categoryTreeView = new CategoryTreeView(project);
			categoryTreeView.Margin = margin;
			categoryTreeView.SetValue(Grid.ColumnProperty, 0);


			contentGrid.Children.Add(categoryTreeView);

			SectionDisplay sectionDisplay = new SectionDisplay(project);
			sectionDisplay.Margin = margin;
			sectionDisplay.SetValue(Grid.ColumnProperty, 1);
			contentGrid.Children.Add(sectionDisplay);

			PropertiesPane propertiesPane = new PropertiesPane(project);
			propertiesPane.Margin = margin;
			propertiesPane.SetValue(Grid.ColumnProperty, 2);
			contentGrid.Children.Add(propertiesPane);

			GridSplitter splitterCategory = new GridSplitter();
			splitterCategory.Width = 3;
			splitterCategory.Margin = new Thickness(0, 30, 0, 5);
			splitterCategory.VerticalAlignment = VerticalAlignment.Stretch;
			splitterCategory.ShowsPreview = true;
			splitterCategory.Focusable = false;
			splitterCategory.Background = Brushes.Transparent;

			splitterCategory.LayoutUpdated += (sender, e) =>
			{
				double calcWidth = catColumn.ActualWidth - (margin.Left + margin.Right);
				if (calcWidth == project.CategoryViewWidth || calcWidth < 0) return;

				project.CategoryViewWidth = calcWidth;
				categoryTreeView.Width = calcWidth;
			};

			splitterCategory.SetValue(Grid.ColumnProperty, 0);
			contentGrid.Children.Add(splitterCategory);

			GridSplitter splitterProperties = new GridSplitter();
			splitterProperties.Width = 3;
			splitterProperties.Margin = new Thickness(5, 5, 0, 5);
			splitterProperties.VerticalAlignment = VerticalAlignment.Stretch;
			splitterProperties.HorizontalAlignment = HorizontalAlignment.Left;
			splitterProperties.ShowsPreview = true;
			splitterProperties.Focusable = false;
			splitterProperties.Background = Brushes.Transparent;


			splitterProperties.LayoutUpdated += (sender, e) =>
			{
				double calcWidth = propColumn.ActualWidth - (margin.Left + margin.Right);
				if (calcWidth == project.PropertyPaneWidth || calcWidth < 0) return;

				project.PropertyPaneWidth = calcWidth;
				propertiesPane.Width = calcWidth;
			};

			splitterProperties.SetValue(Grid.ColumnProperty, 2);
			contentGrid.Children.Add(splitterProperties);
		}
	}
}
