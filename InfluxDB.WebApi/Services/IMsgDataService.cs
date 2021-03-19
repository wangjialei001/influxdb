using InfluxDB.WebApi.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace InfluxDB.WebApi.Services
{
    public interface IMsgDataService
    {
        Task ConsumRealData();
    }
}
