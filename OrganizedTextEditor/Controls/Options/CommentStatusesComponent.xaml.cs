using OrganizedTextEditor.Classes;
using OrganizedTextEditor.Windows.Modals;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace OrganizedTextEditor.Controls.Options
{
	/// <summary>
	/// Interaction logic for CommentStatusesComponent.xaml
	/// </summary>
	public partial class CommentStatusesComponent : UserControl
	{
		private List<CommentStatus> CommentStatuses { get; set; }
		public CommentStatusesComponent()
		{
			InitializeComponent();

			addStatusButton.Click += AddStatusButton_Click;
		}

		public void LoadStatuses(List<CommentStatus> statuses)
		{
			CommentStatuses = statuses;
			BuildStatusesList();
		}

		public void BuildStatusesList()
		{
			statusStackPanel.Children.Clear();

			foreach (CommentStatus status in CommentStatuses)
			{
				Grid grid = new Grid();
				grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });
				grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Auto) });
				grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Auto) });

				grid.Margin = new Thickness(0, 0, 5, 5);

				Label label = new Label();
				label.Content = status.StatusText;
				label.FontSize = 16;
				label.VerticalAlignment = VerticalAlignment.Center;
				label.HorizontalAlignment = HorizontalAlignment.Left;

				grid.Children.Add(label);

				Button editButton = new Button();
				editButton.Content = IconBuilder.BuildIcon("Edit.png");
				editButton.Width = 16;
				editButton.Height = 16;
				editButton.Margin = new Thickness(5, 0, 0, 0);
				editButton.Background = Brushes.Transparent;
				editButton.BorderThickness = new Thickness(0);

				editButton.Click += (sender, e) =>
				{
					EditCommentStatusModal modal = new EditCommentStatusModal(status, EditCommentStatusModal.Mode.Edit);
					modal.Owner = Window.GetWindow(this);
					if (modal.ShowDialog() == true)
					{
						BuildStatusesList();
					}
				};

				Grid.SetColumn(editButton, 1);
				grid.Children.Add(editButton);

				Button deleteButton = new Button();
				deleteButton.Content = IconBuilder.BuildIcon("Delete.png");
				deleteButton.Width = 16;
				deleteButton.Height = 16;
				deleteButton.Margin = new Thickness(5, 0, 0, 0);
				deleteButton.Background = Brushes.Transparent;
				deleteButton.BorderThickness = new Thickness(0);

				deleteButton.Click += (sender, e) =>
				{
					if (MessageBox.Show("Are you sure you want to delete this status?", "Delete Status", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
					{
						CommentStatuses.Remove(status);
						BuildStatusesList();
					}
				};

				Grid.SetColumn(deleteButton, 2);
				grid.Children.Add(deleteButton);

				statusStackPanel.Children.Add(grid);
			}
		}

		private void AddStatusButton_Click(object sender, RoutedEventArgs e)
		{
			CommentStatus newStatus = new CommentStatus();

			EditCommentStatusModal modal = new EditCommentStatusModal(newStatus, EditCommentStatusModal.Mode.Create);
			modal.Owner = Window.GetWindow(this);
			if (modal.ShowDialog() == true)
			{
				CommentStatuses.Add(newStatus);
				BuildStatusesList();
			}
		}
	}
}
