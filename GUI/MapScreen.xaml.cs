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
using System.Windows.Media.Effects;

namespace Locomotion
{
    /// <summary>
    /// Interaction logic for MapScreen.xaml
    /// </summary>
    public partial class MapScreen : Window
    {
        Window prevWindow;
        Timer timer = new Timer();
        GameOptions gameOptions;
        int campaignProgress = 0;

        public MapScreen(Window w, GameOptions gameOptions)
        {
            prevWindow = w;
            this.gameOptions = gameOptions;

            timer.Interval = 500;
            timer.Start();
            timer.Tick += new EventHandler(timer_Tick);

            InitializeComponent();

            contentGrid.LayoutTransform = new ScaleTransform(System.Windows.SystemParameters.PrimaryScreenWidth / 1440,
                System.Windows.SystemParameters.PrimaryScreenHeight / 900,
                System.Windows.SystemParameters.PrimaryScreenWidth / 2,
                System.Windows.SystemParameters.PrimaryScreenHeight / 2);

            switch(ProfileManager.currentProfile.CampaignProgress)
            {
                case 0:
                    Level1Rect.IsEnabled = true;
                    break;
                case 1:
                    Track1.Visibility = System.Windows.Visibility.Visible;
                    Level1Stars.Source = new BitmapImage(new Uri("/Locomotion;component/Media/Graphics/star1full.png", UriKind.Relative));
                    Level1Rect.IsEnabled = true;
                    Level2Rect.IsEnabled = true;
                    Level2Rect.Opacity = 1;
                    break;
                case 2:
                    Track1.Visibility = System.Windows.Visibility.Visible;
                    Track2.Visibility = System.Windows.Visibility.Visible;
                    Level1Stars.Source = new BitmapImage(new Uri("/Locomotion;component/Media/Graphics/star1full.png", UriKind.Relative));
                    Level2Stars.Source = new BitmapImage(new Uri("/Locomotion;component/Media/Graphics/star2full.png", UriKind.Relative));
                    Level1Rect.IsEnabled = true;
                    Level2Rect.IsEnabled = true;
                    Level3Rect.IsEnabled = true;
                    Level2Rect.Opacity = 1;
                    Level3Rect.Opacity = 1;
                    break;
                case 3: // 3, 4 and otherwise
                    Track1.Visibility = System.Windows.Visibility.Visible;
                    Track2.Visibility = System.Windows.Visibility.Visible;
                    Track3.Visibility = System.Windows.Visibility.Visible;
                    Level1Stars.Source = new BitmapImage(new Uri("/Locomotion;component/Media/Graphics/star1full.png", UriKind.Relative));
                    Level2Stars.Source = new BitmapImage(new Uri("/Locomotion;component/Media/Graphics/star2full.png", UriKind.Relative));
                    Level3Stars.Source = new BitmapImage(new Uri("/Locomotion;component/Media/Graphics/star3full.png", UriKind.Relative));

                    Level1Rect.IsEnabled = true;
                    Level2Rect.IsEnabled = true;
                    Level3Rect.IsEnabled = true;
                    Level4Rect.IsEnabled = true;
                    Level2Rect.Opacity = 1;
                    Level3Rect.Opacity = 1;
                    Level4Rect.Opacity = 1;
                    break;
                default:
                    Track1.Visibility = System.Windows.Visibility.Visible;
                    Track2.Visibility = System.Windows.Visibility.Visible;
                    Track3.Visibility = System.Windows.Visibility.Visible;
                    Level1Stars.Source = new BitmapImage(new Uri("/Locomotion;component/Media/Graphics/star1full.png", UriKind.Relative));
                    Level2Stars.Source = new BitmapImage(new Uri("/Locomotion;component/Media/Graphics/star2full.png", UriKind.Relative));
                    Level3Stars.Source = new BitmapImage(new Uri("/Locomotion;component/Media/Graphics/star3full.png", UriKind.Relative));
                    Level4Stars.Source = new BitmapImage(new Uri("/Locomotion;component/Media/Graphics/star4full.png", UriKind.Relative));

                    Track4.Visibility = System.Windows.Visibility.Visible;
                    Level1Rect.IsEnabled = true;
                    Level2Rect.IsEnabled = true;
                    Level3Rect.IsEnabled = true;
                    Level4Rect.IsEnabled = true;
                    Level2Rect.Opacity = 1;
                    Level3Rect.Opacity = 1;
                    Level4Rect.Opacity = 1;
                    break;
            }

            if (SoundManager.isMuted == false)
            {
                MapScreenUnMute();
            }
            else
            {
                MapScreenMute();
            }

            gameOptions.typeOfGame = Game.TypeOfGame.Campaign;
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

        void timer_Tick(object sender, EventArgs e)
        {
            timer.Stop();
            prevWindow.Close();
        }

        private void level1StoryBoard_Completed(object sender, EventArgs e)
        {
            gameOptions.campaignLevel = 1;
            gameOptions.level = 1;
            LoadingScreen ls = new LoadingScreen(this, gameOptions);
            App.Current.MainWindow = ls;
            ls.Show();
        }

        private void level2StoryBoard_Completed(object sender, EventArgs e)
        {
            gameOptions.campaignLevel = 2;
            gameOptions.level = 2;
            LoadingScreen ls = new LoadingScreen(this, gameOptions);
            App.Current.MainWindow = ls;
            ls.Show();
        }

        private void level3StoryBoard_Completed(object sender, EventArgs e)
        {
            gameOptions.campaignLevel = 3;
            gameOptions.level = 3;
            LoadingScreen ls = new LoadingScreen(this, gameOptions);
            App.Current.MainWindow = ls;
            ls.Show();
        }

        private void level4StoryBoard_Completed(object sender, EventArgs e)
        {
            gameOptions.campaignLevel = 4;
            gameOptions.level = 4;
            LoadingScreen ls = new LoadingScreen(this, gameOptions);
            App.Current.MainWindow = ls;
            ls.Show();
        }

        private void backStoryboard_Completed(object sender, EventArgs e)
        {
            MainMenu mm = new MainMenu(this);
            App.Current.MainWindow = mm;
            mm.Show();
        }

        private void Level1Rect_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            DropShadowEffect temp = new DropShadowEffect();
            temp.Color = Colors.LightGoldenrodYellow;
            temp.BlurRadius = 15;
            temp.ShadowDepth = 0;
            Level1Rect.Effect = temp;
        }

        private void Level1Rect_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            Level1Rect.Effect = null;
        }

        private void Level2Rect_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (Level2Rect.IsEnabled)
            {
                DropShadowEffect temp = new DropShadowEffect();
                temp.Color = Colors.LightGoldenrodYellow;
                temp.BlurRadius = 15;
                temp.ShadowDepth = 0;
                Level2Rect.Effect = temp;
            }
        }

        private void Level2Rect_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            Level2Rect.Effect = null;
        }

        private void Level3Rect_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (Level3Rect.IsEnabled)
            {
                DropShadowEffect temp = new DropShadowEffect();
                temp.Color = Colors.LightGoldenrodYellow;
                temp.BlurRadius = 15;
                temp.ShadowDepth = 0;
                Level3Rect.Effect = temp;
            }
        }

        private void Level3Rect_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            Level3Rect.Effect = null;
        }

        private void Level4Rect_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (Level4Rect.IsEnabled)
            {
                DropShadowEffect temp = new DropShadowEffect();
                temp.Color = Colors.LightGoldenrodYellow;
                temp.BlurRadius = 15;
                temp.ShadowDepth = 0;
                Level4Rect.Effect = temp;
            }
        }

        private void Level4Rect_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            Level4Rect.Effect = null;
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

        private void Level1Rect_MouseDown(object sender, MouseButtonEventArgs e)
        {
            mapScreen.IsEnabled = false;
        }

        private void Level2Rect_MouseDown(object sender, MouseButtonEventArgs e)
        {
            mapScreen.IsEnabled = false;
        }

        private void Level3Rect_MouseDown(object sender, MouseButtonEventArgs e)
        {
            mapScreen.IsEnabled = false;
        }

        private void Level4Rect_MouseDown(object sender, MouseButtonEventArgs e)
        {
            mapScreen.IsEnabled = false;
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            mapScreen.IsEnabled = false;
            contentGrid.IsEnabled = false;
        }

    }
}

