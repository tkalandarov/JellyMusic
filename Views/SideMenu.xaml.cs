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
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using MaterialDesignThemes.Wpf;
using MaterialDesignThemes.Wpf.Transitions;

namespace JellyMusic.Views
{
    /// <summary>
    /// Interaction logic for SideMenu.xaml
    /// </summary>
    public partial class SideMenu : UserControl
    {
        public Transitioner ContentTransitioner { get; private set; }
        public SideMenu()
        {
            InitializeComponent();
            Loaded += (s, e) =>
            {
                ContentTransitioner = ((MainWindow)Window.GetWindow(this))?.ContentTransitioner;
            };
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

        private void ItemTracks_Selected(object sender, RoutedEventArgs e)
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
