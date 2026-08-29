using DataLogging;
using VirtualEMS.DataServices;
using Microsoft.EntityFrameworkCore;

var builder = Host.CreateApplicationBuilder(args);

// register Windows Service metadata early
builder.Services.AddWindowsService(options =>
    options.ServiceName = "DataLog"
);

// get connection string from configuration (appsettings.json or environment)
var connectionString = builder.Configuration.GetConnectionString("ConfigDBConnString");
if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException("Connection string 'ConfigDBConnString' not found. Add it to appsettings.json or environment.");
}

// register EF Core DbContext and repository used by DataLog
//builder.Services.AddDbContext<AppDbContext>(options =>
//    options.UseSqlServer(connectionString));

builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseSqlServer(connectionString)
           .EnableSensitiveDataLogging()
           .LogTo(Console.WriteLine, LogLevel.Information);
});

builder.Services.AddScoped<iDbRepository, dbRepository>();

// register hosted service
builder.Services.AddHostedService<DataLog>();

var host = builder.Build();
host.Run();
