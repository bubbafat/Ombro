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
        public RainGauge()
        {
            InitializeComponent();
            this.DataContext = this;
            this.LayoutUpdated += RainGauge_LayoutUpdated;
        }

        void RainGauge_LayoutUpdated(object sender, EventArgs e)
        {

        }

        public static readonly DependencyProperty MaxDepthProperty =
            DependencyProperty.Register("MaxDepth", typeof(Double), typeof(RainGauge), new PropertyMetadata(null));

        public Double MaxDepth
        {
            get { return (Double)GetValue(MaxDepthProperty); }
            set { SetValue(MaxDepthProperty, value); }
        }

        public static readonly DependencyProperty DepthProperty =
            DependencyProperty.Register("Depth", typeof(Double), typeof(RainGauge), new PropertyMetadata(null));

        public Double Depth
        {
            get { return (Double)GetValue(DepthProperty); }
            set
            {
                double newWaterHeight;

                if(value >= MaxDepth)
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

                SetValue(DepthProperty, value);

                this.ChangeDepthStoryboard.Begin();
            }
        }
    }
}
