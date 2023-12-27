using AIlanding;
using FormElementsLib;
using MathNet.Numerics;
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
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Serialization;

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
        Texture2D posdeviation_rect; Color[] posdeviation_cd;
        Button makeroutebutton = new Button(Program.my_device, 10, 400, 150, 40, "MAKE ROUTE", Program.font15, 15, 15);
        Button launchbutton = new Button(Program.my_device, 10, 530, 150, 40, "LAUNCH", Program.font15, 15, 15);
        Button sessionbutton = new Button(Program.my_device, 350, 197, 120, 30, "SELECT", Program.font15, 15, 13);
        Button loadroutebutton = new Button(Program.my_device, 350, 262, 120, 30, "LOAD", Program.font15, 15, 13);
        Button saveroutebutton = new Button(Program.my_device, 350, 297, 120, 30, "SAVE", Program.font15, 15, 13);
        Button loaddronebutton = new Button(Program.my_device, 350, 662, 120, 30, "LOAD", Program.font15, 15, 13);
        Button savedronebutton = new Button(Program.my_device, 350, 697, 120, 30, "SAVE", Program.font15, 15, 13);
        //Button calculateroutebutton = new Button(Program.my_device, 300, 400, 250, 40, "CALCULATE ROUTE", Program.font15, 15, 15);
        Label sessionlabel = new Label(10, 200, -1, -1, "Session name:", Program.font15, 15, 15, Color.Red.PackedValue);
        Label loadroutelabel = new Label(10, 265, -1, -1, "Load route :", Program.font15, 15, 15, Color.Red.PackedValue);
        Label saveroutelabel = new Label(10, 300, -1, -1, "Save route :", Program.font15, 15, 15, Color.Red.PackedValue);
        Label loaddronelabel = new Label(10, 665, -1, -1, "Load drone :", Program.font15, 15, 15, Color.Red.PackedValue);
        Label savedronelabel = new Label(10, 700, -1, -1, "Save drone :", Program.font15, 15, 15, Color.Red.PackedValue);
        Label lengthroutelabel = new Label(10, 455, -1, -1, "Route length: 0m", Program.font15, 15, 15, Color.Red.PackedValue);
        Label positionroutelabel = new Label(10, 485, -1, -1, "Current position : 0m", Program.font15, 15, 15, Color.Red.PackedValue);
        Label loadprogresslabel = new Label(480, 265, -1, -1, "5/5", Program.font15, 15, 15, Color.Red.PackedValue);
        Label makeprogresslabel = new Label(170, 410, -1, -1, "5/5", Program.font15, 15, 15, Color.Red.PackedValue);
        Label dronespeedlabel = new Label(10, 580, -1, -1, "Drone speed: 179km/h", Program.font15, 15, 15, Color.Red.PackedValue);
        Label debuglabel = new Label(10, 1050, -1, -1, "Debug: True", Program.font15, 15, 12, Color.Red.PackedValue);
        Label graphicslabel = new Label(150, 1050, -1, -1, "Save graphics: True", Program.font15, 15, 12, Color.Red.PackedValue);
        Label center_dlabel = new Label(350, 1050, -1, -1, "Center drone: False", Program.font15, 15, 12, Color.Red.PackedValue);
        Label manual_dlabel = new Label(550, 1050, -1, -1, "Manual control: False", Program.font15, 15, 12, Color.Red.PackedValue);
        Label dSlabel = new Label(550, 560, -1, -1, "Position deviation: 10.58m", Program.font15, 15, 12, Color.Red.PackedValue);
        Label dRlabel = new Label(550, 580, -1, -1, "Rotation deviation: 2.82deg", Program.font15, 15, 12, Color.Red.PackedValue);
        Label maxspeedlabel = new Label(10, 745, -1, -1, "Max speed (km/h):", Program.font15, 15, 12, Color.Red.PackedValue);
        Label acclabel = new Label(10, 775, -1, -1, "Acceleration (m/s):", Program.font15, 15, 12, Color.Red.PackedValue);
        Label maxrolllabel = new Label(10, 805, -1, -1, "Max. roll (deg):", Program.font15, 15, 12, Color.Red.PackedValue);
        Label rollalabel = new Label(10, 835, -1, -1, "Roll acceleration (deg/s):", Program.font15, 15, 12, Color.Red.PackedValue);
        Label rotefflabel = new Label(10, 865, -1, -1, "Rotation/roll coeficient:", Program.font15, 15, 12, Color.Red.PackedValue);
        Label rotdeslabel = new Label(10, 895, -1, -1, "dSpeed/roll coeficient:", Program.font15, 15, 12, Color.Red.PackedValue);
        //Label rotdeslabel = new Label(10, 895, -1, -1, "dSpeed/roll coeficient:", Program.font15, 15, 12, Color.Red.PackedValue);
        Label loopspersecondlabel = new Label(500, 715, -1, -1, "Loops per second:", Program.font15, 15, 12, Color.Red.PackedValue);
        Label raycountlabel = new Label(500, 745, -1, -1, "Ray count:", Program.font15, 15, 12, Color.Red.PackedValue);
        Label raylengthlabel = new Label(500, 775, -1, -1, "Max ray distance (m):", Program.font15, 15, 12, Color.Red.PackedValue);
        Label posmultlabel = new Label(500, 805, -1, -1, "Position coeficient:", Program.font15, 15, 12, Color.Red.PackedValue);
        Label rotmultlabel = new Label(500, 835, -1, -1, "Rotation coeficient:", Program.font15, 15, 12, Color.Red.PackedValue);
        Label raypowlabel = new Label(500, 865, -1, -1, "Pos coef/rays magnitude:", Program.font15, 15, 12, Color.Red.PackedValue);
        Label displayscalelabel = new Label(500, 895, -1, -1, "Display scale (m):", Program.font15, 15, 12, Color.Red.PackedValue);
        Label ignorebestlabel = new Label(500, 925, -1, -1, "Ignore best:", Program.font15, 15, 12, Color.Red.PackedValue);
        Label ignoreworstlabel = new Label(500, 955, -1, -1, "Ignore worst:", Program.font15, 15, 12, Color.Red.PackedValue);
        Label rangefinderserrorlabel = new Label(500, 985, -1, -1, "Rangefinders error (m):", Program.font15, 15, 12, Color.Red.PackedValue);
        TextBox sessiontextbox = new TextBox(Program.my_device, 145, 197, 200, Program.font15, 15, 15);
        TextBox loadroutetextbox = new TextBox(Program.my_device, 145, 262, 200, Program.font15, 15, 15);
        TextBox saveroutetextbox = new TextBox(Program.my_device, 145, 297, 200, Program.font15, 15, 15);
        TextBox loaddronetextbox = new TextBox(Program.my_device, 145, 662, 200, Program.font15, 15, 15);
        TextBox savedronetextbox = new TextBox(Program.my_device, 145, 697, 200, Program.font15, 15, 15);
        TextBox maxspeedtextbox = new TextBox(Program.my_device, 195, 742, 100, Program.font15, 15, 12);
        TextBox acctextbox = new TextBox(Program.my_device, 195, 772, 100, Program.font15, 15, 12);
        TextBox maxrolltextbox = new TextBox(Program.my_device, 195, 802, 100, Program.font15, 15, 12);
        TextBox rollatextbox = new TextBox(Program.my_device, 195, 832, 100, Program.font15, 15, 12);
        TextBox rotefftextbox = new TextBox(Program.my_device, 195, 862, 100, Program.font15, 15, 12);
        TextBox rotdestextbox = new TextBox(Program.my_device, 195, 892, 100, Program.font15, 15, 12);
        TextBox loopspersecondtextbox = new TextBox(Program.my_device, 695, 712, 100, Program.font15, 15, 12);
        TextBox raycounttextbox = new TextBox(Program.my_device, 695, 742, 100, Program.font15, 15, 12);
        TextBox raylengthtextbox = new TextBox(Program.my_device, 695, 772, 100, Program.font15, 15, 12);
        TextBox posmulttextbox = new TextBox(Program.my_device, 695, 802, 100, Program.font15, 15, 12);
        TextBox rotmulttextbox = new TextBox(Program.my_device, 695, 832, 100, Program.font15, 15, 12);
        TextBox raypowtextbox = new TextBox(Program.my_device, 695, 862, 100, Program.font15, 15, 12);
        TextBox displayscaletextbox = new TextBox(Program.my_device, 695, 892, 100, Program.font15, 15, 12);
        TextBox ignorebesttextbox = new TextBox(Program.my_device, 695, 922, 100, Program.font15, 15, 12);
        TextBox ignoreworsttextbox = new TextBox(Program.my_device, 695, 952, 100, Program.font15, 15, 12);
        TextBox rangefinderserrortextbox = new TextBox(Program.my_device, 695, 982, 100, Program.font15, 15, 12);
        KeySwitch keyd = new KeySwitch(Keys.D), keyc = new KeySwitch(Keys.C), keyg = new KeySwitch(Keys.G), keym = new KeySwitch(Keys.M);
        List<KeySwitch> switches;
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
        DroneCharacteristics characteristics = new DroneCharacteristics();

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
            posdeviation_rect = new Texture2D(Program.my_device, 200, 200);
            posdeviation_cd = new Color[200 * 200]; 
            for (int x = 0; x < 200; x++)
            {
                for (int y = 0; y < 200; y++)
                {
                    if (x < 5 || x >= 195 || y < 5 || y > 195) posdeviation_cd[y * 200 + x] = Color.Green;
                    else posdeviation_cd[y * 200 + x] = Color.Black;
                }
            }
            posdeviation_rect.SetData(posdeviation_cd);
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
            savedronetextbox.text = Program.setupProp["drones_name"];
            loaddronetextbox.text = Program.setupProp["dronel_name"];
            makeroutebutton.Click += Makeroutebutton_Click;
            sessionbutton.Click += Sessionbutton_Click;
            loadroutebutton.Click += Loadroutebutton_Click;
            saveroutebutton.Click += Saveroutebutton_Click;
            launchbutton.Click += Launchbutton_Click;
            loaddronebutton.Click += Loaddronebutton_Click;
            savedronebutton.Click += Savedronebutton_Click;
            keyd.state_changed += Keyd_state_changed;
            elements = new List<FormElement>() { loopspersecondlabel, loopspersecondtextbox, ignorebesttextbox, ignoreworsttextbox, ignorebestlabel, rangefinderserrortextbox, rangefinderserrorlabel, ignorebestlabel, ignoreworstlabel, displayscalelabel, displayscaletextbox, raypowtextbox, raypowlabel, manual_dlabel, center_dlabel, rotmultlabel, rotmulttextbox, posmultlabel, posmulttextbox, raylengthlabel, raylengthtextbox, raycountlabel, raycounttextbox, rotdeslabel, rotdestextbox, rotefflabel, rotefftextbox, maxrolllabel, maxrolltextbox, acclabel, acctextbox, rollalabel, rollatextbox, maxspeedlabel, maxspeedtextbox, loaddronelabel, loaddronebutton, loaddronetextbox, savedronelabel, savedronebutton, savedronetextbox, savedronetextbox, dSlabel, dRlabel, debuglabel,  graphicslabel, dronespeedlabel, makeprogresslabel, loadprogresslabel, launchbutton, makeroutebutton, sessionbutton, loadroutebutton, saveroutebutton, sessionlabel, sessiontextbox, loadroutelabel, saveroutelabel, loadroutetextbox, saveroutetextbox, lengthroutelabel, positionroutelabel };
            switches = new List<KeySwitch>() { keyd, keyc, keyg, keym };
            SetCharacteristics();
        }

        private void SetCharacteristics()
        {
            maxspeedtextbox.text = (characteristics.maxs * 60 * 3.6).ToString("0.####");
            acctextbox.text = (characteristics.acc * 60 * 60).ToString("0.####");
            maxrolltextbox.text = (characteristics.maxa / Math.PI * 180).ToString("0.####");
            rollatextbox.text = (characteristics.rolla * 60 * 60 / Math.PI * 180).ToString("0.####");
            rotefftextbox.text = (characteristics.roteff * 60 / Math.PI * 180).ToString("0.####");
            rotdestextbox.text = (characteristics.rotdes * 60 / Math.PI * 180).ToString("0.####");
            loopspersecondtextbox.text = characteristics.loops_per_second.ToString("0.####");
            raycounttextbox.text = characteristics.raycount.ToString("0.####");
            raylengthtextbox.text = characteristics.raylength.ToString("0.####");
            posmulttextbox.text = characteristics.pos_mult.ToString("0.####");
            rotmulttextbox.text = characteristics.rot_mult.ToString("0.####");
            raypowtextbox.text = characteristics.raypow.ToString("0.####");
            displayscaletextbox.text = characteristics.displayscale.ToString("0.#####");
            ignorebesttextbox.text = characteristics.ignorebest.ToString("0.####");
            ignoreworsttextbox.text = characteristics.ignoreworst.ToString("0.####");
            rangefinderserrortextbox.text = characteristics.rangefiders_error.ToString("0.####");
        }

        private bool AssignCharacteristics()
        {
            bool success = true;
            DroneCharacteristics tcharacteristics = new DroneCharacteristics(rollatextbox, maxspeedtextbox, acctextbox, rotdestextbox, rotefftextbox, maxrolltextbox, raycounttextbox, raylengthtextbox, posmulttextbox, rotmulttextbox, raypowtextbox, displayscaletextbox, ignorebesttextbox, ignoreworsttextbox, rangefinderserrortextbox, loopspersecondtextbox, out success);
            if (success)
            {
                characteristics = tcharacteristics;
            }
            return success;
        }

        private void Savedronebutton_Click(object sender, ClickEventArgs e)
        {
            if (AssignCharacteristics())
            {
                Program.setupProp["drones_name"] = savedronetextbox.text;
                Program.ChangeSetup();
                characteristics.SaveToFile(savedronetextbox.text);
                if (!flight) real_drone.characteristics = abstract_drone.characteristics = characteristics;
            }
        }

        private void Loaddronebutton_Click(object sender, ClickEventArgs e)
        {
            if (flight) return;
            loaddronetextbox.MakeAction(new Action(() => 
            {
                Program.setupProp["dronel_name"] = loaddronetextbox.text;
                Program.ChangeSetup();
                characteristics = DroneCharacteristics.FromFile(loaddronetextbox.text);
                real_drone.characteristics = abstract_drone.characteristics = characteristics;
                SetCharacteristics();
            }), null);
        }

        private void Keyd_state_changed(object sender, EventArgs e)
        {
            cnt_b = 0;
        }

        private void Launchbutton_Click(object sender, ClickEventArgs e)
        {
            if (session_loading || route_building_indexes.Count == 0 || trackpoints.Count == 0) return;
            flight = !flight;
            real_drone = new RealDrone(building_representation, route_building_indexes, new BeizerCurve(trackpoints));
            if (flight)
            {
                abstract_drone = new RealDrone(building_representation, route_building_indexes, new BeizerCurve(trackpoints));
                abstract_drone.abstract_INS = real_drone.INS;
                if (!AssignCharacteristics()) return;
                real_drone.characteristics = abstract_drone.characteristics = characteristics;
                abstract_drone.speed =  real_drone.speed = 0.9 * characteristics.maxs;
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

        int lolres = 0;

        void LOL(ref int a, string s)
        {
            a = Convert.ToInt32(s);
        }

        

        private void Loadroutebutton_Click(object sender, ClickEventArgs e)
        {
            if (session_loading || flight) return;
            Program.setupProp["routel_name"] = loadroutetextbox.text;
            Program.ChangeSetup();
            Thread loadroute_thread = new Thread(() =>
            {
                session_loading = true;
                track_changed = true;
                loadroutetextbox.changecolor_actionfinally = () =>
                {
                    session_loading = false;
                };
                loadroutetextbox.changecolor_action = () =>
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
                };
                loadroutetextbox.MakeAction();
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


        void SetColor(ref Color[] cd, PointD point, Color color)
        {
            PointD realpos = point * 2000;
            int x = (int)Math.Round(realpos.X) + 100, y = -(int)Math.Round(realpos.Y) + 100;
            if ((x >= 0) && (x < 200) && (y >= 0) && (y < 200)) cd[y * 200 + x] = color;
        }

        PointD scalc_p; double scalc_rot;
        public void Run(MouseState mouse, KeyboardState keyboard)
        {
            if (abstract_drone.debug_abstract_scalc)
            {
                scalc_p = real_drone.position;
                scalc_rot = real_drone.rotation;
                abstract_drone.debug_abstract_scalc = false;
            }
            if (abstract_drone.debug_abstract_fcalc)
            {
                dSlabel.text = "Position deviation: " + MapMath.DistanceLongLat(abstract_drone.position, scalc_p).Round(3);
                dRlabel.text = "Rotation deviation: " + MHeleper.AngleDif(abstract_drone.rotation, scalc_rot).ToDegrees().Round(2);
                Color[] cd = new Color[200 * 200];
                posdeviation_cd.CopyTo(cd, 0);
                foreach (var pos in abstract_drone.debug_abstract_posdev)
                {
                    SetColor(ref cd, pos, Color.Blue);
                }
                PointD meter_pos = new PointD(0, MapMath.DistanceLongLat(abstract_drone.position, scalc_p)).Turn(MapMath.AngleLongLat(abstract_drone.position, scalc_p));
                SetColor(ref cd, abstract_drone.posch, Color.Red);
                SetColor(ref cd, meter_pos, Color.Yellow);
                posdeviation_rect.SetData(cd);
                if (!keym.state) real_drone.target_a = abstract_drone.target_a;
                abstract_drone.debug_abstract_fcalc = false;
            }
            map.Run(mouse, keyboard);
            if (MHeleper.ApplicationIsActivated())
            {
                foreach (var item in switches) item.Run(keyboard);
                if (!keyd.state)
                {
                    keyc.state = true;
                    keyg.state = flight;
                    keym.state = false;
                }
                graphicslabel.visible = keyd.state;
                center_dlabel.visible = keyd.state;
                manual_dlabel.visible = keyd.state;
                Program.save_graphics = keyg.state;
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
                        else if (point_lock < trackpoints.Count) trackpoints[point_lock] = MapMath.ScreenToLongLat(mouse.Position.ToVector2(), map.center, map.level);
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
                    if (keym.state) real_drone.DirectCommand(keyboard);
                    real_drone.CalculateMovement(1);
                    abstract_drone.abstract_RangeFinders = real_drone.GetRangeFindersDebug(real_drone.index_pos, real_drone.position, real_drone.rotation);
                    if (keyc.state) map.center = MapMath.ScreenToCoordinates(MapMath.LongLatToScreen(real_drone.position, map.center, map.level), map.center, map.level).ToVector2();
                }
            }    
        }

        int prev_drpos = -1, cnt_b = 0;

        public void Draw()
        {
            map.Draw();
            debuglabel.text = "Debug [D]: " + keyd.state;
            graphicslabel.text = "Save graphics [G]: " + keyg.state;
            center_dlabel.text = "Center drone [C]: " + keyc.state;
            manual_dlabel.text = "Manual control [M]: " + keym.state;
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
                if (!keyg.state) foreach (var building in route_buildings)
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
                    if (!flight && keyd.state)
                    {
                        if (real_drone.index_pos != prev_drpos)
                        {
                            cnt_b = 0;
                            prev_drpos = real_drone.index_pos;
                        }
                        cnt_b = Math.Min(cnt_b, route_building_indexes[real_drone.index_pos].Count);
                    }
                    else cnt_b = route_building_indexes[real_drone.index_pos].Count;
                    if (keyd.state || flight)
                    {
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
                        Program.spriteBatch.Draw(trackpoint, MapMath.LongLatToScreen(points_on_track[real_drone.index_pos], map.center, map.level), null, Microsoft.Xna.Framework.Color.White, 0f, new Vector2(11.5f, 11.5f), MathF.Pow(1.1f, map.level - 15), SpriteEffects.None, 1);
                        foreach (var item in real_drone.debug_bbounds)
                        {
                            Program.spriteBatch.Draw(building_edge, MapMath.LongLatToScreen(item, map.center, map.level), null, Microsoft.Xna.Framework.Color.White, 0f, new Vector2(10f, 10f), 0.5f * MathF.Pow(1.5f, map.level - 14), SpriteEffects.None, 1);
                        }
                    }
                    cnt_b++;
                    if (flight)
                    {
                        foreach (PointD hit_p in real_drone.debug_hitpoints)
                        {
                            if (hit_p != PointD.Empty) Program.spriteBatch.DrawLine(MapMath.LongLatToScreen(real_drone.position, map.center, map.level), MapMath.LongLatToScreen(hit_p, map.center, map.level), Color.Green);
                        }
                    }
                }
            }
            Program.spriteBatch.Draw(map_mask, new Vector2(0, 0), Color.White);
            foreach (var item in elements) item.Draw(Program.spriteBatch);
            if (flight)
            {
                dronespeedlabel.text = "Drone speed:" + (real_drone.speed * 60 * 3.6).ToString("#.#") + "km/h";
                Program.spriteBatch.Draw(gforce_meter, new Vector2(170, 1000), null, Color.White, 0, new Vector2(50, 50), 1f, SpriteEffects.None, 1);
                Program.spriteBatch.Draw(gforce_ball, new Vector2((float)(170 + Math.Pow(2, 1/ 0.61728395061) * 10 * Math.Sin(2)), (float)(1000 - Math.Pow(2, 1 / 0.61728395061) * 10 * Math.Cos(2))), null, Color.White, 0, new Vector2(4.5f, 4.5f), 1f, SpriteEffects.None, 1);
                Program.spriteBatch.Draw(angle_indicator, new Vector2(60, 1005), null, Color.White, 0, new Vector2(42, 38), 1.1f, SpriteEffects.None, 1);
                Program.spriteBatch.Draw(angle_indicator_line, new Vector2(60, 1005), null, Color.White, (float)real_drone.roll, new Vector2(29, 9), 1.1f, SpriteEffects.None, 1);
                Program.spriteBatch.Draw(rotation_bar, new Vector2(60, 1005), null, Color.White, (float)real_drone.target_a, new Vector2(2, 39), 1f, SpriteEffects.None, 1);
                Program.spriteBatch.Draw(drone_arrow, MapMath.LongLatToScreen(abstract_drone.position, map.center, map.level), null, new Color(0, 0, 0, 128), (float)abstract_drone.rotation - MathF.PI / 2, new Vector2(37.5f, 37.5f), MathF.Pow(1.3f, map.level - 15), SpriteEffects.None, 1);
                Program.spriteBatch.Draw(posdeviation_rect, new Vector2(550, 350), Color.White);
            }
        }
    }
}
 