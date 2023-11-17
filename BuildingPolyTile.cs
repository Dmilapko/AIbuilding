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
    internal class BuildingPolyTile:AbstractTile
    {
        List<List<Vector2>> tilebuildings;

        public BuildingPolyTile(Microsoft.Xna.Framework.Point pos, string folder) :base(12, pos, folder)
        {

        }

        public override string TileFile()
        {
            return "buildingpoly_" + level.ToString() + "_" + pos.X.ToString() + "_" + pos.Y.ToString() + ".dat";
        }

        public override string TileURL()
        {
            var leftv = MapMath.PointToLongLat(pos, level);
            var rightv = MapMath.PointToLongLat(pos + new Point(1, 1), level);
            return "https://overpass-api.de/api/interpreter?data=way[\"building\"](if:number(t[\"building:levels\"])%3E=7)(" + (rightv.Y).ToString(CultureInfo.InvariantCulture) + "," + leftv.X.ToString(CultureInfo.InvariantCulture) + "," + (leftv.Y).ToString(CultureInfo.InvariantCulture) + "," + rightv.X.ToString(CultureInfo.InvariantCulture) + ");%20out%20geom;";
        }

        internal override Texture2D GetTileTexture(string folder)
        {
            string contents;
            Texture2D texture = new Texture2D(Program.my_device, 2, 2);
            string cur_tile = folder + "\\" + tilefile;
            bool blank = false;
            try
            {
                if (System.IO.File.Exists(cur_tile))
                {
                    using (BinaryReader binreader = new BinaryReader(new FileStream(cur_tile, FileMode.Open, FileAccess.Read)))
                    {
                        int buildings_count = binreader.ReadInt32();
                        tilebuildings = new List<List<Vector2>>(buildings_count);
                        for (int i = 0; i < buildings_count; i++)
                        {
                            int nodescnt = binreader.ReadInt32();
                            tilebuildings.Add(new List<Vector2>());
                            for (int j = 0; j < nodescnt; j++)
                            {
                                tilebuildings[i].Add(new Vector2(binreader.ReadSingle(), binreader.ReadSingle()));
                            } 
                        }
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
                    int prev = Program.overpasscnt;
                    Program.overpasscnt++;
                    Thread.Sleep(prev * 500);
                    using (WebClient client = new WebClient())
                    {

                        client.Headers.Add("User-Agent: Mozilla/5.0 (compatible; MSIE 9.0; Windows NT 6.1; WOW64; Trident/5.0)");
                        contents = client.DownloadString(tileurl);
                    }
                    Program.overpasscnt--;
                    if (Program.overpasscnt < 0) Program.overpasscnt = 0;
                    tilebuildings = new List<List<Vector2>>();
                    int stringnow = 0, stringprev = -1;
                    for (int i = 8; i < contents.Length; i++)
                    {
                        if (contents[i] == '\n') stringnow++;
                        if (contents.Substring(i-7, 7) == "\" lat=\"")
                        {
                            if (stringnow != stringprev + 1) tilebuildings.Add(new List<Vector2>());
                            stringprev = stringnow;
                            tilebuildings.Last().Add(256 * 
                                (MapMath.LongLatToPoint(new PointD(Convert.ToDouble(contents.Substring(i, 10), CultureInfo.InvariantCulture), 
                                Convert.ToDouble(contents.Substring(i + 17, 10), CultureInfo.InvariantCulture)), level).ToVector2() - pos.ToVector2()));
                        }
                    }
                    using (FileStream file = new FileStream(cur_tile, FileMode.Create, System.IO.FileAccess.Write))
                    {
                        MemoryStream ms = new MemoryStream();
                        using (BinaryWriter binwriter = new BinaryWriter(ms, Encoding.Default, true))
                        {
                            binwriter.Write(tilebuildings.Count);
                            for (int i = 0; i < tilebuildings.Count; i++)
                            {
                                binwriter.Write(tilebuildings[i].Count);
                                for (int j = 0; j < tilebuildings[i].Count; j++)
                                {
                                    binwriter.Write(tilebuildings[i][j].X);
                                    binwriter.Write(tilebuildings[i][j].Y);
                                }
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
            return texture;
        }

        public override void DrawTile(int cur_level, SpriteBatch spriteBatch, Vector2 center)
        {
            if (Program.save_graphics) return;
            var scr_pos = ScreenPosition(cur_level, center);
            foreach (List<Vector2> building in tilebuildings)
            {
                for (int i = 0; i < building.Count; i++)
                {
                    Vector2 stpos = scr_pos.First + building[i] * scr_pos.Second;
                    Vector2 fnpos = scr_pos.First + building[(i + 1) % building.Count] * scr_pos.Second;
                    if (MapMath.LineInScreen(stpos, fnpos)) GraphicsPrimitives.DrawLine(Program.spriteBatch, stpos, fnpos, Color.Blue, MathF.Pow(1.2f, cur_level - level + 1));
                }
            }
        }
    }
}
