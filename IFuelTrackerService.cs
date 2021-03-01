using System.Collections.Generic;

namespace TauManagerBot
{
    public interface IFuelTrackerService
    {
        IEnumerable<FuelInfo> GetPrices(string stationOrSystemName);
        IEnumerable<string> GetValidSystemNames();
        IEnumerable<string> GetValidSystemShortcuts();
    }
}