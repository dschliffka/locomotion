using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Locomotion
{
    public class TrainTree
    {
        public TrainTree(int r, int c, TrainTree prev)
        {
            _row = r;
            _col = c;
            _previous = prev;
        }

        private int _row;
        public int Row
        {
            get { return _row; }
        }

        private int _col;
        public int Col
        {
            get { return _col; }
        }

        private int _directionalIndex;
        public int DirectionalIndex
        {
            get { return _directionalIndex; }
            set { _directionalIndex = value; }
        }

        private TrainTree _previous;
        public TrainTree Previous
        {
            get { return _previous; }
            set { _previous = value; }
        }

        public static string getTrackSource(int s)
        {
            string source = "/Locomotion;component/Media/Graphics/add.png";
            switch (s)
            {
                case 4: // NW
                    source = "/Locomotion;component/Media/Graphics/NW.png";
                    break;
                case 8: // NE
                    source = "/Locomotion;component/Media/Graphics/NE.png";
                    break;
                case 18: // SW
                    source = "/Locomotion;component/Media/Graphics/SW.png";
                    break;
                case 22: // SE
                    source = "/Locomotion;component/Media/Graphics/SE.png";
                    break;
                case 10: // EW
                    source = "/Locomotion;component/Media/Graphics/EW.png";
                    break;
                case 16: // NS
                    source = "/Locomotion;component/Media/Graphics/NS.png";
                    break;
            }
            return source;
        }
    }
}
