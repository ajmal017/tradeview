﻿using DevelopmentInProgress.Common.Extensions;
using DevelopmentInProgress.Wpf.Common.Model;
using DevelopmentInProgress.Wpf.Common.Services;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;
using System;
using System.Linq;
using DevelopmentInProgress.Wpf.Controls.Messaging;
using DevelopmentInProgress.Wpf.Common.ViewModel;
using Prism.Logging;

namespace DevelopmentInProgress.Wpf.Configuration.ViewModel
{
    public class SymbolsViewModel : ExchangeViewModel
    {
        private UserAccount userAccount;
        private List<Symbol> symbols;
        private bool showFavourites;
        private bool isLoadingSymbols;
        private bool disposed;

        public SymbolsViewModel(IWpfExchangeService exchangeService, UserAccount userAccount, ILoggerFacade logger)
            : base(exchangeService, logger)
        {
            this.userAccount = userAccount;

            GetSymbols().FireAndForget();
        }

        public string Exchange
        {
            get { return userAccount.Exchange.ToString(); }
        }

        public List<Symbol> Symbols
        {
            get { return symbols; }
            set
            {
                if (symbols != value)
                {
                    symbols = value;
                    OnPropertyChanged("Symbols");
                }
            }
        }

        public bool ShowFavourites
        {
            get { return showFavourites; }
            set
            {
                if (showFavourites != value)
                {
                    showFavourites = value;
                    if (showFavourites)
                    {
                        Symbols.ForEach(s => s.IsVisible = s.IsFavourite);
                    }
                    else
                    {
                        Symbols.ForEach(s => s.IsVisible = true);
                    }

                    OnPropertyChanged("ShowFavourites");
                }
            }
        }

        public bool IsLoadingSymbols
        {
            get { return isLoadingSymbols; }
            set
            {
                if (isLoadingSymbols != value)
                {
                    isLoadingSymbols = value;
                    OnPropertyChanged("IsLoadingSymbols");
                }
            }
        }

        public override void Dispose(bool disposing)
        {
            if (disposed)
            {
                return;
            }

            if (disposing)
            {
                // dispose stuff...
            }

            disposed = true;
        }

        private async Task GetSymbols()
        {
            IsLoadingSymbols = true;

            try
            {
                var results = await ExchangeService.GetSymbolsAsync(userAccount.Exchange, new CancellationToken());

                Func<Symbol, string, Symbol> f = ((s, p) =>
                {
                    s.IsFavourite = true;
                    return s;
                });

                (from s in results join p in userAccount.Preferences.FavouriteSymbols on s.Name equals p select f(s, p)).ToList();

                Symbols = new List<Symbol>(results);
            }
            catch (Exception ex)
            {
                Dialog.ShowException(ex);
            }

            IsLoadingSymbols = false;
        }
    }
}
