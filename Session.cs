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
        Texture2D trackpoint, building_edge, drone_arrow, angle_indicator, angle_indicator_line, gforce_meter, gforce_ball, rotation_bar;
        bool press_mr = false;
        List<PointD> trackpoints = new List<PointD>();
        Texture2D map_mask;
        Button makeroutebutton = new Button(Program.my_device, 10, 400, 150, 40, "MAKE ROUTE", Program.font15, 15, 15);
        Button launchbutton = new Button(Program.my_device, 10, 530, 150, 40, "LAUNCH", Program.font15, 15, 15);
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
        Label dronespeedlabel = new Label(10, 580, -1, -1, "Drone speed: 179km/h", Program.font15, 15, 15, Color.Red.PackedValue);
        Label debuglabel = new Label(10, 1050, -1, -1, "Debug: True", Program.font15, 15, 12, Color.Red.PackedValue);
        Label graphicslabel = new Label(135, 1050, -1, -1, "Save graphics: True", Program.font15, 15, 12, Color.Red.PackedValue);
        Label iterate_blabel  = new Label(310, 1050, -1, -1, "Iterate buildings: True", Program.font15, 15, 12, Color.Red.PackedValue);
        Label center_dlabel = new Label(500, 1050, -1, -1, "Center drone: False", Program.font15, 15, 12, Color.Red.PackedValue);
        Label dSlabel = new Label(500, 530, -1, -1, "Position deviation: 10.58m", Program.font15, 15, 12, Color.Red.PackedValue);
        Label dRlabel = new Label(500, 560, -1, -1, "Rotation deviation: 2.82deg", Program.font15, 15, 12, Color.Red.PackedValue);
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
        int progress_b = 3;
        List<PointD> points_on_track = new List<PointD>();
        RealDrone real_drone = new RealDrone(), abstract_drone = new RealDrone();
        bool flight = false;

        public Session()
        {
            drone_arrow = Texture2D.FromStream(Program.my_device, new FileStream("MyContent/drone_arrow.png", FileMode.Open));
            gforce_meter = Texture2D.FromStream(Program.my_device, new FileStream("MyContent/gforce_meter.png", FileMode.Open));
            angle_indicator = Texture2D.FromStream(Program.my_device, new FileStream("MyContent/angle_indicator.png", FileMode.Open));
            angle_indicator_line = Texture2D.FromStream(Program.my_device, new FileStream("MyContent/angle_indicator_line.png", FileMode.Open));
            gforce_ball = new Texture2D(Program.my_device, 10, 10);
            gforce_ball.Fill(Color.Transparent);
            gforce_ball.DrawCircle(new Vector2(4.5f, 4.5f), 0, 5, Color.Yellow);
            rotation_bar = MHeleper.CreateRectangle(Program.my_device, 4, 10, Color.Red);
            map = new MapEngine(1000000);
            building_edge = MHeleper.CreateCircle(Program.my_device, 10, Color.Black);
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
            launchbutton.Click += Launchbutton_Click;
            elements = new List<FormElement>() { dSlabel, dRlabel, debuglabel, iterate_blabel, graphicslabel, dronespeedlabel, makeprogresslabel, loadprogresslabel, launchbutton, makeroutebutton, sessionbutton, loadroutebutton, saveroutebutton, sessionlabel, sessiontextbox, loadroutelabel, saveroutelabel, loadroutetextbox, saveroutetextbox, lengthroutelabel, positionroutelabel };
        }

        private void Launchbutton_Click(object sender, ClickEventArgs e)
        {
            flight = !flight;
            real_drone = new RealDrone(building_representation, route_building_indexes, new BeizerCurve(trackpoints));
            if (flight)
            {
                abstract_drone = new RealDrone(building_representation, route_building_indexes, new BeizerCurve(trackpoints));
                abstract_drone.abstract_INS = real_drone.INS;
                abstract_drone.StartMainLoop();
                launchbutton.text = "Abort flight";
            }
            else
            {
                abstract_drone.alive = false;
                launchbutton.text = "LAUNCH";
            }
        }

        private void CalculateTrack()
        {
            var curve = new BeizerCurve(trackpoints);
            route_building_indexes = Drone.MakeBuilidng(curve, 50, 1000, route_buildings);
            points_on_track = Drone.GetTrack(curve, 50, -1);
            progress_b++;
            building_representation = Drone.GetBuildingRepresentations(route_buildings);
            real_drone = new RealDrone(building_representation, route_building_indexes, new BeizerCurve(trackpoints));
            progress_b++;
        }

        private void Saveroutebutton_Click(object sender, ClickEventArgs e)
        {
            if (session_loading || trackpoints.Count < 3) return;
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


        private void Loadroutebutton_Click(object sender, ClickEventArgs e)
        {
            if (session_loading || flight) return;
            session_loading = true;
            track_changed = true;
            Program.setupProp["routel_name"] = loadroutetextbox.text;
            Program.ChangeSetup();
            Thread loadroute_thread = new Thread(() =>
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
            });
            loadroute_thread.Start();
        }

        private void Sessionbutton_Click(object sender, ClickEventArgs e)
        {
            Program.setupProp["session_name"] = sessiontextbox.text;
            Program.ChangeSetup();
            if (!Directory.Exists(sessiontextbox.text)) Directory.CreateDirectory(sessiontextbox.text);
        }

        private void Makeroutebutton_Click(object sender, ClickEventArgs e)
        {
            if (trackpoints.Count < 3 || session_loading || flight) return;
            session_loading = true;
            Thread load_thread = new Thread(() =>
            {
                progress_b = 0;
                List<PointD> track_pos = Drone.GetTrack(new BeizerCurve(trackpoints), 600, -1);
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
                real_drone.curlength = 0;
                session_loading = false;
            });
            load_thread.Start();
        }

        public void Stop()
        {
            abstract_drone.alive = real_drone.alive = false;
        }

        bool press_g = false, press_d = false, press_i = false, press_c = false;
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
                if (keyboard.IsKeyDown(Keys.D))
                {
                    if (!press_d) Program.debug = !Program.debug;
                    press_d = true;
                }
                else press_d = false;
                if (keyboard.IsKeyDown(Keys.I))
                {
                    if (!press_i) Program.iterate_b = !Program.iterate_b;
                    press_i = true;
                    cnt_b = 0;
                }
                else press_i = false;
                if (keyboard.IsKeyDown(Keys.C))
                {
                    if (!press_c) Program.center_drone = !Program.center_drone;
                    press_c = true;
                    cnt_b = 0;
                }
                else press_c = false;
                if (!session_loading)
                {
                    if (keyboard.IsKeyDown(Keys.OemPlus))
                    {
                        real_drone.index_pos = Math.Min(real_drone.index_pos + 1, route_building_indexes.Count - 1);
                    }
                    if (keyboard.IsKeyDown(Keys.OemMinus))
                    {
                        real_drone.index_pos = Math.Max(real_drone.index_pos - 1, 0);
                    }
                }
                positionroutelabel.text = "Current position : " + real_drone.curlength.ToString();
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
                    if (mouse.MiddleButton == ButtonState.Pressed || keyboard.IsKeyDown(Keys.E))
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
            if (!session_loading && route_building_indexes.Count > 0)
            {
                if (!flight) real_drone.GetRangeFindersDebug(real_drone.index_pos, points_on_track[real_drone.index_pos], real_drone.rotation);
                else
                {
                    real_drone.DirectCommand(keyboard);
                    real_drone.CalculateMovement(1);
                    real_drone.GetRangeFindersDebug(real_drone.index_pos, real_drone.position, real_drone.rotation);
                    if (Program.center_drone) map.center = MapMath.ScreenToCoordinates(MapMath.LongLatToScreen(real_drone.position, map.center, map.level), map.center, map.level).ToVector2();
                }
            }    
        }

        int prev_drpos = -1, cnt_b = 0;

        public void Draw()
        {
            map.Draw();
            debuglabel.text = "Debug: " + Program.debug;
            iterate_blabel.text = "Iterate buildings: " + Program.iterate_b;
            graphicslabel.text = "Save graphics: " + Program.save_graphics;
            center_dlabel.text = "Center drone: " + Program.center_drone;
            if (progress_b != 3) makeprogresslabel.text = loadprogresslabel.text = progress_b.ToString() + "/3";
            else makeprogresslabel.text = loadprogresslabel.text = "";
            if (!session_loading)
            {
                foreach (PointD point in trackpoints)
                {
                    Vector2 pos = MapMath.LongLatToScreen(point, map.center, map.level);
                    if (pos.InRect(MapMath.start_screen, MapMath.end_screen))
                        Program.spriteBatch.Draw(trackpoint, pos, null, Microsoft.Xna.Framework.Color.White, 0f, new Vector2(11.5f, 11.5f), MathF.Pow(1.1f, map.level - 15), SpriteEffects.None, 1);
                }
                if (trackpoints.Count > 2)
                {
                    var lp = Drone.GetTrack(new BeizerCurve(trackpoints), MapMath.ScreenToLongLat(MapMath.start_screen, map.center, map.level), MapMath.ScreenToLongLat(MapMath.end_screen, map.center, map.level));
                    for (int i = 0; i < lp.Count - 1; i++)
                    {
                        Vector2 p0 = MapMath.LongLatToScreen(lp[i], map.center, map.level), p1 = MapMath.LongLatToScreen(lp[i + 1], map.center, map.level);
                        if (MapMath.LineInScreen(p0, p1)) Program.spriteBatch.DrawLine(p0, p1, Color.Red, Math.Max(2, 4 * MathF.Pow(1.2f, map.level - 15)));
                    }
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
                    if (!flight && Program.iterate_b)
                    {
                        if (real_drone.index_pos != prev_drpos)
                        {
                            cnt_b = 0;
                            prev_drpos = real_drone.index_pos;
                        }
                        cnt_b = Math.Min(cnt_b, route_building_indexes[real_drone.index_pos].Count);
                    }
                    else cnt_b = route_building_indexes[real_drone.index_pos].Count;
                    for (int c = 0; c < cnt_b; c++)
                    {
                        var building = route_buildings[route_building_indexes[real_drone.index_pos][c]];
                        for (int i = 0; i < building.Count; i++)
                        {
                            Vector2 stpos = MapMath.LongLatToScreen(building[i], map.center, map.level);
                            Vector2 fnpos = MapMath.LongLatToScreen(building[(i + 1) % building.Count], map.center, map.level);
                            if (MapMath.LineInScreen(stpos, fnpos)) GraphicsPrimitives.DrawLine(Program.spriteBatch, stpos, fnpos, Color.Red, MathF.Pow(1.2f, map.level - 12 + 1));
                        }
                    }
                    cnt_b++;
                    Program.spriteBatch.Draw(trackpoint, MapMath.LongLatToScreen(points_on_track[real_drone.index_pos], map.center, map.level), null, Microsoft.Xna.Framework.Color.White, 0f, new Vector2(11.5f, 11.5f), MathF.Pow(1.1f, map.level - 15), SpriteEffects.None, 1);
                    foreach (var item in real_drone.debug_bbounds)
                    {
                        Program.spriteBatch.Draw(building_edge, MapMath.LongLatToScreen(item, map.center, map.level), null, Microsoft.Xna.Framework.Color.White, 0f, new Vector2(10f, 10f), 0.5f * MathF.Pow(1.5f, map.level - 14), SpriteEffects.None, 1);
                    }
                    real_drone.debug_bbounds.Clear();
                    if (flight)
                    {
                        foreach (PointD hit_p in real_drone.debug_hitpoints)
                        {
                            if (hit_p != PointD.Empty) Program.spriteBatch.DrawLine(MapMath.LongLatToScreen(real_drone.position, map.center, map.level), MapMath.LongLatToScreen(hit_p, map.center, map.level), Color.Green);
                        }
                        real_drone.debug_hitpoints.Clear();
                    }
                }
            }
            Program.spriteBatch.Draw(map_mask, new Vector2(0, 0), Color.White);
            foreach (var item in elements) item.Draw(Program.spriteBatch);
            if (flight)
            {
                dronespeedlabel.text = "Drone speed:" + (real_drone.speed * 60 * 3.6).ToString("#.#") + "km/h";
//                Program.spriteBatch.Draw(drone_arrow, MapMath.LongLatToScreen(real_drone.position, map.center, map.level), null, Color.Black, (float)real_drone.rotation - MathF.PI / 2, new Vector2(37.5f, 37.5f), MathF.Pow(1.3f, map.level - 15), SpriteEffects.None, 1);
                Program.spriteBatch.Draw(gforce_meter, new Vector2(170, 1000), null, Color.White, 0, new Vector2(50, 50), 1f, SpriteEffects.None, 1);
                Program.spriteBatch.Draw(gforce_ball, new Vector2((float)(170 + Math.Pow(2, 1/ 0.61728395061) * 10 * Math.Sin(2)), (float)(1000 - Math.Pow(2, 1 / 0.61728395061) * 10 * Math.Cos(2))), null, Color.White, 0, new Vector2(4.5f, 4.5f), 1f, SpriteEffects.None, 1);
                Program.spriteBatch.Draw(angle_indicator, new Vector2(60, 1005), null, Color.White, 0, new Vector2(42, 38), 1.1f, SpriteEffects.None, 1);
                Program.spriteBatch.Draw(angle_indicator_line, new Vector2(60, 1005), null, Color.White, (float)real_drone.roll, new Vector2(29, 9), 1.1f, SpriteEffects.None, 1);
                Program.spriteBatch.Draw(rotation_bar, new Vector2(60, 1005), null, Color.White, (float)real_drone.target_a, new Vector2(2, 39), 1f, SpriteEffects.None, 1);
                Program.spriteBatch.Draw(drone_arrow, MapMath.LongLatToScreen(abstract_drone.position, map.center, map.level), null, new Color(0, 0, 0, 128), (float)abstract_drone.rotation - MathF.PI / 2, new Vector2(37.5f, 37.5f), MathF.Pow(1.3f, map.level - 15), SpriteEffects.None, 1);
                Program.spriteBatch.Draw(abstract_drone.debug_matrice, new Vector2(300, 500), Color.White);
            }
        }
    }
}
 