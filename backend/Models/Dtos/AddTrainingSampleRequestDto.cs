using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace backend.Models.Dtos
{
    public class AddTrainingSampleRequestDto
    {
        public int SegmentId { get; set; }
        public double VoltageRms { get; set; }
        public double CurrentRms { get; set; }
        public string Label { get; set; }
    }
}