﻿using StockExchange.Business.Indicators;
using StockExchange.Business.Models;
using StockExchange.Business.Models.Indicators;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using StockExchange.Business.ServiceInterfaces;

namespace StockExchange.Business.Services
{
    public class IndicatorsService : IIndicatorsService
    {
        private readonly IIndicatorFactory _indicatorFactory;
        private readonly IPriceService _priceService;

        public IndicatorsService(IIndicatorFactory indicatorFactory, IPriceService priceService)
        {
            _indicatorFactory = indicatorFactory;
            _priceService = priceService;
        }

        public IList<IndicatorType> GetAvailableIndicators()
        {
            List<IndicatorType> list = new List<IndicatorType>();
            foreach (IndicatorType type in typeof(IndicatorType).GetEnumValues())
                list.Add(type);
            return list;
        }

        public IList<IndicatorProperty> GetPropertiesForIndicator(IndicatorType type)
        {
            var indicator = _indicatorFactory.CreateIndicator(type);
            var properties = indicator.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.Name != nameof(IIndicator.Type));

            return properties.Select(property => new IndicatorProperty
            {
                Name = property.Name,
                Value = (int)property.GetValue(indicator)
            }).ToList();
        }

        public IList<CompanyIndicatorValues> GetIndicatorValues(IIndicator indicator, IList<int> companyIds)
        {
            IList<CompanyPricesDto> companyPrices = _priceService.GetPricesForCompanies(companyIds);
            return ComputeIndicatorValues(indicator, companyPrices);
        }

        public IList<CompanyIndicatorValues> GetIndicatorValues(IndicatorType type, IList<int> companyIds)
        {
            var indicator = _indicatorFactory.CreateIndicator(type);
            return GetIndicatorValues(indicator, companyIds);
        }

        private static IList<CompanyIndicatorValues> ComputeIndicatorValues(IIndicator indicator, IList<CompanyPricesDto> companyPrices)
        {
            var result = new List<CompanyIndicatorValues>();
            foreach (var company in companyPrices)
            {
                var values = indicator.Calculate(company.Prices);
                result.Add(new CompanyIndicatorValues
                {
                    Company = company.Company,
                    IndicatorValues = values
                });
            }
            return result;
        }
    }
}
