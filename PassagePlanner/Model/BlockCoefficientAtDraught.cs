using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace PassagePlanner
{
    public class BlockCoefficientAtDraught
    {
        private int _meanDraught;
        private double? _blockCoefficient;

        public BlockCoefficientAtDraught()
        {
            MeanDraught = 0;
            BlockCoefficient = null;
        }

        public BlockCoefficientAtDraught(int meanDraught, double? blockCoefficient)
        {
            MeanDraught = meanDraught;
            BlockCoefficient = blockCoefficient;
        }

        // STATIC dirty flag takes care of the case where ANY NavigationalBridgeWatchCondition is dirty.
        public static bool IsDirty { get; set; }

        public int MeanDraught
        {
            get
            {
                return _meanDraught;
            }
            set
            {
                if (_meanDraught == value)
                {
                    return;
                }

                _meanDraught = value;
                IsDirty = true;
            }
        }

        public double? BlockCoefficient
        {
            get
            {
                return _blockCoefficient;
            }
            set
            {
                if (_blockCoefficient == value)
                {
                    return;
                }

                _blockCoefficient = value;
                IsDirty = true;
            }
        }

    }
}
