using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace InfluxDB.WebApi.Services
{
    public interface IInsertRealDataService
    {
        Task ConsumProjectRealData(long projectId);
    }
}
