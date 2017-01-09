using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Locomotion
{
    public class Player
    {
        /// <summary>
        /// Enum: Player.Color
        /// </summary>
        public enum Color { White, Black, Nil };
        public enum TypeOfPlayer { Human, AI, Network };


        /// <summary>
        /// Default Constructor
        /// (+1) overload
        /// </summary>
        public Player()
        {
            // for inheritance
        }
        public Player(string name, Color color, TypeOfPlayer playerType)
        {
            this._name = name;
            this._color = color;
            this._playerType = playerType;
        }
        public Player(string name, int cP)
        {
            this._name = name;
            this._campaignProgress = cP;
            this._playerType = TypeOfPlayer.Human;
            this._color = Color.White;
        }

        /// <summary>
        /// Class member variables
        /// </summary>
        private string _name;
        private Color _color;
        private TypeOfPlayer _playerType;
        private int _campaignProgress;

        public Player player
        {
            get { return this; }
        }
        public Color color
        {
            get { return _color; }
            protected set { this._color = value; }
        }

        public string name
        {
            get { return _name; }
            protected set { this._name = value; }
        }

        public TypeOfPlayer playerType
        {
            get { return _playerType; }
            protected set { this._playerType = value; } 
        }

        public int campaignProgress
        {
            get { return _campaignProgress; }
            protected set { this._campaignProgress = value; }
        }

        public void setColor(Color color)
        {
            this._color = color;
        }

        public PieceMove getAIMove(Player currentPlayer, PegBoard pegBoard, DiskBoard diskBoard, int treeDepth)
        {
            PieceMove move = null;
            TreeNode root = new TreeNode(currentPlayer, pegBoard, diskBoard, TreeNode.MiniMax.Min, treeDepth);
            int selectedMove = root.AlphaBeta(root, int.MinValue, int.MaxValue);

            foreach(TreeNode node in root.children)
            {
                if (node.heuristicValue < selectedMove)
                {
                    selectedMove = node.heuristicValue;
                    move = node.moveMade;
                }
            }
            if (move == null)
            {
                move = root.children.ElementAt(0).moveMade;
            }

            return move;
        }

        public void beatCampaign(int c)
        {
            if (_campaignProgress < c)
                _campaignProgress = c;
        }
    }
}
