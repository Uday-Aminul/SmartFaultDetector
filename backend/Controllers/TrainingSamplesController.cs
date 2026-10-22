using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using backend.Models;
using backend.Models.Dtos;
using backend.Repository.SQLTrainingSample;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TrainingSamplesController : ControllerBase
    {
        private readonly IMapper _mapper;
        private readonly ITrainingSampleRepository _trainingSampleRepository;

        public TrainingSamplesController(IMapper mapper, ITrainingSampleRepository trainingSampleRepository)
        {
            _mapper = mapper;
            _trainingSampleRepository = trainingSampleRepository;
        }

        [HttpPost]
        public async Task<IActionResult> TrainingSamplesCreate([FromBody] AddTrainingSampleRequestDto newTrainingSample)
        {
            var trainingSampleDomain = _mapper.Map<TrainingSample>(newTrainingSample);
            trainingSampleDomain = await _trainingSampleRepository.CreateTrainingSampleAsync(trainingSampleDomain);
            var trainingSampleDto = _mapper.Map<TrainingSampleDto>(trainingSampleDomain);
            return Ok(trainingSampleDto);
        }
    }
}