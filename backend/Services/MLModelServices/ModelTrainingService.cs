using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using backend.Data;
using backend.ML.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.ML;

namespace backend.Services.MLModelServices
{
    public class ModelTrainingService : IModelTrainingService
    {
        private readonly SmartFaultDetectorDbContext _dbContext;
        private readonly string _modelPath;

        public ModelTrainingService(SmartFaultDetectorDbContext dbContext, IWebHostEnvironment env)
        {
            _dbContext = dbContext;
            _modelPath = Path.Combine(env.ContentRootPath, "ML", "TrainedModel", "fault-model.zip");
        }

        public async Task<object> ModelTrainAsync()
        {
            // 1. Load training data from DB
            var samples = await _dbContext.TrainingSamples
                .Select(s => new ModelInput
                {
                    VoltageRms = (float)s.VoltageRms,
                    CurrentRms = (float)s.CurrentRms,
                    Label = s.Label
                })
                .ToListAsync();

            if (samples.Count < 10)
                throw new InvalidOperationException("Not enough training samples. Need at least 10.");

            var mlContext = new MLContext(seed: 0);

            // 2. Build pipeline
            var pipeline = mlContext.Transforms.Conversion
                .MapValueToKey("Label")
                .Append(mlContext.Transforms.Concatenate("Features", "VoltageRms", "CurrentRms"))
                .Append(mlContext.MulticlassClassification.Trainers
                    .SdcaMaximumEntropy(labelColumnName: "Label", featureColumnName: "Features"))
                .Append(mlContext.Transforms.Conversion.MapKeyToValue("PredictedLabel"));

            // 3. Train with 80/20 split
            var dataView = mlContext.Data.LoadFromEnumerable(samples);
            var split = mlContext.Data.TrainTestSplit(dataView, testFraction: 0.2);
            var model = pipeline.Fit(split.TrainSet);

            // 4. Evaluate
            var predictions = model.Transform(split.TestSet);
            var metrics = mlContext.MulticlassClassification.Evaluate(predictions);

            // 5. Save model
            Directory.CreateDirectory(Path.GetDirectoryName(_modelPath)!);
            mlContext.Model.Save(model, dataView.Schema, _modelPath);

            return new
            {
                TrainedAt = DateTime.UtcNow,
                Samples = samples.Count,
                Accuracy = metrics.MacroAccuracy,
                MicroAccuracy = metrics.MicroAccuracy,
                LogLoss = metrics.LogLoss,
                ModelPath = _modelPath
            };
        }
    }
}