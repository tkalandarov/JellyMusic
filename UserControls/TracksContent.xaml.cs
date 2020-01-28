using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using JellyMusic.Core;

namespace JellyMusic.UserControls
{
    /// <summary>
    /// Interaction logic for HomeContent.xaml
    /// </summary>
    public partial class HomeContent : UserControl
    {
        public HomeContent()
        {
            InitializeComponent();
        }

        private void GoToActive_Click(object sender, RoutedEventArgs e)
        {
            TracksListBox.ScrollIntoView(TracksListBox.SelectedItem);
        }

        private void Sort_Click(object sender, RoutedEventArgs e)
        {
            SortPanel.Visibility = Visibility.Collapsed;
        }

        private void PlaylistsSort_Click(object sender, RoutedEventArgs e)
        {
            if (SortPanel.IsVisible)
                SortPanel.Visibility = Visibility.Collapsed;
            else
                SortPanel.Visibility = Visibility.Visible;
        }
    }
}
