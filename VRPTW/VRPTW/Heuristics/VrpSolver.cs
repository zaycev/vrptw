using System;
using System.Collections.Generic;
using System.Linq;
using VRP.VRPTW.Data;

namespace VRP.VRPTW.Heuristics
{
    public abstract class VrpSolver
    {
        public abstract Solution Solve(Problem problem, Solution initialSolution);


        public int SearhForHighestDistance(Depot depot, List<Customer> customers)
        {
            int index = 0;
            double maxDist = 0;
            for (int i = 0; i < customers.Count; ++i)
            {
                double distance = depot.Distance(customers[i]);
                if (distance < maxDist) continue;
                maxDist = distance;
                index = i;
            }
            return index;
        }


        public int SearhForHighestDemand(Depot depot, List<Customer> customers)
        {
            int index = 0;
            double minDemand = 0;
            for (int i = 0; i < customers.Count; ++i)
            {
                float demand = customers[i].Info.DueDate;
                //customers[i].DueDate - customers[i].ReadyTime - customers[i].ServiceTime;
                if (demand > minDemand) continue;
                minDemand = demand;
                index = i;
            }
            return index;
        }


        public IEnumerable<T> RandomSampleSequence<T>(IEnumerable<T> population, int n)
        {
            var rnd = new Random();
            IEnumerable<T> samples = population.OrderBy(r => rnd.Next()).Take(n);
            return samples;
        }


        public List<int> RandomIndexSampleSequence(int bottom, int top, int n)
        {
            var rnd = new Random();
            var sampleSet = new HashSet<int>();
            while (sampleSet.Count < n)
                sampleSet.Add(rnd.Next(bottom, top));
            return new List<int>(sampleSet);
        }


        public int RandomSampleIndex(int bottom, int top)
        {
            return RandomIndexSampleSequence(bottom, top, 1)[0];
        }


        public T RandomSample<T>(IEnumerable<T> population)
        {
            return RandomSampleSequence(population, 1).First();
        }
    }
}