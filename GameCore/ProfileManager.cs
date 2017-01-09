using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Locomotion
{
    public sealed class ProfileManager
    {
        private static List<Profile> profiles;
        public static Profile currentProfile;
        Random random = new Random();

        public List<string> getNames()
        {
            List<string> names = new List<string>();
            foreach (Profile p in profiles)
            {
                names.Add(p.ProfileName);
            }

            return names;
        }

        public bool nameExists(string name)
        {
            List<string> names = getNames();
            foreach (string n in names)
            {
                if (n == name)
                    return true;
            }
            return false;
        }

        #region singleton design pattern

        private ProfileManager()
        {
            profiles = new List<Profile>();
        }

        private static ProfileManager instance;

        public static ProfileManager InstanceCreator()
        {
            if (instance == null)
                instance = new ProfileManager();

            return instance;
        }

        #endregion

        #region read/write

        public void parseProfiles()
        {
            addProfile(new Profile("Guest " + random.Next(10) + random.Next(10) + random.Next(10), 0, 0, 1));

            if (!System.IO.Directory.Exists("C:\\Users\\Public\\Documents\\LOCOmotion"))
            {
                // create dir
                System.IO.Directory.CreateDirectory("C:\\Users\\Public\\Documents\\LOCOmotion");
            }

            if (File.Exists("C:\\Users\\Public\\Documents\\LOCOmotion\\profile.loco"))
            {
                string[] profileParser = File.ReadAllLines("C:\\Users\\Public\\Documents\\LOCOmotion\\profile.loco");
                foreach (string line in profileParser)
                {
                    if (line != "")
                    {
                        try
                        {
                            string name = line.Substring(0, line.Length - 5);
                            int campaignProgress = Convert.ToInt32(line.Substring(line.Length - 4, 1));
                            int lifetimeWins = Convert.ToInt32(line.Substring(line.Length - 3, 3));
                            int source = Convert.ToInt32(line.Substring(line.Length - 5, 1));

                            addProfile(new Profile(name, campaignProgress, lifetimeWins, source));
                        }
                        catch { }
                    }
                }
            }

            currentProfile = profiles.ElementAt(0);
        }

        public void writeToFile()
        {
            string[] profileParser = new string[profiles.Count-1];
            for(int i = 1; i < profiles.Count; i++)
            {
                string wins;
                switch(profiles.ElementAt(i).LifetimeWins.ToString().Length)
                {
                    case 1:
                        wins = "00" + profiles.ElementAt(i).LifetimeWins;
                        break;
                    case 2:
                        wins = "0" + profiles.ElementAt(i).LifetimeWins;
                        break;
                    default:
                        wins = profiles.ElementAt(i).LifetimeWins.ToString();
                        break;
                }
                profileParser[i-1] = profiles.ElementAt(i).ProfileName + profiles.ElementAt(i).CharacterNumber + profiles.ElementAt(i).CampaignProgress + wins;
            }


            if (!System.IO.Directory.Exists("C:\\Users\\Public\\Documents\\LOCOmotion"))
            {
                // create dir
                System.IO.Directory.CreateDirectory("C:\\Users\\Public\\Documents\\LOCOmotion");
            }


            try
            {
                System.IO.File.WriteAllLines("C:\\Users\\Public\\Documents\\LOCOmotion\\profile.loco", profileParser);
            }
            catch { }
        }

        #endregion

        #region getters and incrementers

        public int getCampaignProgress(string name)
        {
            int cp = 0;
            if (name.Trim() != "")
            {

            foreach (Profile p in profiles)
            {
                if (p.ProfileName == name)
                {
                    cp = p.CampaignProgress;
                }
            }
                }
            return cp;
        }

        public int getLifetimeWins(string name)
        {
            int lw = 0;

            if (name.Trim() != "")
            {
                foreach (Profile p in profiles)
                {
                    if (p.ProfileName == name)
                    {
                        lw = p.LifetimeWins;
                    }
                }
            }
            return lw;
        }

        public string getImageSource(string name)
        {
            string source = "/Locomotion;component/Media/Graphics/char1circle.png";

            if (name.Trim() != "")
            {
                foreach (Profile p in profiles)
                {
                    if (p.ProfileName == name)
                    {
                        source = getImageSource(p.CharacterNumber);
                    }
                }
            }
            return source;
        }

        public int getCharacterNumber(string name)
        {
            int number = 1;
            foreach (Profile p in profiles)
            {
                if (p.ProfileName == name)
                {
                    number = p.CharacterNumber;
                }
            }
            return number;
        }

        public string getImageSource(int s)
        {
            string source;
            switch (s)
            {
                case 1:
                    source = "/Locomotion;component/Media/Graphics/char1circle.png";
                    break;
                case 2:
                    source = "/Locomotion;component/Media/Graphics/char2circle.png";
                    break;
                case 3:
                    source = "/Locomotion;component/Media/Graphics/char3circle.png";
                    break;
                case 4:
                    source = "/Locomotion;component/Media/Graphics/char4circle.png";
                    break;
                default:
                    source = "/Locomotion;component/Media/Graphics/char1circle.png";
                    break;
            }

            return source;
        }

        public void addNewProfile(string name, int source)
        {
            Profile p = new Profile(name, 0, 0, source);
            addProfile(p);
        }

        private void addProfile(Profile profile)
        {
            profiles.Add(profile);
        }


        public static void incrementLifetimeWins(string name)
        {
            foreach (Profile p in profiles)
            {
                if (p.ProfileName == name)
                {
                    p.incrementWin();
                }
            }
        }

        public static void incrementCampainProgress(string name, int level)
        {
            foreach (Profile p in profiles)
            {
                if (p.ProfileName == name)
                    if (level > p.CampaignProgress)
                        p.incrementCampaignProgress();
            }
        }

        #endregion

        #region current profile

        static public void setCurrentProfile(string name)
        {
            currentProfile = profiles.ElementAt(0);
            foreach (Profile p in profiles)
            {
                if (p.ProfileName == name)
                {
                    currentProfile = p;
                }
            }
        }

        static public void resetCurrentProfile()
        {
            currentProfile = profiles.ElementAt(0);
        }

        static public string getDefaultProfile()
        {
            return profiles.ElementAt(0).ProfileName;
        }

        #endregion

        #region edit and delete

        public static void deleteProfile(string name)
        {
            foreach (Profile p in profiles)
            {
                if (p.ProfileName == name)
                {
                    profiles.Remove(p);
                    break;
                }
            }
        }

        public static void editProfile(string oldName, string newName, int profilePic)
        {
            foreach (Profile p in profiles)
            {
                if (p.ProfileName == oldName)
                {
                    p.ProfileName = newName;
                    p.CharacterNumber = profilePic;
                }
            }
        }

        #endregion



        /// <summary> Profile
        /// Private nested class to encapsulate 
        /// attributes of a profile
        /// </summary>
        public class Profile
        {
            private string _profileName;
            private int _campaignProgress;
            private int _lifetimeWins;
            private int _characterNumber;

            public Profile(string p, int cp, int lw, int cnum)
            {
                this._profileName = p;
                this._campaignProgress = cp;
                this._lifetimeWins = lw;
                this._characterNumber = cnum;
            }

            public string ProfileName
            {
                get { return this._profileName; }
                set { this._profileName = value; }
            }

            public int CampaignProgress
            {
                get { return this._campaignProgress; }
            }

            public int LifetimeWins
            {
                get { return this._lifetimeWins; }
            }

            public int CharacterNumber
            {
                get { return this._characterNumber; }
                set { this._characterNumber = value; }
            }

            public void incrementWin()
            {
                _lifetimeWins++;
            }

            public void incrementCampaignProgress()
            {
                _campaignProgress++;
            }
        }
    }
}
