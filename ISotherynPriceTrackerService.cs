using System.Collections.Generic;

namespace TauManagerBot
{
    public interface ISotherynPriceTrackerService
    {
        IDictionary<string, decimal> GetFuelPrices(string stationOrSystemName);
    }
}