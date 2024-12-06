using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using Kohai;
using Kohai.Configuration;
using Kohai.Attributes;

namespace Kohai.Services
{
    public abstract class ServiceBase<TConfig> : IService<TConfig>
        where TConfig : class, new()
    {
        protected CancellationTokenSource shutdownTokenSource;

        private TConfig? _serviceConfiguration = null;
        public TConfig ServiceConfiguration
        {
            get
            {
                var sectionName = GetType().GetCustomAttribute<ServiceConfigurationAttribute>(false)?.SectionName;
                _serviceConfiguration ??= IConfiguration.LoadConfiguration<TConfig>(sectionName);

                return _serviceConfiguration;
            }

            private set
            {
                _serviceConfiguration = value;
            }
        }

        public virtual bool ServiceRunning => shutdownTokenSource.IsCancellationRequested;

        protected ServiceBase(CancellationTokenSource shutdownTokenSource)
        {
            this.shutdownTokenSource = shutdownTokenSource;
            this.shutdownTokenSource.Token.Register(async () => await ServiceStopped());
        }

        public virtual async Task ServiceStarted() { await Task.CompletedTask; }
        public virtual async Task ServiceStopped() { await Task.CompletedTask; }
    }
}
