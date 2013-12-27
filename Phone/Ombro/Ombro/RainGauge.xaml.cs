using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;

namespace Ombro
{
    public partial class RainGauge : UserControl
    {
        private const double MaxDepth = 5.0;

        public RainGauge()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty DepthProperty =
            DependencyProperty.Register("Depth", typeof(Double), typeof(RainGauge), new PropertyMetadata(OnDepthPropertyChanged));

        private static void OnDepthPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            RainGauge gauge = (RainGauge)d;
            Double value = (Double)e.NewValue;

            double newWaterHeight;

            if (value >= MaxDepth)
            {
                newWaterHeight = gauge.ActualHeight;
            }
            else if (value <= 0.0)
            {
                newWaterHeight = 0.0;
            }
            else
            {
                double percentage = value / MaxDepth;
                newWaterHeight = percentage * gauge.ActualHeight;
            }

            gauge.DepthAnimation.From = gauge.Water.Height;
            gauge.DepthAnimation.To = newWaterHeight;

            gauge.ChangeDepthStoryboard.Begin();
        }

        public Double Depth
        {
            get { return (Double)GetValue(DepthProperty); }
            set { SetValue(DepthProperty, value); }
        }
    }
}
