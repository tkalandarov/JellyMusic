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
    }
}
