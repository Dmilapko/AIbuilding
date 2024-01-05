using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MonoHelper;
using Microsoft.Xna.Framework;
using AIbuilding;
using System.Threading;
using System.Net;
using System.Xml.Linq;
using System.Diagnostics;
using System.IO;

namespace AIbuilding
{
    internal class MapTile:AbstractTile
    {
        public MapTile(int level, Microsoft.Xna.Framework.Point pos, string folder) : base(level, pos, folder)
        {
        }

        public override string TileFile()
        {
            return "tile_" + level.ToString() + "_" + pos.X.ToString() + "_" + pos.Y.ToString() + ".png";
        }

        public override string TileURL()
        {
            return "https://tile.openstreetmap.org/" + level.ToString() + "/" + pos.X.ToString() + "/" + pos.Y.ToString() + ".png";
        }


        internal override Texture2D GetTileTexture(string folder)
        {
            string cur_tile = folder + "\\" + tilefile;
            bool blank = false;
            try
            {
                if (System.IO.File.Exists(cur_tile)) return Texture2D.FromStream(Program.my_device, new System.IO.FileStream(cur_tile, FileMode.Open, FileAccess.Read));
                else
                {
                    blank = true;
                    throw new Exception();
                }
            }
            catch (Exception ex)
            {
                if (!blank)
                    Thread.Sleep(500);
                try
                {
                    using (WebClient client = new WebClient())
                    {
                        client.Headers.Add("Accept: text/html, application/xhtml+xml, */*");
                        client.Headers.Add("User-Agent: Mozilla/5.0 (compatible; MSIE 9.0; Windows NT 6.1; WOW64; Trident/5.0)");
                        client.DownloadFile(new Uri(tileurl), cur_tile);
                    }
                    return Texture2D.FromStream(Program.my_device, new FileStream(cur_tile, FileMode.Open, FileAccess.Read));
                }
                catch (Exception ex2)
                {
                    return null;
                }
            }
        }
    }
}
