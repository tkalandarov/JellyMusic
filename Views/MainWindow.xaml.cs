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
        }

        private void WindowCloseButton_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.PlaybarVM.AudioPlayer?.Dispose();
            ViewModel.SaveAppSettings();
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
    }
}
