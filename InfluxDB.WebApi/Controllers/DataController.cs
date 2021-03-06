using InfluxDB.WebApi.Model;
using InfluxDB.WebApi.Util;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
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
        private readonly ILogger<DataController> _logger;
        public DataController(InfluxDBUtil influxDBUtil, ILogger<DataController> logger)
        {
            _logger = logger;
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
                    //tagDic.Add("EquipId", item.Equid.ToString());
                    //tagDic.Add("StandarParamId", item.Standarparamid.ToString());
                    tagDic.Add("Id", item.Id.ToString());
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
                    _logger.LogError(ex, "InsertRealData");
                    Console.WriteLine("错误：" + ex.Message);
                }
            }
            try
            {
                _influxDBUtil.WriteDataPoints("RealData", tableName, dicList);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "InsertRealData");
                Console.WriteLine("错误：" + ex.Message);
            }
            return await Task.FromResult("ok");
        }
        /// <summary>
        /// 实时数据查询
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<List<QueryRealDataItemModel>> QueryRealData(QueryRealDataModel input)
        {
            //var result = await _influxDBUtil.QueryData<QueryRealDataItemModel>(input.StartTime, input.EndTime, "RealData");
            var queryDic = new Dictionary<string, string> { };
            if (input.EquipId > 0)
                queryDic.Add("EquipId", input.EquipId.ToString());
            if (input.StandarParamId > 0)
                queryDic.Add("StandarParamId", input.StandarParamId.ToString());
            var result = await _influxDBUtil.QueryData(input.StartTime, input.EndTime, "RealData", queryDic);
            return new List<QueryRealDataItemModel> { };
        }
        [HttpPost]
        public async Task<List<Dictionary<string, object>>> QueryRealDataList(QueryRealDataModel input)
        {
            //var result = await _influxDBUtil.QueryData<QueryRealDataItemModel>(input.StartTime, input.EndTime, "RealData");
            var queryDic = new Dictionary<string, string> { };
            if (input.EquipId > 0)
                queryDic.Add("EquipId", input.EquipId.ToString());
            if (input.StandarParamId > 0)
                queryDic.Add("StandarParamId", input.StandarParamId.ToString());
            var result = await _influxDBUtil.QueryData(input.StartTime, input.EndTime, "RealData", queryDic);
            return result;
        }
        [HttpPost]
        public async Task<ApiResult<List<RealDataItemModel>>> QueryRealDataItem(QueryRealDataModel input)
        {
            var result = new ApiResult<List<RealDataItemModel>> { Code = 200, Data = new List<RealDataItemModel> { } };
            try
            {
                var queryDic = new Dictionary<string, string> { };
                //if (input.EquipId > 0)
                //    queryDic.Add("EquipId", input.EquipId.ToString());
                //if (input.StandarParamId > 0)
                //    queryDic.Add("StandarParamId", input.StandarParamId.ToString());
                queryDic.Add("Id", input.Id.ToString());
                var resultDic = await _influxDBUtil.QueryData(input.StartTime, input.EndTime, "RealData", queryDic);
                foreach (var _resultDic in resultDic)
                {
                    string _value = _resultDic["RealValue"] != null ? _resultDic["RealValue"].ToString() : string.Empty;
                    result.Data.Add(new RealDataItemModel
                    {
                        Time = DateTime.Parse(_resultDic["_time"].ToString()).ToString("yyyy-MM-dd HH:mm:ss"),
                        Value = _value
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "QueryRealDataItem");
            }
            return result;
        }
        [HttpPost]
        public async Task<List<TestModel>> QueryData(QueryDataModel input)
        {
            var result = await _influxDBUtil.QueryData<TestModel>(input.StartTime, input.EndTime, input.Bucket);
            return result;
        }
        [HttpPost]
        public async Task<List<TestModel1>> QueryMeasurementData(QueryDataModel input)
        {
            var result = await _influxDBUtil.QueryMeasurementData<TestModel1>(input.StartTime, input.EndTime, input.Bucket);
            return result;
        }
    }
}
