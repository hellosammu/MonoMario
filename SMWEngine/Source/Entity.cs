using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace SMWEngine.Source
{
    public abstract class Entity : Basic
    {
        // Depth mgmt
        public float depth = 0;

        // Movement
        public Vector2 speed = new Vector2();
        public Vector2 velocity = new Vector2();
        public Vector2 position = new Vector2();
        public Vector2 positionLast = new Vector2();


        // Sprite & Animation
        public Texture2D texture;
        public float alpha = 1;
        public float curImage;
        public float imgSpeed;
        public string curAnim = "idle";
        public Dictionary<string, List<dynamic>> animList;
        public SpriteEffects flipX = SpriteEffects.None;

        // Mask information
        protected int maskHeight = 32;
        protected int maskWidth = 32;

        // ACTUAL PLACES YOU SHOULD CLIP! THE TOP INFORMATION IS THE SIZE OF THE TEXTURE CUT-OUT YOU ARE USING
        protected int leftClip = 0;
        protected int rightClip = 0;
        protected int topClip = 0;
        // Quick accessor for bounding box of object
        public Rectangle boundingBox
        {
            get
            {
                return new Rectangle((int) position.X + leftClip, (int) position.Y + topClip, maskWidth - rightClip - leftClip, maskHeight - topClip);
            }
        }

        #region Variables not to modify

        // Position
        public bool isGrounded = true;
        public bool onSlope = false;
        public float slopeIntensity = 0;
        public bool atWall = false;

        #endregion

        // Called before normal update
        public abstract void EarlyUpdate();
        public abstract void Update();
        public abstract void LateUpdate();
        public abstract void Draw();
        public abstract void HandleInteractions();

        public void UpdateAnimations()
        {
            // Update the current image by the image speed
            curImage += imgSpeed;

            // If the current image is above the animation number, go downwards by the list amount
            if (animList != null)
                while (curImage >= animList[curAnim].Count)
                    curImage -= animList[curAnim].Count;

            // Remember last position
            positionLast = position;
        }

        /**
         * Global collision system, no real need to modify this per-entity unless some incredibly proprietary collision system is necessary
         * */
        public void HandleCollisions()
        {
            isGrounded = false;
            atWall = false;
            var tempBB = boundingBox;

            var tWidth = level.tiles.GetLength(0);
            var tHeight = level.tiles.GetLength(1);
            var foundSlope = false;
            var wasOnSlope = onSlope;

            for (int x = (int)(boundingBox.Left - 1 - Math.Abs(speed.X)); x <= (int)(boundingBox.Right + 1 + Math.Abs(speed.X)); x += 2)
            {
                for (int y = (int)(boundingBox.Top - 1 - Math.Abs(speed.Y)); y <= (int)(boundingBox.Bottom + 4 + Math.Abs(speed.Y)); y += 2)
                {
                    // Tile collision position
                    var xCheck = (int)Math.Floor(x / 16f);
                    var yCheck = (int)Math.Floor(y / 16f);

                    // If not in array bounds
                    if ((xCheck >= tWidth)
                    || (yCheck >= tHeight)
                    || (xCheck < 0)
                    || (yCheck < 0))
                        continue;

                    var tile = level.tiles[xCheck, yCheck];

                    if (tile == null)
                        continue;

                    if ((xCheck * 16 < tempBB.Left - 32)
                    || (xCheck * 16 > tempBB.Right + 32))
                        continue;
                    var tileWorldPosition = new Rectangle(xCheck * 16, yCheck * 16, 16, 16);
                    var BB_Ref = boundingBox;
                    var BB_Down = new Rectangle(BB_Ref.X - 1, BB_Ref.Y - 1, BB_Ref.Width + 2, BB_Ref.Height + 2);
                    bool stupidTile = false;
                    if (yCheck < tHeight-1)
                        if (level.tiles[xCheck, yCheck + 1] != null)
                            if (level.tiles[xCheck, yCheck + 1].colliderType == ColliderType.PLATFORM || level.tiles[xCheck, yCheck + 1].colliderType == ColliderType.SOLID)
                                stupidTile = true;
                    if ((speed.Y > 0 || BB_Ref.Bottom <= tileWorldPosition.Top || (speed.Y >= 0 && (onSlope || !stupidTile)))
                    && (tile.colliderType == ColliderType.SLOPE_L1
                    || tile.colliderType == ColliderType.SLOPE_L2
                    || tile.colliderType == ColliderType.SLOPE_R1
                    || tile.colliderType == ColliderType.SLOPE_R2
                    || tile.colliderType == ColliderType.SLOPE_R
                    || tile.colliderType == ColliderType.SLOPE_L))
                    {
                        Vector2 sPoint1 = Vector2.Zero;
                        Vector2 sPoint2 = Vector2.Zero;
                        switch (tile.colliderType)
                        {
                            case (ColliderType.SLOPE_L1):
                                sPoint1 = new Vector2(tileWorldPosition.X, tileWorldPosition.Y);
                                sPoint2 = new Vector2(tileWorldPosition.X + 32, tileWorldPosition.Y + tileWorldPosition.Height);
                                break;
                            case (ColliderType.SLOPE_L2):
                                sPoint1 = new Vector2(tileWorldPosition.X - 16, tileWorldPosition.Y);
                                sPoint2 = new Vector2(tileWorldPosition.X + 16, tileWorldPosition.Y + tileWorldPosition.Height);
                                break;
                            case (ColliderType.SLOPE_R1):
                                sPoint1 = new Vector2(tileWorldPosition.X, tileWorldPosition.Y + tileWorldPosition.Height);
                                sPoint2 = new Vector2(tileWorldPosition.X + 32, tileWorldPosition.Y);
                                break;
                            case (ColliderType.SLOPE_R2):
                                sPoint1 = new Vector2(tileWorldPosition.X - 16, tileWorldPosition.Y + tileWorldPosition.Height);
                                sPoint2 = new Vector2(tileWorldPosition.X + 16, tileWorldPosition.Y);
                                break;
                            case (ColliderType.SLOPE_R):
                                sPoint1 = new Vector2(tileWorldPosition.X, tileWorldPosition.Y + tileWorldPosition.Height);
                                sPoint2 = new Vector2(tileWorldPosition.X + 16, tileWorldPosition.Y);
                                break;
                            case (ColliderType.SLOPE_L):
                                sPoint1 = new Vector2(tileWorldPosition.X, tileWorldPosition.Y);
                                sPoint2 = new Vector2(tileWorldPosition.X + 16, tileWorldPosition.Y + tileWorldPosition.Height);
                                break;
                        }
                        slopeIntensity = ((sPoint2.Y - sPoint1.Y) / (sPoint2.X - sPoint1.X));
                        // In the middle of the slope
                        if (boundingBox.Center.X >= sPoint1.X && boundingBox.Center.X <= sPoint2.X)
                        {
                            var sPosition = sPoint1.Y + ((sPoint2.Y - sPoint1.Y) * ((boundingBox.Center.X - sPoint1.X) / (sPoint2.X - sPoint1.X)));
                            // Below or inside slope
                            if ((boundingBox.Bottom >= sPosition - Math.Clamp(Math.Abs(speed.X)*2, 1, 8))
                            && (boundingBox.Bottom-6 < sPosition))
                            {
                                foundSlope = true;
                                position.Y = sPosition - maskHeight;
                                velocity.Y = 0;
                                speed.Y = 0;
                                isGrounded = true;
                                onSlope = true;
                            }
                        }
                    }
                }

            }

            for (int x = (int)(boundingBox.Left - 1 - Math.Abs(speed.X)); x <= (int)(boundingBox.Right + 1 + Math.Abs(speed.X)); x += 2)
            {
                for (int y = (int)(boundingBox.Top - 1 - Math.Abs(speed.Y)); y <= (int)(boundingBox.Bottom + 4 + Math.Abs(speed.Y)); y += 2)
                {
                    // Tile collision position
                    var xCheck = (int) Math.Floor(x / 16f);
                    var yCheck = (int) Math.Floor(y / 16f);

                    // If not in array bounds
                    if ((xCheck >= tWidth)
                    || (yCheck >= tHeight)
                    || (xCheck < 0)
                    || (yCheck < 0))
                        continue;

                    var tile = level.tiles[xCheck, yCheck];

                    if (tile == null)
                        continue;

                    if ((xCheck * 16 < tempBB.Left - 32)
                    || (xCheck * 16 > tempBB.Right + 32))
                        continue;

                    var tileWorldPosition = new Rectangle(xCheck * 16, yCheck * 16, 16, 16);
                    var BB_Ref = boundingBox;
                    var BB_Down = new Rectangle(BB_Ref.X, BB_Ref.Y-1, BB_Ref.Width, BB_Ref.Height+2);
                    // Down
                    if (!onSlope && speed.Y >= 0 && (tile.colliderType == ColliderType.SOLID || tile.colliderType == ColliderType.PLATFORM))
                    {
                        if (BB_Down.Intersects(tileWorldPosition))
                        {
                            if ((boundingBox.Bottom >= tileWorldPosition.Top)
                            && (boundingBox.Bottom < tileWorldPosition.Top + 4 + speed.Y)
                            && (boundingBox.Right + speed.X >= tileWorldPosition.Left)
                            && (boundingBox.Left + speed.X <= tileWorldPosition.Right))
                            {
                                position.Y = tileWorldPosition.Top - maskHeight;
                                velocity.Y = 0;
                                speed.Y = 0;
                                isGrounded = true;
                            }
                        }
                    }
                    // Up
                    else if (speed.Y < 0 && tile.colliderType == ColliderType.SOLID)
                    {
                        if (BB_Down.Intersects(tileWorldPosition))
                        {
                            if ((boundingBox.Top <= tileWorldPosition.Bottom)
                            && (boundingBox.Top > tileWorldPosition.Bottom - 8)
                            && (boundingBox.Right-1 >= tileWorldPosition.Left + speed.X)
                            && (boundingBox.Left+1 <= tileWorldPosition.Right + speed.X))
                            {
                                position.Y = tileWorldPosition.Bottom - topClip;
                                speed.Y = 0;
                            }
                        }
                    }
                    // Right
                    if (speed.X >= 0 && tile.colliderType == ColliderType.SOLID)
                    {
                        if (BB_Down.Intersects(tileWorldPosition))
                        {
                            if ((boundingBox.Right >= tileWorldPosition.Left)
                            && (boundingBox.Right < tileWorldPosition.Left + 1 + Math.Abs(speed.X))
                            && (boundingBox.Bottom > tileWorldPosition.Top + 4)
                            && (boundingBox.Top < tileWorldPosition.Bottom + speed.Y))
                            {
                                position.X = tileWorldPosition.Left - maskWidth + rightClip;
                                velocity.X = 0;
                                speed.X = 0;
                                atWall = true;
                            }
                        }
                    }
                    // Left
                    else if (speed.X <= 0 && tile.colliderType == ColliderType.SOLID)
                    {
                        if (BB_Down.Intersects(tileWorldPosition))
                        {
                            if ((boundingBox.Left <= tileWorldPosition.Right)
                            && (boundingBox.Left > tileWorldPosition.Right - 1 - Math.Abs(speed.X))
                            && (boundingBox.Bottom > tileWorldPosition.Top + 4)
                            && (boundingBox.Top < tileWorldPosition.Bottom + speed.Y))
                            {
                                position.X = tileWorldPosition.Right - leftClip;
                                velocity.X = 0;
                                speed.X = 0;
                                atWall = true;
                            }
                        }
                    }
                }
            }

            if (!foundSlope)
            {
                if (!isGrounded && wasOnSlope && speed.Y >= 0)
                {
                    speed.Y += Math.Abs(slopeIntensity * speed.X);
                }
                onSlope = false;
            }
        }

    }
}
