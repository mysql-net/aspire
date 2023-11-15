// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Data.Common;
using Aspire;
using Aspire.MySqlConnector;
using HealthChecks.MySql;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using MySqlConnector;

namespace Microsoft.Extensions.Hosting;

/// <summary>
/// Extension methods for connecting MySQL database with MySqlConnector client
/// </summary>
public static class AspireMySqlConnectorExtensions
{
    private const string DefaultConfigSectionName = "Aspire:MySql";

    /// <summary>
    /// Registers <see cref="MySqlDataSource"/> service for connecting MySQL database with MySqlConnector client.
    /// Configures health check, logging and telemetry for the MySqlConnector client.
    /// </summary>
    /// <param name="builder">The <see cref="IHostApplicationBuilder" /> to read config from and add services to.</param>
    /// <param name="connectionName">A name used to retrieve the connection string from the ConnectionStrings configuration section.</param>
    /// <param name="configureSettings">An optional delegate that can be used for customizing options. It's invoked after the settings are read from the configuration.</param>
    /// <remarks>Reads the configuration from "Aspire:MySql" section.</remarks>
    /// <exception cref="ArgumentNullException">Thrown if mandatory <paramref name="builder"/> is null.</exception>
    /// <exception cref="InvalidOperationException">Thrown when mandatory <see cref="MySqlConnectorSettings.ConnectionString"/> is not provided.</exception>
    public static void AddMySqlDataSource(this IHostApplicationBuilder builder, string connectionName, Action<MySqlConnectorSettings>? configureSettings = null)
        => AddMySqlDataSource(builder, DefaultConfigSectionName, configureSettings, connectionName, serviceKey: null);

    /// <summary>
    /// Registers <see cref="MySqlDataSource"/> as a keyed service for given <paramref name="name"/> for connecting MySQL database with MySqlConnector client.
    /// Configures health check, logging and telemetry for the MySqlConnector client.
    /// </summary>
    /// <param name="builder">The <see cref="IHostApplicationBuilder" /> to read config from and add services to.</param>
    /// <param name="name">The name of the component, which is used as the <see cref="ServiceDescriptor.ServiceKey"/> of the service and also to retrieve the connection string from the ConnectionStrings configuration section.</param>
    /// <param name="configureSettings">An optional method that can be used for customizing options. It's invoked after the settings are read from the configuration.</param>
    /// <remarks>Reads the configuration from "Aspire:MySql:{name}" section.</remarks>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="builder"/> or <paramref name="name"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown if mandatory <paramref name="name"/> is empty.</exception>
    /// <exception cref="InvalidOperationException">Thrown when mandatory <see cref="MySqlConnectorSettings.ConnectionString"/> is not provided.</exception>
    public static void AddKeyedMySqlDataSource(this IHostApplicationBuilder builder, string name, Action<MySqlConnectorSettings>? configureSettings = null)
    {
        ArgumentException.ThrowIfNullOrEmpty(name);

        AddMySqlDataSource(builder, $"{DefaultConfigSectionName}:{name}", configureSettings, connectionName: name, serviceKey: name);
    }

    private static void AddMySqlDataSource(IHostApplicationBuilder builder, string configurationSectionName,
        Action<MySqlConnectorSettings>? configureSettings, string connectionName, object? serviceKey)
    {
        ArgumentNullException.ThrowIfNull(builder);

        MySqlConnectorSettings settings = new();
        builder.Configuration.GetSection(configurationSectionName).Bind(settings);

        if (builder.Configuration.GetConnectionString(connectionName) is string connectionString)
        {
            settings.ConnectionString = connectionString;
        }

        configureSettings?.Invoke(settings);

        builder.RegisterMySqlServices(settings, configurationSectionName, connectionName, serviceKey);

        // Same as SqlClient connection pooling is on by default and can be handled with connection string 
        // https://mysqlconnector.net/connection-options/#Pooling
        if (settings.HealthChecks)
        {
            builder.TryAddHealthCheck(new HealthCheckRegistration(
                serviceKey is null ? "MySql" : $"MySql_{connectionName}",
                sp => new MySqlHealthCheck(new MySqlHealthCheckOptions()
                {
                    ConnectionString = serviceKey is null
                        ? sp.GetRequiredService<MySqlDataSource>().ConnectionString
                        : sp.GetRequiredKeyedService<MySqlDataSource>(serviceKey).ConnectionString
                }),
                failureStatus: default,
                tags: default,
                timeout: default));
        }

        if (settings.Tracing)
        {
            builder.Services.AddOpenTelemetry()
                .WithTracing(tracerProviderBuilder =>
                {
                    tracerProviderBuilder.AddSource("MySqlConnector");
                });
        }

        if (settings.Metrics)
        {
            builder.Services.AddOpenTelemetry()
                .WithMetrics(MySqlConnectorCommon.AddMySqlMetrics);
        }
    }

    private static void RegisterMySqlServices(this IHostApplicationBuilder builder, MySqlConnectorSettings settings, string configurationSectionName, string connectionName, object? serviceKey)
    {
        if (serviceKey is null)
        {
            // delay validating the ConnectionString until the DataSource is requested. This ensures an exception doesn't happen until a Logger is established.
            builder.Services.AddMySqlDataSource(settings.ConnectionString ?? string.Empty, dataSourceBuilder =>
            {
                ValidateConnection();
            });
        }
        else
        {
            // Currently MySqlConnector does not support Keyed DI Registration, so we implement it on our own.
            // Register a MySqlDataSource factory method, based on https://github.com/mysql-net/MySqlConnector/blob/d895afc013a5849d33a123a7061442e2cbb9ce76/src/MySqlConnector.DependencyInjection/MySqlConnectorServiceCollectionExtensions.cs#L57-L60
            builder.Services.AddKeyedSingleton<MySqlDataSource>(serviceKey, (serviceProvider, _) =>
            {
                ValidateConnection();

                var dataSourceBuilder = new MySqlDataSourceBuilder(settings.ConnectionString);
                dataSourceBuilder.UseLoggerFactory(serviceProvider.GetService<ILoggerFactory>());
                return dataSourceBuilder.Build();
            });
            // Common Services, based on https://github.com/mysql-net/MySqlConnector/blob/d895afc013a5849d33a123a7061442e2cbb9ce76/src/MySqlConnector.DependencyInjection/MySqlConnectorServiceCollectionExtensions.cs#L64-L70
            // They let the users resolve MySqlConnection directly.
            builder.Services.AddKeyedSingleton<DbDataSource>(serviceKey, static (serviceProvider, key) => serviceProvider.GetRequiredKeyedService<MySqlDataSource>(key));
            builder.Services.AddKeyedTransient<MySqlConnection>(serviceKey, static (serviceProvider, key) => serviceProvider.GetRequiredKeyedService<MySqlDataSource>(key).CreateConnection());
            builder.Services.AddKeyedTransient<DbConnection>(serviceKey, static (serviceProvider, key) => serviceProvider.GetRequiredKeyedService<MySqlConnection>(key));
        }

        void ValidateConnection()
        {
            if (string.IsNullOrEmpty(settings.ConnectionString))
            {
                throw new InvalidOperationException($"ConnectionString is missing. It should be provided in 'ConnectionStrings:{connectionName}' or under the 'ConnectionString' key in '{configurationSectionName}' configuration section.");
            }
        }
    }
}
