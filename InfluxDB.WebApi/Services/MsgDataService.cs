using InfluxDB.WebApi.Model;
using InfluxDB.WebApi.Util;
using Msg.Core.MQ;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace InfluxDB.WebApi.Services
{
    public class MsgDataService : IMsgDataService
    {
        private readonly HashCache _hashCache;
        private readonly List<long> _consumDic;
        private readonly IServiceProvider _serviceProvider;
        public MsgDataService(HashCache hashCache, InfluxDBUtil influxDBUtil, IServiceProvider serviceProvider)
        {
            _hashCache = hashCache;
            _consumDic = new List<long> { };
            _serviceProvider = serviceProvider;
        }
        public async Task ConsumRealData()
        {
            var hashId = typeof(ProjectInfo).Name;
            while (true)
            {
                var projects = _hashCache.GetValue<ProjectInfo>(hashId);
                projects = projects.Where(t => t.State == 1).ToList();
                foreach (var project in projects)
                {
                    try
                    {
                        if (!_consumDic.Contains(project.Id))
                        {
                            _consumDic.Add(project.Id);
                            var insertRealDataService = _serviceProvider.GetService(typeof(IInsertRealDataService)) as IInsertRealDataService;
                            insertRealDataService.ConsumProjectRealData(project.Id);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex);
                    }
                }
                await Task.Delay(300000);//5分钟
            }
        }
    }
}
