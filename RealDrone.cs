using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoHelper;
using System;
using System.Collections.Generic;
using System.Drawing.Printing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AIlanding;

namespace AIbuilding
{
    internal class RealDrone
    {
        List<BuildingRepresentation> buildings = new List<BuildingRepresentation>();
        //List<Pair<PointD, List<int>>> route_building_indexes = new List<Pair<PointD, List<int>>>();
        List<List<int>> route_building_indexes = new List<List<int>>();
        public DroneINS INS = new DroneINS(12, 1000);
        BeizerCurve track;
        public bool alive = true;
        public PointD position = new PointD(0,0);
        public int index_pos = 0;
        public double rotation = 0;
        public double curlength;
        public double roll = 0d;
        double rolla = 0.0002d;
        private double rolls = 0;
        double maxs = 1;
        public double speed = 0;
        double acc = 0.003;
        double rotdes = 0.003d;
        double roteff = 0.006d;
        double maxa = 0.6;
        double air_res = 1*0.003;
        public double target_a;
        public int ray_count = 20;
        int loops_per_second = 5;
        public Texture2D debug_matrice;


        public RealDrone(List<BuildingRepresentation> buildings, List<List<int>> route_building_indexes, BeizerCurve track) 
        {
            debug_matrice = new Texture2D(Program.my_device, 600, 100);
            this.buildings = buildings;
            this.track = track;
            this.route_building_indexes = route_building_indexes;
            this.rotation = MapMath.AngleLongLat(track.segments[0].GetPoint(0), track.segments[0].GetPoint(0.01));
            //this.rotation = Math.PI / 2;
            position = track.segments[0].GetPoint(0);
        }

        public RealDrone() 
        {
            air_res = 2 / maxs;
        }

        public void CalculateMovement(double thrust)
        {
            //PointD prev_pos = position; double prev_rotation = rotation, prev_speed = speed;
            PointD alter_pos = MapMath.RotateLongtLat(position, speed, rotation);
            double alt_rotation = rotation + roll * roteff;
            if (target_a > maxa) target_a = maxa;
            if (target_a < -maxa) target_a = -maxa;
            double minabs = 1000, minrolls = 0;
            for (int i = -1; i <= 1; i++)
            {
                double prolls = rolls + i * rolla;
                double prolla = -rolla * Math.Sign(prolls);
                double n = Math.Abs(prolls / rolla);
                double ps = Math.Abs((2 * prolls + prolla * (n - 1)) / 2 * n - (target_a - roll));
                if (prolla == 0) ps = Math.Abs(target_a - roll);
                if (ps < minabs) { minabs = ps; minrolls = prolls; }
            }
            rolls = minrolls;
            roll += rolls;

            speed -= speed * Math.Abs(roll) * rotdes;
            rotation += roll * roteff;
            speed -= speed * air_res;
            speed += acc;
            position = MapMath.RotateLongtLat(position, speed, rotation);

            var next_points = Drone.GetTrack(track, 1, 101, curlength);
            double mind = double.MaxValue; int cur_lt = 0;
            for (int i = 0; i < Math.Min(100, next_points.Count-1); i++)
            {
                double cur_d = MapMath.DistanceLongLat(position, next_points[i+1]);
                if (cur_d < mind) { cur_lt = i; mind = cur_d; }
            }
            curlength += cur_lt;
            index_pos = (int)Math.Round(curlength / 50);
            rotation %= 2*Math.PI;
            
            INS.CalcSpeed(new PointD(0, MapMath.DistanceLongLat(alter_pos, position)).Turn(MapMath.AngleLongLat(alter_pos, position) - rotation), rotation - alt_rotation);
        }



        public void DirectCommand(KeyboardState keyboard)
        {
            if (keyboard.IsKeyDown(Keys.Left))
            {
                target_a -= 0.02;
            }
            if (keyboard.IsKeyDown(Keys.Right))
            {
                target_a += 0.02;
            }
        }

        public DroneINS abstract_INS = new DroneINS(12, 1000);
        public List<double> abstract_RangeFinders = new List<double>();

        public void CalculateMovementAbstract()
        {
            PointD prev_pos = position;
            PointD meter_pos = abstract_INS.res_pos;
            double prev_rot = rotation, difr;
            lock (abstract_INS)
            {
                if (abstract_INS.res_t == 0) return;
                position = MapMath.RotateLongtLat(position, meter_pos.Length(), meter_pos.Angle());
                rotation += abstract_INS.res_rot;
                difr = MHeleper.Normalize(rotation, 2 * Math.PI) - MHeleper.Normalize(prev_rot, 2 * Math.PI);
                if (Math.Abs(difr) > Math.PI) difr = MHeleper.Normalize(rotation + Math.PI, 2 * Math.PI) - MHeleper.Normalize(prev_rot + Math.PI, 2 * Math.PI);
            }
         /*   Color[] cd = new Color[600 * 100];
            var al = abstract_INS.probabilities[12];
            for (int i = 0; i < al.Count; i++)
            {
                cd[(int)Math.Round(al[i] * 100)] = Color.Red;
            }
            debug_matrice.SetData(cd);*/
            abstract_INS.Reset(new PointD(0, MapMath.DistanceLongLat(prev_pos, position)).Turn(MapMath.AngleLongLat(prev_pos, position)), difr);
        }

        public void StartMainLoop()
        {
            abstract_INS.Reset(speed, rotation);
            Thread loop_thread = new Thread(() =>
            {
                MainLoop();
            });
            loop_thread.Start();
        }

        public void MainLoop()
        {
            // 1e7 - ticks in one second
            double time_loop = 1e7 / (double)loops_per_second;
            while (true)
            {
                if (!alive) break;
                DateTime start = DateTime.Now;
                CalculateMovementAbstract();
                Thread.Sleep(new TimeSpan((int)Math.Max(0, time_loop - (DateTime.Now - start).Ticks)));
            }
        }


        public List<PointD> debug_bbounds = new List<PointD>();
        public List<PointD> debug_hitpoints = new List<PointD>();
        public List<double> GetRangeFindersDebug(int chunk, PointD meas_pos, double meas_a)
        {
            debug_hitpoints.Clear();
            List<double> res = new List<double>(ray_count);
            for (int i = 0; i < ray_count; i++) res.Add(double.MaxValue);
            for (int i = 0; i < ray_count; i++) debug_hitpoints.Add(PointD.Empty);
            double ray_interval = Math.PI*2 / (double)ray_count;
            for (int i = 0; i < ray_count; i++)
            {
                PointD intersection_point = MapMath.RotateLongtLat(meas_pos, 1000, meas_a + i * ray_interval);
                debug_hitpoints[i] = intersection_point;
            }
            foreach (int c in route_building_indexes[chunk])
            {
                BuildingBound cur_bound = buildings[c].GetBound(MapMath.AngleLongLat(meas_pos, buildings[c].centroid));
                double rot_width = buildings[c].GetWidth(MapMath.AngleLongLat(meas_pos, buildings[c].centroid) - Math.PI / 2);
                double dist_b = MapMath.DistanceLongLat(meas_pos, buildings[c].centroid);
                debug_bbounds.Add(cur_bound.leftp); debug_bbounds.Add(cur_bound.rightp);
                int iter_st = 0, iter_fn = ray_count - 1;
                if (dist_b > rot_width)
                {
                    double st_a = MapMath.AngleLongLat(meas_pos, cur_bound.leftp) - 0.1;
                    double fn_a = MapMath.AngleLongLat(meas_pos, cur_bound.rightp) + 0.1;
                    iter_st = (((int)Math.Ceiling((st_a - meas_a) / ray_interval) % ray_count) + ray_count) % ray_count;
                    iter_fn = (((int)Math.Floor((fn_a - meas_a) / ray_interval) % ray_count) + ray_count) % ray_count;
                    if (ray_count - iter_st + iter_fn < ray_count / 2)
                    {
                        iter_st -= ray_count;
                    }    
                }
                for (int i = iter_st; i <= iter_fn; i++) 
                {
                    int rr = (i % ray_count + ray_count) % ray_count;
                    if (res[rr] > dist_b - rot_width)
                    {
                        for (int j = 0; j < buildings[c].points.Count; j++)
                        {
                            PointD intersection_point = Drone.FindLineIntersection(meas_pos, MapMath.RotateLongtLat(meas_pos, 10, meas_a + i * ray_interval), buildings[c].points[j], buildings[c].points[(j + 1) % buildings[c].points.Count]);
                            if (intersection_point != PointD.Empty)
                            {
                                double c_dist = MapMath.DistanceLongLat(meas_pos, intersection_point);
                                if (c_dist < res[rr] && c_dist < 1000)
                                {
                                    debug_hitpoints[rr] = intersection_point;
                                    res[rr] = c_dist;
                                }
                            }
                        }
                    }
                }
            }
            return res;
        }

        public List<double> GetRangeFinders(int chunk, PointD meas_pos, double meas_a)
        {
            List<double> res = new List<double>(ray_count);
            for (int i = 0; i < ray_count; i++) res.Add(double.MaxValue);
            double ray_interval = Math.PI * 2 / (double)ray_count;
            foreach (int c in route_building_indexes[chunk])
            {
                BuildingBound cur_bound = buildings[c].GetBound(MapMath.AngleLongLat(meas_pos, buildings[c].centroid));
                double rot_width = buildings[c].GetWidth(MapMath.AngleLongLat(meas_pos, buildings[c].centroid) - Math.PI / 2);
                double dist_b = MapMath.DistanceLongLat(meas_pos, buildings[c].centroid);
                int iter_st = 0, iter_fn = ray_count - 1;
                if (dist_b > rot_width)
                {
                    double st_a = MapMath.AngleLongLat(meas_pos, cur_bound.leftp) - 0.1;
                    double fn_a = MapMath.AngleLongLat(meas_pos, cur_bound.rightp) + 0.1;
                    iter_st = (((int)Math.Ceiling((st_a - meas_a) / ray_interval) % ray_count) + ray_count) % ray_count;
                    iter_fn = (((int)Math.Floor((fn_a - meas_a) / ray_interval) % ray_count) + ray_count) % ray_count;
                    if (ray_count - iter_st + iter_fn < ray_count / 2)
                    {
                        iter_st -= ray_count;
                    }
                }
                for (int i = iter_st; i <= iter_fn; i++)
                {
                    int rr = (i % ray_count + ray_count) % ray_count;
                    if (res[rr] > dist_b - rot_width)
                    {
                        for (int j = 0; j < buildings[c].points.Count; j++)
                        {
                            PointD intersection_point = Drone.FindLineIntersection(meas_pos, MapMath.RotateLongtLat(meas_pos, 10, meas_a + i * ray_interval), buildings[c].points[j], buildings[c].points[(j + 1) % buildings[c].points.Count]);
                            if (intersection_point != PointD.Empty)
                            {
                                double c_dist = MapMath.DistanceLongLat(meas_pos, intersection_point);
                                if (c_dist < res[rr] && c_dist < 1000) res[rr] = c_dist;
                            }
                        }
                    }
                }
            }
            return res;
        }
    }
}
