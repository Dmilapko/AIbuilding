using MonoHelper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIbuilding
{
    internal class RealDrone
    {
        List<BuildingRepresentation> buildings = new List<BuildingRepresentation>();
        List<Pair<PointD, List<int>>> route_building_indexes = new List<Pair<PointD, List<int>>>();
        BeizerCurve track;
        PointD position;
        double rotation;
        double curlength;


        public RealDrone(List<BuildingRepresentation> buildings, List<Pair<PointD, List<int>>> route_building_indexes, BeizerCurve track) 
        { 
            this.buildings = buildings;
            this.track = track;
            this.route_building_indexes = route_building_indexes;
            position = track.segments[0].GetPoint(0);
        }

        public List<double> GetRangeFinders(int chunk, PointD meas_pos, double meas_a, bool debug = false)
        {
            List<double> ranges = new List<double>();
            return ranges;
        }
    }
}
