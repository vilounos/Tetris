using System;
using System.Collections.Generic;
using System.Linq;
using System.Media;
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
using static System.Formats.Asn1.AsnWriter;

namespace Tetris
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // Porměnné
        bool pause = false;
        bool success = false;
        static bool music = true;
        static System.Media.SoundPlayer musicPlayer = new System.Media.SoundPlayer();


        // Textury
        private ImageSource[] tileImages = new ImageSource[]
        {
            new BitmapImage(new Uri("Assets/TileEmpty.png", UriKind.Relative)),
            new BitmapImage(new Uri("Assets/TileCyan.png", UriKind.Relative)),
            new BitmapImage(new Uri("Assets/TileBlue.png", UriKind.Relative)),
            new BitmapImage(new Uri("Assets/TileOrange.png", UriKind.Relative)),
            new BitmapImage(new Uri("Assets/TileYellow.png", UriKind.Relative)),
            new BitmapImage(new Uri("Assets/TileGreen.png", UriKind.Relative)),
            new BitmapImage(new Uri("Assets/TilePurple.png", UriKind.Relative)),
            new BitmapImage(new Uri("Assets/TileRed.png", UriKind.Relative))
        };

        private readonly ImageSource[] blockImages = new ImageSource[]
        {
            new BitmapImage(new Uri("Assets/Block-Empty.png", UriKind.Relative)),
            new BitmapImage(new Uri("Assets/Block-I.png", UriKind.Relative)),
            new BitmapImage(new Uri("Assets/Block-J.png", UriKind.Relative)),
            new BitmapImage(new Uri("Assets/Block-L.png", UriKind.Relative)),
            new BitmapImage(new Uri("Assets/Block-O.png", UriKind.Relative)),
            new BitmapImage(new Uri("Assets/Block-S.png", UriKind.Relative)),
            new BitmapImage(new Uri("Assets/Block-T.png", UriKind.Relative)),
            new BitmapImage(new Uri("Assets/Block-Z.png", UriKind.Relative))
        };

        // Předem nastavené proměnné
        private readonly Image[,] imageControls;
        private readonly int maxDelay = 1000;
        private readonly int minDelay = 60;
        private readonly int delayDecrease = 15;

        private GameState gameState = new GameState();
        
        public MainWindow()
        {
            InitializeComponent();
            imageControls = SetupGameCanvas(gameState.GameGrid);
        }


        // Nastavení visuálného prostředí
        private Image[,] SetupGameCanvas(GameGrid grid)
        {
            Image[,] imageControls = new Image[grid.Rows, grid.Columns];
            int cellSize = 25;

            for (int r = 0; r < grid.Rows; r++)
            {
                for (int c = 0; c < grid.Columns; c++)
                {
                    Image imageControl = new Image
                    {
                        Width = cellSize,
                        Height = cellSize
                    };

                    Canvas.SetTop(imageControl, (r - 2) * cellSize + 10);
                    Canvas.SetLeft(imageControl, c * cellSize);
                    GameCanvas.Children.Add(imageControl);
                    imageControls[r, c] = imageControl;
                }
            }

            return imageControls;
        }


        // Vykreslení mřížky
        private void DrawGrid(GameGrid grid)
        {
            for (int r = 0; r < grid.Rows; r++)
            {
                for (int c = 0; c < grid.Columns; c++)
                {
                    int id = grid[r, c];
                    imageControls[r, c].Opacity = 1;
                    imageControls[r, c].Source = tileImages[id];
                }
            }
        }


        // Vykreslení dílků
        private void DrawBlock(Block block)
        {
            foreach (Position p in block.TilePositions())
            {
                imageControls[p.Row, p.Column].Opacity = 1;
                imageControls[p.Row, p.Column].Source = tileImages[block.Id];
            }
        }


        // Vykreslení dalšího dílku
        private void DrawNextBlock(BlockQueue blockQueue)
        {
            Block next = blockQueue.NextBlock;
            NextImage.Source = blockImages[next.Id];
        }


        // Vykreslení držícího dílku
        private void DrawHeldBlock(Block heldBlock)
        {
            if (heldBlock == null)
            {
                HoldImage.Source = blockImages[0];
            }
            else
            {
                HoldImage.Source = blockImages[heldBlock.Id];
            }
        }


        // Vykreslení předpovědi dopadu
        private void DrawGhostBlock(Block block)
        {
            int dropDistance = gameState.BlockDropDistance();

            foreach (Position p in block.TilePositions())
            {
                imageControls[p.Row + dropDistance, p.Column].Opacity = 0.25;
                imageControls[p.Row + dropDistance, p.Column].Source = tileImages[block.Id];
            }
        }


        // COelkové vykreslení
        private void Draw(GameState gameState)
        {
            DrawGrid(gameState.GameGrid);
            DrawGhostBlock(gameState.CurrentBlock);
            DrawBlock(gameState.CurrentBlock);
            DrawNextBlock(gameState.BlockQueue);
            DrawHeldBlock(gameState.HeldBlock);
            ScoreText.Text = $"Score: {gameState.Score}";
        }

        // Hlavní smyčka hry
        private async Task GameLoop()
        {
            Draw(gameState);         
            PlayMusic();
            while (!gameState.GameOver)
            {
                int delay = Math.Max(minDelay, maxDelay - (gameState.Score * delayDecrease));
                await Task.Delay(delay);
                if (!pause)
                {
                    gameState.MoveBlockDown();
                }

                if (gameState.Score >= 100)
                {
                    success = true;
                    gameState.GameOver = true;
                }
                Draw(gameState);
            }

            if (success)
            {
                FinalScoreTextS.Text = $"Score: {gameState.Score}";
                PlayGameSuccess();
            }
            else if (!success)
            {
                GameOverMenu.Visibility = Visibility.Visible;
                FinalScoreText.Text = $"Score: {gameState.Score}";
            }
        }

        // Hudba v pozadí
        public static void PlayMusic()
        {
            if (music)
            {
                musicPlayer.SoundLocation = "mainsong.wav";
                musicPlayer.PlayLooping();
            }
            else if (!music)
            {
                musicPlayer.Stop();
            }
        }


        // Ovládání klávesnicí
        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (gameState.GameOver)
            {
                return;
            }

            switch (e.Key)
            {
                case Key.A:
                    if (!pause)
                    {
                        gameState.MoveBlockLeft();
                    }
                    break;
                case Key.D:
                    if (!pause)
                    {
                        gameState.MoveBlockRight();
                    }
                    break;
                case Key.S:
                    if (!pause)
                    {
                        gameState.MoveBlockDown();
                    }
                    break;
                case Key.E:
                    if (!pause)
                    {
                        gameState.RotateBlockCW();
                    }
                    break;
                case Key.Q:
                    if (!pause)
                    {
                        gameState.RotateBlockCCW();
                    }
                    break;
                case Key.W:
                    if (!pause)
                    {
                        gameState.HoldBlock();
                    }
                    break;
                case Key.Space:
                    if (!pause)
                    {
                        gameState.DropBlock();
                    }
                    break;
                case Key.Escape:
                    if (!pause)
                    {
                            pause = true;
                            music = false;
                            PlayMusic();
                            GamePauseMenu.Visibility = Visibility.Visible;
                    }
                    else if (pause)
                    {
                        pause = false;
                        music = true;
                        PlayMusic();
                        GamePauseMenu.Visibility = Visibility.Hidden;
                        GameHelpMenu.Visibility = Visibility.Hidden;
                    }
                    break;
                default:
                    return;
            }

            Draw(gameState);
        }


        // Když se načte GameCanvas
        private async void GameCanvas_Loaded(object sender, RoutedEventArgs e)
        {
            await GameLoop();
        }


        // Kliknutí na tlačíkto Play Again
        private async void PlayAgain_Click(object sender, RoutedEventArgs e)
        {
            success = false;
            gameState = new GameState();
            GameOverMenu.Visibility = Visibility.Hidden;
            GameSuccessMenu.Visibility = Visibility.Hidden;
            await GameLoop();
        }


        // Kliknutí na tlačíkto Pause
        private void PlayPause_Click(object sender, RoutedEventArgs e)
        {    
            pause = false;
            GamePauseMenu.Visibility= Visibility.Hidden;
            musicPlayer.SoundLocation = "mainsong.wav";
            musicPlayer.PlayLooping();
        }


        // Kliknutí na tlačíkto Restart
        private void PlayPauseRestart_Click(object sender, RoutedEventArgs e)
        {
            pause = false;
            music = true;
            PlayMusic();
            GamePauseMenu.Visibility = Visibility.Hidden;
            gameState = new GameState();
            GameOverMenu.Visibility = Visibility.Hidden;
        }


        // Kliknutí na tlačíkto Pomoc
        private void PlayPauseHelp_Click(object sender, RoutedEventArgs e)
        {
            GamePauseMenu.Visibility = Visibility.Hidden;
            GameHelpMenu.Visibility= Visibility.Visible;
        }

        // Kliknutí na tlačíkto Zpět v nabídce Pomoc
        private void PlayHelpBack_Click(object sender, RoutedEventArgs e)
        {
            GamePauseMenu.Visibility = Visibility.Visible;
            GameHelpMenu.Visibility = Visibility.Hidden;
        }


        // Akce po konci hry
        public void PlayGameSuccess()
        {
            GameSuccessMenu.Visibility = Visibility.Visible;
            PlaySuccessSound();
        }


        // Zvuk na konci hry
        public static void PlaySuccessSound()
        {
            SoundPlayer succedSound = new SoundPlayer();
            succedSound.SoundLocation = "success.wav";
            succedSound.Play();
        }
    }
}
