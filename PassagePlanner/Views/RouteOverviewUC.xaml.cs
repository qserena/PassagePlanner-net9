//#define DEBUG

using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;

using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using C1.WPF.Maps;
using System.IO;
using ExcelLayer;
using System.Collections.ObjectModel;
using System.Reflection;



namespace PassagePlanner
{
	/// <summary>
	/// Interaction logic for RouteOverviewUC.xaml
	/// </summary>
	public partial class RouteOverviewUC : UserControl
	{
	
		public RouteOverviewUC()
		{
			this.InitializeComponent();

            bool designMode = System.ComponentModel.DesignerProperties.GetIsInDesignMode(new DependencyObject());

            if (!designMode)
            {
                if (map.Source != null)
                {
                    map.Source = new LocalMapSource();
                    map.Zoom = 0.5;
                    map.Center = new Point(0, 15);
                }

                //vlWaypoints.Visibility = Visibility.Visible;
                //checkBoxShowWaypoints.IsChecked = true;

                if ((bool)checkBoxShowWaypoints.IsChecked)
                {
                    vlWaypoints.Visibility = Visibility.Visible;
                }
                else
                {
                    vlWaypoints.Visibility = Visibility.Hidden;
                }

                polyline1.Visibility = Visibility.Visible;
                polyline2.Visibility = Visibility.Visible;
            }
		}

        public static Geometry CreateBaloon()
        {
            PathGeometry pg = new PathGeometry();
            pg.Transform = new TranslateTransform() { X = -10, Y = -24.14 };
            PathFigure pf = new PathFigure() { StartPoint = new Point(10, 24.14), IsFilled = true, IsClosed = true };
            pf.Segments.Add(new ArcSegment() { SweepDirection = SweepDirection.Counterclockwise, Point = new Point(5, 19.14), RotationAngle = 45, Size = new Size(8, 8) });
            pf.Segments.Add(new ArcSegment() { SweepDirection = SweepDirection.Clockwise, Point = new Point(15, 19.14), RotationAngle = 270, Size = new Size(8, 8), IsLargeArc = true });
            pf.Segments.Add(new ArcSegment() { SweepDirection = SweepDirection.Counterclockwise, Point = new Point(10, 24.14), RotationAngle = 45, Size = new Size(8, 8) });
            pg.Figures.Add(pf);
            return pg;
        }

        public void ShowRoute(string fileName)
        {
            Route route = new Route(fileName);
        }

        private void ShowWaypoints_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if ((bool)((CheckBox)sender).IsChecked)
            {
                vlWaypoints.Visibility = Visibility.Visible;
            }
            else
            {
                vlWaypoints.Visibility = Visibility.Hidden;
            }
        }

        private void VectorLayer_SourceUpdated(object sender, DataTransferEventArgs e)
        {
            List<Point> linePoints = new List<Point>();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            ViewModelLocator locator = new ViewModelLocator();
            locator.RouteVM.UpdateWaypointRelatedStuff(null);

            if ((bool)checkBoxShowWaypoints.IsChecked)
            {
                vlWaypoints.Visibility = Visibility.Visible;
            }
            else
            {
                vlWaypoints.Visibility = Visibility.Hidden;
            }
        }
	}

    public class LocalMapSource : C1MultiScaleTileSource
    {
        string filepath;
        public LocalMapSource()
            : base(0x8000000, 0x8000000, 256, 256, 0)
        {
            string executableDirectory = AppDomain.CurrentDomain.BaseDirectory;

            string tileslocation = string.Empty;

            if (Directory.Exists(executableDirectory + "MapTiles"))
            {
                tileslocation = string.Format(@"{0}MapTiles\", executableDirectory);
            }
            else if (Directory.Exists(executableDirectory + "..\\..\\MapTiles"))
            {
                tileslocation = string.Format(@"{0}..\..\MapTiles\", executableDirectory);
            }
            else
            {
                throw new DirectoryNotFoundException("Directory MapTiles not found (executable directory: " + executableDirectory + ")");
            }

            filepath = tileslocation + @"{0}\{1}\{2}.png";
        }

        protected override void GetTileLayers(int tileLevel, int tilePositionX, int tilePositionY, IList<object> tileImageLayerSources)
        {
            bool designMode = System.ComponentModel.DesignerProperties.GetIsInDesignMode(new DependencyObject());

            if (!designMode)
            {
                if (tileLevel > 8)
                {
                    tileImageLayerSources.Add(new Uri(string.Format(filepath, tileLevel - 8, tilePositionX, tilePositionY)));
                }
            }
        }
    }
}