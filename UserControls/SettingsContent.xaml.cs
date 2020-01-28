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
        public event Action AppRestartRequired;
        public SettingsContent()
        {
            InitializeComponent();

            AssignSettingsValues();
        }

        private void AssignSettingsValues()
        {
            IntroToggle.IsChecked = App.Settings.IntroEnabled;
            IntroSoundToggle.IsChecked = App.Settings.IntroMuted;
            VirtualizationToggle.IsChecked = App.Settings.Virtualization;
        }

        private void IntroToggle_Clcik(object sender, RoutedEventArgs e)
        {
            App.Settings.IntroEnabled = IntroToggle.IsChecked.HasValue && IntroToggle.IsChecked.Value;
        }

        private void IntroMutedToggle_Click(object sender, RoutedEventArgs e)
        {
            App.Settings.IntroMuted = IntroSoundToggle.IsChecked.HasValue && IntroSoundToggle.IsChecked.Value;
        }

        private void VirtualizationToggle_Click(object sender, RoutedEventArgs e)
        {
            App.Settings.Virtualization = VirtualizationToggle.IsChecked.HasValue && VirtualizationToggle.IsChecked.Value;
            AppRestartRequired.Invoke();
        }

        private void ResetDefaultsButton_Click(object sender, RoutedEventArgs e)
        {
            bool oldVirtValue = App.Settings.Virtualization;

            App.CreateDedaultAppSettings();
            AssignSettingsValues();

            if (App.Settings.Virtualization != oldVirtValue)
                AppRestartRequired.Invoke();
        }
    }
}
