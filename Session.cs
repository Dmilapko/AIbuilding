using AIlanding;
using FormElementsLib;
using Microsoft.VisualBasic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonogameButton;
using MonogameLabel;
using MonogameTextBoxLib;
using MonoHelper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AIbuilding
{
    internal class Session
    {
        MapEngine map;
        PhysicsEngine physics;
        Texture2D trackpoint;
        bool press_mr = false;
        List<PointD> trackpoints = new List<PointD>();
        Texture2D map_mask;
        Button makeroutebutton = new Button(Program.my_device, 10, 400, 150, 40, "MAKE ROUTE", Program.font15, 15, 15);
        Button launchbutton = new Button(Program.my_device, 200, 600, 150, 40, "LAUNCH", Program.font15, 15, 15);
        Button sessionbutton = new Button(Program.my_device, 350, 197, 120, 30, "SELECT", Program.font15, 15, 13);
        Button loadroutebutton = new Button(Program.my_device, 350, 262, 120, 30, "LOAD", Program.font15, 15, 13);
        Button saveroutebutton = new Button(Program.my_device, 350, 297, 120, 30, "SAVE", Program.font15, 15, 13);
        //Button calculateroutebutton = new Button(Program.my_device, 300, 400, 250, 40, "CALCULATE ROUTE", Program.font15, 15, 15);
        Label sessionlabel = new Label(10, 200, -1, -1, "Session name:", Program.font15, 15, 15, Color.Red.PackedValue);
        Label loadroutelabel = new Label(10, 265, -1, -1, "Load route :", Program.font15, 15, 15, Color.Red.PackedValue);
        Label saveroutelabel = new Label(10, 300, -1, -1, "Save route :", Program.font15, 15, 15, Color.Red.PackedValue);
        Label lengthroutelabel = new Label(10, 455, -1, -1, "Route length: 0m", Program.font15, 15, 15, Color.Red.PackedValue);
        Label positionroutelabel = new Label(10, 485, -1, -1, "Current position : 0m", Program.font15, 15, 15, Color.Red.PackedValue);
        Label loadprogresslabel = new Label(480, 265, -1, -1, "5/5", Program.font15, 15, 15, Color.Red.PackedValue);
        Label makeprogresslabel = new Label(170, 410, -1, -1, "5/5", Program.font15, 15, 15, Color.Red.PackedValue);
        TextBox sessiontextbox = new TextBox(Program.my_device, 145, 197, 200, Program.font15, 15, 15);
        TextBox loadroutetextbox = new TextBox(Program.my_device, 145, 262, 200, Program.font15, 15, 15);
        TextBox saveroutetextbox = new TextBox(Program.my_device, 145, 297, 200, Program.font15, 15, 15);
        List<FormElement> elements;
        List<List<PointD>> route_buildings = new List<List<PointD>>();
        List<List<int>> route_building_indexes = new List<List<int>>();
        List<BuildingRepresentation> building_representation = new List<BuildingRepresentation>();
        bool session_loading = false;
        bool track_changed = true; int point_lock = -1;
        int test_position = 0;
        int progress_b = 0;
        List<PointD> points_on_track = new List<PointD>();

        public Session()
        {
            map = new MapEngine(1000000);
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
            sessiontextbox.text = Program.setupProp["session_name"];
            saveroutetextbox.text = Program.setupProp["routes_name"];
            loadroutetextbox.text = Program.setupProp["routel_name"];
            makeroutebutton.Click += Makeroutebutton_Click;
            sessionbutton.Click += Sessionbutton_Click;
            loadroutebutton.Click += Loadroutebutton_Click;
            saveroutebutton.Click += Saveroutebutton_Click;
            elements = new List<FormElement>() { makeprogresslabel, loadprogresslabel, launchbutton, makeroutebutton, sessionbutton, loadroutebutton, saveroutebutton, sessionlabel, sessiontextbox, loadroutelabel, saveroutelabel, loadroutetextbox, saveroutetextbox, lengthroutelabel, positionroutelabel };
        }

        private void CalculateTrack()
        {
            var curve = new BeizerCurve(trackpoints);
            route_building_indexes = Drone.MakeBuilidng(curve, 50, 1000, route_buildings);
            points_on_track = Drone.GetTrack(curve, 50);
            progress_b++;
            building_representation = Drone.GetBuildingRepresentations(route_buildings);
            progress_b++;
        }

        private void Saveroutebutton_Click(object sender, ClickEventArgs e)
        {
            if (session_loading) return;
            Program.setupProp["routes_name"] = saveroutetextbox.text;
            Program.ChangeSetup();
            using (FileStream file = new FileStream("routes\\" + Program.setupProp["routes_name"], FileMode.Create, System.IO.FileAccess.Write))
            {
                MemoryStream ms = new MemoryStream();
                using (BinaryWriter binwriter = new BinaryWriter(ms, Encoding.Default, true))
                {
                    binwriter.Write(trackpoints.Count);
                    for (int i = 0; i < trackpoints.Count; i++)
                    {
                        binwriter.Write(trackpoints[i].X);
                        binwriter.Write(trackpoints[i].Y);
                    }
                    binwriter.Write(route_buildings.Count);
                    for (int i = 0; i < route_buildings.Count; i++)
                    {
                        binwriter.Write(route_buildings[i].Count);
                        for (int j = 0; j < route_buildings[i].Count; j++)
                        {
                            binwriter.Write(route_buildings[i][j].X);
                            binwriter.Write(route_buildings[i][j].Y);
                        }
                    }
                }
                ms.WriteTo(file);
            }
        }

        void LoadRoute()
        {
            progress_b = 0;
            using (BinaryReader binreader = new BinaryReader(new FileStream("routes\\" + Program.setupProp["routel_name"], FileMode.Open, FileAccess.Read)))
            {
                int trackpoints_count = binreader.ReadInt32();
                trackpoints = new List<PointD>(trackpoints_count);
                for (int i = 0; i < trackpoints_count; i++)
                {
                    trackpoints.Add(new PointD(binreader.ReadDouble(), binreader.ReadDouble()));
                }
                int buildings_count = binreader.ReadInt32();
                route_buildings = new List<List<PointD>>(buildings_count);
                for (int i = 0; i < buildings_count; i++)
                {
                    int nodescnt = binreader.ReadInt32();
                    route_buildings.Add(new List<PointD>());
                    for (int j = 0; j < nodescnt; j++)
                    {
                        route_buildings[i].Add(new PointD(binreader.ReadDouble(), binreader.ReadDouble()));
                    }
                }
            }
            progress_b++;
            CalculateTrack();
            session_loading = false;
        }

        private void Loadroutebutton_Click(object sender, ClickEventArgs e)
        {
            if (session_loading) return;
            session_loading = true;
            Program.setupProp["routel_name"] = loadroutetextbox.text;
            Program.ChangeSetup();
            Thread loadroute_thread = new Thread(() => LoadRoute());
        }

        private void Sessionbutton_Click(object sender, ClickEventArgs e)
        {
            Program.setupProp["session_name"] = sessiontextbox.text;
            Program.ChangeSetup();
            if (!Directory.Exists(sessiontextbox.text)) Directory.CreateDirectory(sessiontextbox.text);
        }

        void LoadTrack()
        {
            progress_b = 0;
            List<PointD> track_pos = Drone.GetTrack(new BeizerCurve(trackpoints), 600);
            string sts = "";
            foreach (var item in track_pos)
            {
                sts += "way(around:1100," + item.Y.ToString("0.######", CultureInfo.InvariantCulture) + "," + item.X.ToString("0.######", CultureInfo.InvariantCulture) + ")[building](if:number(t[\"building:levels\"])>=7);";
            }
            string request_s = "https://overpass-api.de/api/interpreter?data=(" + sts + ");out geom;";
            string contents = "";
            int prev = Program.overpasscnt;
            Program.overpasscnt++;
            Thread.Sleep(prev * 500);
            using (WebClient client = new WebClient())
            {
                client.Headers.Add("User-Agent: Mozilla/5.0 (compatible; MSIE 9.0; Windows NT 6.1; WOW64; Trident/5.0)");
                contents = client.DownloadString(request_s);
            }
            Program.overpasscnt--;
            if (Program.overpasscnt < 0) Program.overpasscnt = 0;
            route_buildings = new List<List<PointD>>();
            int stringnow = 0, stringprev = -1;
            for (int i = 8; i < contents.Length; i++)
            {
                if (contents[i] == '\n') stringnow++;
                if (contents.Substring(i - 7, 7) == "\" lat=\"")
                {
                    if (stringnow != stringprev + 1) route_buildings.Add(new List<PointD>());
                    stringprev = stringnow;
                    route_buildings.Last().Add(new PointD(Convert.ToDouble(contents.Substring(i + 17, 10), CultureInfo.InvariantCulture), Convert.ToDouble(contents.Substring(i, 10), CultureInfo.InvariantCulture)));
                }
            }
            progress_b++;
            CalculateTrack();
            test_position = 0;
            session_loading = false;
        }

        private void Makeroutebutton_Click(object sender, ClickEventArgs e)
        {
            if (trackpoints.Count < 3) return;
            session_loading = true;
            Thread load_thread = new Thread(() => LoadTrack());
            load_thread.Start();
        }

        bool press_g = false;
        public void Run(MouseState mouse, KeyboardState keyboard)
        {
            map.Run(mouse, keyboard);
            if (MHeleper.ApplicationIsActivated())
            {
                if (keyboard.IsKeyDown(Keys.G))
                {
                    if (!press_g) Program.save_graphics = !Program.save_graphics;
                    press_g = true;
                }
                else press_g = false;
                if (!session_loading)
                {
                    if (keyboard.IsKeyDown(Keys.OemPlus))
                    {
                        test_position = Math.Min(test_position + 1, route_building_indexes.Count - 1);
                    }
                    if (keyboard.IsKeyDown(Keys.OemMinus))
                    {
                        test_position = Math.Max(test_position - 1, 0);
                    }
                }
                positionroutelabel.text = "Current position : " + test_position.ToString();
                if (mouse.Position.ToVector2().InRect(MapMath.start_screen, MapMath.end_screen))
                {
                    if (mouse.RightButton == ButtonState.Pressed)
                    {
                        if (point_lock == -1)
                        {
                            for (int i = 0; i < trackpoints.Count; i++)
                            {
                                Vector2 pos = MapMath.LongLatToScreen(trackpoints[i], map.center, map.level);
                                if ((pos - mouse.Position.ToVector2()).Length() < 20) point_lock = i;
                            }
                        }
                        if (point_lock == -1) trackpoints.Add(MapMath.ScreenToLongLat(mouse.Position.ToVector2(), map.center, map.level));
                        else trackpoints[point_lock] = MapMath.ScreenToLongLat(mouse.Position.ToVector2(), map.center, map.level);
                        track_changed = true;
                    }
                    else point_lock = -1;
                    if (mouse.MiddleButton == ButtonState.Pressed)
                    {
                        int intersect_id = -1;
                        for (int i = 0; i < trackpoints.Count; i++)
                        {
                            Vector2 pos = MapMath.LongLatToScreen(trackpoints[i], map.center, map.level);
                            if ((pos - mouse.Position.ToVector2()).Length() < 20) intersect_id = i;
                        }
                        if (intersect_id != -1)
                        {
                            trackpoints = trackpoints.GetRange(0, intersect_id);
                            track_changed = true;
                        }
                    }
                }
                if (track_changed)
                {
                    lengthroutelabel.text = "Route length: " + ((int)Math.Round(new BeizerCurve(trackpoints).length)).ToString() + "m";
                    track_changed = false;
                }
                foreach (var item in elements) item.Check(mouse, keyboard);
            }
        }

        double rott = 0;

        public void Draw()
        {
            map.Draw();
            loadprogresslabel.text = progress_b.ToString();
            Program.spriteBatch.Draw(trackpoint, MapMath.LongLatToScreen(MapMath.RotateLongtLat(new PointD(37.622406978653736, 55.744331661391), 162000, rott), map.center, map.level), null, Microsoft.Xna.Framework.Color.White, 0f, new Vector2(11.5f, 11.5f), MathF.Pow(1.1f, map.level - 15), SpriteEffects.None, 1);
            rott += 0.1;
            foreach (PointD point in trackpoints)
            {
                Vector2 pos = MapMath.LongLatToScreen(point, map.center, map.level);
                if (pos.InRect(MapMath.start_screen, MapMath.end_screen))
                    Program.spriteBatch.Draw(trackpoint, pos, null, Microsoft.Xna.Framework.Color.White, 0f, new Vector2(11.5f, 11.5f), MathF.Pow(1.1f, map.level - 15), SpriteEffects.None, 1);
            }
            if (trackpoints.Count > 2) 
            {
                var lp = Drone.GetTrack(new BeizerCurve(trackpoints), MapMath.ScreenToLongLat(MapMath.start_screen, map.center, map.level), MapMath.ScreenToLongLat(MapMath.end_screen, map.center, map.level));
                for (int  i = 0;  i < lp.Count -1;  i++)
                {
                    Vector2 p0 = MapMath.LongLatToScreen(lp[i], map.center, map.level), p1 = MapMath.LongLatToScreen(lp[i + 1], map.center, map.level);
                    if (MapMath.LineInScreen(p0, p1)) Program.spriteBatch.DrawLine(p0, p1, Color.Red, Math.Max(2, 4 * MathF.Pow(1.2f, map.level - 15)));
                }
            }
            if (!session_loading && map.level > 10)
            {
                if (!Program.save_graphics) foreach (var building in route_buildings)
                {
                    for (int i = 0; i < building.Count; i++)
                    {
                        Vector2 stpos = MapMath.LongLatToScreen(building[i], map.center, map.level);
                        Vector2 fnpos = MapMath.LongLatToScreen(building[(i + 1) % building.Count], map.center, map.level);
                        if (MapMath.LineInScreen(stpos, fnpos)) GraphicsPrimitives.DrawLine(Program.spriteBatch, stpos, fnpos, Color.Purple, MathF.Pow(1.2f, map.level - 12 + 1));
                    }
                }
                if (route_building_indexes.Count > 0)
                {
                    foreach (var b_ind in route_building_indexes[test_position])
                    {
                        var building = route_buildings[b_ind];
                        for (int i = 0; i < building.Count; i++)
                        {
                            Vector2 stpos = MapMath.LongLatToScreen(building[i], map.center, map.level);
                            Vector2 fnpos = MapMath.LongLatToScreen(building[(i + 1) % building.Count], map.center, map.level);
                            if (MapMath.LineInScreen(stpos, fnpos)) GraphicsPrimitives.DrawLine(Program.spriteBatch, stpos, fnpos, Color.Red, MathF.Pow(1.2f, map.level - 12 + 1));
                        }
                    }
                    Program.spriteBatch.Draw(trackpoint, MapMath.LongLatToScreen(points_on_track[test_position], map.center, map.level), null, Microsoft.Xna.Framework.Color.White, 0f, new Vector2(11.5f, 11.5f), MathF.Pow(1.1f, map.level - 15), SpriteEffects.None, 1);
                }
            }
            Program.spriteBatch.Draw(map_mask, new Vector2(0, 0), Color.White);
            foreach (var item in elements) item.Draw(Program.spriteBatch);
        }
    }
}
 