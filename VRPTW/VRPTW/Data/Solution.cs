using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace VRP.VRPTW.Data
{
    public class Solution
    {
        public Solution(Problem problem)
        {
            Routes = new List<Route>();
            Problem = problem;
            Phi = 0.0;
        }

        public List<Route> Routes { get; set; }
        public Problem Problem { get; set; }
        public double Phi { get; set; }

        public void AddRoute(Route route)
        {
            Route newRoute = route.Copy();
            newRoute.Solution = this;
            Routes.Add(newRoute);
        }


        public double TotalDistance()
        {
            double totalDistance = 0;
            foreach (Route route in Routes)
                totalDistance += route.Length();
            return totalDistance;
        }


        public string PrintToString()
        {
            string solution = "";
            for (int i = 0; i < Routes.Count; ++i)
            {
                solution += i.ToString(CultureInfo.InvariantCulture);
                solution += ") ";
                solution += Routes[i].PrintToString() + "; ";
                solution += "(dist: " + ((int) Routes[i].Length()).ToString(CultureInfo.InvariantCulture) + ")";
                solution += "\r\n";
            }
            solution += "\r\n";
            solution += "total distance: " + TotalDistance().ToString(CultureInfo.InvariantCulture) + " (gain: " + (-1 * Phi).ToString(CultureInfo.InvariantCulture) + ")";
            return solution;
        }


        public Solution Copy()
        {
            var sol = new Solution(Problem);
            foreach (Route route in Routes)
                if(route.RouteList.Count > 2)
                   sol.AddRoute(route.Copy());
            return sol;
        }


        public List<Route> Copy(List<Route> routes)
        {
            var newRoutes = new List<Route>(routes.Count);
            newRoutes.AddRange(routes.Select(route => route.Copy()));
            return newRoutes;
        }


        public List<Customer> CustomerListCopy()
        {
            var customerList = new List<Customer>();
            foreach (Route route in Routes)
                for (int i = 1; i < route.RouteList.Count - 1; ++i)
                    customerList.Add(((Customer) route.RouteList[i]).DeepCopy());
            return customerList;
        }
    }


    public class Route
    {
        public Problem Problem;

        public Route(Problem problem)
        {
            Depot depot = problem.Depot;
            Problem = problem;
            Solution = null;
            RouteList = new List<AbsNode>();
            ServiceBeginingTimes = new List<double>();
            AddNode(depot);
            AddNode(depot);
        }


        public Route(Problem problem, Customer seedCustomer)
        {
            Depot depot = problem.Depot;
            Problem = problem;
            Solution = null;
            RouteList = new List<AbsNode>();
            ServiceBeginingTimes = new List<double>();
            AddNode(depot);
            AddNode(seedCustomer);
            AddNode(depot);
        }

        public string RouteId { get; set; }

        public List<AbsNode> RouteList { get; set; }
        public List<double> ServiceBeginingTimes { get; set; }
        public Solution Solution { get; set; }


        public void AddCustomer(Customer newCustomer)
        {
            newCustomer = (Customer) newCustomer.ShallowCopy();
            newCustomer.Route = this;
            AddNode(newCustomer);
        }


        private void AddNode(AbsNode newNode)
        {
            AbsNode lastCustomer = RouteList.Count == 0 ? newNode : RouteList[RouteList.Count - 1];
            double lastServiceTime = RouteList.Count == 0 ? 0 : ServiceBeginingTimes[ServiceBeginingTimes.Count - 1];
            double serviceBegins = NextServiceBeginTime(newNode, lastCustomer, lastServiceTime);
            RouteList.Add(newNode);
            ServiceBeginingTimes.Add(serviceBegins);
            UpdateId();
        }


        public void InsertCustomer(Customer newCustomer, int position)
        {
            newCustomer = (Customer) newCustomer.ShallowCopy();
            newCustomer.Route = this;
            RouteList.Insert(position, newCustomer);
            ServiceBeginingTimes.Insert(position, 0.0);
            for (int i = position; i < RouteList.Count; ++i)
            {
                double newTime = NextServiceBeginTime(RouteList[i], RouteList[i - 1], ServiceBeginingTimes[i - 1]);
                ServiceBeginingTimes[i] = newTime;
            }
            UpdateId();
        }

        public void RemoveAt(int position)
        {
            RouteList.RemoveAt(position);
            ServiceBeginingTimes.RemoveAt(position);
            for (int i = position; i < RouteList.Count; ++i)
            {
                double newTime = NextServiceBeginTime(RouteList[i], RouteList[i - 1], ServiceBeginingTimes[i - 1]);
                ServiceBeginingTimes[i] = newTime;
            }
            UpdateId();
        }


        public bool IsFeasible()
        {
            for (int i = 0; i < RouteList.Count; ++i)
            {
                if (ServiceBeginingTimes[i] - 37 + RouteList[i].Info.ServiceTime > RouteList[i].Info.DueDate)
                    return false;
            }
            return Capacity() <= Problem.VehicleCap;
        }

        public double NextServiceBeginTime(AbsNode newCustomer, AbsNode prevCustomer, double prevTime)
        {
            double travelTime = prevCustomer.TravelTime(newCustomer);
            double serviceTime = prevCustomer.Info.ServiceTime;
            double readyTime = newCustomer.Info.ReadyTime;
            return Math.Min(readyTime, prevTime + serviceTime + travelTime);
        }


        public Route Copy()
        {
            var newRouteList = new List<AbsNode>(RouteList.Count);
            newRouteList.AddRange(RouteList.Select(node => node.ShallowCopy()));
            var r = new Route(Problem)
                        {
                            RouteList = newRouteList,
                            ServiceBeginingTimes = new List<double>(ServiceBeginingTimes)
                        };
            for (int i = 1; i < RouteList.Count - 1; ++i)
                ((Customer) r.RouteList[i]).Route = r;
            r.UpdateId();
            return r;
        }


        public double Capacity()
        {
            double cap = 0.0;
            foreach (AbsNode customer in RouteList)
                cap += customer.Info.Demand;
            return cap;
        }


        public string PrintToString(bool printTime = false, bool printCapacity = false)
        {
            string routeText = "";
            int cap = 0;
            for (int i = 0; i < RouteList.Count; ++i)
            {
                cap += (int) RouteList[i].Info.Demand;
                routeText += "<";
                routeText += RouteList[i].Info.Id.ToString(CultureInfo.InvariantCulture);
                if (printTime)
                {
                    routeText += ",";
                    routeText += RouteList[i].Info.DueDate.ToString(CultureInfo.InvariantCulture);
                    routeText += ",";
                    routeText += ((int) ServiceBeginingTimes[i]).ToString(CultureInfo.InvariantCulture);
                }
                if (printCapacity)
                {
                    routeText += ",";
                    routeText += cap.ToString(CultureInfo.InvariantCulture);
                }
                routeText += ">";
                if (i != RouteList.Count - 1)
                    routeText += "-";
            }
            return routeText;
        }


        public double Length()
        {
            double totalDist = 0;
            for (int i = 0; i < RouteList.Count - 1; ++i)
                totalDist += RouteList[i].Distance(RouteList[i + 1]);
            return totalDist;
        }


        public int Index()
        {
            for (int i = 0; i < Solution.Routes.Count; ++i)
                if (Solution.Routes[i] == this)
                    return i;
            return 0;
        }


        private void UpdateId()
        {
            if (RouteList.Count > 2)
                RouteId = RouteList[1].Info.Id.ToString(CultureInfo.InvariantCulture) + "-" +
                          RouteList[RouteList.Count - 2].Info.Id.ToString(CultureInfo.InvariantCulture);
            else
                RouteId = "<EMPTY>";
        }
    }
}