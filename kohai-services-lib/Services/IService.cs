using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Kohai;
using Kohai.Configuration;

namespace Kohai.Services
{
    public interface IService
    {
        bool ServiceRunning { get; }
    }

    public interface IService<TConfig> : IService
        where TConfig : class, new()
    {
        TConfig ServiceConfiguration { get; }
    }
}
