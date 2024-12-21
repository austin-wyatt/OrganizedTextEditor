using OrganizedTextEditor.Classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace OrganizedTextEditor.Controls.Sections
{
	/// <summary>
	/// Interaction logic for CategoryComponent.xaml
	/// </summary>
	public partial class CategoryComponent : UserControl
	{
		public Category Category { get; set; }

		public CategoryComponent(Category category)
		{
			InitializeComponent();

			Category = category;

			outerBorder.BorderBrush = Theme.CategoryComponentTextColor;
			categoryNameBlock.Foreground = Theme.CategoryComponentTextColor;

			categoryNameBlock.Text = GenerateTitle(category);

			var indentationLevel = GlobalEventManager.ItemInfoMap[category.Id].IndentationLevel;

			//categoryNameBlock.Margin = new Thickness(indentationLevel * 32 + 10, 0, 0, 0);
			categoryNameBlock.Margin = new Thickness(10, 0, 0, 0);

			categoryNameBlock.MouseDown += (sender, e) =>
			{
				if (e.ClickCount == 2)
				{
					GlobalEventManager.OnPropertiesBaseItemInspected(this, Category, focus: true);

					GlobalEventManager.OnSelectItem(this, Category);
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

		private string GenerateTitle(PropertiesBase item)
		{
			StringBuilder builder = new StringBuilder();

			List<PropertiesBase> parents = new List<PropertiesBase>();

			parents.Add(item);

			var parent = GlobalEventManager.GetParent(item);
			while (parent != null && parent.Id != Editor.ActiveProject?.Root.Id && Editor.ActiveProject != null)
			{
				parents.Add(parent);
				parent = GlobalEventManager.GetParent(parent);
			}

			for (int i = parents.Count - 1; i >= 0; i--)
			{
				builder.Append(parents[i].Title);

				if (i != 0)
					builder.Append(" > ");
			}

			return builder.ToString();
		}

		private void GlobalEventManager_SectionContentScaled(object? sender, double e)
		{
			categoryNameBlock.FontSize = 12 * e;

			foreach (var child in tagsPanel.Children)
			{
				if (child is TextBlock tagComponent)
				{
					tagComponent.FontSize = 12 * e;
				}
			}
		}

		private void GlobalEventManager_ItemChanged(object? sender, PropertiesBaseChangedEventArgs e)
		{
			if(e.Item.Id == Category.Id)
			{
				categoryNameBlock.Text = GenerateTitle(Category);

				if (Editor.ActiveProject != null)
				{
					tagsPanel.Children.Clear();

					foreach (var tagId in Category.TagIds)
					{
						var tag = Editor.ActiveProject.Settings.Tags.FirstOrDefault(t => t.Id == tagId);

						if (tag != null)
						{
							var tagComponent = new TextBlock();
							tagComponent.Text = tag.Name;
							tagComponent.Margin = new Thickness(0, 0, 10, 0);
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
				outerBorder.BorderBrush = Theme.SelectedColorSection;
				categoryNameBlock.Foreground = Theme.SelectedColorSection;
			}
			else
			{
				outerBorder.BorderBrush = Theme.CategoryComponentTextColor;
				categoryNameBlock.Foreground = Theme.CategoryComponentTextColor;
			}
		}
	}
}
