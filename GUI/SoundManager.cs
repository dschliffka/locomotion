using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Media;
using System.Windows.Media;

namespace Locomotion
{
    /*
     Sound Manager Class:
     * In order to add a sound, add a sound to the soundEffectDictionary in the constructor, with a unique integer for a key
     * and the name of the file+".wav" for a value. Add all sound files to Media/Sounds in Locomotion project, and leave no
     * spaces in sound file name to maintain consistency. Also, select soundfile in solution explorer, and in the properties
     * set BuildAction to "Content" and Copy to Output directory to "Copy always".
     * 
     *  - Optional: Create a new function call to access your specific soundfile (as the dictionary is private, we can only access
     *  music from the sound manager functions). Also helpful to determine specific situation sound will be used in in function
     *  call, to help others maintain consistency in sound management.
     * */

    public sealed class SoundManager
    {
        private static Dictionary<int, string> soundEffectDictionary;
        public static bool isMuted = false;


        private SoundManager()
        {
            soundEffectDictionary = new Dictionary<int, string>();

            string directory = "Media/Sounds/";
            soundEffectDictionary.Add(1, directory + "ButtonPush.wav");
            soundEffectDictionary.Add(2, directory + "Pop1.wav");
            soundEffectDictionary.Add(3, directory + "Pop2.wav");
            soundEffectDictionary.Add(4, directory + "Tick.wav");
            soundEffectDictionary.Add(5, directory + "BossTheme.wav");
            soundEffectDictionary.Add(6, directory + "MainMenuTheme.wav");
            soundEffectDictionary.Add(7, directory + "GamePlayTheme.wav");
            soundEffectDictionary.Add(8, directory + "record-scratch-1.wav");
            soundEffectDictionary.Add(9, directory + "squeeze-toy-3.wav");
            soundEffectDictionary.Add(10, directory + "boing.wav");
            soundEffectDictionary.Add(11, directory + "constructionsoundeffects.wav");
            soundEffectDictionary.Add(12, directory + "TrainHorn.wav");
            soundEffectDictionary.Add(13, directory + "capture.wav");
            soundEffectDictionary.Add(14, directory + "tick1.wav");
            soundEffectDictionary.Add(15, directory + "tock2.wav");
            soundEffectDictionary.Add(16, directory + "timeout.wav");
            soundEffectDictionary.Add(17, directory + "notimetick.wav");
            soundEffectDictionary.Add(18, directory + "silence.wav");
            soundEffectDictionary.Add(19, directory + "MapTheme.wav");
        }

        private static SoundManager instance;

        public static SoundManager InstanceCreator()
        {
            if (instance == null)
                instance = new SoundManager();

            return instance;
        }

        public void Mute()
        {
            isMuted = true;
        }

        public void Unmute()
        {
            isMuted = false;
        }

        private void playSoundEffect(int soundEffectNum)
        {
            if (soundEffectNum > 0 && soundEffectDictionary.Keys.Contains(soundEffectNum) && !isMuted)
            {
                SoundPlayer player = new SoundPlayer();
                player.SoundLocation = soundEffectDictionary[soundEffectNum];
                player.Load();

                player.Play();
            }
        }

        #region "GUI/SoundEffect component"
        public void playMenuButtonClickSound()
        {
            playSoundEffect(4);
        }

        public void playSelectPieceSound()
        {
            playSoundEffect(3);
        }

        public void playPlacePieceSound()
        {
            playSoundEffect(2);
        }

        public void playMainMenuThemePieceSound()
        {
            //playSoundEffect(6);
        }

        public void playGamePlayTheme()
        {
            playSoundEffect(7);
        }

        public void playRecordScratchSound()
        {
            playSoundEffect(8);
        }

        public void playSelectEnemyPieceSound()
        {
            playSoundEffect(13);
        }

        public void playChallengeSound()
        {
            playSoundEffect(10);
        }

        public void playIncomingMessageSound()
        {
            playSoundEffect(10);
        }

        public void playBuildingRailRoadSound()
        {
            playSoundEffect(11);
        }

        public void playTrainHornSound()
        {
            playSoundEffect(12);
        }

        public void playTick()
        {
            playSoundEffect(14);
        }

        public void playTock()
        {
            playSoundEffect(15);
        }

        public void playTimeOut()
        {
            playSoundEffect(16);
        }

        public void playNoTime()
        {
            playSoundEffect(17);
        }

        public void playsilence()
        {
            playSoundEffect(18);
        }

        public void playTutorialTheme()
        {
            playSoundEffect(19);
        }

        #endregion


    }
}
