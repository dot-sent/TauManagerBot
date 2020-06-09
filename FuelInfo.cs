using System;

namespace TauManagerBot
{
        public class FuelInfo
        {
            public double Last_Price { get; set; }
            public double Max_Price { get; set; }
            public double Min_Price { get; set; }
            public DateTime Last_Reading { get; set; }
            public string Station_Name { get; set; }
            public string Station_Short_Name { get; set; }
            public string System_Name { get; set; }
        }
}