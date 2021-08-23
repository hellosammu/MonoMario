using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace SMWEngine.Source
{
    public abstract class Effect : Entity
    {

    }

    public class Impact : Effect
    {

        public Impact()
        {
            animList = new Dictionary<string, List<dynamic>>
            {
                ["impact"] = new List<dynamic> { 0, 1, 0, 1 },
            };
            curAnim = "impact";
            texture = Level.Load("Effects/Impact");
            imgSpeed = 0.5f;
            maskHeight = maskWidth = 16;
            depth = -2;
        }

        public override void Draw()
        {
            DrawSprite(texture, (int) position.X, (int) position.Y, Vector2.Zero, new Rectangle(new Point((int)animList[curAnim][(int)Math.Floor((float)Math.Abs((int)Math.Floor(curImage)))] * maskWidth, 0), new Point(maskWidth, maskHeight)), SpriteEffects.None);
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
            animList = new Dictionary<string, List<dynamic>>
            {
                ["smoke"] = new List<dynamic> { 0, 1, 2 },
            };
            texture = Level.Load("Effects/Skid");
            curAnim = "smoke";
            maskHeight = maskWidth = 8;
            imgSpeed = 0.25f;
            this.position = position;
        }

        public override void Draw()
        {
            var posRect = new Rectangle((int)position.X, (int)position.Y, maskWidth, maskHeight);
            var imgIndexReal = (int)Math.Floor(curImage);
            var cutOutRect = new Rectangle(imgIndexReal * maskWidth, 0, maskWidth, maskHeight);
            level.spriteBatch.Draw(texture, posRect, cutOutRect, Color.White);
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
        private float distOut = 0;
        public Star()
        {
            texture = Level.Load("Effects/Star");
            new Timer().Start(15, delegate()
            {
                Level.Remove(this);
            });
        }

        public override void Draw()
        {
            var xTensity = 1.5f;
            var yTensity = 0.75f;
            DrawSprite(texture, (int) (position.X + (distOut * xTensity)), (int) (position.Y + (distOut * yTensity)), new Vector2(0.5f, 0.5f), null, SpriteEffects.None);
            DrawSprite(texture, (int) (position.X - (distOut * xTensity)), (int) (position.Y - (distOut * yTensity)), new Vector2(0.5f, 0.5f), null, SpriteEffects.None);
            DrawSprite(texture, (int) (position.X - (distOut * xTensity)), (int) (position.Y + (distOut * yTensity)), new Vector2(0.5f, 0.5f), null, SpriteEffects.None);
            DrawSprite(texture, (int) (position.X + (distOut * xTensity)), (int) (position.Y - (distOut * yTensity)), new Vector2(0.5f, 0.5f), null, SpriteEffects.None);
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
            if (distOut < 15)
                distOut += 1.5f;
        }
    }

    public class Smoke : Effect
    {
        private Star star;
        public Smoke()
        {
            depth = 1;
            maskHeight = maskWidth = 16;
            texture = Level.Load("Effects/SpinSmoke");
            animList = new Dictionary<string, List<dynamic>>
            {
                ["smoke"] = new List<dynamic> { 0, 0, 1, 1, 2, 3, 4 },
            };
            curAnim = "smoke";
            imgSpeed = 0.25f;
            Level.Add(star = new Star());
        }

        public override void Draw()
        {
            var posRect = new Rectangle((int) position.X, (int) position.Y, maskWidth, maskHeight);
            var imgIndexReal = (int) Math.Floor(curImage);
            var cutOutRect = new Rectangle(imgIndexReal * maskWidth, 0, maskWidth, maskHeight);
            level.spriteBatch.Draw(texture, posRect, cutOutRect, Color.White);
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