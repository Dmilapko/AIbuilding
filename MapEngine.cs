using AIbuilding;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Framework.Utilities.Deflate;
using MonoHelper;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection.Metadata;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AIbuilding
{
    using static System.Net.WebRequestMethods;
    using TupleLSL = Tuple<long, string, long>;

    public static class MapMath
    {
        public static Vector2 start_screen = new Vector2(868, 28);
        public static Vector2 end_screen = new Vector2(868 + 1024, 28 + 1024);

        /// <summary>
        /// Returns longitude and Y from position and level
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="level"></param>
        /// <returns></returns>
        public static Vector2 PointToCoordinates(Point pos, int level)
        {
            return new Vector2(-180 + pos.X / MathF.Pow(2, level) * 360f, -180 + pos.Y / MathF.Pow(2, level) * 360f);
        }

        public static Point CoordinatesToPoint(PointD coords, int level)
        {
            return new Point((int)((coords.X + 180) * Math.Pow(2, level) / 360), (int)((coords.Y + 180) * Math.Pow(2, level) / 360));
        }

        public static Vector2 CoordiantesToScreen(Vector2 coords, Vector2 center, int cur_level)
        {
            return new Vector2((coords.X - center.X) / 360 * MathF.Pow(2, cur_level) * 1024 + 1380, (coords.Y - center.Y) / 360 * MathF.Pow(2, cur_level) * 1024 + 540);
        }

        public static float PixelsToDistance(int p, int level)
        {
            return p / 1024f / MathF.Pow(2, level) * 360;
        }

        public static PointD ScreenToCoordinates(Vector2 scr_pos, Vector2 center, int cur_level)
        {
            return new PointD((scr_pos.X - 1380) * 360 / 1024 / Math.Pow(2, cur_level) + center.X, (scr_pos.Y - 540) * 360 / 1024 / Math.Pow(2, cur_level) + center.Y);
        }

        public static PointD PointToLongLat(Point point, int level)
        {
            double n = Math.PI - 2.0 * Math.PI * point.Y / (double)(1 << level);
            return new PointD(point.X / (double)(1 << level) * 360.0 - 180, 180.0 / Math.PI * Math.Atan(0.5 * (Math.Exp(n) - Math.Exp(-n))));
        }

        public static PointD PointToLongLat(PointD point, int level)
        {
            double n = Math.PI - 2.0 * Math.PI * point.Y / (double)(1 << level);
            return new PointD(point.X / (double)(1 << level) * 360.0 - 180, 180.0 / Math.PI * Math.Atan(0.5 * (Math.Exp(n) - Math.Exp(-n))));
        }

        public static PointD LongLatToPoint(PointD latlong, int level)
        {
            var latRad = latlong.X / 180 * Math.PI;
            return new PointD((latlong.Y + 180.0) / 360.0 * (1 << level), (1 - Math.Log(Math.Tan(latRad) + 1 / Math.Cos(latRad)) / Math.PI) / 2 * (1 << level));
        }

        public static PointD ScreenToLongLat(Vector2 scr_pos, Vector2 center, int level)
        {
            PointD point = (scr_pos - MapMath.start_screen).ToPointD() / 256 - new PointD(2, 2) + new PointD((center.X + 180) * (1 << (level + 2)) / 360, (center.Y + 180) * (1 << (level + 2)) / 360);
            return PointToLongLat(point, level + 2);
        }

        public static Vector2 LongLatToScreen(PointD longlat, Vector2 center, int level)
        {
            double latRad = longlat.Y / 180 * Math.PI;
            PointD point_dif = new PointD((longlat.X - center.X) / 360 * (1 << (level + 2)), ((1 - Math.Log(Math.Tan(latRad) + 1 / Math.Cos(latRad)) / Math.PI) / 2 - (center.Y + 180) / 360) * (1 << (level + 2))) + new PointD(2, 2);
            return (point_dif * 256).ToVector2() + start_screen;
        }

        public static PointD LongLatDif(PointD point, PointD startpoint)
        {
            double dy = MapMath.DistanceLongLat(point, new PointD(point.X, startpoint.Y));
            double a = MapMath.DistanceLongLat(point, startpoint);
            double dx = Math.Sqrt(a * a - dy * dy);
            return new PointD(MHeleper.GetSign(point.X - startpoint.X) * dx, MHeleper.GetSign(point.Y - startpoint.Y) * dy);
        }

        public static double AngleLongLat(PointD p1, PointD p2)
        {
            return angleFromCoordinate(p1.Y.ToRadians(), p1.X.ToRadians(), p2.Y.ToRadians(), p2.X.ToRadians());
        }

        private static double angleFromCoordinate(double lat1, double long1, double lat2, double long2)
        {

            double dLon = (long2 - long1);

            double y = Math.Sin(dLon) * Math.Cos(lat2);
            double x = Math.Cos(lat1) * Math.Sin(lat2) - Math.Sin(lat1)
                    * Math.Cos(lat2) * Math.Cos(dLon);

            double brng = Math.Atan2(y, x);
            return brng;
        }


        public static bool LineInScreen(Vector2 p0, Vector2 p1)
        {
            return SegmentIntersectRectangle(start_screen.X, start_screen.Y, end_screen.X, end_screen.Y, p0.X, p0.Y, p1.X, p1.Y);
        }

        public static double DistanceLongLat(PointD longlat0, PointD longlat1)
        {
            return GetDistance(longlat0.X, longlat0.Y, longlat1.X, longlat1.Y);
        }

        //https://stackoverflow.com/a/24712129
        public static double DistanceTo(double lat1, double lon1, double lat2, double lon2)
        {
            double rlat1 = Math.PI * lat1 / 180;
            double rlat2 = Math.PI * lat2 / 180;
            double theta = lon1 - lon2;
            double rtheta = Math.PI * theta / 180;
            double dist =
                Math.Sin(rlat1) * Math.Sin(rlat2) + Math.Cos(rlat1) *
                Math.Cos(rlat2) * Math.Cos(rtheta);
            dist = Math.Acos(dist);
            dist = dist * 180 / Math.PI;
            dist = dist * 60 * 1.1515;

            return dist * 1609.344;
        }

        private static double GetDistance(double longitude, double latitude, double otherLongitude, double otherLatitude)
        {
            var d1 = latitude * (Math.PI / 180.0);
            var num1 = longitude * (Math.PI / 180.0);
            var d2 = otherLatitude * (Math.PI / 180.0);
            var num2 = otherLongitude * (Math.PI / 180.0) - num1;
            var d3 = Math.Pow(Math.Sin((d2 - d1) / 2.0), 2.0) + Math.Cos(d1) * Math.Cos(d2) * Math.Pow(Math.Sin(num2 / 2.0), 2.0);

            return 6376500.0 * (2.0 * Math.Atan2(Math.Sqrt(d3), Math.Sqrt(1.0 - d3)));
        }

        public static PointD RotateLongtLat(PointD center, double radiusm, double theta)
        {
            // Convert input miles to degrees latitude and longitude.
            // 110574+55.7*12.5
            var radiusLon = radiusm / (111291.1 * Math.Cos(center.Y * (Math.PI / 180)));
            var radiusLat = radiusm / 111290.8;


            return new PointD(center.X + radiusLon * Math.Sin(theta), center.Y + radiusLat * Math.Cos(theta)); 
        }

        public static bool SegmentIntersectRectangle(
            float rectangleMinX,
            float rectangleMinY,
            float rectangleMaxX,
            float rectangleMaxY,
            float p1X,
            float p1Y,
            float p2X,
            float p2Y)
        {
            // Find min and max X for the segment
            float minX = p1X;
            float maxX = p2X;

            if (p1X > p2X)
            {
                minX = p2X;
                maxX = p1X;
            }

            // Find the intersection of the segment's and rectangle's x-projections
            if (maxX > rectangleMaxX)
            {
                maxX = rectangleMaxX;
            }

            if (minX < rectangleMinX)
            {
                minX = rectangleMinX;
            }

            if (minX > maxX) // If their projections do not intersect return false
            {
                return false;
            }

            // Find corresponding min and max Y for min and max X we found before
            float minY = p1Y;
            float maxY = p2Y;

            float dx = p2X - p1X;

            if (MathF.Abs(dx) > 0.0000001)
            {
                float a = (p2Y - p1Y) / dx;
                float b = p1Y - a * p1X;
                minY = a * minX + b;
                maxY = a * maxX + b;
            }

            if (minY > maxY)
            {
                float tmp = maxY;
                maxY = minY;
                minY = tmp;
            }

            // Find the intersection of the segment's and rectangle's y-projections
            if (maxY > rectangleMaxY)
            {
                maxY = rectangleMaxY;
            }

            if (minY < rectangleMinY)
            {
                minY = rectangleMinY;
            }

            if (minY > maxY) // If Y-projections do not intersect return false
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Method to compute the centroid of a polygon. This does NOT work for a complex polygon.
        /// </summary>
        /// <param name="poly">points that define the polygon</param>
        /// <returns>centroid point, or PointF.Empty if something wrong</returns>
        public static PointD GetCentroid(List<PointD> poly)
        {
            double accumulatedArea = 0.0f;
            double centerX = 0.0f;
            double centerY = 0.0f;

            for (int i = 0, j = poly.Count - 1; i < poly.Count; j = i++)
            {
                double temp = poly[i].X * poly[j].Y - poly[j].X * poly[i].Y;
                accumulatedArea += temp;
                centerX += (poly[i].X + poly[j].X) * temp;
                centerY += (poly[i].Y + poly[j].Y) * temp;
            }

            if (Math.Abs(accumulatedArea) < 1E-8f)
                return new PointD(0,0);  // Avoid division by zero

            accumulatedArea *= 3f;
            return new PointD(centerX / accumulatedArea, centerY / accumulatedArea);
        }

    }

    internal class MapEngine
    {
        List<List<AbstractTile>> tiles = new List<List<AbstractTile>> { new List<AbstractTile>(), new List<AbstractTile>(), new List<AbstractTile>() };
        public Vector2 center = new Vector2(0, 0);
        public int level = 1;
        List<HashSet<TileDesc>> tiles_set = new List<HashSet<TileDesc>> { new HashSet<TileDesc>(), new HashSet<TileDesc>(), new HashSet<TileDesc>() };
        string datafile;
        long byte_limit;
        Point prev_mouse_pos = new Point(0, 0);
        Texture2D map_mask;
        bool lastmove = true;

        public MapEngine(long byte_limit)
        {
            this.byte_limit = byte_limit;
            datafile = Directory.GetCurrentDirectory()+"\\data";
            map_mask = new Texture2D(Program.my_device, 1920, 1080);
            Color[] cd = new Color[1920 * 1080];
            for (int i = 0; i < 1920 * 1080; i++) cd[i] = Color.Black;
            for (int x = (int)MapMath.start_screen.X; x < (int)MapMath.start_screen.X + 1024; x++)
            {
                for (int y = (int)MapMath.start_screen.Y; y < (int)MapMath.start_screen.Y + 1024; y++)
                {
                    cd[y * 1920 + x] = Color.Transparent;
                }
            }
            map_mask.SetData(cd);
        }

        public int tile_level(int t)
        {
            if (t==0) return Math.Min(16, level + 2); 
            if (t==1) return Math.Min(10, level + 2);
            if (t == 2) return 12;
            return -1;
        }

        public List<Point> GetVisibleTiles(int t)
        {
            Point tilepos = MapMath.CoordinatesToPoint(MapMath.ScreenToCoordinates(MapMath.start_screen, center, level), tile_level(t));
            List<Point> res = new List<Point>();
            int width_tiles = 4, height_tiles = 4;
            if (t == 0)
            {
                if (level == 15) width_tiles = height_tiles = 2;
                if (level >= 16) width_tiles = height_tiles = 1;
            }
            else if (t == 1) 
            {
                if (level == 8) width_tiles = height_tiles = 4;
                else if (level == 9) width_tiles = height_tiles = 2;
                else if (level == 10) width_tiles = height_tiles = 1;
                else return res;
            }
            else if (t == 2)
            {
                if (level == 11) width_tiles = height_tiles = 2;
                else if (level >= 12) width_tiles = height_tiles = 1;
                else return res;
            }
            Vector2 topleft_tilepos = MapMath.CoordiantesToScreen(MapMath.PointToCoordinates(tilepos, tile_level(t)), center, level);
            if (topleft_tilepos.X < MapMath.start_screen.X) width_tiles++;
            if (topleft_tilepos.Y < MapMath.start_screen.Y) height_tiles++;

            for (int x = Math.Max(tilepos.X, 0); x < Math.Min(tilepos.X + width_tiles, 1 << (tile_level(t))); x++)
            {
                for (int y = Math.Max(tilepos.Y, 0); y < Math.Min(tilepos.Y + height_tiles, 1 << (tile_level(t))); y++)
                {
                    res.Add(new Point(x, y));
                }
            }
            return res;
        }

        void CleanOld()
        {
            var files = Directory.GetFiles(Program.setupProp["session_name"]);
            List<TupleLSL> file_dates = new List<TupleLSL>();
            long total_bytes = 0;
            foreach (string filename in files)
            {
                if (filename != datafile)
                {
                    var fileinfo = new FileInfo(filename);
                    file_dates.Add(new TupleLSL(fileinfo.CreationTime.Ticks, filename, fileinfo.Length));
                    total_bytes += fileinfo.Length;
                }
            }
            file_dates.Sort();
            for (int i = 0; (i < file_dates.Count) && (total_bytes > byte_limit); i++)
            {
                System.IO.File.Delete(file_dates[i].Item2);
                total_bytes -= file_dates[i].Item3;
            }
        }

        bool press_ml = false;
        bool press_q = false, press_w = false;
        Vector2 prev_center;

        public void Run(MouseState mouse, KeyboardState keyboard)
        {
            if (MHeleper.ApplicationIsActivated())
            {
                if (mouse.LeftButton == ButtonState.Pressed && mouse.Position.ToVector2().InRect(MapMath.start_screen, MapMath.end_screen))
                {
                    if (!press_ml)
                    {
                        prev_mouse_pos = mouse.Position;
                        prev_center = center;
                    }
                    var p = prev_mouse_pos - mouse.Position;
                    center = prev_center + new Vector2(MapMath.PixelsToDistance(p.X, level), MapMath.PixelsToDistance(p.Y, level));
                    press_ml = true;
                }
                else press_ml = false;
                if (keyboard.IsKeyDown(Keys.Q))
                {
                    if (!press_q)
                    {
                        level++;
                        lastmove = true;
                        center = MapMath.ScreenToCoordinates(new Vector2(mouse.Position.X, mouse.Position.Y), center, level).ToVector2();
                    }
                    press_q = true;
                }
                else press_q = false;
                if (keyboard.IsKeyDown(Keys.W))
                {
                    if (!press_w)
                    {
                        //                    center = MapMath.ScreenToCoordinates(new Vector2(mouse.Position.X, mouse.Position.Y), center, level);
                        level--;
                        lastmove = false;
                    }
                    press_w = true;
                }
                else press_w = false;
            }
            for (int t = 0; t < 3; t++)
            {
                foreach (Point pos in GetVisibleTiles(t))
                {
                    if (!tiles_set[t].Contains(new TileDesc(tile_level(t), pos)))
                    {
                        tiles_set[t].Add(new TileDesc(tile_level(t), pos));
                        if (t == 0) tiles[t].Add(new MapTile(tile_level(t), pos, Program.setupProp["session_name"]));
                        else if (t == 1) tiles[t].Add(new BuildingMaskTile(pos, Program.setupProp["session_name"]));
                        else if (t == 2) tiles[t].Add(new BuildingPolyTile(pos, Program.setupProp["session_name"]));
                    }
                }
            }
        }

        //[out:csv(::lat, ::lon; false; ";")];
        //https://overpass-api.de/api/interpreter?data=way[%22building%22](if:%20number(t[%22building:levels%22])%3E=7)(55.59105440134483,37.451963424682624,55.63371345098194,37.5425148010254);%20out%20geom;
        public void Draw()
        {
            for (int t = 0; t < 3; t++)
            {
                List<bool> tile_used = new List<bool>(new bool[tiles[t].Count]);
                Pair<List<int>, List<int>> drawlist = new Pair<List<int>, List<int>>(new List<int>(), new List<int>());

                foreach (Point pos in GetVisibleTiles(t))
                {
                    if (lastmove)
                    {
                        int i = tiles[t].FindIndex(a => a.level == (tile_level(t)) && a.pos == pos);
                        if (i != -1 && tiles[t][i].loaded) drawlist.Second.Add(i);
                        else
                        {
                            int i2 = tiles[t].FindIndex(a => a.level == (tile_level(t) - 1) && a.pos == new Point(pos.X >> 1, pos.Y >> 1));
                            if (i2 != -1 && tiles[t][i2].loaded) drawlist.First.Add(i2);
                        }
                    }
                    else
                    {
                        int i = tiles[t].FindIndex(a => a.level == (tile_level(t)) && a.pos == pos);
                        if (i != -1 && tiles[t][i].loaded) drawlist.First.Add(i);
                        else
                        {
                            int i2 = tiles[t].FindIndex(a => a.level == (tile_level(t) + 1) && a.pos == new Point(pos.X * 2, pos.Y * 2));
                            if (i2 != -1 && tiles[t][i2].loaded) drawlist.Second.Add(i2);
                            i2 = tiles[t].FindIndex(a => a.level == (tile_level(t) + 1) && a.pos == new Point(pos.X * 2 + 1, pos.Y * 2 + 1));
                            if (i2 != -1 && tiles[t][i2].loaded) drawlist.Second.Add(i2);
                            i2 = tiles[t].FindIndex(a => a.level == (tile_level(t) + 1) && a.pos == new Point(pos.X * 2 + 1, pos.Y * 2));
                            if (i2 != -1 && tiles[t][i2].loaded) drawlist.Second.Add(i2);
                            i2 = tiles[t].FindIndex(a => a.level == (tile_level(t) + 1) && a.pos == new Point(pos.X * 2, pos.Y * 2 + 1));
                            if (i2 != -1 && tiles[t][i2].loaded) drawlist.Second.Add(i2);
                        }
                    }
                }
                foreach (int i in drawlist.First)
                {
                    tiles[t][i].DrawTile(level, Program.spriteBatch, center);
                    tile_used[i] = true;
                }
                foreach (int i in drawlist.Second)
                {
                    tiles[t][i].DrawTile(level, Program.spriteBatch, center);
                    tile_used[i] = true;
                }
                int tiles_deleted = 0;
                for (int i = 0; i < tile_used.Count; i++)
                {
                    if (!tile_used[i] && tiles[t][i - tiles_deleted].loaded)
                    {
                        tiles_set[t].RemoveWhere(a => a.level == tiles[t][i - tiles_deleted].level && a.pos == tiles[t][i - tiles_deleted].pos);
                        tiles[t].RemoveAt(i - tiles_deleted);
                        tiles_deleted++;
                    }
                }
            }
        }
    }
}
