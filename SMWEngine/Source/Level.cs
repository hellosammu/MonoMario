using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using FontStashSharp;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace SMWEngine.Source
{
    public class Level : Basic
    {
        public HUD hud;
        public Matrix camera;
        public Point camPos = Point.Zero;
        public SMW game;
        public List<Entity> entities;
        public Point levelSize = new Point(256, 15);

        public SpriteBatch spriteBatch;
        public FontSystem fontSystem;
        public ContentManager Content;

        Texture2D bgTexture;

        public static Dictionary<int, Texture2D> tileMapTextures = new Dictionary<int, Texture2D>();
        public static Dictionary<string, Texture2D> spriteTextures = new Dictionary<string, Texture2D>();

        private Player player;
        public Tile[,] tiles;
        public Background background;
        public bool frozen = false;

        public Rectangle cameraBounds
        {
            get
            {
                var _l = MathHelper.Clamp(camPos.X, 0, (levelSize.X * 16) - 256);
                return new Rectangle(_l, -32, 256, (levelSize.Y*16) + 64);
            }
        }

        public Level(SpriteBatch spriteBatch, GraphicsDevice graphicsDevice, ContentManager Content)
        {
            this.spriteBatch = spriteBatch;
            this.Content = Content;
            entities = new List<Entity>();
            hud = new HUD { level = this };

            bgTexture = Load("Backgrounds/Hills");

            LoadLevel();

            player = new Player(new Vector2(16 * 2, 16 * 10));
            Add(player);

            var e = new Enemy(Vector2.Zero);
            Add(e);
            var e2 = new Enemy(new Vector2(64, 0));
            Add(e2);
        }

        public void LoadLevel()
        {
            var srcTex = "";
            // Load level XML data, populate a XDocument with parsed information
            var xmlParse = XDocument.Load("Assets/Levels/Level.tmx");

            // Iterate to the "map" element found in the tileset, it will be used as a head pointer
            // (Everything we need is located in the hierarchy of this element)
            var mainMap = xmlParse.Element("map");

            // Grab information about the size of the room
            levelSize.X = (int) mainMap.Attribute("width");
            levelSize.Y = (int)mainMap.Attribute("height");

            // Create tileset of room size
            tiles = new Tile[levelSize.X, levelSize.Y];

            // Clear the cache of the previous level's tiles
            tileMapTextures.Clear();

            // Create list that will hold layer information
            List<string> layerData = new List<string>();

            // Dictionary full of different collider values (this is a tile's 'type' parameter - I use it to define a collision type)
            // It lines up with an enum's numerical value
            var colliderDictionary = new Dictionary<int, int>();

            // Iterate through every element in the root node
            foreach (var element in mainMap.Elements())
            {
                // If the element is a layer, grab all of the layer information and put it inside of the layer data
                if (element.Name == "layer")
                {
                    var node = element.Element("data").FirstNode;
                    layerData.Add(node.ToString().TrimStart());
                }
                // If the element is an object, grab all of the object information
                else if (element.Name == "objectgroup")
                {
                    var objects = element.Elements();
                    foreach (var obj in objects)
                    {
                        // TODO: Make actually work!
                        /*var objName = (string) obj.Attribute("name");
                        Vector2 objPos = new Vector2((int) obj.Attribute("x"), (int) obj.Attribute("y"));
                        Type objType = Type.GetType();
                        Console.WriteLine(objType);
                        var entity = Activator.CreateInstance(objType);
                        var realEntity = (Entity) entity;
                        realEntity.position = objPos;
                        Add((Entity) entity);*/
                    }
                }
                // Get tileset information
                else if (element.Name == "tileset")
                {
                    // Tileset texture (.png file location)
                    var src = (string) element.Attribute("source");

                    // Put the PNG file name in a string array, split from where the '.PNG' will supposedly be
                    // This step is absolutely NOT necessary and will not work with most project hierarchies.
                    // What you want to do is use the tile's source to then find the PNG inside of the .tsx
                    // (See "Load in the .tsx file", you're looking for the "source" attribute inside of the "image" element)
                    string[] srcArr = src.Split(".");
                    srcTex = srcArr[0].Trim();
                    Texture2D tlsTexture = Load("Tilesets/"+srcTex);

                    // Get the initial spot that the tileset numbers start at (1 for first tileset, then after, -
                    // - it will be whichever tile the next tileset would start at (the length of the previous tileset))
                    var gid = (int) element.Attribute("firstgid");

                    // Remember this, we'll use it to grab the correct tile data
                    tileMapTextures.Add(gid, tlsTexture);

                    // Load in the .tsx file
                    var tlsXML = XDocument.Load("Assets/Levels/"+src.Trim());

                    // Root node everything else will be underneath
                    var tlsFrontElement = tlsXML.Element("tileset");

                    // Get every element that is under the root node
                    var allTLSElements = tlsFrontElement.Elements();

                    // Iterate through every element
                    foreach (var elementInTLS in allTLSElements)
                    {
                        // If the element is a tile, store its information
                        if (elementInTLS.Name == "tile")
                        {
                            var tileValue = (int)elementInTLS.Attribute("id");
                            var tileData = (int)elementInTLS.Attribute("type");
                            colliderDictionary.Add(tileValue, tileData);
                        }
                    }
                }
            }

            // Create background & assign BG texture in memory
            background = new Background();
            background.texture = bgTexture;

            // Populate string w/ level data
            layerData.ForEach(delegate (string layerDataIteration)
            {
                var listLinesY = layerDataIteration.Split("\n");
                for (int y = 0; y < listLinesY.Length; y ++)
                {
                    var listLinesX = listLinesY[y].Split(",");
                    for (int x = 0; x < listLinesX.Length; x ++)
                    {
                        var tileGraphicPosition = 0;
                        int.TryParse(listLinesX[x].Trim(), out tileGraphicPosition);
                        if (tileGraphicPosition != 0) {
                            // Create tile & assign ground texture in memory
                            tiles[x, y] = new Tile();
                            Texture2D tex;
                            var keys = tileMapTextures.Keys;
                            List<int> keyVals = new List<int>();
                            var myIteration = tileGraphicPosition;
                            var keyArr = keys.ToArray();
                            for (int i = 0; i < keyArr.Length; i ++)
                            {
                                if (tileGraphicPosition > keyArr[i])
                                {
                                    myIteration = keyArr[i];
                                    if (i+1 < keyArr.Length)
                                    {
                                        if (keyArr[i+1] > tileGraphicPosition)
                                            myIteration = keyArr[i];
                                    }
                                }
                            }
                            var keyValArr = keyVals.ToArray();
                            var test = tileMapTextures.TryGetValue(myIteration, out tex);
                            tiles[x, y].texture = tex;

                            // Get proper graphic
                            var xLeftOver = (tileGraphicPosition - myIteration) % (tex.Width / 16);
                            var yLeftOver = (int) Math.Floor((float) (tileGraphicPosition - myIteration) / (tex.Width / 16));
                            tiles[x, y].textureLocation = new Point(xLeftOver, yLeftOver);

                            // Find collider type from collision dictionary
                            int colType = -1;
                            if (myIteration == 1)
                            colliderDictionary.TryGetValue(tileGraphicPosition - 1, out colType);
                            tiles[x, y].colliderType = (ColliderType) colType;
                        }
                    }
                }
            });
        }

        public static Texture2D Load(string directory)
        {
            var realDir = "Assets/Sprites/"+directory+".png";
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

        static List<Entity> removeList = new List<Entity>();
        static List<Entity> addList = new List<Entity>();
        public void update(GameTime gameTime)
        {
            if (!frozen)
            {
                if (entities.Count > 0)
                {
                    entities.ForEach(delegate(Entity entity)
                    {
                        if (entity is Enemy)
                        {
                            if (!((entity.boundingBox.Right > cameraBounds.Left-32)
                            && (entity.boundingBox.Left < cameraBounds.Right + 32)))
                            {
                                return;
                            }
                        }
                        entity.EarlyUpdate();
                    });
                    entities.ForEach(delegate (Entity entity)
                    {
                        if (entity is Enemy)
                        {
                            if (!((entity.boundingBox.Right > cameraBounds.Left - 32)
                            && (entity.boundingBox.Left < cameraBounds.Right + 32)))
                            {
                                return;
                            }
                        }
                        entity.Update();
                    });
                    entities.ForEach(delegate (Entity entity)
                    {
                        if (entity is Enemy)
                        {
                            if (!((entity.boundingBox.Right > cameraBounds.Left - 32)
                            && (entity.boundingBox.Left < cameraBounds.Right + 32)))
                            {
                                return;
                            }
                        }
                        entity.LateUpdate();
                        entity.UpdateAnimations();

                    });
                }
            }

            // Static function wrappers
            if (removeList.Count > 0)
            {
                removeList.ToList().ForEach(delegate (Entity deadEntity)
                {
                    if (entities.Contains(deadEntity))
                        entities.Remove(deadEntity);
                });
                Sort();
            }
            if (addList.Count > 0)
            {
                addList.ToList().ForEach(delegate (Entity newEntity)
                {
                    if (!entities.Contains(newEntity)) {
                        entities.Add(newEntity);
                        newEntity.level = this;
                    }
                });
                Sort();
            }
            removeList.Clear();
            addList.Clear();

            // CAM STUFF
            var center = player.boundingBox.Center.X;
            if (center < xMod - 40)
            {
                xSide = -1;
            }
            else if (center > xMod + 40)
            {
                xSide = 1;
            }

            if ((xSide == 1) && (center > xMod - 16))
            {
                var spdMod = ((player.speed.X > 0) ? player.speed.X : 0);
                if (center > xMod - (16 - 2 - spdMod))
                    xMod += spdMod + 2;
                else
                    xMod = center + 16;
            }
            else if ((xSide == -1) && (center < xMod + 16))
            {
                var spdMod = ((player.speed.X < 0) ? player.speed.X : 0);
                if (center < xMod + (16 - 2 + spdMod))
                    xMod -= 2 - spdMod;
                else
                    xMod = center - 16;
            }

            camPos.X = (int) Math.Floor(rawCamPos);
            var _x = MathHelper.Clamp(camPos.X, 0, (levelSize.X * 16) - 256);
            camera = Matrix.CreateTranslation(new Vector3(-_x, 0, 0));

        }
        public int xSide = 0;
        public float xMod = 128;
        public float rawCamPos
        {
            get => (float) xMod - 128;
            set => rawCamPos = value;
        }

        public static void Add(Entity addEntity)
        {
            if (!addList.Contains(addEntity))
                addList.Add(addEntity);
        }

        public static void Remove(Entity deadEntity)
        {
            if (!removeList.Contains(deadEntity))
                removeList.Add(deadEntity);
        }
        private void Sort()
        {
            entities = entities.OrderBy(o => -o.depth).ToList();
        }

        public void DrawBackground()
        {
            Rectangle bgRect = Rectangle.Empty;
            for (int i = 0; i < Math.Ceiling(((levelSize.X / (background.texture.Width / 16f)) / 2)); i ++)
            {
                var _x = (camPos.X / 2) + (i * background.texture.Width);
                var clampedX = MathHelper.Clamp(_x, 0+(background.texture.Width*i), (levelSize.X * 16) - 128 - (background.texture.Width * i));
                //var clampedX = MathHelper.Clamp(((player.position.X-120) / parallaxIntensity)/* + (i * background.texture.Width)*/, 0, (levelSize.X * 16) - 256);
                bgRect = new Rectangle((int) clampedX, -208, background.texture.Width, background.texture.Height);
                spriteBatch.Draw(background.texture, bgRect, Color.White);
            }
            bgRect.X += background.texture.Width;
            spriteBatch.Draw(background.texture, bgRect, Color.White);
        }

        public void DrawTiles()
        {
            float farLeft = MathHelper.Clamp((camPos.X / 16f), 0, (levelSize.X) - 256f/16f);
            // Tile
            for (int x = (int) Math.Floor(farLeft); x < (int) (Math.Ceiling(farLeft+(256f/16f))); x ++)
            {
                for (int y = 0; y < levelSize.Y; y ++)
                {
                    var myTile = tiles[x,y];
                    if (myTile != null)
                    {
                        var position = new Rectangle(x*16, y*16, 16, 16);
                        var cutOut = new Rectangle(myTile.textureLocation.X*16, myTile.textureLocation.Y*16, 16, 16);
                        spriteBatch.Draw(myTile.texture, position, cutOut, Color.White);
                    }

                }
            }
        }
        public void Draw()
        {
            // Entity
            entities.ForEach(delegate (Entity entity)
            {
                entity.Draw();
            });
        }

        public void DrawHUD()
        {
            hud.Draw();
        }
    }
}