using System;

namespace TauManagerBot
{
        public class FuelInfo
        {
            public decimal Last_Price { get; set; }
            public decimal Max_Price { get; set; }
            public decimal Min_Price { get; set; }
            public DateTime Last_Reading { get; set; }
            public string Station_Name { get; set; }
            public string Station_Short_Name { get; set; }
            public string System_Name { get; set; }
        }
}