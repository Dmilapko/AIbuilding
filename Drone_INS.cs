using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using MathNet.Numerics;
using MathNet.Numerics.Distributions;
using MathNet.Numerics.Statistics;
using Microsoft.Xna.Framework;

namespace MonoHelper
{
    public class DroneINS
    {
        public PointD res_pos = new PointD(0, 0);
        public PointD sum_v = new PointD(0, 0);
        public int res_t = 0;
        public double res_rot = 0;
        public double sum_rot = 0;
        double prev_rot = 0;
        static public List<List<Pair<PointD, double>>> oper_lists = new List<List<Pair<PointD, double>>>();
        static public List<List<Pair<int, PointD>>> debug_oper_lists = new List<List<Pair<int, PointD>>>();
        int prec = 100;
        public Queue<Pair<PointD, double>> locked_data = new Queue<Pair<PointD, double>>();
        public double rot_deviation = 2 / 7505.74711621 / 60;
        public double pos_deviation = 2 * 9.8 / 60d / 8192d;
        static int spec_prec = 50;
        static List<List<double>> spec_list = new List<List<double>>();
        public double posdevn = 0;
        public double rotdevn = 0;

        public DroneINS(int prec, double _posdevn, double _rotdevn, int res_c = 6)
        {
            this.prec = prec;
            posdevn = _posdevn;
            rotdevn = _rotdevn;
        }


        static DroneINS()
        {
            for (int i = 1; i < 24; i++)
            {
                oper_lists.Add(ProbabilitySurface(i, 4, i * 500));
            }
        }

        public void ProcessData()
        {
            while (locked_data.Any())
            {
                DirectCalcSpeed(locked_data.Dequeue());
            }
        }

        public void AddData(PointD pos_dif, double rotation_dif)
        {
            locked_data.Enqueue(new Pair<PointD, double>(pos_dif, rotation_dif));
        }

        private static int Log2(int value)
        {
            int pv = value;
            int i;
            for (i = -1; value != 0; i++)
                value >>= 1;

            int a = i;
            if (pv > 1 << (a - 1)) a++;
            if (a == -1) return 0;
            else return a;
        }

        static List<Pair<PointD, double>> ProbabilitySurface(int lres_t, int res_c, int prec)
        {
            List<double> prob_pos = ProbabilityBell(lres_t, res_c, prec);
            DateTime thinking_start = DateTime.Now;
            Dictionary<Point, int> points = new Dictionary<Point, int>();
            List<Pair<PointD, double>> cur_oper_list = new List<Pair<PointD, double>>();
            List<Pair<int, PointD>> cur_debug_oper_list = new List<Pair<int, PointD>>();
            for (int c = 0; c < res_c; c++)
            {
                for (int i = 0; i < (1 << c); i++)
                {
                    for (int j = 0; j < (1 << c); j++)
                    {
                        Point pos = new Point(i, j);
                        int add_clc = Log2(Math.Max(i, j));
                        int stp = (1 << (c - add_clc - 1));
                        if (c == add_clc)
                        {
                            stp = 0;
                            cur_debug_oper_list.Add(new Pair<int, PointD>(cur_oper_list.Count, new PointD(prob_pos[pos.X], prob_pos[pos.Y])));
                        }
                        for (int a = stp; a < (1 << (c - add_clc)); a++)
                        {
                            cur_oper_list.Add(new Pair<PointD, double>(new PointD(prob_pos[pos.X], prob_pos[pos.Y]), prob_pos[a]));
                            if (prob_pos[pos.X] != 0) cur_oper_list.Add(new Pair<PointD, double>(new PointD(-prob_pos[pos.X], prob_pos[pos.Y]), prob_pos[a]));
                            if (prob_pos[pos.Y] != 0) cur_oper_list.Add(new Pair<PointD, double>(new PointD(prob_pos[pos.X], -prob_pos[pos.Y]), prob_pos[a]));
                            if (prob_pos[pos.Y] != 0 && prob_pos[pos.X] != 0) cur_oper_list.Add(new Pair<PointD, double>(new PointD(-prob_pos[pos.X], -prob_pos[pos.Y]), prob_pos[a]));
                            if (prob_pos[a] != 0)
                            {
                                cur_oper_list.Add(new Pair<PointD, double>(new PointD(prob_pos[pos.X], prob_pos[pos.Y]), -prob_pos[a]));
                                if (prob_pos[pos.X] != 0) cur_oper_list.Add(new Pair<PointD, double>(new PointD(-prob_pos[pos.X], prob_pos[pos.Y]), -prob_pos[a]));
                                if (prob_pos[pos.Y] != 0) cur_oper_list.Add(new Pair<PointD, double>(new PointD(prob_pos[pos.X], -prob_pos[pos.Y]), -prob_pos[a]));
                                if (prob_pos[pos.Y] != 0 && prob_pos[pos.X] != 0) cur_oper_list.Add(new Pair<PointD, double>(new PointD(-prob_pos[pos.X], -prob_pos[pos.Y]), -prob_pos[a]));
                            }
                        }
                    }
                }
            }
            debug_oper_lists.Add(cur_debug_oper_list);
            return cur_oper_list;
        }

        static double ProbabilitySum(int x, int lres_t)
        {
            double sum_n = 0;
            for (int n = 0; n <= spec_prec; n++)
            {
                int dfn = x - n;
                if (dfn >= 0 && dfn < spec_list[lres_t - 2].Count) sum_n += spec_list[lres_t - 2][dfn];
            }
            return sum_n;
        }

        static List<double> ProbabilityBell(int lres_t, int res_c, int prec)
        {
            if (lres_t == 1)
            {
                spec_list.Add(new List<double>());
                for (int i = 0; i < spec_prec; i++)
                {
                    spec_list[0].Add(1);
                }
            }
            else
            {
                spec_list.Add(new List<double>());
                double max_s = ProbabilitySum(spec_prec * lres_t / 2, lres_t);
                for (int x = 0; x < spec_prec * lres_t; x++)
                {
                    spec_list[lres_t - 1].Add(ProbabilitySum(x, lres_t) / max_s);
                }
            }
            List<double> pos_res = new List<double>(1<<res_c);
            List<double> sums = new List<double>(spec_list[lres_t - 1].Count);
            double i_coef = 2 / (double)spec_list[lres_t - 1].Count;
            double prev = 1;
            double sum = 0;
            pos_res.Add(0);
            /*for (int i = 0; i < prec; i++)
            {
                double x = i * i_coef;
                double now_g = (SpecialFunctions.Gamma(lres_t / 2d + 1) / SpecialFunctions.Gamma(0.5 * x + 0.5 * lres_t + 1)) * (SpecialFunctions.Gamma(lres_t / 2d + 1) / SpecialFunctions.Gamma(0.5 * -x + 0.5 * lres_t + 1));
                sums.Add(sum + (prev + now_g) / 2d);
                prev = now_g;
                sum = sums.Last();
            }*/
            for (int i = spec_prec * lres_t / 2; i < spec_list[lres_t - 1].Count; i++)
            {
                sums.Add(sum + (prev + spec_list[lres_t - 1][i]) / 2d);
                prev = spec_list[lres_t - 1][i];
                sum = sums.Last();
            }
            double diap = sum;
            for (int i = 0; i < res_c; i++)
            {
                double search_s = diap / 2;
                for (int j = 0; j < 1 << i; j++) 
                {
                    int now_track = 0;
                    int l = 0, r = sums.Count - 1;
                    while (l < r)
                    {
                        now_track = (l + r + 1) / 2;
                        if (sums[now_track] > search_s) r = now_track - 1;
                        else l = now_track;
                    }
                    pos_res.Add((now_track+1) * i_coef);
                    search_s += diap;
                }
                diap/= 2;
            }
            return pos_res;
        }

        public List<Pair<PointD, double>> GetMyProbability()
        {
            for (int i = oper_lists.Count; i < res_t; i++)
            {
                var l = ProbabilitySurface(i, 4, res_t * 100);
                oper_lists.Add(l);
            }
            return oper_lists[res_t-1];
        }


        private void DirectCalcSpeed(Pair<PointD, double> posrot)
        {
            posrot.Second = (int)Math.Round(posrot.Second * 7505.74711621 * 60 + rotdevn * MHeleper.RandomDouble()) / 7505.74711621 / 60;
            posrot.First = new PointD((int)Math.Round(posrot.First.X * 60 / 9.8 * 8192 + posdevn * MHeleper.RandomDouble()) * 9.8 / 60d / 8192d, (int)Math.Round(posrot.First.Y * 60 / 9.8 * 8192 + posdevn * MHeleper.RandomDouble()) * 9.8 / 60d / 8192d);

            sum_rot += posrot.Second;
            res_rot += sum_rot;
            sum_v += posrot.First.Turn(prev_rot + res_rot);
            res_pos += sum_v;
            res_t++;
        }

        public void Reset(double speed, double rot)
        {
            prev_rot = rot;
            res_t = 0;
            sum_v = new PointD(0, speed).Turn(rot);
            res_pos = new PointD(0, 0);
        }

        public void Reset(PointD real_res_pos, double real_res_rot)
        {
            PointD mgp = real_res_pos / res_pos - new PointD(1, 1);
            sum_v *= new PointD(1,1) + mgp;
            if (res_rot != 0)
            {
                double mgr = real_res_rot / res_rot - 1;
                if (mgr < 1.25) sum_rot *= 1 + mgr;
            }
            res_pos = new PointD(0, 0);
            res_rot = 0;
            res_t = 0;
            prev_rot += real_res_rot;
        }
    }
}
