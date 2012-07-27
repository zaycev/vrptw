using System;
using System.Collections.Generic;
using System.Linq;

namespace VRP.VRPTW.Data
{
    public class Problem
    {
        public string Abbr { get; set; }
        public int VehicleCap { get; set; }
        public Depot Depot { get; set; }
        public List<Customer> Customers { get; set; }
        public List<NodeInfo> AllNodes { get; set; }


        public void SetNodes(List<NodeInfo> nodes, string abbr="None", int capacity=200)
        {
            VehicleCap = capacity;
            Abbr = abbr;
            AllNodes = nodes;
            Depot = new Depot(nodes[0]);
            Customers = new List<Customer>();
            for (var i = 1; i < nodes.Count; ++i)
                Customers.Add(new Customer(nodes[i]));
        }


        public Customer SearchbyId(int id)
        {
            foreach (var customer in Customers)
                if (customer.Info.Id == id)
                    return customer;
            throw new Exception("Customer not found");
        }


        public void SetAllNodes()
        {
            AllNodes = new List<NodeInfo> {Depot.Info};
            foreach (var customer in Customers)
                AllNodes.Add(customer.Info);
        }
    }



    public class NodeInfo
    {
        public int Id { get; set; }
        public float X { get; set; }
        public float Y { get; set; }
        public float Demand { get; set; }
        public float ReadyTime { get; set; }
        public float DueDate { get; set; }
        public float ServiceTime { get; set; }
    }



    public abstract class AbsNode
    {
        public NodeInfo Info;


        public double Distance(AbsNode destination)
        {
            var xDist = Info.X - destination.Info.X;
            var yDist = Info.Y - destination.Info.Y;
            return Math.Sqrt(xDist * xDist + yDist * yDist);
        }


        public double TravelTime(AbsNode destination)
        {
            return Distance(destination);
        }


        public virtual AbsNode ShallowCopy()
        {
            throw new Exception("You cannot copy abstract node");
        }
    }



    public class Depot : AbsNode
    {
        public Depot(NodeInfo info)
        {
            Info = info;
        }


        public override AbsNode ShallowCopy()
        {
            return new Depot(Info);
        }
    }



    public class Customer : AbsNode
    {
        public Route Route { get; set; }


        public Customer(NodeInfo info)
        {
            Info = info;
            Route = null;
        }


        public override AbsNode ShallowCopy()
        {
            return DeepCopy();
        }



        public Customer DeepCopy()
        {
            return new Customer(Info)
                                  {
                                      Route = Route
                                  };
        }


        public int Index()
        {
            for (var i = 0; i < Route.RouteList.Count; ++i)
                if (Route.RouteList[i].Info.Id == Info.Id)
                    return i;
            return -1;
        }
    }
}