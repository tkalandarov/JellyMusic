using System;
using System.IO;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media.Animation;
using JellyMusic.Core;
using JellyMusic.Models;
using JellyMusic.ViewModels;

using MaterialDesignThemes.Wpf;

namespace JellyMusic.Views
{
    public partial class MainWindow : Window
    {
        private MainViewModel MainVM;

        public MainWindow()
        {
            InitializeComponent();

            if (App.Settings.IntroEnabled)
            {
                Storyboard sb = FindResource("Startup") as Storyboard;
                sb.Begin();

                IntroVideo.Source = new Uri("pack://siteoforigin:,,,/Assets/Intro.mov", UriKind.Absolute);
            }

            MainVM = new MainViewModel();
            Playbar.SetViewModel(MainVM.PlaybarVM);

            DataContext = MainVM;

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

                            MainVM.PlaylistsVM.AddPlaylist(NewPlaylistDialog.NewPlaylistName.Text, dialog.FileNames);
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
                            MainVM.PlaylistsVM.AddPlaylist(NewPlaylistDialog.NewPlaylistName.Text, dialog.SelectedPath);
                        }
                        else
                        {
                            return;
                        }
                    }
                }

                NewPlaylistDialog.CancelBttn_Click(null, null);
            };
        }

        private void IntroVideo_MediaEnded(object sender, RoutedEventArgs e)
        {
            Storyboard sb = FindResource("VideoFade") as Storyboard;
            sb.Completed += (sndr, args) =>
            {
                IntroVideo.Source = null;
            };
            sb.Begin();
        }


        private void WindowCloseButton_Click(object sender, RoutedEventArgs e)
        {
            MainVM.PlaybarVM.AudioPlayer?.Dispose();
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

        private void ItemHome_Selected(object sender, RoutedEventArgs e)
        {
            ContentTransitioner.SelectedIndex = 0;
        }
        private void ItemPlaylists_Selected(object sender, RoutedEventArgs e)
        {
            ContentTransitioner.SelectedIndex = 1;
        }
        private void ItemSearch_Selected(object sender, RoutedEventArgs e)
        {
            ContentTransitioner.SelectedIndex = 2;
        }
        private void ItemSettings_Selected(object sender, RoutedEventArgs e)
        {
            ContentTransitioner.SelectedIndex = 3;
        }
    }
}
