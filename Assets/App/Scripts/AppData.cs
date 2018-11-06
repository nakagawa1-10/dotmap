using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Docomo.Map5g
{
    public class MapSetting
    {
        public static readonly string PATH = "mapsetting.json";

        public int Width;

        public int Height;

        public float WidthGapSize;

        public float HeightGapSize;

        public MapSetting()
        {
            Width = 64;
            Height = 64;
            WidthGapSize = 1.0f;
            HeightGapSize = 1.0f;
        }
    }
}