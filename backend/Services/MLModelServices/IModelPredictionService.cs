using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using backend.ML.Models;

namespace backend.Services.MLModelServices
{
    public interface IModelPredictionService
    {
        PredictionResult Predict(double voltageRms, double currentRms);
        void Reload();
    }
}