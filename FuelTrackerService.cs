using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Nito.AsyncEx.Synchronous;

namespace TauManagerBot
{
    public class FuelTrackerService : IFuelTrackerService
    {
        private DateTime _lastUpdated;
        private string _fuelInfoUrl;

        private List<FuelInfo> _fuelInfo;

        private class FuelModel
        {
            public FuelInfo[] Stations { get; set; }
        }

        public FuelTrackerService(IConfiguration configuration)
        {
            _fuelInfoUrl =  configuration.GetValue<string>("FuelInfoUrl");
        }

        private async Task RefreshFuelPrices()
        {
            if (DateTime.Now - _lastUpdated < TimeSpan.FromMinutes(10)) return;
            var client = new HttpClient();
            client.DefaultRequestHeaders.Accept.TryParseAdd("application/json");
            var result = await client.GetAsync(_fuelInfoUrl);
            if (result.IsSuccessStatusCode) 
            {
                var content = await result.Content.ReadAsStringAsync();
                _fuelInfo = JsonConvert.DeserializeObject<FuelModel>(content).Stations.ToList();
                _lastUpdated = DateTime.Now;
            }
        }

        public IEnumerable<FuelInfo> GetPrices(string stationOrSystemName)
        {
            var result = RefreshFuelPrices();
            result.WaitAndUnwrapException();
            return _fuelInfo == null ? null : _fuelInfo.Where(fi => fi.Station_Short_Name == stationOrSystemName || 
                String.Equals(fi.System_Name, stationOrSystemName, StringComparison.CurrentCultureIgnoreCase) ||
                String.Equals(fi.Station_Name, stationOrSystemName, StringComparison.CurrentCultureIgnoreCase) ||
                fi.Station_Short_Name.ToLower().StartsWith(stationOrSystemName.ToLower()) ||
                fi.Station_Short_Name.ToLower().EndsWith(stationOrSystemName.ToLower()));
        }

        public IEnumerable<string> GetValidSystemNames()
        {
            var result = RefreshFuelPrices();
            result.WaitAndUnwrapException();
            return _fuelInfo.Select(fi => fi.System_Name).Distinct().OrderBy(x => x).ToList();
        }

        public IEnumerable<string> GetValidSystemShortcuts()
        {
            var result = RefreshFuelPrices();
            result.WaitAndUnwrapException();
            return _fuelInfo.Select(fi => fi.Station_Short_Name.Substring(0, fi.Station_Short_Name.IndexOf('/'))).Distinct().OrderBy(x => x).ToList();
        }
    }
}