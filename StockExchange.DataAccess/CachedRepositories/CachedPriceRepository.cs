﻿using StockExchange.DataAccess.Cache;
using StockExchange.DataAccess.IRepositories;
using StockExchange.DataAccess.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StockExchange.DataAccess.CachedRepositories
{
    public class CachedPriceRepository : CachedRepositoryBase<Price>, IPriceRepository
    {
        private readonly IPriceRepository _baseRepository;
        private readonly ICompanyRepository _companyRepository;

        public CachedPriceRepository(IPriceRepository baseRepository, ICache cache, ICompanyRepository companyRepository) 
            : base(baseRepository, cache)
        {
            _baseRepository = baseRepository;
            _companyRepository = companyRepository;
        }

        public async Task<IList<Price>> GetCurrentPrices(int days) => 
            await _baseRepository.GetCurrentPrices(days);

        public async Task<IList<Price>> GetCurrentPrices(IList<int> companyIds)
        {
            // not pretty, but seems to work well in this case
            string cacheKey = CacheKeys.CurrentPrices + "_" + string.Join(",", companyIds.OrderBy(x => x));
            return await Get(cacheKey, async () => await _baseRepository.GetCurrentPrices(companyIds));
        }
    }
}