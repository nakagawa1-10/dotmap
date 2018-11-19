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

        public float IdleWaveIntensity;

        // タップした位置から周囲何pxまで波紋を広げるかの設定値
        public int RipplePxRange;

        public float RippleIntensity;

        public float RippleDuration;

        public MapSetting()
        {
            Width = 64;
            Height = 64;
            WidthGapSize = 1.0f;
            HeightGapSize = 1.0f;
            IdleWaveIntensity = 0.5f;
            RipplePxRange = 13;
            RippleIntensity = 6.0f;
            RippleDuration = 1.3f;
        }
    }
}