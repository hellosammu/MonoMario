using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace SMWEngine.Source
{
    public abstract class Effect : CatEntity
    {

    }

    public class Impact : Effect
    {

        public Impact()
        {
            animList = new Dictionary<string, List<double>>
            {
                ["impact"] = new List<double> { 0, 1, 0, 1 },
            };
            curAnim = "impact";
            texture = SMW.Load("Effects/Impact");
            imgSpeed = 0.5f;
            spriteHeight = spriteWidth = 16;
            depth = -2;
        }

        public override void Draw()
        {
            DrawSprite(texture, (int) position.X, (int) position.Y, Vector2.Zero, spriteCutOut);
        }

        public override void EarlyUpdate()
        {

        }

        public override void HandleInteractions()
        {

        }

        public override void LateUpdate()
        {

        }

        public override void Update()
        {
            if (curImage >= animList[curAnim].Count - 1)
                if (level.entities.Contains(this))
                    Level.Remove(this);
        }
    }

    public class SkidSmoke : Effect
    {
        public SkidSmoke() => Create(Vector2.Zero);
        public SkidSmoke(Vector2 position) => Create(position);
        private void Create(Vector2 position)
        {
            animList = new Dictionary<string, List<double>>
            {
                ["smoke"] = new List<double> { 0, 1, 2 },
            };
            texture = SMW.Load("Effects/Skid");
            curAnim = "smoke";
            spriteHeight = spriteWidth = 8;
            imgSpeed = 0.25f;
            this.position = position;
        }

        public override void Draw()
        {
            var posRect = new Rectangle((int)position.X, (int)position.Y, spriteWidth, spriteHeight);
            var imgIndexReal = (int)Math.Floor(curImage);
            var cutOutRect = new Rectangle(imgIndexReal * spriteWidth, 0, spriteWidth, spriteHeight);
            DrawSprite(texture, (int) position.X, (int) position.Y, Vector2.Zero, cutOutRect);
        }

        public override void Update()
        {
            if (curImage >= animList[curAnim].Count - 1)
                if (level.entities.Contains(this))
                    Level.Remove(this);
        }

        public override void EarlyUpdate()
        {

        }

        public override void HandleInteractions()
        {

        }

        public override void LateUpdate()
        {

        }

    }

    public class Star : Effect
    {
        private CatTimer timer;

        public Star()
        {
            texture = SMW.Load("Effects/Star");
            timer = new CatTimer().Start(15, delegate()
            {
                Destroy();
            });
        }

        public override void Draw()
        {
            var xTensity = 2.25f;
            var yTensity = 1.125f;
            var distOut = Math.Clamp(timer.elapsedTime, 0, 10);
            DrawSprite(texture, (int) (position.X + (distOut * xTensity)), (int) (position.Y + (distOut * yTensity)), new Vector2(0.5f, 0.5f), Rectangle.Empty);
            DrawSprite(texture, (int) (position.X - (distOut * xTensity)), (int) (position.Y - (distOut * yTensity)), new Vector2(0.5f, 0.5f), Rectangle.Empty);
            DrawSprite(texture, (int) (position.X - (distOut * xTensity)), (int) (position.Y + (distOut * yTensity)), new Vector2(0.5f, 0.5f), Rectangle.Empty);
            DrawSprite(texture, (int) (position.X + (distOut * xTensity)), (int) (position.Y - (distOut * yTensity)), new Vector2(0.5f, 0.5f), Rectangle.Empty);
        }

        public override void EarlyUpdate()
        {

        }

        public override void HandleInteractions()
        {

        }

        public override void LateUpdate()
        {

        }

        public override void Update()
        {
        }
    }

    public class Smoke : Effect
    {
        private Star star;
        public Smoke()
        {
            depth = 1;
            spriteHeight = spriteWidth = 16;
            texture = SMW.Load("Effects/SpinSmoke");
            animList = new Dictionary<string, List<double>>
            {
                ["smoke"] = new List<double> { 0, 0, 1, 1, 2, 3, 4 },
            };
            curAnim = "smoke";
            imgSpeed = 0.25f;
            Level.Add(star = new Star());
        }

        public override void Draw()
        {
            DrawSprite(texture, (int) position.X, (int) position.Y, Vector2.Zero, spriteCutOut);
        }

        public override void EarlyUpdate()
        {

        }

        public override void HandleInteractions()
        {

        }

        public override void LateUpdate()
        {
            star.position = new Vector2(boundingBox.Center.X, boundingBox.Center.Y);
        }

        public override void Update()
        {
            if (curImage >= animList[curAnim].Count-1)
                if (level.entities.Contains(this))
                    Level.Remove(this);
        }
    }

}