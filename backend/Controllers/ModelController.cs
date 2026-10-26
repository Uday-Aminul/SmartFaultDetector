using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using backend.Services.MLModelServices;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ModelController : ControllerBase
    {
        private readonly IModelTrainingService _modelTrainingService;
        private readonly IModelPredictionService _modelPredictionService;

        public ModelController(IModelTrainingService modelTrainingService, IModelPredictionService modelPredictionService)
        {
            _modelTrainingService = modelTrainingService;
            _modelPredictionService = modelPredictionService;
        }

        [HttpPost]
        [Route("train")]
        public async Task<IActionResult> Train()
        {
            var result = await _modelTrainingService.ModelTrainAsync();
            _modelPredictionService.Reload();   // pick up the new model immediately
            return Ok(result);
        }
    }
}