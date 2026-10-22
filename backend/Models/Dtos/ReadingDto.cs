using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace backend.Models.Dtos
{
    public class ReadingDto
    {
        public int Id { get; set; }
        public DateTime RecordedAt { get; set; }
        public double VoltageRms { get; set; }
        public double CurrentRms { get; set; }
        public string PredictedLabel { get; set; } = null!;
        public double Confidence { get; set; }
    }
}