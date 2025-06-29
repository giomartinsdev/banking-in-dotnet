using System.Diagnostics;
using BankingProject.API.Extensions;
using BankingProject.Application.Services;
using BankingProject.Domain.Context.CustomerAggregate.Repositories;
using BankingProject.Infrastructure;
using BankingProject.Infrastructure.DataPersistence.Banking;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.ConfigureHttpClientDefaults(http =>
{
    http.ConfigureHttpClient(client =>
    {
        client.Timeout = TimeSpan.FromMinutes(5);
    });
});

builder.Services.AddProblemDetails();

builder.Services.AddOpenApi();

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen
(
    option =>
    {
        option.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
        {
            Title = "Banking Project API",
            Version = "v1",
            Description = "API for the Banking Project"
        });
    }
);

builder.Services.AddScoped<CustomerService>();
builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();
builder.Services.AddScoped<BalanceOperationService>();

builder.Services.AddSingleton<ActivitySource>(sp => 
    new ActivitySource(Extensions.ActivitySourceConfiguration.ApiSourceName, Extensions.ActivitySourceConfiguration.Version));

builder.AddMongoDbInfrastructure();

var app = builder.Build();

app.UseGlobalExceptionHandling();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.RoutePrefix = string.Empty;
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
    });
}

app.UseRouting();

app.MapControllers();

app.MapDefaultEndpoints();

app.Run();
