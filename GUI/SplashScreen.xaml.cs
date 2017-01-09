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
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace Locomotion
{
    /// <summary>
    /// Interaction logic for IntroScreen.xaml
    /// </summary>
    public partial class IntroScreen : Window
    {
        bool endedEarly = false;

        public IntroScreen()
        {
            InitializeComponent();

            contentGrid.LayoutTransform = new ScaleTransform(System.Windows.SystemParameters.PrimaryScreenWidth / 1440,
                System.Windows.SystemParameters.PrimaryScreenHeight / 900,
                System.Windows.SystemParameters.PrimaryScreenWidth / 2,
                System.Windows.SystemParameters.PrimaryScreenHeight / 2);
        }

        private void Storyboard_Completed(object sender, EventArgs e)
        {
            if (!endedEarly)
            {
                SplashScreen intro = new SplashScreen(this);
                App.Current.MainWindow = intro;
                intro.Show();
                //this.Close();
            }
        }

        private void SplashScreen_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {   
            SplashScreen intro = new SplashScreen(this);
            App.Current.MainWindow = intro;
            intro.Show();
            //this.Close();
            endedEarly = true;
        }

        private void contentGrid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            SplashScreen intro = new SplashScreen(this);
            App.Current.MainWindow = intro;
            intro.Show();
            //this.Close();
            endedEarly = true;
        }
    }
}
