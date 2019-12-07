using System;
using System.IO;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media.Animation;

using JellyMusic.Models;
using JellyMusic.ViewModels;

using MaterialDesignThemes.Wpf;

namespace JellyMusic.Views
{
    public partial class MainWindow : Window
    {
        private MainViewModel ViewModel;

        public MainWindow()
        {
            InitializeComponent();

            ViewModel = new MainViewModel();
            Playbar.SetViewModel(ViewModel.PlaybarVM);
            DataContext = ViewModel;

            PlaylistContent.AddPlaylist.Click += (object sender, RoutedEventArgs e) =>
            {
                NewPlaylistDialog.Visibility = Visibility.Visible;
            };

            NewPlaylistDialog.CreateBttn.Click += (object sender, RoutedEventArgs e) =>
            {
                bool IsFolderBased = NewPlaylistDialog.TypeToggle.IsChecked != true;

                if (!IsFolderBased)
                {
                    using (var dialog = new OpenFileDialog { Title = "Select tracks to add", Multiselect = true })
                    {
                        dialog.Filter = "MP3 files|*.mp3";

                        if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                        {
                            if (dialog.FileNames.Length == 0) return;

                            ViewModel.playlistsVM.AddPlaylist(NewPlaylistDialog.NewPlaylistName.Text, dialog.FileNames);
                        }
                        else
                        {
                            return;
                        }
                    }
                }
                else
                {
                    using (var dialog = new FolderBrowserDialog())
                    {
                        if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                        {
                            if (Directory.GetFiles(dialog.SelectedPath, "*.mp3").Length == 0) return;
                            ViewModel.playlistsVM.AddPlaylist(NewPlaylistDialog.NewPlaylistName.Text, dialog.SelectedPath);
                        }
                        else
                        {
                            return;
                        }
                    }
                }

                NewPlaylistDialog.CancelBttn_Click(null, null);
                ViewModel.OnPropertyChanged("PlaylistsCollection");
            };
        }

        private void WindowCloseButton_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.PlaybarVM.AudioPlayer?.Dispose();
            Close();
        }

        private void WindowMinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void OnDragMoveWindow(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        private void ButtonOpenSideMenu_Checked(object sender, RoutedEventArgs e)
        {
            Storyboard sb = FindResource("OpenSideMenu") as Storyboard;
            sb.Begin();
        }

        private void ButtonOpenSideMenu_Unchecked(object sender, RoutedEventArgs e)
        {
            Storyboard sb = FindResource("CloseSideMenu") as Storyboard;
            sb.Begin();
        }

        private void PlaylistsCollection_IsMouseDirectlyOverChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            Console.WriteLine(e.NewValue.ToString());
        }

        private void ItemHome_Selected(object sender, RoutedEventArgs e)
        {
            ContentTransitioner.SelectedIndex = 0;
        }

        private void ItemPlaylists_Selected(object sender, RoutedEventArgs e)
        {
            ContentTransitioner.SelectedIndex = 1;
        }

        private void ItemFavorite_Selected(object sender, RoutedEventArgs e)
        {

        }

        private void ItemSettings_Selected(object sender, RoutedEventArgs e)
        {

        }

        #region Additional methods

        #endregion
    }
}
