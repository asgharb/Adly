using Adly.Application.Contracts.FileService.Interfaces;
using Adly.Application.Extensions;
using Adly.Infrastructure.CrossCutting.FileStorageService.Extensions;
using Adly.Infrastructure.CrossCutting.Logging;
using Adly.Infrastructure.Identity.Extensions;
using Adly.Infrastructure.Persistence.Extensions;
using Adly.WebFramework.Extensions;
using Adly.WebFramework.Filters;
using Adly.WebFramework.Models;
using Adly.WebFramework.Swagger;
using Microsoft.AspNetCore.Mvc;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog(LoggingConfiguration.ConfigureLogger);
// Add services to the container.

builder
    .AddSwagger("v1")
    .AddVersioning()
   ;

builder.Services
    .AddIdentityServices(builder.Configuration)
    .AddFileService(builder.Configuration)
    .AddApplicationAutomapper()
    .AddApplicationMediatorServices()
    .RegisterApplicationValidators()
    .AddPersistenceDbContext(builder.Configuration);

builder.ConfigureAuthenticationAndAuthorization();

builder.Services.AddControllers(options =>
{
    options.Filters.Add(typeof(OkResultAttribute));
    options.Filters.Add(typeof(NotFoundAttribute));
    options.Filters.Add(typeof(ModelStateValidationAttribute));
    options.Filters.Add(typeof(BadRequestAttribute));
    options.Filters.Add(new ProducesResponseTypeAttribute(typeof(ApiResult<Dictionary<string, List<string>>>),
        StatusCodes.Status400BadRequest));
    options.Filters.Add(new ProducesResponseTypeAttribute(typeof(ApiResult),
        StatusCodes.Status401Unauthorized));
    options.Filters.Add(new ProducesResponseTypeAttribute(typeof(ApiResult),
        StatusCodes.Status403Forbidden));
    options.Filters.Add(new ProducesResponseTypeAttribute(typeof(ApiResult),
        StatusCodes.Status500InternalServerError));

}).ConfigureApiBehaviorOptions(options =>
{
    options.SuppressModelStateInvalidFilter = true;
    options.SuppressMapClientErrors = true;
});


builder.Services.AddEndpointsApiExplorer();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    await app.ApplyMigrationsAsync();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();