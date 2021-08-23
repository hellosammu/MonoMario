using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;

namespace SMWEngine.Source
{
    public class Enemy : Entity
    {
        public bool isAlive = true;

        public Enemy(Vector2 position)
        {
            depth = 0;
            this.position = position;
            animList = new Dictionary<string, List<dynamic>> {

                ["walk"] = new List<dynamic>{0, 1},

            };
            curAnim = "walk";
            speed.X = 0.5f;
            maskHeight = 16;
            maskWidth = 16;
            topClip = 4;
            rightClip = 2;
            leftClip = 2;
            texture = Level.Load("Entities/Goomba");
        }

        public override void EarlyUpdate()
        {

        }

        public override void Update()
        {
            HandleInteractions();
            var myBB = boundingBox;
            imgSpeed = Math.Abs(speed.X / 4f);
            velocity.Y = 0.1875f;
            speed += velocity;
            if (speed.Y > 4)
                speed.Y = 4;
            position += speed;
            var speedLast = speed;
            if (isGrounded)
                speed.Y = 0;
            HandleCollisions();
            if (atWall && speed.X == 0)
                speed.X = -speedLast.X;
            flipX = (speed.X < 0) ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
            if (boundingBox.Y >= level.levelSize.Y*16)
                Level.Remove(this);
        }

        public override void LateUpdate()
        {

        }

        public override void HandleInteractions()
        {
            var entities = level.entities;
            entities.ForEach(delegate(Entity other)
            {
                if (other is Enemy)
                {
                    var enemy = (Enemy) other;
                    if (enemy.position.X > position.X - 64
                    && enemy.position.X < position.X + 64
                    && enemy.isAlive)
                    {
                        HandleEnemyInteractions(enemy);
                    }
                }
            });
        }

        private void HandleEnemyInteractions(Enemy enemy)
        {
            var myBB = boundingBox;
            var enemyBB = enemy.boundingBox;
            #region Flip
            if (myBB.Intersects(enemyBB))
            {
                if (speed.X <= 0 && enemy.speed.X >= 0)
                {
                    if (myBB.Right >= enemyBB.Left)
                    {
                        speed.X = -speed.X;
                        enemy.speed.X = -enemy.speed.X;
                    }
                }
                else if (speed.X >= 0 && enemy.speed.X <= 0)
                {
                    if (myBB.Left >= enemyBB.Right)
                    {
                        speed.X = -speed.X;
                        enemy.speed.X = -enemy.speed.X;
                    }
                }
            }
            #endregion
        }

        public override void Draw()
        {
            var isFlipped = flipX;
            var positionRect = new Rectangle(new Point((int)Math.Floor(position.X), (int)Math.Floor(position.Y + 1)), new Point(maskWidth, maskHeight));
            var cutOut = new Rectangle(new Point((int)animList[curAnim][(int)Math.Floor((float)Math.Abs((int)Math.Floor(curImage)))] * maskWidth, 0), new Point(maskWidth, maskHeight));

            if (animList[curAnim][Math.Abs((int)Math.Floor(curImage))] != Math.Floor((float)animList[curAnim][Math.Abs((int)Math.Floor(curImage))]))
                isFlipped = (flipX == SpriteEffects.FlipHorizontally) ? SpriteEffects.None : SpriteEffects.FlipHorizontally;

            level.spriteBatch.Draw(texture, positionRect, cutOut, Color.White, 0f, Vector2.Zero, isFlipped, 0);
        }

        internal void OnHit(Player player)
        {
            Level.Remove(this);
            if (!player.isSpinning)
                player.speed.Y = -5.5f;
            else
            {
                player.speed.Y = -0.925f;
                var smoke = new Smoke() { position = position };
                Level.Add(smoke);
            }
            player.isJumping = false;
            var impact = new Impact() { position = new Vector2(player.boundingBox.Center.X - 8, player.boundingBox.Bottom - 16) };
            Level.Add(impact);
        }
    }
}