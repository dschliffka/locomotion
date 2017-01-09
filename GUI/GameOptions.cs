using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Locomotion
{
    public class GameOptions
    {
        public GameOptions()
        { }

        public Game.TypeOfGame typeOfGame;
        public TreeNode.Difficulty difficulty;
        public string player1Name;
        public string player2Name;
        public int campaignLevel;
        public bool player1 = false;
        public bool startTutorial = false;
        public bool boyCharacter = true;
        public bool firstRun = true;
        public bool soundOn = true;
        public int stopWatchTime;
        public int level;
    }
}
