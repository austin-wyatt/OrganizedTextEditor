using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace OrganizedTextEditor.Classes
{
	public static class Theme
	{
		public static ThemeType CurrentTheme = ThemeType.Light;

		public static Brush ProgramBackground = Brushes.White;
		public static Brush ProgramForeground = Brushes.Black;
		public static Brush ProgramMenu = Brushes.White;

		public static Brush DarkModeBackground = new SolidColorBrush(Color.FromRgb(30, 30, 30));
		public static Brush DarkModeForeground = new SolidColorBrush(Color.FromRgb(200, 200, 200));
		public static Brush DarkModeMenu = new SolidColorBrush(Color.FromRgb(46, 46, 46));
		public static Brush DarkSelectedColor = new SolidColorBrush(Color.FromRgb(61, 61, 61));
		public static Brush DarkOutlineColor = new SolidColorBrush(Color.FromRgb(112, 112, 112));
		public static Brush LightSelectedColor = new SolidColorBrush(Color.FromRgb(204, 232, 255));

		public static Brush LightSelectedColorSection = new SolidColorBrush(Color.FromRgb(0, 122, 204));
		public static Brush SelectedColor { get => CurrentTheme == ThemeType.Light ? LightSelectedColor : DarkSelectedColor; }
		public static Brush SelectedColorSection { get => CurrentTheme == ThemeType.Light ? Brushes.Indigo : DarkModeForeground; }

		public static Brush CategoryComponentTextColor = new SolidColorBrush(Color.FromRgb(77, 141, 194));

		public static Brush SectionComponentTitleColor = new SolidColorBrush(Color.FromRgb(213, 156, 132));
		public static Brush SectionComponentDescriptionColor = new SolidColorBrush(Color.FromRgb(214, 158, 219));



		private static Style DefaultTextBoxStyle = new Style(typeof(TextBox)) 
		{
			BasedOn = (Style)Application.Current.FindResource(typeof(TextBox))
		};

		private static Style DefaultTextBlockStyle = new Style(typeof(TextBlock))
		{
			BasedOn = (Style)Application.Current.FindResource(typeof(TextBlock))
		};

		private static Style DefaultLabelStyle = new Style(typeof(Label))
		{
			BasedOn = (Style)Application.Current.FindResource(typeof(Label))
		};

		private static Style DefaultMenuItemStyle = new Style(typeof(MenuItem))
		{
			//BasedOn = (Style)Application.Current.FindResource(typeof(MenuItem))
		};

		private static Style DefaultMenuStyle = new Style(typeof(Menu))
		{
			//BasedOn = (Style)Application.Current.FindResource(typeof(Menu))
		};

		private static Style WindowStyle = new Style(typeof(Window))
		{
			BasedOn = (Style)Application.Current.FindResource(typeof(Window))
		};

		private static Style GridStyle = new Style(typeof(Grid))
		{
			//BasedOn = (Style)Application.Current.FindResource(typeof(Grid))
		};

		private static Style StackPanelStyle = new Style(typeof(StackPanel))
		{
			//BasedOn = (Style)Application.Current.FindResource(typeof(StackPanel))
		};

		private static Style ScrollViewerStyle = new Style(typeof(ScrollViewer))
		{
			//BasedOn = (Style)Application.Current.FindResource(typeof(ScrollViewer))
		};

		private static Style PageStyle = new Style(typeof(Page))
		{
			//BasedOn = (Style)Application.Current.FindResource(typeof(Page))
		};

		private static Style BorderStyle = new Style(typeof(Border))
		{
			//BasedOn = (Style)Application.Current.FindResource(typeof(Border))
		};

		private static Style GridSplitterStyle = new Style(typeof(GridSplitter))
		{
			//BasedOn = (Style)Application.Current.FindResource(typeof(GridSplitter))
		};

		private static Style ButtonStyle = new Style(typeof(Button))
		{
			BasedOn = (Style)Application.Current.FindResource(typeof(Button))
		};

		public enum ThemeType
		{
			Light,
			Dark
		}

		public static void ApplyTheme(ThemeType type)
		{
			CurrentTheme = type;

			if (type == ThemeType.Dark)
			{
				DefaultTextBoxStyle.Setters.Add(new Setter(TextBox.BackgroundProperty, Brushes.Transparent));
				DefaultTextBoxStyle.Setters.Add(new Setter(TextBox.ForegroundProperty, DarkModeForeground));
				DefaultTextBoxStyle.Setters.Add(new Setter(TextBox.BorderBrushProperty, DarkOutlineColor));

				Application.Current.Resources.Add(typeof(TextBox), DefaultTextBoxStyle);

				DefaultTextBlockStyle.Setters.Add(new Setter(TextBlock.BackgroundProperty, Brushes.Transparent));
				DefaultTextBlockStyle.Setters.Add(new Setter(TextBlock.ForegroundProperty, DarkModeForeground));

				Application.Current.Resources.Add(typeof(TextBlock), DefaultTextBlockStyle);

				DefaultLabelStyle.Setters.Add(new Setter(Label.BackgroundProperty, Brushes.Transparent));
				DefaultLabelStyle.Setters.Add(new Setter(Label.ForegroundProperty, DarkModeForeground));
				DefaultLabelStyle.Setters.Add(new Setter(Label.BorderBrushProperty, DarkOutlineColor));

				Application.Current.Resources.Add(typeof(Label), DefaultLabelStyle);

				

				DefaultMenuItemStyle.Setters.Add(new Setter(MenuItem.BackgroundProperty, DarkModeMenu));
				DefaultMenuItemStyle.Setters.Add(new Setter(MenuItem.ForegroundProperty, DarkModeForeground));
				DefaultMenuItemStyle.Setters.Add(new Setter(MenuItem.BorderBrushProperty, Brushes.Transparent));


				//var border = new FrameworkElementFactory(typeof(Border));
				//border.SetValue(Border.PaddingProperty, new Thickness(17, 0, 17, 0));
				//border.SetValue(Border.SnapsToDevicePixelsProperty, true);
				//var contentPresenter = new FrameworkElementFactory(typeof(ContentPresenter));
				//border.AppendChild(contentPresenter);
				//DefaultMenuItemStyle.Setters.Add(new Setter(MenuItem.TemplateProperty, new ControlTemplate()
				//{
				//	TargetType = typeof(MenuItem),
				//	VisualTree = border,
				//	Triggers=
				//	{
				//		new Trigger()
				//		{
				//			Property = MenuItem.IsHighlightedProperty,
				//			Value = true,
				//			Setters =
				//			{
				//				new Setter(MenuItem.BackgroundProperty, DarkSelectedColor),
				//				new Setter(MenuItem.BorderBrushProperty, DarkOutlineColor)
				//			}
				//		}
				//	}
				//}));

				DefaultMenuItemStyle.Triggers.Add(new Trigger()
				{
					Property = MenuItem.IsHighlightedProperty,
					Value = true,
					Setters =
					{
						new Setter(MenuItem.BackgroundProperty, DarkSelectedColor),
						new Setter(MenuItem.BorderBrushProperty, DarkOutlineColor),
					}
				});

				Application.Current.Resources.Add(typeof(MenuItem), DefaultMenuItemStyle);

				DefaultMenuStyle.Setters.Add(new Setter(Menu.BorderBrushProperty, DarkOutlineColor));
				DefaultMenuStyle.Setters.Add(new Setter(Menu.BackgroundProperty, DarkModeBackground));

				Application.Current.Resources.Add(typeof(Menu), DefaultMenuStyle);

				//WindowStyle.Setters.Add(new Setter(Window.BackgroundProperty, DarkModeBackground));
				//WindowStyle.Setters.Add(new Setter(Window.ForegroundProperty, DarkModeForeground));

				//Application.Current.Resources.Add(typeof(Window), WindowStyle);

				GridStyle.Setters.Add(new Setter(Grid.BackgroundProperty, Brushes.Transparent));

				Application.Current.Resources.Add(typeof(Grid), GridStyle);

				StackPanelStyle.Setters.Add(new Setter(StackPanel.BackgroundProperty, Brushes.Transparent));

				Application.Current.Resources.Add(typeof(StackPanel), StackPanelStyle);

				ScrollViewerStyle.Setters.Add(new Setter(ScrollViewer.BackgroundProperty, Brushes.Transparent));
				ScrollViewerStyle.Setters.Add(new Setter(ScrollViewer.ForegroundProperty, DarkModeForeground));

				Application.Current.Resources.Add(typeof(ScrollViewer), ScrollViewerStyle);
				
				PageStyle.Setters.Add(new Setter(Page.BackgroundProperty, Brushes.Transparent));
				PageStyle.Setters.Add(new Setter(Page.ForegroundProperty, DarkModeForeground));

				Application.Current.Resources.Add(typeof(Page), PageStyle);

				BorderStyle.Setters.Add(new Setter(Border.BackgroundProperty, Brushes.Transparent));
				BorderStyle.Setters.Add(new Setter(Border.BorderBrushProperty, DarkModeForeground));

				Application.Current.Resources.Add(typeof(Border), BorderStyle);

				GridSplitterStyle.Setters.Add(new Setter(GridSplitter.BackgroundProperty, DarkModeForeground));
				GridSplitterStyle.Setters.Add(new Setter(GridSplitter.ForegroundProperty, DarkModeForeground));

				Application.Current.Resources.Add(typeof(GridSplitter), GridSplitterStyle);

				ButtonStyle.Setters.Add(new Setter(Button.BackgroundProperty, DarkModeMenu));
				ButtonStyle.Setters.Add(new Setter(Button.ForegroundProperty, DarkModeForeground));
				ButtonStyle.Setters.Add(new Setter(Button.BorderBrushProperty, DarkModeForeground));

				Application.Current.Resources.Add(typeof(Button), ButtonStyle);
			}
			else if(type == ThemeType.Light)
			{
				BorderStyle.Setters.Add(new Setter(Border.BorderBrushProperty, ProgramForeground));

				Application.Current.Resources.Add(typeof(Border), BorderStyle);
			}
		}
	}
}
