using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Nito.AsyncEx.Synchronous;
using TauManager.Core.Utils;

namespace TauManagerBot
{
    public class RationInfoService : IRationInfoService
    {
        private DateTime _lastUpdated;
        private decimal _lastPrice;
        private IServiceProvider _serviceProvider;

        public RationInfoService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        private async Task RefreshRationPrice()
        {
            if (DateTime.Now - _lastUpdated < TimeSpan.FromMinutes(10)) return;
            using (var scope = _serviceProvider.CreateScope())
            {
                var tauStationService = scope.ServiceProvider.GetRequiredService<ITauStationClient>();
                var prices = await tauStationService.GetItemPriceRange("ration-1");
                _lastUpdated = DateTime.Now;
                if (prices.Key > 0) {
                    _lastPrice = prices.Key;
                }
            }
        }

        public decimal GetCurrentPricePerTier()
        {
            var result = RefreshRationPrice();
            result.WaitAndUnwrapException();
            return _lastPrice;
        }
    }
}