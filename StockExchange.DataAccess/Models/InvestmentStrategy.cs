﻿using System.Collections.Generic;

namespace StockExchange.DataAccess.Models
{
    public class InvestmentStrategy
    {
        public int Id { get; set; }

        public int UserId { get; set; }
        public virtual User User { get; set; }

        public virtual ICollection<InvestmentCondition> Conditions { get; set; } = new HashSet<InvestmentCondition>();
    }
}
