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
using System.Windows.Shapes;

namespace OrganizedTextEditor.Windows
{
	/// <summary>
	/// Interaction logic for ProjectSettings.xaml
	/// </summary>
	public partial class ProjectSettingsWindow : Window
	{
		private ProjectSettings Settings { get; set; }
		private Project CurrentProject { get; set; }

		public ProjectSettingsWindow(Project project)
		{
			InitializeComponent();

			CurrentProject = project;
			Settings = project.Settings;

			tagOptions.LoadTags(Settings.Tags);
			commentStatusOptions.LoadStatuses(Settings.CommentStatuses);
		}
	}
}
