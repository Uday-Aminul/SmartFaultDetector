using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using backend.ML.Models;
using Microsoft.ML;

namespace backend.Services.MLModelServices
{
    public class ModelPredictionService : IModelPredictionService
    {
        private readonly object _lock = new();
        private readonly string _modelPath;
        private PredictionEngine<ModelInput, ModelOutput>? _engine;

        public string ModelPath => _modelPath;

        public ModelPredictionService(IWebHostEnvironment env)
        {
            _modelPath = Path.Combine(env.ContentRootPath, "ML", "TrainedModel", "fault-model.zip");
            Reload();
        }

        public void Reload()
        {
            lock (_lock)
            {
                if (!File.Exists(_modelPath))
                {
                    _engine = null;
                    return;
                }

                var mlContext = new MLContext();
                var model = mlContext.Model.Load(_modelPath, out _);
                _engine = mlContext.Model.CreatePredictionEngine<ModelInput, ModelOutput>(model);
            }
        }

        public PredictionResult Predict(double voltageRms, double currentRms)
        {
            lock (_lock)
            {
                if (_engine is null)
                    throw new InvalidOperationException(
                        "Model not trained yet. Call POST /api/Model/train first.");

                var input = new ModelInput
                {
                    VoltageRms = (float)voltageRms,
                    CurrentRms = (float)currentRms
                };

                var output = _engine.Predict(input);

                return new PredictionResult
                {
                    Label = output.PredictedLabel,
                    Confidence = output.Score.Length > 0 ? output.Score.Max() : 0
                };
            }
        }
    }
}