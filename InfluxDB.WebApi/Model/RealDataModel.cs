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
        public string Uid { get; set; }
        public long? Projectid { get; set; }
        public string Descname { get; set; }
        public string Tagname { get; set; }
        public string Aitype { get; set; }
        public string Unit { get; set; }
        public string Realvalue { get; set; }
        public float? Hihi { get; set; }
        public float? Hi { get; set; }
        public float? Lolo { get; set; }
        public float? Lo { get; set; }
        public short? Isalarm { get; set; }
        public short? Isvoice { get; set; }
        public string Valuetype { get; set; }
        public int? Startbyte { get; set; }
        public int? Datalength { get; set; }
        public string Zeromean { get; set; }
        public string Onemean { get; set; }
        public int? Valuesort { get; set; }
        public long? Equid { get; set; }
        public int? Controllsort { get; set; }
        public int? Transarray { get; set; }
        public string Datafmtdesc { get; set; }
        public long? Plcid { get; set; }
        public string Setname { get; set; }
        public short? Ismodified { get; set; }
        public long? Standarparamid { get; set; }
        public DateTime Updatetime { get; set; }
        public string Paramean { get; set; }
    }
    public class QueryRealDataModel
    {
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
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
}