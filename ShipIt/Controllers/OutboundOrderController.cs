﻿using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using ShipIt.Exceptions;
using ShipIt.Models.ApiModels;
using ShipIt.Repositories;

namespace ShipIt.Controllers
{
    [Route("orders/outbound")]
    public class OutboundOrderController : ControllerBase
    {
        private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod()?.DeclaringType);

        private readonly IStockRepository _stockRepository;
        private readonly IProductRepository _productRepository;

        public OutboundOrderController(IStockRepository stockRepository, IProductRepository productRepository)
        {
            _stockRepository = stockRepository;
            _productRepository = productRepository;
        }

        [HttpPost("")]
        // OutbounOrderRM has warehouse ID and list of orderline. Orderline has gtin and quantity
        public void Post([FromBody] OutboundOrderRequestModel request)
        {
            Log.Info(String.Format("Processing outbound order: {0}", request));

            var gtins = new List<String>();
            //checks there are no duplicate gtins and only add after to gtins list
            foreach (var orderLine in request.OrderLines)
            {
                if (gtins.Contains(orderLine.gtin))
                {
                    throw new ValidationException(String.Format("Outbound order request contains duplicate product gtin: {0}", orderLine.gtin));
                }
                gtins.Add(orderLine.gtin);
            }

            //  function returns a list of productdatamodel which includes weight
            var productDataModels = _productRepository.GetProductsByGtin(gtins);

            //products is a dictionary with key Gtin and value with Product object with properties like Id,Gtin,Name,Weight 
            var products = productDataModels.ToDictionary(p => p.Gtin, p => new Product(p));

            var lineItems = new List<StockAlteration>();
            var productIds = new List<int>();
            var errors = new List<string>();


            foreach (var orderLine in request.OrderLines)
            {
                if (!products.ContainsKey(orderLine.gtin))
                {
                    errors.Add(string.Format("Unknown product gtin: {0}", orderLine.gtin));
                }
                else
                {
                    var product = products[orderLine.gtin];
                    //Stock Alteration is an object that has productId, quantity
                    lineItems.Add(new StockAlteration(product.Id, orderLine.quantity));
                    productIds.Add(product.Id);
                    Console.WriteLine("Hello");
                    double numOfTrucks = GetNumberOfTrucks(lineItems);
                    Console.WriteLine($"Product Id :{product.Id}, Quantity: {orderLine.quantity}, Weight: {product.Weight}, Number of Trucks: {numOfTrucks}");
                }
            }

            if (errors.Count > 0)
            {
                throw new NoSuchEntityException(string.Join("; ", errors));
            }

            
            // function that returns a dictionary of key and value stockdatamodel. stockdatamodel has productid, warehouseid and held Dictionary<int, StockDataModel> GetStockByWarehouseAndProductIds(int warehouseId, List<int> productIds);
            var stock = _stockRepository.GetStockByWarehouseAndProductIds(request.WarehouseId, productIds);

            var orderLines = request.OrderLines.ToList();
            errors = new List<string>();

            
            // line items is a list of stock alterations
            for (int i = 0; i < lineItems.Count; i++)
            {
                var lineItem = lineItems[i];
                var orderLine = orderLines[i];

                if (!stock.ContainsKey(lineItem.ProductId))
                {
                    errors.Add(string.Format("Product: {0}, no stock held", orderLine.gtin));
                    continue;
                }

                var item = stock[lineItem.ProductId];
                if (lineItem.Quantity > item.held)
                {
                    errors.Add(
                        string.Format("Product: {0}, stock held: {1}, stock to remove: {2}", orderLine.gtin, item.held,
                            lineItem.Quantity));
                }
            }

            if (errors.Count > 0)
            {
                throw new InsufficientStockException(string.Join("; ", errors));
            }

            _stockRepository.RemoveStock(request.WarehouseId, lineItems);

            // double numOfTrucks = GetNumberOfTrucks(lineItems);
            // Console.WriteLine("{0}",numOfTrucks);
        }
   
       private double GetNumberOfTrucks (List<StockAlteration> lineItems)
        {
        double totalWeight = 0;
            double numOfTrucks = 0;
          foreach (var lineItem in lineItems)
          {
            totalWeight += _productRepository.GetProductById(lineItem.ProductId).Weight* lineItem.Quantity;
            numOfTrucks = Math.Ceiling (totalWeight/2000);
           
          }

        return numOfTrucks;
        } 





    }
}