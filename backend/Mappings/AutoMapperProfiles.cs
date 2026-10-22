using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using backend.Models;
using backend.Models.Dtos;

namespace backend.Mappings
{
    public class AutoMapperProfiles : Profile
    {
        public AutoMapperProfiles()
        {
            CreateMap<TrainingSample, TrainingSampleDto>();
            CreateMap<AddTrainingSampleRequestDto, TrainingSample>();

            CreateMap<Reading, ReadingDto>();
            CreateMap<AddReadingRequestDto, Reading>();
        }
    }
}