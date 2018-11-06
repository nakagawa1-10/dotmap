using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Docomo.Map5g
{
    public static class AppUtility
    {
        // Threshold用のColorに対してTarget用のColorがより暗いかどうか判定 (マスク用)
        public static bool IsDarkerThan(this Color targetColor, Color thresholdColor)
        {
            return
                targetColor.r >= thresholdColor.r ||
                targetColor.g >= thresholdColor.g ||
                targetColor.b >= thresholdColor.b;
        }
    }
}