using System.Collections.Generic;
using System.Globalization;
using Kent.Boogaart.KBCsv;

namespace VRP.VRPTW.Data
{
    interface IProblemReader
    {
        Problem Read(string source);
    }


    internal class CsvProblemReader : IProblemReader
    {
        public Problem Read(string csvFilePath)
        {
            var nodes = new List<NodeInfo>();
            var p = new Problem();
            using (var reader = new CsvReader(csvFilePath))
            {
                reader.ReadHeaderRecord();
                foreach (var record in reader.DataRecords)
                {
                    nodes.Add(new NodeInfo
                                      {
                                          Id = int.Parse(record["Id"]),
                                          X = float.Parse(record["X"], CultureInfo.InvariantCulture),
                                          Y = float.Parse(record["Y"], CultureInfo.InvariantCulture),
                                          Demand = float.Parse(record["Demand"], CultureInfo.InvariantCulture),
                                          ReadyTime = float.Parse(record["ReadyTime"], CultureInfo.InvariantCulture),
                                          DueDate = float.Parse(record["DueDate"], CultureInfo.InvariantCulture),
                                          ServiceTime = float.Parse(record["ServiceTime"], CultureInfo.InvariantCulture)
                                      });
                }     
            }
            p.SetNodes(nodes);
            return p;
        }

        public static string ExtractProblemAbbr(string csvFilePath)
        {
            return null;
        }

        public static int ExtractVehicleCap(string csvFilePath)
        {
            return 0;
        }
}

}