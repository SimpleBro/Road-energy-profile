using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GMap_Test
{
    public class Vrh
    {
        public int ID;
        public Vrh Prethodni;
        public double difficultyTime, difficultyLength, difficultyEnergy;

        public Vrh(int id)
        {
            ID = id;
            Prethodni = null;
            difficultyLength = 0;
            difficultyTime = 0;
        }
    }
}
