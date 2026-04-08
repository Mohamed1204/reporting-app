using Microsoft.EntityFrameworkCore;
using ReportingApi1.Data;
using ReportingApi1.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<VatReportingContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Register application services
builder.Services.AddScoped<ICompanyService, CompanyService>();
builder.Services.AddScoped<IVatReportService, VatReportService>();
builder.Services.AddScoped<IReportingPeriodService, ReportingPeriodService>();

// Configure CORS for Vue frontend
builder.Services.AddCors(options =>
{
    options.AddPolicy("VueFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:5173", "http://localhost:3000")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("VueFrontend");

app.UseHttpsRedirection();

app.MapControllers();

app.Run();