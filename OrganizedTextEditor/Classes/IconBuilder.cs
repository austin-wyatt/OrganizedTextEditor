using SharpVectors.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace OrganizedTextEditor.Classes
{
	public static class IconBuilder
	{
		public static Image BuildIcon(string iconName)
		{
			return new Image()
			{
				Source = new BitmapImage(new Uri("pack://application:,,,/Resources/Icons/" + iconName))
			};
		}

		public static SvgViewbox BuildSvgIcon(string iconName)
		{
			return new SvgViewbox()
			{
				Source = new Uri("pack://application:,,,/Resources/Icons/" + iconName)
			};
		}

		public static BitmapImage BuildIconBitmapImage(string iconName)
		{
			return new BitmapImage(new Uri("pack://application:,,,/Resources/Icons/" + iconName));
		}
	}
}
