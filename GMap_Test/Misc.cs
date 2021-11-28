using GMap.NET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GMap_Test
{
    public static class Misc
    {
        public static int indexToMinute(int index)
        {
            return index * 5;
        }

        public static int minuteToIndex(int minute)
        {
            return Convert.ToInt32(Math.Floor(minute / 5.0));//npr. 20 / 5 = 4.. sto predstavlja 4.index u polju
        }

        public static Vlonlat CenterOfGravity(Vlonlat linkStartPoint, Vlonlat linkEndPoint) // Sredisnja tocka(koordinata) Linka
        {
            double CoGLat = (linkStartPoint.y + linkEndPoint.y) / 2;
            double CoGLng = (linkStartPoint.x + linkEndPoint.x) / 2;
            return new Vlonlat(CoGLng, CoGLat);
        }

        public static double GetDistance(Vlonlat mouseCord, Vlonlat linkCord) // Metoda za izracun udaljenosti tocaka //Euklidska udaljenost
        {
            double xDelta = mouseCord.x - linkCord.x;
            double yDelta = mouseCord.y - linkCord.y;
            return Math.Sqrt(Math.Pow(xDelta, 2) + Math.Pow(yDelta, 2)); // S(T1,T2) = Math.Sqrt( ( Math.Pow(x1,2) - Math.Pow(x2,2) ) + ( Math.Pow(y1,2) - Math.Pow(y2,2) ) )
        }

        public static PointLatLng convertToGmapPoint(Vlonlat point)
        {
            return new PointLatLng(point.y, point.x);
        }

        public static bool LinkInRectangle(SpeedLinks linkInfo,Vlonlat recBeg,Vlonlat recEnd)
        {
            if(linkInfo.b.x>=recBeg.x && linkInfo.b.x<=recEnd.x && linkInfo.e.x>=recBeg.x && linkInfo.e.x<=recEnd.x &&
                linkInfo.b.y >= recBeg.y && linkInfo.b.y <= recEnd.y && linkInfo.e.y >= recBeg.y && linkInfo.e.y <= recEnd.y)
            {
                return true;
            }
            return false;
        }

        public static double AcceleroMeter(double v1, double v2)
        {
            double V1 = v1 / 3.6; // Km/h -> m/s
            double V2 = v2 / 3.6;
            double a = (V2 - V1) / 300;// m/s^2
            return a;
        }

        public static double Ecalc(double Force, double vehicleSpeed)//ZAPRAVO P(SNAGA)
        {
            double energyEfficiency = 0.9; //eE represents the energy efficiency of transmission, motor and power conversion
            double F = Force;
            double v = vehicleSpeed;

            double E = (F * v) / energyEfficiency;

            return E;
        }

        public static double ForceCalc(double vehicleMass, double vehicleAcceleration, double roadGrade, double vehicleSpeed, double _crf, double _ro, double _A, double _cw)
        {
            double g = 9.81; // gravitation acceleration
            double crf = _crf;// rolling friction coefficient
            double ro = _ro; // air density
            double A = _A; // vehicle front surface area
            double cw = _cw; // air drag coefficient
            double m = vehicleMass;
            double a = vehicleAcceleration;
            double i = roadGrade / (180 / Math.PI);//Road grade in ° -> rad
            double v = vehicleSpeed;

            double Fr = (m * g * Math.Sin(i)) + (m * g * Math.Cos(i) * crf) + (0.5 * (ro * A * cw * Math.Pow(v, 2)));// Resistance force acting on the vehicle || Grade + Rolling + Air
            double F = m * a + Fr; // Force needed to overcome resistances to move at a certain speed

            return F;
        }
    }
}
