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
using static System.Collections.Specialized.BitVector32;
using Section = OrganizedTextEditor.Classes.Section;

namespace OrganizedTextEditor.Controls.Sections
{
	/// <summary>
	/// Interaction logic for SectionComponent.xaml
	/// </summary>
	public partial class SectionComponent : UserControl
	{
		public Section Section { get; set; }

		public SectionComponent(Section section)
		{
			InitializeComponent();

			Section = section;

			sectionTextBlock.Text = section.Title;
			sectionTextBlock.Foreground = Theme.SectionComponentTitleColor;
			sectionTextBlock.Margin = new Thickness(0, 0, 0, 2);

			descriptionTextBlock.Text = section.Description;

			descriptionTextBlock.Foreground = Theme.SectionComponentDescriptionColor;
			//descriptionTextBlock.FontStyle = FontStyles.Italic;
			descriptionTextBlock.FontSize = 12;
			descriptionTextBlock.Margin = new Thickness(10, 0, 5, 0);
			descriptionTextBlock.TextWrapping = TextWrapping.Wrap;
			descriptionTextBlock.TextAlignment = TextAlignment.Left;

			if(Editor.ActiveProject != null)
			{
				foreach (var tagId in section.TagIds)
				{
					var tag = Editor.ActiveProject.Settings.Tags.FirstOrDefault(t => t.Id == tagId);

					if (tag != null)
					{
						var tagComponent = new TextBlock();
						tagComponent.Text = tag.Name;
						tagComponent.Margin = new Thickness(5, 0, 5, 0);
						tagComponent.FontSize = 12;
						tagComponent.Foreground = new SolidColorBrush(Color.FromRgb(171, 173, 179));
						tagComponent.FontStyle = FontStyles.Italic;
						tagComponent.TextWrapping = TextWrapping.NoWrap;
						tagComponent.TextAlignment = TextAlignment.Left;

						tagsPanel.Children.Add(tagComponent);
					}
				}
			}
			

			//sectionRichTextBox.Document = section.Document;
			sectionTextBox.Text = section.Text;
			sectionTextBox.AcceptsReturn = true;
			sectionTextBox.TextWrapping = TextWrapping.Wrap;
			sectionTextBox.Width = 200;
			sectionTextBox.SpellCheck.IsEnabled = false;

			sectionTextBox.TextChanged += (sender, e) =>
			{
				Section.Text = sectionTextBox.Text;
			};

			sectionTextBox.PreviewKeyDown += (s, e) =>
			{
				bool shiftDown = Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift);

				if(e.Key == Key.Enter && shiftDown)
				{
					e.Handled = true;

					InsertSectionBelow();
				}
			};

			addButton.Visibility = Visibility.Hidden;
			addButton.Content = IconBuilder.BuildIcon("AddNoColor.png");
			addButton.Background = Brushes.Transparent;
			addButton.BorderThickness = new Thickness(0);
			addButton.Margin = new Thickness(10, 0, 0, 0);

			labelPanel.MouseEnter += (sender, e) =>
			{
				addButton.Visibility = Visibility.Visible;
			};	

			labelPanel.MouseLeave += (sender, e) =>
			{
				addButton.Visibility = Visibility.Hidden;
			};

			addButton.Click += (sender, e) =>
			{
				InsertSectionBelow();
			};

			//Binding binding = new Binding("ActualWidth") { Source = sectionRichTextBox };
			//section.Document.SetBinding(FlowDocument.PageWidthProperty, binding);

			const int sidebarWidth = 150;

			contentGrid.ColumnDefinitions[0].Width = new GridLength(sidebarWidth);
			contentGrid.ColumnDefinitions[1].Width = new GridLength(1, GridUnitType.Star);
			contentGrid.ColumnDefinitions[2].Width = new GridLength(sidebarWidth);

			contentGrid.SizeChanged += (sender, e) =>
			{
				sectionTextBox.Width = contentGrid.ActualWidth - sidebarWidth * 2;
				sectionTextBox.MaxWidth = contentGrid.ActualWidth - sidebarWidth * 2;
			};

			sectionTextBlock.MouseDown += (sender, e) =>
			{
				if(e.ClickCount == 2)
				{
					GlobalEventManager.OnPropertiesBaseItemInspected(this, Section, focus: true);
					
					GlobalEventManager.OnSelectItem(this, Section);
				}
			};

			GlobalEventManager_SectionContentScaled(null, GlobalEventManager.SectionContentScale);


			GlobalEventManager.ItemChanged += GlobalEventManager_ItemChanged;
			GlobalEventManager.SectionContentScaled += GlobalEventManager_SectionContentScaled;

			Unloaded += (sender, e) =>
			{
				GlobalEventManager.ItemChanged -= GlobalEventManager_ItemChanged;
				GlobalEventManager.SectionContentScaled -= GlobalEventManager_SectionContentScaled;
			};
 		}

		private void InsertSectionBelow()
		{
			var parent = GlobalEventManager.GetParent(Section);

			var currIndex = parent.Children.FindIndex(s => s == Section);

			var newSection = new Section();
			parent.Children.Insert(currIndex + 1, newSection);

			GlobalEventManager.BuildScopedInfoMap(parent, GlobalEventManager.ItemInfoMap[parent.Id].IndentationLevel);
			GlobalEventManager.OnPropertiesBaseItemAdded(this, newSection);

			Task.Run(() =>
			{
				Thread.Sleep(50);
				Dispatcher.Invoke(() =>
				{
					GlobalEventManager.OnFocusSection(newSection);
					GlobalEventManager.OnPropertiesBaseItemInspected(this, newSection, focus: false);
				});
			});
		}

		private void GlobalEventManager_SectionContentScaled(object? sender, double e)
		{
			sectionTextBlock.FontSize = 12 * e;
			descriptionTextBlock.FontSize = 12 * e;
			sectionTextBox.FontSize = 12 * e;

			foreach(var child in tagsPanel.Children)
			{
				if(child is TextBlock tagComponent)
				{
					tagComponent.FontSize = 12 * e;
				}
			}
		}

		private void GlobalEventManager_ItemChanged(object? sender, PropertiesBaseChangedEventArgs e)
		{
			if(e.Item.Id == Section.Id)
			{
				sectionTextBlock.Text = Section.Title;
				sectionTextBox.Text = Section.Text;
				descriptionTextBlock.Text = Section.Description;

				if (Editor.ActiveProject != null)
				{
					tagsPanel.Children.Clear();

					foreach (var tagId in Section.TagIds)
					{
						var tag = Editor.ActiveProject.Settings.Tags.FirstOrDefault(t => t.Id == tagId);

						if (tag != null)
						{
							var tagComponent = new TextBlock();
							tagComponent.Text = tag.Name;
							tagComponent.Margin = new Thickness(5, 0, 5, 0);
							tagComponent.FontSize = 12 * GlobalEventManager.SectionContentScale;
							tagComponent.Foreground = new SolidColorBrush(Color.FromRgb(171, 173, 179));
							tagComponent.FontStyle = FontStyles.Italic;
							tagComponent.TextWrapping = TextWrapping.NoWrap;
							tagComponent.TextAlignment = TextAlignment.Left;

							tagsPanel.Children.Add(tagComponent);
						}
					}
				}
			}
		}

		public void Select(bool select)
		{
			if (select)
			{
				sectionTextBlock.Foreground = Theme.SelectedColorSection;
			}
			else
			{
				sectionTextBlock.Foreground = Theme.SectionComponentTitleColor;
			}
		}
	}
}
