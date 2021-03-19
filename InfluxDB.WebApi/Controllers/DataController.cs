using InfluxDB.WebApi.Model;
using InfluxDB.WebApi.Util;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace InfluxDB.WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class DataController : ControllerBase
    {
        private readonly InfluxDBUtil _influxDBUtil;
        public DataController(InfluxDBUtil influxDBUtil)
        {
            _influxDBUtil = influxDBUtil;
        }
        /// <summary>
        /// 新增实时值
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<string> InsertRealData([FromBody] InsertRealDataModel input)
        {
            if (input.ProjectId < 0)
            {
                Console.WriteLine("错误：" + "bad request");
                return "bad request";
            }
            string tableName = "RealData" + input.ProjectId;
            List<DataPointModel> dicList = new List<DataPointModel> { };
            foreach (var item in input.Items)
            {
                try
                {
                    Dictionary<string, string> tagDic = new Dictionary<string, string> { };
                    tagDic.Add("EquipId", item.Equid.ToString());
                    tagDic.Add("StandarParamId", item.Standarparamid.ToString());
                    Dictionary<string, string> fieldDic = new Dictionary<string, string> { };
                    fieldDic.Add("RealValue", item.Realvalue);

                    DateTime utcNow = item.Updatetime.ToUniversalTime();// TimeUtil
                    //double utc = TimeUtil.ConvertDateTimeInt(utcNow);
                    //DateTime dTime = TimeUtil.ConvertIntDatetime(utc);
                    //_influxDBUtil.WriteDataPoint("RealData", tableName, tagDic, fieldDic, utcNow);
                    dicList.Add(new DataPointModel
                    {
                        TagDic = tagDic,
                        FieldDic = fieldDic,
                        Timestamp = utcNow
                    });
                }
                catch (Exception ex)
                {
                    Console.WriteLine("错误：" + ex.Message);
                }
            }
            try
            {
                _influxDBUtil.WriteDataPoints("RealData", tableName, dicList);
            }
            catch (Exception ex)
            {
                Console.WriteLine("错误：" + ex.Message);
            }
            return await Task.FromResult("ok");
        }

        [HttpPost]
        public async Task<List<QueryRealDataItemModel>> QueryRealData(QueryRealDataModel input)
        {
            var startTimeUtc = input.StartTime.ToUniversalTime();
            var endTimeUtc = input.EndTime.ToUniversalTime();
            var result = await _influxDBUtil.QueryData<QueryRealDataItemModel>(startTimeUtc, endTimeUtc, "RealData");
            return result;
        }
        [HttpPost]
        public async Task<List<TestModel>> QueryData(QueryDataModel input)
        {
            var startTimeUtc = input.StartTime.ToUniversalTime();
            var endTimeUtc = input.EndTime.ToUniversalTime();
            var result = await _influxDBUtil.QueryData<TestModel>(startTimeUtc, endTimeUtc, input.Bucket);
            return result;
        }
        [HttpPost]
        public async Task<List<TestModel1>> QueryMeasurementData(QueryDataModel input)
        {
            var startTimeUtc = input.StartTime.ToUniversalTime();
            var endTimeUtc = input.EndTime.ToUniversalTime();
            var result = await _influxDBUtil.QueryMeasurementData<TestModel1>(startTimeUtc, endTimeUtc, input.Bucket);
            return result;
        }
    }
}
