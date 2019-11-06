using JellyMusic.Core;
using JellyMusic.ViewModels;
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
using System.Windows.Threading;

namespace JellyMusic.Views
{
    /// <summary>
    /// Interaction logic for PlaybarView.xaml
    /// </summary>
    public partial class PlaybarView : UserControl
    {
        public PlaybarViewModel ViewModel { get; set; }
        public PlaybarView()
        {
            InitializeComponent();
        }

        public void SetViewModel(PlaybarViewModel viewModel)
        {
            ViewModel = viewModel;
            DataContext = viewModel;
        }

        private void ProgressBar_DragStarted(object sender, System.Windows.Controls.Primitives.DragStartedEventArgs e)
        {
            ViewModel.DragStarted();
        }

        private void ProgressBar_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {
            ViewModel.DragCompleted();
        }
    }
}
