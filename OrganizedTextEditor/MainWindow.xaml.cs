using Microsoft.Win32;
using OrganizedTextEditor.Classes;
using OrganizedTextEditor.Windows;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace OrganizedTextEditor
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		public MainWindow()
		{
			InitializeComponent();

			Editor.ProjectOpened += (project) =>
			{
				UpdateWindowTitleForProject(project);
			};

			PreviewKeyDown += (sender, e) =>
			{
				PortalFocusManager.OnKeyDown(e);

				if (e.Key == Key.S && Keyboard.Modifiers == ModifierKeys.Control)
				{
					SaveProject_Click(sender, e);
				}
			};

			MouseDown += (sender, e) =>
			{
				PortalFocusManager.SetFocusedPortal("");
			};

			MouseUp += (sender, e) =>
			{
				PortalFocusManager.OnMouseUp(e);
			};

			Loaded += (sender, e) =>
			{
				Editor.OpenNewProject();
			};

			//Theme.ThemeType theme = Theme.ThemeType.Dark;
			Theme.ThemeType theme = Theme.ThemeType.Light;

			Theme.ApplyTheme(theme);

			if(theme == Theme.ThemeType.Dark)
			{
				Background = Theme.DarkModeBackground;
				Foreground = Theme.DarkModeForeground;
			}
		}

		public void UpdateWindowTitleForProject(Project project) 
		{
			Title = $"Organized Text Editor - {project.Title}";
		}

		private void NewProject_Click(object sender, RoutedEventArgs e)
		{
			if(Editor.ActiveProject != null)
			{
				MessageBoxResult result = MessageBox.Show("Do you want to save the current project?", "Save Project", MessageBoxButton.YesNoCancel);
				if (result == MessageBoxResult.Yes)
				{
					Editor.ActiveProject.Save();
				}
				else if (result == MessageBoxResult.Cancel)
				{
					return;
				}
			}

			Editor.OpenNewProject();
		}

		private void OpenProject_Click(object sender, RoutedEventArgs e)
		{
			OpenFileDialog dialog = new OpenFileDialog();
			dialog.Filter = "OTE Project Files (*.oteproj)|*.oteproj";

			if (dialog.ShowDialog() == true)
			{
				try
				{
					var loadedProject = Project.Load(dialog.FileName);
					Editor.OpenProject(loadedProject);
				}
				catch
				{
					MessageBox.Show("Failed to load project.");
				}
			}
		}

		private void SaveProject_Click(object sender, RoutedEventArgs e)
		{
			Project project = Editor.ActiveProject;
			if (project != null) 
			{
				if (Editor.ProjectIsFresh)
				{
					SaveFileDialog dialog = new SaveFileDialog();
					dialog.Filter = "OTE Project Files (*.oteproj)|*.oteproj";
					dialog.DefaultExt = "oteproj";
					dialog.FileName = project.Title;

					if (dialog.ShowDialog() == true)
					{
						project.Title = System.IO.Path.GetFileNameWithoutExtension(dialog.FileName);
						UpdateWindowTitleForProject(project);

						project.Save();
						Editor.ProjectIsFresh = false;
					}
				}
				else
				{
					project.Save();
				}
			}
		}

		private void EditorSettings_Click(object sender, RoutedEventArgs e)
		{
			Editor.OpenProject(Editor.ActiveProject);
		}
		private void ProjectSettings_Click(object sender, RoutedEventArgs e)
		{
			if(Editor.ActiveProject == null)
			{
				MessageBox.Show("No project is open", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
				return;
			}

			ProjectSettingsWindow projectSettingsWindow = new ProjectSettingsWindow(Editor.ActiveProject);
			projectSettingsWindow.Owner = this;
			projectSettingsWindow.ShowDialog();
		}
	}
}