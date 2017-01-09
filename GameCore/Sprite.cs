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
using System.ComponentModel;
using System.Runtime.InteropServices;

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
        private bool pauseAnim = false;
        private string sheetName;
        private Timer animationTimer = new Timer();

        public Sprite(string name, int width, int height, int numImages, int animSpeed)
        {
            spriteHeight = height;
            spriteWidth = width;
            numSheetImages = numImages;
            //this.Source = new BitmapImage(new Uri("/Locomotion;component/Media/Sprites/" + name + ".png", UriKind.Relative));

            animationTimer.Interval = animSpeed;
            animationTimer.Start();
            animationTimer.Tick += new EventHandler(animationTimer_Tick);
        }

        void animationTimer_Tick(object sender, EventArgs e)
        {
            if (!pauseAnim)
            {
                System.Drawing.Image temp = System.Drawing.Image.FromFile("D://Desktop/Creeper/Locomotion/Locomotion/Media/Sprites/sample.png");
                System.Drawing.Rectangle cropWindow = new System.Drawing.Rectangle(spriteWidth * currentFrame, 0, spriteWidth, spriteHeight);
                Bitmap spriteSheet = new Bitmap(temp);
                Bitmap croppedSheet = spriteSheet.Clone(cropWindow, spriteSheet.PixelFormat);

                if (currentFrame < numSheetImages)
                    currentFrame++;
                else
                    currentFrame = 0;

                this.Source = loadBitmap(croppedSheet);
            }
        }

        [DllImport("gdi32")]
        static extern int DeleteObject(IntPtr o);

        public static BitmapSource loadBitmap(System.Drawing.Bitmap source)
        {
            IntPtr ip = source.GetHbitmap();
            BitmapSource bs = null;
            try
            {
                bs = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(ip,
                   IntPtr.Zero, Int32Rect.Empty,
                   System.Windows.Media.Imaging.BitmapSizeOptions.FromEmptyOptions());
            }
            finally
            {
                DeleteObject(ip);
            }

            return bs;
        }

        public void StopAnimation()
        {
            pauseAnim = true;
        }

        public void StartAnimation()
        {
            pauseAnim = false;
        }

        public void ChangeAnimationSpeed(int speed)
        {
            animationTimer.Interval = speed;
        }
    }
}
