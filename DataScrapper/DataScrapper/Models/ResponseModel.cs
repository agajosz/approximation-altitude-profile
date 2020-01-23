using System.Collections.Generic;

namespace DataScrapper.Models
{
    public class ResponseModel
    {
        public IEnumerable<ResultModel> Results { get; set; }
    }
}
