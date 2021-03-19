using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace InfluxDB.WebApi.Model
{
    public class DataPointModel
    {
        public Dictionary<string, string> TagDic { get; set; }
        public Dictionary<string, string> FieldDic { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
