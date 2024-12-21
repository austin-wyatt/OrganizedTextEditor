using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace OrganizedTextEditor.Classes
{
	public class Project
	{
		public Id Id { get; set; } = new Id();

		public string Title { get; set; } = "New Project";

		public ProjectSettings Settings { get; set; } = new ProjectSettings();

		public Category Root { get; set; } = new Category() 
		{ 
			Title = "Root" 
		};

		public double CategoryViewWidth { get; set; } = 200;
		public double PropertyPaneWidth { get; set; } = 200;

		/// <summary>
		/// Save the project into the Projects directory
		/// </summary>
		public void Save()
		{
			string path = "Projects/" + Title + ".oteproj";

			string serializedProject = Serializer.Serialize(this);

			Directory.CreateDirectory("Projects");
			File.WriteAllText(path, serializedProject);
		}

		/// <summary>
		/// Load a project from the Projects directory
		/// </summary>
		/// <param name="path"></param>
		/// <returns></returns>
		public static Project Load(string path)
		{
			string serializedProject = File.ReadAllText(path);
			
			Project? project = Serializer.Deserialize<Project>(serializedProject);

			if(project == null)
			{
				throw new Exception("Failed to load project");
			}

			string projectName = Path.GetFileNameWithoutExtension(path);
			project.Title = projectName;

			return project;
		}

		public static List<string> GetProjectFiles()
		{
			const string PROJECT_EXTENSION = ".oteproj";

			const string PROJECTS_DIRECTORY = "Projects";

			var files = new List<string>();

			foreach(var file in Directory.GetFiles(PROJECTS_DIRECTORY))
			{
				if(file.EndsWith(PROJECT_EXTENSION))
				{
					files.Add(file);
				}
			}

			// Get a list of project files
			return files;
		}
	}

	public class ProjectSettings
	{
		public Dictionary<string, object> Data { get; set; } = new();



		public List<Tag> Tags { get; set; } = new();

		public List<CommentStatus> CommentStatuses { get; set; } = new();
	}
}
