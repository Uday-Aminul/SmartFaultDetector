using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using backend.Models;

namespace backend.Repository.SQLReading
{
    public interface IReadingRepository
    {
        Task<List<Reading>> GetAllReadingsAsync(int count = 10);
        Task<Reading> CreateReadingAsync(Reading newReading);
        Task<Reading?> GetLatestReadingAsync(TimeSpan maxAge);
    }
}