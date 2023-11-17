using MonoHelper;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Text;
using System.Threading.Tasks;
using System.Dynamic;
using System.Xml.Linq;
using System.Drawing;
using System.Text.Json.Serialization;

namespace AIbuilding
{
    public class BeizerSegment
    {
        public PointD p0, p1, p2;
        public PointD conv_p0, conv_p1, conv_p2;
        public double length;

        public BeizerSegment(PointD p0, PointD p1, PointD p2)
        {
            this.p0 = p0;
            this.p1 = p1;
            this.p2 = p2;
            double a = MapMath.DistanceLongLat(p0, p2), b = MapMath.DistanceLongLat(p2, p1), c = MapMath.DistanceLongLat(p0, p1);
            double beta = Math.Acos(Math.Max(0, (Math.Pow(a, 2) + Math.Pow(c, 2) - Math.Pow(b, 2)) / (2 * a * c)));
            conv_p0 = new PointD(0, 0); conv_p1 = new PointD(c, 0); conv_p2 = new PointD(Math.Cos(beta) * a, Math.Sin(beta) * a);
            length = MHeleper.BezierLength(conv_p0, conv_p1, conv_p2);
        }

        /// <summary>
        /// Searches position (in meters) with precision (also in meters)
        /// </summary>
        public double BinarySearch(double position, double precision)
        {
            double left = 0, right = 1, res = -1;
            int depth = (int)Math.Ceiling(Math.Log2(length / precision));
            for (int i = 0; i < depth; i++)
            {
                res = (left + right) / 2;
                if (GetLength(res) > position) right = res;
                else left = res;
            }
            return res;
        }

        public double GetLength(double posd)
        {
            return MHeleper.BezierLength(conv_p0, conv_p1, conv_p2, posd);
        }
            
        public PointD GetPoint(double posd)
        {
            return ((1 - posd) * (1 - posd) * p0 + 2 * (1 - posd) * posd * p1 + posd * posd * p2);
        }
    }

    public class BuildingBound
    {
        PointD leftp, rightp;
        double lsub;
        
        public BuildingBound(PointD leftp, PointD rightp, PointD centroid)
        {
            this.leftp = leftp;
            this.rightp = rightp;
            double y = (leftp.Y + rightp.Y) / 2;
            lsub = MapMath.DistanceLongLat(new PointD(leftp.X, y), new PointD(rightp.X, y));
        }
    }


    public class BuildingRepresentation
    {
        private List<BuildingBound> bounds_list = new List<BuildingBound>(180);
        public PointD centroid;
        public List<PointD> points;

        public BuildingRepresentation(List<PointD> points)
        {
            centroid = MapMath.GetCentroid(points);
            List<Pair<double, double>> points_dl =  new List<Pair<double, double>>();
            foreach (var tp in points)
            {
                points_dl.Add(new Pair<double, double>(MapMath.DistanceLongLat(centroid, tp), MapMath.AngleLongLat(centroid, tp)));
            }
            double step = 1.ToRadians();
            for (int i = 0; i < 180; i++)
            {
                PointD mostleft = new PointD(+1000, 0), mostright = new PointD(-1000, 0); 
                int ind_ml = 0, ind_mr = 0;
                for (int j = 0; j < points.Count; j++)
                { 
                    PointD turn_tp = MapMath.RotateLongtLat(centroid, points_dl[j].First , points_dl[j].Second + i.ToRadians());
                    if (turn_tp.X < mostleft.X) { mostleft = turn_tp; ind_ml = j; }
                    if (turn_tp.X > mostright.X) { mostright = turn_tp; ind_mr = j; }
                }
                bounds_list.Add(new BuildingBound(mostleft, mostright, centroid));
            }
        }
    }

    public class BeizerCurve
    {
        public List<BeizerSegment> segments = new List<BeizerSegment>();
        public double length = 0;

        public BeizerCurve(List<PointD> trackpoints)
        {
            List<PointD> res = new List<PointD>();
            if (trackpoints.Count < 3) return;
            for (int i = 0; i <= trackpoints.Count - 3; i += 1)
            {
                PointD p0 = (trackpoints[i] + trackpoints[i + 1]) / 2;
                if (i == 0) p0 = trackpoints[0];
                PointD p1 = trackpoints[i + 1];
                PointD p2 = (trackpoints[i + 1] + trackpoints[i + 2]) / 2;
                if (i == trackpoints.Count - 3) p2 = trackpoints[i + 2];
                segments.Add(new BeizerSegment(p0, p1, p2));
                length += segments.Last().length;
            }
        }
    }

    internal class Drone
    {
        public static List<PointD> GetTrack(BeizerCurve curve, PointD biggerthan, PointD smallerthan)
        {
            List<PointD> res = new List<PointD>();
            foreach (BeizerSegment segment in curve.segments)
            {
                PointD bgth = new PointD(Math.Min(segment.p0.X, Math.Min(segment.p1.X, segment.p2.X)), Math.Min(segment.p0.Y, Math.Min(segment.p1.Y, segment.p2.Y)));
                PointD smth = new PointD(Math.Max(segment.p0.X, Math.Max(segment.p1.X, segment.p2.X)), Math.Max(segment.p0.Y, Math.Max(segment.p1.Y, segment.p2.Y)));
                var curverect = new System.Drawing.RectangleF();
                curverect.Location = new System.Drawing.PointF(MathF.Min((float)segment.p0.X, (float)Math.Min(segment.p1.X, segment.p2.X)), MathF.Min((float)segment.p0.Y, (float)Math.Min(segment.p1.Y, segment.p2.Y)));
                curverect.Width = MathF.Max((float)segment.p0.X, (float)Math.Max(segment.p1.X, segment.p2.X)) - curverect.Location.X;
                curverect.Height = MathF.Max((float)segment.p0.Y, (float)Math.Max(segment.p1.Y, segment.p2.Y)) - curverect.Location.Y;
                var locrect = new System.Drawing.RectangleF();
                locrect.Location = new System.Drawing.PointF(MathF.Min((float)biggerthan.X, (float)smallerthan.X), MathF.Min((float)biggerthan.Y, (float)smallerthan.Y));
                locrect.Width = MathF.Max((float)biggerthan.X, (float)smallerthan.X) - locrect.Location.X;
                locrect.Height = MathF.Max((float)biggerthan.Y, (float)smallerthan.Y) - locrect.Location.Y;
                if (curverect.IntersectsWith(locrect))
                {
                    for (double posd = 0; posd < 1; posd += 0.01)
                    {
                        res.Add(segment.GetPoint(posd));
                    }
                }
            }
            return res;
        }

        public static List<BuildingRepresentation> GetBuildingRepresentations(List<List<PointD>> buildings)
        {
            List<BuildingRepresentation> res = new List<BuildingRepresentation>();
            for (int i = 0; i < buildings.Count; i++)
            {
                res.Add(new BuildingRepresentation(buildings[i]));
            }
            return res;
        }

        /// <summary>
        /// Returns points on curve with certain diapason (in meters)
        /// </summary>
        public static List<PointD> GetTrack(BeizerCurve curve, double diap, double startpos = 0)
        {
            List<PointD> res = new List<PointD>();
            double cur_dist = 0;
            int now_track = 0;
            double cur_diap = startpos;
            while (true)
            {
                while (curve.segments[now_track].length - cur_dist < cur_diap) 
                {
                    cur_diap -= curve.segments[now_track].length - cur_dist;
                    now_track++;
                    cur_dist = 0;
                    if (now_track == curve.segments.Count)
                    {
                        res.Add(curve.segments.Last().GetPoint(1));
                        return res;
                    }
                }
                res.Add(curve.segments[now_track].GetPoint(curve.segments[now_track].BinarySearch(cur_dist, diap/256)));
                cur_dist += diap;
                cur_diap = diap;
            }
        }

        public class DeltaLinkComparer : IComparer<Pair<int,int>>//, IEqualityComparer<Pair<int, int>>
        {
            public int Compare(Pair<int, int> lnk1, Pair<int, int> lnk2)
            {
                if (lnk1.First < lnk2.First) return -1;
                if (lnk1.First > lnk2.First) return 1;
                if (lnk1.Second < lnk2.Second) return -1;
                if (lnk1.Second > lnk2.Second) return 1;
                return 0;
            }
        }


        /// <summary>
        /// very inefficient function, so must be used as one-time calculation
        /// </summary>
        public static List<List<int>> MakeBuilidng(BeizerCurve curve, double diap, double maxdist, List<List<PointD>> buildings)
        {
            var points_on_track = GetTrack(curve, diap);
            List<List<int>> res = new List<List<int>>(points_on_track.Count);
            List<Pair<int, int>> act_buildings = new List<Pair<int, int>>(buildings.Count);
            for (int i = 0; i < buildings.Count; i++) act_buildings.Add(new Pair<int, int>(0, i));
            SortedSet<Pair<int,int>> lengthset = new SortedSet<Pair<int, int>>(new DeltaLinkComparer());
            int now = 0;
            foreach (var cur_point in points_on_track)
            {
                res.Add(new List<int>());
                var elem = lengthset.Min;
                while (elem != null && elem.First == now)
                {
                    lengthset.Remove(elem);
                    act_buildings.Add(elem);
                    elem = lengthset.Min;
                }
                List<Pair<int,int>> next_act_buildings = new List<Pair<int, int>>(act_buildings.Count);
                for (int b = 0; b < act_buildings.Count; b++)
                {
                    if (act_buildings[b].First == now)
                    {
                        double mindist = Double.MaxValue;
                        foreach (PointD building_point in buildings[act_buildings[b].Second])
                        {
                            mindist = Math.Min(MapMath.DistanceLongLat(cur_point, building_point), mindist);
                        }
                        if (mindist < maxdist + diap / 2) res[now].Add(act_buildings[b].Second);
                        int approx_delta = (int)Math.Ceiling((mindist - maxdist) / diap);
                        if (approx_delta <= 0) next_act_buildings.Add(new Pair<int, int>(now + 1, act_buildings[b].Second));
                        else if (approx_delta <= 8) next_act_buildings.Add(new Pair<int, int>(now + approx_delta, act_buildings[b].Second));
                        else lengthset.Add(new Pair<int, int>(now + approx_delta, act_buildings[b].Second));
                    }
                    else
                    {
                        next_act_buildings.Add(act_buildings[b]);
                    }
                }
                act_buildings = next_act_buildings;
                now++;
            }
            return res;
        }
    }
}
