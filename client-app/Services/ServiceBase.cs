using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using client_app.Attributes;

namespace client_app.Services
{
    public abstract class ServiceBase<TConfig> : IService<TConfig>
        where TConfig : class, new()
    {
        private TConfig? _serviceConfiguration = null;
        public TConfig ServiceConfiguration
        {
            get
            {
                var sectionName = GetType().GetCustomAttribute<ConfigurationSectionNameAttribute>(false)?.SectionName;

                if (_serviceConfiguration == null)
                    _serviceConfiguration = ConfigurationUtils.LoadConfiguration<TConfig>(sectionName);

                return _serviceConfiguration;
            }

            private set
            {
                _serviceConfiguration = value;
            }
        }

        public abstract bool ServiceRunning { get; }
    }
}
