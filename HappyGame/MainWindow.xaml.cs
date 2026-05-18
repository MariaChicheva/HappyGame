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
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace HappyGame
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            StartRoadAnimation();
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                this.Close();
            }
        }

        private void StartRoadAnimation()
        {
            double roadHeight = 648.5;

            DoubleAnimation anim = new DoubleAnimation();
            anim.From = 0;
            anim.To = roadHeight;
            anim.Duration = TimeSpan.FromSeconds(5);
            anim.RepeatBehavior = RepeatBehavior.Forever;

            Road.BeginAnimation(Canvas.TopProperty, anim);

            DoubleAnimation anim2 = new DoubleAnimation();
            anim2.From = -roadHeight;
            anim2.To = 0;
            anim2.Duration = TimeSpan.FromSeconds(5);
            anim2.RepeatBehavior = RepeatBehavior.Forever;

            RoadBack.BeginAnimation(Canvas.TopProperty, anim2);
        }
    }
}
