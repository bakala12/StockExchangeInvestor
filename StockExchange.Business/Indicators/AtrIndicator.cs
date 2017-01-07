﻿using StockExchange.Business.Indicators.Common;
using StockExchange.Business.Models.Indicators;
using StockExchange.DataAccess.Models;
using System;
using System.Collections.Generic;

namespace StockExchange.Business.Indicators
{
    /// <summary>
    /// Average True Range technical indicator
    /// </summary>
    public class AtrIndicator : IIndicator
    {
        /// <summary>
        /// Default <see cref="Term"/> value for the ATR indicator
        /// </summary>
        public const int DefaultAtrTerm = 14;

        /// <summary>
        /// The number of prices from previous days to include when computing 
        /// an indicator value
        /// </summary>
        public int Term { get; set; } = DefaultAtrTerm;

        /// <inheritdoc />
        public IndicatorType Type => IndicatorType.Atr;

        /// <inheritdoc />
        public IList<IndicatorValue> Calculate(IList<Price> prices)
        {
            var rsValues = GetRsValues(prices);
            return MovingAverageHelper.SmoothedMovingAverage(rsValues, Term);
        }

        /// <inheritdoc />
        public IList<Signal> GenerateSignals(IList<Price> prices)
        {
            var signals = new List<Signal>();
            var values = Calculate(prices);
            var trend = MovingAverageHelper.ExpotentialMovingAverage(prices, Term);
            SignalAction lastAction = SignalAction.NoSignal;
            for (int i = Term; i < prices.Count-1; i++)
            {
                if (values[i - Term].Value < values[i - Term + 1].Value && trend[i - Term].Value < trend[i - Term + 1].Value)
                {
                    if (lastAction != SignalAction.Buy)
                    {
                        signals.Add(new Signal(SignalAction.Buy) {Date = values[i-Term+1].Date});
                        lastAction = SignalAction.Buy;
                    }
                }
                if (values[i - Term].Value <= values[i - Term + 1].Value ||
                    trend[i - Term].Value <= trend[i - Term + 1].Value) continue;
                if (lastAction == SignalAction.Sell) continue;
                signals.Add(new Signal(SignalAction.Sell) { Date = values[i - Term + 1].Date });
                lastAction = SignalAction.Sell;
            }
            return signals;
        }

        private static List<IndicatorValue> GetRsValues(IList<Price> prices)
        {
            var rsValues = new List<IndicatorValue>
            {
                new IndicatorValue
                {
                    Date = prices[0].Date,
                    Value = prices[0].HighPrice - prices[0].LowPrice
                }
            };
            for (var i = 1; i < prices.Count; i++)
            {
                var rs = Math.Max(
                    prices[i].HighPrice - prices[i].LowPrice, Math.Max(
                        Math.Abs(prices[i].HighPrice - prices[i - 1].ClosePrice),
                        Math.Abs(prices[i].LowPrice - prices[i - 1].ClosePrice)));
                rsValues.Add(new IndicatorValue
                {
                    Date = prices[i].Date,
                    Value = rs
                });
            }
            return rsValues;
        }
    }
}
