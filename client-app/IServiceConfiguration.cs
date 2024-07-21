// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace client_app
{
    public interface IServiceConfiguration
    {


        public static T LoadConfiguration<T>(string sectionName) where T : new()
        {
            var configInstance = new T();

            // Create switch mappings for environment variables
            var switchMappings = ConfigurationUtils.CreateSwitchMappings<T>(sectionName);

            // Build section-specific configuration
            var sectionConfiguration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables()
                .AddEnvironmentVariables(prefix: $"{sectionName}__")
                .AddCommandLine(Environment.GetCommandLineArgs(), switchMappings)
                .Build();

            // Bind the combined configuration to the instance
            sectionConfiguration.GetSection(sectionName).Bind(configInstance);

            return configInstance;

        }
    }
