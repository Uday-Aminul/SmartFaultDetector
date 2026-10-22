using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using backend.Models;

namespace backend.Repository.SQLTrainingSample
{
    public interface ITrainingSampleRepository
    {
        Task<TrainingSample> CreateTrainingSampleAsync(TrainingSample newTrainingSample);
    }
}