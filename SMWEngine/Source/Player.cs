using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;
using MonoGame.Extended.Input;

namespace SMWEngine.Source
{
    public class Player : Entity
    {
        public Vector2 lastSpeed;
        public Microsoft.Xna.Framework.Graphics.Effect paletteSwap;
        public bool isRunning = false;
        public bool isSpinning = false;
        public bool isJumping = false;
        public bool isCrouching = false;
        public bool isSliding = false;
        public bool noClip = false;
        public int pmeter = 0;
        public int pmeterMax = 112;
        public int scuttle = 0;

        bool inputRight => Keyboard.GetState().IsKeyDown(Keys.Right);
        bool inputLeft => Keyboard.GetState().IsKeyDown(Keys.Left);
        bool inputUp => Keyboard.GetState().IsKeyDown(Keys.Up);
        bool inputDown => Keyboard.GetState().IsKeyDown(Keys.Down);
        bool inputRun => Keyboard.GetState().IsKeyDown(Keys.Z);
        bool inputJump => Keyboard.GetState().IsKeyDown(Keys.X) || Keyboard.GetState().IsKeyDown(Keys.C);
        bool inputJumpPressed => SMW.KeyboardState.WasKeyJustUp(Keys.X);
        bool inputSpinPressed => SMW.KeyboardState.WasKeyJustUp(Keys.C);

        public Player(Vector2 position)
        {
            depth = -1;
            this.position = position;
            this.velocity.Y = 0.1875f;
            curAnim = "idle";
            animList = new Dictionary<string, List<dynamic>>
            {
                ["idle"] = new List<dynamic> { 0, 1 },
                ["walk"] = new List<dynamic> { 3, 0 },
                ["crouch"] = new List<dynamic> { 2 },
                ["swim"] = new List<dynamic> { 13, 14, 15 },
                ["idle hold"] = new List<dynamic> { 7, 8 },
                ["walk hold"] = new List<dynamic> { 10, 7 },
                ["crouch hold"] = new List<dynamic> { 9 },
                ["swim hold"] = new List<dynamic> { 16, 17, 18 },
                ["run"] = new List<dynamic> { 5, 4 },
                ["jump"] = new List<dynamic> { 11, 12 },
                ["skid"] = new List<dynamic> { 6 },
                ["run jump"] = new List<dynamic> { 15 },
                ["spin"] = new List<dynamic> { 0, 19, 0.5, 20 },
                ["slide"] = new List<dynamic> { 21 },
                ["kick"] = new List<dynamic> { 22 }
            };
            maskHeight = maskWidth = 32;
            rightClip = 10;
            leftClip = 10;
            topClip = 16;
            texture = Level.Load("Entities/Mario");
        }

        // Called before normal update
        public override void EarlyUpdate()
        {

        }

        // Main update
        Timer skidTimer = new Timer();
        public override void Update()
        {
            var thisStateFrame = SMW.KeyboardState;

            if (thisStateFrame.WasKeyJustUp(Keys.V))
            {

            }

            if (thisStateFrame.WasKeyJustUp(Keys.N))
            {
                noClip = !noClip;
            }

            if (noClip)
            {
                var runFactor = (inputRun) ? 2 : 1;
                speed = Vector2.Zero;
                if (inputRight)
                {
                    position.X += 4 * runFactor;
                    speed.X = 4 * runFactor;
                }
                else if (inputLeft)
                {
                    position.X -= 4 * runFactor;
                    speed.X = -4 * runFactor;
                }
                if (inputUp)
                {
                    position.Y -= 4 * runFactor;
                    speed.Y = -4 * runFactor;
                }
                else if (inputDown)
                {
                    position.Y += 4 * runFactor;
                    speed.Y = 4 * runFactor;
                }
                imgSpeed = 0;
                return;
            }

            // Necessary for single presses unfortunately
            var wasGrounded = isGrounded;
            // Clamp vertical speed
            if (speed.Y > 4)
                speed.Y = 4;

            // ADD A FUCK TON OF STUFF TO ITSELF
            velocity.Y = (inputJump) ? 0.1875f : 0.375f;
            var turning = ((speed.X > 0 && inputLeft && !inputRight) || (speed.X < 0 && inputRight)) ? true : false;
            var accelVar = (turning) ? 0.15625f : 0.09375f;
            var spdCap = (inputRun) ? ((isRunning) ? 3 : 2.25f) : 1.25f;
            var slipMod = 1;

            if ((turning || isSliding || isCrouching) && isGrounded && speed.X != 0)
            {
                if (skidTimer.timeLeft == 0)
                {
                    skidTimer = new Timer().Start(4);
                    var skidSmoke = new SkidSmoke(new Vector2(boundingBox.Center.X - 4, boundingBox.Bottom - 4));
                    skidSmoke.depth = -2;
                    Level.Add(skidSmoke);
                }
            }

            if (inputRight && (!isCrouching || !isGrounded))
            {
                flipX = SpriteEffects.None;
                if (speed.X < spdCap)
                {
                    speed.X += accelVar;
                    if (speed.X > spdCap)
                        speed.X = spdCap;
                }
                else if (speed.X > spdCap)
                {
                    if (speed.X > spdCap + (accelVar * 4))
                    {
                        speed.X -= (accelVar * 2) * (slipMod);
                    }
                }
            }
            else if (inputLeft && (!isCrouching || !isGrounded))
            {
                flipX = SpriteEffects.FlipHorizontally;
                if (speed.X > -spdCap)
                {
                    speed.X -= accelVar;
                    if (speed.X < -spdCap)
                        speed.X = -spdCap;
                }
                else if (speed.X < spdCap)
                {
                    if (speed.X < -spdCap - (accelVar * 4))
                    {
                        speed.X += (accelVar * 2) * (slipMod);
                    }
                }
            }
            else if (!(isSliding && onSlope))
            {
                // Slow down
                if (speed.X != 0 && isGrounded)
                {
                    speed.X -= Math.Sign(speed.X)*0.0625f;
                    if (Math.Abs(speed.X) <= 0.0625f)
                    {
                        speed.X = 0;
                    }
                }

            }

            if (inputDown && isGrounded)
            {
                if (!onSlope && !isSliding)
                {
                    isCrouching = true;
                }
                else
                {
                    isSliding = true;
                    isCrouching = false;
                }
            }
            else if (isGrounded)
            {
                isCrouching = false;
            }

            if (isSliding && onSlope)
            {
                speed.X += (slopeIntensity * 0.09375f);
                if (slopeIntensity != 0)
                {
                    if (Math.Sign(speed.X) == (Math.Sign(slopeIntensity)))
                    {
                        if (Math.Abs(speed.X) >= 3)
                            speed.X = 3 * Math.Sign(speed.X);
                    }
                }
            }
            else if (isSliding)
            {
                if (speed.X == 0)
                    isSliding = false;
            }

            if ((inputRight
            || inputLeft
            || inputJumpPressed
            || inputSpinPressed)
            && isSliding)
                isSliding = false;

            if (inputRun && (inputLeft || inputRight))
                if (Math.Abs(speed.X) >= 2.25f && isGrounded)
                    pmeter += 2;

            if ((!inputRun || (!inputLeft && !inputRight) || (Math.Abs(speed.X) < 2.25f)))
                pmeter -= 2;

            if (pmeter <= 0)
                pmeter = 0;

            if (pmeter >= pmeterMax)
            {
                pmeter = pmeterMax;
                isRunning = true;
            }
            else if (isGrounded)
            {
                isRunning = false;
            }

            speed += velocity;
            position += speed;

            float runningFloat = (!isRunning) ? 0 : 1f;
            if (isGrounded)
            {
                if (isSpinning)
                {
                    isSpinning = false;
                    if (speed.X == 0) {
                        var rand = new Random().Next(1, 3);
                        flipX = (rand == 1) ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
                    }
                }
                isJumping = false;
                velocity.Y = 0;
                speed.Y = 0;
            }

            if (scuttle > 0)
            {
                scuttle --;
            }

            if (inputJumpPressed && isGrounded)
            {
                speed.Y = -5f - (Math.Abs(speed.X) / 3.2f / ((2f - runningFloat) / 6f) / 7f);
                isJumping = true;
            }
            else if (inputSpinPressed && isGrounded)
            {
                isSpinning = true;
                isJumping = true;
                isCrouching = false;
                speed.Y = -4.6875f - (Math.Abs(speed.X) / 3.2f / ((2f - runningFloat) / 6f) / 7f);
            }

            lastSpeed = speed;
            HandleCollisions();
            HandleInteractions();

            if (wasGrounded != isGrounded && !isGrounded && !isSpinning)
                scuttle = (int) (Math.Abs(speed.X) * 3.5);

            if (isSliding)
            {
                curAnim = "slide";
            }
            else if (isCrouching)
            {
                curAnim = "crouch";
                curImage = 0;
                imgSpeed = 0;
            }
            else if (isGrounded || scuttle > 0)
            {
                if (speed.X != 0)
                {
                    curAnim = (!isRunning) ? "walk" : "run";
                    if (turning && isGrounded)
                        curAnim = "skid";
                    imgSpeed = Math.Abs(speed.X / 8f);
                }
                else
                {
                    curAnim = "idle";
                    imgSpeed = 0;
                    curImage = (!inputUp) ? 0 : 1;
                }
            }
            else
            {
                curAnim = (!isSpinning) ? (!(isRunning && isJumping) ? "jump" : "run jump") : "spin";
                if (!isSpinning)
                {
                    imgSpeed = 0;
                    if (speed.Y >= 0 || !isJumping)
                        curImage = 1;
                    else
                        curImage = 0;
                }
                else
                {
                    imgSpeed = 0.7f;
                }
            }

            if (position.Y-16 >= level.levelSize.Y*16)
                position.Y = (level.levelSize.Y * 16) - 16;

        }

        // Called after main update
        public override void LateUpdate()
        {

        }


        public override void Draw()
        {
            var curImgToIndex = Math.Abs((int) Math.Floor(curImage));
            List<dynamic> intList;
            animList.TryGetValue(curAnim, out intList);
            var isFlipped = flipX;
            if (intList[curImgToIndex] != Math.Floor((float) intList[curImgToIndex]))
                isFlipped = (flipX == SpriteEffects.FlipHorizontally) ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
            int positionInSheet = (int) intList[(int) Math.Floor((float) Math.Abs(curImgToIndex))];
            var drawnPos = new Point((int)position.X+(maskWidth/2), (int)position.Y+(maskHeight/2) + 1);
            var drawnSize = new Point(maskWidth, maskHeight);
            var cutOut = new Rectangle(new Point(positionInSheet * drawnSize.X, 0), drawnSize);
            level.spriteBatch.Draw(texture, new Rectangle(drawnPos, drawnSize), cutOut, Color.White, 0f, new Vector2(maskWidth/2, maskHeight/2), isFlipped, 0);
        }

        public override void HandleInteractions()
        {
            var entities = level.entities;
            entities.ForEach(delegate (Entity other)
            {
                if (other == this)
                    return;

                if (other is Enemy)
                {
                    var enemy = (Enemy) other;
                    if (enemy.position.X > position.X - 64
                    && enemy.position.X < position.X + 64
                    && enemy.isAlive)
                    {
                        if (boundingBox.Intersects(enemy.boundingBox))
                        {
                            if (boundingBox.Bottom > enemy.boundingBox.Top && speed.Y > 0)
                            {
                                enemy.OnHit(this);
                            }
                        }
                    }
                }

            });
        }
    }
}