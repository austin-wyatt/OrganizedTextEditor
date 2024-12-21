using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace OrganizedTextEditor.Classes
{
	public class Comment
	{
		/// <summary>
		/// The text of the comment
		/// </summary>
		public string Text { get; set; }
		/// <summary>
		/// The date the comment was posted
		/// </summary>
		public DateTime PostedDate { get; set; }
		/// <summary>
		/// The author of the comment
		/// </summary>
		public User Author { get; set; }
		/// <summary>
		/// Automatically generated id
		/// </summary>
		public Id Id { get; set; }
		/// <summary>
		/// The user specified status of the comment (as specified in the project's settings)
		/// </summary>
		public Id StatusId { get; set; }
		/// <summary>
		/// Child comments of this comment
		/// </summary>
		public List<Comment> Children { get; set; }
	}

	public class CommentStatus
	{
		/// <summary>
		/// The status text of the comment (as specified in the project's settings)
		/// </summary>
		public string StatusText { get; set; } = "";
		/// <summary>
		/// Automatically generated id
		/// </summary>
		public Id Id { get; set; } = new Id();
	}
}
