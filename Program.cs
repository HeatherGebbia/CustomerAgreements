using CustomerAgreements.Data;
using CustomerAgreements.Services;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Serilog.Filters;
using Serilog.Sinks.MSSqlServer;
using System;
using System.Diagnostics;

var columnOptions = new Serilog.Sinks.MSSqlServer.ColumnOptions();
columnOptions.Store.Remove(StandardColumn.Properties);

Log.Logger = new LoggerConfiguration()
    .WriteTo.File("Logs/app-log-.txt", rollingInterval: RollingInterval.Day) 
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, services, configuration) =>
{
    configuration
        .ReadFrom.Configuration(context.Configuration) 
        .WriteTo.File("Logs/app-log-.txt", rollingInterval: RollingInterval.Day);

    var connString = context.Configuration.GetConnectionString("DefaultConnection");

    configuration.WriteTo.Logger(lc => lc
        .Filter.ByIncludingOnly(Matching.FromSource("CustomerAgreements"))
        .WriteTo.MSSqlServer(
            connectionString: connString,
            sinkOptions: new Serilog.Sinks.MSSqlServer.MSSqlServerSinkOptions
            {
                TableName = "UserLogs",
                AutoCreateSqlTable = false 
            },
            restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Information,
            columnOptions: columnOptions
        )
    );
});

// Add services to the container.
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"))
           .EnableSensitiveDataLogging()
           .EnableDetailedErrors());

builder.Services.AddScoped<AgreementResponseService>();

builder.Services.AddRazorPages();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
else
{
    app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapRazorPages();

app.Run();
