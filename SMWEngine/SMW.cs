using System;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended.Input;
using SMWEngine.Source;

namespace SMWEngine
{
    public class SMW : Game
    {
        public Point gameResolution = new Point(256, 224);
        public int gameScale
        {
            get
            {
                return _gameScale;
            }
            set
            {
                _gameScale = value;
                _graphics.PreferredBackBufferWidth = gameResolution.X * value;
                _graphics.PreferredBackBufferHeight = gameResolution.Y * value;
                _graphics.ApplyChanges();
            }

        }
        private int _gameScale;

        public GraphicsDeviceManager _graphics;
        public SpriteBatch _spriteBatch;
        public Level level;
        private bool letGo;
        public RenderTarget2D Layer1;
        public RenderTarget2D Layer2;
        public RenderTarget2D sprites;
        public RenderTarget2D HUD;
        public static GraphicsDevice graphicsDevice;

        public SMW()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Assets";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            // TODO: Add your initialization logic here

            base.Initialize();
        }

        protected override void LoadContent()
        {
            graphicsDevice = this.GraphicsDevice;
            gameScale = 3;
            Window.Title = "Super Mario C# Framework";

            Layer1 = new RenderTarget2D(GraphicsDevice, gameResolution.X, gameResolution.Y);
            Layer2 = new RenderTarget2D(GraphicsDevice, gameResolution.X, gameResolution.Y);
            sprites = new RenderTarget2D(GraphicsDevice, gameResolution.X, gameResolution.Y);
            HUD = new RenderTarget2D(GraphicsDevice, gameResolution.X, gameResolution.Y);

            _spriteBatch = new SpriteBatch(GraphicsDevice);
            level = new Level(_spriteBatch, GraphicsDevice, Content);
        }

        public static KeyboardStateExtended KeyboardState;

        Color colorInterp = Color.White;
        protected override void Update(GameTime gameTime)
        {
            // Global keyboard state
            SMW.KeyboardState = KeyboardExtended.GetState();

            if (SMW.KeyboardState.WasKeyJustUp(Keys.V))
            {
                if (colorInterp != Color.Black)
                    colorInterp = Color.Black;
                else
                    colorInterp = Color.Blue;
            }

            if (SMW.KeyboardState.WasKeyJustUp(Keys.R) && letGo)
            {
                Console.WriteLine(DateTime.Now.Ticks);
                level = new Level(_spriteBatch, GraphicsDevice, Content);
                letGo = false;
                Console.WriteLine(DateTime.Now.Ticks);
            }
            if (SMW.KeyboardState.WasKeyJustUp(Keys.P))
                gameScale ++;
            if (SMW.KeyboardState.WasKeyJustUp(Keys.O))
                if (gameScale > 1)
                    gameScale --;

            if (!KeyboardExtended.GetState().IsKeyDown(Keys.R))
                letGo = true;

            #region Updates

            level.update(gameTime);
            Timer.Update();
            base.Update(gameTime);
            Console.WriteLine(Timer.timers.Count);

            #endregion
        }

        

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);
            GraphicsDevice.SetRenderTarget(Layer1);
            _spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null, level.camera);
            GraphicsDevice.Clear(Color.BlanchedAlmond);
            level.DrawBackground();
            level.DrawTiles();
            _spriteBatch.End();

            GraphicsDevice.SetRenderTarget(sprites);
            GraphicsDevice.Clear(Color.Transparent);
            _spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null, level.camera);
            level.Draw();
            _spriteBatch.End();
            GraphicsDevice.SetRenderTarget(null);

            GraphicsDevice.SetRenderTarget(HUD);
            GraphicsDevice.Clear(Color.Transparent);
            _spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null);
            level.DrawHUD();
            _spriteBatch.End();
            GraphicsDevice.SetRenderTarget(null);

            _spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointClamp);
            col = Color.Lerp(col, colorInterp, 0.05f);
            _spriteBatch.Draw(Layer1, new Rectangle(Point.Zero, new Point(gameResolution.X * gameScale, gameResolution.Y * gameScale)), col);
            _spriteBatch.Draw(sprites, new Rectangle(Point.Zero, new Point(gameResolution.X * gameScale, gameResolution.Y * gameScale)), Color.White);
            _spriteBatch.Draw(HUD, new Rectangle(Point.Zero, new Point(gameResolution.X * gameScale, gameResolution.Y * gameScale)), Color.White);
            _spriteBatch.End();

            base.Draw(gameTime);
        }
        Color col = Color.White;
    }
}
