using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GMap_Test
{
    public class Dijkstra
    {
        private List<Vrh> Vrhovi;
        private List<Brid> Bridovi;
        public string FinalText = string.Empty;
        public List<Vrh> finalPath;
        public List<Vrh> Medium_Rare;
        bool _length, _energy, _speed;
        int _t = 0;
        string _dayType = "Workweek";
        double _crf, _ro, _A, _cw, _vMass;
        public double _minEnergy = 0;
        Dictionary<int, SpeedLinks> _Vmapa_Linkova = new Dictionary<int, SpeedLinks>();       

        public Dijkstra(List<Vrh> vrhovi, List<Brid> bridovi)
        {
            Vrhovi = vrhovi;
            Bridovi = bridovi;
        }

        public void Data(bool length, bool energy, bool speed, int t, string dayType, double crf, double ro, double A, double cw, double vehicleMass, Dictionary<int, SpeedLinks> Vmapa_Linkova, double minE)
        {
            _length = length;
            _energy = energy;
            _speed = speed;
            _t = t;
            _dayType = dayType;
            _crf = crf;
            _ro = ro;
            _A = A;
            _cw = cw;
            _vMass = vehicleMass;
            _Vmapa_Linkova = Vmapa_Linkova;
            _minEnergy = minE;
        }

        private Vrh IzvuciNajboljeg(List<Vrh> red)
        {
            int najbolji = 0;

            for (int i = 1; i < red.Count; i++)
            {
                if (_length)
                {
                    if (red[i].difficultyLength < red[najbolji].difficultyLength)//Length
                    {
                        najbolji = i;
                    }
                }
                else if (_energy)
                {
                    if(red[i].difficultyEnergy < red[najbolji].difficultyEnergy)
                    {
                        najbolji = i;
                    }
                }
                else//_speed
                {
                    if(red[i].difficultyTime < red[najbolji].difficultyTime)
                    {
                        najbolji = i;
                    }
                }
            }

            Vrh c = red[najbolji];
            red.RemoveAt(najbolji);
            return c;
        }

        public void Izracunaj(Vrh polaziste, Vrh odrediste)
        {
            // Inicijalizacija
            List<Vrh> obradjeni = new List<Vrh>();
            List<Vrh> redCekanja = new List<Vrh>();

            foreach (Vrh c in Vrhovi)
            {
                c.difficultyLength = double.MaxValue;
                c.difficultyTime = double.MaxValue;
                c.difficultyEnergy = double.MaxValue;
                
                c.Prethodni = null;
            }

            redCekanja.AddRange(Vrhovi);

            polaziste.difficultyLength = 0;
            polaziste.difficultyTime = 0;
            polaziste.difficultyEnergy = 0;
            
            if(_energy)//Pronadji najmanju energetski vrijednost koja se pojavljuje u zadanom vremenskom intervalu
            {
                double minEnergy = double.MaxValue;
                foreach (KeyValuePair<int, SpeedLinks> kvp in _Vmapa_Linkova)
                {
                    double nextSpeed = 0;
                    double a = 0;
                    double[] array = kvp.Value.getArray(_dayType);
                    double intervalSpeed = array[Misc.minuteToIndex(Convert.ToInt32(_t))];

                    if (Misc.minuteToIndex(Convert.ToInt32(_t)) <= array.Length - 2)
                    {
                        a = Misc.AcceleroMeter(intervalSpeed, nextSpeed);
                    }

                    double roadGrade = kvp.Value.deg;

                    double F = Misc.ForceCalc(Convert.ToDouble(_vMass), a, roadGrade, (intervalSpeed / 3.6), _crf, _ro, _A, _cw);
                    double P = Misc.Ecalc(F, (intervalSpeed / 3.6));
                    double E = P * (kvp.Value.length / (intervalSpeed / 3.6));

                    if (E < minEnergy )
                    {
                        minEnergy = E;
                        _minEnergy = E;
                    }
                }
            }

            // Postupak
            Vrh u, v;
            double valueV = 0, valueL = 0, valueE = 0;
            double avgV = 0;
            while (redCekanja.Count > 0)
            {
                u = IzvuciNajboljeg(redCekanja);
                obradjeni.Add(u);

                foreach (Brid brid in Bridovi)
                {
                    if (brid.Od == u)
                    {
                        v = brid.Do;
                        if (v == null)//Neki vrhovi ne postoje, nemam podatke o njima.
                        {
                            continue;
                        }

                        if (_length)
                        {
                            valueL = brid.Value.length;
                            double vT = brid.Value.getArray(_dayType)[Misc.minuteToIndex(Convert.ToInt32(_t))];
                            if (vT < 0)
                            {
                                vT = 1;
                            }
                            valueV = (valueL / 1000) / vT;//Time = length [km] / speed [km/h] --> T[h]

                            if (v.difficultyLength > u.difficultyLength + valueL)//Trazimo sto manji zbroj udaljenosti, manje == vise
                            {
                                v.difficultyLength = u.difficultyLength + valueL;
                                v.Prethodni = u;

                                if (v.difficultyTime > u.difficultyTime + valueV)
                                {
                                    v.difficultyTime = u.difficultyTime + valueV;
                                }
                            }
                        }
                        else if (_energy)
                        {
                            double nextSpeed = 0;
                            double a = 0;
                            double[] array = brid.Value.getArray(_dayType);
                            double intervalSpeed = array[Misc.minuteToIndex(Convert.ToInt32(_t))];
                            //if (intervalSpeed < 0)
                            //    continue;
                            double Vmax = -10, percent = 0;
                            int index = 0;
                            double vT = intervalSpeed;
                            if (vT < 0)//Prva provjera: Da li postoji barem jedna pozitivna (realna) vrijednost brzine u danu 
                            {                                                
                                for (int i = 0; i < array.Length; i++)
                                {
                                    if (array[i] > Vmax)
                                    {
                                        Vmax = array[i];
                                    }
                                }
                                if (Vmax > 0)//Postoji prihvatljiv zapis //Skaliranje nepoznatih vrijednosti na temelju poznatog zapisa
                                {
                                    index = Misc.minuteToIndex(_t);
                                    percent = index / 288; // 144/288 -> 50%
                                    vT = Vmax - Vmax * percent;//100 - 100 * 0.5 = 50 km/h
                                    intervalSpeed = vT;
                                }
                                else
                                {
                                    vT = 1;
                                }
                            }

                            if (Misc.minuteToIndex(Convert.ToInt32(_t)) <= array.Length - 2)
                            {
                                nextSpeed = array[Misc.minuteToIndex(Convert.ToInt32(_t)) + 1];
                                if (nextSpeed < 0)
                                {
                                    percent = percent + 0.05;//+ 5%
                                    nextSpeed = Vmax - Vmax * percent;
                                }
                                a = Misc.AcceleroMeter(intervalSpeed, nextSpeed);
                            }

                            double roadGrade = brid.Value.deg;

                            double F = Misc.ForceCalc(Convert.ToDouble(_vMass), a, roadGrade, (intervalSpeed / 3.6), _crf, _ro, _A, _cw);
                            double P = Misc.Ecalc(F, (intervalSpeed / 3.6));
                            double E = P * (brid.Value.length / (intervalSpeed /3.6));

                            E = (E + (_minEnergy * (-1) + 100));

                            valueE = E;

                            valueL = brid.Value.length;                            

                            valueV = (valueL / 1000) / vT;//Time = length [km] / speed [km/h] --> T[h]

                            if(v.difficultyEnergy > u.difficultyEnergy + valueE)//Energy
                            {
                                v.Prethodni = u;
                                v.difficultyEnergy = u.difficultyEnergy + valueE;

                                if (v.difficultyLength > u.difficultyLength + valueL)//Length
                                {
                                    v.difficultyLength = u.difficultyLength + valueL;
                                }
                                if (v.difficultyTime > u.difficultyTime + valueV)//Speed
                                {
                                    v.difficultyTime = u.difficultyTime + valueV;
                                }
                            }
                        }
                        else//SPEED
                        {
                            valueL = brid.Value.length;
                            double vT = brid.Value.getArray(_dayType)[Misc.minuteToIndex(Convert.ToInt32(_t))];
                            if(vT < 0)//Prva provjera: Da li postoji barem jedna pozitivna (realna) vrijednost brzine u danu 
                            {
                                double[] array = brid.Value.getArray(_dayType);
                                double Vmax = -10, percent = 0;
                                int index = 0;
                                for (int i = 0; i < array.Length; i++)
                                {
                                    if(array[i] > Vmax)
                                    {
                                        Vmax = array[i];
                                    }
                                }
                                if(Vmax > 0)//Postoji prihvatljiv zapis
                                {
                                    index = Misc.minuteToIndex(_t);
                                    percent = index / 288; // 144/288 -> 50%
                                    vT = Vmax - Vmax * percent;//100 - 100 * 0.5 = 50 km/h
                                }
                                else
                                {
                                    vT = 1;
                                }
                            }

                            valueV = (valueL / 1000) / vT;//Time = length [km] / speed [km/h] --> T[h]

                            if(v.difficultyTime > u.difficultyTime + valueV)//Speed
                            {
                                v.Prethodni = u;
                                v.difficultyTime = u.difficultyTime + valueV;

                                if(v.difficultyLength > u.difficultyLength + valueL)//Length
                                {
                                    v.difficultyLength = u.difficultyLength + valueL;
                                }
                            }
                        }                       
                    }
                }
            }

            // Rekonstruiranje puta
            List<Vrh> put = new List<Vrh>();
            Vrh trenutni = odrediste;

            do
            {
                put.Insert(0, trenutni);
                trenutni = trenutni.Prethodni;
            }
            while (trenutni != null);
            //Normalizacija energije (n - 1) jer se pocetni link ne broji
            odrediste.difficultyEnergy = odrediste.difficultyEnergy - ((put.Count - 1) * (_minEnergy * (-1) + 100));

            // Ispis
            avgV = (odrediste.difficultyLength / 1000) / odrediste.difficultyTime;
            string result = string.Format("Directions\n{0}\nStart: {1}\nDestination: {2}\n{0}\nStatistics:\n", "--------------------------------------------", polaziste.ID, odrediste.ID);

            if(_length ||_speed)
            {
                result += string.Format("Distance: {0} m\nTime: {1} h ~= {2} min\nAverage speed: {3} km/h\n--------------------------------------------\n", odrediste.difficultyLength, Math.Round(odrediste.difficultyTime, 2), Math.Round(odrediste.difficultyTime * 60, 2), Math.Round(avgV, 2));
            }
            else
            {
                result += string.Format("Distance: {0} m\nTime: {1} h ~= {2} min\nAverage speed: {3} km/h\nEnergy: {4} J\n--------------------------------------------\n", odrediste.difficultyLength, Math.Round(odrediste.difficultyTime, 2), Math.Round(odrediste.difficultyTime * 60, 2), Math.Round(avgV, 2), Math.Round(odrediste.difficultyEnergy, 2));
            }
            int counter = 1;
            result += "Path...\n--------------------------------------------\n";
            int rowCount = 0;
            foreach (Vrh c in put)
            {
                if (rowCount == 5)
                {
                    result += "\n";
                    rowCount = 0;
                }
                if (counter <= 9)
                {
                    if (c.ID.ToString().StartsWith("-"))
                        result += ("   " + counter + ". " + c.ID + " ||");
                    else
                        result += ("   " + counter + ".  " + c.ID + " ||");
                }
                else if (counter > 9 && counter < 100)
                {
                    if (c.ID.ToString().StartsWith("-"))
                        result += ("  " + counter + ". " + c.ID + " ||");
                    else
                        result += ("  " + counter + ".  " + c.ID + " ||");
                }
                else
                {
                    if (c.ID.ToString().StartsWith("-"))
                        result += (counter + ". " + c.ID + " ||");
                    else
                        result += (counter + ".  " + c.ID + " ||");
                }
                rowCount++;
                counter++;
            }
            FinalText = result;
            finalPath = put;
            Medium_Rare = obradjeni;
        }

        public void newDestination(Vrh start, Vrh destination)
        {
            double avgV = 0;
            List<Vrh> wellDone = Medium_Rare;

            List<Vrh> put = new List<Vrh>();
            Vrh trenutni = destination;

            do
            {
                put.Insert(0, trenutni);
                trenutni = trenutni.Prethodni;
            }
            while (trenutni != null);
            destination.difficultyEnergy = destination.difficultyEnergy - ((put.Count - 1) * (_minEnergy * (-1) + 100));

            // Ispis
            avgV = (destination.difficultyLength / 1000) / destination.difficultyTime;
            string result = string.Format("Directions\n{0}\nStart: {1}\nDestination: {2}\n{0}\nStatistics:\n", "--------------------------------------------", start.ID, destination.ID);

            if (_length || _speed)
            {
                result += string.Format("Distance: {0} m\nTime: {1} h ~= {2} min\nAverage speed: {3} km/h\n--------------------------------------------\n", destination.difficultyLength, Math.Round(destination.difficultyTime, 2), Math.Round(destination.difficultyTime * 60, 2), Math.Round(avgV, 2));
            }
            else
            {
                result += string.Format("Distance: {0} m\nTime: {1} h ~= {2} min\nAverage speed: {3} km/h\nEnergy: {4} J\n--------------------------------------------\n", destination.difficultyLength, Math.Round(destination.difficultyTime, 2), Math.Round(destination.difficultyTime * 60, 2), Math.Round(avgV, 2), Math.Round(destination.difficultyEnergy, 2));
            }
            int counter = 1;
            result += "Path...\n--------------------------------------------\n";
            int rowCount = 0;
            foreach (Vrh c in put)
            {
                if (rowCount == 5)
                {
                    result += "\n";
                    rowCount = 0;
                }
                if (counter <= 9)
                {
                    if (c.ID.ToString().StartsWith("-"))
                        result += ("   " + counter + ". " + c.ID + " ||");
                    else
                        result += ("   " + counter + ".  " + c.ID + " ||");
                }
                else if (counter > 9 && counter < 100)
                {
                    if (c.ID.ToString().StartsWith("-"))
                        result += ("  " + counter + ". " + c.ID + " ||");
                    else
                        result += ("  " + counter + ".  " + c.ID + " ||");
                }
                else
                {
                    if (c.ID.ToString().StartsWith("-"))
                        result += (counter + ". " + c.ID + " ||");
                    else
                        result += (counter + ".  " + c.ID + " ||");
                }
                rowCount++;
                counter++;
            }
            FinalText = result;
            finalPath = put;
        }
    }
}
