using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GMap_Test
{
    public class Vlonlat
    {
        public Vlonlat(double x, double y)
        {
            this.x = x;
            this.y = y;
        }
        public double x;
        public double y;
    }

    public class SpeedLinks
    {
        public int linkID;
        public int min;
        public Vlonlat b;
        public Vlonlat e;
        public double length;
        public double h1;
        public double h2;
        public double deg;
        public int LinkClass;
        public double[] speedsWeekend;
        public double[] speedsWorkWeek;
        public List<Vlonlat> interpolation;

        public SpeedLinks()
        {
            speedsWeekend = Enumerable.Repeat(-1.0, 288).ToArray();
            speedsWorkWeek = Enumerable.Repeat(-1.0, 288).ToArray();
        }

        public double[] getArray(string key)
        {
            if (key == "Weekend")
            {
                return speedsWeekend;
            }

            else if (key == "Workweek")
            {
                return speedsWorkWeek;
            }

            else
            {
                return null;
            }
        }
    }
}
