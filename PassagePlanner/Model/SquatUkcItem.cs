using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Seaware.Navigation;
//using EcdisLayer;

namespace PassagePlanner
{
    public class SquatUkcItem : INotifyPropertyChanged, ICloneable
    {
        private double _speed;
        private double? _waterDepth;
        private double _vesselBeam;
        private double _channelBeam;
        private bool _showSquatEvenAtGreatDepths = false;
        private bool _considerSquatEqualsNullAsZero = false;

        private double? _squatOpenWater;
        private double? _squatChannel;
        private double _draughtCorrection;
        private double _depthCorrection;
        private double _draughtAndDepthCorrection;
        private double? _deepestDraughtOpenWater;
        private double? _deepestDraughtChannel;
        private double? _ukcOpenWater;
        private double? _ukcChannel;
        private double? _deepestDraughtOpenWaterCorrected;
        private double? _deepestDraughtChannelCorrected;
        private double? _ukcOpenWaterCorrected;
        private double? _ukcChannelCorrected;

        private double _blockCoefficient;
        private double _maxDraughtVessel;
        private double _meanDraughtVessel;

        public event PropertyChangedEventHandler PropertyChanged;


        public SquatUkcItem(double speed, double? waterDepth, double vesselBeam)
        {
            _speed = speed;
            _waterDepth = waterDepth;
            _vesselBeam = vesselBeam;
        }

        public SquatUkcItem(double speed, 
            double waterDepth, 
            double vesselBeam, 
            double blockCoefficient, 
            double maxDraughtVessel, 
            double meanDraughtVessel,
            bool considerSquatEqualsNullAsZero)
        {
            _speed = speed;
            _waterDepth = waterDepth;
            _vesselBeam = vesselBeam;
            _blockCoefficient = blockCoefficient;
            _maxDraughtVessel = maxDraughtVessel;
            _meanDraughtVessel = meanDraughtVessel;
            _considerSquatEqualsNullAsZero = considerSquatEqualsNullAsZero;
            Calculate();
        }

        public double Speed
        {
            get
            {
                return _speed;
            }
            set
            {
                if (_speed == value)
                {
                    return;
                }

                _speed = value;
                if (_speed > 0.0)
                {
                    Calculate();
                }
                OnPropertyChanged("Speed");
            }
        }

        public double? WaterDepth
        {
            get
            {
                return _waterDepth;
            }
            set
            {
                if (_waterDepth == value)
                {
                    return;
                }

                _waterDepth = value;

                Calculate();
     
                OnPropertyChanged("WaterDepth");
            }
        }

        public double VesselBeam
        {
            get
            {
                return _vesselBeam;
            }
            set
            {
                if (_vesselBeam == value)
                {
                    return;
                }

                _vesselBeam = value;
                OnPropertyChanged("VesselBeam");
            }
        }

        public double ChannelBeam
        {
            get
            {
                return _channelBeam;
            }
            set
            {
                if (_channelBeam == value)
                {
                    return;
                }

                _channelBeam = value;
                Calculate();
                OnPropertyChanged("ChannelBeam");
            }
        }

        /// <summary>
        /// False: Do NOT show Squat if depth > 2*draught
        /// True: Do show Squat even if depth > 2*draught
        /// </summary>
        public bool ShowSquatEvenAtGreatDepths
        {
            get
            {
                return _showSquatEvenAtGreatDepths;
            }
            set
            {
                if (_showSquatEvenAtGreatDepths == value)
                {
                    return;
                }

                _showSquatEvenAtGreatDepths = value;
                OnPropertyChanged("ShowSquatEvenAtGreatDepths");
            }
        }

        /// <summary>
        /// False: Squat = null => Treat it still as Squat = null
        /// True: Squat = null => Treat it as Squat = 0.0
        /// </summary>
        public bool ConsiderSquatEqualsNullAsZero
        {
            get
            {
                return _considerSquatEqualsNullAsZero;
            }
            set
            {
                if (_considerSquatEqualsNullAsZero == value)
                {
                    return;
                }

                _considerSquatEqualsNullAsZero = value;
                OnPropertyChanged("ConsiderSquatEqualsNullAsZero");
            }
        }

       
        public double? SquatOpenWater
        {
            get
            {
                return _squatOpenWater;
            }
            set
            {
                if (_squatOpenWater == value)
                {
                    return;
                }

                _squatOpenWater = value;
                OnPropertyChanged("SquatOpenWater");
            }
        }

        public double? SquatChannel
        {
            get
            {
                return _squatChannel;
            }
            set
            {
                if (_squatChannel == value)
                {
                    return;
                }

                _squatChannel = value;
                
                OnPropertyChanged("SquatChannel");
            }
        }

        public double DraughtCorrection
        {
            get
            {
                return _draughtCorrection;
            }
            set
            {
                if (_draughtCorrection == value)
                {
                    return;
                }

                _draughtCorrection = value;
                OnPropertyChanged("DraughtCorrection");
                Calculate();
            }
        }

        public double DepthCorrection
        {
            get
            {
                return _depthCorrection;
            }
            set
            {
                if (_depthCorrection == value)
                {
                    return;
                }

                _depthCorrection = value;
                OnPropertyChanged("DepthCorrection");
                Calculate();
            }
        }

        public double DraughtAndDepthCorrection
        {
            get
            {
                return _draughtAndDepthCorrection;
            }
            set
            {
                if (_draughtAndDepthCorrection == value)
                {
                    return;
                }

                _draughtAndDepthCorrection = value;
                OnPropertyChanged("DraughtAndDepthCorrection");
            }
        }

        public double? DeepestDraughtOpenWater
        {
            get
            {
                return _deepestDraughtOpenWater;
            }
            set
            {
                if (_deepestDraughtOpenWater == value)
                {
                    return;
                }

                _deepestDraughtOpenWater = value;
                OnPropertyChanged("DeepestDraughtOpenWater");
                Calculate();
            }
        }

        public double? DeepestDraughtOpenWaterCorrected
        {
            get
            {
                return _deepestDraughtOpenWaterCorrected;
            }
            set
            {
                if (_deepestDraughtOpenWaterCorrected == value)
                {
                    return;
                }

                _deepestDraughtOpenWaterCorrected = value;
                OnPropertyChanged("DeepestDraughtOpenWaterCorrected");
            }
        }

        public double? DeepestDraughtChannel
        {
            get
            {
                return _deepestDraughtChannel;
            }
            set
            {
                if (_deepestDraughtChannel == value)
                {
                    return;
                }

                _deepestDraughtChannel = value;
                OnPropertyChanged("DeepestDraughtChannel");
                Calculate();
            }
        }

        public double? DeepestDraughtChannelCorrected
        {
            get
            {
                return _deepestDraughtChannelCorrected;
            }
            set
            {
                if (_deepestDraughtChannelCorrected == value)
                {
                    return;
                }

                _deepestDraughtChannelCorrected = value;
                OnPropertyChanged("DeepestDraughtChannelCorrected");
            }
        }

        public double? UkcOpenWater
        {
            get
            {
                return _ukcOpenWater;
            }
            set
            {
                if (_ukcOpenWater == value)
                {
                    return;
                }

                _ukcOpenWater = value;
                OnPropertyChanged("UkcOpenWater");
                Calculate();
            }
        }

        public double? UkcOpenWaterCorrected
        {
            get
            {
                return _ukcOpenWaterCorrected;
            }
            set
            {
                if (_ukcOpenWaterCorrected == value)
                {
                    return;
                }

                _ukcOpenWaterCorrected = value;
                OnPropertyChanged("UkcOpenWaterCorrected");
            }
        }

        public double? UkcChannel
        {
            get
            {
                return _ukcChannel;
            }
            set
            {
                if (_ukcChannel == value)
                {
                    return;
                }

                _ukcChannel = value;
                OnPropertyChanged("UkcChannel");
                Calculate();
            }
        }

        public double? UkcChannelCorrected
        {
            get
            {
                return _ukcChannelCorrected;
            }
            set
            {
                if (_ukcChannelCorrected == value)
                {
                    return;
                }

                _ukcChannelCorrected = value;
                OnPropertyChanged("UkcChannelCorrected");
            }
        }

        public double BlockCoefficient
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
                Calculate();
                OnPropertyChanged("BlockCoefficient");
            }
        }

        public double MaxDraughtVessel
        {
            get
            {
                return _maxDraughtVessel;
            }
            set
            {
                if (_maxDraughtVessel == value)
                {
                    return;
                }

                _maxDraughtVessel = value;
                Calculate();
                OnPropertyChanged("MaxDraughtVessel");
            }
        }

        public double MeanDraughtVessel
        {
            get
            {
                return _meanDraughtVessel;
            }
            set
            {
                if (_meanDraughtVessel == value)
                {
                    return;
                }

                _meanDraughtVessel = value;
                Calculate();
                OnPropertyChanged("MeanDraughtVessel");
            }
        }

        /// <summary>
        /// Main calculation method for Squat, Deepest draught and Ukc
        /// </summary>
        public void Calculate()
        {
            // Only meaningful to calculate if certain values are greater than zero
            if (BlockCoefficient > 0.0 && Speed > 0.0 && MaxDraughtVessel > 0.0 && WaterDepth != null && WaterDepth > 0.0)
            {
                //
                // Open water calculations
                //

                if (!ShowSquatEvenAtGreatDepths && WaterDepth > 2.0 * MaxDraughtVessel)
                {
                    SquatOpenWater = null;
                }
                else
                {
                    // SquatOpenWater = (_blockCoefficient / 30.0) * Math.Pow(_speed, 2.08) * Math.Pow((_vesselBeam * _maxDraughtVessel) / ((WaterDepth * VesselBeam * (Math.Pow(1.0 - _blockCoefficient, 2) * 20.0 + 7.7)) - (_vesselBeam * _maxDraughtVessel)), 2.0 / 3.0);
                    // Same formula as above, just removed vessel beam as it is redundant.
                    SquatOpenWater = (BlockCoefficient / 30.0) * Math.Pow(Speed, 2.08) * Math.Pow(MaxDraughtVessel / (((double)WaterDepth * (Math.Pow(1.0 - BlockCoefficient, 2) * 20.0 + 7.7)) - MaxDraughtVessel), 2.0 / 3.0);
                }

                if (SquatOpenWater == null && ConsiderSquatEqualsNullAsZero)
                {
                    DeepestDraughtOpenWater = MaxDraughtVessel;
                }
                else
                {
                    DeepestDraughtOpenWater = MaxDraughtVessel + SquatOpenWater;
                }
                DeepestDraughtOpenWaterCorrected = DeepestDraughtOpenWater + DraughtCorrection;
                UkcOpenWater = WaterDepth - DeepestDraughtOpenWater;
                UkcOpenWaterCorrected = UkcOpenWater - DraughtCorrection - DepthCorrection;

                //
                // Channel calculations
                //
                if (ChannelBeam > 0.0 && MeanDraughtVessel > 0.0 && VesselBeam > 0.0)
                {
                    if ((!ShowSquatEvenAtGreatDepths && WaterDepth > 2.0 * MaxDraughtVessel) || ((double)WaterDepth * ChannelBeam) <= (VesselBeam * MeanDraughtVessel))
                    {
                        SquatChannel = null;
                    }
                    else
                    {
                        SquatChannel = (BlockCoefficient / 30.0) * Math.Pow(Speed, 2.08) * Math.Pow((VesselBeam * MeanDraughtVessel) / (((double)WaterDepth * ChannelBeam) - (VesselBeam * MeanDraughtVessel)), 2.0 / 3.0);
                    }

                    // Ensure that "Squat Channel" is never less than "Squat Open Water" 
                    // (happens when the two equations cross each other, when channel width is greater than about 8-9 times the vessel beam).
                    // That is, for great values of Channel Width: Set "Squat Channel" equals "Squat Open Water"!
                    if (SquatChannel != null && SquatOpenWater != null && SquatChannel < SquatOpenWater)
                    {
                        SquatChannel = SquatOpenWater;
                    }

                    if (SquatChannel == null && ConsiderSquatEqualsNullAsZero)
                    {
                        DeepestDraughtChannel = MaxDraughtVessel;
                    }
                    else
                    {
                        DeepestDraughtChannel = MaxDraughtVessel + SquatChannel;
                    }

                    DeepestDraughtChannel = MaxDraughtVessel + SquatChannel;
                    DeepestDraughtChannelCorrected = DeepestDraughtChannel + DraughtCorrection;
                    UkcChannel = WaterDepth - DeepestDraughtChannel;
                    UkcChannelCorrected = UkcChannel - DraughtCorrection - DepthCorrection;
                }
                else
                {
                    SquatChannel = null;
                    DeepestDraughtChannel = null;
                    DeepestDraughtChannelCorrected = null;
                    UkcChannel = null;
                    UkcChannelCorrected = null;
                }
            }
            else
            {
                SquatOpenWater = null;
                DeepestDraughtOpenWater = null;
                DeepestDraughtOpenWaterCorrected = null;
                UkcOpenWater = null;
                UkcOpenWaterCorrected = null;

                SquatChannel = null;
                DeepestDraughtChannel = null;
                DeepestDraughtChannelCorrected = null;
                UkcChannel = null;
                UkcChannelCorrected = null;
            }
    
            OnPropertyChanged("SquatOpenWater");
            OnPropertyChanged("DeepestDraughtOpenWater");
            OnPropertyChanged("DeepestDraughtOpenWaterCorrected");
            OnPropertyChanged("UkcOpenWater");
            OnPropertyChanged("UkcOpenWaterCorrected");

            OnPropertyChanged("SquatChannel");
            OnPropertyChanged("DeepestDraughtChannel");
            OnPropertyChanged("DeepestDraughtChannelCorrected");
            OnPropertyChanged("UkcChannel");
            OnPropertyChanged("UkcChannelCorrected");
        }

        protected virtual void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = System.Threading.Interlocked.CompareExchange(ref PropertyChanged, null, null);
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }

        public object Clone()
        {
            SquatUkcItem sui = (SquatUkcItem)this.MemberwiseClone();
            return (object)sui;
        }

    }
}
