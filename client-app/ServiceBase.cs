using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace client_app
{
    public abstract class ServiceBase<TConfig> : IService<TConfig>
        where TConfig : class, new()
    {
        private TConfig? _serviceConfiguration = null;
        public TConfig ServiceConfiguration
        {
            get
            {
                var sectionName = GetType().GetCustomAttribute<SectionNameAttribute>(false)?.SectionName;

                if (_serviceConfiguration == null)
                    _serviceConfiguration = ConfigurationUtils.LoadConfiguration<TConfig>(sectionName);

                return _serviceConfiguration;
            }

            private set
            {
                _serviceConfiguration = value;
            }
        }
    }
}
