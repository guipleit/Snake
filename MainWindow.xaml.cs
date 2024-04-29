﻿using Snake.Classes;
using Snake.Enums;
using System.Diagnostics;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Snake
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly Dictionary<GridValue, ImageSource> gridValToImage = new()
        {
            { GridValue.Empty, Images.Empty },
            { GridValue.Snake, Images.Body },
            { GridValue.Food, Images.Food }
        };

        private readonly Dictionary<Direction, int> ditToRotation = new()
        {
            { Direction.Up, 0 },
            { Direction.Right, 90 },
            { Direction.Down, 180 },
            { Direction.Left, 270 },
        };

        private readonly int rows = 15, cols = 15;
        private readonly Image[,] gridImages;
        private GameLogic gameLogic;
        private bool gameRunning = false;

        public MainWindow()
        {
            InitializeComponent();
            gridImages = SetupGrid();
            gameLogic = new GameLogic(rows, cols);
        }

        private async Task RunGame()
        {
            Draw();
            await ShowCountDown();
            Overlay.Visibility = Visibility.Hidden;
            await GameLoop();
            await ShowGameOver();
            gameLogic = new GameLogic(rows, cols);
        }

        private async void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if(Overlay.Visibility == Visibility.Visible)
            {
                e.Handled = true;
            }

            if(!gameRunning)
            {
                gameRunning = true;
                await RunGame();
                gameRunning = false;
            }
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (gameLogic.GameOver)
            {
                return;
            }

            switch (e.Key)
            {
                case Key.Left:
                    gameLogic.ChangeDirection(Direction.Left);
                    break;
                case Key.Right:
                    gameLogic.ChangeDirection(Direction.Right);
                    break;
                case Key.Up:
                    gameLogic.ChangeDirection(Direction.Up);
                    break;
                case Key.Down:
                    gameLogic.ChangeDirection(Direction.Down);
                    break;
                case Key.A:
                    gameLogic.ChangeDirection(Direction.Left);
                    break;
                case Key.D:
                    gameLogic.ChangeDirection(Direction.Right);
                    break;
                case Key.W:
                    gameLogic.ChangeDirection(Direction.Up);
                    break;
                case Key.S:
                    gameLogic.ChangeDirection(Direction.Down);
                    break;

            }
        }

        private async Task GameLoop()
        {
            while(!gameLogic.GameOver)
            {
                await Task.Delay(110);
                gameLogic.Move();
                Draw();
            }
        }

        private Image[,] SetupGrid()
        {
            Image[,] images = new Image[rows, cols];
            
            GameGrid.Rows = rows;
            GameGrid.Columns = cols;
            
            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < cols; c++)
                {
                    Image image = new Image()
                    {
                        Source = Images.Empty,
                        RenderTransformOrigin = new Point(0.5, 0.5)
                    };

                    images[r, c] = image;
                    GameGrid.Children.Add(image);
                }
            }

            return images;
        }

        private void Draw()
        {
            DrawGrid();
            DrawSnakeHead();
            ScoreText.Text = $"SCORE {gameLogic.Score}";
        }

        private void DrawGrid()
        {
            for(int r = 0; r < rows; r++)
            {
                for(int c = 0; c < cols; c++)
                {
                    GridValue gridVal = gameLogic.Grid[r, c];
                    gridImages[r, c].Source = gridValToImage[gridVal];
                    gridImages[r, c].RenderTransform = Transform.Identity;
                }
            }
        }

        private async Task ShowCountDown()
        {
            for (int i = 3; i >= 1; i--)
            {
                OverlayText.Text = i.ToString();
                await Task.Delay(500);
            }
            await Task.Delay(100);
        }

        private async Task ShowGameOver()
        {
            await Task.Delay(500);
            await DrawDeadSnake();
            await Task.Delay(500);

            Overlay.Visibility = Visibility.Visible;
            OverlayText.Text = @"
            GAME OVER

            PRESS ANY KEY TO RESTART";
        }

        private void DrawSnakeHead()
        {
            Position headPos = gameLogic.HeadPosition();
            Image image = gridImages[headPos.Row, headPos.Col];

            image.Source = Images.Head;

            int rotation = ditToRotation[gameLogic.Dir];
            image.RenderTransform = new RotateTransform(rotation);
        }

        private async Task DrawDeadSnake()
        {
            List<Position> positions = new List<Position>(gameLogic.SnakePositions());

            for (int i = 0; i < positions.Count; i++)
            {
                Position pos = positions[i];
                ImageSource source = (i == 0) ? Images.DeadHead : Images.DeadBody;
                gridImages[pos.Row, pos.Col].Source = source;
                await Task.Delay(80);
            }
        }
    }
}