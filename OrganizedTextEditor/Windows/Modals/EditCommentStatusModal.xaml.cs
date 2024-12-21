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

namespace OrganizedTextEditor.Windows.Modals
{
	/// <summary>
	/// Interaction logic for EditCommentStatusModal.xaml
	/// </summary>
	public partial class EditCommentStatusModal : Window
	{
		public enum Mode
		{
			Edit,
			Create
		}

		public CommentStatus Status { get; set; }
		public Mode ModalMode { get; set; }

		public EditCommentStatusModal(CommentStatus status, Mode mode)
		{
			InitializeComponent();

			Status = status;
			ModalMode = mode;

			Title = mode == Mode.Edit ? "Edit Comment Status" : "Create Comment Status";

			tagNameTextBox.Text = status.StatusText;

			saveButton.Click += SaveButton_Click;
			cancelButton.Click += CancelButton_Click;
		}

		private void CancelButton_Click(object sender, RoutedEventArgs e)
		{
			DialogResult = false;
			Close();
		}

		private void SaveButton_Click(object sender, RoutedEventArgs e)
		{
			if (!Validate())
				return;

			Status.StatusText = tagNameTextBox.Text;

			DialogResult = true;
			Close();
		}

		private bool Validate()
		{
			if (string.IsNullOrWhiteSpace(tagNameTextBox.Text))
			{
				MessageBox.Show("Status text cannot be empty", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
				return false;
			}

			return true;
		}
	}
}
