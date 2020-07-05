using System;
using System.IO;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media.Animation;

using JellyMusic.ViewModels;

namespace JellyMusic.Views
{
    public partial class MainWindow : Window
    {
        private MainViewModel MainVM;

        public MainWindow()
        {
            InitializeComponent();

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

            SettingsContent.AppRestartRequired += () =>
            {
                RestartDialog.Visibility = Visibility.Visible;
            };

        }


        public void OnWindowClose()
        {
            MainVM.PlaybarVM.AudioPlayer?.Dispose();
            Close();
        }
    }
}
