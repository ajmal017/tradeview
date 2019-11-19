﻿using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace DevelopmentInProgress.Wpf.Common.Model
{
    public class Preferences : EntityBase
    {
        public Preferences()
        {
            FavouriteSymbols = new ObservableCollection<string>();
        }

        public bool ShowFavourites { get; set; }
        public string SelectedSymbol { get; set; }
        public int TradeLimit { get; set; }
        public int TradesChartDisplayCount { get; set; }
        public int TradesDisplayCount { get; set; }
        public bool UseAggregateTrades { get; set; }
        public int OrderBookLimit { get; set; }
        public int OrderBookChartDisplayCount { get; set; }
        public int OrderBookDisplayCount { get; set; }
        public ObservableCollection<string> FavouriteSymbols { get; set; }
    }
}