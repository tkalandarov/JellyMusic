using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace JellyMusic.CustomControls
{
    public class WheelSpeedScrollViewer : ScrollViewer
    {
        public static readonly DependencyProperty SpeedFactorProperty =
            DependencyProperty.Register(nameof(SpeedFactor),
                                        typeof(Double),
                                        typeof(WheelSpeedScrollViewer),
                                        new PropertyMetadata(2.5));

        public Double SpeedFactor
        {
            get { return (Double)GetValue(SpeedFactorProperty); }
            set { SetValue(SpeedFactorProperty, value); }
        }

        protected override void OnPreviewMouseWheel(MouseWheelEventArgs e)
        {
            if (!e.Handled &&
                ScrollInfo is ScrollContentPresenter scp &&
                ComputedVerticalScrollBarVisibility == Visibility.Visible)
            {
                scp.SetVerticalOffset(VerticalOffset - e.Delta * SpeedFactor);
                e.Handled = true;
            }
        }
    };
}
