using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace SMWEngine.Source
{
    public class Player : CatEntity
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

        bool inputRight => SMW.Input.Pressed(Keys.Right);
        bool inputLeft => SMW.Input.Pressed(Keys.Left);
        bool inputUp => SMW.Input.Pressed(Keys.Up);
        bool inputDown => SMW.Input.Pressed(Keys.Down);
        bool inputRun => SMW.Input.Pressed(Keys.Z);
        bool inputJump => SMW.Input.Pressed(Keys.X) || SMW.Input.Pressed(Keys.C);
        bool inputJumpPressed => SMW.Input.JustPressed(Keys.X);
        bool inputSpinPressed => SMW.Input.JustPressed(Keys.C);

        public Player(Vector2 position)
        {
            depth = -1;
            this.position = position;
            this.velocity.Y = 0.1875f;
            curAnim = "idle";
            animList = new Dictionary<string, List<double>>
            {
                ["idle"] = new List<double> { 0, 1 },
                ["walk"] = new List<double> { 3, 0 },
                ["crouch"] = new List<double> { 2 },
                ["swim"] = new List<double> { 13, 14, 15 },
                ["idle hold"] = new List<double> { 7, 8 },
                ["walk hold"] = new List<double> { 10, 7 },
                ["crouch hold"] = new List<double> { 9 },
                ["swim hold"] = new List<double> { 16, 17, 18 },
                ["run"] = new List<double> { 5, 4 },
                ["jump"] = new List<double> { 11, 12 },
                ["skid"] = new List<double> { 6 },
                ["run jump"] = new List<double> { 15 },
                ["spin"] = new List<double> { 0, 19, 0.5, 20 },
                ["slide"] = new List<double> { 21 },
                ["kick"] = new List<double> { 22 }
            };
            spriteHeight = spriteWidth = 32;
            rightClip = 10;
            leftClip = 10;
            topClip = 16;
            texture = SMW.Load("Entities/Mario");
        }

        // Called before normal update
        public override void EarlyUpdate()
        {

        }

        // Main update
        CatTimer skidCatTimer = new CatTimer();
        public override void Update()
        {

            if (SMW.Input.AnyJustPressed(new Keys[] { Keys.W, Keys.N }))
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

            velocity.Y = (inputJump) ? 0.1875f : 0.375f;
            var turning = ((speed.X > 0 && inputLeft && !inputRight) || (speed.X < 0 && inputRight)) ? true : false;
            var accelVar = (turning) ? 0.15625f : 0.09375f;
            var spdCap = (inputRun) ? ((isRunning) ? 3 : 2.25f) : 1.25f;
            var slipMod = 1;

            if ((turning || isSliding || isCrouching) && isGrounded && speed.X != 0)
            {
                if (skidCatTimer.timeLeft == 0)
                {
                    skidCatTimer = new CatTimer().Start(5);
                    var skidSmoke = new SkidSmoke(new Vector2(boundingBox.Center.X - 4, boundingBox.Bottom - 4));
                    skidSmoke.depth = -2;
                    Level.Add(skidSmoke);
                }
            }

            if (inputRight && (!isCrouching || !isGrounded))
            {
                flipX = false;
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
                flipX = true;
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
                if (Math.Abs(speed.X) >= 2.25f && isGrounded && !isCrouching)
                    pmeter += 2;

            if (!inputRun || (!inputLeft && !inputRight) || (isCrouching && isGrounded) || (Math.Abs(speed.X) < 2.25f))
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
                        flipX = (rand == 1) ? true : false;
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
                position.Y = (level.levelSize.Y * 16)+16;

        }

        // Called after main update
        public override void LateUpdate()
        {

        }


        public override void Draw()
        {
            DrawSprite(texture, (int) position.X, (int) position.Y + 1, new Vector2(0.5f, 0.5f), spriteCutOut);
        }

        public override void HandleInteractions()
        {
            var entities = level.entities;
            entities.ForEach(delegate (CatEntity other)
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
                            enemy.PlayerInteraction(this);
                        }
                    }
                }

            });
        }
    }
}