using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Xna.Framework;

namespace SMWEngine.Source.Engine
{
	/**
	 * Flixel colors
	 */
    public class CatColor : IEquatable<Color>
    {
		public static Color TRANSPARENT = FixColor(0x00000000);
        public static Color WHITE = FixColor(0xFFFFFFFF);
		public static Color GRAY = FixColor(0xFF808080);
		public static Color BLACK = FixColor(0xFF000000);
		public static Color GREEN = FixColor(0xFF008000);
		public static Color LIME = FixColor(0xFF00FF00);
		public static Color YELLOW = FixColor(0xFFFFFF00);
		public static Color ORANGE = FixColor(0xFFFFA500);
		public static Color RED = FixColor(0xFFFF0000);
		public static Color PURPLE = FixColor(0xFF800080);
		public static Color BLUE = FixColor(0xFF0000FF);
		public static Color BROWN = FixColor(0xFF8B4513);
		public static Color PINK = FixColor(0xFFFFC0CB);
		public static Color MAGENTA = FixColor(0xFFFF00FF);
		public static Color CYAN = FixColor(0xFF00FFFF);

		private static Color FixColor(uint value)
		{
			var col = new Color(value);
			var _r = col.R;
			col.R = col.B;
			col.B = _r;
			return col;
		}

		public bool Equals(Color other)
        {
			return (other is Color color) && this.Equals(color);
		}
    }
}
