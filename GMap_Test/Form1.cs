using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using GMap.NET.MapProviders;
using GMap.NET.WindowsForms;
using GMap.NET.WindowsPresentation;
using GMap.NET.CacheProviders;
using System.IO;
using System.Windows.Forms.DataVisualization.Charting;
using System.Diagnostics;
using GMap.NET;
using GMap.NET.WindowsForms.Markers;

namespace GMap_Test
{
    public partial class Form1 : Form
    {
        Dictionary<int, SpeedLinks> VmapaLinkova_2;
        Dictionary<int, Cluster> V_E_cluster;//cluster
        Vlonlat curMousePoint;
        SpeedLinks targetLink;
        List<SpeedLinks> DijkstraStartEnd;

        public Form1()
        {
            VmapaLinkova_2 = new Dictionary<int, SpeedLinks>();
            V_E_cluster = new Dictionary<int, Cluster>();
            DijkstraStartEnd = new List<SpeedLinks>();
            InitializeComponent();
        }

        private void map_Load(object sender, EventArgs e)
        {
            try//Geog. Sirina = +45 // Latitude; Geog. Duzina = +15 // Longitude
            {
                GMap.NET.GMaps.Instance.Mode = GMap.NET.AccessMode.ServerAndCache; // Postavanje uvjeta koristenja mape, moze se koristiti kad ima interneta, a kad nema onda vadi podatke iz prethodno ucitanih vrijednosti, znaci barem jednom se treba spojiti sa internetom da se ucita mapa
                                                                                   //GMap.NET.CacheProviders.MemoryCache   45.7666622807047, 15.9223544597626
                                                                                   //map.MapProvider = GMap.NET.MapProviders.GoogleMapProvider.Instance;
                map.MapProvider = GMap.NET.MapProviders.OpenStreetMapProvider.Instance;
                map.ShowCenter = false; // Uklanjanje kursora koji se nalazi na sredini mape, predstavlja fokus
                                        // Starting position 45.815299, 15.978568
                map.Position = new GMap.NET.PointLatLng(45.815299, 15.978568);//Pri pokretanju mape ovo postaje zarisna lokacija
                // Minimum zoom
                map.MinZoom = 11;
                // Maximum zoom
                map.MaxZoom = 17;
                // Starting zoom
                map.Zoom = 13.5;
                // lets the map use the mousewheel to zoom
                map.MouseWheelZoomType = GMap.NET.MouseWheelZoomType.MousePositionAndCenter;
                // lets the user drag the map
                map.CanDragMap = true;
                // lets the user drag the map with the left mouse button
                map.DragButton = MouseButtons.Left;
            }
            catch (Exception ex)
            {
                MessageBox.Show("map_Load \n" + ex);
            }
        }

        private void Btn_Open_Settings_Click(object sender, EventArgs e)
        {
            Settings_GB.Visible = true;
            Btn_Open_Settings.Visible = false;
            Stng_GTFO.Visible = true;
            Btn_Dijkstra_Close.PerformClick();//Remove the Dijkstra panel if it is open
            Btn_Close_Cluster.PerformClick();
        }

        private void Stng_GTFO_Click(object sender, EventArgs e)
        {
            Settings_GB.Visible = false;
            Stng_GTFO.Visible = false;
            Variable_Vehicle_GB.Visible = false;
            Btn_Close_Vehicle_Settings.Visible = false;
            Btn_Open_Settings.Visible = true;
            Btn_Open_Vehicle_Settings.Visible = true;
        }

        private void Btn_WF_Plt_Click(object sender, EventArgs e)//Generiranje charta
        {
            try
            {
                if (GB_Cluster.Visible == false)
                {
                    if (targetLink == null)
                    {
                        MessageBox.Show("Traget link is null!");

                        if (String.IsNullOrEmpty(LinkID_Txt.Text))
                        {
                            MessageBox.Show("LinkID_Txt is null or empty!");
                            return;
                        }
                        else
                        {
                            try
                            {
                                int linkID = Convert.ToInt32(LinkID_Txt.Text);
                                targetLink = VmapaLinkova_2[linkID];
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show("Not able to parse link id to int!" + "\n" + ex.Message);
                                return;
                            }
                        }
                    }
                }

                bool onlySpeed = Radio_Btn_Speed.Checked == true && Radio_Btn_Acceleration.Checked == false && Radio_Btn_Energy.Checked == false;
                bool onlyAcceleration = Radio_Btn_Acceleration.Checked == true && Radio_Btn_Speed.Checked == false && Radio_Btn_Energy.Checked == false;
                bool onlyEnergy = Radio_Btn_Energy.Checked == true && Radio_Btn_Acceleration.Checked == false && Radio_Btn_Speed.Checked == false;
                bool clusterSpeed = RB_Cluster_Speed.Checked; bool clusterEnergy = RB_Cluster_Energy.Checked;
                bool clusterWW = RB_Cluster_Workweek.Checked; bool clusterWE = RB_Cluster_Weekend.Checked;

                chart1.Titles.Clear();
                DataTable dt = new DataTable();
                double[] array = new double[288];
                int vehicleMass = Convert.ToInt32(Txt_Vehicle_Mass.Text);
                if (GB_Cluster.Visible == false)
                {
                    dt.Columns.Add("X_Value", typeof(double)); // stvaranje stupaca, X_Value ce sadrzavati minutne itervale
                    dt.Columns.Add("Y_Value", typeof(double)); // Brzinski uzorci, oba stupca su medjusobno povezana. x[0] je vezan za y[0]..

                    array = targetLink.getArray(DayType_Lab.Text);//Dohvacanje polja za (Tip Dana)                   
                }

                if (onlySpeed == true || (clusterSpeed && GB_Cluster.Visible))//Na temelju korisnikovog odabira samo prikaza grafa brzina
                {
                    double minSpd = 200;
                    double maxSpd = 0;

                    if (GB_Cluster.Visible == true && RB_Class_Off.Checked)
                    {
                        string dataKey = "Speed", dayKey = "Workweek";
                        if (!clusterSpeed) { dataKey = "Energy"; }
                        if (!clusterWW) { dayKey = "Weekend"; }

                        List<int> clArray = V_E_cluster[Convert.ToInt32(Cmb_Cluster.GetItemText(Cmb_Cluster.SelectedItem))].getArray(dataKey, dayKey);
                        chart1.Series.Clear();
                        dt = new DataTable();
                        double[,] clAVG = new double[2, 288];
                        int counter = 1;

                        chart1.Series.Add("ClusterCentorid");
                        chart1.Series["ClusterCentorid"].ChartType = SeriesChartType.Line;
                        chart1.Series["ClusterCentorid"].BorderWidth = 3;
                        chart1.Series["ClusterCentorid"].Color = Color.Red;

                        for (int i = 0; i < clArray.Count; i++)//Add series equal to the count of the cluster
                        {
                            chart1.Series.Add("SS" + i);
                            chart1.Series["SS" + i].ChartType = SeriesChartType.Line;
                            chart1.Series["SS" + i].BorderWidth = 2;
                            chart1.Series["SS" + i].Color = Color.Blue;

                            double[] dictData = VmapaLinkova_2[clArray[i]].getArray(dayKey);

                            for (int j = 0; j < dictData.Length; j++)
                            {
                                if (dictData[j] == -1)
                                    continue;

                                int intervalMin = Misc.indexToMinute(j);
                                double intervalSpeed = dictData[j];
                                chart1.Series["SS" + i].Points.AddXY(intervalSpeed, intervalMin);

                                clAVG[0, j] += intervalSpeed; clAVG[1, j] += counter;
                                if (intervalSpeed > maxSpd) { maxSpd = intervalSpeed; }
                                if (intervalSpeed < minSpd) { minSpd = intervalSpeed; }
                            }                            
                        }

                        for (int i = 0; i < clAVG.GetLength(1); i++)
                        {
                            if (clAVG[0, i] == 0)
                                continue;
                            double speed = clAVG[0, i] / clAVG[1, i];
                            int intervalMin = Misc.indexToMinute(i);
                            chart1.Series["ClusterCentorid"].Points.AddXY(speed, intervalMin);
                        }
                        chart1.Titles.Add("Cluster ID: " + Convert.ToInt32(Cmb_Cluster.GetItemText(Cmb_Cluster.SelectedItem)) + " Count: " + clArray.Count);
                    }

                    else
                    {
                        for (int i = 0; i < array.Length; i++) // pridodavanje vrijednosti redovima stupaca
                        {
                            if (array[i] != -1)//Provjera da li postoji zapis za odredjeni vremenski interval u polju
                            {
                                int intervalMinute = Misc.indexToMinute(i);
                                double intervalSpeed = array[i];
                                dt.Rows.Add(Convert.ToDouble(intervalMinute), Math.Round(intervalSpeed, 2));

                                if (intervalSpeed > maxSpd) { maxSpd = intervalSpeed; }
                                if (intervalSpeed < minSpd) { minSpd = intervalSpeed; } //Utvrdjivanje najmanje brzinske vrijednosti koja se pojavljuje
                            }
                        }

                        chart1.DataSource = dt; // Spajanje grafa sa tablicom u kojoj se nalaze vrijednosti
                        chart1.Titles.Add("Link id: " + Target_Link_Txt.Text + "\n DayType: " + DayType_Lab.Text + "  Minute: " + Txt_Min.Text + "  Speed: " + (targetLink.getArray(DayType_Lab.Text))[Misc.minuteToIndex(Convert.ToInt32(Txt_Min.Text))] + " Km/h"); // Naslov Grafa
                                                                                                                                                                                                                                                                      //SpeedLinks [ID.Linka] -> Dohvaćanje polja koje sadrzava zapise brzina za (Tip dana) -> [index u tom polju deriviran iz odabranog minutnog intervala]
                        chart1.Series["Series1"].XValueMember = "X_Value"; // Spajanje X-vrijednosti sa vrijednostima koje se nalaze u stupcu tablice u kojoj su pohranjeni vremenski intervali
                        chart1.Series["Series1"].YValueMembers = "Y_Value"; // Ista stvar samo se dohvacaju vrijednosti brzina

                        chart1.Series["Series1"].LabelToolTip = "#VALX [#VALY]";
                    }
                    //Odredjivanje najmanje tocke y-osi grafa brzina
                    if ((minSpd - 5) >= 10) //u slucaju da je raspon brzina od 80 do +100 bez ovog korigiranja y-os bi imala raspon od 0 do +100 te bi sami izgled grafa bio necitvljiv
                        chart1.ChartAreas[0].AxisY.Minimum = Convert.ToInt32(minSpd - 5); // Tako da se ovjde raspon y-osi postavlja u okvirima stvarnih vrijednosti brzina
                    else
                        chart1.ChartAreas[0].AxisY.Minimum = 0;

                    chart1.ChartAreas[0].AxisY.Maximum = Convert.ToInt32(maxSpd + 5); // povecanje zadnje tocke(vrijednosti na y-osi) y-osi radi boljeg pregleda grafa                    

                    chart1.ChartAreas[0].AxisX.Title = "Time t [min]";
                    chart1.ChartAreas[0].AxisY.Title = "Speed V [Km/h]";
                }

                else if (onlyAcceleration == true)//Ako je izabran samo prikaz grafa akceleracija
                {
                    double minAcc = 200;
                    double maxAcc = -200;

                    for (int i = 0; i < array.Length; i++)
                    {
                        if (array[i] != -1)
                        {
                            int intervalMinute = Misc.indexToMinute(i);
                            double intervalSpeed = array[i];

                            if (i == array.Length - 1)//Nakon zadnjeg vremenskog intervala ne postoji sljedeći, stoga zadnju akceleraciju racunavo uzimajuci u obzir samo njegovu brzinu
                            {
                                dt.Rows.Add(Convert.ToDouble(intervalMinute), 0);// 0 iz razloga sto vrijednost bude pre velika ako uzmemo u obzir samo jednu brzinu, te graf bude necitljiv zbog prevelike razlike u iznosu
                            }
                            else
                            {
                                double acceleration = Misc.AcceleroMeter(intervalSpeed, array[i+1]);
                                dt.Rows.Add(Convert.ToDouble(intervalMinute), Math.Round(acceleration, 5));
                                if (acceleration > maxAcc) { maxAcc = acceleration; }
                                if (acceleration < minAcc) { minAcc = acceleration; }
                            }
                        }                                           
                    }

                    double targetA = 0;
                    int indexMin = Misc.minuteToIndex(Convert.ToInt32(Txt_Min.Text));

                    if (indexMin >= 0 && indexMin <= array.Length - 2)
                    {
                        targetA = Misc.AcceleroMeter(array[indexMin], array[indexMin + 1]);
                    }

                    chart1.DataSource = dt;
                    chart1.Titles.Add("Link id: " + Target_Link_Txt.Text + "\n DayType: " + DayType_Lab.Text + "  Minute: " + Txt_Min.Text + "  Acceleration: " + Math.Round(targetA, 5) + " m/s^2");

                    chart1.Series["Series1"].XValueMember = "X_Value";
                    chart1.Series["Series1"].YValueMembers = "Y_Value";

                    chart1.ChartAreas[0].AxisY.Minimum = Math.Round(minAcc - (minAcc * 0.2), 5);
                    chart1.ChartAreas[0].AxisY.Maximum = Math.Round(maxAcc + (maxAcc * 0.2), 5);

                    chart1.Series["Series1"].LabelToolTip = "#VALX [#VALY]";

                    chart1.ChartAreas[0].AxisY.Title = "Acceleration \na [m/s^2]";
                }

                else if (onlyEnergy == true || (clusterEnergy && GB_Cluster.Visible))//Izabran samo graf energije
                {
                    double minE = double.MaxValue;
                    double maxE = double.MinValue;
                    double targetE = 0;
                    double crf = Convert.ToDouble(Txt_Rolling_friction_coeff.Text);// rolling friction coefficient
                    double ro = Convert.ToDouble(Txt_Air_density.Text); // air density
                    double A = Convert.ToDouble(Txt_Front_surface_area.Text); // vehicle front surface area
                    double cw = Convert.ToDouble(Txt_Air_drag_coeff.Text); // air drag coefficient

                    int indexMin = Misc.minuteToIndex(Convert.ToInt32(Txt_Min.Text));

                    //Ispunjavanje matrice za graf
                    for (int i = 0; i < array.Length; i++)
                    {
                        if (array[i] != -1)
                        {
                            int intervalMinute = Misc.indexToMinute(i);
                            double intervalSpeed = array[i];
                            double acceleration = 0;
                            double roadGrade = targetLink.deg;

                            if (i == array.Length - 1)
                            {
                                acceleration = 0; ;// 0 iz razloga sto vrijednost bude pre velika ako uzmemo u obzir samo jednu brzinu, te graf bude necitljiv zbog prevelike razlike u iznosu
                            }
                            else
                            {
                                acceleration = Misc.AcceleroMeter(intervalSpeed, array[i + 1]);//Valjana akceleracija
                            }

                            double F = Misc.ForceCalc(vehicleMass, acceleration, roadGrade, (intervalSpeed / 3.6), crf, ro, A, cw);
                            double P = Misc.Ecalc(F, (intervalSpeed / 3.6));
                            double E = P * (targetLink.length / (intervalSpeed / 3.6));

                            dt.Rows.Add(Convert.ToDouble(intervalMinute), Math.Round(E, 2));

                            if (E > maxE) { maxE = E; }
                            if (E < minE) { minE = E; }

                            if (i == Misc.minuteToIndex(Convert.ToInt32(Txt_Min.Text)))//Ako je i == odabranom vremenskom intervalu
                            {
                                targetE = Math.Round(E, 2);
                            }
                        }
                    }               

                    chart1.DataSource = dt;
                    chart1.Titles.Add("Link id: " + Target_Link_Txt.Text + "\n DayType: " + DayType_Lab.Text + "  Minute: " + Txt_Min.Text + "  Energy: " + targetE + " J");

                    chart1.Series[0].XValueMember = "X_Value";
                    chart1.Series[0].YValueMembers = "Y_Value";

                    chart1.ChartAreas[0].AxisY.Minimum = Math.Round(minE - 1000, 2);
                    chart1.ChartAreas[0].AxisY.Maximum = Math.Round(maxE + 1000, 2);

                    chart1.Series["Series1"].LabelToolTip = "#VALX [#VALY]";

                    chart1.ChartAreas[0].AxisY.Title = "Energy [J]";
                }

                //Određivanje konstantnih parametara grafa
                chart1.Titles[0].Alignment = ContentAlignment.TopLeft;
                chart1.Titles[0].TextStyle = TextStyle.Shadow;
                chart1.Titles[0].Font = new Font("Arial", 10, FontStyle.Bold);

                chart1.Series[0].ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line; //Postavljanje tipa grafa
                chart1.Legends[0].Enabled = false;
                chart1.Series[0].BorderWidth = 3; // Predstavlja debljinu iscrtane linije grafa
                chart1.ChartAreas[0].AxisY.LabelStyle.Format = "";
                chart1.ChartAreas[0].AxisX.Minimum = 0;
                chart1.ChartAreas[0].AxisX.Maximum = 1440;

                //Specifikacija karaktera vrijednosti x i y osi
                chart1.ChartAreas[0].AxisX.TitleAlignment = StringAlignment.Center;
                chart1.ChartAreas[0].AxisX.TitleFont = new Font("Arial", 10, FontStyle.Bold);
                chart1.ChartAreas[0].AxisX.Interval = 60; // Tick interval
                chart1.ChartAreas[0].AxisX.LabelStyle.Angle = -75;
                chart1.ChartAreas[0].AxisX.LabelStyle.Font = new Font("Arial", 9, FontStyle.Bold);
                chart1.ChartAreas[0].AxisY.TitleAlignment = StringAlignment.Center;
                chart1.ChartAreas[0].AxisY.TextOrientation = TextOrientation.Rotated270;
                chart1.ChartAreas[0].AxisY.TitleFont = new Font("Arial", 10, FontStyle.Bold);
                chart1.ChartAreas[0].AxisY.LabelStyle.Font = new Font("Arial", 9, FontStyle.Bold);
                chart1.ChartAreas[0].BackColor = Color.SeaShell;
                chart1.BackColor = Color.OldLace;

                //Dodatna x-os, nalazi se iznad grafa te predstavlja vrijeme u Satima
                chart1.ChartAreas[0].AxisX2.Enabled = AxisEnabled.True;
                chart1.ChartAreas[0].AxisX2.Minimum = 0;
                chart1.ChartAreas[0].AxisX2.Maximum = 24;
                chart1.ChartAreas[0].AxisX2.Interval = 1;
                chart1.ChartAreas[0].AxisX2.MajorGrid.Enabled = false;
                chart1.ChartAreas[0].AxisX2.MajorTickMark.Enabled = true;
                chart1.ChartAreas[0].AxisX2.LabelStyle.Angle = -75;
                chart1.ChartAreas[0].AxisX2.Title = "Time t [h]";
                chart1.ChartAreas[0].AxisX2.TitleFont = new Font("Arial", 10, FontStyle.Bold);
                chart1.ChartAreas[0].AxisX2.LabelStyle.Font = new Font("Arial", 9, FontStyle.Bold);
                chart1.ChartAreas[0].AxisX2.TitleAlignment = StringAlignment.Center;
                chart1.ChartAreas[0].AxisX2.CustomLabels.Add(1, DateTimeIntervalType.Number); // raspon vrijednosti od 0 do 24 uz pomak od 0.5

                chart1.Visible = true; //Po defaulta je graf nevidljiv
            }
            catch (Exception ex)
            {
                MessageBox.Show("Btn_WF_Plt_Click \n" + ex);
            }
        }

        private void chart1_MouseMove(object sender, MouseEventArgs e)//Pri kretanju misom po grafu
        {
            try
            {
                Point mousePoint = new Point(e.X, e.Y); //Inicijalizacija koordinata miša na grafu
                chart1.ChartAreas[0].CursorX.SetCursorPixelPosition(mousePoint, true); //Stvaranje curosra na grafu te linija koje dodatno pomazu pri iscitavanju tocne vrijednosti grafa
                chart1.ChartAreas[0].CursorY.SetCursorPixelPosition(mousePoint, true);

                if (Radio_Btn_Speed.Checked == true || Radio_Btn_Energy.Checked == true)//Iznos akceleracije se drasticno mijenja u svakom vremenskom zapisu stoga se stvara redundancija
                {
                    ToolTip tp = new ToolTip(); // Mali okvir koji sadrzi vrijednosti tocke koja je u fokusu misa
                    ToolTip tp2 = new ToolTip();
                    var pos = e.Location;
                    var results = chart1.HitTest(e.X, e.Y, false, ChartElementType.DataPoint);
                    foreach (var result in results)
                    {
                        if (result.ChartElementType == ChartElementType.DataPoint) //Samo ako su koordinate misa i linije grafa iste prikazi vrijednosti grafa
                        {
                            tp.RemoveAll();
                            tp2.RemoveAll();
                            var yVal = result.ChartArea.AxisY.PixelPositionToValue(e.Y);
                            var xVal = result.ChartArea.AxisX.PixelPositionToValue(e.X);
                            if (Radio_Btn_Speed.Checked == true && Radio_Btn_Energy.Checked == false)
                            {
                                tp.Show("Speed: " + Math.Round(((double)yVal), 2).ToString() + " [Km/h]", chart1, e.X, e.Y - 40, 1);
                                tp2.Show("Time: " + ((int)xVal).ToString() + " [min]", chart1, e.X, e.Y + 20, 1);
                            }
                            if (Radio_Btn_Speed.Checked == false && Radio_Btn_Energy.Checked == true)
                            {
                                tp.Show("Energy: " + Math.Round(((double)yVal), 2).ToString() + " [J]", chart1, e.X, e.Y - 40, 1);
                                tp2.Show("Time: " + ((int)xVal).ToString() + " [min]", chart1, e.X, e.Y + 20, 1);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("chart1_MouseMove \n" + ex);
            }
        }

        public void Btn_MapGen_Click(object sender, EventArgs e)// Metoda za stvaranje dodatnog sloja preko mape koji predstavlja sve linkove za koje postoje podaci za trazeni vrem. interval
        {
            try
            {
                progressBar1.Visible = true;
                progressBar1.Value = 0;
                progressBar1.Maximum = VmapaLinkova_2.Count;
                map.Overlays.Clear();// Pri generiranju mape za novi vremenski interval trebaju se izbrisati prethodni slojevi
                //Energy needed variables
                double crf = Convert.ToDouble(Txt_Rolling_friction_coeff.Text);// rolling friction coefficient
                double ro = Convert.ToDouble(Txt_Air_density.Text); // air density
                double A = Convert.ToDouble(Txt_Front_surface_area.Text); // vehicle front surface area
                double cw = Convert.ToDouble(Txt_Air_drag_coeff.Text); // air drag coefficient

                if (Txt_Min.Text == String.Empty || Convert.ToInt32(Txt_Min.Text) < 0 || Convert.ToInt32(Txt_Min.Text) > 1440)
                {
                    MessageBox.Show("Please enter a valid time frame interval.");
                    return;
                }

                int indexTimeFrame = Misc.minuteToIndex(Convert.ToInt32(Txt_Min.Text));

                //Area of the user defined rectangle //1.The rectangle parameters are defined as the maximum possible scale, in other words, the rectangle contains all links
                Vlonlat rectLeftDown = new Vlonlat(double.MinValue, double.MinValue);
                Vlonlat rectRightUp = new Vlonlat(double.MaxValue, double.MaxValue);

                if (Radio_Btn_Rect_Off.Checked == false)//Draw the whole map
                {
                    rectLeftDown = new Vlonlat(LocationX1Y1.Lng, LocationX1Y1.Lat);
                    rectRightUp = new Vlonlat(LocationX2Y2.Lng, LocationX2Y2.Lat);
                }

                bool longChange = rectLeftDown.x > rectRightUp.x;//Longitude value of starting point is bigger than long val. of ending point
                bool latChange = rectLeftDown.y > rectRightUp.y; //Latitude value of ending point is bigger that lat. val. of starting point

                Vlonlat pom = new Vlonlat(rectLeftDown.x, rectLeftDown.y);//Starting point of the rectangle

                if (longChange && latChange)
                {
                    rectLeftDown = rectRightUp;
                    rectRightUp = pom;
                }

                else if (longChange && latChange == false)
                {
                    rectLeftDown.x = rectRightUp.x;
                    rectRightUp.x = pom.x;
                }

                else if (longChange == false && latChange)
                {
                    rectLeftDown.y = rectRightUp.y;
                    rectRightUp.y = pom.y;
                }

                List<GMap.NET.PointLatLng> points = new List<GMap.NET.PointLatLng>();

                //Cluster K-Means initialisation
                string clusterDay = "Workweek";
                string clusterType = "Speed";
                if (RB_Cluster_Weekend.Checked == true)
                    clusterDay = "Weekend";
                if (RB_Cluster_Energy.Checked == true)
                    clusterType = "Energy";
                int clusterID = 1;
                if (GB_Cluster.Visible == true && !RB_Class_On.Checked)
                    clusterID = Convert.ToInt32(Cmb_Cluster.GetItemText(Cmb_Cluster.SelectedItem));//Convert.ToInt32(List_Box_ClusterID.);
                List<int> clusterArray = V_E_cluster[clusterID].getArray(clusterType, clusterDay);
                //Link Class
                int idClass = 1010;
                bool id1010 = RB_Class_1010.Checked; bool id1020 = RB_Class_1020.Checked; bool id1030 = RB_Class_1030.Checked; bool id1040 = RB_Class_1040.Checked;
                bool id1050 = RB_Class_1050.Checked; bool id1060 = RB_Class_1060.Checked; bool id1070 = RB_Class_1070.Checked; bool id1080 = RB_Class_1080.Checked;
                if (id1010) { idClass = 1010; } else if (id1020) { idClass = 1020; } else if (id1030) { idClass = 1030; } else if (id1040) { idClass = 1040; }
                else if (id1050) { idClass = 1060; } else if(id1070) { idClass = 1070; } else { idClass = 1080; }

                foreach (KeyValuePair<int, SpeedLinks> linkInfoKeyValue in VmapaLinkova_2)
                {
                    progressBar1.Increment(1);
                    SpeedLinks linkInfo = linkInfoKeyValue.Value;

                    double[] array = linkInfo.getArray(DayType_Lab.Text);
                    double intervalSpeed = -1;

                    //K-Means
                    if (GB_Cluster.Visible == true && !RB_Class_On.Checked)
                    {
                        if (!clusterArray.Contains(linkInfo.linkID))
                            continue;
                        //bool clusterContains = false;
                        //array = linkInfo.getArray(clusterDay);

                        //for (int i = 0; i < clusterArray.Count; i++)
                        //{
                        //    if (linkInfo.linkID == clusterArray.ElementAt(i))
                        //        clusterContains = true;
                        //}
                        //if (!clusterContains)
                        //    continue;
                    }

                    //Class
                    if(RB_Class_On.Checked && GB_Cluster.Visible == true)
                    {
                        if (linkInfo.LinkClass != idClass)
                            continue;
                    }

                    //Rectangle
                    if (!Misc.LinkInRectangle(linkInfo, rectLeftDown, rectRightUp))//Not within the rectangle
                    {
                        continue;
                    }

                    if (indexTimeFrame < 0 || indexTimeFrame >= array.Length)
                    {
                        MessageBox.Show("The time frame does not exists");
                        return;
                    }

                    else
                    {
                        intervalSpeed = array[indexTimeFrame];
                    }

                    if (intervalSpeed == -1)
                    {
                        Console.WriteLine("The value for the time frame does not exists");
                    }

                    points.Clear();

                    for (int i = 0; i < linkInfo.interpolation.Count; i++)//Popunjavanje od pocetne do zavrsne tocke (stvarne)
                    {
                        points.Add(Misc.convertToGmapPoint(linkInfo.interpolation[i]));
                    }
                    if (linkInfo.interpolation.Count > 2) //Barem 3 tocke postoje
                    {
                        for (int i = linkInfo.interpolation.Count - 2; i >= 1; i--)//Generiraj nove tocke, od zavrsne do pocetne, ne ukljucujuci pocetnu i zavrsnu
                        {
                            points.Add(Misc.convertToGmapPoint(linkInfo.interpolation[i]));
                        }
                    }

                    GMapOverlay polyOverlay = new GMapOverlay("polygons");
                    var j = new GMap.NET.WindowsForms.GMapPolygon(points, "mypolygon");
                    j.Fill = new SolidBrush(Color.Empty);

                    if (Radio_Btn_Speed.Checked == true || (GB_Cluster.Visible == true && RB_Cluster_Speed.Checked == true))
                    {
                        Pen pG = new Pen(Color.Green, 2); //Stvaranje boja za crtanje overlay-a
                        Pen pB = new Pen(Color.GreenYellow, 2);
                        Pen pO = new Pen(Color.Orange, 2);
                        Pen pR = new Pen(Color.Red, 2);

                        if (intervalSpeed > 50.0) //Ovisno o brzini kretanja na dionica opituraj overlay
                            j.Stroke = pG;

                        else if (intervalSpeed > 40 && intervalSpeed <= 50)
                            j.Stroke = pB;

                        else if (intervalSpeed > 20 && intervalSpeed <= 40)
                            j.Stroke = pO;

                        else
                            j.Stroke = pR;
                    }

                    else if (Radio_Btn_Acceleration.Checked == true && GB_Cluster.Visible == false)
                    {
                        Pen pG = new Pen(Color.Green, 2);
                        Pen pR = new Pen(Color.Red, 2);
                        double a = 0;
                        double nextSpeed = 0;

                        if (indexTimeFrame <= array.Length - 2)
                        {
                            nextSpeed = array[indexTimeFrame + 1];
                            a = Misc.AcceleroMeter(intervalSpeed, nextSpeed);
                        }

                        if (a > 0)
                            j.Stroke = pG;
                        else
                            j.Stroke = pR;
                    }

                    else if(Radio_Btn_Energy.Checked == true || (GB_Cluster.Visible == true && RB_Cluster_Energy.Checked == true))
                    {
                        Pen pG = new Pen(Color.Green, 2);
                        Pen pR = new Pen(Color.Red, 2);
                        double nextSpeed = 0;
                        double a = 0;

                        if (indexTimeFrame <= array.Length - 2)
                        {
                            nextSpeed = array[indexTimeFrame + 1];
                            a = Misc.AcceleroMeter(intervalSpeed, nextSpeed);
                        }

                        double roadGrade = linkInfo.deg;

                        double F = Misc.ForceCalc(Convert.ToDouble(Txt_Vehicle_Mass.Text), a, roadGrade, (intervalSpeed / 3.6), crf, ro, A, cw);
                        double P = Misc.Ecalc(F, (intervalSpeed / 3.6));
                        double E = P * (linkInfo.length / (intervalSpeed / 3.6));

                        if (E > 0)//Pretpostavka: Pri negativnom iznosu energije postoji mogucnost regeneriranja energije u elektricnom vozilu
                            j.Stroke = pR;
                        else
                            j.Stroke = pG;
                    }

                    var jRoutes = new GMapOverlay(linkInfo.linkID.ToString());
                    jRoutes.Polygons.Add(j);
                    map.Overlays.Add(jRoutes);
                    points.Clear();
                }

                progressBar1.Visible = false;
                map.Zoom += 1; //zoom + i - jer se inace mapa ne refresha, te se samim time ne prikazuju dodane rute
                map.Zoom -= 1;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Btn_MapGen_Click \n" + ex);
            }
        }

        private void Form1_Load(object sender, EventArgs e) //Pri ucitavanju Forme, pokrece se ucitavanje podataka koji ce se koristiti u svakoj daljnoj obradi podataka
        {
            try
            {
                progressBar1.Visible = false;
                StreamReader ww = new StreamReader("Kaizen_Remastered.txt");             
                VmapaLinkova_2.Clear();
                VmapaLinkova_2 = new Dictionary<int, SpeedLinks>(); // Dict za spremanje po Minutama
                Stopwatch sw = new Stopwatch();
                sw.Start();

                while (!ww.EndOfStream) //citanje datoteke u kojoj su upisane sve informacije linka
                {
                    string line = ww.ReadLine();
                    string[] splitLine = line.Split(';');
                    int finalLength;
                    int lineCount = splitLine.Count();//Utvrdjivanje velicine jednodimenzionalnog polja

                    SpeedLinks link = new SpeedLinks(); // klasa SpeedLinks u sebe posjeduje funkcije za procesiranje podataka, koordinate, minute, brzine...
                    link.linkID = Convert.ToInt32(splitLine[0]); //ID Linka
                    link.b = new Vlonlat(Convert.ToDouble(splitLine[1]), Convert.ToDouble(splitLine[2])); // Metoda za spremanje geografskih koordinata, 1.param.:pocetni Lat, 2.par:pocetni Lng
                    link.e = new Vlonlat(Convert.ToDouble(splitLine[3]), Convert.ToDouble(splitLine[4]));// zavrsni Lat, zavrsni Lng
                    link.length = Convert.ToDouble(splitLine[5]);
                    link.h1 = Convert.ToDouble(splitLine[6]);
                    link.h2 = Convert.ToDouble(splitLine[7]);
                    link.deg = Convert.ToDouble(splitLine[8]);
                    link.LinkClass = Convert.ToInt32(splitLine[lineCount - 1]);

                    int workweekIndexFinder = Array.IndexOf(splitLine, "Workweek");//Pronalazi index na kojem se nalazi "Weekend". 1.Parametar prima polje u kojem se obavlja potraga, 2.Par. je trazena vrijednost
                    int weekendIndexFinder = Array.IndexOf(splitLine, "Weekend");//Znam da se nalazi samo po jedan zapis za oba tipa dana
                    int interpolationFinder = Array.IndexOf(splitLine, "Interpolation");//Added points of the link
                    int LinkClassFinder = Array.IndexOf(splitLine, "LinkClass");

                    if (workweekIndexFinder > 8)//Provjera da li postoji zapis za radni tip dana
                    {
                        if (weekendIndexFinder > 8) { finalLength = weekendIndexFinder; }//Iz razloga sto neki linkovi nemaju zapis za vikend tip dana
                        else { finalLength = interpolationFinder; }
                        int indArrayWorkWeek = 0;

                        for (int i = workweekIndexFinder + 1; i < finalLength; i += 2)//Petlja za tip dana Workweek||i+=2 iz razloga sto trazim vrijednost minutnog intervala. Zapis u file-u je u sljedecem obliku: min_interval;avgspeed;min_interval;avgspeed;...
                        {
                            link.speedsWorkWeek[indArrayWorkWeek] = Convert.ToDouble(splitLine[i + 1]);
                            indArrayWorkWeek++;
                        }
                    }

                    //Nakon sto smo zapisali sve workweek vrijednosti, sad se zapisuju weekend vrijednosti na isti nacin
                    if (weekendIndexFinder > 8)//Provjera da li postoji vikend tip dana
                    {
                        int indArrayWeekend = 0;
                        for (int i = weekendIndexFinder + 1; i < interpolationFinder; i += 2)//Petlja za tip dana Weekend | Petlja krece od indexa na kojem pocinju zapisi za Weekend tip dana, preskace sve do tog indexa
                        {
                            link.speedsWeekend[indArrayWeekend] = Convert.ToDouble(splitLine[i + 1]);
                            indArrayWeekend++;
                        }
                    }

                    //Spremanje tocaka (interpolacija)
                    link.interpolation = new List<Vlonlat>();
                    if(interpolationFinder != LinkClassFinder - 1)//Postoje dodatne tocke
                    {
                        for (int i = interpolationFinder + 1; i < LinkClassFinder; i += 2)
                        {
                            link.interpolation.Add(new Vlonlat(Convert.ToDouble(splitLine[i]), Convert.ToDouble(splitLine[i + 1])));
                        }
                    }
                    else
                    {
                        link.interpolation.Add(link.b);
                        link.interpolation.Add(link.e);
                    }

                    VmapaLinkova_2[link.linkID] = link;//Nakon ispunjavanja (polja) spremamo cijeli link u Dictionary pod kljucem "linkID"
                }

                StreamReader srCluster = new StreamReader("Clusters_WW_WE_EN_SPD.txt");
                V_E_cluster.Clear();
                V_E_cluster = new Dictionary<int, Cluster>();

                while(!srCluster.EndOfStream)
                {
                    string line = srCluster.ReadLine();
                    string[] value = line.Split(';');

                    Cluster cl = new Cluster();
                    string dataType = value[0];
                    int lineCount = value.Length;
                    int clusterID = Convert.ToInt32(value[1]);
                    int V_WEindexFinder = Array.IndexOf(value, "Speed_WE");
                    int E_WWindexFinder = Array.IndexOf(value, "Energy_WW" + clusterID);
                    int E_WEindexFinder = Array.IndexOf(value, "Energy_WE" + clusterID);

                    List<int> l_v_ww = new List<int>();
                    List<int> l_v_we = new List<int>();
                    List<int> l_e_ww = new List<int>();
                    List<int> l_e_we = new List<int>();

                    //SPEED_WORKWEEK
                    for (int i = 2; i < V_WEindexFinder; i++)
                    {
                        l_v_ww.Add(Convert.ToInt32(value[i]));
                    }
                    cl.V_WW = l_v_ww;
                    //Speed_WeekEnd
                    for (int i = V_WEindexFinder + 2; i < E_WWindexFinder; i++)//indexFinder + 2 iz razloga sto nakon stringa "Speed_WE" ide ID clustera, npr. ;Speed_WE;27;linkID...
                    {
                        l_v_we.Add(Convert.ToInt32(value[i]));
                    }
                    cl.V_WE = l_v_we;
                    //Energy_WorkWeek
                    for (int i = E_WWindexFinder + 1; i < E_WEindexFinder; i++)//Dok kod Energy je druga stvar jer sam pogrijesio pri ispisu energija u datoteku te za en ide: ;Energy_WE23;linkID...
                    {
                        l_e_ww.Add(Convert.ToInt32(value[i]));
                    }
                    cl.E_WW = l_e_ww;
                    //Energy_WeekEnd
                    for (int i = E_WEindexFinder + 1; i < lineCount; i++)
                    {
                        l_e_we.Add(Convert.ToInt32(value[i]));
                    }
                    cl.E_WE = l_e_we;
                    //Final add to dict
                    V_E_cluster[clusterID] = cl;
                }

                sw.Stop();
                MessageBox.Show(sw.Elapsed.ToString());

                Btn_MapGen.Click += (sender2, e2) => Btn_MapGen_Click(sender2, e2);
                Link_Find_Btn.Click += (sender3, e3) => Link_Find_Btn_Click(sender3, e3); //when mousedoubleclick event is triggered, this is triggered too
                Btn_WF_Plt.Click += (sender4, e4) => Btn_WF_Plt_Click(sender4, e4);//Crtanje grafa

                Lng_Txt.Text = "";
                Lat_Txt.Text = "";
                Txt_Min.Text = "0";
                DayType_Lab.Text = "Weekend";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Form1_Load \n" + ex);
            }
        }

        public void map_MouseDoubleClick(object sender, MouseEventArgs e)//Pri duplom pritisku na mapu, ovdje se generiraju koordinate clicka te se te iste koordinate procesiraju u metodi Link_Find_Btn.Click
        {
            curMousePoint = null;
            double lat = map.FromLocalToLatLng(e.X, e.Y).Lat;
            double lng = map.FromLocalToLatLng(e.X, e.Y).Lng;
            curMousePoint = new Vlonlat(lng, lat);
            Lat_Txt.Text = lat.ToString();//Spremanje koordinata unutar txt-a da bi ih se moglo iscitati u drugim radnjama
            Lng_Txt.Text = lng.ToString();

            if (e.Button == MouseButtons.Left)
            {
                try
                {
                    Link_Find_Btn.Select();
                    Link_Find_Btn.PerformClick();//Pokretanje funkcije odredjivanja najblizeg linka 
                }
                catch (Exception ex)
                {
                    MessageBox.Show("map_MouseDoubleClick \n" + ex.Message);
                }
            }

            if(e.Button == MouseButtons.Right)
            {
                try
                {
                    if ((DijkstraStartEnd.Count % 2 == 0) && DijkstraStartEnd.Count > 1)//Start + End
                    {
                        Btn_Remove_Destination_Dijkstra.Visible = true;
                        Btn_Set_Destination_Dijkstra.Visible = false;
                    }
                    else if (!(DijkstraStartEnd.Count % 2 == 0) && DijkstraStartEnd.Count > 0)//Start
                    {
                        Btn_Remove_Destination_Dijkstra.Visible = false;
                        Btn_Set_Destination_Dijkstra.Visible = true;
                    }
                    Grp_Bx_Quick_Settings.Location = e.Location;
                    Grp_Bx_Quick_Settings.Visible = true;
                }
                catch (Exception ee)
                {
                    MessageBox.Show("Right mouse double click \n" + ee.Message);
                }
            }
        }
        
        private void MouseClickLinkDraw()//Funkcija odredjivanja najblizeg linka
        {
            try
            {
                if (curMousePoint != null && VmapaLinkova_2 != null)
                {
                    double minCoG = double.MaxValue;//Pocetna minimalna udaljenost od sredista linka (minimum center of gravity)

                    targetLink = null;

                    foreach (KeyValuePair<int, SpeedLinks> linkInfKeyValue in VmapaLinkova_2)
                    {
                        Vlonlat center = Misc.CenterOfGravity(linkInfKeyValue.Value.b, linkInfKeyValue.Value.e);
                        double cogMin = Misc.GetDistance(curMousePoint, center);

                        if (cogMin < minCoG)
                        {
                            minCoG = cogMin;
                            targetLink = linkInfKeyValue.Value;//Ako ispunjava uvjet onda pamtimo sve informacije o linku unutar "targetLink"
                        }
                    }

                    Pen pC = new Pen(Color.Black, 7);
                    List<GMap.NET.PointLatLng> links = new List<GMap.NET.PointLatLng>();

                    for (int i = 0; i < targetLink.interpolation.Count; i++)//Popunjavanje od pocetne do zavrsne tocke (stvarne)
                    {
                        links.Add(Misc.convertToGmapPoint(targetLink.interpolation[i]));
                    }
                    if (targetLink.interpolation.Count > 2) //Barem 3 tocke postoje
                    {
                        for (int i = targetLink.interpolation.Count - 2; i >= 1; i--)//Generiraj nove tocke, od zavrsne do pocetne, ne ukljucujuci pocetnu i zavrsnu
                        {
                            links.Add(Misc.convertToGmapPoint(targetLink.interpolation[i]));
                        }
                    }
                    
                    GMapOverlay polyOverlay = new GMapOverlay("polygons");
                    var j = new GMap.NET.WindowsForms.GMapPolygon(links, "mypolygon");

                    if (targetLink.speedsWorkWeek[0] >= 0)
                        j.Stroke = pC;
                    j.Fill = new SolidBrush(Color.Empty);//Ne ispunjavaj prostor izmedju tocaka..                   

                    var jRoutes = new GMapOverlay(targetLink.linkID.ToString());
                    jRoutes.Polygons.Add(j);
                    map.Overlays.Add(jRoutes);
                    links.Clear();
                    map.Zoom += 0.01;
                    map.Zoom -= 0.01;
                    Target_Link_Txt.Text = targetLink.linkID.ToString();
                    LinkID_Txt.Text = Target_Link_Txt.Text;

                    map.Overlays.RemoveAt(map.Overlays.Count - 1); // Nakon odabira linka te iscrtavanja, izbrisi zadnji dodani overlay

                    if (Radio_Btn_Marker_On.Checked == false)//Show data type Graph
                    {
                        Btn_WF_Plt.PerformClick();//Crtanje grafa
                    }
                    
                    else//Pinpoint the start/end of the link
                    {
                        GMapOverlay marker = new GMapOverlay("marker");
                        Vlonlat CoG = Misc.CenterOfGravity(targetLink.b, targetLink.e);
                        PointLatLng p = Misc.convertToGmapPoint(CoG);//Iscrtavamo pinpoint na sredini linka, ali pamtimo cijeli link(Class object)
                        SpeedLinks point = targetLink;                        
                        GMap.NET.WindowsForms.GMapMarker markerType = new GMarkerGoogle(p, GMarkerGoogleType.green_pushpin);
                        
                        if(!(DijkstraStartEnd.Count % 2 == 0) )//End location in red
                        {
                            markerType = new GMarkerGoogle(p, GMarkerGoogleType.red_pushpin);
                        }

                        else// (DijkstraStartEnd.Count % 2 == 0)//Even number,,, Start + destination
                        {
                            map.Overlays.Clear();//Clears the whole map... not so good
                            map.Zoom += 1;
                            map.Zoom -= 1;
                        }
                        DijkstraStartEnd.Add(point);

                        for (int i = 0; i < 3; i++)//Tek nakon treceg klika se iscrta na tocnoj poziciji... 
                        {
                            marker.Markers.Add(markerType);
                            map.Overlays.Add(marker);
                        }
                        map.Zoom += 1;//Brisu "lazne" markere
                        map.Zoom -= 1;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("MouseClickLinkDraw \n" + ex);
            }
        }

        private void Link_Find_Btn_Click(object sender, EventArgs e)//Pri duplom kliku na mapu aktivira se ovaj botun
        { 
            MouseClickLinkDraw();//Pokretanje metode za pronalazak odabranog linka
        }

        private void Radio_Btn_Weekday_CheckedChanged(object sender, EventArgs e) // Vikend select
        {
            if (Radio_Btn_Weekday.Checked == true)
                DayType_Lab.Text = "Weekend";
        }

        private void Radio_Btn_Workday_CheckedChanged(object sender, EventArgs e) // Radni dan select
        {
            if (Radio_Btn_Workday.Checked == true)
                DayType_Lab.Text = "Workweek";
        }

        private void map_MouseClick(object sender, MouseEventArgs e)//Uklanjanje grafa pri uporabi desnog botuna na misu
        {
            if (e.Button == MouseButtons.Right)
            {
                chart1.Visible = false;
            }
            if(e.Button == MouseButtons.Left)
            {
                Grp_Bx_Quick_Settings.Visible = false;
            }
        }

        private void Btn_Default_Vehicle_Settings_Click(object sender, EventArgs e)//Vracanje vrijednosti u izvorni oblik
        {
            Txt_Vehicle_Mass.Text = "1521";
            Txt_Rolling_friction_coeff.Text = "0.008";
            Txt_Air_density.Text = "1.25";
            Txt_Front_surface_area.Text = "2.19";
            Txt_Air_drag_coeff.Text = "0.29";
        }

        private void Btn_Open_Vehicle_Settings_Click(object sender, EventArgs e)
        {
            Variable_Vehicle_GB.Visible = true;
            Btn_Close_Vehicle_Settings.Visible = true;
            Btn_Open_Vehicle_Settings.Visible = false;
        }

        private void Btn_Close_Vehicle_Settings_Click(object sender, EventArgs e)
        {
            Variable_Vehicle_GB.Visible = false;
            Btn_Open_Vehicle_Settings.Visible = true;
            Btn_Close_Vehicle_Settings.Visible = false;
        }

        private void Btn_Clear_Map_Click(object sender, EventArgs e)//Uklanjanje generiranih slojeva na mapi te grafa
        {
            chart1.Visible = false;
            map.Overlays.Clear();
            DijkstraStartEnd.Clear();
            map.Zoom += 1;
            map.Zoom -= 1;
        }

        private void Btn_MapZoomOut_Click(object sender, EventArgs e)
        {
            map.Zoom -= 0.33;
        }

        private void Btn_MapZoomIn_Click(object sender, EventArgs e)
        {
            map.Zoom += 0.33;
        }

        bool IsMouseDown = false;//Mouse down for rectangle drawing
        Rectangle mapRect;
        PointLatLng LocationX1Y1;//Starting location for drawing links
        PointLatLng LocationX2Y2;//Ending location
        Point LocationT1;//Starting location for drawing the rectangle
        Point LocationT2;

        private void map_MouseDown(object sender, MouseEventArgs e)
        {
            if (Radio_Btn_Rect_On.Checked == true)
            {
                if (e.Button == MouseButtons.Left)//Draw only if the left mouse button is pressed
                {
                    map.CanDragMap = false;//Disable the option for draging the map
                    IsMouseDown = true;
                    double lat = map.FromLocalToLatLng(e.X, e.Y).Lat;//Transfering the coordinates from the map
                    double lng = map.FromLocalToLatLng(e.X, e.Y).Lng;
                    LocationX1Y1.Lat = lat;
                    LocationX1Y1.Lng = lng;//Location for linkDrawing    

                    LocationT1 = e.Location;//Entering the starting location of the rectangle
                }

                else if (e.Button == MouseButtons.Right)//Reset the rectangle if the right mouse button is pressed
                {
                    LocationX1Y1 = LocationX2Y2;
                    LocationT1 = LocationT2;
                }
            }
            else
                map.CanDragMap = true;
        }

        private void map_MouseMove(object sender, MouseEventArgs e)
        {
            if (IsMouseDown == true)//The block is not executed until mouse down event is not a fire
            {
                double lat = map.FromLocalToLatLng(e.X, e.Y).Lat;//Transfering the coordinates from the map
                double lng = map.FromLocalToLatLng(e.X, e.Y).Lng;
                LocationX2Y2.Lat = lat; //Entering the current location of Point X and Y
                LocationX2Y2.Lng = lng;

                LocationT2 = e.Location;
                Refresh();//Refreshing the map
            }
        }

        private void map_MouseUp(object sender, MouseEventArgs e)//Mouse has left the building!
        {
            if (IsMouseDown == true)//The block is not executed until mouse down event is not a fire
            {
                double lat = map.FromLocalToLatLng(e.X, e.Y).Lat;//Transfering the coordinates from the map
                double lng = map.FromLocalToLatLng(e.X, e.Y).Lng;
                LocationX2Y2.Lat = lat;
                LocationX2Y2.Lng = lng;//Entering the ending point of x and y                

                LocationT2 = e.Location;
                IsMouseDown = false;//Mouse has left the building
            }
        }

        private void map_Paint(object sender, PaintEventArgs e)
        {
            if (Radio_Btn_Rect_On.Checked == true)
            {
                Brush b = new SolidBrush(Color.FromArgb(100, Color.Navy));

                if (mapRect != null)//Chek if the rectangle is not a null
                {
                    e.Graphics.FillRectangle(b, GetRect());
                }
            }
        }

        private Rectangle GetRect()//Function for the determination of the size and location of the rectangle
        {
            mapRect = new Rectangle();

            mapRect.X = Math.Min(LocationT1.X, LocationT2.X);//Value x of rectangle should be the minimum between the start x and current x
            mapRect.Y = Math.Min(LocationT1.Y, LocationT2.Y);
            mapRect.Width = Math.Abs(LocationT1.X - LocationT2.X);
            mapRect.Height = Math.Abs(LocationT1.Y - LocationT2.Y);

            return mapRect;
        }

        private void Btn_Help_Rect_Click(object sender, EventArgs e)
        {
            MessageBox.Show("If enabled, the user is able to draw a rectangle on the map.\nOnly the links within this rectangle shall be drawn.\nUse the left mouse button to draw, the right to clear.");
        }

        private void Btn_Dijkstra_Open_Click(object sender, EventArgs e)
        {
            Btn_Dijkstra_Close.Visible = true;
            Btn_Dijkstra_Open.Visible = false;
            GB_Dijkstra.Visible = true;
            Stng_GTFO.PerformClick();//Remove the setting panel if it is open
            Btn_Close_Cluster.PerformClick();
        }

        private void Btn_Dijkstra_Close_Click(object sender, EventArgs e)
        {
            Btn_Dijkstra_Open.Visible = true;
            Btn_Dijkstra_Close.Visible = false;
            GB_Dijkstra.Visible = false;
        }

        private void Btn_Dijkstra_Clear_Map_Click(object sender, EventArgs e)
        {
            map.Overlays.Clear();
            DijkstraStartEnd.Clear();
            Rich_Txt_Bx_Dijkstra_Results.Text = string.Empty;
            map.Zoom += 1;
            map.Zoom -= 1;
            finalPath.Clear();
        }

        private void Btn_Enlarge_Dijkstra_Results_Click(object sender, EventArgs e)
        {
            MessageBox.Show(Rich_Txt_Bx_Dijkstra_Results.Text);
        }

        private void Btn_Markers_Help_Click(object sender, EventArgs e)
        {
            MessageBox.Show("If the 'On' button is checked the user can pinpoint a link by double clicking the map.\nThe green pinpoint represents the path's starting location, and the red indicates the destination.");
        }

        //--------------------------------------------------------------------------------------------------------------------------------------------------------------------
        //Dijkstra
        List<Vrh> Vrhovi;
        List<Brid> Bridovi;
        bool dataLoaded = false;
        List<Vrh> finalPath = new List<Vrh>();
        bool startChanged = true;
        double _minE = 0;

        public Vrh VrhSaID(int id)
        {
            foreach (Vrh c in Vrhovi)
                if (c.ID == id)
                    return c;
            return null;
        }

        private void Btn_Dijkstra_Launcer_Click(object sender, EventArgs e)
        {
            try
            {
                Stopwatch sw = new Stopwatch();
                sw.Start();

                if (dataLoaded)
                {
                    Vrh polaziste = VrhSaID(DijkstraStartEnd[DijkstraStartEnd.Count - 2].linkID);
                    Vrh odrediste = VrhSaID(DijkstraStartEnd[DijkstraStartEnd.Count - 1].linkID);

                    bool length = Radio_Btn_Dijkstra_Length.Checked; bool energy = Radio_Btn_Dijkstra_Energy.Checked; bool speed = Radio_Btn_Dijkstra_Speed.Checked;

                    Dijkstra dijkstra = new Dijkstra(Vrhovi, Bridovi);
                    double crf = Convert.ToDouble(Txt_Rolling_friction_coeff.Text);// rolling friction coefficient
                    double ro = Convert.ToDouble(Txt_Air_density.Text); // air density
                    double A = Convert.ToDouble(Txt_Front_surface_area.Text); // vehicle front surface area
                    double cw = Convert.ToDouble(Txt_Air_drag_coeff.Text); // air drag coefficient
                    double vMass = Convert.ToDouble(Txt_Vehicle_Mass.Text);
                    dijkstra.Data(length, energy, speed, Convert.ToInt32(Txt_Min.Text), DayType_Lab.Text, crf, ro, A, cw, vMass, VmapaLinkova_2, _minE);

                    if (startChanged || finalPath.Count == 0)
                    {
                        dijkstra.Izracunaj(polaziste, odrediste);
                        _minE = dijkstra._minEnergy;
                    }
                    else
                        dijkstra.newDestination(polaziste, odrediste);

                    finalPath = dijkstra.finalPath;
                    Rich_Txt_Bx_Dijkstra_Results.Text = string.Empty;
                    Rich_Txt_Bx_Dijkstra_Results.Text += dijkstra.FinalText;

                    drawPath(finalPath);
                }

                sw.Stop();
                MessageBox.Show("Time elapsed: " + sw.Elapsed);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Dijkstra Launcher." + "\n" + ex.Message);
            }
        }

        private void Btn_Load_Dijkstra_Vertex_Click(object sender, EventArgs e)
        {
            if (!dataLoaded)
            {
                Vrhovi = new List<Vrh>();
                Bridovi = new List<Brid>();
                string datotekaBridovi = "Dijkstra_Edge_Reformed.txt";
                StreamReader srBrid = new StreamReader(datotekaBridovi);
                string[] podaci;
                string redak;
                SpeedLinks value;

                try
                {
                    foreach (KeyValuePair<int, SpeedLinks> kvp in VmapaLinkova_2)
                    {
                        int id = kvp.Key;
                        Vrhovi.Add(new Vrh(id));
                    }

                    while (!srBrid.EndOfStream)
                    {
                        redak = srBrid.ReadLine();
                        podaci = redak.Split(';');

                        int linkID = Convert.ToInt32(podaci[0]);
                        if (VmapaLinkova_2.ContainsKey(linkID))//Some of the links do not exist
                        {
                            Vrh VrhOd = VrhSaID(linkID);
                            value = VmapaLinkova_2[linkID];

                            if (podaci.Length > 1)//Link ima barem jednu konekciju
                            {
                                for (int i = 1; i < podaci.Length; i++)
                                {
                                    Vrh VrhDo = VrhSaID(Convert.ToInt32(podaci[i]));
                                    Bridovi.Add(new Brid(VrhOd, VrhDo, value));
                                }
                            }
                        }
                    }
                    srBrid.Close();

                    dataLoaded = true;
                    Lab_Vortex_State.Text = "True";

                    MessageBox.Show("Data loaded successfully!");
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Data_Load_Dijkstra.\n" + ex.Message);
                }
            }
        }

        public void drawPath(List<Vrh> path)
        {
            Pen kroma = new Pen(Color.FromArgb(100, Color.Navy), 7);
            SpeedLinks targetLink;
            List<GMap.NET.PointLatLng> link = new List<GMap.NET.PointLatLng>();

            for (int i = 0; i < path.Count; i++)
            {
                targetLink = VmapaLinkova_2[path[i].ID];

                link.Clear();
                for (int l = 0; l < targetLink.interpolation.Count; l++)//Popunjavanje od pocetne do zavrsne tocke (stvarne)
                {
                    link.Add(Misc.convertToGmapPoint(targetLink.interpolation[l]));
                }
                if (targetLink.interpolation.Count > 2) //Barem 3 tocke postoje
                {
                    for (int k = targetLink.interpolation.Count - 2; k >= 1; k--)//Generiraj nove tocke, od zavrsne do pocetne, ne ukljucujuci pocetnu i zavrsnu
                    {
                        link.Add(Misc.convertToGmapPoint(targetLink.interpolation[k]));
                    }
                }

                GMapOverlay polyOverlay = new GMapOverlay("polygons");
                var j = new GMap.NET.WindowsForms.GMapPolygon(link, "mypolygon");
                j.Stroke = kroma;
                var jRoutes = new GMapOverlay(targetLink.linkID.ToString());
                jRoutes.Polygons.Add(j);
                map.Overlays.Add(jRoutes);
            }

            map.Zoom += 0.01;
            map.Zoom -= 0.01;
        }
        //END Dijkstra
        //--------------------------------------------------------------------------------------------------------------------------------------------------
        private void Btn_Set_Destination_Dijkstra_Click(object sender, EventArgs e)
        {
            if (!(DijkstraStartEnd.Count % 2 == 0) && DijkstraStartEnd.Count > 0)//There must be a start point in order to set a destination point
            {
                Radio_Btn_Marker_On.Checked = true;
                Radio_Btn_Marker_Off.Checked = false;
                Btn_Remove_Destination_Dijkstra.Visible = true;
                Btn_Set_Destination_Dijkstra.Visible = false;

                Link_Find_Btn.Select();
                Link_Find_Btn.PerformClick();//Pokretanje funkcije odredjivanja najblizeg linka 
                startChanged = false;//Start point did not change
            }
            else
                MessageBox.Show("Please set a starting point first!");
        }

        private void Btn_Remove_Destination_Dijkstra_Click(object sender, EventArgs e)
        {
            if ((DijkstraStartEnd.Count % 2 == 0) && DijkstraStartEnd.Count > 1)//2 points in dijkstra active,,,,, Remove the last (destination point)
            {
                Btn_Set_Destination_Dijkstra.Visible = true;
                Btn_Remove_Destination_Dijkstra.Visible = false;
                Radio_Btn_Marker_On.Checked = true;
                Radio_Btn_Marker_Off.Checked = false;
                drawOnePoint();//Delete only the destination point                
            }
            else
                MessageBox.Show("Destination point does not exist!");
        }

        private void Btn_Set_Start_Dijkstra_Click(object sender, EventArgs e)
        {
            Radio_Btn_Marker_On.Checked = true;
            Radio_Btn_Marker_Off.Checked = false;

            DijkstraStartEnd.Clear();//Delete all points
            map.Overlays.Clear();//Delete all visible points

            Link_Find_Btn.Select();
            Link_Find_Btn.PerformClick();//Pokretanje funkcije odredjivanja najblizeg linka 

            startChanged = true;//Start point did change
        }

        private void Btn_Quick_Stng_Clear_Map_Click(object sender, EventArgs e)
        {
            chart1.Visible = false;
            map.Overlays.Clear();
            DijkstraStartEnd.Clear();
            Rich_Txt_Bx_Dijkstra_Results.Text = string.Empty;
            map.Zoom += 1;
            map.Zoom -= 1;
            finalPath.Clear();
        }

        public void drawOnePoint()
        {
            DijkstraStartEnd.RemoveAt(DijkstraStartEnd.Count - 1);
            int listCount = DijkstraStartEnd.Count;
            map.Overlays.Clear();
            GMapOverlay marker = new GMapOverlay("marker");
            Vlonlat CoG = Misc.CenterOfGravity(DijkstraStartEnd[listCount - 1].b, DijkstraStartEnd[listCount - 1].e);
            PointLatLng p = Misc.convertToGmapPoint(CoG);//Iscrtavamo pinpoint na sredini linka, ali pamtimo cijeli link(Class object)
            SpeedLinks point = targetLink;
            GMap.NET.WindowsForms.GMapMarker markerType = new GMarkerGoogle(p, GMarkerGoogleType.green_pushpin);

            for (int i = 0; i < 3; i++)//Tek nakon treceg klika se iscrta na tocnoj poziciji... 
            {
                marker.Markers.Add(markerType);
                map.Overlays.Add(marker);
            }
            map.Zoom += 1;//Brisu "lazne" markere
            map.Zoom -= 1;
        }
        //-------------------------------------------------------------------------------------------------------------
        //K_Means Clustering
        private void Btn_Open_Cluster_Click(object sender, EventArgs e)
        {
            GB_Cluster.Visible = true;
            Btn_Close_Cluster.Visible = true;
            Btn_Open_Cluster.Visible = false;
            Btn_Dijkstra_Close.PerformClick();
            Stng_GTFO.PerformClick();
        }

        private void Btn_Close_Cluster_Click(object sender, EventArgs e)
        {
            GB_Cluster.Visible = false;
            Btn_Close_Cluster.Visible = false;
            Btn_Open_Cluster.Visible = true;
        }

        private void Btn_Cluster_Draw_Click(object sender, EventArgs e)
        {
            Btn_MapGen_Click(sender, e);           
            string clusterDay = "Workweek";
            string clusterType = "Speed";
            if (RB_Cluster_Weekend.Checked == true)
                clusterDay = "Weekend";
            if (RB_Cluster_Energy.Checked == true)
                clusterType = "Energy";
            int clusterID = 1;
            if (GB_Cluster.Visible == true && !RB_Class_On.Checked)
                clusterID = Convert.ToInt32(Cmb_Cluster.GetItemText(Cmb_Cluster.SelectedItem));
            List<int> clusterArray = V_E_cluster[clusterID].getArray(clusterType, clusterDay);
            Lab_Cluster_Count.Text = clusterArray.Count.ToString();

            //Btn_WF_Plt_Click(sender, e);
        }

        private void Btn_Cluster_Clear_Click(object sender, EventArgs e)
        {
            map.Overlays.Clear();
            map.Zoom += 1;
            map.Zoom -= 1;
        }

        private void Btn_Class_Id_Draw_Click(object sender, EventArgs e)
        {
            if (RB_Class_On.Checked)
            {
                Btn_MapGen_Click(sender, e);
            }
        }
    }
}
