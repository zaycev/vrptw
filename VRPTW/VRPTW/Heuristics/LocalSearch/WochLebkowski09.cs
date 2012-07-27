using System;
using System.Collections.Generic;
using System.Linq;
using VRP.VRPTW.Data;

namespace VRP.VRPTW.Heuristics.LocalSearch
{
    public class WochLebkowski09 : VrpSolver
    {
        public float IrmCoefA = (float) 1.15;
        public float CNumCoef = (float) .15;
        public int IrmIterations = 35;

        public override Solution Solve(Problem problem, Solution initialSolution)
        {
            var cNumber = (int) (problem.Customers.Count*CNumCoef);

            /* 1. */
            var selected = SelectionStage(initialSolution, cNumber);

            /* 2. */
            var p = .75;
            var probablyOrdered = OrderRoutesWithProb(initialSolution, p);

            /* 3. */
            var newSolution = ImproveSolution(initialSolution, selected, probablyOrdered);


            return newSolution;
        }


        public Solution ImproveSolution(Solution initialSolution, List<Customer> selected, List<Route> routes)
        {
            if (selected == null)
                return initialSolution;
            var oldDist = initialSolution.TotalDistance();
            Solution bestSolution = initialSolution;
            foreach (var c in selected)
            {
                foreach (var r in routes)
                {
                    var transfer = BestInsertionPlaceCzech2001(initialSolution, c, r);
                    if(transfer.Value < oldDist)
                    {
                        bestSolution = transfer.Key;
                        oldDist = transfer.Value;
                    }
                }
            }
            return bestSolution;
        }


        public List<Route> OrderRoutesWithProb(Solution solution, double prob)
        {
            var routes = solution.Copy().Routes;
            var rnd = new Random();
            var dice = rnd.NextDouble();
            if(dice > prob)
                return new List<Route>(routes);
            return routes.OrderBy(r => r.RouteList.Count).ToList();
        }


        public List<Customer> NeighbourhoodIrmSearch(List<Customer> customerList, Customer customer)
        {
            var custDist = new List<KeyValuePair<Customer, double>>();
            foreach (Customer cust in customerList)
                custDist.Add(new KeyValuePair<Customer, double>(cust, customer.Distance(cust)));
            custDist = new List<KeyValuePair<Customer, double>>(custDist.OrderBy(p => p.Value));
            double r = custDist[1].Value;
            var rnd = new Random();
            var selectedCustomers = new Dictionary<int, Customer>();

            for (int i = 0; i < IrmIterations; ++i)
            {
                double prob = 1/r;
                foreach (var cust in custDist)
                {
                    if (cust.Value > r) break;

                    double dice = rnd.NextDouble();
                    if (dice < prob)
                        if (! selectedCustomers.ContainsKey(cust.Key.Info.Id))
                            selectedCustomers.Add(cust.Key.Info.Id, cust.Key);
                }
                r *= IrmCoefA;
            }
            var neighbourhood = new List<Customer>(selectedCustomers.Values);
            return neighbourhood;
        }


        public KeyValuePair<Solution, double> BestInsertionPlaceCzech2001(Solution solution, Customer customer, Route route)
        {
            // Route customerRoute = solution.Routes[customerRouteIndex];
            // Customer customer = (Customer)customerRoute.RouteList[cusotomerIndex];
            // Route targetRoute = solution.Routes[targetRouteIndex];

            //int bestSolution = -1;
            double bestCost = solution.TotalDistance();

            //solution.Routes[cusotomerIndex] = customerRoute.Copy();
            //solution.Routes[cusotomerIndex].RouteList.RemoveAt(cusotomerIndex);

            var routeIndex = route.Index();
            var newSolution = solution.Copy();
            var oldRouteIdx = customer.Route.Index();
            var oldRoute = newSolution.Routes[oldRouteIdx].Copy();
            oldRoute.RemoveAt(customer.Index());
            newSolution.Routes[oldRouteIdx] = oldRoute;
            Solution bestSolutionCopy = null;

            for (int i = 1; i < route.RouteList.Count; ++i)
            {

                var newRoute = solution.Routes[routeIndex].Copy();
                newRoute.InsertCustomer(customer, i);
                newSolution.Routes[routeIndex] = newRoute;
                double newCost = newSolution.TotalDistance();
                if (newRoute.IsFeasible() && newCost < bestCost)
                {
                    bestSolutionCopy = newSolution.Copy();
                    bestCost = newCost;
                }
            }

            return new KeyValuePair<Solution, double>(bestSolutionCopy, bestCost);
        }


        public List<Customer> SelectionStage(Solution initialSolution, int cNumber)
        {
            //var deletionList = new List<Customer>(cNumber);


            var iter = 0;
            var neighbourhood = new List<Customer>();
            var routes = new List<Route>(initialSolution.Routes);

            while(neighbourhood.Count < cNumber){
                
                var customerList = initialSolution.CustomerListCopy();

                /* 1.1. */
                int routeOneIdx = RandomSampleIndex(0, routes.Count - 1);
            
                /* 1.2. */
                int customerIdx = RandomSampleIndex(1, routes[routeOneIdx].RouteList.Count - 2);
                Route routeOne = initialSolution.Routes[routeOneIdx];
                Customer customer = (Customer)routeOne.RouteList[customerIdx];

                /* 1.3. */
                neighbourhood = NeighbourhoodIrmSearch(customerList, customer);
            
                /* 1.4. */
                foreach (Customer cust in neighbourhood)
                    customerList.Remove(cust);

                /* 1.5. */
                if (neighbourhood.Count == 1)
                {
                    //routeOneIdx = RandomSampleIndex(0, routes.Count - 1);
                    //routeOne = initialSolution.Routes[routeOneIdx];
                    // implement 1.6.-1.8.
                    /*
                    int bestInsertion = BestInsertionPlaceCzech2001(initialSolution, customer.Route.Index(),
                                                                customer.Index(),
                                                                routeOne.Index());*/
                    //throw new Exception("WL09 LS: Only one neighbour is found");
                }

                //if (neighbourhood.Count < cNumber)
                //   return SelectionStage(initialSolution, cNumber);

                if (iter == 30000)
                    return null;
                iter++;
            }



            return neighbourhood;
        }
    }
}