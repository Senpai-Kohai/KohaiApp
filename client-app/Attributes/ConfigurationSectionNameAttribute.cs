using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace client_app.Attributes
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public sealed class ConfigurationSectionNameAttribute(string sectionName) : Attribute
    {
        public string SectionName { get; set; } = sectionName ?? throw new ArgumentNullException(nameof(sectionName));
    }
}
