using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace OrganizedTextEditor.Classes
{
	public static class Editor
	{
		public static EditorSettings Settings { get; set; }
		public static List<Project> OpenProjects { get; set; }
		public static Project? ActiveProject { get; set; }
		public static bool ProjectIsFresh { get; set; } = false;

		static Editor()
		{
			Settings = new EditorSettings();
			OpenProjects = new List<Project>();
		}

		public delegate void ProjectOpenedEventHandler(Project project);
		public static event ProjectOpenedEventHandler? ProjectOpened;

		public static void OpenProject(Project project)
		{
			ProjectIsFresh = false;
			ActiveProject = project;
			ProjectOpened?.Invoke(project);
		}

		public static void OpenNewProject()
		{
			ProjectIsFresh = true;
			Project project = new Project();
			ActiveProject = project;
			ProjectOpened?.Invoke(project);
		}
	}

	public class EditorSettings
	{
		public User? CurrentUser { get; set; } = null;

		public List<User> Users { get; set; } = new();

	}
}
