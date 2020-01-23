using CsvHelper;
using DataScrapper.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Net.Http;

namespace DataScrapper
{
    public class Program
    {
        private static void Main(string[] args)
        {
            var nice = new LocationModel(43.7009358, 7.2683912);
            var sisteron = new LocationModel(44.1993162, 5.8925705);

            var leTeil = new LocationModel(44.5493789, 4.6829629);
            var montAigoual = new LocationModel(44.1209581, 3.5815802);

            var laTourDuPin = new LocationModel(45.566620, 5.442405);
            var villardDeLans = new LocationModel(45.070537, 5.551536);

            var routes = new List<(LocationModel origin, LocationModel destination)>
            {
                (nice, sisteron),
                (leTeil, montAigoual),
                (laTourDuPin, villardDeLans)

            };

            const int samples = 500;
            const string key = "AIzaSyBTENy5Ve8693m6fzJXVmkHZODCenzgRsw";
            const string endpoint = "elevation/json?path=";
            using var client = new HttpClient
            {
                BaseAddress = new Uri("https://maps.googleapis.com/maps/api/")
            };

            foreach (var (origin, destination) in routes)
            {
                var response = client
                    .GetAsync( endpoint 
                               + origin.Lat.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture) + ","
                               + origin.Lng.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture) + "|"
                               + destination.Lat.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture) + ","
                               + destination.Lng.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture)
                               + "&samples=" + samples + "&key=" + key)
                    .Result;
                response.EnsureSuccessStatusCode();
                var result = response.Content.ReadAsStringAsync().Result;
                JsonToCsv(result, ";");
            }


        }

        public static void JsonToCsv(string jsonContent, string delimiter)
        {
            var csvString = new StreamWriter("results.csv");
            using var csv = new CsvWriter(csvString);
            csv.Configuration.Delimiter = delimiter;

            using var dt = JsonStringToTable(jsonContent);
            foreach (DataColumn column in dt.Columns)
            {
                csv.WriteField(column.ColumnName);
            }
            csv.NextRecord();

            foreach (DataRow row in dt.Rows)
            {
                for (var i = 0; i < dt.Columns.Count; i++)
                {
                    csv.WriteField(row[i]);
                }
                csv.NextRecord();
            }
        }

        public static DataTable JsonStringToTable(string jsonContent)
        {
            var dt = JsonConvert.DeserializeAnonymousType(jsonContent, new { Makes = default(DataTable) }).Makes;
            //var dt = JsonConvert.DeserializeObject<DataTable>(jsonContent);
            return dt;
        }
    }
}
