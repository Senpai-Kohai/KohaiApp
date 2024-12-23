﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Kohai;
using Kohai.Attributes;
using Microsoft.Extensions.Configuration;

namespace Kohai.Configuration
{
    public interface IConfiguration
    {
        /// <summary>
        /// Create a "switch name" from a "property name"-
        /// makes lowercase, and replaces all underscores
        /// with dashes. Switches will always be returned
        /// with "--" at the start.
        /// </summary>
        /// <param name="prefix"></param>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        public static string CreateSwitchFromName(string? prefix, string propertyName)
        {
            propertyName = propertyName.ToLower();
            propertyName = propertyName.Replace('_', '-');

            if (!string.IsNullOrWhiteSpace(prefix))
            {
                if (!prefix.EndsWith("__"))
                    prefix += "__";

                propertyName = prefix + propertyName;
            }

            propertyName = "--" + propertyName;
            return propertyName;
        }

        /// <summary>
        /// Returns the set of switch mappings for a given configuration class, T.
        /// </summary>
        /// <typeparam name="T">CLR configuration type, to reflect properties on.</typeparam>
        /// <param name="prefix">An optional common prefix for mappings, usually ending in __ by convention.</param>
        /// <returns></returns>
        public static IDictionary<string, string> CreateSwitchMappings<T>(string? prefix)
        {
            var keyMappings = new Dictionary<string, string>();
            foreach (var propertyInfo in typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                keyMappings[CreateSwitchFromName(prefix, propertyInfo.Name)] = propertyInfo.Name;
            }

            return keyMappings;
        }


        /// <summary>
        /// </summary>
        /// <param name="prefix">An optional common prefix for mappings, usually ending in __ by convention.</param>
        /// <returns></returns>
        public static IDictionary<string, string> CreateSwitchMappings(Type type, string? prefix)
        {
            var keyMappings = new Dictionary<string, string>();
            foreach (var propertyInfo in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                keyMappings[CreateSwitchFromName(prefix, propertyInfo.Name)] = propertyInfo.Name;
            }

            return keyMappings;
        }

        /// <summary>
        /// Builds the given configuration from all available data sources-
        /// ENVs, run switches / args, appsettings.json, etc.
        /// 
        /// The standard format is that an appsettings "sections" is given
        /// as {section_name}__variable in ENVs, and similarly (replacing
        /// underscored with dashes) for run switches/args.
        /// </summary>
        /// <typeparam name="T">CLR type for returned configuration object.</typeparam>
        /// <param name="sectionName">Optional configuration section name.</param>
        /// <returns></returns>
        public static T LoadConfiguration<T>(string? sectionName = null) where T : new()
        {
            var configInstance = new T();

            // Create switch mappings for environment variables
            var switchMappings = CreateSwitchMappings<T>(sectionName);

            // Build section-specific configuration
            var sectionConfiguration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables()
                .AddEnvironmentVariables(prefix: sectionName)
                .AddCommandLine(Environment.GetCommandLineArgs(), null)
                .AddCommandLine(Environment.GetCommandLineArgs(), switchMappings)
                .Build();

            // Bind the combined configuration to the instance
            sectionConfiguration.Bind(configInstance);

            // Bind the section too, if provided
            if (!string.IsNullOrWhiteSpace(sectionName))
                sectionConfiguration.GetSection(sectionName).Bind(configInstance);

            return configInstance;
        }

        /// <summary>
        /// Builds the given configuration from all available data sources-
        /// ENVs, run switches / args, appsettings.json, etc.
        /// 
        /// The standard format is that an appsettings "sections" is given
        /// as {section_name}__variable in ENVs, and similarly (replacing
        /// underscored with dashes) for run switches/args.
        /// </summary>
        /// <typeparam name="T">CLR type for returned configuration object.</typeparam>
        /// <param name="sectionName">Optional configuration section name.</param>
        /// <returns></returns>
        public static void LoadConfiguration(object? configuration, string? sectionName = null)
        {
            if (configuration == null)
                return;

            // Create switch mappings for environment variables
            var switchMappings = CreateSwitchMappings(configuration.GetType(), sectionName);

            // Build section-specific configuration
            var sectionConfiguration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables()
                .AddEnvironmentVariables(prefix: sectionName)
                .AddCommandLine(Environment.GetCommandLineArgs(), null)
                .AddCommandLine(Environment.GetCommandLineArgs(), switchMappings)
                .Build();

            // Bind the combined configuration to the instance
            sectionConfiguration.Bind(configuration);

            // Bind the section too, if provided
            if (!string.IsNullOrWhiteSpace(sectionName))
                sectionConfiguration.GetSection(sectionName).Bind(configuration);
        }

        public static bool ValidateConfiguration(object config)
        {
            var configType = config.GetType();
            var properties = configType.GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (var property in properties)
            {
                if (property.GetCustomAttribute<RequiredConfigurationAttribute>() != null)
                {
                    var value = property.GetValue(config);
                    if (value == null)
                    {
                        System.Diagnostics.Debug.WriteLine($"Configuration property '{property.Name}' is required but is not set.");
                        return false;
                    }
                }
            }
            return true;
        }
    }
}
