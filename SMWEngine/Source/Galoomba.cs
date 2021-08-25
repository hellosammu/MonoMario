using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace SMWEngine.Source
{
    public class Galoomba : Enemy
    {
        public bool isFlipped = false;
        public Galoomba(Vector2 position) : base(position)
        {
            animList = new Dictionary<string, List<double>>
            {
                ["walk"] = new List<double> { 0, 1 },
                ["struggle"] = new List<double> { 2, 3 }
            };
            curAnim = "walk";
        }

        public override void Update()
        {
            base.Update();
        }

        protected override void OnJump(Player player)
        {
            speed.X = 0;
            imgSpeed = 0.125f;
            isFlipped = true;
            curAnim = "struggle";

            player.speed.Y = -5.5f;
            player.isJumping = false;
            Level.Add(new Impact()
            {
                position = new Vector2(player.boundingBox.Center.X - 8, player.boundingBox.Bottom - 16)
            });
        }

    }
}