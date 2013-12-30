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

            this.SizeChanged += RainGauge_SizeChanged;
        }

        void RainGauge_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            Resize(Depth);
        }

        public static readonly DependencyProperty DepthProperty =
            DependencyProperty.Register("Depth", typeof(Double), typeof(RainGauge), new PropertyMetadata(OnDepthPropertyChanged));

        private static void OnDepthPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            RainGauge gauge = (RainGauge)d;
            Double value = (Double)e.NewValue;

            gauge.Resize(value);
        }

        public Double Depth
        {
            get { return (Double)GetValue(DepthProperty); }
            set { SetValue(DepthProperty, value); }
        }

        private void Resize(Double value)
        {
            double newWaterHeight;

            if (value >= MaxDepth)
            {
                newWaterHeight = this.ActualHeight;
            }
            else if (value <= 0.0)
            {
                newWaterHeight = 0.0;
            }
            else
            {
                double percentage = value / MaxDepth;
                newWaterHeight = percentage * this.ActualHeight;
            }

            this.DepthAnimation.From = this.Water.Height;
            this.DepthAnimation.To = newWaterHeight;

            this.ChangeDepthStoryboard.Begin();
        }

    }
}
