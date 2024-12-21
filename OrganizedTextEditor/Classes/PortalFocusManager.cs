using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace OrganizedTextEditor.Classes
{
	public static class PortalFocusManager
	{
		public static event Action<KeyEventArgs, string>? KeyDown;
		public static event Action<MouseEventArgs, string>? MouseUp;
		public static event Action<string, string>? FocusChanged;
		private static string _focusedPortal = "";

		public static string FocusedPortal => _focusedPortal;

		public static void SetFocusedPortal(string portal)
		{
			if(_focusedPortal == portal)
				return;

			string previousFocusedPortal = _focusedPortal;

			_focusedPortal = portal;
			FocusChanged?.Invoke(portal, previousFocusedPortal);
		}

		public static void OnKeyDown(KeyEventArgs e)
		{
			KeyDown?.Invoke(e, _focusedPortal);
		}

		public static void OnMouseUp(MouseEventArgs e)
		{
			MouseUp?.Invoke(e, _focusedPortal);
		}
	}
}
