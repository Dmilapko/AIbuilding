using AIlanding;
using Microsoft.VisualBasic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoHelper;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AIbuilding
{
    internal class BuildingMaskTile:AbstractTile
    {
        internal BuildingMaskTile(Microsoft.Xna.Framework.Point pos, string folder):base(10, pos, folder)
        {
        }

       /* public static string TileFile(Microsoft.Xna.Framework.Point pos, int level)
        {
            return "tile_" + level.ToString() + "_" + pos.X.ToString() + "_" + pos.Y.ToString() + ".png";
        }*/
        public override string TileFile()
        {
            return "buildingmask_" + level.ToString() + "_" + pos.X.ToString() + "_" + pos.Y.ToString() + ".png";
        }

        public override string TileURL()
        {
            //     "https://overpass-api.de/api/interpreter?data=way[%22building%22](if:%20number(t[%22building:levels%22])%3E=7)(55.59105440134483,37.451963424682624,55.63371345098194,37.5425148010254);%20out%20geom;";
            var leftv = MapMath.PointToLongLat(pos, level);
            var rightv = MapMath.PointToLongLat(pos + new Point(1, 1), level);
            return "https://overpass-api.de/api/interpreter?data=[out:csv(::lat,::lon;false;\";\")];way[\"building\"](if:number(t[\"building:levels\"])%3E=7)(" + (rightv.Y).ToString(CultureInfo.InvariantCulture) + "," + leftv.X.ToString(CultureInfo.InvariantCulture) + "," + (leftv.Y).ToString(CultureInfo.InvariantCulture) + "," + rightv.X.ToString(CultureInfo.InvariantCulture) + ");%20out%20center;";
        }

        /*internal override Texture2D GetTileTexture(string folder)
        {
            string cur_tile = folder + "\\" + tilefile;
            bool blank = false;
            List<Vector2> tiledata;
            try
            {
                if (System.IO.File.Exists(cur_tile))
                {
                    using (BinaryReader binreader = new BinaryReader(new FileStream(cur_tile, FileMode.Open, FileAccess.Read)))
                    {
                        int buildings_count = binreader.ReadInt32();
                        tiledata = new List<Vector2>(buildings_count);
                        for (int i = 0; i < buildings_count; i++) tiledata.Add(new Vector2(binreader.ReadSingle()));
                    }
                }
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
                    string contents;
                    using (WebClient client = new WebClient())
                    {
                     
                        client.Headers.Add("User-Agent: Mozilla/5.0 (compatible; MSIE 9.0; Windows NT 6.1; WOW64; Trident/5.0)");
                        contents = client.DownloadString(tileurl);
                    }
                    tiledata = new List<Vector2>();
                    for (int i = 0; i < contents.Length / 22; i++)
                    {
                        tiledata.Add(new Vector2(Convert.ToSingle(contents.Substring(i, 10)), Convert.ToSingle(contents.Substring(i + 11, 10))));
                    }
                    using (FileStream file = new FileStream(cur_tile, FileMode.Create, System.IO.FileAccess.Write))
                    {
                        MemoryStream ms = new MemoryStream();
                        using (BinaryWriter binwriter = new BinaryWriter(ms, Encoding.Default, true))
                        {
                            binwriter.Write(tiledata.Count);
                            for (int i = 0; i < tiledata.Count; i++)
                            {
                                binwriter.Write(tiledata[i].X);
                                binwriter.Write(tiledata[i].Y);
                            }
                        }
                        ms.WriteTo(file);
                    }
                }
                catch (Exception ex2)
                {
                    return null;
                }
            }
            Texture2D res = new Texture2D(Program.my_device, 256, 256);
            Color[] cd = new Color[256 * 256];
            return res;
        }*/

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
                string contents;
                if (!blank)
                    Thread.Sleep(500);
                try
                {
                    int prev = Program.overpasscnt;
                    Program.overpasscnt++;
                    Thread.Sleep(prev * 500);
                    using (WebClient client = new WebClient())
                    {

                        client.Headers.Add("User-Agent: Mozilla/5.0 (compatible; MSIE 9.0; Windows NT 6.1; WOW64; Trident/5.0)");
                        contents = client.DownloadString(tileurl);
                    }
                    Program.overpasscnt--;
                    Texture2D res = new Texture2D(Program.my_device, 256, 256);
                    Color[] cd = new Color[256 * 256];
                    for (int i = 0; i < 256 * 256; i++) cd[i] = Color.Transparent; 
                    for (int i = 0; i < contents.Length; i+=22)
                    {
                        PointD vp = MapMath.LongLatToPoint(new PointD(Convert.ToDouble(contents.Substring(i, 10), CultureInfo.InvariantCulture), Convert.ToDouble(contents.Substring(i + 11, 10), CultureInfo.InvariantCulture)), level) - new PointD(pos.X, pos.Y);
                        if ((vp.X > 0 && vp.Y > 0 && vp.X < 1 && vp.Y < 1) && ((int)(vp.X * 256) + (int)(vp.Y * 256) * 256) < 256*256) 
                            cd[(int)(vp.X * 256) + (int)(vp.Y * 256) * 256] = new Color(0, 0, 255, 128);
                    }
                    res.SetData(cd);
                    using (System.Drawing.Bitmap pic = new System.Drawing.Bitmap(256, 256, System.Drawing.Imaging.PixelFormat.Format32bppArgb))
                    {
                        for (int x = 0; x < 256; x++)
                        {
                            for (int y = 0; y < 256; y++)
                            {
                                int arrayIndex = y * 256 + x;
                                System.Drawing.Color c = System.Drawing.Color.FromArgb(
                                   cd[arrayIndex].A,
                                   cd[arrayIndex].R,
                                   cd[arrayIndex].G,
                                   cd[arrayIndex].B
                                );
                                pic.SetPixel(x, y, c);
                            }
                        }
                        pic.Save(cur_tile, System.Drawing.Imaging.ImageFormat.Png);  
                    }
                    return res;
                }
                catch (Exception ex2)
                {
                    return null;
                }
            }
        }
    }
}
