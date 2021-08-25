using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SMWEngine.Source.Engine;

namespace SMWEngine.Source
{
    /**
     * Sprite object that most objects will inherit from.
     * Provides useful features such as drawing support
     */
    public class CatSprite
    {
        // Level this object belongs to
        public Level level;

        // CatSprite & Animation
        public Texture2D texture;

        // CatSprite size information
        protected int spriteHeight = 32;
        protected int spriteWidth = 32;

        // Name of sprite animation
        public string curAnim = "idle";

        /**
         * Example usage:
         * animList = new Dictionary<string, List<dynamic>>
         * {
         *      ["spin"] = { 0, 19, 0.5, 20 },
         * }
         * -- NOTE: The list is doubles because anything between an integer will flip the sprite! --
         */
        public Dictionary<string, List<double>> animList;

        // The current image that the sprite is currently. UpdateAnimations() called is required to loop this around automatically
        public float curImage;

        // Speed that the animation goes up per-frame. UpdateAnimations() must be called to do this automatically
        public float imgSpeed;

        // Opacity of sprite
        public float imageAlpha = 1f;

        // Color of sprite
        public Color imageBlend = CatColor.WHITE;

        // Angle of sprite (0-360)
        public float imageAngle = 0f;

        // Whether the object is flipped or not
        public bool flipX = false;
        public bool flipY = false;

        protected Rectangle spriteCutOut
        {
            // No animation list, no sprite cut-out.
            get
            {
                if (animList == null)
                    return Rectangle.Empty;

                // Get proper cut-out, apply
                List<double> intList;
                animList.TryGetValue(curAnim, out intList);
                int positionInSheet = (int) intList[(int) Math.Floor((float) Math.Abs(Math.Floor(curImage)))];
                var cutOut = new Rectangle(new Point(positionInSheet * spriteWidth, 0), new Point(spriteWidth, spriteHeight));
                return cutOut;
            }
        }

        /*
         * Call this ONCE per-frame to update animations of object. Done automatically for added entities
         * */
        public void UpdateAnimations()
        {
            // Update the current image by the image speed
            curImage += imgSpeed;

            // If the current image is above the animation number, go downwards by the list amount
            if (animList != null)
                while (curImage >= animList[curAnim].Count)
                    curImage -= animList[curAnim].Count;
        }

        protected void DrawSprite(Texture2D sprite, float X, float Y, Vector2 _pivot, Rectangle _spriteCutOut)
        {
            Rectangle drawnCutOut = _spriteCutOut;
            if (_spriteCutOut == Rectangle.Empty)
            {
                drawnCutOut = new Rectangle(Point.Zero, new Point(sprite.Width, sprite.Height));
            }

            // Flip sprite (X axis)
            SpriteEffects isFlippedX = (flipX) ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

            // Extra flip based on animation parameter
            if (animList != null)
                if (animList[curAnim][Math.Abs((int)Math.Floor(curImage))] != Math.Floor((float)animList[curAnim][Math.Abs((int)Math.Floor(curImage))]))
                    isFlippedX = (flipX) ? SpriteEffects.None : SpriteEffects.FlipHorizontally;

            // Flip sprite (Y axis)
            SpriteEffects isFlippedY = (flipY) ? SpriteEffects.FlipVertically : SpriteEffects.None;

            // Get true pivot by multiplying the halves
            var __pivot = new Vector2((float) Math.Floor(drawnCutOut.Width * _pivot.X), (float)Math.Floor(drawnCutOut.Height * _pivot.Y));
            // Draw the sprite
            level.spriteBatch.Draw(sprite, new Rectangle(new Point((int) Math.Floor(X) + (int) Math.Floor(__pivot.X), (int) Math.Floor(Y) + (int) Math.Floor(__pivot.Y)), new Point(drawnCutOut.Width, drawnCutOut.Height)), drawnCutOut, imageBlend * imageAlpha, imageAngle / 360f, __pivot, isFlippedX | isFlippedY, 0f);
        }
    }
}