using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using System.Windows.Media;
using System.Windows.Shapes;

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
        private DispatcherTimer coinSpawnTimer;
        private double targetRoadSpeed = 5;
        private bool isSpeedChanging = false;

        // Система монеток
        private List<Coin> coins = new List<Coin>();
        private int coinScore = 0;
        private Random random = new Random();
        private double coinSpawnInterval = 1.0;
        private double coinSize = 30;
        private double coinBaseSpeed = 5;

        public MainWindow()
        {
            InitializeComponent();
            GameCanvas.Focus();

            Canvas.SetTop(Road, currentRoadTop);
            Canvas.SetTop(RoadBack, currentRoadBackTop);

            StartRoadAnimation();
            StartCoinSpawning();

            UpdateSpeedDisplay();
            UpdateCoinDisplay();
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

            UpdateCoinsPosition(delta);
            CheckCoinCollection();
        }

        private void StartCoinSpawning()
        {
            coinSpawnTimer = new DispatcherTimer();
            coinSpawnTimer.Interval = TimeSpan.FromSeconds(coinSpawnInterval);
            coinSpawnTimer.Tick += SpawnCoin;
            coinSpawnTimer.Start();
        }

        private void SpawnCoin(object sender, EventArgs e)
        {
            double minLeft = carMinLeft;
            double maxLeft = carMaxLeft;
            double randomLeft = minLeft + random.NextDouble() * (maxLeft - minLeft);

            double top = -coinSize;

            Coin newCoin = new Coin
            {
                Shape = new Ellipse
                {
                    Width = coinSize,
                    Height = coinSize,
                    Fill = new SolidColorBrush(Colors.Gold),
                    Stroke = new SolidColorBrush(Colors.Orange),
                    StrokeThickness = 2
                },
                X = randomLeft,
                Y = top,
                IsCollected = false
            };

            var gradient = new RadialGradientBrush();
            gradient.GradientStops.Add(new GradientStop(Colors.Gold, 0));
            gradient.GradientStops.Add(new GradientStop(Colors.Orange, 1));
            newCoin.Shape.Fill = gradient;

            TextBlock coinText = new TextBlock
            {
                Text = "💰",
                FontSize = 20,
                Foreground = new SolidColorBrush(Colors.DarkGoldenrod),
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };

            Canvas.SetLeft(newCoin.Shape, newCoin.X);
            Canvas.SetTop(newCoin.Shape, newCoin.Y);
            GameCanvas.Children.Add(newCoin.Shape);

            coins.Add(newCoin);
        }

        private void UpdateCoinsPosition(double delta)
        {
            for (int i = coins.Count - 1; i >= 0; i--)
            {
                Coin coin = coins[i];
                if (!coin.IsCollected)
                {
                    coin.Y += delta;

                    if (coin.Y > GameCanvas.Height)
                    {
                        GameCanvas.Children.Remove(coin.Shape);
                        coins.RemoveAt(i);
                    }
                    else
                    {
                        Canvas.SetTop(coin.Shape, coin.Y);
                    }
                }
                else
                {
                    GameCanvas.Children.Remove(coin.Shape);
                    coins.RemoveAt(i);
                }
            }
        }

        private void CheckCoinCollection()
        {
            double carLeft = Canvas.GetLeft(Car);
            double carTop = Canvas.GetTop(Car);
            double carRight = carLeft + Car.Width;
            double carBottom = carTop + Car.Height;

            for (int i = coins.Count - 1; i >= 0; i--)
            {
                Coin coin = coins[i];
                if (!coin.IsCollected)
                {
                    double coinLeft = coin.X;
                    double coinRight = coin.X + coinSize;
                    double coinTop = coin.Y;
                    double coinBottom = coin.Y + coinSize;

                    if (carLeft < coinRight && carRight > coinLeft &&
                        carTop < coinBottom && carBottom > coinTop)
                    {

                        coin.IsCollected = true;
                        coinScore++;
                        UpdateCoinDisplay();

                        ShowCoinCollectEffect(coin.X, coin.Y);

                    }
                }
            }
        }

        private void ShowCoinCollectEffect(double x, double y)
        {
            TextBlock effect = new TextBlock
            {
                Text = "+1 💰",
                FontSize = 24,
                FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush(Colors.Gold),
                Opacity = 1
            };

            Canvas.SetLeft(effect, x);
            Canvas.SetTop(effect, y);
            GameCanvas.Children.Add(effect);

            DispatcherTimer fadeTimer = new DispatcherTimer();
            fadeTimer.Interval = TimeSpan.FromMilliseconds(50);
            int fadeCount = 0;

            fadeTimer.Tick += (s, e) =>
            {
                fadeCount++;
                effect.Opacity -= 0.1;
                Canvas.SetTop(effect, Canvas.GetTop(effect) - 2);

                if (fadeCount >= 10)
                {
                    fadeTimer.Stop();
                    GameCanvas.Children.Remove(effect);
                }
            };
            fadeTimer.Start();

            var originalLeft = Canvas.GetLeft(Car);
            DispatcherTimer vibrateTimer = new DispatcherTimer();
            vibrateTimer.Interval = TimeSpan.FromMilliseconds(30);
            int vibrateCount = 0;

            vibrateTimer.Tick += (v, ev) =>
            {
                vibrateCount++;
                if (vibrateCount % 2 == 0)
                    Canvas.SetLeft(Car, originalLeft + 2);
                else
                    Canvas.SetLeft(Car, originalLeft - 2);

                if (vibrateCount >= 6)
                {
                    vibrateTimer.Stop();
                    Canvas.SetLeft(Car, originalLeft);
                }
            };
            vibrateTimer.Start();
        }

        private void UpdateCoinDisplay()
        {
            CoinCounterText.Text = $"💰 {coinScore}";

            if (coinScore >= 10)
                CoinCounterText.Foreground = new SolidColorBrush(Colors.LightGreen);
            else if (coinScore >= 5)
                CoinCounterText.Foreground = new SolidColorBrush(Colors.LightBlue);
            else
                CoinCounterText.Foreground = new SolidColorBrush(Colors.Gold);
        }

        private void UpdateSpeedDisplay()
        {
            this.Title = $"Happy Game - Speed: {carSpeed:F1} - Coins: {coinScore}";
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
            if (coinSpawnTimer != null)
            {
                coinSpawnTimer.Stop();
            }

            foreach (var coin in coins)
            {
                if (coin.Shape != null)
                    GameCanvas.Children.Remove(coin.Shape);
            }
            coins.Clear();
        }
    }
    public class Coin
    {
        public System.Windows.Shapes.Shape Shape { get; set; }
        public double X { get; set; }
        public double Y { get; set; }
        public bool IsCollected { get; set; }
    }
}