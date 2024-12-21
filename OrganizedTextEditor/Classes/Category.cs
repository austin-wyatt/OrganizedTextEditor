using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace OrganizedTextEditor.Classes
{
	public class Category : PropertiesBase
	{
		/// <summary>
		/// The UI will need to use reflection to determine whether these are sections or categories.
		/// </summary>
		public List<PropertiesBase> Children { get; set; }
		
		public Category() : base()
		{
			Title = "New Category";
			Children = new List<PropertiesBase>();
		}
	}
}
