using C1.WPF.Maps;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PassagePlanner
{
    public class LocalMapSource : C1MultiScaleTileSource
    {
        string filepath;
        public LocalMapSource()
            : base(0x8000000, 0x8000000, 256, 256, 0)
        {
            var index = Directory.GetCurrentDirectory().IndexOf("\\bin");
            string tileslocation = string.Format(@"{0}\MyTiles\", Directory.GetCurrentDirectory().Substring(0, index));
            filepath = tileslocation + @"{0}\{1}\{2}.png";
        }

        protected override void GetTileLayers(int tileLevel, int tilePositionX, int tilePositionY, IList<object> tileImageLayerSources)
        {
            if (tileLevel > 8)
            {
                tileImageLayerSources.Add(new Uri(string.Format(filepath, tileLevel - 8, tilePositionX, tilePositionY)));
            }
        }
    }
}
