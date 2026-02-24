// This file is part of the SkywallEmu project and is published under the
// GPL 3.0. See LICENSE for further information.

using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Configuration;

namespace SkywallEmu.Utils.Configuration;

/// <summary>
/// An Accessor class that allows reading loaded appsettings.json configuration values from anywhere once the
/// application has been initialized
/// </summary>
public static class AppSettings
{
    private static IConfigurationManager? s_configurationManager;

    /// <summary>
    /// Sets the configuration manager which contains the loaded appsettings.json configuration.
    /// </summary>
    /// <param name="configurationManager">The instance of the configuration manager which will be used
    /// to retrieve its values from</param>
    public static void SetConfigurationManager(IConfigurationManager configurationManager)
    {
        s_configurationManager = configurationManager;
    }

    /// <summary>
    /// Attempts to retrieve a value set within the appsettings.json configuration file
    /// </summary>
    /// <param name="key">The key under which the setting has been stored in the .json file</param>
    /// <param name="value">The configuration value when the key has been found, otherwise null/default</param>
    /// <typeparam name="T">The type of the configuration value into which it will be cast</typeparam>
    /// <returns>True when the key has been found within the configuration and the value has been successfully
    /// converted</returns>
    public static bool TryGetConfigValue<T>(string key, [NotNullWhen(true)] out T? value)
    {
        value = default;
        if (s_configurationManager == null)
            return false;

        value = s_configurationManager.GetValue<T>(key);
        return value != null;
    }

    /// <summary>
    /// Attempts to retrieve a database connection string set within the appsettings.json configuration file
    /// </summary>
    /// <param name="key">The key under which the connection string has been stored in the .json file</param>
    /// <param name="connectionString">The connection string when the value has been found, otherwise
    /// string.Empty</param>
    /// <returns>True when the key has been found and the connection still is not null, empty or whitespaces</returns>
    public static bool TryGetConnectionString(string key, [NotNullWhen(true)] out string? connectionString)
    {
        connectionString = string.Empty;
        if (s_configurationManager == null)
            return false;

        connectionString = s_configurationManager.GetConnectionString(key);
        return !string.IsNullOrWhiteSpace(connectionString);
    }
}
