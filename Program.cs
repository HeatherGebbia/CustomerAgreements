using CustomerAgreements.Data;
using Microsoft.EntityFrameworkCore;
using System;
using Serilog;
using Serilog.Sinks.MSSqlServer;
using Serilog.Filters;


var columnOptions = new Serilog.Sinks.MSSqlServer.ColumnOptions();
columnOptions.Store.Remove(StandardColumn.Properties);

Log.Logger = new LoggerConfiguration()
    .WriteTo.File("Logs/app-log-.txt", rollingInterval: RollingInterval.Day) 
    .CreateLogger();

Log.Logger = new LoggerConfiguration()  
    // Database for only your app events
    .WriteTo.Logger(lc => lc
        .Filter.ByIncludingOnly(Matching.FromSource("CustomerAgreements"))
        .WriteTo.MSSqlServer(
            connectionString: "Server=localhost;Database=HeatherLocalDB;Trusted_Connection=True;TrustServerCertificate=True;",
            sinkOptions: new Serilog.Sinks.MSSqlServer.MSSqlServerSinkOptions { TableName = "UserLogs", AutoCreateSqlTable = true },
            restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Information,
            columnOptions: columnOptions)
    )
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog();

// Add services to the container.
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"))
           .EnableSensitiveDataLogging()
           .EnableDetailedErrors());        

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
