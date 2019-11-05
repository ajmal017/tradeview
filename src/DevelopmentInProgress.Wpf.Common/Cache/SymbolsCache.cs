﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DevelopmentInProgress.Wpf.Common.Model;
using DevelopmentInProgress.Wpf.Common.Services;
using DevelopmentInProgress.MarketView.Interface.Extensions;
using DevelopmentInProgress.MarketView.Interface.Enums;

namespace DevelopmentInProgress.Wpf.Common.Cache
{
    public class SymbolsCache : ISymbolsCache, IDisposable
    {
        private Exchange exchange;
        private IWpfExchangeService wpfExchangeService;
        private List<Symbol> symbols;
        private Symbol btcUsdt;
        private CancellationTokenSource subscribeSymbolsCxlTokenSrc = new CancellationTokenSource();
        private object lockSubscriptions = new object();
        private bool disposed;

        public SymbolsCache(Exchange exchange, IWpfExchangeService wpfExchangeService)
        {
            this.exchange = exchange;
            this.wpfExchangeService = wpfExchangeService;

            symbols = new List<Symbol>();
        }

        public event EventHandler<Exception> OnSymbolsCacheException;

        public async Task<List<Symbol>> GetSymbols()
        {
            if (!symbols.Any())
            {
                var results = await wpfExchangeService.GetSymbols24HourStatisticsAsync(exchange, subscribeSymbolsCxlTokenSrc.Token);

                lock (lockSubscriptions)
                {
                    if (!symbols.Any())
                    {
                        symbols.AddRange(results);

                        btcUsdt = symbols.Single(s => s.Name.Equals("BTCUSDT"));

                        wpfExchangeService.SubscribeStatistics(exchange, symbols, SubscribeStatisticsException, subscribeSymbolsCxlTokenSrc.Token);
                    }
                }
            }

            return symbols;
        }

        public void ValueAccount(Account account)
        {
            if (btcUsdt == null
                || !symbols.Any())
            {
                return;
            }

            decimal usdt = 0m;
            decimal btc = 0m;

            foreach (var balance in account.Balances)
            {
                var qty = balance.Free + balance.Locked;

                if (qty <= 0)
                {
                    continue;
                }

                if (balance.Asset.Equals("BTC"))
                {
                    btc += qty;
                }
                else
                {
                    var symbol = symbols.FirstOrDefault(s => s.Name.Equals($"{balance.Asset}BTC"));
                    if (symbol != null)
                    {
                        btc += symbol.SymbolStatistics.LastPrice * qty;
                    }
                }
            }

            usdt = btcUsdt.SymbolStatistics.LastPrice * btc;

            account.BTCValue = Math.Round(btc, 8);
            account.USDTValue = usdt.Trim(btcUsdt.PricePrecision);
        }

        private void SubscribeStatisticsException(Exception exception)
        {
            var onSubscribeSymbolsException = OnSymbolsCacheException;
            onSubscribeSymbolsException?.Invoke(this, exception);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (!subscribeSymbolsCxlTokenSrc.IsCancellationRequested)
                {
                    subscribeSymbolsCxlTokenSrc.Cancel();
                }

                disposed = false;
            }
        }

    }
}
