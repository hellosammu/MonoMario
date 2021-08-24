using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace SMWEngine.Source
{
    public class HUD : CatSprite
    {
        private Texture2D reserveSprite;
        private Texture2D timeSprite;

        public HUD()
        {
            reserveSprite = SMW.Load("HUD/Reserve");
            timeSprite = SMW.Load("HUD/Time");
        }

        public void Draw()
        {
            DrawSprite(reserveSprite, 128-16, 24-16, new Vector2(0.5f, 0.5f), SpriteEffects.None, Rectangle.Empty);
            DrawSprite(timeSprite, 152, 15, Vector2.Zero, SpriteEffects.None, Rectangle.Empty);
        }

    }
}