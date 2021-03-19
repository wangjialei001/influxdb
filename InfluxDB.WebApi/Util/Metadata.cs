using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace InfluxDB.WebApi.Util
{
    public class Metadata
    {
        public static readonly List<string> RecordKey = new List<string> { "result","table","_start","_stop","_time","_value","_field", "_measurement" };
    }
}
