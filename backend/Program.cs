using backend.Data;
using backend.Mappings;
using backend.Repository.SQLReading;
using backend.Repository.SQLTrainingSample;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<SmartFaultDetectorDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("SmartFaultDetectorConnectionString")));


builder.Services.AddAutoMapper(typeof(AutoMapperProfiles));

builder.Services.AddScoped<ITrainingSampleRepository, TrainingSampleRepository>();
builder.Services.AddScoped<IReadingRepository, ReadingRepository>();



builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi

//builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:5173")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();  // needed for SignalR later
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Classroom Management API v1");
        options.RoutePrefix = string.Empty; // makes Swagger UI the default page
    });
}

app.UseHttpsRedirection();

app.UseCors("AllowFrontend");

app.UseAuthorization();

app.MapControllers();

app.Run();
