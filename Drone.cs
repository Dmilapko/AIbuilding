using MonoHelper;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Text;
using System.Threading.Tasks;

namespace AIbuilding
{
    internal class Drone
    {
        public static List<PointD> GetTrack(List<PointD> trackpoints, PointD biggerthan, PointD smallerthan)
        {
            List<PointD> res = new List<PointD>();
            if (trackpoints.Count < 3) return res;
            for (int i = 0; i <= trackpoints.Count - 3; i+=1)
            {
                PointD p0 = (trackpoints[i] + trackpoints[i + 1]) / 2;
                if (i == 0) p0 = trackpoints[0];
                PointD p1 = trackpoints[i + 1];
                PointD p2 = (trackpoints[i + 1] + trackpoints[i + 2]) / 2;
                if (i == trackpoints.Count - 3) p2 = trackpoints[i + 2];
                PointD bgth = new PointD(Math.Min(p0.X, Math.Min(p1.X, p2.X)), Math.Min(p0.Y, Math.Min(p1.Y, p2.Y)));
                PointD smth = new PointD(Math.Max(p0.X, Math.Max(p1.X, p2.X)), Math.Max(p0.Y, Math.Max(p1.Y, p2.Y)));
                var curverect = new System.Drawing.RectangleF();
                curverect.Location = new System.Drawing.PointF(MathF.Min((float)p0.X, (float)Math.Min(p1.X, p2.X)), MathF.Min((float)p0.Y, (float)Math.Min(p1.Y, p2.Y)));
                curverect.Width = MathF.Max((float)p0.X, (float)Math.Max(p1.X, p2.X)) - curverect.Location.X;
                curverect.Height = MathF.Max((float)p0.Y, (float)Math.Max(p1.Y, p2.Y)) - curverect.Location.Y;
                var locrect = new System.Drawing.RectangleF();
                locrect.Location = new System.Drawing.PointF(MathF.Min((float)biggerthan.X, (float)smallerthan.X), MathF.Min((float)biggerthan.Y, (float)smallerthan.Y));
                locrect.Width = MathF.Max((float)biggerthan.X, (float)smallerthan.X) - locrect.Location.X;
                locrect.Height = MathF.Max((float)biggerthan.Y, (float)smallerthan.Y) - locrect.Location.Y;
                if (curverect.IntersectsWith(locrect))
                {
                    for (double posd = 0; posd < 1; posd += 0.01)
                    {
                        res.Add((1 - posd) * (1 - posd) * p0 + 2 * (1 - posd) * posd * p1 + posd * posd * p2);
                    }
                }
            }
            return res;
        }
    }
}
