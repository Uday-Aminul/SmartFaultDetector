using AutoMapper;
using backend.Models;
using backend.Models.Dtos;
using backend.Repository.SQLReading;
using backend.Services.MLModelServices;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReadingsController : ControllerBase
    {
        private readonly IMapper _mapper;
        private readonly IReadingRepository _readingRepository;
        private readonly IModelPredictionService _modelPredictionService;

        public ReadingsController(IMapper mapper, IReadingRepository readingRepository, IModelPredictionService modelPredictionService)
        {
            _mapper = mapper;
            _readingRepository = readingRepository;
            _modelPredictionService = modelPredictionService;
        }

        [HttpGet]
        public async Task<IActionResult> ReadingGetAll()
        {
            var readingsDomain = await _readingRepository.GetAllReadingsAsync();
            var readingsDto = _mapper.Map<List<ReadingDto>>(readingsDomain);
            return Ok(readingsDto);
        }

        [HttpGet]
        [Route("latest")]
        public async Task<IActionResult> ReadingGetLatest()
        {
            var readingDomain = await _readingRepository.GetLatestReadingAsync(TimeSpan.FromSeconds(1));
            if (readingDomain is null)
            {
                return NoContent();
            }
            var readingDto = _mapper.Map<ReadingDto>(readingDomain);
            return Ok(readingDto);
        }

        [HttpPost]
        public async Task<IActionResult> ReadingCreate([FromBody] AddReadingRequestDto newReading)
        {
            var readingDomain = _mapper.Map<Reading>(newReading);

            var prediction = _modelPredictionService.Predict(readingDomain.VoltageRms, readingDomain.CurrentRms);
            readingDomain.PredictedLabel = prediction.Label;
            readingDomain.Confidence = prediction.Confidence;

            readingDomain = await _readingRepository.CreateReadingAsync(readingDomain);
            var readingDto = _mapper.Map<ReadingDto>(readingDomain);
            return Ok(readingDto);
        }
    }
}