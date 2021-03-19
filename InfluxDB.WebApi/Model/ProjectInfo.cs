using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace InfluxDB.WebApi.Model
{
    public class ProjectInfo
    {
        public long Id { get; set; }
        public string Projectname { get; set; }
        public string Shortname { get; set; }
        public long? Ownerid { get; set; }
        public int? ProvoiceCode { get; set; }
        public int? CityCode { get; set; }
        public int? Position { get; set; }
        public string Address { get; set; }
        public int? SummerTem { get; set; }
        public int? WinterTem { get; set; }
        public string Longitude { get; set; }
        public string Latitude { get; set; }
        public float? Constructionarea { get; set; }
        public float? Ratedload { get; set; }
        public string Projectlogo { get; set; }
        public string Projectcode { get; set; }
        public string Reamrk { get; set; }
        public short? State { get; set; }
    }
}
