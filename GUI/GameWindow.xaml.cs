using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Effects;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.ComponentModel;
using Locomotion.Networking;
using System.Runtime.InteropServices;
using Neon.Core;

namespace Locomotion
{
    /// <summary>
    /// Interaction logic for GameWindow.xaml
    /// </summary>
    public partial class GameWindow : Window
    {
        bool pieceSelected = false;
        bool justClickedAPiece = false;
        bool doRipple = true;
        bool suggestCancelled = false;
        bool gameWindowClosing = false;
        bool winnerExists = false;
        bool playerIsWinner = false;

        Window prevWindow;
        Timer timer = new Timer();
        Timer placeLiftedPeg_Timer = new Timer();
        Timer thinkingBubble_timer = new Timer();
        Timer stopWatch_Timer = new Timer();
        Timer decrementStopWatch_Timer = new Timer();
        Timer RobotSprite_Timer = new Timer();
        Timer ManSprite_Timer = new Timer();
        Timer Sprite_Timer = new Timer();
        Timer WhaleAnim_Timer = new Timer();
        Timer nextDialogue_Timer = new Timer();

        Timer layTrainTrack_Timer = new Timer();
        int layTrainTrack_Inc = 0;
        List<TrainTree> winningPath;

        Queue<Sprite> rippleQ = new Queue<Sprite>();
        int thinkingBubble_inc = 0;
        NetworkManager networkManager = NetworkManager.InstanceCreator();
        GameOptions gameOptions;
        List<Pair> availableMoves;
        List<Sprite> robotList = new List<Sprite>();
        List<Sprite> manList = new List<Sprite>();
        Game game;
        Pair from;
        Pair networkFrom;
        Pair placedPeg;
        Random random = new Random();
        private BackgroundWorker worker = new BackgroundWorker();
        private BackgroundWorker suggestMoveWorker = new BackgroundWorker();
        private PieceMove suggestedMove = new PieceMove();
        Sprite sbTarget = new Sprite();
        Sprite stopWatchSprite;
        Sprite WHALE = new Sprite("whale", 139, 83, 32, 60);
        Sprite storedSprite = new Sprite();
        Sprite turnIndicator = new Sprite("glowSprite", 298, 298, 18, 80);
        Point moveDirection;
        SolidColorBrush whiteColor = new SolidColorBrush();
        SolidColorBrush blackColor= new SolidColorBrush();
        private int aiDepth;
        SoundManager soundmanager = SoundManager.InstanceCreator();
        Random rand = new Random();
        bool sbInProgress = false;
        System.Windows.Controls.ToolTip universalToolTip = new System.Windows.Controls.ToolTip();

        string humanSpriteSheetSource;
        string humanCornerCircleSource;
        string robotSpriteSheetSource;
        int humanSpriteSheetLength;
        int robotSpriteSheetLength;
        int progress = 0;

        public GameWindow (Window w, GameOptions gameOptions)
        {
            InitializeComponent();
            networkManager.allowKillNetworkThread = true;

            prevWindow = w;
            this.gameOptions = gameOptions;

            contentGrid.LayoutTransform = new ScaleTransform(System.Windows.SystemParameters.PrimaryScreenWidth / 1440,
                System.Windows.SystemParameters.PrimaryScreenHeight / 900,
                System.Windows.SystemParameters.PrimaryScreenWidth / 2,
                System.Windows.SystemParameters.PrimaryScreenHeight / 2);

            // after picking a board
            //gameOptions.prevBoard = 1;
            setCharacterSources();
            cornerCharacters();
            // Starting game pegs
            addInitialPegs();

            turnIndicator.Visibility = System.Windows.Visibility.Hidden;

            WHALE.Margin = new Thickness(300, 0, 0, 850);
            WHALE.Height = 100;
            WHALE.Width = 65;
            
           // UIElement temp = contentGrid.Children[contentGrid.Children.IndexOf(PauseMenuGrid)];
           // int index = contentGrid.Children.IndexOf(PauseMenuGrid);
          //  contentGrid.Children.RemoveAt(index);
          //  contentGrid.Children.Add(WHALE);
          //  contentGrid.Children.Add(temp);

            InitializeGameOptions(gameOptions);
            InitializeTimers();

            if (game.currentPlayer.playerType == Player.TypeOfPlayer.AI
                || game.currentPlayer.playerType == Player.TypeOfPlayer.Network)
            {
                // Starts game or starts listening
                GameBoard.IsEnabled = false;
                worker.RunWorkerAsync();
            }


            if (SoundManager.isMuted)
            {
                GamePlayTheme.IsMuted = true;
                GamePlayTheme.Volume = 0;
                soundmanager.Mute();

                MusicButton.Source = new BitmapImage(new Uri("/Locomotion;component/Media/Graphics/musicOff.png", UriKind.Relative));
            }

            else
            {
                GamePlayTheme.IsMuted = false;
                GamePlayTheme.Volume = 0.5;
                soundmanager.Unmute();
            }

            fillHelpBox();
            //GamePlayTheme.Play();

            if (gameOptions.typeOfGame == Game.TypeOfGame.Campaign)
                startDialogue();
        }

        #region INITIALIZATION

        private void PickARandomBoard()
        {
            Random rand = new Random();
            BitmapImage boardImg = new BitmapImage();
            BitmapImage backgroundImg = new BitmapImage();
            BitmapImage waterTexture = new BitmapImage();
            //BitmapImage watertexture = new BitmapImage(new Uri("/Locomotion;component/Media/Graphics/waterTexture.png", UriKind.Relative));
            /*int temp;

            if (gameOptions.firstRun)
            {
                temp = rand.Next(1, 5);
                gameOptions.prevBoard = temp;
                gameOptions.firstRun = false;
            }
            else
                temp = gameOptions.prevBoard;*/

            if (gameOptions.level == 1)
            {
                boardImg = new BitmapImage(new Uri("/Locomotion;component/Media/Graphics/1stLevelBoard.png", UriKind.Relative));
                backgroundImg = new BitmapImage(new Uri("/Locomotion;component/Media/Graphics/CompleteBoard.png", UriKind.Relative));
                waterTexture = new BitmapImage(new Uri("/Locomotion;component/Media/Graphics/subtleWaterTexture.png", UriKind.Relative));
            }
            else if (gameOptions.level == 2)
            {
                boardImg = new BitmapImage(new Uri("/Locomotion;component/Media/Graphics/2ndLevelBoard.png", UriKind.Relative));
                backgroundImg = new BitmapImage(new Uri("/Locomotion;component/Media/Graphics/2ndLevelBackground.png", UriKind.Relative));
                waterTexture = new BitmapImage(new Uri("/Locomotion;component/Media/Graphics/waterTexture.png", UriKind.Relative));
                WHALE.Visibility = Visibility.Hidden;
                SnowParticleSystem.Start();
                GameSoundtrack.Source = new Uri("Media/Sounds/SnowIsland-Soundtrack.wav", UriKind.Relative);
                GamePlayTheme.LoadedBehavior = MediaState.Manual;

            }
            else if (gameOptions.level == 3)
            {
                boardImg = new BitmapImage(new Uri("/Locomotion;component/Media/Graphics/3rdLevelBoard.png", UriKind.Relative));
                backgroundImg = new BitmapImage(new Uri("/Locomotion;component/Media/Graphics/3rdLevelBackground.png", UriKind.Relative));
                waterTexture = new BitmapImage(new Uri("/Locomotion;component/Media/Graphics/subtleWaterTexture.png", UriKind.Relative));
                WHALE.Visibility = Visibility.Hidden;
                SandParticleSystem.Start();
                GameSoundtrack.Source = new Uri("Media/Sounds/DesertIsland-Soundtrack.wav", UriKind.Relative);
                GamePlayTheme.LoadedBehavior = MediaState.Manual;
            }
            else if (gameOptions.level == 4)
            {
                boardImg = new BitmapImage(new Uri("/Locomotion;component/Media/Graphics/4thLevelBoard.png", UriKind.Relative));
                backgroundImg = new BitmapImage(new Uri("/Locomotion;component/Media/Graphics/4thLevelBackgroundDark.png", UriKind.Relative));
                waterTexture = new BitmapImage(new Uri("/Locomotion;component/Media/Graphics/subtleWaterTexture.png", UriKind.Relative));
                WHALE.Visibility = Visibility.Hidden;
                //AshParticleSystem.Start();
                VolcanoParticleSystem.Start();
                GameSoundtrack.Source = new Uri("Media/Sounds/BossTheme.wav", UriKind.Relative);
                GamePlayTheme.LoadedBehavior = MediaState.Manual;
            }

            WaterTexture.Source = waterTexture;
            BoardBackground.Source = backgroundImg;
            BoardImage.Source = boardImg;

        }

        private void InitializeGameOptions(GameOptions gameOptions)
        {
            if (gameOptions.typeOfGame == Game.TypeOfGame.AI)
            {
                ChatBoardImage.Visibility = System.Windows.Visibility.Hidden;
                chatBox.Visibility = System.Windows.Visibility.Hidden;
                chatBox.IsEnabled = false;
                aiDepth = 4;

                PickARandomBoard();

                game = new Game(Game.TypeOfGame.AI);
            }
            else if (gameOptions.typeOfGame == Game.TypeOfGame.QuickMatch)
            {
                if (gameOptions.difficulty == TreeNode.Difficulty.Easy)
                {
                    SuggestMove.IsEnabled = true;
                    SuggestMove.Visibility = Visibility.Visible;

                    aiDepth = 1;
                }
                else if (gameOptions.difficulty == TreeNode.Difficulty.Medium)
                {
                    aiDepth = 2;
                }
                else
                {
                    aiDepth = 4;
                }

                ChatBoardImage.Visibility = System.Windows.Visibility.Hidden;
                chatBox.Visibility = System.Windows.Visibility.Hidden;
                chatBox.IsEnabled = false;
                game = new Game(new Player(ProfileManager.currentProfile.ProfileName, Player.Color.White, Player.TypeOfPlayer.Human), gameOptions);


                PickARandomBoard();
                LeftCornerName.Content = ProfileManager.currentProfile.ProfileName;
                RightCornerName.Content = gameOptions.player2Name;

                //LeftCorner.Source = new BitmapImage(new Uri("/Locomotion;component/Media/Graphics/char1circle.png", UriKind.Relative));
                //int robotnum = random.Next(1, 5);
                //RightCorner.Source = new BitmapImage(new Uri("/Locomotion;component/Media/Graphics/robot" + robotnum.ToString() + "corner.png", UriKind.Relative));
            }
            else if (gameOptions.typeOfGame == Game.TypeOfGame.Local)
            {
                ChatBoardImage.Visibility = System.Windows.Visibility.Hidden;
                chatBox.Visibility = System.Windows.Visibility.Hidden;
                chatBox.IsEnabled = false;

                LeftCornerName.Content = gameOptions.player1Name;
                RightCornerName.Content = gameOptions.player2Name;

                PickARandomBoard();

                //LeftCorner.Source = new BitmapImage(new Uri("/Locomotion;component/Media/Graphics/char1circle.png", UriKind.Relative));
                //RightCorner.Source = new BitmapImage(new Uri("/Locomotion;component/Media/Graphics/robot2corner.png", UriKind.Relative));

                game = new Game(gameOptions.player1Name, gameOptions.player2Name); // H v H
            }
            else if (gameOptions.typeOfGame == Game.TypeOfGame.Network)
            {
                // Register networking events
                networkManager = NetworkManager.InstanceCreator();
                networkManager.getLocoPeer().NetWorker.moveReceived += new MoveEventHandler(this.NetworkMoveReceived);
                networkManager.getLocoPeer().NetWorker.messageReceived += new MessageEventHandler(NetWorker_messageReceived);
                networkManager.getLocoPeer().NetWorker.connectionStatusReceived += new ConnectionEventHandler(this.NetworkStatusReceived);

                ChatBoardImage.Visibility = System.Windows.Visibility.Visible;
                chatBox.Visibility = System.Windows.Visibility.Visible;
                chatBox.IsEnabled = true;
                chatInput.Visibility = System.Windows.Visibility.Visible;
                chatInput.IsEnabled = true;

                PickARandomBoard();

                if (gameOptions.player1) // I am player 1 (human)
                {
                    //ChatBoardImage.Source = new BitmapImage(new Uri("/Locomotion;component/Media/Graphics/robot1chatbox.png", UriKind.Relative));
                    //LeftCorner.Source = new BitmapImage(new Uri("/Locomotion;component/Media/Graphics/char1circle.png", UriKind.Relative));
                    game = new Game(ProfileManager.currentProfile.ProfileName, networkManager.getLocoPeer().name, 1);

                    RightCornerName.Content = networkManager.challengername;
                    LeftCornerName.Content = networkManager.getLocoPeer().name;
                }
                else // I am player 2 (robot)
                {
                    //ChatBoardImage.Source = new BitmapImage(new Uri("/Locomotion;component/Media/Graphics/humanchatbox.png", UriKind.Relative));
                    //LeftCorner.Source = new BitmapImage(new Uri("/Locomotion;component/Media/Graphics/robot2corner.png", UriKind.Relative));
                    game = new Game(networkManager.challengername, ProfileManager.currentProfile.ProfileName, 2);

                    LeftCornerName.Content = networkManager.getLocoPeer().name;
                    RightCornerName.Content = networkManager.challengername;
                }

                if (!networkManager.checkForConnection())
                {
                    ConnectionEventArgs status = new ConnectionEventArgs();
                    status.connected = false;
                    status.message = "Woops, sorry something \n went wrong while loading";
                    Object obj = new Object();
                    NetworkStatusReceived(obj, status);
                    networkManager.peerDisconnect("Oh bummer! Your opponent resigned.");
                }

                RightCornerName.Margin = new Thickness(0, 0, 250, 0);
            }
            else if (gameOptions.typeOfGame == Game.TypeOfGame.TimeAttack)
            {
                if (gameOptions.difficulty == TreeNode.Difficulty.Easy)
                {
                    aiDepth = 1;
                }
                else if (gameOptions.difficulty == TreeNode.Difficulty.Medium)
                {
                    aiDepth = 3;
                }
                else
                {
                    aiDepth = 4;
                }

                stopWatchCanvas.Visibility = System.Windows.Visibility.Visible;

                //if (gameOptions.boyCharacter)
                    //LeftCorner.Source = new BitmapImage(new Uri("/Locomotion;component/Media/Graphics/char1circle.png", UriKind.Relative));
                //else
                    //LeftCorner.Source = new BitmapImage(new Uri("/Locomotion;component/Media/Graphics/girlcorner.png", UriKind.Relative));
                //int robotnum = random.Next(1, 5);
                //RightCorner.Source = new BitmapImage(new Uri("/Locomotion;component/Media/Graphics/robot" + robotnum.ToString() + "corner.png", UriKind.Relative));

                stopWatchSprite = new Sprite("clocksprite", 696, 577, 10, 100);
                stopWatchSprite.Height = 140;
                stopWatchSprite.Width = 210;
                stopWatchSprite.VerticalAlignment = System.Windows.VerticalAlignment.Top;
                stopWatchSprite.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;

                stopWatchCanvas.Children.Add(stopWatchSprite);
                stopWatchSprite.StopAnimation();

                ChatBoardImage.Visibility = System.Windows.Visibility.Hidden;
                chatBox.Visibility = System.Windows.Visibility.Hidden;
                chatBox.IsEnabled = false;

                LeftCornerName.Content = ProfileManager.currentProfile.ProfileName;
                RightCornerName.Content = gameOptions.player2Name;

                PickARandomBoard();

                decrementStopWatch_Timer.Tick += new EventHandler(decrementStopWatch_Timer_Tick);
                decrementStopWatch_Timer.Interval = 1000;

                TimeSpan startingTime = TimeSpan.FromMinutes((double)gameOptions.stopWatchTime);
                stopWatch.Content = String.Format("{0:d2}:{1:d2}", startingTime.Minutes, startingTime.Seconds);

                game = new Game(new Player(ProfileManager.currentProfile.ProfileName, Player.Color.White, Player.TypeOfPlayer.Human), gameOptions);
            }
            else // Game.TypeOfGame.Campaign
            {

                chatBox.Visibility = System.Windows.Visibility.Visible;
                chatBox.IsEnabled = true;

                LeftCornerName.Content = ProfileManager.currentProfile.ProfileName;
                RightCornerName.Content = gameOptions.player2Name;

                //if (gameOptions.boyCharacter)
                    //LeftCorner.Source = new BitmapImage(new Uri("/Locomotion;component/Media/Graphics/char1circle.png", UriKind.Relative));
                //else
                    //LeftCorner.Source = new BitmapImage(new Uri("/Locomotion;component/Media/Graphics/girlcorner.png", UriKind.Relative));

                gameOptions.player1 = true;

                switch (gameOptions.campaignLevel)
                {
                    case 1:
                        // fill in whichever robot
                        BoardImage.Source = new BitmapImage(new Uri("/Locomotion;component/Media/Graphics/1stLevelBoard.png", UriKind.Relative));
                        BoardBackground.Source = new BitmapImage(new Uri("/Locomotion;component/Media/Graphics/CompleteBoard.png", UriKind.Relative));
                        WaterTexture.Source = new BitmapImage(new Uri("/Locomotion;component/Media/Graphics/subtleWaterTexture.png", UriKind.Relative));
                        RightCorner.Source = new BitmapImage(new Uri("/Locomotion;component/Media/Graphics/robot1corner.png", UriKind.Relative));
                        // AshParticleSystem.Start();
                        Storyboard level1sb = this.Resources["Level1Storyboard"] as Storyboard;
                        // level1sb.Begin();
                        aiDepth = 1;
                        break;
                    case 2:
                        BoardImage.Source = new BitmapImage(new Uri("/Locomotion;component/Media/Graphics/2ndLevelBoard.png", UriKind.Relative));
                        BoardBackground.Source = new BitmapImage(new Uri("/Locomotion;component/Media/Graphics/2ndLevelBackground.png", UriKind.Relative));
                        WaterTexture.Source = new BitmapImage(new Uri("/Locomotion;component/Media/Graphics/waterTexture.png", UriKind.Relative));
                        RightCorner.Source = new BitmapImage(new Uri("/Locomotion;component/Media/Graphics/robot2corner.png", UriKind.Relative));
                        SnowParticleSystem.Start();

                        GameSoundtrack.Source = new Uri("Media/Sounds/SnowIsland-Soundtrack.wav", UriKind.Relative);
                        GamePlayTheme.LoadedBehavior = MediaState.Manual;

                        WHALE.Visibility = Visibility.Hidden;
                        aiDepth = 1;
                        break;
                    case 3:
                        BoardImage.Source = new BitmapImage(new Uri("/Locomotion;component/Media/Graphics/3rdLevelBoard.png", UriKind.Relative));
                        BoardBackground.Source = new BitmapImage(new Uri("/Locomotion;component/Media/Graphics/3rdLevelBackground.png", UriKind.Relative));
                        WaterTexture.Source = new BitmapImage(new Uri("/Locomotion;component/Media/Graphics/subtleWaterTexture.png", UriKind.Relative));
                        RightCorner.Source = new BitmapImage(new Uri("/Locomotion;component/Media/Graphics/robot3corner.png", UriKind.Relative));
                        SandParticleSystem.Start();

                        GameSoundtrack.Source = new Uri("Media/Sounds/DesertIsland-Soundtrack.wav", UriKind.Relative);
                        GamePlayTheme.LoadedBehavior = MediaState.Manual;
                        WHALE.Visibility = Visibility.Hidden;
                        aiDepth = 3;
                        break;
                    case 4:
                        BoardImage.Source = new BitmapImage(new Uri("/Locomotion;component/Media/Graphics/4thLevelBoard.png", UriKind.Relative));
                        BoardBackground.Source = new BitmapImage(new Uri("/Locomotion;component/Media/Graphics/4thLevelBackgroundDark.png", UriKind.Relative));
                        WaterTexture.Source = new BitmapImage(new Uri("/Locomotion;component/Media/Graphics/subtleWaterTexture.png", UriKind.Relative));
                        RightCorner.Source = new BitmapImage(new Uri("/Locomotion;component/Media/Graphics/robot4corner.png", UriKind.Relative));
                        //AshParticleSystem.Start();
                        VolcanoParticleSystem.Start();

                        GameSoundtrack.Source = new Uri("Media/Sounds/BossTheme.wav", UriKind.Relative);
                        GamePlayTheme.LoadedBehavior = MediaState.Manual;
                        WHALE.Visibility = Visibility.Hidden;
                        aiDepth = 4;
                        break;
                }
                game = new Game(new Player(ProfileManager.currentProfile.ProfileName, Player.Color.White, Player.TypeOfPlayer.Human), gameOptions);
                gameOptions.firstRun = false;
                // campaignStuff(gameOptions.typeOfGame); Don't necessarily call this way, just for now
            }
            if (gameOptions.typeOfGame != Game.TypeOfGame.Network && !gameOptions.player1)
            {
                game.switchPlayer();

                turnIndicator.Height = 420;
                turnIndicator.Width = 298;

                turnIndicator.StartAnimation();

                TurnIndicatorCanvas.Children.Add(turnIndicator);
                Canvas.SetLeft(turnIndicator, 1215);
                Canvas.SetBottom(turnIndicator, -40);
                turnIndicator.Visibility = System.Windows.Visibility.Visible;
            }
            else if (gameOptions.typeOfGame == Game.TypeOfGame.Network && !gameOptions.player1)
            {
                turnIndicator.Height = 420;
                turnIndicator.Width = 298;

                turnIndicator.StartAnimation();

                TurnIndicatorCanvas.Children.Add(turnIndicator);
                Canvas.SetLeft(turnIndicator, 1215);
                Canvas.SetBottom(turnIndicator, -40);
                turnIndicator.Visibility = System.Windows.Visibility.Visible;        
            }
            else
            {
                turnIndicator.Height = 420;
                turnIndicator.Width = 298;

                turnIndicator.StartAnimation();

                TurnIndicatorCanvas.Children.Add(turnIndicator);
                Canvas.SetLeft(turnIndicator, 24);
                Canvas.SetBottom(turnIndicator, -40);
                turnIndicator.Visibility = System.Windows.Visibility.Visible;
            }
        }

        private void fillHelpBox()
        {
            helpBox.Text += "Locomotion Instructions\n\n";
            helpBox.Text += "Gameboard:\n-- -- -- -- -- -- -- --\n";
            helpBox.Text += "A 6x6 grid separates cities on opposite corners.\n";    
            helpBox.Text += "Tokens are placed in each grid square\n";
            helpBox.Text += "Engineers move along the grid lines.\n\n";        
            helpBox.Text += "Objective:\n-- -- -- -- -- -- -- --\nBe the first conductor to build a railroad across the island.";		
            helpBox.Text += "\nUse tokens to connect your cities.\n\n";
            helpBox.Text += "Engineers:\n-- -- -- -- -- -- -- --\n";
            helpBox.Text += "Each conductor starts with 8 engineers.\n";
            helpBox.Text += "Click on an engineer to show available moves.\n";
            helpBox.Text += "Types of movement:\n";
            helpBox.Text += "   1. Jump over grass to place a token or replace an enemy token.\n";
            helpBox.Text += "   2. Slide along lines.\n";
            helpBox.Text += "   3. Vault over the enemy to eliminate their piece\n\n";
            helpBox.Text += "End of game:\n-- -- -- -- -- -- -- --\nUse your engineers to form a chain of disks between your corner cities.\n";
            helpBox.Text += "Diagonals do not count.\n";
            helpBox.Text += "The first conductor to connect tokens across the island wins.\n";
            helpBox.Text += "Do not capture all your opponent's engineers or it will only be a tie.\n";
        }

        private void setCharacterSources()
        {
            int charNum;
            if (gameOptions.typeOfGame == Game.TypeOfGame.Network)
            {
                charNum = gameOptions.level;
            }
            else
            {
                charNum = ProfileManager.currentProfile.CharacterNumber;
            }

            switch (charNum)
            {
                case 1:
                    humanCornerCircleSource = "/Locomotion;component/Media/Graphics/char1circle.png";
                    humanSpriteSheetSource = "humanSprite";
                    humanSpriteSheetLength = 35;
                    break;
                case 2:
                    humanCornerCircleSource = "/Locomotion;component/Media/Graphics/char2circle.png";
                    humanSpriteSheetSource = "girlSprite";
                    humanSpriteSheetLength = 26;
                    break;
                case 3:
                    humanCornerCircleSource = "/Locomotion;component/Media/Graphics/char3circle.png";
                    humanSpriteSheetSource = "womanSprite";
                    humanSpriteSheetLength = 32;
                    break;
                case 4:
                    humanCornerCircleSource = "/Locomotion;component/Media/Graphics/char4circle.png";
                    humanSpriteSheetSource = "boySprite";
                    humanSpriteSheetLength = 22;
                    break;
                default:
                    humanCornerCircleSource = "/Locomotion;component/Media/Graphics/char4circle.png";
                    humanSpriteSheetSource = "humanSprite";
                    humanSpriteSheetLength = 35;
                    break;
            }

            switch (gameOptions.level)
            {
                case 1:
                    robotSpriteSheetSource = "robotSprite";
                    robotSpriteSheetLength = 40;
                    break;
                case 2:
                    robotSpriteSheetSource = "blueRobotSprite";
                    robotSpriteSheetLength = 28;
                    break;
                case 3:
                    robotSpriteSheetSource = "roboCactusSprite";
                    robotSpriteSheetLength = 26;
                    break;
                case 4:
                    robotSpriteSheetSource = "robotSprite";
                    robotSpriteSheetLength = 40;
                    break;
                default:
                    robotSpriteSheetSource = "robotSprite";
                    robotSpriteSheetLength = 40;
                    break;
            }
        }

        private void cornerCharacters()
        {
            if (gameOptions.typeOfGame == Game.TypeOfGame.Network)
            {
                if (gameOptions.player1) // I am player 1
                {
                    LeftCorner.Source = new BitmapImage(new Uri(humanCornerCircleSource, UriKind.Relative));
                    ChatBoardImage.Visibility = System.Windows.Visibility.Visible;
                    switch (gameOptions.level)
                    {
                        case 1:
                            ChatBoardImage.Source = new BitmapImage(new Uri("/Locomotion;component/Media/Graphics/robot1ChatBox.png", UriKind.Relative));
                            RightCorner.Visibility = Visibility.Hidden;//.Source = new BitmapImage(new Uri("/Locomotion;component/Media/Graphics/robot1corner.png", UriKind.Relative));
                            break;
                        case 2:
                            ChatBoardImage.Source = new BitmapImage(new Uri("/Locomotion;component/Media/Graphics/robot2ChatBox.png", UriKind.Relative));
                            RightCorner.Visibility = Visibility.Hidden;//.Source = new BitmapImage(new Uri("/Locomotion;component/Media/Graphics/robot2corner.png", UriKind.Relative));
                            break;
                        case 3:
                            ChatBoardImage.Source = new BitmapImage(new Uri("/Locomotion;component/Media/Graphics/roboCactusChatBox.png", UriKind.Relative));
                            RightCorner.Visibility = Visibility.Hidden;//.Source = new BitmapImage(new Uri("/Locomotion;component/Media/Graphics/roboCactusCorner.png", UriKind.Relative));
                            break;
                        case 4:
                            ChatBoardImage.Source = new BitmapImage(new Uri("/Locomotion;component/Media/Graphics/robot2ChatBox.png", UriKind.Relative));
                            RightCorner.Visibility = Visibility.Hidden;//.Source = new BitmapImage(new Uri("/Locomotion;component/Media/Graphics/robot4corner.png", UriKind.Relative));
                            break;
                    }

                }
                else // I am player 2
                {
                    //RightCorner.Source = new BitmapImage(new Uri(humanCornerCircleSource, UriKind.Relative));
                    switch (gameOptions.level)
                    {
                        case 1:
                            LeftCorner.Source = new BitmapImage(new Uri("/Locomotion;component/Media/Graphics/robot1corner.png", UriKind.Relative));
                            ChatBoardImage.Source = new BitmapImage(new Uri("/Locomotion;component/Media/Graphics/humanchatbox.png", UriKind.Relative));
                            break;
                        case 2:
                            ChatBoardImage.Source = new BitmapImage(new Uri("/Locomotion;component/Media/Graphics/girlChatBox.png", UriKind.Relative));
                            LeftCorner.Source = new BitmapImage(new Uri("/Locomotion;component/Media/Graphics/robot2corner.png", UriKind.Relative));
                            break;
                        case 3:
                            ChatBoardImage.Source = new BitmapImage(new Uri("/Locomotion;component/Media/Graphics/womanChatBox.png", UriKind.Relative));
                            LeftCorner.Source = new BitmapImage(new Uri("/Locomotion;component/Media/Graphics/roboCactusCircle.png", UriKind.Relative));
                            break;
                        case 4:
                            ChatBoardImage.Source = new BitmapImage(new Uri("/Locomotion;component/Media/Graphics/boyChatBox.png", UriKind.Relative));
                            LeftCorner.Source = new BitmapImage(new Uri("/Locomotion;component/Media/Graphics/robot4corner.png", UriKind.Relative));
                            break;
                    }
                }
            }
            else if (gameOptions.typeOfGame == Game.TypeOfGame.Campaign)
            {
                LeftCorner.Source = new BitmapImage(new Uri(humanCornerCircleSource, UriKind.Relative));


                // pending level
                switch (gameOptions.campaignLevel)
                {
                    case 1:
                        RightCorner.Source = new BitmapImage(new Uri("/Locomotion;component/Media/Graphics/robot1corner.png", UriKind.Relative));
                        break;
                    case 2:
                        RightCorner.Source = new BitmapImage(new Uri("/Locomotion;component/Media/Graphics/robot2corner.png", UriKind.Relative));
                        break;
                    case 3:
                        RightCorner.Source = new BitmapImage(new Uri("/Locomotion;component/Media/Graphics/roboCactusCircle.png", UriKind.Relative));
                        break;
                    case 4:
                        RightCorner.Source = new BitmapImage(new Uri("/Locomotion;component/Media/Graphics/robot4corner.png", UriKind.Relative));
                        break;
                }
            }
            else if (gameOptions.typeOfGame == Game.TypeOfGame.Local)
            {
                LeftCorner.Source = new BitmapImage(new Uri(humanCornerCircleSource, UriKind.Relative));

                switch (gameOptions.level)
                {
                    case 1:
                        RightCorner.Source = new BitmapImage(new Uri("/Locomotion;component/Media/Graphics/robot1corner.png", UriKind.Relative));
                        break;
                    case 2:
                        RightCorner.Source = new BitmapImage(new Uri("/Locomotion;component/Media/Graphics/robot2corner.png", UriKind.Relative));
                        break;
                    case 3:
                        RightCorner.Source = new BitmapImage(new Uri("/Locomotion;component/Media/Graphics/roboCactusCircle.png", UriKind.Relative));
                        break;
                    case 4:
                        RightCorner.Source = new BitmapImage(new Uri("/Locomotion;component/Media/Graphics/robot4corner.png", UriKind.Relative));
                        break;
                }
                // pick random
            }
            else if (gameOptions.typeOfGame == Game.TypeOfGame.AI)
            {
                LeftCorner.Source = new BitmapImage(new Uri(humanCornerCircleSource, UriKind.Relative));
             
                // ??????????
            }
            else // quick match OR time attack
            {
                LeftCorner.Source = new BitmapImage(new Uri(humanCornerCircleSource, UriKind.Relative));

                switch (gameOptions.level)
                {
                    case 1:
                        RightCorner.Source = new BitmapImage(new Uri("/Locomotion;component/Media/Graphics/robot1corner.png", UriKind.Relative));
                        break;
                    case 2:
                        RightCorner.Source = new BitmapImage(new Uri("/Locomotion;component/Media/Graphics/robot2corner.png", UriKind.Relative));
                        break;
                    case 3:
                        RightCorner.Source = new BitmapImage(new Uri("/Locomotion;component/Media/Graphics/roboCactusCircle.png", UriKind.Relative));
                        break;
                    case 4:
                        RightCorner.Source = new BitmapImage(new Uri("/Locomotion;component/Media/Graphics/robot4corner.png", UriKind.Relative));
                        break;
                }
            }
        }

        private void InitializeTimers()
        {
            timer.Tick += new EventHandler(timer_Tick);
            timer.Interval = 1000; 
            timer.Start(); // to close previous window

            ManSprite_Timer.Tick += new EventHandler(Mantimer_Tick);
            ManSprite_Timer.Interval = rand.Next(5000, 10000);
            ManSprite_Timer.Start();

            RobotSprite_Timer.Tick += new EventHandler(Robottimer_Tick);
            RobotSprite_Timer.Interval = rand.Next(5000, 10000);
            RobotSprite_Timer.Start();

            placeLiftedPeg_Timer.Tick += new EventHandler(placeLiftedPeg_Timer_Tick);
            placeLiftedPeg_Timer.Interval = 1000;

            thinkingBubble_timer.Tick += new EventHandler(thinkingBubble_timer_Tick);
            thinkingBubble_timer.Interval = 770;

            worker.DoWork += worker_DoWork;
            worker.RunWorkerCompleted += worker_RunWorkerCompleted;
            worker.WorkerSupportsCancellation = true;

            layTrainTrack_Timer.Tick += new EventHandler(layTrainTrack_Timer_Tick);
            layTrainTrack_Timer.Interval = 1200;

            WhaleAnim_Timer.Tick += new EventHandler(WhaleAnim_Timer_Tick);
            WhaleAnim_Timer.Interval = 10000;
            WhaleAnim_Timer.Start();

            suggestMoveWorker.DoWork += new DoWorkEventHandler(suggestMoveWorker_DoWork);
            suggestMoveWorker.RunWorkerCompleted +=new RunWorkerCompletedEventHandler(suggestMoveWorker_RunWorkerCompleted);
            suggestMoveWorker.WorkerSupportsCancellation = true;

            nextDialogue_Timer.Interval = 800;
            nextDialogue_Timer.Tick += new EventHandler(nextDialogue_Timer_Tick);
        }

        void WhaleAnim_Timer_Tick(object sender, EventArgs e)
        {
            TransformGroup test = new TransformGroup();
            WHALE.RenderTransform = new ScaleTransform(1, -1);
            TranslateTransform moveWHALE = new TranslateTransform();
            WHALE.RenderTransform = moveWHALE;
            DoubleAnimation upWHALE = new DoubleAnimation(200, TimeSpan.FromSeconds(10));
            DoubleAnimation rightWHALE = new DoubleAnimation(500, TimeSpan.FromSeconds(10));
            moveWHALE.BeginAnimation(TranslateTransform.XProperty, rightWHALE);
            moveWHALE.BeginAnimation(TranslateTransform.YProperty, upWHALE);

            Storyboard tester = new Storyboard();
            //Storyboard.SetTarget(Sprite, WHALE);
        }

        void layTrainTrack_Timer_Tick(object sender, EventArgs e)
        {
            layTrainTrack_Timer.Stop();

            TrainTree t = winningPath.ElementAt(layTrainTrack_Inc);
            string diskName = "DS" + t.Row + t.Col;
            foreach (UIElement uie in GameBoard.Children)
            {
                if (uie.Uid == diskName)
                {
                    uie.Visibility = System.Windows.Visibility.Hidden;
                    ((Image)uie).Source = new BitmapImage(new Uri(TrainTree.getTrackSource(t.DirectionalIndex), UriKind.Relative));
                    ((Image)uie).Height = 110;
                    ((Image)uie).Width = 186;
                    ((Image)uie).Margin = new Thickness(getPegLeft(t.Row, t.Col) - 6 + (t.Col - t.Row) - 45, getPegTop(t.Row, t.Col) + 109 + (t.Row + t.Col) - 17, 0, 0);
                    uie.Visibility = System.Windows.Visibility.Visible;
                    break;
                }
            }

            if (layTrainTrack_Inc == 0)
            {
                layTrainTrack_Timer.Interval = 230;
            }

            layTrainTrack_Inc++;
            layTrainTrack_Timer.Start();

            if (layTrainTrack_Inc == winningPath.Count)
            {
                layTrainTrack_Timer.Stop();
                Storyboard gameOverSB = FindResource("gameOverStoryboard") as Storyboard;
                gameOverSB.Begin();

                if (playerIsWinner)
                {
                    victoryAnimation();
                    soundmanager.playTrainHornSound();
                }
            }
        }

        private void victoryAnimation()
        {
            TranslateTransform moveMiniTrain = new TranslateTransform();
            miniTrain.RenderTransform = moveMiniTrain;
            DoubleAnimation rightTrain = new DoubleAnimation(4100, TimeSpan.FromSeconds(6.5));
            moveMiniTrain.BeginAnimation(TranslateTransform.XProperty, rightTrain);

            miniTrain.Margin = new Thickness(-2300, 600, 0, 0);
            System.Windows.Controls.Panel.SetZIndex(miniTrain, 99);
            TranslateTransform moveMiniTrain1 = new TranslateTransform();
            miniTrain.RenderTransform = moveMiniTrain1;
            //DoubleAnimation upTrain1 = new DoubleAnimation(-950, TimeSpan.FromSeconds(5));
            //upTrain1.AccelerationRatio = .2;
            DoubleAnimation rightTrain1 = new DoubleAnimation(4100, TimeSpan.FromSeconds(5));
            moveMiniTrain1.BeginAnimation(TranslateTransform.XProperty, rightTrain1);
            //moveMiniTrain1.BeginAnimation(TranslateTransform.YProperty, upTrain1);
            miniTrain.Visibility = System.Windows.Visibility.Visible;
        }

        void decrementStopWatch_Timer_Tick(object sender, EventArgs e)
        {
            TimeSpan oldTime;

            TimeSpan.TryParseExact(stopWatch.Content.ToString(), @"mm\:ss", null, out oldTime);

            if (oldTime.TotalSeconds <= 11 && oldTime.TotalSeconds != 0)
            {
                soundmanager.playNoTime();
            }
            else if(oldTime.TotalSeconds > 11)
            {
                if (oldTime.TotalSeconds % 2 == 1)
                {
                    soundmanager.playTick();
                }
                else
                {
                    soundmanager.playTock();
                }
            }

            if (oldTime.TotalSeconds == 1)
            {
                soundmanager.playTimeOut();

                TimeSpan oneSecond = TimeSpan.FromSeconds(1);
                oldTime = oldTime.Subtract(oneSecond);

                stopWatch.Content = String.Format("{0:d2}:{1:d2}", oldTime.Minutes, oldTime.Seconds);
            }
            else if(oldTime.TotalSeconds == 0)
            {
                GameBoard.IsEnabled = false;
                gameOverTextBlock.Text = "Oh no, you ran out of time! Keep practicing to improve your times!";
                gameOverMessageBox.Visibility = System.Windows.Visibility.Visible;

                Storyboard gameOver = FindResource("gameOverStoryboard") as Storyboard;
                gameOver.Begin();

                decrementStopWatch_Timer.Stop();
                stopWatchSprite.StopAnimation();
            }
            else
            {
                TimeSpan oneSecond = TimeSpan.FromSeconds(1);

                oldTime = oldTime.Subtract(oneSecond);

                stopWatch.Content = String.Format("{0:d2}:{1:d2}", oldTime.Minutes, oldTime.Seconds);
            }
        }

        void thinkingBubble_timer_Tick(object sender, EventArgs e)
        {
            if (thinkingBubble_inc % 3 == 0)
                ThinkingBubble.Source = new BitmapImage(new Uri("/Locomotion;component/Media/Graphics/think1.png", UriKind.Relative));
            else if (thinkingBubble_inc % 3 == 1)
                ThinkingBubble.Source = new BitmapImage(new Uri("/Locomotion;component/Media/Graphics/think2.png", UriKind.Relative));
            else
                ThinkingBubble.Source = new BitmapImage(new Uri("/Locomotion;component/Media/Graphics/think3.png", UriKind.Relative));
            thinkingBubble_inc++;
        }

        private void addInitialPegs()
        {
            manList.Add(addPeg(0, 1, Player.Color.White));

            manList.Add(addPeg(0, 2, Player.Color.White));
            manList.Add(addPeg(1, 0, Player.Color.White));
            manList.Add(addPeg(2, 0, Player.Color.White));
            manList.Add(addPeg(6, 5, Player.Color.White));
            manList.Add(addPeg(6, 4, Player.Color.White));
            manList.Add(addPeg(5, 6, Player.Color.White));
            manList.Add(addPeg(4, 6, Player.Color.White));

            robotList.Add(addPeg(0, 5, Player.Color.Black));
            robotList.Add(addPeg(0, 4, Player.Color.Black));
            robotList.Add(addPeg(1, 6, Player.Color.Black));
            robotList.Add(addPeg(2, 6, Player.Color.Black));
            robotList.Add(addPeg(4, 0, Player.Color.Black));
            robotList.Add(addPeg(5, 0, Player.Color.Black));
            robotList.Add(addPeg(6, 1, Player.Color.Black));
            robotList.Add(addPeg(6, 2, Player.Color.Black));
        }

        private void timer_Tick(object sender, EventArgs e)
        {
            timer.Stop();
            prevWindow.Close();
        }

        private void Robottimer_Tick(object sender, EventArgs e)
        {
            robotList[rand.Next(0, robotList.Count - 1)].AnimateOnce();

            RobotSprite_Timer.Interval = rand.Next(10000, 15000);
        }

        private void Mantimer_Tick(object sender, EventArgs e)
        {
            manList[rand.Next(0, manList.Count - 1)].AnimateOnce();

           ManSprite_Timer.Interval = rand.Next(10000, 15000);
        }

        #endregion

        #region MOVE LOGIC

        /// <summary> gamewindowMove
        /// Moves peg on the board
        /// Logic for jumps and disks
        /// No logic for valid move
        /// </summary>
        private void gamewindowMove(Pair to, Pair from)
        {
            if (game.currentPlayer.playerType == Player.TypeOfPlayer.AI)
            {
                ThinkingBubble.Source = new BitmapImage(new Uri("/Locomotion;component/Media/Graphics/thinksolve.png", UriKind.Relative));
            }

            System.Windows.Controls.Panel.SetZIndex(fetchPegImage(from), 2*(to.row + to.col));
            animatePeg(to, from);
            placedPeg = to;

            if (isDiskJumpDistance(to, from) && !isCorner(to, from))
            {
                Color c;
                SolidColorBrush mybrush;
                byte alpha = (byte)(150 - 10 * game.diskBoard.movesFromWin(game.currentPlayer.color));

                if (game.currentPlayer.color == Player.Color.White)
                {
                    //#620400FF
                    c = Color.FromArgb(alpha,4,0,255);
                    
                    mybrush = new SolidColorBrush(c);

                    //sets the color of all captured disk spaces to current "color from win"
                    for (int i = 0; i < DiskBoard.Children.Count; i++)
                    {
                        SolidColorBrush rectangleColor = (SolidColorBrush)DiskBoard.Children.Cast<Rectangle>().ElementAt(i).Fill;

                        if (rectangleColor != null && rectangleColor.Color == whiteColor.Color)
                            DiskBoard.Children.Cast<Rectangle>().ElementAt(i).Fill = mybrush;
                    }

                    //store the curent "color from win" color
                    whiteColor.Color = c;

                    // Remove old disk (if any)
                    if (DiskBoard.Children.Cast<Rectangle>().First(
                        f => Grid.GetRow(f) == ((to.row + from.row) / 2) && Grid.GetColumn(f) == ((to.col + from.col) / 2)).Fill != mybrush)
                    {
                        removeDisk((to.row + from.row) / 2, (to.col + from.col) / 2);

                        addDisk((to.row + from.row) / 2, (to.col + from.col) / 2);
                    }

                    // Add disk
                    DiskBoard.Children.Cast<Rectangle>().First(
                        f => Grid.GetRow(f) == ((to.row + from.row) / 2) && Grid.GetColumn(f) == ((to.col + from.col) / 2)).Fill = mybrush; // If you remove currentDiskColor <- DELETE THE FUNCTION
                
                }
                else // Black
                {
                    //#73FF7C00
                    c = Color.FromArgb(alpha, 255, 124, 0);
                    mybrush = new SolidColorBrush(c);

                    //sets the color of all captured disk spaces to current "color from win"
                    for (int i = 0; i < DiskBoard.Children.Count; i++)
                    {
                        SolidColorBrush rectangleColor = (SolidColorBrush)DiskBoard.Children.Cast<Rectangle>().ElementAt(i).Fill;

                        if (rectangleColor != null && rectangleColor.Color == blackColor.Color)
                            DiskBoard.Children.Cast<Rectangle>().ElementAt(i).Fill = mybrush;
                    }

                    //store the curent "color from win" color
                    blackColor.Color = c;

                    // Remove old disk (if any)
                    if (DiskBoard.Children.Cast<Rectangle>().First(
                        f => Grid.GetRow(f) == ((to.row + from.row) / 2) && Grid.GetColumn(f) == ((to.col + from.col) / 2)).Fill != mybrush)
                    {
                        removeDisk((to.row + from.row) / 2, (to.col + from.col) / 2);

                        addDisk((to.row + from.row) / 2, (to.col + from.col) / 2);
                    }

                    // Add disk
                    DiskBoard.Children.Cast<Rectangle>().First(
                        f => Grid.GetRow(f) == ((to.row + from.row) / 2) && Grid.GetColumn(f) == ((to.col + from.col) / 2)).Fill = mybrush; // If you remove currentDiskColor <- DELETE THE FUNCTION
                
                }
            }
            else if ((Math.Abs(from.row - to.row) == 2) ^ (Math.Abs(from.col - to.col) == 2))
            {
                // Capture peg
                removePeg((to.row + from.row) / 2, (to.col + from.col) / 2, true);
                soundmanager.playSelectEnemyPieceSound();
            }

            if (gameOptions.typeOfGame == Game.TypeOfGame.TimeAttack)
            {
                decrementStopWatch_Timer.Stop();
                stopWatchSprite.StopAnimation();
            }

            GameBoard.IsEnabled = false;

            /// Checks for a win in which:
            /// 1. GameBoard disabled
            /// 2. Win message alert
            if (game.diskBoard.checkForWin())
            {
                winningPath = game.diskBoard.getWinningPath(game.currentPlayer.color);
                layTrainTrack_Timer.Start();

                if (SoundManager.isMuted == false)
                {
                    Storyboard tempSB = this.Resources["fadeVolumeDown"] as Storyboard;
                    tempSB.Begin();
                }

                winnerExists = true;
                soundmanager.playBuildingRailRoadSound();

                

                string winner = game.currentPlayer.name;
                game.switchPlayer();
                string loser = game.currentPlayer.name;

                if (gameOptions.typeOfGame == Game.TypeOfGame.Local)
                {
                    gameOverTextBlock.Text = "Congratulations, " + winner + "! You have completed your railroad and defeated " + loser + ".";
                    playerIsWinner = true;
                }
                else if (game.currentPlayer.playerType == Player.TypeOfPlayer.Human)
                    gameOverTextBlock.Text = "Sorry " + loser + ". " + winner + " has completed their railroad first. Better luck next time!";
                else
                {
                    gameOverTextBlock.Text = "Congratulations, " + winner + "! You have completed your railroad and defeated " + loser + ".";
                    playerIsWinner = true;
                }

                if (gameOptions.typeOfGame != Game.TypeOfGame.Local && winner == ProfileManager.currentProfile.ProfileName)
                    ProfileManager.incrementLifetimeWins(ProfileManager.currentProfile.ProfileName);

                if (gameOptions.typeOfGame == Game.TypeOfGame.Campaign && winner == ProfileManager.currentProfile.ProfileName)
                {
                    int previousProgress = ProfileManager.currentProfile.CampaignProgress;
                    ProfileManager.incrementCampainProgress(ProfileManager.currentProfile.ProfileName, gameOptions.campaignLevel);

                    int currentProgress = ProfileManager.currentProfile.CampaignProgress;

                    if (previousProgress == 3 &&  currentProgress == 4)
                    {
                        gameOverTextBlock.Text = "Congratulations! You have saved the LOCO Islands! You deserve a statue!";
                    }
                }
            }
            else if (game.pegBoard.checkForTie())
            {
                string winner = game.currentPlayer.name;
                game.switchPlayer();
                string loser = game.currentPlayer.name;

                gameOverTextBlock.Text = "Sorry " + winner + ". You have captured every opponent engineer which results in a tie. Try and finish your railroad next time!";

                gameOverMessageBox.Visibility = System.Windows.Visibility.Visible;
                Storyboard gameOverSB = FindResource("gameOverStoryboard") as Storyboard;
                gameOverSB.Begin();
            }
            else
            {
                game.switchPlayer();

                if (game.currentPlayer.playerType == Player.TypeOfPlayer.AI)
                {

                    ThinkingBubble.Source = new BitmapImage(new Uri("/Locomotion;component/Media/Graphics/think0.png", UriKind.Relative));
                    ThinkingBubble.Visibility = System.Windows.Visibility.Visible;
                    thinkingBubble_inc = 0;
                    thinkingBubble_timer.Start();
                }
                else
                {
                    //ThinkingBubble.Visibility = System.Windows.Visibility.Hidden;
                    thinkingBubble_timer.Stop();
                }

                if (game.currentPlayer.color == Player.Color.Black)
                {
                    Canvas.SetLeft(turnIndicator, 1215);
                }
                else
                {
                    Canvas.SetLeft(turnIndicator, 24);
                }

                if (gameOptions.typeOfGame == Game.TypeOfGame.Network && !gameOptions.player1)
                {
                    if (game.currentPlayer.color == Player.Color.Black)
                    {
                        Canvas.SetLeft(turnIndicator, 24);
                    }
                    else
                    {
                        Canvas.SetLeft(turnIndicator, 1215);
                    }
                }
                //if (!game.diskBoard.checkForWin())
                worker.RunWorkerAsync();
            }
        }

        /// <summary> isCorner
        /// Will not allow a disk jump on the corner
        /// </summary>
        private static bool isCorner(Pair to, Pair from)
        {
            return ((to.row + from.row) / 2 == 0 && (to.col + from.col) / 2 == 0) 
                || ((to.row + from.row) / 2 == 0 && (to.col + from.col) / 2 == 5)
                || ((to.row + from.row) / 2 == 5 && (to.col + from.col) / 2 == 0) 
                || ((to.row + from.row) / 2 == 5 && (to.col + from.col) / 2 == 5);
        }

        /// <summary> isDiskJumpDistance
        /// Reports if a the move made is a disk jump
        /// </summary>
        private static bool isDiskJumpDistance(Pair to, Pair from)
        {
            return (Math.Abs(from.row - to.row) == 1) && (Math.Abs(from.col - to.col) == 1);
        }

        /// <summary> showAvailableMoves
        /// Tests possible moves for each direction
        /// Saves a list of possible moves and highlights
        /// </summary>
        private void showAvailableMoves()
        {
            availableMoves = new List<Pair>();
            List<Pair> moveSet = new List<Pair>();
            moveSet.Add (new Pair( 1, 0)); // S
            moveSet.Add (new Pair( 0, 1)); // E
            moveSet.Add (new Pair( -1, 0)); // N
            moveSet.Add (new Pair( 0, -1)); // W
            moveSet.Add (new Pair( 1, -1)); // SW
            moveSet.Add (new Pair( -1, 1)); // NE
            moveSet.Add (new Pair( -1, -1)); // NW
            moveSet.Add (new Pair( 1, 1)); // SE
            moveSet.Add (new Pair( 2, 0)); // Jump S
            moveSet.Add (new Pair( 0, 2)); // Jump E
            moveSet.Add (new Pair( -2, 0)); // Jump N
            moveSet.Add (new Pair( 0, -2)); // Jump W


            foreach (Pair p in moveSet)
            {
                if (game.isValid(new Pair(from.row+p.row, from.col+p.col), from))
                {
                    availableMoves.Add( new Pair( from.row + p.row,from.col + p.col));
                    //ImageBrush ib = new ImageBrush();
                    //ib.ImageSource = new BitmapImage(new Uri("/Locomotion;component/Media/Graphics/blackspot.png", UriKind.Relative));
                    PegBoard.Children.Cast<Rectangle>().First(e =>
                        Grid.GetRow(e) == from.row * 2 + p.row * 2
                        && Grid.GetColumn(e) == from.col * 2 + p.col * 2).Visibility = System.Windows.Visibility.Visible; //.Fill = ib;Brushes.LightGray;

                }
            }
        }

        /// <summary> cancelAvailableMoves
        /// Deslects available moves from stored list
        /// </summary>
        private void cancelAvailableMoves()
        {
            if (availableMoves != null)
                foreach (Pair to in availableMoves)
                    PegBoard.Children.Cast<Rectangle>().First(
                        e => Grid.GetRow(e) == to.row * 2 && Grid.GetColumn(e) == to.col * 2).Visibility = System.Windows.Visibility.Hidden;

            availableMoves = null;
        }

        private SolidColorBrush currentDiskColor()
        {
            if (game.currentPlayer.color == Player.Color.Black)
            {
                return Brushes.Black;
            }
            else // Player.Color.White
            {
                return Brushes.White;
            }
        }

        #endregion

        #region PEGS & DISKS

        /// <summary> addPeg
        /// Creates a new peg image
        /// Adds peg image to the UI
        /// (+2) overloads
        /// </summary>
        private Sprite addPeg(int row, int col, Player.Color color)
        {

            // Physical attributes
            Sprite img = null;

            // Graphics source
            if (color == Player.Color.Black)
            {
                img = new Sprite(robotSpriteSheetSource, 84, 97, robotSpriteSheetLength, 80);
                img.StopAnimation();
            }
            else if (color == Player.Color.White)
            {
                
                img = new Sprite(humanSpriteSheetSource, 84, 97, humanSpriteSheetLength, 80);
                img.StopAnimation();
            }

            // Physical attributes
            img.Height = 97;
            img.Width = 84;
            img.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
            img.VerticalAlignment = System.Windows.VerticalAlignment.Top;
            img.Stretch = Stretch.Fill;

            // Positional attributes
            img.Name = "rc" + row + col;
            img.Uid = "rc" + row + col;
            double left = getPegLeft(row, col);
            double top = getPegTop(row, col);

            if( pieceSelected )
            {
                img.StartAnimation();
            }

            img.Margin = new Thickness(left, top, 0, 0);
            img.MouseDown += new MouseButtonEventHandler(peg_MouseDown);
  

            // Add the new peg
            GameBoard.Children.Add(img);
            System.Windows.Controls.Panel.SetZIndex(img, 2*(row + col));
            return img;
        }
        private Sprite addPeg(Pair pos)
        {       
            return addPeg(pos.row, pos.col, game.currentPlayer.color);
        }
        private Sprite addPeg(Pair pos, Player.Color color)
        {
            return addPeg(pos.row, pos.col, color);
        }

        private void RemovePegAnimation(Sprite s)
        {
            Storyboard sb = new Storyboard();
            TimeSpan duration = TimeSpan.FromSeconds(1.5);
            DoubleAnimation animation = new DoubleAnimation { From = 1.0, To = 0.0, Duration = new Duration(duration) };

            Storyboard.SetTarget(animation, s);
            Storyboard.SetTargetProperty(animation, new PropertyPath("Opacity", 0));
            sb.Completed += new EventHandler(RemovePegAfterAnim);

            sb.Children.Add(animation);
            sb.Begin(s);
            storedSprite = s;
        }

        void RemovePegAfterAnim(object sender, EventArgs e)
        {
            GameBoard.Children.Remove(storedSprite);
        }

        /// <summary> removePeg
        /// removes peg image from the UI
        /// (+1) overload
        /// </summary>
        private void removePeg(int row, int col, bool capture)
        {
            if (capture)
            {
                RemovePegAnimation(fetchPegImage(row, col));
            }
            else
            {
                Sprite target = fetchPegImage(row, col);
                GameBoard.Children.Remove(target);
            }
            
        }
        private void removePeg(Pair pos, bool capture)
        {
            removePeg(pos.row, pos.col, capture);
        }

        /// <summary> fetchPegImage
        /// Returns a reference to the peg Image at some row,col
        /// </summary>
        private Sprite fetchPegImage(int row, int col)
        {
            string fromPegName = "rc" + row + col;
            Sprite target = null;
            foreach (UIElement uie in GameBoard.Children)
            {
                if (uie.Uid == fromPegName)
                {
                    target = (Sprite)uie;
                    break;
                }
            }
            return target;
        }
        private Sprite fetchPegImage(Pair pos)
        {
            return fetchPegImage(pos.row, pos.col);
        }

        /// <summary> getPegTop
        /// Gets the top coordinate of peg at a position from anchor point
        /// </summary>
        private double getPegTop(Pair pos)
        {
            return getPegTop(pos.row, pos.col);
        }
        private double getPegTop(int row, int col)
        {
            double top = 0;// System.Windows.SystemParameters.PrimaryScreenHeight - 800;//(System.Windows.SystemParameters.PrimaryScreenHeight - 700) / 2;//(double)uie.GetValue(Canvas.TopProperty)-110;
            top += (double)(row + col) * 55.12 - 67;
            return top;
        }

        /// <summary> getPegLeft
        /// Gets the left coordinate of peg at a position from anchor point
        /// </summary>
        private double getPegLeft(Pair pos)
        {
            return getPegLeft(pos.row, pos.col);
        }
        private double getPegLeft(int row, int col)
        {
            double left = 370; 
            left += (double)(col - row) * 93.28 - 64;
            return left;
        }

        /// <summary> peg_MouseDown
        /// On click event for peg image
        /// </summary>
        void peg_MouseDown(object sender, MouseButtonEventArgs e)
        {

            suggestCancelled = true;

            justClickedAPiece = true;
            if (!decrementStopWatch_Timer.Enabled && gameOptions.typeOfGame == Game.TypeOfGame.TimeAttack)
            {
                decrementStopWatch_Timer.Start();
                stopWatchSprite.StartAnimation();
            }
            // Parse Name to coordinates
            int row = Convert.ToInt32(((Image)sender).Name.Substring(2, 1));
            int col = Convert.ToInt32(((Image)sender).Name.Substring(3, 1));

            cancelAvailableMoves();
            SuggestMove.IsEnabled = true;

            if (game.pegBoard.checkColor(row, col) == game.currentPlayer.color)
            {
                if (pieceSelected && row==from.row && col==from.col)
                {
                    // click same piece -> deselect

                    pieceSelected = false;
                    //soundmanager.playMenuButtonClickSound();// must change later
                }
                else
                {
                    // select new peg
                    soundmanager.playMenuButtonClickSound();
                    pieceSelected = true;
                    from = new Pair(row, col);
                    showAvailableMoves();

                    (fetchPegImage(from)).AnimateOnce();
                    //removePeg(from);
                    //addPeg(from);
                }

                // Lift peg
                /*TranslateTransform trans = new TranslateTransform();
                target.RenderTransform = trans;
                DoubleAnimation anim = new DoubleAnimation(-40, TimeSpan.FromSeconds(.2)); // TimeSpan corresponds with a timer interval
                trans.BeginAnimation(TranslateTransform.YProperty, anim);
                placeLiftedPeg_Timer.Start();*/
            }
            else
            {
                // select opponent peg -> deselect
                from = null;
                pieceSelected = false;
            }
        }

        /// <summary> placeLiftedPeg_Timer_Tick
        /// Replaces the moved peg with a new one
        /// </summary>
        void placeLiftedPeg_Timer_Tick(object sender, EventArgs e)
        {
            // Remove old peg
            removePeg(placedPeg, false);

            // Place new peg
            addPeg(placedPeg, game.otherColor());

            if (!winnerExists)
            {
                soundmanager.playPlacePieceSound();
            }
            
            placeLiftedPeg_Timer.Stop();

            if (gameOptions.typeOfGame == Game.TypeOfGame.Network && game.currentPlayer.playerType == Player.TypeOfPlayer.Network)
            {
                //networkManager.sendPeerMove(game.currentPlayer.name, from.row, from.col, placedPeg.row, placedPeg.col);
                networkManager.sendPeerMove(ProfileManager.currentProfile.ProfileName, networkFrom.row, networkFrom.col, placedPeg.row, placedPeg.col);
            }
        }

        /// <summary> animatePeg
        /// finds the peg image
        /// moves and renames the image
        /// </summary>
        private void animatePeg(Pair to, Pair from)
        {
            // Move Peg
            Sprite target = fetchPegImage(from);
            var top = getPegTop(to) - getPegTop(from);//
            var left = getPegLeft(to) - getPegLeft(from);//
            TranslateTransform trans = new TranslateTransform();
            target.RenderTransform = trans;
            DoubleAnimation anim1 = new DoubleAnimation(left, TimeSpan.FromSeconds(1));
            DoubleAnimation anim2 = new DoubleAnimation(top, TimeSpan.FromSeconds(1));
            //DoubleAnimation anim1 = new DoubleAnimation(getPegLeft(from), getPegLeft(to), TimeSpan.FromSeconds(1));
            //DoubleAnimation anim2 = new DoubleAnimation(getPegTop(from), getPegTop(to), TimeSpan.FromSeconds(1));
            anim1.Completed += new EventHandler(anim1_Completed);
            trans.BeginAnimation(TranslateTransform.XProperty, anim1);
            trans.BeginAnimation(TranslateTransform.YProperty, anim2);

            target.Uid = target.Name = "rc" + to.row + to.col;
            System.Windows.Controls.Panel.SetZIndex(target, 2 * (to.row + to.col));
            placeLiftedPeg_Timer.Start();

            /*Storyboard upStoryboard = (Storyboard)this.Resources["upStoryboard"];
            DoubleAnimation temp = (DoubleAnimation)upStoryboard.Children[0];
            DoubleAnimation temp2 = (DoubleAnimation)upStoryboard.Children[1];
            temp2.From = 0;
            temp2.To = temp2.From - 40;
            Storyboard.SetTarget(temp, sbTarget);
            Storyboard.SetTarget(temp2, sbTarget);
            upStoryboard.Children[0] = temp;
            upStoryboard.Children[1] = temp2;
            upStoryboard.Begin();
            sbTarget.StartAnimation();

            moveDirection.Y = getPegTop(to) - getPegTop(from);
            moveDirection.X = getPegLeft(to) - getPegLeft(from);*/
        }

        void anim1_Completed(object sender, EventArgs e)
        {
            if( game.currentPlayer.playerType == Player.TypeOfPlayer.Human )
                ThinkingBubble.Visibility = System.Windows.Visibility.Hidden;
        }

        /// <summary> addDisk
        /// 
        /// </summary>
        private Sprite addDisk(Pair pos)
        {
            return addDisk(pos.row, pos.col);
        }
        private Sprite addDisk(int row, int col)
        {
            Sprite img;

            // Graphics source
            if (game.currentPlayer.color == Player.Color.Black)
            {
                img = new Sprite("ROBOT_PIECE", 200, 200, 1, 80);
            }
            else // (game.currentPlayer.color == Player.Color.White)
            {
                img = new Sprite("CITY_PIECE", 200, 200, 1, 80);
            }

            // Physical attributes
            img.Height = 80;
            img.Width = 100;
            img.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
            img.VerticalAlignment = System.Windows.VerticalAlignment.Top;
            img.Stretch = Stretch.Fill;

            // Positional attributes
            //img.RenderTransform = new RotateTransform(-45);
            img.Name = "DS" + row + col;
            img.Uid = "DS" + row + col;
            double left = getPegLeft(row, col) - 6 + (col-row);
            double top = getPegTop(row, col) + 109 + (row+col);

            img.Margin = new Thickness(left, top, 0, 0);
            img.Visibility = System.Windows.Visibility.Hidden;
            //img.Opacity = 0;

            Storyboard bounceDisk = FindResource("DropDisk") as Storyboard;

            DoubleAnimation temp = (DoubleAnimation)bounceDisk.Children[2];

            temp.From = -150;
            temp.To = 0;

            bounceDisk.Children[2] = temp;

            Storyboard.SetTarget(bounceDisk.Children[0], img);
            Storyboard.SetTarget(bounceDisk.Children[1], img);
            Storyboard.SetTarget(bounceDisk.Children[2], img);

            GameBoard.Children.Add(img);
            System.Windows.Controls.Panel.SetZIndex(img, 2*(row + col)+1 );

            

            img.BeginStoryboard(bounceDisk);
            
            return img;
        }

        private void removeDisk(int row, int col)
        {
            string fromPegName = "DS" + row + col;
            foreach (UIElement uie in GameBoard.Children)
            {
                if (uie.Uid == fromPegName)
                {
                    GameBoard.Children.Remove(uie);
                    break;
                }
            }
        }

        #endregion

        #region AI

        /// <summary> fetchAIMove
        /// Gets the suggested AI move from game core and returns
        /// </summary>
        private PieceMove fetchAIMove()
        {
            PieceMove pc = game.moveAI(aiDepth);
            return pc;
        }

        /// <summary> fetchSuggestedMove
        /// Fetches AI move for current player
        /// </summary>
        private PieceMove fetchSuggestedMove()
        {
            return game.moveAI(4);
            // show suggestion?
        }

        #endregion

        #region ASYNC WORKER

        /// <summary> worker_DoWork
        /// AI: fetches move
        /// Network: waits for move
        /// Human: locks GameBoard
        /// </summary>
        private void worker_DoWork(object sender, DoWorkEventArgs e)
        {
            //to allow storyboard to finish, take out for competition
            // run all background tasks here
            if (game.currentPlayer.playerType == Player.TypeOfPlayer.AI)
            {
                System.Threading.Thread.Sleep(2000);
                e.Result = fetchAIMove();
            }
            else
            {
                System.Threading.Thread.Sleep(2000); // 1000+
            }
        }

        /// <summary> worker_RunWorkerCompleted
        /// AI: makes AI move on GameBoard
        /// Human: unlocks GameBoard
        /// </summary>
        private void worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (!gameWindowClosing)
            {
                // update ui once worker completes his work
                if (game.currentPlayer.playerType == Player.TypeOfPlayer.AI)
                {
                    if (gameOptions.typeOfGame == Game.TypeOfGame.Campaign)
                        campaignStuff(); // banter


                    PieceMove pc = (PieceMove)e.Result;
                    gamewindowMove(pc.to, pc.from);
                    SuggestMove.IsEnabled = true;
                }
                else if (game.currentPlayer.playerType == Player.TypeOfPlayer.Human)
                {
                    GameBoard.IsEnabled = true;

                    if (gameOptions.typeOfGame == Game.TypeOfGame.TimeAttack && PauseMenuGrid.Visibility == System.Windows.Visibility.Hidden)
                    {
                        decrementStopWatch_Timer.Start();
                        stopWatchSprite.StartAnimation();
                    }
                }
            }
        }

        private void suggestMoveWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            suggestedMove = fetchSuggestedMove();
        }

        private void suggestMoveWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs a)
        {
            if (!suggestCancelled)
            {
                from = suggestedMove.from;
                pieceSelected = true;
                justClickedAPiece = false;

                (fetchPegImage(from)).AnimateOnce();
                PegBoard.Children.Cast<Rectangle>().First(e =>
                Grid.GetRow(e) == suggestedMove.to.row * 2
                && Grid.GetColumn(e) == suggestedMove.to.col * 2).Visibility = System.Windows.Visibility.Visible;

                PegBoard.Children.Cast<Rectangle>().First(e =>
                    Grid.GetRow(e) == from.row * 2
                    && Grid.GetColumn(e) == from.col * 2).Visibility = System.Windows.Visibility.Visible;

                availableMoves.Add(suggestedMove.from);
                availableMoves.Add(suggestedMove.to);
            }
        }

        #endregion

        #region NETWORK
        
        /// <summary> NetworkMoveReceived
        /// Invoked trigger for receiving a move in network play
        /// </summary>
        private void NetworkMoveReceived(object sender, MoveEventArgs move )
        {
            Pair to = new Pair(move.toRow, move.toCol);
            Pair from = new Pair(move.fromRow, move.fromCol);

            if (game.isValid(to, from))
            {
                pieceSelected = false;
                game.move(to, from); // inside game core

                 //OK WE NEED TO FIX THIS!
                this.Dispatcher.Invoke((Action)(() =>
                {
                    this.gamewindowMove(to, from);
                }));
            }

        }

        /// <summary> NetWorker_messageReceived
        /// Invoked trigger for network play chat box
        /// </summary>
        void NetWorker_messageReceived(object sender, MessageEventArgs message)
        {
            soundmanager.playIncomingMessageSound();
            this.Dispatcher.Invoke((Action)(()=> {

                chatBox.Text +=  networkManager.challengername +  ": " + message.text.ToString();
                chatBox.Text += "\n";
                chatBox.ScrollToEnd();
            }));
        }

        /// <summary> chatInput_KeyDown
        /// Event to send text from chatbox during network play
        /// </summary>
        private void chatInput_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Enter && chatInput.Text.Trim() != "" )
            {
                networkManager.sendPeerMessage(chatInput.Text);
                chatBox.Text += networkManager.getLocoPeer().name + ": " + chatInput.Text;
                chatBox.Text += "\n";
                chatInput.Clear();
                chatBox.ScrollToEnd();
            }
        }

        private void NetworkStatusReceived(object sender, ConnectionEventArgs status)
        {
            if (!status.connected)
            {
                this.Dispatcher.Invoke((Action)(() =>
                {
                    //System.Windows.Forms.MessageBox.Show(status.message);
                    if (gameOverMessageBox.Visibility == Visibility.Visible)
                    {
                        MainMenu mm = new MainMenu(this);
                        App.Current.MainWindow = mm;
                        mm.Show();
                    }
                    else
                    {

                        ErrorBackdrop.Visibility = Visibility.Visible;
                        GamePlayTheme.Stop();
                        GamePlayTheme.Volume = 0;
                        soundmanager.playRecordScratchSound();
                        ErrorText.Text = status.message;
                        chatInput.IsEnabled = false;
                    }
                }));
            }
        }

        #endregion NETWORK

        #region MOUSE EVENTS

        /// <summary> pegBoard_MouseDown
        /// Registered event for pegboard rectangles
        /// On click: checks for valid move or cancels
        /// </summary>
        private void pegBoard_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Rectangle rect = sender as Rectangle;
            int row = (int)rect.GetValue(Grid.RowProperty) / 2;
            int col = (int)rect.GetValue(Grid.ColumnProperty) / 2;

            if (from == null)
            {
                pieceSelected = false;
            }

            if (pieceSelected)
            {
                

                if (game.isValid(row, col, from))
                {
                    

                    game.move(row, col, from); // inside game core
                    gamewindowMove(new Pair(row, col), from); // animate GUI


                    if (gameOptions.typeOfGame == Game.TypeOfGame.Network)
                        networkFrom = from;
                       // networkManager.sendPeerMove(game.currentPlayer.name, from.row, from.col, row, col);
                }

                cancelAvailableMoves();
                from = null;
                pieceSelected = false;
            }
        }

        /// <summary> Board_MouseDown
        /// Called AFTER rectangle click event
        /// Cancels current selection
        /// </summary>
        private void Board_MouseDown(object sender, MouseButtonEventArgs e)
        {
            // Replace original peg
        }
        private void GameBoard_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (!justClickedAPiece && from != null)
            {
                // deselect for invalid move
                cancelAvailableMoves();
                from = null;
                pieceSelected = false;
            }

            justClickedAPiece = false;
        }

        /// <summary> Grid_MouseEnter
        /// Hover over animation
        /// </summary>
        private void Grid_MouseEnter(object sender, System.EventArgs e)
        {
            Rectangle rect = sender as Rectangle;
            int row = (int)rect.GetValue(Grid.RowProperty)/2;
            int col = (int)rect.GetValue(Grid.ColumnProperty)/2;

            if ((String)rect.DataContext == "Peg")
            {
                if (from != null && isDiskJumpDistance(new Pair(row, col), from) && game.isValid(row, col, from))
                {
                    Color c = Color.FromArgb(70,0,0,255);
                    SolidColorBrush myBrush = new SolidColorBrush(c);
                    //DiskBoard.Children.Cast<Rectangle>().First(f => Grid.GetRow(f) == (row + from.row) / 2 && Grid.GetColumn(f) == (col + from.col) / 2).Fill = whiteColor;
                }
                    //addDisk((row + from.row) / 2, (col + from.col) / 2);
                // prevColor = GameGrid.Children.Cast<Rectangle>().First(f => Grid.GetRow(f) == row && Grid.GetColumn(f) == column).Fill;
                //  GameGrid.Children.Cast<Rectangle>().First(f => Grid.GetRow(f) == row && Grid.GetColumn(f) == column).Fill = Brushes.DarkYellow;
            }
        }

        /// <summary> Grid_MouseLeave
        /// Hover off- return to normal
        /// </summary>
        private void Grid_MouseLeave(object sender, System.EventArgs e)
        {
            Rectangle rect = sender as Rectangle;
            int row = (int)rect.GetValue(Grid.RowProperty)/2;
            int col = (int)rect.GetValue(Grid.ColumnProperty)/2;

            if ((String)rect.DataContext == "Peg")
            {
                if (from != null && isDiskJumpDistance(new Pair(row, col), from) && game.isValid(row, col, from))
                    if (!game.diskBoard.diskExistsAt((row + from.row) / 2, (col + from.col) / 2))
                    {
                        //DiskBoard.Children.Cast<Rectangle>().First(f => Grid.GetRow(f) == (row + from.row) / 2 && Grid.GetColumn(f) == (col + from.col) / 2).Fill = Brushes.Transparent;
                    }
                    //removeDisk((row + from.row) / 2, (col + from.col) / 2);
            }
            //     GameGrid.Children.Cast<Rectangle>().First(f => Grid.GetRow(f) == row && Grid.GetColumn(f) == column).Fill = prevColor;
        }

        #endregion

        #region WINDOW EVENTS

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            if (gameOptions.typeOfGame == Game.TypeOfGame.Network && networkManager.allowKillNetworkThread)
                networkManager.peerDisconnect("Your opponent just left the game!");

            Storyboard fadeVolume = new Storyboard();
            fadeVolume = (Storyboard)FindResource("fadeVolumeOff");
            fadeVolume.Begin();
        }

        #endregion

        #region MISFITS & STORYBOARDS

        private void campaignStuff()
        {
            int campaignLevel = gameOptions.campaignLevel;
            /*GameMode = mode;


            if (GameMode == "CampLevel1")
            {
                t1.Text = "Level 1 character dialog.";
                Player1Dialog.Content = t1;

                t2.Text = "Level 1 enemy dialog.";
                Enemy1Dialog.Content = t2;

                BeginStoryboard((Storyboard)this.Resources["CampLevel1"]);
            }
            else if (GameMode == "CampLevel2")
            {
                t1.Text = "Level 2 character dialog.";
                Player1Dialog.Content = t1;

                t2.Text = "Level 2 enemy dialog.";
                Enemy1Dialog.Content = t2;

                BeginStoryboard((Storyboard)this.Resources["CampLevel2"]);
            }
            else if (GameMode == "CampLevel3")
            {
                t1.Text = "Level 3 character dialog.";
                Player1Dialog.Content = t1;

                t2.Text = "Level 3 enemy dialog.";
                Enemy1Dialog.Content = t2;

                BeginStoryboard((Storyboard)this.Resources["CampLevel3"]);
            }
            else if (GameMode == "CampLevel4")
            {
                t1.Text = "Level 4 character dialog.";
                Player1Dialog.Content = t2;

                t1.Text = "Level 4 enemy dialog.";
                Enemy1Dialog.Content = t2;

                BeginStoryboard((Storyboard)this.Resources["CampLevel4"]);
            }
            else if (GameMode == "QuickMatch")
            { 

            }*/

        }

        private void Enemy1Button_Click(object sender, RoutedEventArgs e)
        {
            soundmanager.playMenuButtonClickSound();

            if (SoundManager.isMuted == false)
            {
                Storyboard tempSB = this.Resources["fadeVolumeOff"] as Storyboard;
                tempSB.Begin();
            }

            Exit();
        }

        private void Exit()
        {
            if (gameOptions.typeOfGame == Game.TypeOfGame.Campaign)
            {
                MapScreen ms = new MapScreen(this, gameOptions);
                App.Current.MainWindow = ms;
                ms.Show();
                worker = new BackgroundWorker();
            }
            else
            {
                MainMenu mm = new MainMenu(this);
                App.Current.MainWindow = mm;
                mm.Show();
                worker = new BackgroundWorker();
            }
        }

        private void Storyboard_Completed(object sender, EventArgs e)
        {
            Canvas.SetLeft(sbTarget, moveDirection.X);
            moveDirection.Y = moveDirection.Y - 40;
            //Canvas.SetTop(sbTarget, top);
            
            Storyboard downStoryboard = (Storyboard)this.Resources["downStoryboard"];
            DoubleAnimation temp = (DoubleAnimation)downStoryboard.Children[0];
            DoubleAnimation temp2 = (DoubleAnimation)downStoryboard.Children[1];
            temp2.From = moveDirection.Y;// Canvas.GetTop(sbTarget);
            temp2.To = temp2.From + 40;
            
            Storyboard.SetTarget(temp, sbTarget);
            Storyboard.SetTarget(temp2, sbTarget);
            downStoryboard.Children[0] = temp;
            downStoryboard.Children[1] = temp2;
            downStoryboard.Begin();
        }

        private void gameOverMessageOkButton_Click(object sender, RoutedEventArgs e)
        {
            Storyboard sb = this.FindResource("ExitStoryboard") as Storyboard;
            sb.Begin();

            soundmanager.playMenuButtonClickSound();
        }

        private void RestartStoryboard_Completed(object sender, EventArgs e)
        {
            gameWindowClosing = true;
            GameWindow gameWindow;
            gameWindow = new GameWindow(this, gameOptions);

            App.Current.MainWindow = gameWindow;
            gameWindow.Show();
        }

        private void BTMStoryboard_Completed(object sender, EventArgs e)
        {
            gameWindowClosing = true;

            MainMenu mainmenu = new MainMenu(this);
            App.Current.MainWindow = mainmenu;
            mainmenu.Show();
        }

        private void Storyboard_Completed_2(object sender, EventArgs e)
        {
            Exit();
        }

        #endregion

        private void ExitButton_MouseDown(object sender, MouseButtonEventArgs e)
        {
            doRipple = false;
            ShowPauseMenu();

            if (SoundManager.isMuted == false)
            {
                Storyboard tempSB = this.Resources["fadeVolumeDown"] as Storyboard;
                tempSB.Begin();
            }
        }

        private void ShowPauseMenu()
        {
            if (decrementStopWatch_Timer.Enabled)
            {
                decrementStopWatch_Timer.Stop();
                stopWatchSprite.StopAnimation();
            }

            Storyboard sb = this.FindResource("PauseStoryboard") as Storyboard;
            sb.Begin();

            if (gameOptions.typeOfGame == Game.TypeOfGame.Network)
            {
                RestartButton.IsEnabled = false;
            }
        }

        private void ResumeButton_Click(object sender, RoutedEventArgs e)
        {
            if (game.currentPlayer.playerType == Player.TypeOfPlayer.Human && gameOptions.typeOfGame == Game.TypeOfGame.TimeAttack)
            {
                decrementStopWatch_Timer.Start();
                if (gameOptions.typeOfGame == Game.TypeOfGame.TimeAttack)
                    stopWatchSprite.StartAnimation();
            }

            soundmanager.playMenuButtonClickSound();

            if (SoundManager.isMuted == false)
            {
                Storyboard tempSB = this.Resources["fadeVolumeUp"] as Storyboard;
                tempSB.Begin();
            }

        }

        private void contentGrid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Sprite s;
            Point p = Mouse.GetPosition(contentGrid);
                    double ratio = (p.Y * 2 - 900) / 1800 +.7;
            if (doRipple)
            {
                if (rippleQ.Count < 25)
                {
                    s = new Sprite("ripple", 150, 102, 17, 100);
                    s.StopAnimation();
                    //s.Height = 70;
                    //s.Width = 100;
                    s.Height = 70 * ratio;
                    s.Width = 100 * ratio;

                    s.Margin = new Thickness(p.X * 2 - 1440, p.Y * 2 - 900, 0, 0);
                    System.Windows.Controls.Panel.SetZIndex(s, -1);

                    contentGrid.Children.Add(s);
                    rippleQ.Enqueue(s);
                }
                else
                {
                    s = rippleQ.Dequeue();
                    s.Visibility = System.Windows.Visibility.Visible;
                    s.Height = 67 * ratio;
                    s.Width = 100 * ratio;
                    s.Margin = new Thickness(p.X * 2 - 1440, p.Y * 2 - 900, 0, 0);
                }
                s.AnimateOnceHide();
            }

            doRipple = true;
        }

        private void BTMButton_Click(object sender, RoutedEventArgs e)
        {
            networkManager.peerDisconnect("Your opponent has resigned.");
            soundmanager.playMenuButtonClickSound();
            if (SoundManager.isMuted == false)
            {
                Storyboard tempSB = this.Resources["fadeVolumeOff"] as Storyboard;
                tempSB.Begin();
            }
        }

        private void gameWindow_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Escape && PauseMenuGrid.Visibility == Visibility.Hidden)
            {
                ShowPauseMenu();
                sbInProgress = false;
            }
            else if (e.Key == Key.Escape && !sbInProgress)
            {
                if (game.currentPlayer.playerType == Player.TypeOfPlayer.Human && gameOptions.typeOfGame == Game.TypeOfGame.TimeAttack)
                {
                    decrementStopWatch_Timer.Start();
                    if (gameOptions.typeOfGame == Game.TypeOfGame.TimeAttack)
                        stopWatchSprite.StartAnimation();
                }

                ResumeStoryboard.Begin();
                sbInProgress = true;

            }
        }

        private void MusicButton_MouseDown(object sender, MouseButtonEventArgs e)
        {
            doRipple = false;


            if (SoundManager.isMuted == false)
            {
                MusicButton.Source = new BitmapImage(new Uri("/Locomotion;component/Media/Graphics/musicOff.png", UriKind.Relative));
                soundmanager.Mute();
                GamePlayTheme.IsMuted = true;
                GamePlayTheme.Volume = 0;
            }

            else
            {
                MusicButton.Source = new BitmapImage(new Uri("/Locomotion;component/Media/Graphics/musicOff.png", UriKind.Relative));
                soundmanager.Unmute();
                GamePlayTheme.IsMuted = false;
                GamePlayTheme.Volume = 0.5;
                
            }
        }

        private void MusicButton_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (SoundManager.isMuted == true)
            {
                MusicButton.Source = new BitmapImage(new Uri("/Locomotion;component/Media/Graphics/musicOffHover.png", UriKind.Relative));
            }
            else
                MusicButton.Source = new BitmapImage(new Uri("/Locomotion;component/Media/Graphics/musicHover.png", UriKind.Relative));
        }

        private void MusicButton_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (SoundManager.isMuted == true)
            {
                MusicButton.Source = new BitmapImage(new Uri("/Locomotion;component/Media/Graphics/musicOff.png", UriKind.Relative));
            }
            else
                MusicButton.Source = new BitmapImage(new Uri("/Locomotion;component/Media/Graphics/music.png", UriKind.Relative));
        }

        private void ExitButton_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            ExitButton.Source = new BitmapImage(new Uri("/Locomotion;component/Media/Graphics/menuButtonHover.png", UriKind.Relative));
        }

        private void ExitButton_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
           ExitButton.Source = new BitmapImage(new Uri("/Locomotion;component/Media/Graphics/menuButton.png", UriKind.Relative));
        }

        private void RestartButton_Click(object sender, RoutedEventArgs e)
        {
            soundmanager.playMenuButtonClickSound();

            if (SoundManager.isMuted == false)
            {
                Storyboard tempSB = this.Resources["fadeVolumeOff"] as Storyboard;
                tempSB.Begin();
            }
        }

        private void DropDisk_Storyboard_Completed(object sender, EventArgs e)
        {
            foreach (UIElement uie in GameBoard.Children)
            {
                if (uie.Uid != null && uie.Uid.Length > 2 && uie.Uid.Substring(0, 2) == "DS")
                    System.Windows.Controls.Panel.SetZIndex(uie, 0);
            }
        }

        private void HelpButton_MouseDown(object sender, MouseButtonEventArgs e)
        {
            soundmanager.playMenuButtonClickSound();

            

            if (HelpGrid.Visibility == Visibility.Visible)
            {
                HelpGrid.Visibility = Visibility.Hidden;

                if (SoundManager.isMuted == false)
                {
                    Storyboard tempSB = this.Resources["fadeVolumeUp"] as Storyboard;
                    tempSB.Begin();
                }
            }
            else
            {
                HelpGrid.Visibility = Visibility.Visible;

                if (SoundManager.isMuted == false)
                {
                    Storyboard tempSB = this.Resources["fadeVolumeDown"] as Storyboard;
                    tempSB.Begin();
                }
            }
        }

        private void HelpButton_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            HelpButton.Source = new BitmapImage(new Uri("/Locomotion;component/Media/Graphics/helpButtonHover.png", UriKind.Relative));
        }

        private void HelpButton_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            HelpButton.Source = new BitmapImage(new Uri("/Locomotion;component/Media/Graphics/helpButton.png", UriKind.Relative));
        }

        private void SuggestMove_MouseDown(object sender, MouseButtonEventArgs m)
        {
            SuggestMove.IsEnabled = false;
            suggestCancelled = false;

            cancelAvailableMoves();
            availableMoves = new List<Pair>();

            suggestMoveWorker.RunWorkerAsync();
        }

        private void HelpGrid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            HelpGrid.Visibility = Visibility.Hidden;
        }

        private void SuggestMove_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            universalToolTip.Content = "Suggest a move.";

            SuggestMove.ToolTip = universalToolTip;

        }

        private void Storyboard_Completed_1(object sender, EventArgs e)
        {
            GameBoard.IsEnabled = true;
            CampaignDialogGrid.Visibility = System.Windows.Visibility.Hidden;
        }

        void nextDialogue_Timer_Tick(object sender, EventArgs e)
        {
            nextDialogue_Timer.Stop();
            HumanNext.IsEnabled = true;
            RobotNext.IsEnabled = true;
        }

        private void RobotNext_Click(object sender, RoutedEventArgs e)
        {
            progress++;


            if (progress == 5)
            {
                HumanNext.IsEnabled = false;
                RobotNext.IsEnabled = false;
                Storyboard end = this.Resources["CampaignDialogStoryboardOut"] as Storyboard;
                end.Begin();
            }
            else
            {
                writeNextDialogue();
                nextDialogue_Timer.Start();
                HumanNext.IsEnabled = false;
                RobotNext.IsEnabled = false;
            }
        }

        private void HumanNext_Click(object sender, RoutedEventArgs e)
        {
            progress++;

            if (progress == 5)
            {
                HumanNext.IsEnabled = false;
                RobotNext.IsEnabled = false;
                Storyboard end = this.Resources["CampaignDialogStoryboardOut"] as Storyboard;
                end.Begin();
            }
            else
            {
                writeNextDialogue();
                nextDialogue_Timer.Start();
                HumanNext.IsEnabled = false;
                RobotNext.IsEnabled = false;
            }
        }

        private void TypewriteTextblock(string textToAnimate, TextBlock txt, TimeSpan timeSpan)
        {
            Storyboard story = new Storyboard();
            story.FillBehavior = FillBehavior.HoldEnd;
            //story.RepeatBehavior = RepeatBehavior.Forever;

            DiscreteStringKeyFrame discreteStringKeyFrame;
            StringAnimationUsingKeyFrames stringAnimationUsingKeyFrames = new StringAnimationUsingKeyFrames();
            stringAnimationUsingKeyFrames.Duration = new Duration(timeSpan);

            string tmp = string.Empty;
            foreach (char c in textToAnimate)
            {
                discreteStringKeyFrame = new DiscreteStringKeyFrame();
                discreteStringKeyFrame.KeyTime = KeyTime.Paced;
                tmp += c;
                discreteStringKeyFrame.Value = tmp;
                stringAnimationUsingKeyFrames.KeyFrames.Add(discreteStringKeyFrame);
            }
            Storyboard.SetTargetName(stringAnimationUsingKeyFrames, txt.Name);
            Storyboard.SetTargetProperty(stringAnimationUsingKeyFrames, new PropertyPath(TextBlock.TextProperty));
            story.Children.Add(stringAnimationUsingKeyFrames);

            story.Begin(txt);
        }

        private void startDialogue()
        {
            CampaignDialogGrid.Visibility = System.Windows.Visibility.Visible;
            Storyboard start = this.Resources["CampaignDialogStoryboard"] as Storyboard;
            start.Begin();
            GameBoard.IsEnabled = false;
            HumanBlurb.Visibility = System.Windows.Visibility.Hidden;
            writeNextDialogue();
            nextDialogue_Timer.Start();
        }

        private void writeNextDialogue()
        {
            switch (gameOptions.campaignLevel)
            {
                case 1:
                    switch (progress)
                    {
                        case 0:
                            TypewriteTextblock("a1", RobotTalk, TimeSpan.FromSeconds(3));
                            break;
                        case 1:
                            TypewriteTextblock("a2", RobotTalk, TimeSpan.FromSeconds(3));
                            break;
                        case 2:
                            TypewriteTextblock("a3", RobotTalk, TimeSpan.FromSeconds(3));
                            break;
                        case 3:
                            TypewriteTextblock("a4", RobotTalk, TimeSpan.FromSeconds(3));
                            break;
                        case 4:
                            TypewriteTextblock("a5", RobotTalk, TimeSpan.FromSeconds(3));
                            break;
                    }
                    break;



                case 2:
                    switch (progress)
                    {
                        case 0:
                            TypewriteTextblock("b1", RobotTalk, TimeSpan.FromSeconds(3));
                            break;
                        case 1:
                            TypewriteTextblock("b2", RobotTalk, TimeSpan.FromSeconds(3));
                            break;
                        case 2:
                            TypewriteTextblock("b2", RobotTalk, TimeSpan.FromSeconds(3));
                            break;
                        case 3:
                            TypewriteTextblock("b2", RobotTalk, TimeSpan.FromSeconds(3));
                            break;
                        case 4:
                            TypewriteTextblock("b2", RobotTalk, TimeSpan.FromSeconds(3));
                            break;
                    }
                    break;



                case 3:
                    switch (progress)
                    {
                        case 0:
                            TypewriteTextblock("c1", RobotTalk, TimeSpan.FromSeconds(3));
                            break;
                        case 1:
                            TypewriteTextblock("c2", RobotTalk, TimeSpan.FromSeconds(3));
                            break;
                        case 2:
                            TypewriteTextblock("c2", RobotTalk, TimeSpan.FromSeconds(3));
                            break;
                        case 3:
                            TypewriteTextblock("c2", RobotTalk, TimeSpan.FromSeconds(3));
                            break;
                        case 4:
                            TypewriteTextblock("c2", RobotTalk, TimeSpan.FromSeconds(3));
                            break;
                    }
                    break;


                case 4:
                    switch (progress)
                    {
                        case 0:
                            TypewriteTextblock("d1", RobotTalk, TimeSpan.FromSeconds(3));
                            break;
                        case 1:
                            TypewriteTextblock("d2", RobotTalk, TimeSpan.FromSeconds(3));
                            break;
                        case 2:
                            TypewriteTextblock("d2", RobotTalk, TimeSpan.FromSeconds(3));
                            break;
                        case 3:
                            TypewriteTextblock("d2", RobotTalk, TimeSpan.FromSeconds(3));
                            break;
                        case 4:
                            TypewriteTextblock("d2", RobotTalk, TimeSpan.FromSeconds(3));
                            break;
                    }
                    break;
            }
        }
    }
}
