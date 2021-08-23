using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace SMWEngine.Source
{
    public class Basic
    {
        public Level level;
        protected void DrawSprite(Texture2D sprite, int X, int Y, Vector2 pivot, Rectangle? cutOut, SpriteEffects spriteEffect)
        {
            var cutOutRect = Rectangle.Empty;
            if (cutOut is Rectangle)
                cutOutRect = (Rectangle)cutOut;
            else cutOutRect = new Rectangle(Point.Zero, new Point(sprite.Width, sprite.Height));
            level.spriteBatch.Draw(sprite, new Rectangle(new Point(X, Y), new Point(cutOutRect.Width, cutOutRect.Height)), cutOutRect, Color.White, 0f, new Vector2((float) Math.Floor(sprite.Width * pivot.X), (float) Math.Floor(sprite.Height * pivot.Y)), spriteEffect, 0f);
        }
    }
}