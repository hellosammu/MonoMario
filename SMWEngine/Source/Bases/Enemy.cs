using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;

namespace SMWEngine.Source
{
    public class Enemy : CatEntity
    {
        public bool isAlive = true;
        public bool isHittable = true;

        public Enemy(Vector2 position)
        {
            depth = 0;
            this.position = position;
            animList = new Dictionary<string, List<double>>
            {
                ["walk"] = new List<double> {0, 1},
            };
            curAnim = "walk";
            speed.X = 0.5f;
            spriteHeight = 16;
            spriteWidth = 16;
            topClip = 4;
            rightClip = 2;
            leftClip = 2;
            texture = SMW.Load("Entities/Goomba");
            imgSpeed = 0.125f;
        }

        public override void EarlyUpdate()
        {

        }

        public override void Update()
        {
            HandleInteractions();
            var myBB = boundingBox;
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
            flipX = (speed.X < 0) ? true : false;
            if (boundingBox.Y >= level.levelSize.Y*16)
                Level.Remove(this);
        }

        public override void LateUpdate()
        {

        }

        public override void HandleInteractions()
        {
            var entities = level.entities;
            entities.ForEach(delegate(CatEntity other)
            {
                // Return if interacting with self
                if (other.Equals((CatEntity) this))
                    return;

                // If the entity you are interacting with is an enemy, collide with them
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
                if (speed.X <= 0 && enemy.speed.X > 0)
                {
                    if (myBB.Right >= enemyBB.Left)
                    {
                        speed.X = -speed.X;
                        enemy.speed.X = -enemy.speed.X;
                        Console.WriteLine("UWU");
                    }
                }
                else if (speed.X >= 0 && enemy.speed.X < 0)
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
            DrawSprite(texture, position.X, position.Y+1, new Vector2(0.5f, 0.5f), spriteCutOut);
        }

        public virtual void PlayerInteraction(Player player)
        {
            if (player.boundingBox.Bottom > boundingBox.Top && player.speed.Y > 0)
                OnHit(player);
        }

        protected virtual void OnHit(Player player)
        {
            if (!isHittable)
                return;

            isHittable = false;
            if (!player.isSpinning)
            {
                OnJump(player);
            }
            else
            {
                OnSpin(player);
            }
            new CatTimer().Start(8, () => isHittable = true);
        }

        protected virtual void OnJump(Player player)
        {
            player.speed.Y = -5.5f;
            player.isJumping = false;
            Level.Add(new Impact()
            {
                position = new Vector2(player.boundingBox.Center.X - 8, player.boundingBox.Bottom - 16)
            });
            Level.Remove(this);

        }

        protected virtual void OnSpin(Player player)
        {
            player.speed.Y = -0.925f;
            Level.Add(new Smoke()
            {
                position = position
            });
            player.isJumping = false;
            Level.Add(new Impact()
            {
                position = new Vector2(player.boundingBox.Center.X - 8, player.boundingBox.Bottom - 16)
            });
            Level.Remove(this);
        }
    }
}