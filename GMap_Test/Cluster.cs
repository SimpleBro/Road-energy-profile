using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GMap_Test
{
    public class Cluster
    {
        public List<int> V_WW;//Speed_WorkWeek
        public List<int> V_WE;
        public List<int> E_WW;
        public List<int> E_WE;//Energy_WeekEnd

        public List<int> getArray(string DataKey,string Daykey)
        {
            if (DataKey == "Speed")
            {
                if (Daykey == "Weekend")
                {
                    return V_WE;
                }

                else if (Daykey == "Workweek")
                {
                    return V_WW;
                }

                else
                {
                    return null;
                }
            }
            else
            {
                if (Daykey == "Weekend")
                {
                    return E_WE;
                }

                else if (Daykey == "Workweek")
                {
                    return E_WW;
                }

                else
                {
                    return null;
                }
            }
        }
    }
}
