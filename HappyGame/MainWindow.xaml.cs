using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace HappyGame
{
    public partial class MainWindow : Window
    {
        private double carSpeed = 10;
        private double carMinLeft = 100;
        private double carMaxLeft = 540;

        private double roadAnimationSpeed = 5;
        private double roadHeight = 648.5;

        private double currentRoadTop = 0;
        private double currentRoadBackTop = -648.5;

        private DispatcherTimer animationTimer;
        private DispatcherTimer speedChangeTimer;
        private double targetRoadSpeed = 5;
        private bool isSpeedChanging = false;

        public MainWindow()
        {
            InitializeComponent();

            Canvas.SetTop(Road, currentRoadTop);
            Canvas.SetTop(RoadBack, currentRoadBackTop);

            StartRoadAnimation();
            GameCanvas.Focus();

            UpdateSpeedDisplay();
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                this.Close();
            }

            if (e.Key == Key.Left || e.Key == Key.A)
            {
                MoveCar(-carSpeed);
            }
            else if (e.Key == Key.Right || e.Key == Key.D)
            {
                MoveCar(carSpeed);
            }
            else if (e.Key == Key.W)
            {
                IncreaseSpeed();
            }
            else if (e.Key == Key.S)
            {
                DecreaseSpeed();
            }
        }

        private void MoveCar(double offset)
        {
            double newLeft = Canvas.GetLeft(Car) + offset;
            if (newLeft >= carMinLeft && newLeft <= carMaxLeft)
            {
                Canvas.SetLeft(Car, newLeft);
            }
        }

        private void IncreaseSpeed()
        {
            if (carSpeed < 300)
            {
                double targetCarSpeed = Math.Min(carSpeed + 5, 40);
                double targetRoadSpeed = Math.Min(roadAnimationSpeed + 0.9, 20);

                SmoothSpeedChange(targetCarSpeed, targetRoadSpeed);
            }
        }

        private void DecreaseSpeed()
        {
            if (carSpeed > 4)
            {
                double targetCarSpeed = Math.Max(carSpeed - 5, 9);
                double targetRoadSpeed = Math.Max(roadAnimationSpeed - 0.9, 2);

                SmoothSpeedChange(targetCarSpeed, targetRoadSpeed);
            }
        }

        private void SmoothSpeedChange(double targetCarSpeed, double targetRoadSpeed)
        {
            if (speedChangeTimer != null)
            {
                speedChangeTimer.Stop();
            }

            isSpeedChanging = true;
            double startCarSpeed = carSpeed;
            double startRoadSpeed = roadAnimationSpeed;
            double duration = 0.3;

            speedChangeTimer = new DispatcherTimer();
            speedChangeTimer.Interval = TimeSpan.FromMilliseconds(16);
            DateTime changeStartTime = DateTime.Now;

            speedChangeTimer.Tick += (s, e) =>
            {
                double elapsed = (DateTime.Now - changeStartTime).TotalSeconds;
                double t = Math.Min(1.0, elapsed / duration);

                double easeOut = 1 - Math.Pow(1 - t, 3);

                carSpeed = startCarSpeed + (targetCarSpeed - startCarSpeed) * easeOut;
                roadAnimationSpeed = startRoadSpeed + (targetRoadSpeed - startRoadSpeed) * easeOut;

                UpdateSpeedDisplay();

                if (t >= 1.0)
                {
                    carSpeed = targetCarSpeed;
                    roadAnimationSpeed = targetRoadSpeed;
                    speedChangeTimer.Stop();
                    isSpeedChanging = false;
                    UpdateSpeedDisplay();
                }
            };

            speedChangeTimer.Start();
        }

        private void StartRoadAnimation()
        {
            animationTimer = new DispatcherTimer();
            animationTimer.Interval = TimeSpan.FromMilliseconds(16);
            animationTimer.Tick += UpdateRoadAnimation;
            animationTimer.Start();
        }

        private void UpdateRoadAnimation(object sender, EventArgs e)
        {
            double delta = roadAnimationSpeed * animationTimer.Interval.TotalSeconds * 60;

            currentRoadTop += delta;
            currentRoadBackTop += delta;

            if (currentRoadTop >= roadHeight)
            {
                currentRoadTop = currentRoadBackTop - roadHeight;
            }

            if (currentRoadBackTop >= roadHeight)
            {
                currentRoadBackTop = currentRoadTop - roadHeight;
            }

            if (currentRoadTop < -roadHeight)
            {
                currentRoadTop = currentRoadBackTop + roadHeight;
            }

            if (currentRoadBackTop < -roadHeight)
            {
                currentRoadBackTop = currentRoadTop + roadHeight;
            }

            Canvas.SetTop(Road, currentRoadTop);
            Canvas.SetTop(RoadBack, currentRoadBackTop);
        }

        private void UpdateSpeedDisplay()
        {
            this.Title = $"Happy Game - Speed: {carSpeed:F1}";
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            if (animationTimer != null)
            {
                animationTimer.Stop();
            }
            if (speedChangeTimer != null)
            {
                speedChangeTimer.Stop();
            }
        }
    }
}