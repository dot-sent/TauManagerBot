using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Nito.AsyncEx.Synchronous;
using TauManager.Core.Utils;

namespace TauManagerBot
{
    public class SotherynPriceTrackerService : ISotherynPriceTrackerService
    {
        private IServiceProvider _serviceProvider;

        private class TrackingSettings
        {
            public string StationCode { get; set; }
            public string TrackingItemSlug { get; set; }
            public decimal FuelPriceCoefficient { get; set; }
            public bool UseHighPrice { get; set; } // If false, use the lower price
            public string SystemCode
            {
                get
                {
                    return StationCode.Split("/")[0];
                }
            }
        }

        // This map might rather belong to the database, but I'm placing it into the code
        // for the initial version of the functionality.
        private readonly List<TrackingSettings> _trackingMap = new List<TrackingSettings>{
            new TrackingSettings{
                StationCode = "Sol/Tau",
                TrackingItemSlug = "fots-line-combat-axe",
                FuelPriceCoefficient = 11.6748M,
                UseHighPrice = false
            },
            new TrackingSettings{
                StationCode = "YZ/NYC",
                TrackingItemSlug = "fots-line-combat-axe",
                FuelPriceCoefficient = 11.1177M,
                UseHighPrice = true
            },
            new TrackingSettings{
                StationCode = "Sol/KBN",
                TrackingItemSlug = "beer-bottles",
                FuelPriceCoefficient = 0.0023M,
                UseHighPrice = true
            },
            new TrackingSettings{
                StationCode = "Sol/DAE",
                TrackingItemSlug = "rudimentary-baton",
                FuelPriceCoefficient = 0.9884M,
                UseHighPrice = true
            },
            new TrackingSettings{
                StationCode = "Sol/TNG",
                TrackingItemSlug = "aged-stun-baton",
                FuelPriceCoefficient = 0.2587M,
                UseHighPrice = true
            },
            new TrackingSettings{
                StationCode = "Sol/NL",
                TrackingItemSlug = "worn-repulsion-armor",
                FuelPriceCoefficient = 0.9547M,
                UseHighPrice = true
            },
            new TrackingSettings{
                StationCode = "AC/MOI",
                TrackingItemSlug = "standard-agility-stim-v22002",
                FuelPriceCoefficient = 1.3014M,
                UseHighPrice = false
            },
            new TrackingSettings{
                StationCode = "AC/PS",
                TrackingItemSlug = "light-liquid-armor-suit",
                FuelPriceCoefficient = 1.746626M,
                UseHighPrice = true
            },
            new TrackingSettings{
                StationCode = "AC/GoM",
                TrackingItemSlug = "ent-smg",
                FuelPriceCoefficient = 2.9841M,
                UseHighPrice = true
            },
            new TrackingSettings{
                StationCode = "AC/CC",
                TrackingItemSlug = "lightly-padded-reflective-suit",
                FuelPriceCoefficient = 1.6613M,
                UseHighPrice = true
            },
            new TrackingSettings{
                StationCode = "AC/SoB",
                TrackingItemSlug = "arc-dancers-dress",
                FuelPriceCoefficient = 1.8266M,
                UseHighPrice = true
            },
            new TrackingSettings{
                StationCode = "AC/BDX",
                TrackingItemSlug = "light-thermoplastic-suit",
                FuelPriceCoefficient = 0.9574M,
                UseHighPrice = true
            },
            new TrackingSettings{
                StationCode = "AC/YoG",
                TrackingItemSlug = "arc-absorption-cloak",
                FuelPriceCoefficient = 1.9574M,
                UseHighPrice = true
            },
            new TrackingSettings{
                StationCode = "BS/CSH",
                TrackingItemSlug = "ration-5",
                FuelPriceCoefficient = 4.8M,
                UseHighPrice = true
            },
            new TrackingSettings{
                StationCode = "BS/HKL",
                TrackingItemSlug = "heavy-diffusion-armor",
                FuelPriceCoefficient = 3.1813M,
                UseHighPrice = true
            },
            new TrackingSettings{
                StationCode = "BS/AMZ",
                TrackingItemSlug = "minor-multi-stim-v3-1-019",
                FuelPriceCoefficient = 1.3413M,
                UseHighPrice = false
            },
            new TrackingSettings{
                StationCode = "BS/MoO",
                TrackingItemSlug = "elite-anti-energy-combat-suit",
                FuelPriceCoefficient = 7.2532M,
                UseHighPrice = true
            },
            new TrackingSettings{
                StationCode = "L726/JG",
                TrackingItemSlug = "arc-masters-suit",
                FuelPriceCoefficient = 5.0453M,
                UseHighPrice = true
            },
            new TrackingSettings{
                StationCode = "L726/OSH",
                TrackingItemSlug = "dielectric-paladin-armor",
                FuelPriceCoefficient = 4.7679M,
                UseHighPrice = true
            },
            new TrackingSettings{
                StationCode = "L726/SoT",
                TrackingItemSlug = "standard-multi-stim-v3-2-019",
                FuelPriceCoefficient = 1.3520M,
                UseHighPrice = true
            },
            new TrackingSettings{
                StationCode = "YZ/ASI",
                TrackingItemSlug = "enhanced-arc-dancers-suit",
                FuelPriceCoefficient = 4.7359M,
                UseHighPrice = true
            },
            new TrackingSettings{
                StationCode = "YZ/CVS",
                TrackingItemSlug = "repulsion-armor",
                FuelPriceCoefficient = 4.5227M,
                UseHighPrice = true
            },
        };

        // Not all of the `FuelInfo` fields are used in this class, but I prefer not to
        // duplicate the classes that have essentially the same functionality.
        private Dictionary<string, FuelInfo> _fuelInfo;
        private Dictionary<string, FuelInfo> _trackingItemPrices;

        public SotherynPriceTrackerService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            _fuelInfo = new Dictionary<string, FuelInfo>();
            _trackingItemPrices = new Dictionary<string, FuelInfo>();
        }

        private async Task RefreshItemPricesFor(string stationOrSystemName)
        {
            var stationsToProcess = _trackingMap.Where(ts => ts.StationCode == stationOrSystemName || ts.SystemCode == stationOrSystemName);
            using (var scope = _serviceProvider.CreateScope())
            {
                var tauStationClient = scope.ServiceProvider.GetRequiredService<ITauStationClient>();
                foreach(var stationTracking in stationsToProcess)
                {
                    if (!_fuelInfo.ContainsKey(stationTracking.StationCode) ||
                        DateTime.Now - _fuelInfo[stationTracking.StationCode].Last_Reading > TimeSpan.FromMinutes(10))
                    {
                        var priceData = await tauStationClient.GetItemPriceRange(stationTracking.TrackingItemSlug);
                        var price = (stationTracking.UseHighPrice ? priceData.Value : priceData.Key) /
                            stationTracking.FuelPriceCoefficient;
                        _fuelInfo[stationTracking.StationCode] = new FuelInfo{
                            Last_Price = price,
                            Last_Reading = DateTime.Now,
                            Station_Short_Name = stationTracking.StationCode,
                            System_Name = stationTracking.SystemCode
                        };
                        _trackingItemPrices[stationTracking.StationCode] = new FuelInfo{
                            Last_Price = stationTracking.UseHighPrice ? priceData.Value : priceData.Key,
                            Last_Reading = DateTime.Now,
                            Station_Short_Name = stationTracking.StationCode,
                            System_Name = stationTracking.SystemCode
                        };
                    }
                }
            }
        }

        public IDictionary<string, decimal> GetFuelPrices(string stationOrSystemName)
        {
            var result = RefreshItemPricesFor(stationOrSystemName);
            result.WaitAndUnwrapException();
            return _fuelInfo == null ? null:
                _fuelInfo.Values.Where(fi => fi.Station_Short_Name == stationOrSystemName ||
                    fi.System_Name == stationOrSystemName ||
                    fi.Station_Short_Name.StartsWith(stationOrSystemName))
                .ToDictionary(
                    fi => fi.Station_Short_Name,
                    fi => fi.Last_Price
                );
        }

        public IDictionary<string, string> GetFuelPricesDebug(string stationOrSystemName)
        {
            var result = RefreshItemPricesFor(stationOrSystemName);
            result.WaitAndUnwrapException();
            return _trackingItemPrices == null ? null:
                _trackingItemPrices.Values.Where(fi => fi.Station_Short_Name == stationOrSystemName ||
                    fi.System_Name == stationOrSystemName ||
                    fi.Station_Short_Name.StartsWith(stationOrSystemName))
                .Select(
                    fi => new {
                        FuelInfo = fi,
                        TrackingInfo = _trackingMap.SingleOrDefault(el => el.StationCode == fi.Station_Short_Name)
                    }
                )
                .ToDictionary(
                    fi => fi.FuelInfo.Station_Short_Name,
                    fi => string.Format("Basic item with slug `{0}` had price {1,7:F2} at {2}; calculation: {1,7:F2} * {3,7:F2} = {4,7:F2}",
                        fi.TrackingInfo == null ? "<undefined>" : fi.TrackingInfo.TrackingItemSlug,
                        fi.FuelInfo.Last_Price,
                        fi.FuelInfo.Last_Reading,
                        fi.TrackingInfo == null ? 0 : fi.TrackingInfo.FuelPriceCoefficient,
                        fi.FuelInfo.Last_Price * fi.TrackingInfo.FuelPriceCoefficient
                    )
                );
        }
    }
}