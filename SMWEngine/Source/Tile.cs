using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace SMWEngine.Source
{
    public enum ColliderType
    {
        NONE = -1,
        SOLID = 0,
        PLATFORM = 1,
        CLIMB = 2,
        SWIM = 3,
        SLOPE_L1 = 4,
        SLOPE_L2 = 5,
        SLOPE_R1 = 6,
        SLOPE_R2 = 7,
        SLOPE_R = 8,
        SLOPE_L = 9,
    }
    public class Tile
    {
        public ColliderType colliderType;
        public const int width = 16;
        public const int height = 16;
        public Texture2D texture;
        public Point textureLocation;
    }
    public class TileInfo
    {
        public static Dictionary<int, ColliderType> colliderDictionary = new Dictionary<int, ColliderType>
        {
            [0] = ColliderType.SOLID,
            [1] = ColliderType.PLATFORM,
            [2] = ColliderType.SOLID,
            [3] = ColliderType.SOLID,
            [4] = ColliderType.SOLID,
            [5] = ColliderType.SOLID,
            [6] = ColliderType.SOLID,
            [7] = ColliderType.PLATFORM,
            [8] = ColliderType.PLATFORM,
            [9] = ColliderType.PLATFORM,
            [140] = ColliderType.SOLID,
            [141] = ColliderType.NONE,
            [142] = ColliderType.SOLID,
            [143] = ColliderType.SOLID,
            [144] = ColliderType.SOLID,
            [145] = ColliderType.SOLID,
            [146] = ColliderType.SOLID,
            [147] = ColliderType.NONE,
            [148] = ColliderType.NONE,
            [149] = ColliderType.NONE,

            // Left facing tippy toppy platform
            [155] = ColliderType.SLOPE_R,
            [156] = ColliderType.NONE,
            [294] = ColliderType.SLOPE_R,
            [295] = ColliderType.NONE,
            [296] = ColliderType.NONE,
            [297] = ColliderType.NONE,
            [434] = ColliderType.NONE,
            [435] = ColliderType.NONE,
            [436] = ColliderType.NONE,
            [437] = ColliderType.NONE,
            [438] = ColliderType.NONE,
            [575] = ColliderType.NONE,
            [576] = ColliderType.NONE,
            [577] = ColliderType.NONE,
            [578] = ColliderType.NONE,
            [579] = ColliderType.NONE,

            [280] = ColliderType.SOLID,
            [281] = ColliderType.SOLID,
            [282] = ColliderType.SOLID,
            [283] = ColliderType.SOLID,
            [284] = ColliderType.SOLID,
            [285] = ColliderType.SOLID,
            [286] = ColliderType.SOLID,
            [426] = ColliderType.PLATFORM,
            [427] = ColliderType.SOLID,
            [428] = ColliderType.SOLID,
            [429] = ColliderType.PLATFORM,
            [430] = ColliderType.PLATFORM,
            [431] = ColliderType.SOLID,
            [439] = ColliderType.SOLID,

            // SLOPE R
            [702] = ColliderType.SLOPE_R1,
            [703] = ColliderType.SLOPE_R2,
            [842] = ColliderType.NONE,

            // SLOPE R STEEP
            [700] = ColliderType.SLOPE_R,
            [840] = ColliderType.NONE,

            // SLOPE L
            [704] = ColliderType.SLOPE_L1,
            [705] = ColliderType.SLOPE_L2,
            [845] = ColliderType.NONE,

            // SLOPE L STEEP
            [701] = ColliderType.SLOPE_L,
            [841] = ColliderType.NONE,

            [3786] = ColliderType.NONE,
            [3787] = ColliderType.NONE,
            [3788] = ColliderType.NONE,

        };
        /*public static Dictionary<string, Dictionary<int, ColliderType>> colliderDictionary = new Dictionary<string, Dictionary<int, ColliderType>>();*/
        Dictionary<int, ColliderType> groundDictionary = new Dictionary<int, ColliderType>
        {
            [8] = ColliderType.PLATFORM,
            [7] = ColliderType.PLATFORM,
            [8] = ColliderType.PLATFORM,
            [9] = ColliderType.PLATFORM,
            [141] = ColliderType.PLATFORM,
            [147] = ColliderType.NONE,
            [148] = ColliderType.NONE,
            [149] = ColliderType.NONE,
            [3786] = ColliderType.NONE,
            [3787] = ColliderType.NONE,
            [3788] = ColliderType.NONE,

        };
        Dictionary<int, ColliderType> forestDictionary = new Dictionary<int, ColliderType>
        {
            [8] = ColliderType.PLATFORM,
            [7] = ColliderType.PLATFORM,
            [8] = ColliderType.PLATFORM,
            [9] = ColliderType.PLATFORM,
            [141] = ColliderType.PLATFORM,
            [147] = ColliderType.NONE,
            [148] = ColliderType.NONE,
            [149] = ColliderType.NONE,
            [3786] = ColliderType.NONE,
            [3787] = ColliderType.NONE,
            [3788] = ColliderType.NONE,

        };
    }
}