using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace SMWEngine.Source
{
    public class HUD : Basic
    {
        private Texture2D reserveSprite;
        private Texture2D timeSprite;

        public HUD()
        {
            reserveSprite = Level.Load("HUD/Reserve");
            timeSprite = Level.Load("HUD/Time");
        }

        public void Draw()
        {
            DrawSprite(reserveSprite, 128, 24, new Vector2(0.5f, 0.5f), null, SpriteEffects.None);
            DrawSprite(timeSprite, 152, 15, Vector2.Zero, null, SpriteEffects.None);
        }

    }
}
