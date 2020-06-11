using System;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Nito.AsyncEx.Synchronous;

namespace TauManagerBot
{
    public class RationInfoService : IRationInfoService
    {
        private DateTime _lastUpdated;
        private decimal _lastPrice;
        private readonly Regex _priceRegex = new Regex("\"currency\">([0-9.]+)");

        private async Task RefreshRationPrice()
        {
            if (DateTime.Now - _lastUpdated < TimeSpan.FromMinutes(10)) return;
            var client = new HttpClient();
            var result = await client.GetAsync("https://alpha.taustation.space/item/ration-1");
            if (result.IsSuccessStatusCode)
            {
                var content = await result.Content.ReadAsStringAsync();
                // Obligatory link: https://stackoverflow.com/a/1732454/625594
                var isMatch = _priceRegex.IsMatch(content);
                if (isMatch)
                {
                    var match = _priceRegex.Match(content);
                    var parseResult = decimal.TryParse(match.Groups[1].Value, out _lastPrice);
                    if (parseResult) _lastUpdated = DateTime.Now;
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