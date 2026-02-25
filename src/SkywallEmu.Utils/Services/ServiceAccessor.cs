// This file is part of the SkywallEmu project and is published under the
// GPL 3.0. See LICENSE for further information.

using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;

namespace SkywallEmu.Utils.Services;

/// <summary>
/// An accessor class that allows accessing service instances from anywhere within the application once it has been
/// initialized
/// </summary>
public static class ServiceAccessor
{
    private static IServiceProvider? s_serviceProvider;

    /// <summary>
    /// Sets the service provider which contains the services of the application.
    /// </summary>
    /// <param name="serviceProvider">The instance of the ServiceProvider which will be used to access services</param>
    public static void SetServiceProvider(IServiceProvider serviceProvider)
    {
        s_serviceProvider = serviceProvider;
    }

    /// <summary>
    /// Attempts to retrieve a service instance of the given type
    /// </summary>
    /// <param name="service">The instance of the service, null when no service has been found</param>
    /// <typeparam name="T">The type of the service to be retrieved</typeparam>
    /// <returns>True, when a service of the given type has been found</returns>
    public static bool TryGetService<T>([NotNullWhen(true)] out T? service) where T : class
    {
        service = null;
        if (s_serviceProvider == null)
            return false;

        service = s_serviceProvider.GetService<T>();
        return service != null;
    }
}
