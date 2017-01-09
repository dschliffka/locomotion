using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Forms;
using Locomotion.Networking;
using Microsoft.VisualBasic;

namespace Locomotion
{
    /// <summary>
    /// Interaction logic for MainMenu.xaml
    /// </summary>
    public partial class MainMenu : Window
    {
        Window prevWindow;
        Timer closePrevWindow_timer = new Timer();
        Timer bounceTrain_timer = new Timer();
        Timer initialTrainMove = new Timer();
        Timer challengeuserresponse_timer = new Timer();
        Random random = new Random();
        NetworkManager networkManager = NetworkManager.InstanceCreator();
        ProfileManager profileManager = ProfileManager.InstanceCreator();
        SoundManager soundManager = SoundManager.InstanceCreator();
        List<String> availablePlayers = new List<String>();
        List<String> profiles = new List<String>();
        GameOptions gameOptions = new GameOptions(); // ELSEWHERE?
        int profileSelection = 1;
        bool editProfile = false;
        System.Windows.Controls.ToolTip universalToolTip = new System.Windows.Controls.ToolTip();


        public MainMenu(Window w)
        {
            prevWindow = w;
            InitializeComponent();
           
            closePrevWindow_timer.Interval = 1000;
            closePrevWindow_timer.Tick += new EventHandler(timer_Tick);
            closePrevWindow_timer.Start();

            bounceTrain_timer.Interval = 3100;
            bounceTrain_timer.Tick += new EventHandler(bounceTrain_timer_Tick);

            challengeuserresponse_timer.Interval = 10000;
            challengeuserresponse_timer.Tick += new EventHandler(challengeuserresponse_timer_tick);

            Color c = new Color();
            c = Color.FromArgb(240, 255, 255, 255);
            SolidColorBrush myBrush = new SolidColorBrush(c);

            universalToolTip.Background = myBrush;
            universalToolTip.BorderThickness = new Thickness(2);
            universalToolTip.BorderBrush = Brushes.White;
            universalToolTip.Foreground = Brushes.Black;
            
            
            contentGrid.LayoutTransform = new ScaleTransform(System.Windows.SystemParameters.PrimaryScreenWidth / 1440,
                System.Windows.SystemParameters.PrimaryScreenHeight / 900,
                System.Windows.SystemParameters.PrimaryScreenWidth / 2,
                System.Windows.SystemParameters.PrimaryScreenHeight / 2);

            networkManager.peerDisconnect("");

            profiles = profileManager.getNames();
            foreach (string s in profiles)
            {
                profile_Combobox.Items.Add(s);
            }

            #region scroll background and move train
            // move background
            TranslateTransform back = new TranslateTransform();
            landscape.RenderTransform = back;
            DoubleAnimation anim2 = new DoubleAnimation(-7041, TimeSpan.FromSeconds(60));
            anim2.RepeatBehavior = RepeatBehavior.Forever;
            back.BeginAnimation(TranslateTransform.XProperty, anim2);

            // move midground
            TranslateTransform mid = new TranslateTransform();
            midground.RenderTransform = mid;
            DoubleAnimation movemid = new DoubleAnimation(-11201, TimeSpan.FromSeconds(60));
            movemid.RepeatBehavior = RepeatBehavior.Forever;
            mid.BeginAnimation(TranslateTransform.XProperty, movemid);

            // move train
            TranslateTransform train = new TranslateTransform();
            MainMenuTrain.RenderTransform = train;
            DoubleAnimation moveTrainUp = new DoubleAnimation(-1800, 0, TimeSpan.FromSeconds(20));
            SmokeParticleSystem.RenderTransform = train;
            moveTrainUp.DecelerationRatio = 1;
            moveTrainUp.Completed += new EventHandler(moveTrainUp_Completed);
            train.BeginAnimation(TranslateTransform.XProperty, moveTrainUp);

            // move front layer (track + etc)
            TranslateTransform tracker = new TranslateTransform();
            FrontLayer.RenderTransform = tracker;
            DoubleAnimation trackerMove = new DoubleAnimation(-16800, TimeSpan.FromSeconds(60));
            trackerMove.RepeatBehavior = RepeatBehavior.Forever;
            tracker.BeginAnimation(TranslateTransform.XProperty, trackerMove);
            #endregion

            Storyboard sb = this.FindResource("LoadMenuButtons") as Storyboard;
            sb.Begin();

            SmokeParticleSystem.Start();

            //loginUsername.Content = "You are signed in as " + ProfileManager.getDefaultProfile();
            profile_Combobox.SelectedValue = ProfileManager.currentProfile.ProfileName;

            //if (!ProfileManager.currentProfile.ProfileName.Contains("Guest"))
            //{
                loginUsername.Content = ProfileManager.currentProfile.ProfileName;
                ProfileButton.Content = "Sign In";
           // }

            if (SoundManager.isMuted == true)
            {
                MainMenuTheme.IsMuted = true;
                soundManager.Mute();
                MusicButton.Source = new BitmapImage(new Uri("/Locomotion;component/Media/Graphics/musicOff.png", UriKind.Relative));
            }
            else
            {
                MainMenuTheme.IsMuted = false;
                soundManager.Unmute();
            }
        }

        void CheckSound()
        {
            if (SoundManager.isMuted == true)
            {
                MainMenuTheme.IsMuted = true;
                gameOptions.soundOn = false;
                soundManager.Mute();
            }
            else
            {
                gameOptions.soundOn = true;
                soundManager.Unmute();
            }
        }

        void bounceTrain_timer_Tick(object sender, EventArgs e)
        {
            TranslateTransform bounce = new TranslateTransform();
            MainMenuTrain.RenderTransform = bounce;
            DoubleAnimation anim = new DoubleAnimation(0,-random.Next(3), TimeSpan.FromMilliseconds(bounceTrain_timer.Interval/7));
            anim.AutoReverse = true;
            bounce.BeginAnimation(TranslateTransform.YProperty, anim);

            bounceTrain_timer.Interval = random.Next(500, 1200);
        }

        void timer_Tick(object sender, EventArgs e)
        {
            closePrevWindow_timer.Stop();
            prevWindow.Close();
        }

        void challengeuserresponse_timer_tick(object sender, EventArgs e)
        {
            //disconnect();
        }

        private void NewGameButton_Click(object sender, RoutedEventArgs e)
        {
            soundManager.playMenuButtonClickSound();
        }

        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            soundManager.playMenuButtonClickSound();

            Storyboard exit = this.Resources["ExitStoryboard"] as Storyboard;
            exit.Begin();
        }

        private void CampStoryBoard_Completed(object sender, EventArgs e)
        {
            gameOptions.firstRun = true;
            gameOptions.typeOfGame = Game.TypeOfGame.Campaign;
            CheckSound();
            MapScreen mapScreen = new MapScreen(this, gameOptions);
            App.Current.MainWindow = mapScreen;
            mapScreen.Show();
        }

        private void MultiplayerStoryBoard_Completed(object sender, EventArgs e)
        {

            multiplayerList.Items.Clear();

            foreach (string name in availablePlayers)
            {
                multiplayerList.Items.Add(name);
            }
        }

        private void myMainMenu_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            //Dont kill it when is a transition call
            //Kill it when is running on a window and it gets closed
            if (networkManager.allowKillNetworkThread)
                networkManager.peerDisconnect("");
        }

        private void TutorialStoryBoard_Completed(object sender, EventArgs e)
        {
            gameOptions = new GameOptions();
            gameOptions.startTutorial = true;
            CheckSound();
            LoadingScreen ls = new LoadingScreen(this, gameOptions);
            App.Current.MainWindow = ls;
            ls.Show();
        }

        private void challengeStatusReceived(object sender, ChallengeEventArgs challenge)
        {
            networkManager.getLocoPeer().NetWorker.allowDiscovery = false;
            networkManager.challengername = challenge.challengerName;
            gameOptions.level = challenge.characterNumber;
            this.Dispatcher.Invoke((Action)(() =>
            {
                //start timer here
                challengeuserresponse_timer.Start();

                Storyboard rejectBackButtonSB = (Storyboard)this.Resources["RejectBackButtonStoryboard"];
                rejectBackButtonSB.Begin();

                multiplayerList.Items.Clear();
                networkManager.peerDiscoverPeers();

                mpMessageLabel.Text = challenge.challengerName + " has challenged you to a game!";
                Storyboard sb = (Storyboard)this.Resources["ChallengeStoryboard"];
                sb.Begin();
            }));

            soundManager.playChallengeSound();

        }

        private void opponentDetected(object sender, OpponentEventArgs opponent)
        {
            this.Dispatcher.Invoke((Action)(() =>
            {
                if(!multiplayerList.Items.Contains(opponent.opponentName))
                    multiplayerList.Items.Add(opponent.opponentName);
            }));
        }

        private void connectionStatusReceived(object sender, ConnectionEventArgs status)
        {

            if (status.connected)
            {
                connect();
            }
            else
            {
                disconnect();
            }

           

            this.Dispatcher.Invoke((Action)(() =>
            {
                //Stop Timer HERE
                challengeuserresponse_timer.Stop();

                multiplayerList.Items.Clear();
                networkManager.peerDiscoverPeers();
                JoinButton.Content = "Connect";
                JoinButton.IsEnabled = true;
                RefreshButton.IsEnabled = true;
            }));
        }

        private void connect()
        {
            challengeuserresponse_timer.Stop();
            soundManager.playMenuButtonClickSound();
            networkManager.allowKillNetworkThread = false;
            this.Dispatcher.Invoke((Action)(() =>
            {
                CheckSound();


                if (SoundManager.isMuted == false)
                {
                    Storyboard tempSB = this.Resources["fadeVolumeOff"] as Storyboard;
                    tempSB.Begin();
                }

                LoadingScreen ls = new LoadingScreen(this, gameOptions);
                App.Current.MainWindow = ls;
                ls.Show();
            }));
            //networkManager.getLocoPeer().NetWorker.goFirst = false;
            networkManager.getLocoPeer().NetWorker.connectionStatusReceived -= this.connectionStatusReceived;
        }

        private void disconnect()
        {
            this.Dispatcher.Invoke((Action)(() =>
            {
                challengeuserresponse_timer.Stop();

                mpRejectLabel.Text = "Your challenge has been rejected!";
                if (mpChallengeCanvas.IsVisible)
                {
                    object obj = new Object();
                    RoutedEventArgs e = new RoutedEventArgs();
                    mpRejectLabel.Text = "Oops! Please try again!";
                    challengeRejectButton_Click(obj, e);
                }
                soundManager.playMenuButtonClickSound();

                Storyboard sb = (Storyboard)this.Resources["RejectStoryboard"];
                sb.Begin();
            }));
            gameOptions.player1 = false;
            //networkManager.getLocoPeer().NetWorker.goFirst = false;
            networkManager.getLocoPeer().NetWorker.allowDiscovery = true;
        }


        private void JoinButton_Click(object sender, RoutedEventArgs e)
        {
            //RefreshButton_Click(sender, e);
            soundManager.playMenuButtonClickSound();
            string selectedopponent = "";
            if (multiplayerList.SelectedIndex >= 0)
            {
                selectedopponent = multiplayerList.SelectedValue.ToString();
                if (selectedopponent != "")
                {

                    foreach (string element in multiplayerList.Items)
                    {
                        if (element == selectedopponent)
                        {
                            //networkManager.allowKillNetworkThread = false;
                            networkManager.getLocoPeer().NetWorker.allowDiscovery = false;
                            JoinButton.Content = "Waiting...";
                            JoinButton.IsEnabled = false;
                            RefreshButton.IsEnabled = false;

                            networkManager.getLocoPeer().NetWorker.goFirst = true;
                            networkManager.peerConnect(selectedopponent, ProfileManager.currentProfile.CharacterNumber);
                            gameOptions.player1 = true;
                        }
                    }
                }
            }
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            soundManager.playMenuButtonClickSound();
            availablePlayers = networkManager.peerDiscoverPeers();
            multiplayerList.Items.Clear();

            //foreach (string name in availablePlayers)
            //{
            //    multiplayerList.Items.Add(name);
            //}
        }

        private void NetworkButton_Click(object sender, RoutedEventArgs e)
        {
            soundManager.playMenuButtonClickSound();
            string username = networkManager.createPeer(ProfileManager.currentProfile.ProfileName);
            networkManager.getLocoPeer().NetWorker.challengeReceived += new ChallengeEventHandler(this.challengeStatusReceived);
            networkManager.getLocoPeer().NetWorker.connectionStatusReceived += new ConnectionEventHandler(this.connectionStatusReceived);
            networkManager.getLocoPeer().NetWorker.opponentDetected += new OpponentEventHandler(this.opponentDetected);
            gameOptions.typeOfGame = Game.TypeOfGame.Network;
            gameOptions.firstRun = true;
            //multiplayerList.Items.Clear();

            availablePlayers = networkManager.peerDiscoverPeers();

            //foreach (string name in availablePlayers)
            //{
            //    multiplayerList.Items.Add(name);
            //}
        }

        /*private void LocalButtonClick_Completed(object sender, EventArgs e)
        {
            // old form??
            gameOptions.typeOfGame = Game.TypeOfGame.Local;

            LocalForm playerForm = new LocalForm();
            playerForm.ShowDialog();

            gameOptions.player1Name = playerForm.player1Name;
            gameOptions.player2Name = playerForm.player2Name;

            LoadingScreen ls = new LoadingScreen(this, gameOptions);
            App.Current.MainWindow = ls;
            ls.Show();
        }*/

        private void localPlayerOkButton_Click(object sender, RoutedEventArgs e)
        {
            soundManager.playMenuButtonClickSound();
            gameOptions.firstRun = true;
            gameOptions.typeOfGame = Game.TypeOfGame.Local;

            if (player1Input.Text.Trim() == "" )
                player1Input.Text = "Player 1";
            if (player2Input.Text.Trim() == "" )
                player2Input.Text = "Player 2";

            gameOptions.player1Name = player1Input.Text;
            gameOptions.player2Name = player2Input.Text;
            CheckSound();
            LoadingScreen ls = new LoadingScreen(this, gameOptions);
            App.Current.MainWindow = ls;
            ls.Show();
        }

        private void LocalButton_Click(object sender, RoutedEventArgs e)
        {
            soundManager.playMenuButtonClickSound();
            player1Input.Text = ProfileManager.currentProfile.ProfileName;
            player2Input.Text = "Guest";
            /*if(player1Input.Text.Trim() == "" || player2Input.Text.Trim() == "")
            {
                if (player1Input.Text.Trim() == "")
                    player1Input.Text = "Player1";
                if (player2Input.Text.Trim() == "")
                    player2Input.Text = "Player2";
            }*/
        }

        private void challengeAcceptButton_Click(object sender, RoutedEventArgs e)
        {
            networkManager.getLocoPeer().NetWorker.allowDiscovery = true;
            soundManager.playMenuButtonClickSound();
            networkManager.acceptChallenge();
            gameOptions.player1 = false;
        }

        private void challengeRejectButton_Click(object sender, RoutedEventArgs e)
        {
            networkManager.getLocoPeer().NetWorker.allowDiscovery = true;
            soundManager.playMenuButtonClickSound();
            networkManager.denyChallenge();
            Storyboard sb = (Storyboard)this.Resources["RejectChallengeStoryboard"];
            sb.Begin();
        }

        #region PROFILES

        private void ProfileButton_Click(object sender, RoutedEventArgs e)
        {
            if (ProfileButton.Content.ToString() == "Sign In")
            {
                profile_Combobox.IsEnabled = true;
                startButton.IsEnabled = true;
                profileBackButton.IsEnabled = true;

                Storyboard sb = this.FindResource("InitialLoad") as Storyboard;
                sb.Begin();
            }
            else
            {
                ProfileButton.Content = "Sign In"; 
                profile_Combobox.SelectedItem = ProfileManager.getDefaultProfile();
                ProfileManager.resetCurrentProfile();
                loginUsername.Content = ProfileManager.getDefaultProfile();
            }

            soundManager.playMenuButtonClickSound();
        }

        private void Storyboard_Completed(object sender, EventArgs e)
        {
            loginUsername.Content = ProfileManager.getDefaultProfile();

            Storyboard temp = this.FindResource("LogoutStoryboard2") as Storyboard;
            temp.Begin();
        }

        private void Storyboard_Completed_1(object sender, EventArgs e)
        {
           // loginInfoGrid.IsEnabled = true;
        }

        private void loginInfoGrid_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            loginBackground.Source = new BitmapImage(new Uri("/Locomotion;component/Media/Graphics/profileCorner.png", UriKind.Relative));

            //SolidColorBrush myBrush = new SolidColorBrush();
            //myBrush.Color = Color.FromArgb(126, 0, 0, 0);
            //loginInfoBorder.Background = myBrush; //#7E000000
            //loginInfoBorder.BorderBrush = myBrush; //#7E000000
        }

        private void profile_Combobox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string name = profile_Combobox.SelectedValue.ToString();
            ProfilePic.Source = new BitmapImage(new Uri(profileManager.getImageSource(name), UriKind.Relative));
            lifetimeWinCount.Content = profileManager.getLifetimeWins(name);

            if (profile_Combobox.SelectedIndex == 0)
            {
                editCurrent.Opacity = .35;
                deleteCurrent.Opacity = .35;
            }
            else
            {
                addNewProfileButton.Opacity = 1;
                editCurrent.Opacity = 1;
                deleteCurrent.Opacity = 1;
            }

            profileSelection = profileManager.getCharacterNumber(name);
            switch (profileManager.getCampaignProgress(name))
            {
                case 1:
                    campaignStars.Source = new BitmapImage(new Uri("/Locomotion;component/Media/Graphics/star1.png", UriKind.Relative));
                    break;
                case 2:
                    campaignStars.Source = new BitmapImage(new Uri("/Locomotion;component/Media/Graphics/star2.png", UriKind.Relative));
                    break;
                case 3:
                    campaignStars.Source = new BitmapImage(new Uri("/Locomotion;component/Media/Graphics/star3.png", UriKind.Relative));
                    break;
                case 4:
                    campaignStars.Source = new BitmapImage(new Uri("/Locomotion;component/Media/Graphics/star4.png", UriKind.Relative));
                    break;
                default:
                    campaignStars.Source = new BitmapImage(new Uri("/Locomotion;component/Media/Graphics/star0.png", UriKind.Relative));
                    break;
            }
        }

        private void addNewProfileButton_MouseDown(object sender, RoutedEventArgs e)
        {
            ChooseTicket_Grid.IsEnabled = false;
            SelectedProfilePic.Source = new BitmapImage(new Uri("/Locomotion;component/Media/Graphics/char1circle.png", UriKind.Relative));
            profileSelection = 1;
            NewProfile_TextBox.Text = "New Player";
            NewProfile_TextBox.Focus();
            NewProfile_TextBox.SelectAll();

            TranslateTransform moveleft = new TranslateTransform();
            ChooseTicket_Grid.RenderTransform = moveleft;
            DoubleAnimation anim1 = new DoubleAnimation(-900, TimeSpan.FromSeconds(1));
            anim1.AccelerationRatio = .2;
            anim1.DecelerationRatio = .2;
            anim1.Completed += new EventHandler(anim1_Completed);
            moveleft.BeginAnimation(TranslateTransform.XProperty, anim1);
        }

        private void CreateProfile_Button_Click(object sender, RoutedEventArgs e)
        {
            soundManager.playMenuButtonClickSound();
            ProfileEditError_Label.Content = "";
            if (NewProfile_TextBox.Text.Trim() == "")
            {
                ProfileEditError_Label.Content = "Please enter a name";
                NewProfile_TextBox.Focus();
                NewProfile_TextBox.SelectAll();
            }
            else if (editProfile)
            {
                if (NewProfile_TextBox.Text.Trim() == profile_Combobox.SelectedItem.ToString() || !profileManager.nameExists(NewProfile_TextBox.Text.Trim()))
                {
                    ProfileManager.editProfile(profile_Combobox.SelectedItem.ToString(), NewProfile_TextBox.Text.Trim(), profileSelection);
                    if (editProfile) editProfile = false;

                    profiles = profileManager.getNames();
                    profile_Combobox.SelectionChanged -= profile_Combobox_SelectionChanged;
                    profile_Combobox.Items.Clear();
                    foreach (string s in profiles)
                    {
                        profile_Combobox.Items.Add(s);
                    }

                    profile_Combobox.SelectionChanged += profile_Combobox_SelectionChanged;
                    profile_Combobox.SelectedItem = NewProfile_TextBox.Text.Trim();

                    NewTicket_Grid.IsEnabled = false;
                    TranslateTransform moveRight = new TranslateTransform();
                    NewTicket_Grid.RenderTransform = moveRight;
                    DoubleAnimation anim2 = new DoubleAnimation(900, TimeSpan.FromSeconds(1));
                    anim2.AccelerationRatio = .2;
                    anim2.DecelerationRatio = .2;
                    anim2.Completed += new EventHandler(anim2_Completed);
                    moveRight.BeginAnimation(TranslateTransform.XProperty, anim2);
                }
                else
                {
                    ProfileEditError_Label.Content = "Sorry! This name already exists!";
                    NewProfile_TextBox.Focus();
                    NewProfile_TextBox.SelectAll();
                }
            }
            else if (!profileManager.nameExists(NewProfile_TextBox.Text.Trim()))
            {
                profileManager.addNewProfile(NewProfile_TextBox.Text.Trim(), profileSelection); // 1 for first picture

                ProfileManager.setCurrentProfile(NewProfile_TextBox.Text.Trim());
                profile_Combobox.Items.Add(NewProfile_TextBox.Text.Trim());
                profile_Combobox.SelectedItem = NewProfile_TextBox.Text.Trim();

                NewTicket_Grid.IsEnabled = false;
                TranslateTransform moveRight = new TranslateTransform();
                NewTicket_Grid.RenderTransform = moveRight;
                DoubleAnimation anim2 = new DoubleAnimation(900, TimeSpan.FromSeconds(1));
                anim2.AccelerationRatio = .2;
                anim2.DecelerationRatio = .2;
                anim2.Completed += new EventHandler(anim2_Completed);
                moveRight.BeginAnimation(TranslateTransform.XProperty, anim2);
            }
            else
            {
                ProfileEditError_Label.Content = "Sorry! This name already exists!";
                NewProfile_TextBox.Focus();
                NewProfile_TextBox.SelectAll();
            }
        }

        void anim1_Completed(object sender, EventArgs e)
        {
            System.Windows.Controls.Panel.SetZIndex(ChooseTicket_Grid, 10);
            System.Windows.Controls.Panel.SetZIndex(NewTicket_Grid, 11);
            NewTicket_Grid.IsEnabled = true;
            addNewProfileButton.Opacity = .35;
            editCurrent.Opacity = .35;
            deleteCurrent.Opacity = .35;

            TranslateTransform moveright = new TranslateTransform();
            ChooseTicket_Grid.RenderTransform = moveright;
            DoubleAnimation anim1 = new DoubleAnimation(-900,0, TimeSpan.FromSeconds(1));
            anim1.AccelerationRatio = .2;
            anim1.DecelerationRatio = .2;
            moveright.BeginAnimation(TranslateTransform.XProperty, anim1);
        }

        private void cancelCreateNew_Button_Click(object sender, RoutedEventArgs e)
        {
            NewTicket_Grid.IsEnabled = false;
            ProfileEditError_Label.Content = "";
            ProfilePic.Source = new BitmapImage(new Uri(profileManager.getImageSource(profile_Combobox.SelectedValue.ToString()), UriKind.Relative));
            profileSelection = profileManager.getCharacterNumber(profile_Combobox.SelectedValue.ToString());

            TranslateTransform moveRight = new TranslateTransform();
            NewTicket_Grid.RenderTransform = moveRight;
            DoubleAnimation anim2 = new DoubleAnimation(900, TimeSpan.FromSeconds(1));
            anim2.AccelerationRatio = .2;
            anim2.DecelerationRatio = .2;
            anim2.Completed += new EventHandler(anim2_Completed);
            moveRight.BeginAnimation(TranslateTransform.XProperty, anim2);

            if (editProfile) editProfile = false;
        }

        void anim2_Completed(object sender, EventArgs e)
        {
            System.Windows.Controls.Panel.SetZIndex(ChooseTicket_Grid, 11);
            System.Windows.Controls.Panel.SetZIndex(NewTicket_Grid, 10);
            ChooseTicket_Grid.IsEnabled = true;
            addNewProfileButton.Opacity = 1;
            if (profile_Combobox.SelectedIndex != 0)
            {
                editCurrent.Opacity = 1;
                deleteCurrent.Opacity = 1;
            }


            TranslateTransform moveleft = new TranslateTransform();
            NewTicket_Grid.RenderTransform = moveleft;
            DoubleAnimation anim2 = new DoubleAnimation(900, 0, TimeSpan.FromSeconds(1));
            anim2.AccelerationRatio = .2;
            anim2.DecelerationRatio = .2;
            moveleft.BeginAnimation(TranslateTransform.XProperty, anim2);
        }

        private void startButton_Click(object sender, RoutedEventArgs e)
        {
            soundManager.playMenuButtonClickSound();

            ProfileManager.setCurrentProfile(profile_Combobox.SelectedValue.ToString());

            startButton.IsEnabled = false;
            profileBackButton.IsEnabled = false;
            profile_Combobox.IsEnabled = false;

            Storyboard sb = this.FindResource("LoadMenuButtons") as Storyboard;
            sb.Begin();

            if (profile_Combobox.SelectedIndex != 0)
            {
                loginUsername.Content = profile_Combobox.SelectedValue.ToString();
                ProfileButton.Content = "Sign Out";
            }
            else
            {
                loginUsername.Content = profile_Combobox.SelectedValue.ToString();
                ProfileButton.Content = "Sign In";
            }
        }

        private void editCurrent_MouseDown(object sender, RoutedEventArgs e)
        {
            if (profile_Combobox.SelectedItem.ToString() != ProfileManager.getDefaultProfile() )
            {
                ChooseTicket_Grid.IsEnabled = false;
                profileSelection = profileManager.getCharacterNumber(profile_Combobox.SelectedItem.ToString());

                SelectedProfilePic.Source = new BitmapImage(new Uri(profileManager.getImageSource(profile_Combobox.SelectedItem.ToString()), UriKind.Relative));


                NewProfile_TextBox.Text = profile_Combobox.SelectedValue.ToString();
                NewProfile_TextBox.Focus();
                NewProfile_TextBox.SelectAll();


                TranslateTransform moveleft = new TranslateTransform();
                ChooseTicket_Grid.RenderTransform = moveleft;
                DoubleAnimation anim1 = new DoubleAnimation(-900, TimeSpan.FromSeconds(1));
                anim1.AccelerationRatio = .2;
                anim1.DecelerationRatio = .2;
                anim1.Completed += new EventHandler(anim1_Completed);
                moveleft.BeginAnimation(TranslateTransform.XProperty, anim1);

                editProfile = true;
            }
            else
            {
                // error: cannot edit guest profile
            }
        }

        private void deleteCurrent_MouseDown(object sender, RoutedEventArgs e)
        {
            if (profile_Combobox.SelectedItem.ToString() != ProfileManager.getDefaultProfile())
            {
                ProfileManager.deleteProfile(profile_Combobox.SelectedItem.ToString());

                profiles = profileManager.getNames();
                profile_Combobox.SelectionChanged -= profile_Combobox_SelectionChanged;
                profile_Combobox.Items.Clear();
                foreach (string s in profiles)
                {
                    profile_Combobox.Items.Add(s);
                }

                profile_Combobox.SelectionChanged += profile_Combobox_SelectionChanged;
                profile_Combobox.SelectedIndex = 0;
            }
            else
            {
                // error: cannot delete guest profile
            }
        }

        private void NewProfile_TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            ProfileEditError_Label.Content = "";
        }

        private void addNewProfileButton_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            universalToolTip.Content = "Create new profile";
            this.addNewProfileButton.ToolTip = universalToolTip;
        }

        private void editCurrent_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            universalToolTip.Content = "Edit selected profile";
            this.editCurrent.ToolTip = universalToolTip;
        }

        private void deleteCurrent_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            universalToolTip.Content = "Delete selected profile";
            this.deleteCurrent.ToolTip = universalToolTip;
        }

        private void LeftArrow_MouseDown(object sender, MouseButtonEventArgs e)
        {
            soundManager.playMenuButtonClickSound();
            if (profileSelection == 1)
                profileSelection = 4;
            else
                profileSelection--;


            SelectedProfilePic.Source = new BitmapImage(new Uri(profileManager.getImageSource(profileSelection), UriKind.Relative));
        }

        private void RightArrow_MouseDown(object sender, MouseButtonEventArgs e)
        {
            soundManager.playMenuButtonClickSound();
            if (profileSelection == 4)
                profileSelection = 1;
            else
                profileSelection++;


            SelectedProfilePic.Source = new BitmapImage(new Uri(profileManager.getImageSource(profileSelection), UriKind.Relative));
        }

        #endregion


        void moveTrainUp_Completed(object sender, EventArgs e)
        {
            bounceTrain_timer.Start();
        }

        private void qgRegularStoryBoard_Completed(object sender, EventArgs e)
        {
           /* if ((bool)easyRadioButton.IsChecked)
                gameOptions.difficulty = TreeNode.Difficulty.Easy;
            else if ((bool)mediumRadioButton.IsChecked)
                gameOptions.difficulty = TreeNode.Difficulty.Medium;
            else if ((bool)hardRadioButton.IsChecked)
                gameOptions.difficulty = TreeNode.Difficulty.Hard; */

            gameOptions.player1 = true;
            // else player 2?????????????
            gameOptions.level = 0;

            gameOptions.typeOfGame = Game.TypeOfGame.QuickMatch;
            gameOptions.firstRun = true;
            CheckSound();
            LoadingScreen ls = new LoadingScreen(this, gameOptions);
            App.Current.MainWindow = ls;
            ls.Show();
        }

        private void qgTimeAttackStoryBoard_Completed(object sender, EventArgs e)
        {
         /*   if ((bool)easyRadioButton.IsChecked)
            {
                gameOptions.difficulty = TreeNode.Difficulty.Easy;
                gameOptions.stopWatchTime = 4;
            }
            else if ((bool)mediumRadioButton.IsChecked)
            {
                gameOptions.difficulty = TreeNode.Difficulty.Medium;
                gameOptions.stopWatchTime = 3;
            }
            else if ((bool)hardRadioButton.IsChecked)
            {
                gameOptions.difficulty = TreeNode.Difficulty.Hard;
                gameOptions.stopWatchTime = 2;
            } */

            gameOptions.firstRun = true;
            CheckSound();
            gameOptions.typeOfGame = Game.TypeOfGame.TimeAttack;
            LoadingScreen ls = new LoadingScreen(this, gameOptions);
            App.Current.MainWindow = ls;
            ls.Show();
        }

        private void mainMenuButtons_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {

        }

        private void CreditsButton_Click(object sender, RoutedEventArgs e)
        {
            soundManager.playMenuButtonClickSound();
        }

        private void multiplayerBackButton_Click(object sender, RoutedEventArgs e)
        {
            soundManager.playMenuButtonClickSound();
            
            
        }

        private void TutorialButton_Click(object sender, RoutedEventArgs e)
        {
            soundManager.playMenuButtonClickSound();
            myMainMenu.IsEnabled = false;
        }

        private void CampaignButton_Click(object sender, RoutedEventArgs e)
        {
            soundManager.playMenuButtonClickSound();
        }

        private void QuickMatchButton_Click(object sender, RoutedEventArgs e)
        {
            soundManager.playMenuButtonClickSound();

            gameOptions.level = 0;
            gameOptions.player1 = true;
            gameOptions.difficulty = TreeNode.Difficulty.Easy;
            gameOptions.typeOfGame = Game.TypeOfGame.QuickMatch;
        }

        private void MultiplayerButton_Click(object sender, RoutedEventArgs e)
        {
            soundManager.playMenuButtonClickSound();
        }

        private void newGameBackButton_Click(object sender, RoutedEventArgs e)
        {
            soundManager.playMenuButtonClickSound();
        }

        private void MultiplayerCanvasBackButton_Click(object sender, RoutedEventArgs e)
        {
            soundManager.playMenuButtonClickSound();
            networkManager.peerDisconnect("Sorry, your opponent left the game");
            multiplayerList.Items.Clear();
            RefreshButton.IsEnabled = true;
            JoinButton.IsEnabled = true;
            JoinButton.Content = "Connect";
        }

        private void rejectBackButton_Click(object sender, RoutedEventArgs e)
        {
            soundManager.playMenuButtonClickSound();
        }

        private void localPlayersCancelButton_Click(object sender, RoutedEventArgs e)
        {
            soundManager.playMenuButtonClickSound();
        }

        private void qgRegularButton_Click(object sender, RoutedEventArgs e)
        {
            soundManager.playMenuButtonClickSound();
        }

        private void qgTimeAttackButton_Click(object sender, RoutedEventArgs e)
        {
            soundManager.playMenuButtonClickSound();
        }

        private void qgBackButton_Click(object sender, RoutedEventArgs e)
        {
            soundManager.playMenuButtonClickSound();
        }

        private void qgTimeAttackButton_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            universalToolTip.Content = "Beat the clock! With increasing difficulty, you will have less " + (Environment.NewLine) + " time to make all of your moves plus a tougher opponent!";
           // this.qgTimeAttackButton.ToolTip = universalToolTip;
        }

        private void CampaignButton_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            universalToolTip.Content = "Play through the LocoMotion story mode.";
            this.CampaignButton.ToolTip = universalToolTip;
        }

        private void QuickMatchButton_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            universalToolTip.Content = "Choose a difficulty and play a regular match, or time attack mode";
            this.QuickMatchButton.ToolTip = universalToolTip;
        }

        private void TutorialButton_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            universalToolTip.Content = "Learn how to play LocoMotion!";
            this.TutorialButton.ToolTip = universalToolTip;
        }

        private void LocalButton_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            universalToolTip.Content = "Play against a friend on the same computer.";
            this.LocalButton.ToolTip = universalToolTip;
        }

        private void NetworkButton_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            universalToolTip.Content = "Play against opponents online!";
            this.NetworkButton.ToolTip = universalToolTip;
        }

        private void MusicButton_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (SoundManager.isMuted == false)// if presently unmuted, mute
            {
                MusicButton.Source = new BitmapImage(new Uri("/Locomotion;component/Media/Graphics/musicOff.png", UriKind.Relative));
                MainMenuTheme.Volume = 0.0;
                MainMenuTheme.IsMuted = true;
                soundManager.Mute();
                gameOptions.soundOn = false;
            }
            else // unmute
            {
                MusicButton.Source = new BitmapImage(new Uri("/Locomotion;component/Media/Graphics/music.png", UriKind.Relative));
                MainMenuTheme.Volume = 0.5;
                MainMenuTheme.IsMuted = false;
                soundManager.Unmute();
                gameOptions.soundOn = true;
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

        private void HelpButton_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            HelpButton.Source = new BitmapImage(new Uri("/Locomotion;component/Media/Graphics/helpButtonHover.png", UriKind.Relative));
        }

        private void HelpButton_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            HelpButton.Source = new BitmapImage(new Uri("/Locomotion;component/Media/Graphics/helpButton.png", UriKind.Relative));
        }

        private void HelpButton_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (HelpGrid.Visibility == Visibility.Hidden)
            {

                if (SoundManager.isMuted == false)
                {
                    Storyboard tempSB = this.Resources["fadeVolumeDown"] as Storyboard;
                    tempSB.Begin();
                }

                HelpGrid.Visibility = Visibility.Visible;

                if (mainMenuButtons.Visibility == Visibility.Visible)
                    HelpImage1.Visibility = Visibility.Visible;
                else if (quickGameGrid.Visibility == Visibility.Visible)
                    HelpImage5.Visibility = Visibility.Visible;
                else if (newGameButtons.Visibility == Visibility.Visible)
                    HelpImage2.Visibility = Visibility.Visible;
                else if (multiplayerOptionButtons.Visibility == Visibility.Visible)
                    HelpImage3.Visibility = Visibility.Visible;
                else if (mpPeerCanvas.Visibility == Visibility.Visible)
                    HelpImage4.Visibility = Visibility.Visible;
                
            }
            else
            {
                if (SoundManager.isMuted == false)
                {
                    Storyboard tempSB = this.Resources["fadeVolumeUp"] as Storyboard;
                    tempSB.Begin();
                }

                HelpGrid.Visibility = Visibility.Hidden;
                HelpImage1.Visibility = Visibility.Hidden;
                HelpImage2.Visibility = Visibility.Hidden;
                HelpImage3.Visibility = Visibility.Hidden;
                HelpImage4.Visibility = Visibility.Hidden;
                HelpImage5.Visibility = Visibility.Hidden;
            }
            

        }

        private void HelpGrid_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            HelpGrid.Visibility = Visibility.Hidden;
            HelpImage1.Visibility = Visibility.Hidden;
            HelpImage2.Visibility = Visibility.Hidden;
            HelpImage3.Visibility = Visibility.Hidden;
            HelpImage4.Visibility = Visibility.Hidden;
            HelpImage5.Visibility = Visibility.Hidden;
        }

        private void HelpGrid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            HelpGrid.Visibility = Visibility.Hidden;
            HelpImage1.Visibility = Visibility.Hidden;
            HelpImage2.Visibility = Visibility.Hidden;
            HelpImage3.Visibility = Visibility.Hidden;
            HelpImage4.Visibility = Visibility.Hidden;
            HelpImage5.Visibility = Visibility.Hidden;
        }

        private void DiffLeftArrow_MouseDown(object sender, MouseButtonEventArgs e)
        {
            soundManager.playMenuButtonClickSound();

            if (DiffOptionsLabel.Content.ToString() == "Easy")
            {
                gameOptions.difficulty = TreeNode.Difficulty.Hard;
                DiffOptionsLabel.Content = "Hard";
            }
            else if (DiffOptionsLabel.Content.ToString() == "Hard")
            {
                gameOptions.difficulty = TreeNode.Difficulty.Medium;
                DiffOptionsLabel.Content = "Medium";
            }
            else
            {
                gameOptions.difficulty = TreeNode.Difficulty.Easy;
                DiffOptionsLabel.Content = "Easy";
            }
        }

        private void DiffRightArrow_MouseDown(object sender, MouseButtonEventArgs e)
        {
            soundManager.playMenuButtonClickSound();

            if (DiffOptionsLabel.Content.ToString() == "Easy")
            {
                gameOptions.difficulty = TreeNode.Difficulty.Medium;
                DiffOptionsLabel.Content = "Medium";
            }
            else if (DiffOptionsLabel.Content.ToString() == "Medium")
            {
                gameOptions.difficulty = TreeNode.Difficulty.Hard;
                DiffOptionsLabel.Content = "Hard";
            }
            else
            {
                gameOptions.difficulty = TreeNode.Difficulty.Easy;
                DiffOptionsLabel.Content = "Easy";
            }
        }

        private void MoveLeftArrow_MouseDown(object sender, MouseButtonEventArgs e)
        {
            soundManager.playMenuButtonClickSound();

            if (MoveOptionsLabel.Content.ToString() == "First")
            {
                gameOptions.player1 = false;
                MoveOptionsLabel.Content = "Second";
            }
            else
            {
                gameOptions.player1 = true;
                MoveOptionsLabel.Content = "First";
            }
        }

        private void MoveRightArrow_MouseDown(object sender, MouseButtonEventArgs e)
        {
            soundManager.playMenuButtonClickSound();

            if (MoveOptionsLabel.Content.ToString() == "First")
            {
                gameOptions.player1 = false;
                MoveOptionsLabel.Content = "Second";
            }
            else
            {
                gameOptions.player1 = true;
                MoveOptionsLabel.Content = "First";
            }
        }

        private void MapLeftArrow_MouseDown(object sender, MouseButtonEventArgs e)
        {
            soundManager.playMenuButtonClickSound();

            if (MapOptionsLabel.Content.ToString() == "Random")
            {
                gameOptions.level = 4;
                MapOptionsLabel.Content = "Volcano";
            }
            else if (MapOptionsLabel.Content.ToString() == "Volcano")
            {
                gameOptions.level = 3;
                MapOptionsLabel.Content = "Sand";
            }
            else if (MapOptionsLabel.Content.ToString() == "Sand")
            {
                gameOptions.level = 2;
                MapOptionsLabel.Content = "Snow";
            }
            else if (MapOptionsLabel.Content.ToString() == "Snow")
            {
                gameOptions.level = 1;
                MapOptionsLabel.Content = "Grass";
            }
            else
            {
                gameOptions.level = 0;
                MapOptionsLabel.Content = "Random";
            }
        }

        private void MapRightArrow_MouseDown(object sender, MouseButtonEventArgs e)
        {
            soundManager.playMenuButtonClickSound();

            if (MapOptionsLabel.Content.ToString() == "Random")
            {
                gameOptions.level = 1;
                MapOptionsLabel.Content = "Grass";
            }
            else if (MapOptionsLabel.Content.ToString() == "Grass")
            {
                gameOptions.level = 2;
                MapOptionsLabel.Content = "Snow";
            }
            else if (MapOptionsLabel.Content.ToString() == "Snow")
            {
                gameOptions.level = 3;
                MapOptionsLabel.Content = "Sand";
            }
            else if (MapOptionsLabel.Content.ToString() == "Sand")
            {
                gameOptions.level = 4;
                MapOptionsLabel.Content = "Volcano";
            }
            else
            {
                gameOptions.level = 0;
                MapOptionsLabel.Content = "Random";
            }

        }

        private void qgStartButton_Click(object sender, RoutedEventArgs e)
        {
            if (taSlider.Value == 0)
                gameOptions.typeOfGame = Game.TypeOfGame.QuickMatch;
            else if (taSlider.Value <= 2)
            {
                gameOptions.typeOfGame = Game.TypeOfGame.TimeAttack;
                gameOptions.stopWatchTime = 1;
            }
            else if (taSlider.Value <= 4)
            {
                gameOptions.typeOfGame = Game.TypeOfGame.TimeAttack;
                gameOptions.stopWatchTime = 2;
            }
            else if (taSlider.Value <= 6)
            {
                gameOptions.typeOfGame = Game.TypeOfGame.TimeAttack;
                gameOptions.stopWatchTime = 4;
            }

            Storyboard sb = this.Resources["qgFadeout"] as Storyboard;
            sb.Begin();

            LoadingScreen ls = new LoadingScreen(this, gameOptions);
            App.Current.MainWindow = ls;
            ls.Show();
        }

        private void loginBackground_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            loginBackground.Source = new BitmapImage(new Uri("/Locomotion;component/Media/Graphics/profileCornerHover.png", UriKind.Relative));
        }

        private void loginBackground_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            loginBackground.Source = new BitmapImage(new Uri("/Locomotion;component/Media/Graphics/profileCorner.png", UriKind.Relative));
        }

        private void loginBackground_MouseDown(object sender, MouseButtonEventArgs e)
        {
            profile_Combobox.SelectedItem = ProfileManager.currentProfile.ProfileName;

            startButton.IsEnabled = true;
            profileBackButton.IsEnabled = true;
            profile_Combobox.IsEnabled = true;
            lifetimeWinCount.Content = profileManager.getLifetimeWins(ProfileManager.currentProfile.ProfileName);

            switch (ProfileManager.currentProfile.CampaignProgress)
            {
                case 1:
                    campaignStars.Source = new BitmapImage(new Uri("/Locomotion;component/Media/Graphics/star1.png", UriKind.Relative));
                    break;
                case 2:
                    campaignStars.Source = new BitmapImage(new Uri("/Locomotion;component/Media/Graphics/star2.png", UriKind.Relative));
                    break;
                case 3:
                    campaignStars.Source = new BitmapImage(new Uri("/Locomotion;component/Media/Graphics/star3.png", UriKind.Relative));
                    break;
                case 4:
                    campaignStars.Source = new BitmapImage(new Uri("/Locomotion;component/Media/Graphics/star4.png", UriKind.Relative));
                    break;
                default:
                    campaignStars.Source = new BitmapImage(new Uri("/Locomotion;component/Media/Graphics/star0.png", UriKind.Relative));
                    break;
            }

            ProfilePic.Source = new BitmapImage(new Uri(profileManager.getImageSource(ProfileManager.currentProfile.ProfileName), UriKind.Relative));

            Storyboard sb = this.FindResource("InitialLoad") as Storyboard;
            sb.Begin();
            //redrawBackground_timer.Start();
        }

        private void Storyboard_Completed_2(object sender, EventArgs e)
        {
            this.Close();
        }

        private void CreditCanvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            CreditsStoryBoard.Stop();
            CreditCanvas.Visibility = Visibility.Hidden;
        }
    }
}

