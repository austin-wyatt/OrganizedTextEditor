using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrganizedTextEditor.Classes
{
	public static class Helpers
	{
		public static double Lerp(double a, double b, double t)
		{
			return a + (b - a) * t;
		}

		public static double CubicLerp(double a, double b, double t)
		{
			t = t * t * (3.0 - 2.0 * t);
			return a + (b - a) * t;
		}
	}
}
