using backend.Data;
using backend.Repository.SQLTrainingSample;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<SmartFaultDetectorDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("SmartFaultDetectorDbContext")));



builder.Services.AddScoped<ITrainingSampleRepository, TrainingSampleRepository>();



builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
