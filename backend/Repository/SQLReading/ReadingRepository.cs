using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using backend.Data;
using backend.Models;
using Microsoft.EntityFrameworkCore;

namespace backend.Repository.SQLReading
{
    public class ReadingRepository : IReadingRepository
    {
        private readonly SmartFaultDetectorDbContext _dbContext;

        public ReadingRepository(SmartFaultDetectorDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Reading> CreateReadingAsync(Reading newReading)
        {
            await _dbContext.Readings.AddAsync(newReading);
            await _dbContext.SaveChangesAsync();
            return newReading;
        }

        public async Task<List<Reading>> GetAllReadingsAsync(int count = 10)
        {
            var readings = await _dbContext.Readings
                .OrderByDescending(reading => reading.RecordedAt)
                .Take(count)
                .ToListAsync();
            return readings;
        }

        public async Task<Reading?> GetLatestReadingAsync(TimeSpan maxAge)
        {
            var cutoff = DateTime.UtcNow - maxAge;
            var reading = _dbContext.Readings
                .Where(reading => reading.RecordedAt >= cutoff)
                .OrderByDescending(reading => reading.RecordedAt)
                .FirstOrDefault();
            return reading;
        }
    }
}