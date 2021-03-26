using InfluxDB.Client;
using InfluxDB.Client.Api.Domain;
using InfluxDB.Client.Writes;
using InfluxDB.WebApi.Model;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NodaTime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace InfluxDB.WebApi.Util
{
    public class InfluxDBUtil
    {
        private readonly ILogger<InfluxDBUtil> _logger;
        private readonly InfluxDBClient client;
        private readonly WriteApi writeApi;
        private readonly IConfiguration _configuration;

        // You can generate a Token from the "Tokens Tab" in the UI
        private readonly string token;
        //private const string bucket = "zrh";
        private readonly string org;
        private readonly IHttpClientFactory _clientFactory;

        public InfluxDBUtil(IHttpClientFactory clientFactory, IConfiguration configuration, ILogger<InfluxDBUtil> logger)
        {
            _configuration = configuration;
            token = _configuration["InfluxDB:Token"];
            org = _configuration["InfluxDB:Org"];

            client = InfluxDBClientFactory.Create(_configuration["InfluxDB:Url"], token.ToCharArray());
            writeApi = client.GetWriteApi();
            _clientFactory = clientFactory;
            _logger = logger;
        }
        public void WriteData(string data, string bucket)
        {
            //const string data = "mem,host=host1 used_percent=23.43234543";
            //using (var writeApi = client.GetWriteApi())
            {
                writeApi.WriteRecord(bucket, org, WritePrecision.Ns, data);
            }
        }
        public void WriteDataPoint(string bucket, string table, Dictionary<string, string> tagDic, Dictionary<string, string> fieldDic, DateTime timestamp)
        {
            //var point = PointData
            //  .Measurement(table)
            //  .Tag("equipId", "host1")
            //  .Tag("", "")
            //  .Field("used_percent", 23.43234543)
            //  .Timestamp(DateTime.UtcNow, WritePrecision.Ns);

            var point = PointData.Measurement(table);
            foreach (var tag in tagDic)
            {
                point = point.Tag(tag.Key, tag.Value);
            }
            foreach (var field in fieldDic)
            {
                point = point.Field(field.Key, field.Value);
            }
            point = point.Timestamp(timestamp, WritePrecision.S);

            //using (var writeApi = client.GetWriteApi())
            {
                writeApi.WritePoint(bucket, org, point);
            }
        }
        public void WriteDataPoints(string bucket, string table, List<DataPointModel> dicList)
        {
            List<PointData> points = new List<PointData> { };
            foreach (var dic in dicList)
            {
                var point = PointData.Measurement(table);
                foreach (var tag in dic.TagDic)
                {
                    point = point.Tag(tag.Key, tag.Value);
                }
                foreach (var field in dic.FieldDic)
                {
                    point = point.Field(field.Key, field.Value);
                }
                point = point.Timestamp(dic.Timestamp, WritePrecision.S);
                points.Add(point);
            }


            //using (var writeApi = client.GetWriteApi())
            {
                writeApi.WritePoints(bucket, org, points.ToArray());
            }
        }
        public async Task<List<T>> QueryData<T>(DateTime startTime, DateTime endTime, string bucket) where T : class, new()
        {

            var startTimeUtc = startTime.ToUniversalTime();
            var endTimeUtc = endTime.ToUniversalTime();
            //var query = $"from(bucket: \"" + bucket + "\") |> range(start: -1h)";
            //var query = $"from(bucket: \"" + bucket + "\") |> range(start: -10h)";

            //var query = $"from(bucket: \"" + bucket + "\") |> range(start: 2018-11-05T23:30:00Z, stop: 2018-11-06T00:00:00Z)";
            var query = $"from(bucket: \"" + bucket + "\") |> range(start: " + startTimeUtc.ToString("yyyy-MM-ddTHH:mm:ssZ") + ", stop: " + endTimeUtc.ToString("yyyy-MM-ddTHH:mm:ssZ") + ")";
            //var tables = await client.GetQueryApi().QueryAsync(query, org);
            var tables = await client.GetQueryApi().QueryAsync(query, org);

            var items = new List<T> { };
            PropertyInfo[] properties = typeof(T).GetProperties();

            foreach (var table in tables)
            {
                var records = table.Records;
                foreach (var record in records)
                {
                    //Console.WriteLine($"{record.GetTime()}: {record.GetValue()}");
                    var t = new T();
                    foreach (var valueDic in record.Values)
                    {
                        var property = properties.FirstOrDefault(t => t.Name.Equals(valueDic.Key, StringComparison.OrdinalIgnoreCase));
                        if (property != null)
                        {
                            if (property.PropertyType.Name.Equals("Int64") && valueDic.Value != null)
                            {
                                long lValue = 0;
                                long.TryParse(valueDic.Value.ToString(), out lValue);
                                property.SetValue(t, lValue, null);
                            }
                            else
                            {
                                property.SetValue(t, valueDic.Value, null);
                            }
                        }
                    }
                    items.Add(t);
                }
            }
            return items;
        }
        public async Task<List<Dictionary<string, object>>> QueryData(DateTime startTime, DateTime endTime, string bucket, Dictionary<string, string> filterFieldDic)
        {
            var result = new List<Dictionary<string, object>> { };
            try
            {
                var startTimeUtc = startTime.ToUniversalTime();
                var endTimeUtc = endTime.ToUniversalTime();
                //var query = $"from(bucket: \"" + bucket + "\") |> range(start: -1h)";
                //var query = $"from(bucket: \"" + bucket + "\") |> range(start: -10h)";

                //var query = $"from(bucket: \"" + bucket + "\") |> range(start: 2018-11-05T23:30:00Z, stop: 2018-11-06T00:00:00Z)";
                //var query = $"from(bucket: \"" + bucket + "\") |> range(start: " + startTimeUtc.ToString("yyyy-MM-ddTHH:mm:ssZ") + ", stop: " + endTimeUtc.ToString("yyyy-MM-ddTHH:mm:ssZ") + ")";
                StringBuilder sb = new StringBuilder();
                sb.Append($"from(bucket: \"").Append(bucket).Append("\") |> range(start: ").Append(startTimeUtc.ToString("yyyy-MM-ddTHH:mm:ssZ")).Append(", stop: ").Append(endTimeUtc.ToString("yyyy-MM-ddTHH:mm:ssZ")).Append(")");
                if (filterFieldDic != null && filterFieldDic.Count() > 0)
                {
                    sb.Append(" |> filter(fn:(r)=>");
                    int i = 0;
                    foreach (var _filterFieldDic in filterFieldDic)
                    {
                        sb.Append("r.").Append(_filterFieldDic.Key).Append("==\"").Append(_filterFieldDic.Value).Append("\"");
                        if (i < filterFieldDic.Count() - 1)
                        {
                            sb.Append(" and ");
                        }
                        i++;
                    }
                    sb.Append(")");
                }
                sb.Append(" |> sort(columns: [\"uptime\"], desc: false)");

                //var tables = await client.GetQueryApi().QueryAsync(query, org);
                var query = sb.ToString();
                var tables = await client.GetQueryApi().QueryAsync(query, org);

                //var str = await client.GetQueryApi().QueryRawAsync(query, org);


                foreach (var table in tables)
                {
                    var records = table.Records;
                    //var item = new Dictionary<string, object> { };
                    foreach (var record in records)
                    {
                        //Console.WriteLine($"{record.GetMeasurement()}:{record.GetStart()}:{record.GetStop()}:{record.GetField()}:{record.GetTime()}: {record.GetValue()}");
                        //Console.WriteLine($"{record.Values}");
                        //item.Add(record.GetField(), record.GetValue());
                        var item = new Dictionary<string, object> { };
                        if (record.Values.ContainsKey("_start") && record.Values["_start"] != null)
                        {
                            var _start = ((Instant)record.Values["_start"]).ToDateTimeUtc().ToLocalTime();
                            item.Add("_start", _start.ToString("yyyy-MM-dd HH:mm:ss"));
                        }
                        if (record.Values.ContainsKey("_stop") && record.Values["_stop"] != null)
                        {
                            var _stop = ((Instant)record.Values["_stop"]).ToDateTimeUtc().ToLocalTime();
                            item.Add("_stop", _stop.ToString("yyyy-MM-dd HH:mm:ss"));
                        }
                        if (record.Values.ContainsKey("_time") && record.Values["_time"] != null)
                        {
                            var _time = ((Instant)record.Values["_time"]).ToDateTimeUtc().ToLocalTime();
                            item.Add("_time", _time.ToString("yyyy-MM-dd HH:mm:ss"));
                        }
                        item.Add(record.Values["_field"].ToString(), record.Values["_value"]);
                        item.Add("_measurement", record.Values["_measurement"]);
                        foreach (var value in record.Values)
                        {
                            if (!Metadata.RecordKey.Contains(value.Key))
                            {
                                item.Add(value.Key, value.Value);
                            }
                        }
                        result.Add(item);
                    }
                    //result.Add(new ResultItem
                    //{
                    //    Item = item
                    //});
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "QueryData");
            }
            
            return result;
        }

        public async Task<List<TestModel1>> QueryMeasurementData<T>(DateTime startTime, DateTime endTime, string bucket) where T : class, new()
        {

            var startTimeUtc = startTime.ToUniversalTime();
            var endTimeUtc = endTime.ToUniversalTime();
            //var query = $"from(bucket: \"" + bucket + "\") |> range(start: -1h)";
            //var query = $"from(bucket: \"" + bucket + "\") |> range(start: -10h)";

            //var query = $"from(bucket: \"" + bucket + "\") |> range(start: 2018-11-05T23:30:00Z, stop: 2018-11-06T00:00:00Z)";
            var query = $"from(bucket: \"" + bucket + "\") |> range(start: " + startTimeUtc.ToString("yyyy-MM-ddTHH:mm:ssZ") + ", stop: " + endTimeUtc.ToString("yyyy-MM-ddTHH:mm:ssZ") + ")";

            var items = new List<TestModel1> { };
            await client.GetQueryApi().QueryAsync<TestModel1>(query, org, (cancellable, temperature) =>
              {
                  //
                  // The callback to consume a FluxRecord mapped to POCO.
                  //
                  // cancelable - object has the cancel method to stop asynchronous query
                  //
                  Console.WriteLine($"{temperature.host}: {temperature.testField1} at {temperature.Time}");
                  items.Add(new TestModel1
                  {
                      host= temperature.host,
                      testField1 = temperature.testField1,
                      testField2 = temperature.testField2,
                      Time = temperature.Time,
                  });
              });
            return items;
        }
    }
}
