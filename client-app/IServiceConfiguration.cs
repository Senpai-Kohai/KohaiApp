using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace client_app
{
    public interface IService<TConfig> where TConfig : new()
    {
        TConfig ServiceConfiguration { get; }
    }
}
