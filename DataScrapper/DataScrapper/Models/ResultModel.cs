using System.Collections.Generic;

namespace DataScrapper.Models
{
    public class ResultModel
    {
        public double Elevation { get; set; }
        public IEnumerable<LocationModel> Locations { get; set; }
        public double Resolution { get; set; }
    }
}
