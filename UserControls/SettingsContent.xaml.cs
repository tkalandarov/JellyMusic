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
        private bool _oldVirtValue = App.Settings.Virtualization;

        public SettingsContent()
        {
            InitializeComponent();

            Loaded += (s, e) =>
            {
                AssignSettingsValues();
            };
        }

        private void AssignSettingsValues()
        {
            if (App.Settings == null) return;

            VirtualizationToggle.IsChecked = App.Settings?.Virtualization;
        }

        private void VirtualizationToggle_Click(object sender, RoutedEventArgs e)
        {
            bool ToggleValue = VirtualizationToggle.IsChecked.HasValue && VirtualizationToggle.IsChecked.Value;

            if (ToggleValue == _oldVirtValue)
                return;

            App.Settings.Virtualization = ToggleValue;
            AppRestartRequired.Invoke();
        }

        private void ResetDefaultsButton_Click(object sender, RoutedEventArgs e)
        {
            App.CreateDedaultAppSettings();
            AssignSettingsValues();

            if (App.Settings.Virtualization != _oldVirtValue)
                AppRestartRequired.Invoke();
        }
    }
}
