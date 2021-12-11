# Bachelor's thesis project
This repository contains the C# source code of my Bachelor's  thesis project.
This project was my first "out of class" programming project.

## Important
* The main FCD (Floating Car Data) data set is confidential and thus is not included in the repository.
* The data pre-processing is not included in the repository.
* The GUI was created using the GMap.NET open source visualization packet.


## Title
Road energy profile estimation based on vehicle movement historical records

## Abstract
The share of electric vehicles is gradually increasing in the global vehicle market. Although electric vehicles have several advantages, they are characterised by relatively low cruising range (autonomy). Therefore, optimal battery utilization is a crucial question and challenge. Road traffic monitoring produces vast quantities of real time and historical data. Historical traffic data is a valuable resource in the field of traffic analysis and it constitutes the primary data used in this research. The aim of this research is to analyse road energy profiles from a spatio-temporal aspect and to develop a graphical user interface for integrating and illustrating the results of the research for end users. 

To achieve these aims we have implemented a methodology for estimating road energy profiles based on historical traffic data analysis. Furthermore, we have developed an approach for computing electric vehicle electricity consumption (or generation) on individual road segments. However, the values of the road’s energy profile change in the course of a day. Thus we have created an graphical user interface through which we can observe road energy profile values at a given time of day. We have also implemented an algorithm for clustering road segments based on similar energy profiles. Finally, we have developed an modified vehicle routing algorithm which dynamically calculates optimal routes based on road energy profiles at a given time of day.

The results of this research can help inform policy makers on the given situation for electric vehicles implementation and proliferation in the larger urban area of Zagreb city. More importantly, combined with the graphical user interface and custom routing algorithm, the outcomes of this research could provide electric vehicle users a valuable tool for optimizing their vehicle utilization.

Keywords: NoSQL, C#, large datasets, traffic data analysis, road energy profiles, vehicle routing, road clustering, electric vehicles

## Results overview
<img src="https://github.com/SimpleBro/BachThesis_Project/blob/master/Bacc_Photos/guy_de_maupassant.PNG" alt="GUI" height="500"/>
<img src="https://github.com/SimpleBro/BachThesis_Project/blob/master/Bacc_Photos/jutro_ww_en.PNG" alt="GUI" height="500"/>

The results are divided in three GUI panels:
* General (Road profile visualization)
* Dijkstra (Routing algorithm)
* Cluster (K-Means road clustering)

### Road profile visualization
For the observed set of roads (links) the user can choose to visualize three types of data (link profiles) for each 5-minute time interval in the day:
* Speed profile
* Acceleration profile
* Energy profile

The user can observe only the area of interest by drawing a rectangle on the map. 

The user can click on the map, and the data of a link that is nearest to the point where the user clicked will be visualized.

<img src="https://github.com/SimpleBro/BachThesis_Project/blob/master/Bacc_Photos/gaccvelika.PNG" alt="Acceleration profile of the road network in the user selected rectangle" height="500"/> 
<img src="https://github.com/SimpleBro/BachThesis_Project/blob/master/Bacc_Photos/gen_2.PNG" alt="User drawn rectangle" height="500"/>

Additionally, the user can change the target Vehicle specifications which are used when computing the mathematical model.

<img src="https://github.com/SimpleBro/BachThesis_Project/blob/master/Bacc_Photos/Car_Data.PNG" alt="Vehicle specifications altering" width="500" height="500"/> <img src="https://github.com/SimpleBro/BachThesis_Project/blob/master/Bacc_Photos/Jadranski_Most_Sjever_WW_EN.PNG" alt="Energy road profile" width="500" height="500"/>

### Routing algorithm
In the Dijkstra panel, the user can choose to generate a least-cost path between two selected links in the observed 5-minute time interval. The user can select one of three types of edge weight:
* Length
* Energy
* Speed

Depending on the selected weright, the routing algorithm will calculate the least cost path from A to B and visualize the metadata to the user.
The following images illustrate the routing algorithm results for the same start-destination route pair.

Energy edge weight:

<img src="https://github.com/SimpleBro/BachThesis_Project/blob/master/Bacc_Photos/Dij_Energy_M.PNG" alt="Energy weight route visualization" width="500" height="500"/> <img src="https://github.com/SimpleBro/BachThesis_Project/blob/master/Bacc_Photos/Dij_En_Data.PNG" alt="Energy weight route metadata" width="500" height="500"/>

Speed edge weight:

<img src="https://github.com/SimpleBro/BachThesis_Project/blob/master/Bacc_Photos/Dij_Spd_map.PNG" alt="Speed weight route visualization" width="500" height="500"/> <img src="https://github.com/SimpleBro/BachThesis_Project/blob/master/Bacc_Photos/Dij_spd_data.PNG" alt="Speed weight route metadata" width="500" height="500"/>
