using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        public static Point gameResolution = new Point(256, 224);
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
        public RenderTarget2D tiles;
        public RenderTarget2D sprites;
        public RenderTarget2D hud;
        public static GraphicsDevice graphicsDevice;

        public SMW()
        {
            new CatTimer().Start(60, () =>
            {
                long memory = GC.GetTotalMemory(true);
                Console.WriteLine((memory / 1024f));
            }, 0);
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Assets";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            base.Initialize();
        }

        public delegate void Del();
        Dictionary<RenderTarget2D, Del> renderLayers = new Dictionary<RenderTarget2D, Del>();
        protected override void LoadContent()
        {
            graphicsDevice = this.GraphicsDevice;
            gameScale = 3;
            Window.Title = "Super Mario C# Framework";

            var tiles = new RenderTarget2D(GraphicsDevice, SMW.gameResolution.X, SMW.gameResolution.Y);
            var sprites = new RenderTarget2D(GraphicsDevice, SMW.gameResolution.X, SMW.gameResolution.Y);
            var hud = new RenderTarget2D(GraphicsDevice, SMW.gameResolution.X, SMW.gameResolution.Y);

            renderLayers.Add(tiles, delegate()
            {
                GraphicsDevice.SetRenderTarget(tiles);
                _spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null, level.camera);
                GraphicsDevice.Clear(Color.BlanchedAlmond);
                level.DrawBackground();
                level.DrawTiles();
                _spriteBatch.End();
                GraphicsDevice.SetRenderTarget(null);
            });
            renderLayers.Add(sprites, delegate()
            {
                GraphicsDevice.SetRenderTarget(sprites);
                GraphicsDevice.Clear(Color.Transparent);
                _spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null, level.camera);
                level.DrawEntities();
                _spriteBatch.End();
                GraphicsDevice.SetRenderTarget(null);
            });
            renderLayers.Add(hud, delegate ()
            {
                GraphicsDevice.SetRenderTarget(hud);
                GraphicsDevice.Clear(Color.Transparent);
                _spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null);
                level.DrawHUD();
                _spriteBatch.End();
                GraphicsDevice.SetRenderTarget(null);
            });

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

            if (SMW.KeyboardState.WasKeyJustUp(Keys.R))
            {
                Console.WriteLine(DateTime.Now.Ticks);
                level = new Level(_spriteBatch, GraphicsDevice, Content);
                Console.WriteLine(DateTime.Now.Ticks);
            }

            if (SMW.KeyboardState.WasKeyJustUp(Keys.P))
                gameScale ++;
            if (SMW.KeyboardState.WasKeyJustUp(Keys.O))
                if (gameScale > 1)
                    gameScale --;

            #region Updates

            level.update(gameTime);
            CatTimer.Update();
            base.Update(gameTime);

            #endregion

        }

        

        protected override void Draw(GameTime gameTime)
        {
            // Clear screen buffer
            GraphicsDevice.Clear(Color.Black);

            // Draw each set of elements to their specified render target
            foreach (var layer in renderLayers.Keys)
            {
                renderLayers[layer]();
            }

            _spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointClamp);
            // Take all of the drawn elements & draw them to the screen at their proper size
            foreach (var layer in renderLayers.Keys)
            {
                _spriteBatch.Draw(layer, new Rectangle(Point.Zero, new Point(SMW.gameResolution.X * gameScale, gameResolution.Y * gameScale)), Color.White);
            }
            _spriteBatch.End();

            // Draw call
            base.Draw(gameTime);
        }

        // Graphic loading/caching system
        public static Dictionary<int, Texture2D> tileMapTextures = new Dictionary<int, Texture2D>();
        public static Dictionary<string, Texture2D> spriteTextures = new Dictionary<string, Texture2D>();
        public static Texture2D Load(string directory)
        {
            var realDir = "Assets/Sprites/" + directory + ".png";
            Texture2D ret;
            var added = spriteTextures.TryGetValue(directory, out ret);
            if (!added)
            {
                Console.WriteLine("Loading " + realDir);
                Stream fileStream = File.OpenRead(realDir);
                ret = Texture2D.FromStream(SMW.graphicsDevice, fileStream);
                fileStream.Close();
                spriteTextures.Add(directory, ret);
                Console.WriteLine("[✓] Loaded " + realDir);
            }
            return ret;
        }

    }
}
