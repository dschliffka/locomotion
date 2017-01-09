using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
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
using System.Diagnostics;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows.Interop;
using System.Windows.Navigation;

namespace Locomotion
{
    public class Sprite : System.Windows.Controls.Image
    {
        private int sheetHeight;
        private int sheetWidth;
        private int spriteHeight;
        private int spriteWidth;
        private int numSheetImages;
        private int currentFrame = 0;
        //private bool pauseAnim = false;
        private bool firstTime = true;
        private string sheetName;
        private Timer animationTimer = new Timer();
        private CroppedBitmap[] croppedImages;
        private bool animateOnceHide = false;
        private bool animateOnce = false;


        public Sprite(string name, int width, int height, int numImages, int animSpeed)
        {
            spriteHeight = height;
            spriteWidth = width;
            numSheetImages = numImages;
            sheetName = name;
            //this.Source = new BitmapImage(new Uri("/Locomotion;component/Media/Sprites/" + name + ".png", UriKind.Relative));
            Debug.Write("Sprite created.\n");         


            if (numImages > 1)
            {
                BitmapImage spriteSheet = new BitmapImage(new Uri("/Locomotion;component/Media/Sprites/" + name + ".png", UriKind.Relative));
                spriteSheet.BaseUri = BaseUriHelper.GetBaseUri(this);

                croppedImages = new CroppedBitmap[numImages];

                for (int i = 0; i < numImages; i++)
                {
                    System.Windows.Int32Rect cropWindow = new System.Windows.Int32Rect(spriteWidth * i, 0, spriteWidth, spriteHeight);
                    croppedImages[i] = new CroppedBitmap(spriteSheet, cropWindow);
                }

                this.Source = croppedImages[0];
                currentFrame++;

                animationTimer.Interval = animSpeed;
                animationTimer.Tick += new EventHandler(animationTimer_Tick);

                animationTimer.Start();
            }
            else
            {
                this.Source = new BitmapImage(new Uri("/Locomotion;component/Media/Graphics/" + name + ".png", UriKind.Relative));
                this.StopAnimation();
            }
        }

        public Sprite()
        {

        }

        void animationTimer_Tick(object sender, EventArgs e)
        {
            if (currentFrame < numSheetImages - 1)
                currentFrame++;
            else if (animateOnceHide)
            {
                animationTimer.Stop();
                animateOnceHide = false;
                this.Visibility = System.Windows.Visibility.Hidden;
                currentFrame = 0;
            }
            else if (animateOnce)
            {
                animationTimer.Stop();
                animateOnce = false;
                currentFrame = 0;
            }
            else
                currentFrame = 0;

            this.Source = croppedImages[currentFrame];
        }


        public void StopAnimation()
        {
            animationTimer.Stop();
        }

        public void StartAnimation()
        {
            animationTimer.Start();
        }

        public void AnimateOnce()
        {
            animationTimer.Start();
            animateOnce = true;
        }

        public void AnimateOnceHide()
        {
            animationTimer.Start();
            animateOnceHide = true;
        }


        public void ChangeAnimationSpeed(int speed)
        {
            animationTimer.Interval = speed;
        }

        private void ResetAnimation()
        {
            currentFrame = 0;
        }
    }
}
