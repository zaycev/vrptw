using System;
using System.Collections.Generic;
using VRP.VRPTW.Data;
using VRP.VRPTW.Heuristics;
using VRP.VRPTW.Heuristics.LocalSearch;

namespace VRP.VRPTW.Gain
{

    public delegate void Update(int max, int current);

    public static class ShapleyDivider
    {
        private static readonly List<int> DefaultFraction = new List<int>(){30, 30, 40};
        private static readonly VrpSolver DefaultSolver = new WochLebkowski09();

        public static List<Problem> Divide(Problem problem, List<int> frac=null)
        {
            if(frac == null) frac = DefaultFraction;

            var subproblems = new List<Problem>(frac.Count);
            var rnd = new Random();
            var customers = new List<Customer>(problem.Customers);

            foreach (var count in frac)
            {
                var p = new Problem
                {
                    Abbr = problem.Abbr,
                    VehicleCap = problem.VehicleCap,
                    Depot = problem.Depot,
                    Customers = new List<Customer>(count)
                };

                for(var i = 0; i < count; ++i)
                {
                    var j = rnd.Next(0, customers.Count);
                    p.Customers.Add(customers[j]);
                    customers.RemoveAt(j);
                }             
                p.SetAllNodes();
                subproblems.Add(p);
            }

            return subproblems;
        }

        public static Problem Merge(Problem p1, Problem p2)
        {
            var newP = new Problem()
                           {
                               Depot = p1.Depot,
                               VehicleCap = p1.VehicleCap,
                               Abbr = p1.Abbr,
                               Customers = new List<Customer>()
                           };
            foreach (var c in p1.Customers)
                newP.Customers.Add(c);
            foreach (var c in p2.Customers)
                newP.Customers.Add(c);
            newP.SetAllNodes();
            return newP;
        }


        public static List<double> ComputeGains(ref Solution p1, ref Solution p2, ref Solution p3, ref Solution p12, ref Solution p23, ref Solution p31, ref Solution p123,
            int max_iters = 30, Update update = null, VrpSolver solver = null)
        {
            if (solver == null) solver = DefaultSolver;
            /////

            for (var i = 0; i < max_iters; ++i)
            {
                p1 = solver.Solve(p1.Problem, p1);
                p2 = solver.Solve(p2.Problem, p2);
                p3 = solver.Solve(p3.Problem, p3);
                p12 = solver.Solve(p12.Problem, p12);
                p23 = solver.Solve(p23.Problem, p23);
                p31 = solver.Solve(p31.Problem, p31);
                p123 = solver.Solve(p123.Problem, p123);

                var v1 = p1.TotalDistance();
                var v2 = p2.TotalDistance();
                var v3 = p3.TotalDistance();
                var v12 = p12.TotalDistance();
                var v23 = p23.TotalDistance();
                var v31 = p31.TotalDistance();
                var v123 = p123.TotalDistance();

                var phi1 = 1.0 * 3.0 / v1 + 1.0 / 6.0 * (v12 - v2) + 1.0 / 6.0 * (v31 - v3) + 1.0 / 3.0 * (v123 - v23);
                var phi2 = 1.0 * 3.0 / v2 + 1.0 / 6.0 * (v23 - v3) + 1.0 / 6.0 * (v12 - v1) + 1.0 / 3.0 * (v123 - v31);
                var phi3 = 1.0 * 3.0 / v3 + 1.0 / 6.0 * (v31 - v1) + 1.0 / 6.0 * (v23 - v2) + 1.0 / 3.0 * (v123 - v12);

                p1.Phi = v1 - phi1;
                p2.Phi = v2 - phi2;
                p3.Phi = v3 - phi3;

                update(max_iters, i);                               
            }

            return null;
        }

    }
}
