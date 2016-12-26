﻿using StockExchange.Web.Models.Charts;
using System.Collections.Generic;
using StockExchange.Business.Models.Wallet;

namespace StockExchange.Web.Models.Wallet
{
    public class WalletViewModel
    {
        public decimal FreeBudget { get; set; }

        public decimal AllStocksValue { get; set; }

        public decimal TotalBudget => FreeBudget + AllStocksValue;
        
        public int AllTransactionsCount { get; set; }

        public int CurrentSignalsCount { get; set; }

        public string CurrentStrategyName { get; set; }

        public IList<OwnedCompanyStocksDto> OwnedCompanyStocks { get; set; } = new List<OwnedCompanyStocksDto>();

        public PieChartModel StocksByValue { get; set; }

        public PieChartModel StocksByQuantity { get; set; }
    }
}