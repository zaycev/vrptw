using System.Collections.Generic;
using VRP.VRPTW.Data;

namespace VRP.VRPTW.Heuristics.Construction
{
    internal enum Solomon87InitialStrategy
    {
        HighestDistance,
        HighestDemand
    }



    internal class Solomon87 : VrpSolver
    {
        public static double CoefMu = 1.0;
        public static double CoefAlpha1 = 0.5;
        public static double CoefAlpha2 = 0.5;
        public static double CoefLambda = 1;


        public int SelectSeedCustomer(List<Customer> customers, Depot depot,
                                      Solomon87InitialStrategy strategy = Solomon87InitialStrategy.HighestDemand)
        {
            if (strategy == Solomon87InitialStrategy.HighestDemand)
                return SearhForHighestDemand(depot, customers);
            if (strategy == Solomon87InitialStrategy.HighestDistance)
                return SearhForHighestDistance(depot, customers);
            return -1;
        }


        public override Solution Solve(Problem problem, Solution initialSolution = null)
        {
            var solution = new Solution(problem);
            var unroutedCustomers = new List<Customer>(problem.Customers);
            int seedId = SelectSeedCustomer(unroutedCustomers, problem.Depot);
            Customer seed = unroutedCustomers[seedId];
            unroutedCustomers.RemoveAt(seedId);
            var partialRoute = new Route(problem, seed);

            while (unroutedCustomers.Count > 0)
            {
                Route newRoute = partialRoute.Copy();
                var bestU = new List<int>();
                var c1Vals = new List<double>();
                foreach (Customer cust in unroutedCustomers)
                {
                    double minC1 = double.MaxValue;
                    int optimalU = 1;
                    for (int i = 1; i < newRoute.RouteList.Count - 1; ++i)
                    {
                        double c1 = CriterionC1(i, cust, i + 1, newRoute);
                        if (c1 < minC1)
                        {
                            minC1 = c1;
                            optimalU = i;
                        }
                    }
                    bestU.Add(optimalU);
                    c1Vals.Add(minC1);
                }

                int bestCust = 0;
                double minC2 = double.MaxValue;
                for (int i = 0; i < unroutedCustomers.Count; ++i)
                {
                    double c2 = CriterionC2(unroutedCustomers[i], c1Vals[i], newRoute);
                    if (c2 < minC2)
                    {
                        minC2 = c2;
                        bestCust = i;
                    }
                }

                Customer newCustomer = unroutedCustomers[bestCust];
                newRoute.InsertCustomer(newCustomer, bestU[bestCust]);

                if (newRoute.IsFeasible())
                {
                    unroutedCustomers.RemoveAt(bestCust);
                    partialRoute = newRoute;
                }
                else
                {
                    solution.AddRoute(partialRoute);
                    seedId = SelectSeedCustomer(unroutedCustomers, problem.Depot);
                    seed = unroutedCustomers[seedId];
                    unroutedCustomers.RemoveAt(seedId);
                    partialRoute = new Route(problem, seed);
                }

                if (unroutedCustomers.Count == 0)
                    solution.AddRoute(partialRoute);
            }

            return solution;
        }


        public double CriterionC11(int i, AbsNode u, int j, Route route)
        {
            var custI = route.RouteList[i];
            var custJ = route.RouteList[j];
            double distIu = custI.Distance(u);
            double distUj = u.Distance(custJ);
            double distJi = custJ.Distance(custI);
            return distIu + distUj + CoefMu*distJi;
        }


        public double CriterionC12(int i, AbsNode u, int j, Route route)
        {
            var custI = route.RouteList[i];
            var custJ = route.RouteList[j];
            double bI = route.ServiceBeginingTimes[i];
            double bU = route.NextServiceBeginTime(u, custI, bI);
            double bJu = route.NextServiceBeginTime(custJ, u, bU);
            double bJ = route.ServiceBeginingTimes[j];
            return bJu - bJ;
        }


        public double CriterionC1(int i, AbsNode u, int j, Route route)
        {
            return CoefAlpha1*CriterionC11(i, u, i, route) + CoefAlpha2*CriterionC12(i, u, j, route);
        }


        public double CriterionC2(AbsNode u, double c1Value, Route route)
        {
            double d0U = route.RouteList[0].Distance(u);
            return CoefLambda*d0U - c1Value;
        }
    }
}