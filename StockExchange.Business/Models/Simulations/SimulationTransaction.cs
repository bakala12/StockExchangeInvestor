﻿using System;

namespace StockExchange.Business.Models.Simulations
{
    public class SimulationTransaction
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public int CompanyId { get; set; }
        public SimulationResult SimulationResult { get; set; }
    }
}
