﻿using StockExchange.Business.Models.Indicators;
using StockExchange.DataAccess.Models;
using System.Collections.Generic;
using System.Linq;
using StockExchange.Business.Indicators.Common;

namespace StockExchange.Business.Indicators
{
    /// <summary>
    /// Wskaźnik Macd
    /// </summary>
    public class MacdIndicator : IIndicator
    {
        public const int DefaultLongTerm = 26;
        public const int DefaultShortTerm = 12;
        public const int DefaultSignalTerm = 9;

        public int LongTerm { get; set; } = DefaultLongTerm;

        public int ShortTerm { get; set; } = DefaultShortTerm;

        public int SignalTerm { get; set; } = DefaultSignalTerm;

        public IndicatorType Type => IndicatorType.Macd;

        public IList<IndicatorValue> Calculate(IList<Price> prices)
        {
            var longEma = MovingAverageHelper.ExpotentialMovingAverage(prices, LongTerm);
            var shortEma = MovingAverageHelper.ExpotentialMovingAverage(prices, ShortTerm);
            var macdLine = SubstractLongEmaFromShortEma(shortEma, longEma);
            var signalLine = MovingAverageHelper.ExpotentialMovingAverage(macdLine, SignalTerm);
            return PrepareResult(macdLine, signalLine);
        }

        private IList<IndicatorValue> SubstractLongEmaFromShortEma(IList<IndicatorValue> shortEma, IList<IndicatorValue> longEma)
        {
            var difference = LongTerm - ShortTerm;
            IList<IndicatorValue> values = new List<IndicatorValue>();
            for (var i = difference; i < shortEma.Count; i++)
            {
                var val = new IndicatorValue
                {
                    Date = shortEma[i].Date,
                    Value = shortEma[i].Value - longEma[i - difference].Value
                };
                values.Add(val);
            }
            return values;
        }

        private static IList<IndicatorValue> PrepareResult(IList<IndicatorValue> macdLine, IList<IndicatorValue> signalLine)
        {
            IList<IndicatorValue> resultList = new List<IndicatorValue>();
            var difference = macdLine.Count - signalLine.Count;
            for (var i = difference; i < macdLine.Count; i++)
            {
                resultList.Add(new DoubleLineIndicatorValue
                {
                    Date = macdLine[i].Date,
                    Value = macdLine[i].Value,
                    SecondLineValue = signalLine[i - difference].Value
                });
            }
            return resultList;
        }

        public IList<Signal> GenerateSignals(IList<Price> prices)
        {
            var doubleLineValues = Calculate(prices).Cast<DoubleLineIndicatorValue>().ToList();
            return IntersectionHelper.FindIntersections(doubleLineValues).
                Select(i => new Signal(Convert(i.IntersectionType)) { Date = i.Date }).ToList();
            //Note that if we generate this signal with date one day back results are far much better, but this is a deception
        }

        private static SignalAction Convert(IntersectionType intersectionType)
        {
            // ReSharper disable once SwitchStatementMissingSomeCases
            switch (intersectionType)
            {
                case IntersectionType.FirstAbove:
                    return SignalAction.Buy;
                case IntersectionType.SecondAbove:
                    return SignalAction.Sell;
                default:
                    return SignalAction.NoSignal;
            }
        }
    }
}
