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
using System.Security.Cryptography.X509Certificates;
using System.ComponentModel.DataAnnotations.Schema;

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
        private double rolls = 0;
        public double speed = 0;
        public double target_a = 0;
        public DroneCharacteristics characteristics = new DroneCharacteristics();

        public RealDrone(List<BuildingRepresentation> buildings, List<List<int>> route_building_indexes, BeizerCurve track) 
        {
            this.buildings = buildings;
            this.track = track;
            this.route_building_indexes = route_building_indexes;
            this.rotation = MapMath.AngleLongLat(track.segments[0].GetPoint(0), track.segments[0].GetPoint(0.01));
            //this.rotation = Math.PI / 2;
            position = track.segments[0].GetPoint(0);
            this.speed = characteristics.maxs * 0.9;
            wanted_angle = this.rotation;
        }

        public RealDrone()
        {

        }

        public void CalculateCurLength()
        {
            var next_points = Drone.GetTrack(track, 1, 101, curlength);
            double mind = double.MaxValue; int cur_lt = 0;
            for (int i = 0; i < Math.Min(100, next_points.Count - 1); i++)
            {
                double cur_d = MapMath.DistanceLongLat(position, next_points[i + 1]);
                if (cur_d < mind) { cur_lt = i; mind = cur_d; }
            }
            curlength += cur_lt;
            index_pos = (int)(curlength / 50);
        }

        public void CalculateMovement(double thrust)
        {
            //PointD prev_pos = position; double prev_rotation = rotation, prev_speed = speed;
            PointD alter_pos = MapMath.RotateLongtLat(position, speed, rotation);
            double alt_rotation = rotation + roll * characteristics.roteff;
            if (target_a > characteristics.maxa) target_a = characteristics.maxa;
            if (target_a < -characteristics.maxa) target_a = -characteristics.maxa;
            double minabs = 1000, minrolls = 0;
            for (int i = -1; i <= 1; i++)
            {
                double prolls = rolls + i * characteristics.rolla;
                double prolla = -characteristics.rolla * Math.Sign(prolls);
                double n = Math.Abs(prolls / characteristics.rolla);
                double ps = Math.Abs((2 * prolls + prolla * (n - 1)) / 2 * n - (target_a - roll));
                if (prolla == 0) ps = Math.Abs(target_a - roll);
                if (ps < minabs) { minabs = ps; minrolls = prolls; }
            }
            rolls = minrolls;
            roll += rolls;

            speed -= speed * Math.Abs(roll) * characteristics.rotdes;
            rotation += roll * characteristics.roteff;
            speed -= speed / characteristics.maxs * characteristics.acc;
            speed += characteristics.acc;
            position = MapMath.RotateLongtLat(position, speed, rotation);

            CalculateCurLength();

            rotation.Normalize(Math.PI * 2);

            INS.AddData(new PointD(0, MapMath.DistanceLongLat(alter_pos, position)).Turn(MapMath.AngleLongLat(alter_pos, position) - rotation), MHeleper.AngleDif(rotation, alt_rotation));

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
        public bool debug_abstract_scalc = false;
        public bool debug_abstract_fcalc = false;
        public List<PointD> debug_abstract_posdev = new List<PointD>();
        public double cur_pos_deviation = 0;
        public PointD posch = new PointD(0, 0);
        public double wanted_angle = 0;


        public void CalculateMovementAbstract(DateTime start_time)
        {
            if (abstract_INS.locked_data.Count == 0 || abstract_RangeFinders.Count == 0) return;
            debug_abstract_scalc = true;
            abstract_INS.ProcessData();

            // ---------------- sync storage -----------------------------------
            PointD prev_pos = position;
            double prev_rot = rotation;
            List<double> current_abstract_RangeFinders = new List<double>(abstract_RangeFinders);
            // ---------------- sync storage -----------------------------------

            PointD meter_pos = abstract_INS.res_pos;
            position = MapMath.RotateLongtLat(position, meter_pos.Length(), meter_pos.Angle());
            rotation += abstract_INS.res_rot;
            
            CalculateCurLength();

            List<PointD> cur_debug_abstract_posdev = new List<PointD>();
            List<double> prob_pos  = new List<double>(); //= abstract_INS.GetMyProbability();

            double min_score = double.MaxValue;
            PointD min_pos = position; double min_rot = rotation;
            var cur_oper_list = abstract_INS.GetMyProbability();
            var debug_cur_oper_list = DroneINS.debug_oper_lists[abstract_INS.res_t - 1];
            int cur_dcol = 0;
            int active_rangefinders = 0;
            foreach (var item in current_abstract_RangeFinders)
            {
                if (item < 10000) active_rangefinders++;
            }
            cur_pos_deviation = abstract_INS.res_t * abstract_INS.pos_deviation * characteristics.pos_mult * Math.Pow(active_rangefinders / (double)characteristics.raycount, characteristics.raypow);
            double cur_rot_deviation = abstract_INS.res_t * abstract_INS.rot_deviation * characteristics.rot_mult * Math.Pow(active_rangefinders / (double)characteristics.raycount, characteristics.raypow);
            HashSet<double> lol_o = new HashSet<double>();
            int sel_i = 0;
            for (int c = 0; c < cur_oper_list.Count; c++)
            {
                if ((cur_dcol != debug_cur_oper_list.Count) && debug_cur_oper_list[cur_dcol].First == c)
                {
                    cur_debug_abstract_posdev.Add(cur_pos_deviation*debug_cur_oper_list[cur_dcol].Second.Turn(rotation));
                    if (debug_cur_oper_list[cur_dcol].Second.X != 0) cur_debug_abstract_posdev.Add(cur_pos_deviation*(new PointD(-1, 1) * debug_cur_oper_list[cur_dcol].Second).Turn(rotation));
                    if (debug_cur_oper_list[cur_dcol].Second.Y != 0) cur_debug_abstract_posdev.Add(cur_pos_deviation*(new PointD(1, -1) * debug_cur_oper_list[cur_dcol].Second).Turn(rotation));
                    if (debug_cur_oper_list[cur_dcol].Second.X != 0  && debug_cur_oper_list[cur_dcol].Second.Y != 0) cur_debug_abstract_posdev.Add(cur_pos_deviation * (new PointD(-1, -1) * debug_cur_oper_list[cur_dcol].Second).Turn(rotation));
                    cur_dcol++;
                }
                PointD meter_proposed_pos = cur_oper_list[c].First * cur_pos_deviation;
                PointD proposed_position = MapMath.RotateLongtLat(position, meter_proposed_pos.Length(), rotation + meter_proposed_pos.Angle());
                double proposed_rotation = rotation + cur_oper_list[c].Second * cur_rot_deviation;
                List<double> res_scan = GetRangeFinders(index_pos, proposed_position, proposed_rotation);
                double difs = 0;
                List<double> diflist = new List<double>();
                for (int i = 0; i < res_scan.Count; i++)
                {
                    diflist.Add(Math.Abs(res_scan[i] - current_abstract_RangeFinders[i]));
                }
                diflist.OrderByDescending(c => c).ToArray();
                for (int i = 0; i < diflist.Count; i++)
                {
                    difs += diflist[i];
                }
                lol_o.Add(difs);
                if (difs < min_score)
                {
                    sel_i = c;
                    min_score = difs;
                    min_pos = proposed_position;
                    min_rot = proposed_rotation;
                    posch = (cur_pos_deviation * cur_oper_list[c].First).Turn(rotation);
                }
            }
            position = min_pos; rotation = min_rot;
            abstract_INS.Reset(new PointD(0, MapMath.DistanceLongLat(prev_pos, position)).Turn(MapMath.AngleLongLat(prev_pos, position)), MHeleper.AngleDif(rotation, prev_rot));
            PointD target_poss = MapMath.RotateLongtLat(position, abstract_INS.sum_v.Length() * 60 * 0.5, abstract_INS.sum_v.Angle());
            PointD target_posf = Drone.GetTrack(track, 1, 2, curlength + abstract_INS.sum_v.Length() * 60 * 3)[0];
            target_a = MHeleper.AngleDif(MapMath.AngleLongLat(target_poss, target_posf), rotation);
            debug_abstract_posdev = cur_debug_abstract_posdev;
            debug_abstract_fcalc = true;
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
            double time_loop = 1e7 / (double)characteristics.loops_per_second;
            while (true)
            {
                if (!alive) break;
                DateTime start = DateTime.Now;
                CalculateMovementAbstract(start);
                var tts = new TimeSpan((int)Math.Max(0, time_loop - (DateTime.Now - start).Ticks));
                Thread.Sleep(tts);
            }
        }



        public List<PointD> debug_bbounds = new List<PointD>();
        public List<PointD> debug_hitpoints = new List<PointD>();
        public List<double> GetRangeFindersDebug(int chunk, PointD meas_pos, double meas_a)
        {
            debug_hitpoints.Clear();
            debug_bbounds.Clear();
            List<double> res = new List<double>(characteristics.raycount);
            for (int i = 0; i < characteristics.raycount; i++) res.Add(10000);
            for (int i = 0; i < characteristics.raycount; i++) debug_hitpoints.Add(PointD.Empty);
            double ray_interval = Math.PI*2 / (double)characteristics.raycount;
            for (int i = 0; i < characteristics.raycount; i++)
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
                int iter_st = 0, iter_fn = characteristics.raycount - 1;
                if (dist_b > rot_width)
                {
                    double st_a = MapMath.AngleLongLat(meas_pos, cur_bound.leftp) - 0.1;
                    double fn_a = MapMath.AngleLongLat(meas_pos, cur_bound.rightp) + 0.1;
                    iter_st = (((int)Math.Ceiling((st_a - meas_a) / ray_interval) % characteristics.raycount) + characteristics.raycount) % characteristics.raycount;
                    iter_fn = (((int)Math.Floor((fn_a - meas_a) / ray_interval) % characteristics.raycount) + characteristics.raycount) % characteristics.raycount;
                    if (characteristics.raycount - iter_st + iter_fn < characteristics.raycount / 2)
                    {
                        iter_st -= characteristics.raycount;
                    }    
                }
                for (int i = iter_st; i <= iter_fn; i++) 
                {
                    int rr = (i % characteristics.raycount + characteristics.raycount) % characteristics.raycount;
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
            List<double> res = new List<double>(characteristics.raycount);
            for (int i = 0; i < characteristics.raycount; i++) res.Add(10000);
            double ray_interval = Math.PI * 2 / (double)characteristics.raycount;
            foreach (int c in route_building_indexes[chunk])
            {
                BuildingBound cur_bound = buildings[c].GetBound(MapMath.AngleLongLat(meas_pos, buildings[c].centroid));
                double rot_width = buildings[c].GetWidth(MapMath.AngleLongLat(meas_pos, buildings[c].centroid) - Math.PI / 2);
                double dist_b = MapMath.DistanceLongLat(meas_pos, buildings[c].centroid);
                int iter_st = 0, iter_fn = characteristics.raycount - 1;
                if (dist_b > rot_width)
                {
                    double st_a = MapMath.AngleLongLat(meas_pos, cur_bound.leftp) - 0.1;
                    double fn_a = MapMath.AngleLongLat(meas_pos, cur_bound.rightp) + 0.1;
                    iter_st = (((int)Math.Ceiling((st_a - meas_a) / ray_interval) % characteristics.raycount) + characteristics.raycount) % characteristics.raycount;
                    iter_fn = (((int)Math.Floor((fn_a - meas_a) / ray_interval) % characteristics.raycount) + characteristics.raycount) % characteristics.raycount;
                    if (characteristics.raycount - iter_st + iter_fn < characteristics.raycount / 2)
                    {
                        iter_st -= characteristics.raycount;
                    }
                }
                for (int i = iter_st; i <= iter_fn; i++)
                {
                    int rr = (i % characteristics.raycount + characteristics.raycount) % characteristics.raycount;
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
