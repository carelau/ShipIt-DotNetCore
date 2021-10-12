using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using Npgsql;
using ShipIt.Models.ApiModels;
using ShipIt.Models.DataModels;
using ShipIt.Repositories;

namespace ShipIt.Services
{
    //How many trucks we will need to process the order
    //What items should be contained in each truck
    //The total weight of the items in each truck

    // Trucklist is a list of trucks
    // Each truck has a list of order

    public class TruckFeature {

       //return a list of trucks based on the list of stock alterations
     
        private readonly IProductRepository _productRepository;

        private double MaxTruckLoad = 2000;
        public TruckFeature (IProductRepository productRepository)
        {
            productRepository = _productRepository;
        }

         //Stock Alteration is an object that has productId, quantity
        public List<Truck> LoadTruck (List<StockAlteration> lineItems)
        {
            List<Truck> truckList = new List<Truck>();
            
            foreach (var lineItem in lineItems)
          {
            var product = _productRepository.GetProductById(lineItem.ProductId);
            var  maxQuantityOfProductsPerTruck = Math.Floor(MaxTruckLoad/product.Weight); 
            double currentLineItemQuantity = lineItem.Quantity;

             while (currentLineItemQuantity >0 )
             {
                double quantityAdded = Math.Min ( lineItem.Quantity, maxQuantityOfProductsPerTruck);
                 
                 Order order = new Order {
                   Name = product.Name,
                    Gtin = product.Gtin,
                    Quantity = quantityAdded,
                    ItemWeight = product.Weight,
                    TotalOrderWeight = quantityAdded * product.Weight,  
                };
                
                currentLineItemQuantity = lineItem.Quantity - quantityAdded;

                
                Truck truck = new Truck();
                truckList.Add (truck);
                truck.Orders.Add(order);
                // CalculateTruckWeight (List<Order> orders)

                }
             }
             return truckList;

            }

    
    }
}

   /* public Truck GetAvailableTrucks (List<Truck> truckList, Order order)
   {
       return truckList.Where (p => p.TruckWeight + order.TotalOrderWeight <= MaxTruckLoad).FirstOrDefault();

   }

   
    private double CalculateTruckWeight(Truck truck)
        {
            TruckWeight = truck.Orders.Sum (order => order.TotalOrderWeight );
            return TruckWeight;
        }
   */