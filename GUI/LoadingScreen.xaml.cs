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
using System.Windows.Shapes;

namespace Locomotion
{
    /// <summary>
    /// Interaction logic for LoadingScreen.xaml
    /// </summary>
    public partial class LoadingScreen : Window
    {
        Window prevWindow;
        Timer timer = new Timer();
        GameOptions gameOptions;
        string[] interestingFacts = new string[10];
        int[] factOffsets = new int[10];
        NetworkManager networkManager = NetworkManager.InstanceCreator();

        GameWindow gameWindow;
        Tutorial tut;
        bool loadComplete = false;
        string charSource;
        string robotSource;
        string robotName;

        public LoadingScreen(Window w, GameOptions gameOptions)
        {
            InitializeComponent();
            networkManager.allowKillNetworkThread = false;
            prevWindow = w;
            this.gameOptions = gameOptions;


            Random randFacts = new Random();
            int randomNumber = randFacts.Next(0, 10);

            interestingFacts[0] = "The word 'train' comes from the Old French trahiner.";
            interestingFacts[1] = "Monorails are trains with one rail.";
            interestingFacts[2] = "The first trains were pulled by horses.";
            interestingFacts[3] = "The world's fastest train goes 357 mph.";
            interestingFacts[4] = "The longest stretch of track without curve is 301 miles.";
            interestingFacts[5] = "40% of the world's freight cargo is carried on train.";
            interestingFacts[6] = "70% of trains in England start or end in London.";
            interestingFacts[7] = "The US embraced time zones only after train travel.";
            interestingFacts[8] = "The heaviest train ever recorded weighed 95,000 tons.";
            interestingFacts[9] = "James Watt first patented the steam engine.";

            factOffsets[0] = 0;
            factOffsets[1] = 0;// 130;
            factOffsets[2] = 0;// 120;
            factOffsets[3] = 0;// 110;
            factOffsets[4] = 0;// -10;
            factOffsets[5] = 0;
            factOffsets[6] = 0;
            factOffsets[7] = 0;
            factOffsets[8] = 0;
            factOffsets[9] = 0;// 80;

            FactLabel.Content = interestingFacts[randomNumber].ToString();
            FactLabel.Margin = new Thickness(factOffsets[randomNumber], 0, 0, 0);

            timer.Interval = 1000;
            timer.Start();
            timer.Tick += new EventHandler(timer_Tick);

            contentGrid.LayoutTransform = new ScaleTransform(System.Windows.SystemParameters.PrimaryScreenWidth / 1440,
                 System.Windows.SystemParameters.PrimaryScreenHeight / 900,
                 System.Windows.SystemParameters.PrimaryScreenWidth / 2,
                 System.Windows.SystemParameters.PrimaryScreenHeight / 2);

            if (gameOptions.level < 1 || gameOptions.level > 4)
                gameOptions.level = randFacts.Next(1, 5);

            if (gameOptions.typeOfGame == Game.TypeOfGame.Network && gameOptions.player1)
                gameOptions.level = ProfileManager.currentProfile.CharacterNumber;

            switch(gameOptions.level)
            {
                case 1:
                    robotSource = "/Locomotion;component/Media/Graphics/loadingRobot1.png";
                    robotName = "Gizmo";
                    break;
                case 2:
                    robotSource = "/Locomotion;component/Media/Graphics/loadingRobot2.png";
                    robotName = "Gizmo2";
                    break;
                case 3:
                    robotSource = "/Locomotion;component/Media/Graphics/loadingRoboCactus.png";
                    robotName = "Gizmo3";
                    break;
                case 4:
                    robotSource = "/Locomotion;component/Media/Graphics/robot4Corner.png";
                    robotName = "Gizmo4";
                    break;
            }

            switch(ProfileManager.currentProfile.CharacterNumber)
            {
                case 1:
                    charSource = "/Locomotion;component/Media/Graphics/loadingChar1.png";
                    break;
                case 2:
                    charSource = "/Locomotion;component/Media/Graphics/loadingChar2.png";
                    break;
                case 3:
                    charSource = "/Locomotion;component/Media/Graphics/loadingChar3.png";
                    break;
                case 4:
                    charSource = "/Locomotion;component/Media/Graphics/loadingChar4.png";
                    break;
            }

            if (gameOptions.typeOfGame == Game.TypeOfGame.Network)
            {
                switch (gameOptions.level)
                {
                    case 1:
                        if(!gameOptions.player1)
                        {
                            charSource = "/Locomotion;component/Media/Graphics/loadingRobot1.png";
                            robotSource = "/Locomotion;component/Media/Graphics/loadingChar1.png";
                        }
                        break;
                    case 2:
                        if (!gameOptions.player1)
                        {
                            charSource = "/Locomotion;component/Media/Graphics/loadingRobot2.png";
                            robotSource = "/Locomotion;component/Media/Graphics/loadingChar2.png";
                        }
                        break;
                    case 3:
                        if (!gameOptions.player1)
                        {
                            charSource = "/Locomotion;component/Media/Graphics/loadingRoboCactus.png";
                            robotSource = "/Locomotion;component/Media/Graphics/loadingChar3.png";
                        }
                        break;
                    case 4:
                        if (!gameOptions.player1)
                        {
                            charSource = "/Locomotion;component/Media/Graphics/robot4corner.png";
                            robotSource = "/Locomotion;component/Media/Graphics/loadingChar4.png";
                        }
                        break;
                }
            }



            if (gameOptions.startTutorial)
            {
                LeftCorner.Visibility = System.Windows.Visibility.Hidden;
                LeftCornerName.Visibility = System.Windows.Visibility.Hidden;
                RightCorner.Visibility = System.Windows.Visibility.Hidden;
                RightCornerName.Visibility = System.Windows.Visibility.Hidden;
                Versus.Visibility = System.Windows.Visibility.Hidden;
            }
            else if (gameOptions.typeOfGame == Game.TypeOfGame.Network)
            {
                if (gameOptions.player1) // I am player 1 (human)
                {
                    LeftCorner.Source = new BitmapImage(new Uri(charSource, UriKind.Relative));
                    LeftCornerName.Content = ProfileManager.currentProfile.ProfileName;

                    RightCorner.Source = new BitmapImage(new Uri(robotSource, UriKind.Relative));
                    RightCornerName.Content = networkManager.challengername;
                }
                else // I am player 2 (robot)
                {
                    RightCorner.Source = new BitmapImage(new Uri(charSource, UriKind.Relative));
                    RightCornerName.Content = ProfileManager.currentProfile.ProfileName;

                    LeftCorner.Source = new BitmapImage(new Uri(robotSource, UriKind.Relative));
                    LeftCornerName.Content = networkManager.challengername;
                }
            }
            else if (gameOptions.typeOfGame == Game.TypeOfGame.Local)
            {
                LeftCorner.Source = new BitmapImage(new Uri(charSource, UriKind.Relative));
                LeftCornerName.Content = gameOptions.player1Name;

                RightCorner.Source = new BitmapImage(new Uri(robotSource, UriKind.Relative));
                RightCornerName.Content = gameOptions.player2Name;
            }
            else if (gameOptions.typeOfGame == Game.TypeOfGame.Campaign)
            {
                LeftCorner.Source = new BitmapImage(new Uri(charSource, UriKind.Relative));
                LeftCornerName.Content = ProfileManager.currentProfile.ProfileName;

                RightCorner.Source = new BitmapImage(new Uri(robotSource, UriKind.Relative));
                RightCornerName.Content = robotName;
            }
            else
            {
                //if (gameOptions.player1)
                {
                    LeftCorner.Source = new BitmapImage(new Uri(charSource, UriKind.Relative));
                     LeftCornerName.Content = ProfileManager.currentProfile.ProfileName;

                    RightCorner.Source = new BitmapImage(new Uri(robotSource, UriKind.Relative));
                    RightCornerName.Content = robotName;
                }
                /*else
                {
                    RightCorner.Source = new BitmapImage(new Uri(charSource, UriKind.Relative));
                    RightCornerName.Content = ProfileManager.currentProfile.ProfileName;

                    LeftCorner.Source = new BitmapImage(new Uri(robotSource, UriKind.Relative));
                    LeftCornerName.Content = robotName;
                }*/
            }
        }

        void timer_Tick(object sender, EventArgs e)
        {
            timer.Stop();
            prevWindow.Close();
        }

        private void Storyboard_Completed(object sender, EventArgs e)
        {
            networkManager.allowKillNetworkThread = false;
            if (gameOptions.startTutorial)
            {
                tut = new Tutorial(this, gameOptions);
                App.Current.MainWindow = tut;
                tut.Show();
            }
            else
            {
                loadComplete = true;
                gameWindow = new GameWindow(this, gameOptions);
                App.Current.MainWindow = gameWindow;
                gameWindow.Show();
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!loadComplete)
                networkManager.peerDisconnect("");
        }
    }
}
