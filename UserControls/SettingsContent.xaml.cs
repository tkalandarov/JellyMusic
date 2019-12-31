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

namespace JellyMusic.UserControls
{
    /// <summary>
    /// Interaction logic for SettingsContent.xaml
    /// </summary>
    public partial class SettingsContent : UserControl
    {
        public SettingsContent()
        {
            InitializeComponent();

            IntroToggle.IsChecked = App.Settings.IntroEnabled;
        }

        private void IntroToggle_Clcik(object sender, RoutedEventArgs e)
        {
            App.Settings.IntroEnabled = IntroToggle.IsChecked.HasValue && IntroToggle.IsChecked.Value;
        }
    }
}
