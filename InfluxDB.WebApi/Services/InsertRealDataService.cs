using InfluxDB.WebApi.Model;
using InfluxDB.WebApi.Util;
using Msg.Core.MQ;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace InfluxDB.WebApi.Services
{
    public class InsertRealDataService: IInsertRealDataService
    {
        private readonly InfluxDBUtil _influxDBUtil;
        public InsertRealDataService(InfluxDBUtil influxDBUtil)
        {
            _influxDBUtil = influxDBUtil;
        }
        /// <summary>
        /// 消费数据
        /// </summary>
        /// <param name="projectId"></param>
        /// <returns></returns>
        public async Task ConsumProjectRealData(long projectId)
        {
            try
            {
                Thread thread = new Thread(() =>
                {
                    MQUtil.Consume1("InsertRealData" + projectId, async t =>
                    {
                        try
                        {
                            if (string.IsNullOrEmpty(t))
                            {
                                return;
                            }
                            var input = JsonConvert.DeserializeObject<InsertRealDataModel>(t);
                            var r = await InsertRealData(input);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex);
                        }
                    });
                });
                thread.IsBackground = true;
                thread.Start();

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private async Task<bool> InsertRealData(InsertRealDataModel input)
        {
            bool result = true;
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
                result = false;
            }
            return result;
        }
    }
}
