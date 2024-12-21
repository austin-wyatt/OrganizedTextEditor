using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;

namespace OrganizedTextEditor.Classes
{
	public class Section : PropertiesBase
	{
		public FlowDocument Document { get; set; }
		public String Text { get; set; }

		public Section() : base()
		{
			Title = "New Section";
			Document = new FlowDocument();
			Text = "";
		}
	}
}
