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
	/// Interaction logic for EditTagModal.xaml
	/// </summary>
	public partial class EditTagModal : Window
	{
		public enum Mode
		{
			Edit,
			Create
		}

		public Tag Tag { get; set; }
		public Mode ModalMode { get; set; }

		public EditTagModal(Tag tag, Mode mode)
		{
			InitializeComponent();

			Tag = tag;
			ModalMode = mode;

			Title = mode == Mode.Edit ? "Edit Tag" : "Create Tag";

			tagNameTextBox.Text = tag.Name;

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
			if(!Validate())
				return;
			
			Tag.Name = tagNameTextBox.Text;

			DialogResult = true;
			Close();
		}

		private bool Validate()
		{
			if (string.IsNullOrWhiteSpace(tagNameTextBox.Text))
			{
				MessageBox.Show("Tag name cannot be empty", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
				return false;
			}

			return true;
		}
	}
}
