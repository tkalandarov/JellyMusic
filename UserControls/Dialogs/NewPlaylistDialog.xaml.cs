using JellyMusic.Core;
using JellyMusic.ViewModels;

using System;
using System.Collections.Generic;
using System.IO;
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

namespace JellyMusic.UserControls
{
    /// <summary>
    /// Interaction logic for NewPlaylistDialog.xaml
    /// </summary>
    public partial class NewPlaylistDialog : UserControl
    {
        public NewPlaylistDialog()
        {
            InitializeComponent();
        }

        public void CancelBttn_Click(object sender, RoutedEventArgs e)
        {
            NewPlaylistName.Clear();
            TypeToggle.IsChecked = false;
            this.Visibility = Visibility.Collapsed;
        }

        private void NewPlaylistName_TextChanged(object sender, TextChangedEventArgs e)
        {
            string name = NewPlaylistName.Text;
            string newPath = Path.Combine(PlaylistsViewModel.PlaylistsDir, name + ".json");
            bool IsValid = IOService.IsValidFilename(name) && !String.IsNullOrWhiteSpace(name) && !File.Exists(newPath);

            CreateBttn.IsEnabled = IsValid;
        }
    }
}
