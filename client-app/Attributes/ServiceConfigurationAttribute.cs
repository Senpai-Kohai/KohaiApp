using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace client_app.Attributes
{
    [AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = false)]
    public sealed class ServiceConfigurationAttribute(string? sectionName = null) : Attribute
    {
        public string? SectionName { get; set; } = sectionName;
    }
}
