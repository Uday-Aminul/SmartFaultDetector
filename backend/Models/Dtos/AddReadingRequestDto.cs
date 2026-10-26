using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace backend.Models.Dtos
{
    public class AddReadingRequestDto
    {
        public double VoltageRms { get; set; }
        public double CurrentRms { get; set; }
        public string? PredictedLabel { get; set; }
        public double? Confidence { get; set; }
    }
}