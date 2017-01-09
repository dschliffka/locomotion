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
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Locomotion
{
    /// <summary>
    /// Interaction logic for SplashScreen.xaml
    /// </summary>
    public partial class SplashScreen : Window
    {
        bool endedEarly = false;
        Window prevWindow;
        Timer timer = new Timer();

        public SplashScreen(Window w)
        {
            InitializeComponent();
            prevWindow = w;

            timer.Interval = 200;
            timer.Start();
            timer.Tick += new EventHandler(timer_Tick);

            contentGrid.LayoutTransform = new ScaleTransform(System.Windows.SystemParameters.PrimaryScreenWidth / 1440,
                System.Windows.SystemParameters.PrimaryScreenHeight / 900,
                System.Windows.SystemParameters.PrimaryScreenWidth / 2,
                System.Windows.SystemParameters.PrimaryScreenHeight / 2);
        }

        void timer_Tick(object sender, EventArgs e)
        {
            timer.Stop();
            prevWindow.Close();
        }

        private void Storyboard_Completed(object sender, EventArgs e)
        {
            if (!endedEarly)
            {
                MainMenu mainmenu = new MainMenu(this);
                App.Current.MainWindow = mainmenu;
                mainmenu.Show();
            }
        }

        private void IntroScreen_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            Skip();
        }

        private void contentGrid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Skip();
        }

        private void Skip()
        {
            Storyboard fadeOut = FindResource("FadeOut") as Storyboard;
            fadeOut.Begin();

            MainMenu mainmenu = new MainMenu(this);
            mainmenu.Show();
            App.Current.MainWindow = mainmenu;
            endedEarly = true;
        }
    }
}
