using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace JellyMusic.Views
{
    /// <summary>
    /// Interaction logic for TitleBar.xaml
    /// </summary>
    public partial class TitleBar : UserControl
    {
        public MainWindow MainWindow { get; private set; }
        public TitleBar()
        {
            InitializeComponent();
            Loaded += (s, e) =>
            {
                MainWindow = (MainWindow)Window.GetWindow(this);
            };
        }

        private void WindowCloseButton_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.OnWindowClose();
        }
        private void WindowMinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.WindowState = WindowState.Minimized;
        }

        private void OnDragMoveWindow(object sender, MouseButtonEventArgs e)
        {
            MainWindow.DragMove();
        }
    }
}
