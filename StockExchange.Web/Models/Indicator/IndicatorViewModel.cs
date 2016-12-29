﻿using StockExchange.Business.Indicators.Common;
using System.Collections.Generic;

namespace StockExchange.Web.Models.Indicator
{
    public class IndicatorViewModel
    {
        public IndicatorType Type { get; set; }

        public string Name { get; set; }

        public IList<IndicatorPropertyViewModel> Properties { get; set; } = new List<IndicatorPropertyViewModel>();

    }
}