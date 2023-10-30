using AIlanding;
using FormElementsLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoHelper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace AIbuilding
{
    internal class Session
    {
        MapEngine map;
        PhysicsEngine physics;
        Texture2D trackpoint;
        bool press_mr = false;
        List<PointD> routepoints = new List<PointD>();
        Texture2D map_mask;

        public Session()
        {
            map = new MapEngine(1000000, "lololo");
            trackpoint = MHeleper.CreateCircle(Program.my_device, 12, Color.Yellow);
            trackpoint.DrawCircle(new Vector2(11.5f, 11.5f), 3.5, 4.5, Color.Red);
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

        public void Run(MouseState mouse, KeyboardState keyboard)
        {
            map.Run(mouse, keyboard);
            if (mouse.Position.ToVector2().InRect(MapMath.start_screen, MapMath.end_screen))
            {
                if (mouse.RightButton == ButtonState.Pressed)
                {
                    int intersect_id = -1;
                    for (int i = 0; i < routepoints.Count; i++)
                    {
                        Vector2 pos = MapMath.LatLongToScreen(routepoints[i], map.center, map.level);
                        if ((pos - mouse.Position.ToVector2()).Length() < 30) intersect_id = i;
                    }
                    if (intersect_id == -1) routepoints.Add(MapMath.ScreenToLatLong(mouse.Position.ToVector2(), map.center, map.level));
                    else routepoints[intersect_id] = MapMath.ScreenToLatLong(mouse.Position.ToVector2(), map.center, map.level);
                }
                if (mouse.MiddleButton == ButtonState.Pressed)
                {
                    int intersect_id = -1;
                    for (int i = 0; i < routepoints.Count; i++)
                    {
                        Vector2 pos = MapMath.LatLongToScreen(routepoints[i], map.center, map.level);
                        if ((pos - mouse.Position.ToVector2()).Length() < 20) intersect_id = i;
                    }
                    if (intersect_id != -1) routepoints = routepoints.GetRange(0, intersect_id);
                }
            }
        }

        public void Draw()
        {
            map.Draw();
            foreach (PointD point in routepoints)
            {
                Vector2 pos = MapMath.LatLongToScreen(point, map.center, map.level);
                if (pos.InRect(MapMath.start_screen, MapMath.end_screen))
                    Program.spriteBatch.Draw(trackpoint, MapMath.LatLongToScreen(point, map.center, map.level), null, Microsoft.Xna.Framework.Color.White, 0f, new Vector2(11.5f, 11.5f), MathF.Pow(1.1f, map.level - 15), SpriteEffects.None, 1);
            }
            if (routepoints.Count > 2) 
            {
                var lp = Drone.GetTrack(routepoints, MapMath.ScreenToLatLong(MapMath.start_screen, map.center, map.level), MapMath.ScreenToLatLong(MapMath.end_screen, map.center, map.level));
                for (int  i = 0;  i < lp.Count -1;  i++)
                {
                    Vector2 p0 = MapMath.LatLongToScreen(lp[i], map.center, map.level), p1 = MapMath.LatLongToScreen(lp[i + 1], map.center, map.level);
                    if (MapMath.LineInScreen(p0, p1)) Program.spriteBatch.DrawLine(p0, p1, Color.Red, Math.Max(2, 4 * MathF.Pow(1.2f, map.level - 15)));
                }
            }
            Program.spriteBatch.Draw(map_mask, new Vector2(0, 0), Color.White);
        }
    }
}
 