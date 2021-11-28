using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GMap_Test
{
    public class Brid
    {
        public Vrh Od;
        public Vrh Do;
        public SpeedLinks Value;

        public Brid(Vrh vrhOd, Vrh VrhDo, SpeedLinks value)
        {
            Od = vrhOd;
            Do = VrhDo;
            Value = value;
        }
    }
}
