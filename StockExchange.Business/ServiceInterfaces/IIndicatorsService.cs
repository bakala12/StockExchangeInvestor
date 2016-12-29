﻿using System;
using StockExchange.Business.Models.Indicators;
using System.Collections.Generic;
using StockExchange.Business.Indicators.Common;
using StockExchange.DataAccess.Models;

namespace StockExchange.Business.ServiceInterfaces
{
    public interface IIndicatorsService
    {
        IList<IndicatorProperty> GetPropertiesForIndicator(IndicatorType type);

        IList<CompanyIndicatorValues> GetIndicatorValues(IIndicator indicator, IList<int> companyIds);

        IList<CompanyIndicatorValues> GetIndicatorValues(IndicatorType type, IList<int> companyIds);

        IList<IndicatorType> GetAllIndicatorTypes();

        IList<IndicatorDto> GetAllIndicators();

        IndicatorType? GetTypeFromName(string indicatorName);

        IList<ParameterizedIndicator> ConvertIndicators(IEnumerable<StrategyIndicator> indicators);

        IList<SignalEvent> GetSignals(DateTime startDate, DateTime endDate, IList<int> companiesIds, IList<ParameterizedIndicator> indicators);
    }
}