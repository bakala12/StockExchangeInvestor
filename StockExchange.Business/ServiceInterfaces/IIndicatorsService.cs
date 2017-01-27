﻿using StockExchange.Business.Indicators.Common;
using StockExchange.Business.Models.Filters;
using StockExchange.Business.Models.Indicators;
using StockExchange.Business.Models.Paging;
using StockExchange.DataAccess.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StockExchange.Business.ServiceInterfaces
{
    /// <summary>
    /// Provides operations on technical indicators
    /// </summary>
    public interface IIndicatorsService
    {
        /// <summary>
        /// Returns list of properties for the given indicator
        /// </summary>
        /// <param name="type">The indicator type</param>
        /// <returns>The list of indicator properties</returns>
        IList<IndicatorProperty> GetPropertiesForIndicator(IndicatorType type);

        /// <summary>
        /// Computes the values from the given indicator
        /// </summary>
        /// <param name="indicator">The indicator</param>
        /// <param name="companyIds">The companies which values should be computed</param>
        /// <returns>The computed indicator values</returns>
        Task<IList<CompanyIndicatorValues>> GetIndicatorValues(IIndicator indicator, IList<int> companyIds);

        /// <summary>
        /// Computes the values from the given indicator type
        /// </summary>
        /// <param name="type">The indicator type</param>
        /// <param name="companyIds">The companies which values should be computed</param>
        /// <param name="properties">The indicator properties</param>
        /// <returns>The computed indicator values</returns>
        Task<IList<CompanyIndicatorValues>> GetIndicatorValues(IndicatorType type, IList<int> companyIds, IList<IndicatorProperty> properties);

        /// <summary>
        /// Returns all indicator types available in the system
        /// </summary>
        /// <returns></returns>
        IList<IndicatorType> GetAllIndicatorTypes();

        /// <summary>
        /// Returns all indicators available in the system
        /// </summary>
        /// <returns>The list of indicators</returns>
        IList<IndicatorDto> GetAllIndicators();

        /// <summary>
        /// Returns an indicators type from a name
        /// </summary>
        /// <param name="indicatorName">The indicator name</param>
        /// <returns>The indicator type</returns>
        IndicatorType? GetTypeFromName(string indicatorName);

        /// <summary>
        /// Converts the <see cref="StrategyIndicator"/> entities to <see cref="ParameterizedIndicator"/> DTOs
        /// </summary>
        /// <param name="indicators">The indicators to convert</param>
        /// <returns>The converted indicators</returns>
        IList<ParameterizedIndicator> ConvertIndicators(IEnumerable<StrategyIndicator> indicators);

        /// <summary>
        /// Gets descriptions for indicator.
        /// </summary>
        /// <param name="indicatorType">Type of indicator.</param>
        /// <returns>Object containig indicator descriptions.</returns>
        IndicatorDescriptionDto GetIndicatorDescription(IndicatorType indicatorType);

        /// <summary>
        /// Returns list of indicator types available for strategy view.
        /// </summary>
        /// <returns>List of indicator types available for strategy view</returns>
        IList<IndicatorType> GetIndicatorTypesAvailableForStrategies();

        /// <summary>
        /// Returns list of indicators available for strategy view.
        /// </summary>
        /// <returns>List of indicators available for strategy view</returns>
        IList<IndicatorDto> GetIndicatorsAvailableForStrategies();

        /// <summary>
        /// Returns signals generated by the indicators
        /// </summary>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <param name="companiesIds"></param>
        /// <param name="indicators"></param>
        /// <param name="isAnd"></param>
        /// <param name="daysLimitToAnd"></param>
        /// <returns></returns>
        Task<IList<SignalEvent>> GetSignals(DateTime startDate, DateTime endDate, IList<int> companiesIds, IList<ParameterizedIndicator> indicators, bool isAnd = false, int daysLimitToAnd = 0);

        /// <summary>
        /// Returns paginated signals list
        /// </summary>
        /// <param name="message">The pagination and filtering definition</param>
        /// <returns>The paginated list of generated signals</returns>
        Task<PagedList<TodaySignal>> GetCurrentSignals(PagedFilterDefinition<TransactionFilter> message);

        /// <summary>
        /// Returns the total number of signals generated on the current day
        /// </summary>
        /// <returns>The total number of signals generated on the current day</returns>
        Task<int> GetCurrentSignalsCount();
    }
}