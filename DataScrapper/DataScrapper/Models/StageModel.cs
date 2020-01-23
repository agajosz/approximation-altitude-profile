namespace DataScrapper.Models
{
    public class StageModel
    {
        public int Stage { get; set; }
        public string Type { get; set; }
        public string Origin { get; set; }
        public double OLat { get; set; }
        public double OLon { get; set; }
        public string Destination { get; set; }
        public double DLat { get; set; }
        public double DLon { get; set; }
        public int Km { get; set; }
    }
}
