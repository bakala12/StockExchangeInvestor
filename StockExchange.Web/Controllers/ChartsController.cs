﻿using StockExchange.Business.Indicators.Common;
using StockExchange.Business.Models.Company;
using StockExchange.Business.Models.Indicators;
using StockExchange.Business.Models.Price;
using StockExchange.Business.ServiceInterfaces;
using StockExchange.Common.Extensions;
using StockExchange.Web.Helpers;
using StockExchange.Web.Helpers.Json;
using StockExchange.Web.Models.Charts;
using StockExchange.Web.Models.Indicator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace StockExchange.Web.Controllers
{
    public class ChartsController : BaseController
    {
        private readonly IPriceService _priceService;
        private readonly IIndicatorsService _indicatorsService;
        private readonly ICompanyService _companyService;

        public ChartsController(IPriceService priceService, IIndicatorsService indicatorsService, ICompanyService companyService)
        {
            _priceService = priceService;
            _indicatorsService = indicatorsService;
            _companyService = companyService;
        }

        public ActionResult Index()
        {
            //TODO: load companies list from the view via AJAX
            var companies = _companyService.GetAllCompanies();
            var model = BuildChartIndexModel(companies);
            return View(model);
        }

        [HttpGet]
        public ActionResult GetLineChartData(IList<int> companyIds)
        {
            var companyPrices = _priceService.GetPricesForCompanies(companyIds);
            var model = companyPrices.Select(cp => new LineChartModel
            {
                CompanyId = cp.Company.Id,
                Name = cp.Company.Code,
                Data = cp.Prices.Select(p => new[] { p.Date.ToJavaScriptTimeStamp(), p.ClosePrice }).ToList()
            });
            return new JsonNetResult(model);
        }

        [HttpGet]
        public ActionResult GetCandlestickChartData(IList<int> companyIds)
        {
            var companyPrices = _priceService.GetPricesForCompanies(companyIds);
            var model = BuildCandlestickChartModel(companyPrices);
            return new JsonNetResult(model);
        }

        [HttpGet]
        public ActionResult GetIndicatorValues(IList<int> companyIds, IndicatorType type, IList<IndicatorProperty> properties)
        {
            var values = _indicatorsService.GetIndicatorValues(type, companyIds);
            var model = BuildIndicatorChartModel(values);
            return new JsonNetResult(model);
        }

        private static ChartsIndexModel BuildChartIndexModel(IList<CompanyDto> companies)
        {
            return new ChartsIndexModel
            {
                Companies = companies,
                Indicators = Enum.GetValues(typeof(IndicatorType)).Cast<IndicatorType>()
                    .Select(i => new IndicatorViewModel
                    {
                        Name = i.GetEnumDescription(),
                        Type = i,
                    }).ToList()
            };
        }

        private static IEnumerable<LineChartModel> BuildIndicatorChartModel(IList<CompanyIndicatorValues> values)
        {
            return values.Select(cv => new LineChartModel
            {
                CompanyId = cv.Company.Id,
                Name = cv.Company.Code,
                Data = ConvertIndicatorValuesToData(cv.IndicatorValues)
            });
        }

        private static IList<decimal[]> ConvertIndicatorValuesToData(IList<IndicatorValue> values)
        {
            //TODO: needs refactoring, the inheritance of IndicatorValue is a bit troublesome
            if (values.FirstOrDefault() is DoubleLineIndicatorValue)
            {
                return values.Cast<DoubleLineIndicatorValue>().Select(v => new[]
                {
                    v.Date.ToJavaScriptTimeStamp(),
                    v.Value,
                    v.SecondLineValue
                }).ToList();
            }
            return values.Select(v => new[]
            {
                v.Date.ToJavaScriptTimeStamp(),
                v.Value,
            }).ToList();
        }

        private static IEnumerable<LineChartModel> BuildCandlestickChartModel(IList<CompanyPricesDto> companyPrices)
        {
            return companyPrices.Select(cp => new LineChartModel
            {
                CompanyId = cp.Company.Id,
                Name = cp.Company.Code,
                Data = cp.Prices.Select(p => new[]
                {
                    p.Date.ToJavaScriptTimeStamp(),
                    p.OpenPrice,
                    p.HighPrice,
                    p.LowPrice,
                    p.ClosePrice
                }).ToList()
            });
        }
    }
}