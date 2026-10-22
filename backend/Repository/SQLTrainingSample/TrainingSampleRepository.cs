using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using backend.Data;
using backend.Models;

namespace backend.Repository.SQLTrainingSample
{
    public class TrainingSampleRepository : ITrainingSampleRepository
    {
        private readonly SmartFaultDetectorDbContext _dbContext;
        public TrainingSampleRepository(SmartFaultDetectorDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<TrainingSample> CreateTrainingSampleAsync(TrainingSample trainingSample)
        {
            await _dbContext.TrainingSamples.AddAsync(trainingSample);
            await _dbContext.SaveChangesAsync();
            return trainingSample;
        }
    }
}