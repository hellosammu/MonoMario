using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using SMWEngine.Source.Engine;

namespace SMWEngine.Source
{
    public class Level : CatSprite
    {

        public HUD hud;
        public Matrix camera;
        public Point camPos = Point.Zero;
        public SMW game;
        public List<CatEntity> entities;
        public Point levelSize;

        public SpriteBatch spriteBatch;
        public ContentManager Content;

        public CatText catText;

        private Player player;
        public Tile[,] tiles;
        public Background background;

        public Rectangle cameraBounds
        {
            get
            {
                var leftSide = MathHelper.Clamp(camPos.X, 0, (levelSize.X * 16) - SMW.gameResolution.X);
                return new Rectangle(leftSide, -32, SMW.gameResolution.X, (levelSize.Y*16) + 64);
            }
        }

        public Level(SpriteBatch spriteBatch, GraphicsDevice graphicsDevice, ContentManager Content)
        {
            this.spriteBatch = spriteBatch;
            this.Content = Content;
            entities = new List<CatEntity>();
            hud = new HUD { level = this };

            LoadLevel();

            player = new Player(new Vector2(16 * 2, 16 * 22));
            Add(player);

            // add some test enemies in
            /*var e = new Galoomba(Vector2.Zero);
            Add(e);
            var e2 = new Galoomba(new Vector2(64, 0));
            Add(e2);*/
            catText = new CatText(SMW.Load("HUD/Font"), this);
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
            levelSize.Y = (int) mainMap.Attribute("height");

            // Create tileset of room size
            tiles = new Tile[levelSize.X, levelSize.Y];

            // Clear the cache of the previous level's tiles
            SMW.tileMapTextures.Clear();

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
                        var atrName = (string) obj.Attribute("name");
                        Console.WriteLine(atrName);
                        Enemy enemy = new Enemy(Vector2.Zero);
                        switch (atrName.ToLower())
                        {
                            // Galoomba / default enemy
                            default:
                            case ("galoomba"):
                                enemy = new Galoomba(new Vector2((float) obj.Attribute("x"), (float) obj.Attribute("y") - 16));
                                break;
                            // Koopa
                            case ("koopa"):
                                break;
                        }
                        enemy.speed.X = -Math.Abs(enemy.speed.X);
                        Add(enemy);
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
                    Texture2D tlsTexture = SMW.Load("Tilesets/"+srcTex);

                    // Get the initial spot that the tileset numbers start at (1 for first tileset, then after, -
                    // - it will be whichever tile the next tileset would start at (the length of the previous tileset))
                    var gid = (int) element.Attribute("firstgid");

                    // Remember this, we'll use it to grab the correct tile data
                    SMW.tileMapTextures.Add(gid, tlsTexture);

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
            background.texture = SMW.Load("Backgrounds/Hills");

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
                            var keys = SMW.tileMapTextures.Keys;
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
                            var test = SMW.tileMapTextures.TryGetValue(myIteration, out tex);
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

        static List<CatEntity> removeList = new List<CatEntity>();
        static List<CatEntity> addList = new List<CatEntity>();
        public void update(float elapsed)
        {
            if (!SMW.frozen)
            {
                if (entities.Count > 0)
                {
                    entities.ForEach(delegate(CatEntity entity)
                    {
                        if (entity is Enemy)
                        {
                            if (!((entity.boundingBox.Right > cameraBounds.Left-32)
                            && (entity.boundingBox.Left < cameraBounds.Right + 32)))
                            {
                                return;
                            }
                        }
                        entity.positionLast = entity.position;
                        entity.EarlyUpdate(elapsed);
                    });
                    entities.ForEach(delegate (CatEntity entity)
                    {
                        if (entity is Enemy)
                        {
                            if (!((entity.boundingBox.Right > cameraBounds.Left - 32)
                            && (entity.boundingBox.Left < cameraBounds.Right + 32)))
                            {
                                return;
                            }
                        }
                        entity.Update(elapsed);
                    });
                    entities.ForEach(delegate (CatEntity entity)
                    {
                        if (entity is Enemy)
                        {
                            if (!((entity.boundingBox.Right > cameraBounds.Left - 32)
                            && (entity.boundingBox.Left < cameraBounds.Right + 32)))
                            {
                                return;
                            }
                        }
                        entity.LateUpdate(elapsed);
                        entity.UpdateAnimations(elapsed);
                    });
                }
            }

            // Static function wrappers
            if (removeList.Count > 0)
            {
                removeList.ToList().ForEach(delegate (CatEntity deadCatEntity)
                {
                    var disposeGraphic = true;
                    entities.ForEach(delegate (CatEntity entity)
                    {
                        if (entity != deadCatEntity)
                            if (entity.texture.Name == deadCatEntity.texture.Name)
                                disposeGraphic = false;
                    });
                    var daFuck = SMW.spriteTextures.FirstOrDefault(x => x.Value == deadCatEntity.texture).Key;
                    if (disposeGraphic)
                        SMW.spriteTextures.Remove(daFuck);
                    if (entities.Contains(deadCatEntity))
                        entities.Remove(deadCatEntity);
                });
                Sort();
            }
            if (addList.Count > 0)
            {
                addList.ToList().ForEach(delegate (CatEntity newCatEntity)
                {
                    if (!entities.Contains(newCatEntity)) {
                        entities.Add(newCatEntity);
                        newCatEntity.level = this;
                    }
                });
                Sort();
            }
            removeList.Clear();
            addList.Clear();

            // CAM STUFF

            #region X axis

            var centerX = player.boundingBox.Center.X;
            if (centerX < xMod - 40)
            {
                xSide = -1;
            }
            else if (centerX > xMod + 40)
            {
                xSide = 1;
            }

            if ((xSide == 1) && (centerX > xMod - 16))
            {
                var spdMod = ((player.speed.X > 0) ? player.speed.X : 0);
                if (centerX > xMod - (16 - 2 - spdMod))
                    xMod += (spdMod + 2) * (elapsed * SMW.multiplyFPS);
                else
                    xMod = centerX + 16;
            }
            else if ((xSide == -1) && (centerX < xMod + 16))
            {
                var spdMod = ((player.speed.X < 0) ? player.speed.X : 0);
                if (centerX < xMod + (16 - 2 + spdMod))
                    xMod -= (2 - spdMod) * (elapsed * SMW.multiplyFPS);
                else
                    xMod = centerX - 16;
            }

            #endregion

            #region Y Axis

            if (player.isGrounded)
            {
                yFloor = (int) player.position.Y;
            }

            if (player.isRunning && !player.isGrounded)
            {
                yMod += (player.position.Y - player.positionLast.Y);
            }
            else if (yMod < player.position.Y)
            {
                yMod = player.position.Y;
            }
            else
            {
                if (yMod > yFloor)
                {
                    yMod -= 4 * (elapsed * SMW.multiplyFPS);
                    if (yMod < yFloor)
                        yMod = yFloor;
                }
            }

            #endregion

            camPos.X = (int) Math.Floor(rawCamPosX);
            camPos.Y = (int) Math.Floor(rawCamPosY);
            var _x = MathHelper.Clamp(camPos.X, 0, (levelSize.X * 16) - SMW.gameResolution.X);
            var _y = MathHelper.Clamp(camPos.Y, 0, (levelSize.Y * 16) - SMW.gameResolution.Y - 16);
            camera = Matrix.CreateTranslation(new Vector3(-_x, -_y, 0));

        }
        public int xSide = 0;
        public float xMod = SMW.gameResolution.X/2;
        public float rawCamPosX
        {
            get => (float) xMod - SMW.gameResolution.X/2;
            set => rawCamPosX = value;
        }

        public int yFloor = 0;
        public float yMod = 112;
        public float rawCamPosY
        {
            get => (float) yMod - SMW.gameResolution.Y/2;
            set => rawCamPosY = value;
        }

        public static void Add(CatEntity addCatEntity)
        {
            if (!addList.Contains(addCatEntity))
                addList.Add(addCatEntity);
        }

        public static void Remove(CatEntity deadCatEntity)
        {
            if (!removeList.Contains(deadCatEntity))
                removeList.Add(deadCatEntity);
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
                var clampedX = MathHelper.Clamp(_x, 0+(background.texture.Width*i), (levelSize.X * 16) - SMW.gameResolution.X/2 - (background.texture.Width * i));
                bgRect = new Rectangle((int) clampedX, (levelSize.Y * 16) - background.texture.Height, background.texture.Width, background.texture.Height);
                spriteBatch.Draw(background.texture, bgRect, CatColor.WHITE);
            }
            bgRect.X += background.texture.Width;
            spriteBatch.Draw(background.texture, bgRect, CatColor.WHITE);
        }

        public void DrawTiles()
        {
            float farLeft = MathHelper.Clamp((camPos.X / 16f), 0, (levelSize.X) - SMW.gameResolution.X / 16f);
            // Tile
            for (int x = (int) Math.Floor(farLeft); x < (int) (Math.Ceiling(farLeft+(SMW.gameResolution.X / 16f))); x ++)
            {
                for (int y = 0; y < levelSize.Y; y ++)
                {
                    var myTile = tiles[x,y];
                    if (myTile != null)
                    {
                        var position = new Rectangle(x*16, y*16, 16, 16);
                        var cutOut = new Rectangle(myTile.textureLocation.X*16, myTile.textureLocation.Y*16, 16, 16);
                        spriteBatch.Draw(myTile.texture, position, cutOut, CatColor.WHITE);
                    }

                }
            }
        }
        public void DrawEntities()
        {
            foreach (var entity in entities)
                entity.Draw();
        }

        public void DrawHUD()
        {
            hud.Draw();
            catText.DrawTextUpper(8, 8, "CSHARP MARIO CLONE#" + ((int) (1f / SMW.elapsed)).ToString() + " FPS", 0.8f);
        }
    }
}