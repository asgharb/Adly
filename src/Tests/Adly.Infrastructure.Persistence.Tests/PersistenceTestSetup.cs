using Adly.Application.Extensions;
using Adly.Infrastructure.Persistence.Extensions;
using Adly.Infrastructure.Persistence.Repositories.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Memory;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.MsSql;

namespace Adly.Infrastructure.Persistence.Tests;

public class PersistenceTestSetup:IAsyncLifetime
{
    public UnitOfWork UnitOfWork { get; private set; }

    private readonly MsSqlContainer _sqlContainer = new MsSqlBuilder()
        .WithImage("mcr.microsoft.com/mssql/server:2019-latest") 
        .WithPassword("your_strong_password_123")
        .Build();
    
    public IServiceProvider ServiceProvider { get; private set; }
    
    public async Task InitializeAsync()
    {
        try
        {
            await _sqlContainer.StartAsync();
        }
        catch (Exception ex)
        {
            var logs = await _sqlContainer.GetLogsAsync();
            Console.WriteLine($"Container logs: {logs.Stderr} -- {logs.Stdout}");
            throw;
        }
        
        var dbOptionBuilder = new DbContextOptionsBuilder<AdlyDbContext>()
            .UseSqlServer(_sqlContainer.GetConnectionString());

        var db = new AdlyDbContext(dbOptionBuilder.Options);
        
        await db.Database.MigrateAsync();

        UnitOfWork = new UnitOfWork(db);

        var configs = new Dictionary<string, string>()
        {
            { "ConnectionStrings:AdlyDb", _sqlContainer.GetConnectionString() }
        };

        var configurationBuilder = new ConfigurationBuilder();
        
        var inMemoryConfigs = new MemoryConfigurationSource() { InitialData = configs };

        configurationBuilder.Add(inMemoryConfigs);
        
        var serviceCollection = new ServiceCollection();

        serviceCollection
            .AddApplicationAutomapper()
            .AddApplicationMediatorServices()
            .RegisterApplicationValidators()
            .AddPersistenceDbContext(configurationBuilder.Build());

        ServiceProvider = serviceCollection.BuildServiceProvider(false);
    }

    public async Task DisposeAsync()
    {
            await _sqlContainer.StopAsync();
    }
}