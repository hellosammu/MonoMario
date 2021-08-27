using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;

namespace SMWEngine.Source.Engine
{
    public class CatText
    {
        private Texture2D fontGraphic;
        private String order;
        private Level level;

        public CatText(Texture2D texture, Level level, String order = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ.,*-!@&;='")
        {
            this.fontGraphic = texture;
            this.order = order;
            this.level = level;
        }

        public void DrawText(UInt16 x, UInt16 y, String str, float alpha = 1)
        {
            String substr;
            int y_offset = 0;
            int x_offset = 0;

            for (int i = 0; i < str.Length; i++)
            {
                substr = str.Substring(i, 1);

                if (substr.Equals("#"))
                {
                    x_offset = 0;
                    y_offset++;
                }
                else
                {
                    if (!substr.Equals(" "))
                        level.spriteBatch.Draw(fontGraphic, new Rectangle(x + (x_offset * 8), y + (y_offset * 8), 8, 8), new Rectangle(GetText(substr) * 8, 0, 8, 8), CatColor.WHITE * alpha);

                    x_offset++;
                }
            }
        }

        public void DrawTextUpper(UInt16 x, UInt16 y, String str, float alpha = 1)
        {
            DrawText(x, y, str.ToUpper(), alpha);
        }

        private int GetText(string text)
        {
            int ret = 0;
            ret = order.IndexOf(text);
            return ret;
        }
    }
}
