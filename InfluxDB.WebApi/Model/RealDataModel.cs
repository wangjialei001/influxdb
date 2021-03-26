using InfluxDB.Client.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace InfluxDB.WebApi.Model
{
    public class InsertRealDataModel
    {
        public long ProjectId { get; set; }
        public List<RealDataModel> Items { get; set; }
    }
    public class RealDataModel
    {
        public long Id { get; set; }
        public string Realvalue { get; set; }
        public long? Equid { get; set; }
        public long? Standarparamid { get; set; }
        public DateTime Updatetime { get; set; }
    }
    public class QueryRealDataModel
    {
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public long EquipId { get; set; }
        public long Id { get; set; }
        public long StandarParamId { get; set; }
    }
    public class QueryRealDataItemModel
    {
        public long EquipId { get; set; }
        public long StandarParamId { get; set; }
        public string RealValue { get; set; }
        public DateTime Time { get; set; }
    }
    public class QueryDataModel
    {
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string Bucket { get; set; }
    }
    public class TestModel
    {
        public string host { get; set; }
        public string testField1 { get; set; }
        public string testField2 { get; set; }
        public DateTime Time { get; set; }
    }
    [Measurement("testMeasurement")]
    public class TestModel1
    {
        [Column("host", IsTag = true)] public string host { get; set; }
        [Column("measurement", IsTag = true)] public string testMeasurement { get; set; }

        [Column("value")] public string testField1 { get; set; }
        [Column("value")] public string testField2 { get; set; }

        [Column(IsTimestamp = true)] public DateTime Time { get; set; }
    }
    [Measurement("temperature")]
    public class Temperature
    {
        [Column("location", IsTag = true)] public string Location { get; set; }

        [Column("value")] public double Value { get; set; }

        [Column(IsTimestamp = true)] public DateTime Time { get; set; }
    }

    public class RealDataItemModel
    {
        public string Time { get; set; }
        public string Value { get; set; }
    }
}