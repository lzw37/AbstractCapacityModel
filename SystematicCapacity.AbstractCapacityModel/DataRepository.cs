using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace SystematicCapacity.AbstractCapacityModel
{
    public static class DataRepository
    {
        public static List<Train> TrainList;

        public static List<Movement> MovementList;

        public static List<Resource> ResourceList;

        public static List<Location> LocationList;

        public static List<Segment> SegmentList;

        public static void ReadData(string directory)
        {
            TrainList = new List<Train>();
            MovementList = new List<Movement>();
            ResourceList = new List<Resource>();
            LocationList = new List<Location>();
            SegmentList = new List<Segment>();

            ReadStationData(directory + "/Station.csv");
            ReadResourceData(directory + "/Resource.csv");
            ReadSegmentData(directory + "/Segment.csv");
            ReadTrainData(directory + "/Train.csv");
        }

        private static void ReadStationData(string filePath)
        {
            foreach(Dictionary<string,string> data in CSVReader(filePath))
            {
                Station sta = new Station() { ID = data["ID"], StationName = data["StationName"] };
                LocationList.Add(sta);
            }
        }

        private static void ReadResourceData(string filePath)
        {
            foreach (Dictionary<string, string> data in CSVReader(filePath))
            {
                Type resourceType = Type.GetType("SystematicCapacity.AbstractCapacityModel." + data["Type"]);

                Resource r = (Resource)Activator.CreateInstance(resourceType);

                r.ID = data["ID"];
                r.Description = data["Description"];

                ResourceList.Add(r);
            }
        }

        private static void ReadSegmentData(string filePath)
        {
            foreach (Dictionary<string, string> data in CSVReader(filePath))
            {
                Segment seg = new Segment()
                {
                    ID = data["ID"],
                    FromStation = (Station)LocationList.Find(x => x.ID == data["FromStationID"]),
                    ToStation = (Station)LocationList.Find(x => x.ID == data["ToStationID"]),
                    BindingBlockingSection = (BlockSection)ResourceList.Find(x=>x.ID == data["BindingBlockSection"]),
                    RunningTime = Convert.ToInt32(data["RunningTime"]),
                };
                SegmentList.Add(seg);
            }
        }

        private static void ReadTrainData(string filePath)
        {
            foreach(Dictionary<string, string> data in CSVReader(filePath))
            {
                Train tr = new Train()
                {
                    ID = data["ID"]
                };

                foreach (string segID in data["SegmentList"].Split(';'))
                {
                    Segment seg = SegmentList.Find(x => x.ID == segID);
                    tr.SegmentList.Add(seg);
                }

                TrainList.Add(tr);
            }
        }

        private static List<Dictionary<string, string>> CSVReader(string filePath)
        {
            List<Dictionary<string, string>> resultList = new List<Dictionary<string, string>>();

            FileStream fs = new FileStream(filePath, FileMode.Open);
            StreamReader sr = new StreamReader(fs);

            string[] itemNameArray = sr.ReadLine().Split(',');

            string dataStr = sr.ReadLine();
            while (dataStr != null && dataStr != "")
            {
                string[] data = dataStr.Split(',');
                Dictionary<string, string> dataDic = new Dictionary<string, string>();

                for (int i = 0; i < data.Length; i++)
                {
                    dataDic.Add(itemNameArray[i], data[i]);
                }

                resultList.Add(dataDic);

                dataStr = sr.ReadLine();
            }

            sr.Close();
            fs.Close();

            return resultList;
        }

        static internal void OutputResourceStatus()
        {
            FileStream fs = new FileStream("debug_ResourceStatus.csv", FileMode.Create);
            StreamWriter sw = new StreamWriter(fs);
            sw.WriteLine("ID,Status");
            foreach(Resource r in ResourceList)
            {
                foreach (ResourceStatus s in r.PossibleResourceStatus)
                {
                    sw.WriteLine("{0},{1}", r.ToString(), s.ToString());
                }
            }
            sw.Close();
            fs.Close();
        }

        static internal void OutputResourceStatusTransition()
        {
            FileStream fs = new FileStream("debug_ResourceStatusTrans.csv", FileMode.Create);
            StreamWriter sw = new StreamWriter(fs);
            sw.WriteLine("ID,StatusTransition");
            foreach(Resource r in ResourceList)
            {
                foreach(ResourceStatusTransition st in r.PossibleResourceStatusTransition)
                {
                    sw.WriteLine("{0},{1}", r.ToString(), st.ToString());
                }
            }
            sw.Close();
            fs.Close();
        }

        static internal void OutputMovements()
        {
            FileStream fs = new FileStream("debug_Movement.csv", FileMode.Create);
            StreamWriter sw = new StreamWriter(fs);
            sw.WriteLine("ID,FromLoc,FromTime,ToLoc,ToTime,Type,Description");
            foreach(Movement m in MovementList)
            {
                sw.WriteLine("{0},{1},{2},{3},{4},{5},{6}",
                    m.ID, m.FromLocation.ToString(), m.FromTime, m.ToLocation.ToString(), m.ToTime, m.GetType().Name, m.Description);
            }
            sw.Close();
            fs.Close();
        }

        static internal void OutputTrainPath()
        {
            FileStream fs = new FileStream("debug_TrainPath.csv", FileMode.Create);
            StreamWriter sw = new StreamWriter(fs);
            sw.WriteLine("ID,Path");
            foreach (Train tr in TrainList)
            {
                foreach (List<Movement> mList in tr.PossibleMovementPath)
                {
                    string pathStr = "";
                    foreach(Movement m in mList)
                    {
                        pathStr += m.ToString() + "-";
                    }
                    sw.WriteLine("{0},{1}", tr.ID, pathStr);
                }
            }
            sw.Close();
            fs.Close();
        }

        static internal void OutputMovementResource()
        {
            FileStream fs = new FileStream("debug_MovementResourceOccu.csv", FileMode.Create);
            StreamWriter sw = new StreamWriter(fs);
            sw.WriteLine("ID,Type,Time,Status");

            foreach(Movement m in MovementList)
            {
                foreach(ResourceSelectionGroup group in m.ResourceSelectionGroupSet)
                {
                    foreach(Resource r in group.ResourceStatusRuleList.Keys)
                    {
                        foreach(int t in group.ResourceStatusRuleList[r].BindingStatusDict.Keys)
                        {
                            foreach(ResourceStatus status in group.ResourceStatusRuleList[r].BindingStatusDict[t])
                            {
                                sw.WriteLine("{0},{1},{2},{3},Binding", m, group.ResourceType.Name, t, status);
                            }
                        }
                        foreach (int t in group.ResourceStatusRuleList[r].NecessityStatusDict.Keys)
                        {
                            foreach (ResourceStatus status in group.ResourceStatusRuleList[r].NecessityStatusDict[t])
                            {
                                sw.WriteLine("{0},{1},{2},{3},Necessity", m, group.ResourceType.Name, t, status);
                            }
                        }
                    }
                }
            }
            
            sw.Close();
            fs.Close();
        }

        static internal void OutputSolution(string directory)
        {
            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            FileStream fs = new FileStream(directory + "/movement_result.csv", FileMode.Create);
            StreamWriter sw = new StreamWriter(fs);

            sw.WriteLine("ID,FromLoc,ToLoc,FromTime,ToTime,VehicleID");
            foreach(Movement m in MovementList)
            {
                if (!m.IsActivated)
                    continue;

                ResourceSelectionGroup g = m.ResourceSelectionGroupSet.Find(x => x.ResourceType == typeof(Vehicle));
                Vehicle veh = g.SelectedResource as Vehicle;
                sw.WriteLine("{0},{1},{2},{3},{4},{5}", m.ID, m.FromLocation.ID, m.ToLocation.ID, m.FromTime, m.ToTime, veh.ID);
            }
            sw.Close();
            fs.Close();
        }
    }
}
