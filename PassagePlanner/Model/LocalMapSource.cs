﻿using C1.WPF.Maps;
using System;
using System.Collections.Generic;
using System.IO;

namespace PassagePlanner
{
    public class LocalMapSource : MultiScaleTileSource
    {
        private string filePath;
        public LocalMapSource()
            : base(0x8000000, 0x8000000, 256, 256, 0)
        {
            var index = Directory.GetCurrentDirectory().IndexOf("\\bin");
            string tileslocation = string.Format(@"{0}\MyTiles\", Directory.GetCurrentDirectory().Substring(0, index));
            filePath = tileslocation + @"{0}\{1}\{2}.png";
        }

        protected override void GetTileLayers(int tileLevel, int tilePositionX, int tilePositionY, IList<object> tileImageLayerSources)
        {
            if (tileLevel > 8)
            {
                tileImageLayerSources.Add(new Uri(string.Format(filePath, tileLevel - 8, tilePositionX, tilePositionY)));
            }
        }
    }
}
