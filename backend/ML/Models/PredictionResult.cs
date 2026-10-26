using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace backend.ML.Models
{
    public class PredictionResult
    {
        public string Label { get; set; }
        public double Confidence { get; set; }
    }
}