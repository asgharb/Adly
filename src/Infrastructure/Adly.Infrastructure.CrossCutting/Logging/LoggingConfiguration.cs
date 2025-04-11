using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Enrichers.Span;
using Serilog.Events;
using Serilog.Exceptions;

namespace Adly.Infrastructure.CrossCutting.Logging;

public class LoggingConfiguration
{
    public static Action<HostBuilderContext, LoggerConfiguration> ConfigureLogger => (context, configuration) =>
    {
        var applicationName = context.HostingEnvironment.ApplicationName;

        configuration
            .Enrich.WithSpan()
            .Enrich.WithEnvironmentName()
            .Enrich.WithMachineName()
            .Enrich.WithExceptionDetails()
            .Enrich.WithProperty("ApplicationName", applicationName);

        if (context.HostingEnvironment.IsDevelopment())
        {
            configuration.WriteTo.Console().MinimumLevel.Information();
            configuration.WriteTo.Seq(context.Configuration["Seq:Url"]!, LogEventLevel.Information,
                apiKey: context.Configuration["Seq:ApiKey"]!);
            return;
        }

        if (context.HostingEnvironment.IsProduction())
        {
            configuration.WriteTo.Console().MinimumLevel.Error();
            configuration.WriteTo.Seq(context.Configuration["Seq:Url"]!, LogEventLevel.Error,
                apiKey: context.Configuration["Seq:ApiKey"]!);
        }
    };
}

//builder.Host.UseSerilog(LoggingConfiguration.ConfigureLogger)