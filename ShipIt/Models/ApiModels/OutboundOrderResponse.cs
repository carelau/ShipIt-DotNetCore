using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ShipIt.Models.ApiModels
{

    //How many trucks we will need to process the order
    //What items should be contained in each truck
    //The total weight of the items in each truck
    public class OutboundOrderResponse
    {
        public List<Truck> Trucks { get; set; }
     

    }

      

    public class Truck
    {

        public double TruckWeight;

        public List<Order> Orders;


    }

    public class Order
    {
        public string Name { get; set; }
        public double ItemWeight { get; set; }
        public string Gtin { get; set; }
        public double Quantity { get; set; }
        public double TotalOrderWeight { get; set; }
        
    }
}