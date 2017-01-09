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

namespace Locomotion
{
    /// <summary>
    /// Interaction logic for Tutorial.xaml
    /// </summary>
    public partial class Tutorial : Window
    {
        Window prevWindow;
        Timer closingTimer = new Timer();
        Timer nextInstruction_Timer = new Timer();
        Timer placeLiftedPeg_Timer = new Timer();
        Timer placeNewPegs_Timer = new Timer();
        Timer placeNewDisks_Timer = new Timer();
        Timer disableGameBoard = new Timer();
        Timer disableNextButton = new Timer();
        int placeNewDisks_Inc = 0;
        Game game = new Game(ProfileManager.currentProfile.ProfileName, "Tutorial Nobody");
        Pair from;
        Pair placedPeg;
        bool pieceSelected = false;
        bool justClickedAPiece = false;
        bool checkPoint1 = false;
        bool checkPoint2_learnMove1 = false;
        bool checkPoint2_learnMove2 = false;
        bool checkPoint3 = false;
        bool checkPoint3_p2 = false;
        bool checkPoint4 = false;
        bool checkPoint5 = false;
        private int progress = 0;
        System.Windows.Controls.ToolTip universalToolTip = new System.Windows.Controls.ToolTip();
        SoundManager soundManager = SoundManager.InstanceCreator();
        List<Pair> availableMoves;
        List<Pair> disksAdded;
        GameOptions gameOptions;

        Timer layTrainTrack_Timer = new Timer();
        int layTrainTrack_Inc = 0;
        List<TrainTree> winningPath;
        
        public Tutorial( Window w, GameOptions gameOptions)
        {
            InitializeComponent();
            prevWindow = w;
            closingTimer.Interval = 200; 
            closingTimer.Tick += new EventHandler(closingTimer_Tick);
            closingTimer.Start(); // to close window
            GameBoard.IsEnabled = false;
            disksAdded = new List<Pair>();

            nextInstruction_Timer.Interval = 1200;
            nextInstruction_Timer.Tick += new EventHandler(nextInstruction_Timer_Tick);

            placeLiftedPeg_Timer.Interval = 1000;
            placeLiftedPeg_Timer.Tick += new EventHandler(placeLiftedPeg_Timer_Tick);
            placeNewPegs_Timer.Interval = 2200;
            placeNewPegs_Timer.Tick += new EventHandler(placeNewPegs_Timer_Tick);
            placeNewDisks_Timer.Interval = 100;
            placeNewDisks_Timer.Tick += new EventHandler(placeNewDisks_Timer_Tick);
            disableGameBoard.Interval = 20;
            disableGameBoard.Tick += new EventHandler(disableGameBoard_Tick);
            disableNextButton.Interval = 20;
            disableNextButton.Tick += new EventHandler(disableNextButton_Tick);

            tutorialContentGrid.LayoutTransform = new ScaleTransform(System.Windows.SystemParameters.PrimaryScreenWidth / 1440,
                System.Windows.SystemParameters.PrimaryScreenHeight / 900,
                System.Windows.SystemParameters.PrimaryScreenWidth / 2,
                System.Windows.SystemParameters.PrimaryScreenHeight / 2);

            // move background
            TranslateTransform moveMap = new TranslateTransform();
            MapCanvas.RenderTransform = moveMap;
            DoubleAnimation x = new DoubleAnimation(-720, 0, TimeSpan.FromSeconds(30));
            DoubleAnimation y = new DoubleAnimation(-450, 0, TimeSpan.FromSeconds(30));
            x.AccelerationRatio = .2;
            y.AccelerationRatio = .2;
            x.DecelerationRatio = .2;
            y.DecelerationRatio = .2;
            moveMap.BeginAnimation(TranslateTransform.XProperty, x);
            moveMap.BeginAnimation(TranslateTransform.YProperty, y);

            NextInstruction_Button.IsEnabled = false;
            //NextInstruction_Button.Opacity = .2;
            nextInstruction_Timer.Start();

            layTrainTrack_Timer.Tick += new EventHandler(layTrainTrack_Timer_Tick);
            layTrainTrack_Timer.Interval = 1200;


            this.gameOptions = gameOptions;
                //MapScreenUnMute();

            if ( !SoundManager.isMuted )
            {
                MapScreenUnMute();
            }
            else
            {
                MapScreenMute();
            }
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
                //Storyboard gameOverSB = FindResource("gameOverStoryboard") as Storyboard;
                //gameOverSB.Begin();
            }
        }

        void disableNextButton_Tick(object sender, EventArgs e)
        {
            nextInstruction_Timer.Stop();
            NextInstruction_Button.IsEnabled = false;
            //NextInstruction_Button.Opacity = .2;
        }

        void disableGameBoard_Tick(object sender, EventArgs e)
        {
            cancelAvailableMoves();
            GameBoard.IsEnabled = false;
        }

        private void MapScreenMute()
        {
            MapThemeSong.IsMuted = true;
            MapThemeSong.Volume = 0;
            SoundManager.isMuted = true;
            MusicButton.Source = new BitmapImage(new Uri("/Locomotion;component/Media/Graphics/musicOff.png", UriKind.Relative));
        }

        private void MapScreenUnMute()
        {
            MapThemeSong.IsMuted = false;
            MapThemeSong.Volume = 0.5;
            SoundManager.isMuted = false;
            MusicButton.Source = new BitmapImage(new Uri("/Locomotion;component/Media/Graphics/music.png", UriKind.Relative));
        }

        void placeNewPegs_Timer_Tick(object sender, EventArgs e)
        {
            placeNewPegs_Timer.Stop();
            winningPegSetup();
            game.pegBoard.initializeTutorialForWin();
            game.diskBoard.initializeTutorialForWin();
            //GameBoard.IsEnabled = true;
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

        private void WriteNextInstruction()
        {
            switch (progress)
            {
                // TypewriteTextblock("", bigInstructions, TimeSpan.FromSeconds());
                case 0:
                    TypewriteTextblock("Hello there,\n" + ProfileManager.currentProfile.ProfileName + "!\n\nWelcome to the \nLOCO islands!!", bigInstructions, TimeSpan.FromSeconds(2));
                    break;
                case 1:
                    TypewriteTextblock("...where the main source \nof transportation runs on \nsteam and iron...               \n\nthe LOCO trains!", bigInstructions, TimeSpan.FromSeconds(4));
                    break;
                case 2:
                    TypewriteTextblock("Build new railroads \nfor your train, and \ntravel through lush forests,\n            snowy tundras,\n      vast deserts,      \nand dangerous volcanoes!", bigInstructions, TimeSpan.FromSeconds(6));
                    break;
                case 3:
                    TypewriteTextblock("Let's travel to \nthe first island \nand learn how to play    \nLOCOmotion!", bigInstructions, TimeSpan.FromSeconds(4));
                    break;
                case 4:
                    TypewriteTextblock("             \nHere    \n  we     \n    gooooooooo!", bigInstructions, TimeSpan.FromSeconds(3));
                    nextInstruction_Timer.Stop();
                    // move train and slide in next screen
                    #region animate train & track
                    TranslateTransform slideUp = new TranslateTransform();
                    flatTrack.RenderTransform = slideUp;
                    DoubleAnimation upTrack = new DoubleAnimation(-900-flatTrack.Height, TimeSpan.FromSeconds(6.5));
                    upTrack.Completed += new EventHandler(up_Completed);
                    slideUp.BeginAnimation(TranslateTransform.YProperty, upTrack);
                    
                    TranslateTransform boxUp = new TranslateTransform();
                    chatBoard_Image.RenderTransform = boxUp;
                    DoubleAnimation chatslide = new DoubleAnimation(900 + chatBoard_Image.Height, 0, TimeSpan.FromSeconds(6.5));
                    boxUp.BeginAnimation(TranslateTransform.YProperty, chatslide);
                    chatBoard_Image.Visibility = System.Windows.Visibility.Visible;
                    
                    TranslateTransform chatUp = new TranslateTransform();
                    chatBox.RenderTransform = chatUp;
                    DoubleAnimation chattextslide = new DoubleAnimation(900 + chatBox.Height, 0, TimeSpan.FromSeconds(6.5));
                    chatUp.BeginAnimation(TranslateTransform.YProperty, chattextslide);
                    chatBox.Visibility = System.Windows.Visibility.Visible;
                    chatBox.Text = "LOCOmotion!\n\n";

                    TranslateTransform moveMiniTrain = new TranslateTransform();
                    miniTrain.RenderTransform = moveMiniTrain;
                    DoubleAnimation upTrain = new DoubleAnimation(-950, TimeSpan.FromSeconds(6.5));
                    DoubleAnimation rightTrain = new DoubleAnimation(4100, TimeSpan.FromSeconds(6.5));
                    moveMiniTrain.BeginAnimation(TranslateTransform.XProperty, rightTrain);
                    moveMiniTrain.BeginAnimation(TranslateTransform.YProperty, upTrain);

                    TranslateTransform slideBoardUp = new TranslateTransform();
                    gameBoard_Image.RenderTransform = slideBoardUp;
                    DoubleAnimation upBoard = new DoubleAnimation(950, 0, TimeSpan.FromSeconds(6.5));
                    slideBoardUp.BeginAnimation(TranslateTransform.YProperty, upBoard);

                    TranslateTransform slideWaterUp = new TranslateTransform();
                    WaterTexture.RenderTransform = slideWaterUp;
                    DoubleAnimation upWater = new DoubleAnimation(950, 0, TimeSpan.FromSeconds(6.5));
                    slideWaterUp.BeginAnimation(TranslateTransform.YProperty, upWater);

                    TranslateTransform slideBackUp = new TranslateTransform();
                    BoardBackground.RenderTransform = slideBackUp;
                    DoubleAnimation upBack = new DoubleAnimation(950, 0, TimeSpan.FromSeconds(6.5));
                    slideBackUp.BeginAnimation(TranslateTransform.YProperty, upBack);

                    gameBoard_Image.Visibility = System.Windows.Visibility.Visible;
                    BoardBackground.Visibility = System.Windows.Visibility.Visible;
                    WaterTexture.Visibility = System.Windows.Visibility.Visible;
                    Storyboard st = this.Resources["AnimateWaterStoryboard"] as Storyboard;
                    st.Begin();
                    #endregion
                    // on animation done, inc progress, call this function and start timer
                    break;
                case 5:
                    TypewriteTextblock("Each island is divided \ninto a 6x6 grid,  \nwith opposing cities on \nopposite corners.", bigInstructions, TimeSpan.FromSeconds(4));
                    chatBox.Text += "Gameboard:\n-- -- -- -- -- -- -- --\n";
                    chatBox.Text += "A 6x6 grid separates cities on opposite corners.\n";
                    break;
                case 6:
                    TypewriteTextblock("Your cities are shown \non the top and bottom of \nthe island!", bigInstructions, TimeSpan.FromSeconds(3));
                    chatBox.Text += "Tokens are placed in each grid square\n";
                    chatBox.Text += "Engineers move along the grid lines.\n\n";
                    break;
				case 7:
					TypewriteTextblock("As the conductor,     \nyour goal is to build a \nrailroad across the island \nbefore your opponent.", bigInstructions, TimeSpan.FromSeconds(4));
                    chatBox.Text += "Objective:\n-- -- -- -- -- -- -- --\nBe the first conductor to build a railroad across the island.";
					break;
                case 8:
                    TypewriteTextblock("This is a LOCO token!     \nTokens are placed in \neach grid square.   \n\nConnect your cities \nwith tokens to win \nthe game!", bigInstructions, TimeSpan.FromSeconds(4));
                    chatBox.Text += "\nUse tokens to connect your cities.\n\n";
                    /// animate token
                    #region animate token slide in
                    TranslateTransform diskIn = new TranslateTransform();
                    SampleDisk.RenderTransform = diskIn;
                    DoubleAnimation outdisk = new DoubleAnimation(-900, 0, TimeSpan.FromSeconds(1));
                    diskIn.BeginAnimation(TranslateTransform.XProperty, outdisk);
                    SampleDisk.Visibility = System.Windows.Visibility.Visible;
                    #endregion
                    break;
				case 9:
                    TypewriteTextblock("This is an engineer!     \n\nEngineers are placed \nwhere the grid \nlines cross.", bigInstructions, TimeSpan.FromSeconds(2.6));
                    chatBox.Text += "Engineers:\n-- -- -- -- -- -- -- --\n";
                    TranslateTransform diskOut1 = new TranslateTransform();
                    SampleDisk.RenderTransform = diskOut1;
                    DoubleAnimation outdisk1 = new DoubleAnimation(-900, TimeSpan.FromSeconds(1));
                    outdisk1.Completed += new EventHandler(outdisk1_Completed);
                    diskOut1.BeginAnimation(TranslateTransform.XProperty, outdisk1);
                    #region animate engineer slide in
                    TranslateTransform slideLeft = new TranslateTransform();
                    SamplePeg.RenderTransform = slideLeft;
                    DoubleAnimation leftPeg = new DoubleAnimation(900, 0, TimeSpan.FromSeconds(1));
                    slideLeft.BeginAnimation(TranslateTransform.XProperty, leftPeg);
                    SamplePeg.Visibility = System.Windows.Visibility.Visible;
                    #endregion
                    break;
                case 10:
                    TypewriteTextblock("Each conductor will start \nwith 8 engineers and \ntake turns moving one.", bigInstructions, TimeSpan.FromSeconds(2.4));
                    chatBox.Text += "Each conductor starts with 8 engineers.\n";
					break;
				case 11:
					TypewriteTextblock("Click on your engineer \nand learn how to move...", bigInstructions, TimeSpan.FromSeconds(2));
					nextInstruction_Timer.Stop();
                    #region engineer slideout
                    TranslateTransform slideOut = new TranslateTransform();
                    SamplePeg.RenderTransform = slideOut;
                    DoubleAnimation outPeg = new DoubleAnimation(900, TimeSpan.FromSeconds(1));
                    outPeg.Completed += new EventHandler(outPeg_Completed);
                    slideOut.BeginAnimation(TranslateTransform.XProperty, outPeg);
                    #endregion
					// On click, inc progress, call this function and start timer
					break;
				case 12:
					TypewriteTextblock("Very good!   \n\nClicking on an engineer  \nwill show all \navailable moves...", bigInstructions, TimeSpan.FromSeconds(3));
                    chatBox.Text += "Click on an engineer to show available moves.\n";
					break;
				case 13:
					//TypewriteTextblock("Click a potential move \nand discover the two types \nof movement...", bigInstructions, TimeSpan.FromSeconds(3));
                    TypewriteTextblock("When you jump across \na square of grass,   \nyou can place a token...          \n\nTry it now!", bigInstructions, TimeSpan.FromSeconds(3));
                    chatBox.Text += "Types of movement:\n";
					nextInstruction_Timer.Stop();
                    GameBoard.IsEnabled = true;
					// allow movement
					// fill in the two types on the right.. explanation for each (1. for defence 2. add/capture token)
					// on completion: inc progress, call this function and start timer
					break;
                case 14:
                    TypewriteTextblock("Your engineer can also \nslide between grassy \npatches-\n\nTry it now!", bigInstructions, TimeSpan.FromSeconds(3));
                    disableGameBoard.Stop();
                    GameBoard.IsEnabled = true;
					nextInstruction_Timer.Stop();
                    progress++;
                    break;
                case 15:
                    TypewriteTextblock("This does not place \na token,   \nbut may be a good \ndefensive move!", bigInstructions, TimeSpan.FromSeconds(3));
                    break;
                case 16:
                    TypewriteTextblock("You're doing great!   \n\nKeep moving your \nengineer to replace this \nenemy token with \nyour own!", bigInstructions, TimeSpan.FromSeconds(2));
					nextInstruction_Timer.Stop();
                    NextInstruction_Button.IsEnabled = false;
                    disableNextButton.Start();
                    removeDisk(1, 0);
                    addDisk(1, 0, Player.Color.Black);

                    Color c = Color.FromArgb(60, 255, 124, 0);
                    SolidColorBrush blackBrush = new SolidColorBrush(c);
                    DiskBoard.Children.Cast<Rectangle>().First(
                        f => Grid.GetRow(f) == 1 && Grid.GetColumn(f) == 0).Fill = blackBrush;
                    break;
                case 17:
                    TypewriteTextblock("Nicely done!   \nYou can keep moving \naround the board-    \n\njust click the arrow \nwhen you are ready \nto continue!", bigInstructions, TimeSpan.FromSeconds(4));
                    break;
				case 18:
					TypewriteTextblock("Next we will learn how \nto capture an enemy \nengineer.    \n\nClick on your \nengineer again....", bigInstructions, TimeSpan.FromSeconds(3));
                    checkPoint3 = true;
					nextInstruction_Timer.Stop();
                    GameBoard.IsEnabled = true;


                    game.pegBoard.initializeTutorialWithEnemy();
                    removePeg(placedPeg);
					// move peg back to middle?
                    addPeg(3, 3, Player.Color.White);
					// add enemy peg
                    addPeg(3, 4, Player.Color.Black);
					// on click show available, inc progress, call this function and start timer
					break;
				case 19:
                    checkPoint3_p2 = true;
					TypewriteTextblock("If nothing is blocking \nthe other side,    \nyou can vault over the \nenemy and eliminate \ntheir piece.       \n\nTry it now!", bigInstructions, TimeSpan.FromSeconds(5));
                    chatBox.Text += "   3. Vault over the enemy to eliminate their piece\n\n";
                    nextInstruction_Timer.Stop();
                    GameBoard.IsEnabled = true;
					// allow capture
					// on finish capture: inc progress, call this function and start timer
					break;
				case 20:
					TypewriteTextblock("Excellent!      \n\nIt will be a tie if you eliminate \nevery enemy engineer,   \nbut capturing some will \ngive you a big advantage!", bigInstructions, TimeSpan.FromSeconds(3));
					break;
				case 21:
					TypewriteTextblock("Now that you know \nhow engineers move,    \n\nfind the winning move \nto complete your \nrailroad...", bigInstructions, TimeSpan.FromSeconds(3));
                    chatBox.Text += "End of game:\n-- -- -- -- -- -- -- --\nUse your engineers to form a chain of disks between your corner cities.\n";
                    chatBox.Text += "Diagonals do not count.\n";
                    chatBox.Text += "The first conductor to connect tokens across the island wins.\n";
                    chatBox.Text += "Do not capture all your opponent's engineers or it will only be a tie.\n";
					nextInstruction_Timer.Stop();
                    placeNewPegs_Timer.Start();
					// allow play
					// allow hint?
                    #region animate train & track
                    flatTrack.Margin = new Thickness(0, 900, 0, 0);
                    System.Windows.Controls.Panel.SetZIndex(flatTrack, 99);
                    TranslateTransform slideUp11 = new TranslateTransform();
                    flatTrack.RenderTransform = slideUp11;
                    DoubleAnimation upTrack11 = new DoubleAnimation(-900-flatTrack.Height, TimeSpan.FromSeconds(5));
                    upTrack11.AccelerationRatio = .2;
                    upTrack11.Completed +=new EventHandler(upTrack11_Completed);
                    slideUp11.BeginAnimation(TranslateTransform.YProperty, upTrack11);
                    flatTrack.Visibility = System.Windows.Visibility.Visible;

                    miniTrain.Margin = new Thickness(-2300, 600, 0, 0);
                    System.Windows.Controls.Panel.SetZIndex(miniTrain, 99);
                    TranslateTransform moveMiniTrain1 = new TranslateTransform();
                    miniTrain.RenderTransform = moveMiniTrain1;
                    DoubleAnimation upTrain1 = new DoubleAnimation(-950, TimeSpan.FromSeconds(5));
                    upTrain1.AccelerationRatio = .2;
                    DoubleAnimation rightTrain1 = new DoubleAnimation(4100, TimeSpan.FromSeconds(5));
                    moveMiniTrain1.BeginAnimation(TranslateTransform.XProperty, rightTrain1);
                    moveMiniTrain1.BeginAnimation(TranslateTransform.YProperty, upTrain1);
                    miniTrain.Visibility = System.Windows.Visibility.Visible;

                    System.Windows.Controls.Panel.SetZIndex(IntroStuff, 101);
                    #endregion
					// on win: inc progress, call this function and start timer
					break;
				case 22:
                    TypewriteTextblock("Congratulations!!         \n\nBy connecting your \ncorners with tokens,   \nyou have finished \nyour railroad first \nfor victory!", bigInstructions, TimeSpan.FromSeconds(3.6));
                    Storyboard helpOut = this.Resources["HelpOut"] as Storyboard;
                    helpOut.Begin();
					break;
				//case 21:
					//TypewriteTextblock("Would you like to \nlearn more about \nLOCOmotion menu \nand gameplay?", bigInstructions, TimeSpan.FromSeconds(2));
					//nextInstruction_Timer.Stop();

					// prompt yes/no
                    //yes.IsEnabled = true;
                    //no.IsEnabled = true;
                    //Storyboard temp = this.Resources["FadeButtonsIn"] as Storyboard;
                    //temp.Begin();
					// if no, progress =23
					// if yes progress ++
					// call this function and start timer
					//break;
                /*case 21:
                    TypewriteTextblock("
				case 21:
					TypewriteTextblock("Visit the profile button \nto log off or visit \nprofile options", bigInstructions, TimeSpan.FromSeconds(2.5));
					// point to 
					break;
				case 22:
					// Show tickets
                    Storyboard fadeIn = this.Resources["TicketIn"] as Storyboard;
                    fadeIn.Begin();
					TypewriteTextblock("From here, you can \ncreate a new profile or \nedit your current one.     \nTrack your campaign progress \nand total wins!", bigInstructions, TimeSpan.FromSeconds(3));
					break;
                case 23:
                    Storyboard fadeOut = this.Resources["TicketOut"] as Storyboard;
                    fadeOut.Begin();
					// hide tickets, show second level new game menu
					TypewriteTextblock("This is where you \nclick to play this \ntutorial again.", bigInstructions, TimeSpan.FromSeconds(3));
					break;
				case 24:
					TypewriteTextblock("Continue your LOCO adventure,   \nand save each island \nin story mode!", bigInstructions, TimeSpan.FromSeconds(3));
                    TranslateTransform menuUp = new TranslateTransform();
                    mockMenu_Image.RenderTransform = menuUp;
                    DoubleAnimation up = new DoubleAnimation(0,-69, TimeSpan.FromSeconds(1));
                    menuUp.BeginAnimation(TranslateTransform.YProperty, up);
					break;
				case 25:
                    menuUp = new TranslateTransform();
                    mockMenu_Image.RenderTransform = menuUp;
                    DoubleAnimation up1 = new DoubleAnimation(-69, -138, TimeSpan.FromSeconds(1));
                    menuUp.BeginAnimation(TranslateTransform.YProperty, up1);
					TypewriteTextblock("Play a quick match \nwith a computer opponent.    \nTry time attack for \na LOCO challenge!", bigInstructions, TimeSpan.FromSeconds(3));
					break;
                case 26:
                    menuUp = new TranslateTransform();
                    mockMenu_Image.RenderTransform = menuUp;
                    DoubleAnimation up2 = new DoubleAnimation(-138, -207, TimeSpan.FromSeconds(1));
                    menuUp.BeginAnimation(TranslateTransform.YProperty, up2);
					TypewriteTextblock("Click here to play \nagainst a friend \non the same screen \nor online.", bigInstructions, TimeSpan.FromSeconds(3));
					break*/
				case 23:
					TypewriteTextblock("Thanks " + ProfileManager.currentProfile.ProfileName + "\nfor playing through the \ntutorial!", bigInstructions, TimeSpan.FromSeconds(3));
                    nextInstruction_Timer.Stop();
                    nextInstruction_Timer.Start();
					break;					
				case 24:
					TypewriteTextblock("If you are ever confused,   \nthe green help button \nwill provide instructions \nfor your current screen!", bigInstructions, TimeSpan.FromSeconds(3));
					break;
				case 25:
                    //nextInstruction_Timer.Stop();
                    //nextInstruction_Timer.Interval = 800;
                    //nextInstruction_Timer.Start();
                    NextInstruction_Button.Content = "Menu";
                    TypewriteTextblock("You have now \ncompleted the tutorial!      \n\nWork hard and I know you \nwill someday be the \nfinest conductor in the \nLOCO islands!", bigInstructions, TimeSpan.FromSeconds(5));
                    //NextInstruction_Button.IsEnabled = false;
                    break;
					// change or remove icon -> need to go back to menu
            }
        }

        void outdisk1_Completed(object sender, EventArgs e)
        {
            SampleDisk.Visibility = System.Windows.Visibility.Hidden;
        }

        void upTrack11_Completed(object sender, EventArgs e)
        {
            GameBoard.IsEnabled = true;
        }

        #region events

        private void NextInstruction_Button_Click(object sender, RoutedEventArgs e)
        {
            NextInstruction_Button.IsEnabled = false;
            //NextInstruction_Button.Opacity = .2;
            progress++;

            if (NextInstruction_Button.Content.ToString() == "Menu")
            {
                NextInstruction_Button.IsEnabled = false;
                Storyboard outt = this.Resources["BackToMainMenu"] as Storyboard;
                outt.Begin();
            }
            else
            {
                nextInstruction_Timer.Start();
                WriteNextInstruction();
            }
        }

        private void NextInstruction_MouseDown(object sender, MouseButtonEventArgs e)
        {
            NextInstruction_Button.IsEnabled = false;
            //NextInstruction_Button.Opacity = .2;
            nextInstruction_Timer.Start();
            progress++;
            WriteNextInstruction();
        }

        void up_Completed(object sender, EventArgs e)
        {
            progress++;
            WriteNextInstruction();
            nextInstruction_Timer.Start();
        }

        void nextInstruction_Timer_Tick(object sender, EventArgs e)
        {
            nextInstruction_Timer.Stop();
            NextInstruction_Button.IsEnabled = true;
        }

        private void closingTimer_Tick(object sender, EventArgs e)
        {
            closingTimer.Stop();
            prevWindow.Close();
            bigBubble.Visibility = System.Windows.Visibility.Visible;

            WriteNextInstruction();
        }

        void outPeg_Completed(object sender, EventArgs e)
        {
            SamplePeg.Visibility = System.Windows.Visibility.Hidden;
            //progress++;

            /*// after slide off, drop peg into board
            double left = getPegLeft(3, 3) - 27;
            double top = getPegTop(3, 3) + 115;

            SamplePeg.Height = 100;
            SamplePeg.Width = 50;
            SamplePeg.Margin = new Thickness(left, top, 0, 0);
            SamplePeg.Visibility = System.Windows.Visibility.Visible;
            //img.Opacity = 0;

            Storyboard bouncePeg = FindResource("DropPeg") as Storyboard;  ////////////////////??????????

            DoubleAnimation temp = (DoubleAnimation)bouncePeg.Children[2];

            temp.From = -150;
            temp.To = 0;

            bouncePeg.Children[2] = temp;

            Storyboard.SetTarget(bouncePeg.Children[0], SamplePeg);
            Storyboard.SetTarget(bouncePeg.Children[1], SamplePeg);
            Storyboard.SetTarget(bouncePeg.Children[2], SamplePeg);*/

            addPeg(3, 3, Player.Color.White);

            game.pegBoard.initializeTutorial(); // places the one peg in game core
            GameBoard.IsEnabled = true;
        }

        #endregion

        private void winningPegSetup()
        {
            List<UIElement> oldPegsDisks = new List<UIElement>();
            foreach(UIElement uie1 in GameBoard.Children)
            {
                if ( uie1.Uid != null && uie1.Uid != "" && uie1.Uid.Length == 4 &&
                    (uie1.Uid.Substring(0,2) == "DS" || uie1.Uid.Substring(0,2) == "rc"))
                    oldPegsDisks.Add(uie1);
            }
            foreach(UIElement uie2 in oldPegsDisks)
            {
                uie2.Visibility = System.Windows.Visibility.Hidden;
                GameBoard.Children.Remove(uie2);
                if (uie2.Uid.Substring(0,2) == "DS")
                {
                    int row = Convert.ToInt32(uie2.Uid.Substring(2,1));
                    int col = Convert.ToInt32(uie2.Uid.Substring(3,1));
                    DiskBoard.Children.Cast<Rectangle>().First(f => Grid.GetRow(f) == row && Grid.GetColumn(f) == col).Fill = Brushes.Transparent;
                }
            }
            placeNewDisks_Timer.Start();


        }


        void placeNewDisks_Timer_Tick(object sender, EventArgs e)
        {
            Color c;
            SolidColorBrush whiteBrush, blackBrush;
            c = Color.FromArgb(40, 4, 0, 255);
            whiteBrush = new SolidColorBrush(c);
            c = Color.FromArgb(60, 255, 124, 0);
            blackBrush = new SolidColorBrush(c);
            switch (placeNewDisks_Inc)
            {
                case 0:
                    addPeg(6, 4, Player.Color.White);
                    addDisk(5, 4, Player.Color.White, whiteBrush); 
                    break;
                case 1:
                    addPeg(5, 5, Player.Color.White);
                    addDisk(5, 3, Player.Color.White, whiteBrush); 
                    break;
                case 2:
                    //addPeg(4, 5, Player.Color.White);
                    addDisk(4, 5, Player.Color.White, whiteBrush); 
                    break;
                case 3:
                    //addPeg(4, 3, Player.Color.Black);
                    addDisk(4, 3, Player.Color.White, whiteBrush); 
                    break;
                case 4:
                    addDisk(4, 2, Player.Color.Black, blackBrush); 
                    break;
                case 5:
                    addPeg(4, 1, Player.Color.Black);
                    addDisk(4, 1, Player.Color.Black, blackBrush); 
                    break;
                case 6:
                    addDisk(4, 0, Player.Color.Black, blackBrush); 
                    //addPeg(3, 5, Player.Color.Black);
                    break;
                case 7:
                    addPeg(3, 3, Player.Color.White);
                    addDisk(3, 3, Player.Color.White, whiteBrush); 
                    break;
                case 8:
                    //addPeg(3, 2, Player.Color.White);
                    break;
                case 9:
                    addDisk(2, 2, Player.Color.Black, blackBrush); 
                    addPeg(2, 4, Player.Color.Black);
                    addDisk(2, 3, Player.Color.Black, blackBrush); 
                    break;
                case 10:
                    addDisk(3, 1, Player.Color.White, whiteBrush);
                    break;
                case 12:
                    addDisk(2, 1, Player.Color.White, whiteBrush); 
                    break;
                case 13:
                    addDisk(1, 5, Player.Color.Black, blackBrush); 
                    break;
                case 14:
                    addDisk(1, 4, Player.Color.Black, blackBrush); 
                    break;
                case 15:
                    addPeg(1, 3, Player.Color.Black);
                    addDisk(1, 3, Player.Color.Black, blackBrush); 
                    break;
                case 16:
                    addPeg(1, 1, Player.Color.White);
                    addDisk(1, 1, Player.Color.White, whiteBrush); 
                    break;
                case 17:
                    addDisk(1, 0, Player.Color.White, whiteBrush); 
                    break;
                case 18:
                    addPeg(0, 4, Player.Color.Black);
                    addDisk(0, 4, Player.Color.Black, blackBrush); 
                    break;
                case 19:
                    addDisk(0, 3, Player.Color.Black, blackBrush); 
                    break;
                case 20:
                    addDisk(0, 1, Player.Color.White, whiteBrush); 
                    break;
                case 21:
                    placeNewDisks_Timer.Stop();
                    break;

            }
            placeNewDisks_Inc++;
        }



        #region from GameWindow


        #region available and cancel
        /// <summary> showAvailableMoves
        /// Tests possible moves for each direction
        /// Saves a list of possible moves and highlights
        /// </summary>
        private void showAvailableMoves()
        {
            availableMoves = new List<Pair>();
            List<Pair> moveSet = new List<Pair>();
            moveSet.Add(new Pair(1, 0)); // S
            moveSet.Add(new Pair(0, 1)); // E
            moveSet.Add(new Pair(-1, 0)); // N
            moveSet.Add(new Pair(0, -1)); // W
            moveSet.Add(new Pair(1, -1)); // SW
            moveSet.Add(new Pair(-1, 1)); // NE
            moveSet.Add(new Pair(-1, -1)); // NW
            moveSet.Add(new Pair(1, 1)); // SE
            moveSet.Add(new Pair(2, 0)); // Jump S
            moveSet.Add(new Pair(0, 2)); // Jump E
            moveSet.Add(new Pair(-2, 0)); // Jump N
            moveSet.Add(new Pair(0, -2)); // Jump W


            foreach (Pair p in moveSet)
            {
                if (game.isValid(new Pair(from.row + p.row, from.col + p.col), from))
                {
                    availableMoves.Add(new Pair(from.row + p.row, from.col + p.col));
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
                img = new Sprite("robotSprite", 84, 97, 40, 80);
                img.StopAnimation();
            }
            else if (color == Player.Color.White)
            {
                img = new Sprite("humanSprite", 84, 97, 35, 80);
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

            if (pieceSelected)
            {
                img.StartAnimation();
            }

            img.Margin = new Thickness(left, top, 0, 0);
            img.MouseDown += new MouseButtonEventHandler(peg_MouseDown);


            // Add the new peg
            GameBoard.Children.Add(img);
            System.Windows.Controls.Panel.SetZIndex(img, 2 * (row + col));
            return img;
        }
        private Sprite addPeg(Pair pos)
        {
            return addPeg(pos.row, pos.col, Player.Color.White);
        }
        private Sprite addPeg(Pair pos, Player.Color color)
        {
            return addPeg(pos.row, pos.col, color);
        }

        /// <summary> removePeg
        /// removes peg image from the UI
        /// (+1) overload
        /// </summary>
        private void removePeg(int row, int col)
        {
            Sprite target = fetchPegImage(row, col);
            GameBoard.Children.Remove(target);
        }
        private void removePeg(Pair pos)
        {
            removePeg(pos.row, pos.col);
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
            if (!checkPoint1)
            {
                checkPoint1 = true;
                progress++;
                WriteNextInstruction();
                nextInstruction_Timer.Start();
                GameBoard.IsEnabled = false;
            }

            //if (checkPoint3 && !checkPoint4 && progress < 13)
            //{
            //    checkPoint3 = true;
            //    progress++;
            //    WriteNextInstruction();
            //}

            if (checkPoint3 && !checkPoint3_p2)
            {
                progress++;
                WriteNextInstruction();
            }


            justClickedAPiece = true;
            // Parse Name to coordinates
            int row = Convert.ToInt32(((Image)sender).Name.Substring(2, 1));
            int col = Convert.ToInt32(((Image)sender).Name.Substring(3, 1));

            cancelAvailableMoves();
            // Replace original peg
            if (from != null)
            {
                //removePeg(from);
                //addPeg(from);
                //(fetchPegImage(from)).StopAnimation();
            }

            if (game.pegBoard.checkColor(row, col) == Player.Color.White)
            {
                if (pieceSelected && row == from.row && col == from.col)
                {
                    // click same piece -> deselect

                    pieceSelected = false;
                    soundManager.playMenuButtonClickSound();// must change later
                }
                else
                {
                    // select new peg
                    soundManager.playMenuButtonClickSound();
                    pieceSelected = true;
                    from = new Pair(row, col);
                    showAvailableMoves();

                    (fetchPegImage(from)).StartAnimation();
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
            //else
            {
                // select opponent peg -> deselect
                //from = null;
                //pieceSelected = false;
            }
        }

        /// <summary> placeLiftedPeg_Timer_Tick
        /// Replaces the moved peg with a new one
        /// </summary>
        void placeLiftedPeg_Timer_Tick(object sender, EventArgs e)
        {
            // Remove old peg
            removePeg(placedPeg);

            // Place new peg
            System.Windows.Controls.Panel.SetZIndex( addPeg(placedPeg, Player.Color.White), placedPeg.row*2 + placedPeg.col*2);
            soundManager.playPlacePieceSound();
            placeLiftedPeg_Timer.Stop();

            //gameBoard.IsEnabled = true;

            foreach (UIElement uie in GameBoard.Children)
            {
                if (uie.Uid.Length == 4 && uie.Uid.Substring(0, 2) == "DS")
                {
                    System.Windows.Controls.Panel.SetZIndex(uie, 1);
                }
            }

            if (checkPoint2_learnMove1 && checkPoint2_learnMove2 && !checkPoint3)
            {
                GameBoard.IsEnabled = true;
                nextInstruction_Timer.Start();
            }
            else if (!checkPoint2_learnMove1 && checkPoint2_learnMove2)
            {
                GameBoard.IsEnabled = true;
            }

            if (checkPoint5)
            {
                progress++;
                WriteNextInstruction();
                nextInstruction_Timer.Start();

                HelpButton.IsEnabled = false;
                HelpButton.Visibility = System.Windows.Visibility.Hidden;

                // animation - add track here

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
            var top = getPegTop(to) - getPegTop(from);
            var left = getPegLeft(to) - getPegLeft(from);
            TranslateTransform trans = new TranslateTransform();
            target.RenderTransform = trans;
            DoubleAnimation anim1 = new DoubleAnimation(left, TimeSpan.FromSeconds(1));
            DoubleAnimation anim2 = new DoubleAnimation(top, TimeSpan.FromSeconds(1));
            anim1.Completed += new EventHandler(anim1_Completed);
            trans.BeginAnimation(TranslateTransform.XProperty, anim1);
            trans.BeginAnimation(TranslateTransform.YProperty, anim2);

            target.Uid = target.Name = "rc" + to.row + to.col;
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
            //if (game.currentPlayer.playerType == Player.TypeOfPlayer.Human)
                //ThinkingBubble.Visibility = System.Windows.Visibility.Hidden;
        }

        /// <summary> addDisk
        /// 
        /// </summary>
        private Sprite addDisk(Pair pos)
        {
            return addDisk(pos.row, pos.col, Player.Color.White);
        }
        private Sprite addDisk(int row, int col, Player.Color color, SolidColorBrush mybrush)
        {
            DiskBoard.Children.Cast<Rectangle>().First(f => Grid.GetRow(f) == row && Grid.GetColumn(f) == col).Fill = mybrush;
            return addDisk(row, col, color);
        }

        private Sprite addDisk(int row, int col, Player.Color color)
        {
            Sprite img;

            // Graphics source
            if (color == Player.Color.Black)
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
            double left = getPegLeft(row, col) - 6 + (col - row);
            double top = getPegTop(row, col) + 109 + (row + col);

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
            System.Windows.Controls.Panel.SetZIndex(img, 2 * (row + col) + 1);



            img.BeginStoryboard(bounceDisk);

            return img;
        }
        private Sprite addDisk(int row, int col)
        {
            return addDisk(row, col, Player.Color.White);
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
                    // valid move
                    //fetchPegImage(from).StopAnimation();
                    //removePeg(from);
                    //addPeg(from);




                    if (checkPoint3 && !checkPoint4)
                    {
                        if (row == 3 && col == 5)
                        {
                            game.move(row, col, from); // inside game core
                            gamewindowMove(new Pair(row, col), from); // animate GUI
                        }
                        else
                        {
                            TypewriteTextblock("Valid move...     \n\nbut click again to capture \nthe enemy engineer!", bigInstructions, TimeSpan.FromSeconds(3));
                        }
                    } 
                    else if (checkPoint4)
                    {
                        if (row == 4 && col == 2)
                        {
                            game.move(row, col, from); // inside game core
                            gamewindowMove(new Pair(row, col), from); // animate GUI
                            //addDisk(4, 2, Player.Color.White);
                            checkPoint5 = true;

                            if (HelpButton.Opacity < .1)
                            {
                                Storyboard helpIn = this.Resources["HelpOut"] as Storyboard;
                                helpIn.Begin();
                            }


                            winningPath = game.diskBoard.getWinningPath(game.currentPlayer.color);
                            layTrainTrack_Timer.Start();
                            soundManager.playBuildingRailRoadSound();
                        }
                        else
                        {
                            TypewriteTextblock("Valid move...       \nbut not the winning move!    \n\nHover your mouse over \nthe question mark for \na hint!", bigInstructions, TimeSpan.FromSeconds(3));

                            Storyboard helpIn = this.Resources["HelpIn"] as Storyboard;
                            helpIn.Begin();
                        }
                    }
                    else
                    {
                        game.move(row, col, from); // inside game core
                        gamewindowMove(new Pair(row, col), from); // animate GUI

                    }
                    //if (gameOptions.typeOfGame == Game.TypeOfGame.Network)
                        //networkManager.sendPeerMove(game.currentPlayer.name, from.row, from.col, row, col);
                }
                else // not valid
                {
                    // replace original pin

                    // should be inaccesible due to the the denial of click events for invalid moves, but leave anyway?

                    //removePeg(from.row, from.col);
                    //addPeg(from.row, from.col, game.currentPlayer.color);
                    //(fetchPegImage(from)).StopAnimation();
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
                //removePeg(from); // remove 'selected'
                //addPeg(from); // replace original
                //(fetchPegImage(from)).StopAnimation();
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
            int row = (int)rect.GetValue(Grid.RowProperty) / 2;
            int col = (int)rect.GetValue(Grid.ColumnProperty) / 2;

            if ((String)rect.DataContext == "Peg")
            {
                if (from != null && isDiskJumpDistance(new Pair(row, col), from) && game.isValid(row, col, from))
                {
                    Color c = Color.FromArgb(70, 0, 0, 255);
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
            int row = (int)rect.GetValue(Grid.RowProperty) / 2;
            int col = (int)rect.GetValue(Grid.ColumnProperty) / 2;

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


        private void gamewindowMove(Pair to, Pair from)
        {
            GameBoard.IsEnabled = false;
            //(fetchPegImage(from)).StopAnimation();
            System.Windows.Controls.Panel.SetZIndex(fetchPegImage(from), 2 * (to.row + to.col));
            animatePeg(to, from);
            placedPeg = to;

            if (isDiskJumpDistance(to, from) && !isCorner(to, from))
            {
                if (checkPoint2_learnMove1 && checkPoint2_learnMove2 && !checkPoint3)
                {
                    if ((to.row+from.row)/2 == 1 && (to.col+from.col)/2 == 0)
                    {
                        disableNextButton.Stop();
                        removeDisk(1, 0);
                        progress++;
                        WriteNextInstruction();
                        nextInstruction_Timer.Start();
                    }
                }

                else if (checkPoint2_learnMove1 && !checkPoint2_learnMove2)
                {
                  //  TypewriteTextblock("You already know \nhow to slide engineers \nbetween the grass. \n\nDiscover the second \nway to move!", bigInstructions, TimeSpan.FromSeconds(3));
                }
                else if (!checkPoint2_learnMove1 && checkPoint2_learnMove2)
                {
                    //TypewriteTextblock("You already know \nhow to jump across \ngrass for tokens. \n\nDiscover the second \nway to move!", bigInstructions, TimeSpan.FromSeconds(3));
                    TypewriteTextblock("Try sliding between \nthe grass patches!", bigInstructions, TimeSpan.FromSeconds(2.5));
                    GameBoard.IsEnabled = true;
                }
                else if (!checkPoint2_learnMove2)
                {
                    GameBoard.IsEnabled = false;
                    disableGameBoard.Start();
                    checkPoint2_learnMove2 = true;
                    //TypewriteTextblock("(1/2) Jump over the grass \nto place a token there.          \n\nJump over an enemy token \nand replace it with \nyour own!", bigInstructions, TimeSpan.FromSeconds(3));
                    TypewriteTextblock("Good move!   \n\nPlacing tokens is your \npath to victory!  \n", bigInstructions, TimeSpan.FromSeconds(3));
                    nextInstruction_Timer.Start();
                    chatBox.Text += "   1. Jump over grass to place a token or replace an enemy token.\n";
                }

                Color c;
                SolidColorBrush mybrush;
                //byte alpha = (byte)(150 - 10 * game.diskBoard.movesFromWin(game.currentPlayer.color));

                //if (game.currentPlayer.color == Player.Color.White)
                {
                    //#620400FF
                    c = Color.FromArgb(40, 4, 0, 255);

                    mybrush = new SolidColorBrush(c);

                    //sets the color of all captured disk spaces to current "color from win"
                    /*for (int i = 0; i < DiskBoard.Children.Count; i++)
                    {
                        SolidColorBrush rectangleColor = (SolidColorBrush)DiskBoard.Children.Cast<Rectangle>().ElementAt(i).Fill;

                        if (rectangleColor != null && rectangleColor.Color == whiteColor.Color)
                            DiskBoard.Children.Cast<Rectangle>().ElementAt(i).Fill = mybrush;
                    }*/

                    //store the curent "color from win" color
                    //whiteColor.Color = c;

                    // Remove old disk (if any)
                    if (checkPoint4)
                        disksAdded.Clear();

                    disksAdded.Add(new Pair(7, 7));
                    bool diskExists = false;
                    foreach (Pair p in disksAdded)
                    {
                        if( p.row == (to.row + from.row) / 2 && p.col == (to.col + from.col) / 2)
                        {
                            // already exists
                            diskExists = true;
                            break;
                        }
                    }
                    if (!diskExists)
                    {
                        addDisk((to.row + from.row) / 2, (to.col + from.col) / 2);
                        disksAdded.Add(new Pair((to.row + from.row) / 2, (to.col + from.col) / 2));
                    }

                    /*if (DiskBoard.Children.Cast<Rectangle>().First(
                        f => Grid.GetRow(f) == (to.row + from.row)/2 && Grid.GetColumn(f) == (to.col + from.col)/2).Fill != mybrush)
                    {
                        removeDisk((to.row + from.row) / 2, (to.col + from.col) / 2);
                        addDisk((to.row + from.row) / 2, (to.col + from.col) / 2);
                    }*/

                    // Add disk
                    DiskBoard.Children.Cast<Rectangle>().First(
                        f => Grid.GetRow(f) == ((to.row + from.row) / 2) && Grid.GetColumn(f) == ((to.col + from.col) / 2)).Fill = mybrush; // If you remove currentDiskColor <- DELETE THE FUNCTION

                }
            }
            else if ((Math.Abs(from.row - to.row) == 2) ^ (Math.Abs(from.col - to.col) == 2))
            {
                // Capture peg
                removePeg((to.row + from.row) / 2, (to.col + from.col) / 2);

                if (!checkPoint4)
                {
                    checkPoint4 = true;
                    progress++;
                    WriteNextInstruction();
                    nextInstruction_Timer.Start();
                    GameBoard.IsEnabled = false;
                }
            }
            else
            {
                // "slide"
                if (!checkPoint2_learnMove1 && checkPoint2_learnMove2)
                {
                    checkPoint2_learnMove1 = true;
                    //TypewriteTextblock("(2/2) Your engineer can also \nslide between \ngrassy patches-      \nthis may be a good \ndefensive move!\n\nTry it now!", bigInstructions, TimeSpan.FromSeconds(3));
                    nextInstruction_Timer.Stop();
                    NextInstruction_Button.IsEnabled = false;
                    WriteNextInstruction();
                    chatBox.Text += "   2. Slide along lines.\n";
                    GameBoard.IsEnabled = true;
                }
                else if(!checkPoint2_learnMove1 && !checkPoint2_learnMove2)
                {
                    TypewriteTextblock("Try and jump \nacross the grass \nto place a token!", bigInstructions, TimeSpan.FromSeconds(2.3));
                    GameBoard.IsEnabled = true;
                }
            }
        }

        #endregion



        void upTrack1_Completed(object sender, EventArgs e)
        {
            progress++;
            WriteNextInstruction();
            nextInstruction_Timer.Start();

            
            Storyboard fadeMenu = this.Resources["ViewMenu"] as Storyboard;
            fadeMenu.Begin();
        }

        /*private void yes_Click(object sender, RoutedEventArgs e)
        {
            Storyboard fadeOut = this.Resources["FadeButtonsOut"] as Storyboard;
            fadeOut.Begin();

            yes.IsEnabled = false;
            no.IsEnabled = false;

            TypewriteTextblock("Great!!      \n\nLet's take a look!", bigInstructions, TimeSpan.FromSeconds(2));
            #region animate train & track
            TranslateTransform slideUp1 = new TranslateTransform();
            flatTrack.RenderTransform = slideUp1;
            DoubleAnimation upTrack1 = new DoubleAnimation(-900 - flatTrack.Height, TimeSpan.FromSeconds(5));
            upTrack1.Completed += new EventHandler(upTrack1_Completed);
            slideUp1.BeginAnimation(TranslateTransform.YProperty, upTrack1);

            TranslateTransform moveMiniTrain1 = new TranslateTransform();
            miniTrain.RenderTransform = moveMiniTrain1;
            DoubleAnimation upTrain1 = new DoubleAnimation(-950, TimeSpan.FromSeconds(5));
            DoubleAnimation rightTrain1 = new DoubleAnimation(4100, TimeSpan.FromSeconds(5));
            moveMiniTrain1.BeginAnimation(TranslateTransform.XProperty, rightTrain1);
            moveMiniTrain1.BeginAnimation(TranslateTransform.YProperty, upTrain1);


            // Next screen
            TranslateTransform movescreen = new TranslateTransform();
            MockMainMenu_Canvas.RenderTransform = movescreen;
            DoubleAnimation upmenu = new DoubleAnimation(950, 0, TimeSpan.FromSeconds(5));
            movescreen.BeginAnimation(TranslateTransform.YProperty, upmenu);
            MockMainMenu_Canvas.Visibility = System.Windows.Visibility.Visible;


            #endregion

        }

        private void no_Click(object sender, RoutedEventArgs e)
        {
            Storyboard fadeOut = this.Resources["FadeButtonsOut"] as Storyboard;
            fadeOut.Begin();

            yes.IsEnabled = false;
            no.IsEnabled = false;
            progress = 27;
            WriteNextInstruction();
            nextInstruction_Timer.Start();
        }*/

        private void NextInstruction_Button_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            //NextInstruction_Button.Source = new BitmapImage(new Uri("/Locomotion;component/Media/Graphics/arrowover.PNG", UriKind.Relative));
        }

        private void NextInstruction_Button_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            //NextInstruction_Button.Source = new BitmapImage(new Uri("/Locomotion;component/Media/Graphics/arrow.PNG", UriKind.Relative));
        }

        private void Help_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (this.Help.Opacity > .3)
            {
                universalToolTip.Content = "Try and move your middle engineer left!";
                this.Help.ToolTip = universalToolTip;
            }
        }

        private void chatBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            chatBox.ScrollToEnd();
        }

        private void MusicButton_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (SoundManager.isMuted == false)// if presently unmuted, mute
            {
                MapScreenMute();
            }
            else // unmute
            {
                MapScreenUnMute();
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

        private void ExitButton_MouseDown(object sender, MouseButtonEventArgs e)
        {
            ShowPauseMenu();
            if (!SoundManager.isMuted)
            {
                Storyboard tempSB = this.Resources["fadeVolumeDown"] as Storyboard;
                tempSB.Begin();
            }
        }

        private void ShowPauseMenu()
        {
            Storyboard sb = this.FindResource("PauseStoryboard") as Storyboard;
            sb.Begin();
        }

        private void ExitButton_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            ExitButton.Source = new BitmapImage(new Uri("/Locomotion;component/Media/Graphics/menuButtonHover.png", UriKind.Relative));
        }

        private void ExitButton_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            ExitButton.Source = new BitmapImage(new Uri("/Locomotion;component/Media/Graphics/menuButton.png", UriKind.Relative));
        }

        private void HelpButton_MouseDown(object sender, MouseButtonEventArgs e)
        {
            soundManager.playMenuButtonClickSound();

            if (HelpGrid.Visibility == Visibility.Hidden)
            {
                HelpGrid.Visibility = Visibility.Visible;
            }
            else
            {
                HelpGrid.Visibility = Visibility.Hidden;
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

        private void BTMButton_Click(object sender, RoutedEventArgs e)
        {
            if (SoundManager.isMuted == false)
            {
                Storyboard tempSB = this.Resources["fadeVolumeOff"] as Storyboard;
                tempSB.Begin();
            }
            soundManager.playMenuButtonClickSound();
        }

        private void RestartButton_Click(object sender, RoutedEventArgs e)
        {
            if (SoundManager.isMuted == false)
            {
                Storyboard tempSB = this.Resources["fadeVolumeOff"] as Storyboard;
                tempSB.Begin();
            }
            soundManager.playMenuButtonClickSound();
        }

        private void ResumeButton_Click(object sender, RoutedEventArgs e)
        {

            if (SoundManager.isMuted == false)
            {
                Storyboard tempSB = this.Resources["fadeVolumeUp"] as Storyboard;
                tempSB.Begin();
            }
            soundManager.playMenuButtonClickSound();
        }

        private void BTMStoryboard_Completed(object sender, EventArgs e)
        {
            MainMenu mainmenu = new MainMenu(this);
            App.Current.MainWindow = mainmenu;
            mainmenu.Show();
        }

        private void RestartStoryboard_Completed(object sender, EventArgs e)
        {
            Tutorial tut;
            tut = new Tutorial(this, gameOptions);

            App.Current.MainWindow = tut;
            tut.Show();
        }

        private void Storyboard_Completed(object sender, EventArgs e)
        {        
            MainMenu mainmenu = new MainMenu(this);
            App.Current.MainWindow = mainmenu;
            mainmenu.Show();
        }

        private void HelpGrid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            HelpGrid.Visibility = System.Windows.Visibility.Hidden;
        }
    }
}
