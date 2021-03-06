﻿using log4net;
using StockExchange.Common;
using StockExchange.DataAccess.IRepositories;
using StockExchange.DataAccess.Models;
using StockExchange.Task.Business.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StockExchange.Task.Business
{
    /// <summary>
    /// Synchronizes stock data
    /// </summary>
    public sealed class DataSynchronizer : IDataSynchronizer
    {
        private static readonly ILog Logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private readonly IRepository<Company> _companyRepository;
        private readonly IFactory<IRepository<Price>> _priceRepositoryFactory;

        /// <summary>
        /// Creates a new instance of <see cref="DataSynchronizer"/>
        /// </summary>
        /// <param name="companyRepository"></param>
        /// <param name="priceRepositoryFactory"></param>
        public DataSynchronizer(IRepository<Company> companyRepository, IFactory<IRepository<Price>> priceRepositoryFactory)
        {
            _companyRepository = companyRepository;
            _priceRepositoryFactory = priceRepositoryFactory;
        }

        /// <inheritdoc />
        public void Sync(DateTime startDate, DateTime endDate, IEnumerable<string> companyCodes = null)
        {
            Logger.Debug("Syncing historical data started");
            var startDateString = startDate.ToString(Consts.Formats.DateFormat);
            var endDateString = endDate.ToString(Consts.Formats.DateFormat);
            IList<Company> companies = companyCodes == null 
                ? _companyRepository.GetQueryable().ToList() 
                : _companyRepository.GetQueryable().Where(item => companyCodes.ToList().Contains(item.Code)).ToList();
            IList<Price> prices = _priceRepositoryFactory.CreateInstance().GetQueryable().ToList();
            Parallel.ForEach(companies, company => ThreadSync(startDateString, endDateString, company, prices));
            Logger.Debug("Syncing historical data ended.");
        }

        private void ThreadSync(string startDateString, string endDateString, Company company, IList<Price> prices)
        {
            var url = CreatePathUrl(startDateString, endDateString, company.Code);
            try
            {
                SyncByCompany(url, company, prices);
            }
            catch (Exception ex)
            {
                Logger.Error("Error, " + company.Name + ", " + ex.Message);
            }
        }

        private void SyncByCompany(string url, Company company, IList<Price> prices)
        {
            var data = CsvImporter.GetCsv(url);
            data.RemoveAt(0);
            if (!data.Any())
            {
                Logger.Warn($"No data available for company {company.Code}");
            }
            using (var priceRepository = _priceRepositoryFactory.CreateInstance())
            {
                var inserted = false;
                // ReSharper disable once LoopCanBePartlyConvertedToQuery
                foreach (var row in data)
                {
                    var currentDate = DateTime.Parse(row[0]);
                    // ReSharper disable once InvertIf
                    if (!prices.Any(item => item.CompanyId == company.Id && item.Date == currentDate))
                    {
                        priceRepository.Insert(PriceConverter.Convert(row, company));
                        inserted = true;
                    }
                }
                if (inserted)
                {
                    priceRepository.Save();
                }
            }
        }

        private static string CreatePathUrl(string startDateString, string endDateString, string companyCode)
        {
            return "http://stooq.pl/q/d/l/?s=" + companyCode + "&d1=" + startDateString + "&d2=" + endDateString + "&i=d";
        }
    }
}
