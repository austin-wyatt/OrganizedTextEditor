using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace OrganizedTextEditor.Classes
{
	[JsonDerivedType(typeof(PropertiesBase), typeDiscriminator: 0)]
	[JsonDerivedType(typeof(Category), typeDiscriminator: 1)]
	[JsonDerivedType(typeof(Section), typeDiscriminator: 2)]
	public class PropertiesBase
	{
		public string Title { get; set; }
		public string Description { get; set; }
		public Id Id { get; set; }
		public List<Id> TagIds { get; set; }
		public List<Comment> Comments { get; set; }

		public bool ExcludeFromExport { get; set; } = false;

		public PropertiesBase()
		{
			Title = "";
			Description = "";
			Id = new Id();
			TagIds = new List<Id>();
			Comments = new List<Comment>();
		}

		public List<Tag> ResolveTagIds()
		{
			var project = Editor.ActiveProject;
			if (project == null)
			{
				return new List<Tag>();
			}

			List<Tag> tags = new List<Tag>();
			HashSet<Id> tagIdsSet = new HashSet<Id>(TagIds);

			foreach(var tag in project.Settings.Tags)
			{
				if(tagIdsSet.Contains(tag.Id))
				{
					tags.Add(tag);
					tagIdsSet.Remove(tag.Id);
				}
			}

			foreach(var id in tagIdsSet)
			{
				TagIds.Remove(id);
			}

			return tags;
		}
	}
}
