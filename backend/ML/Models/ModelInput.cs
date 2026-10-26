using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.ML.Data;

namespace backend.ML.Models
{
    public class ModelInput
    {
        [LoadColumn(0)]
        public float VoltageRms { get; set; }

        [LoadColumn(1)]
        public float CurrentRms { get; set; }

        [LoadColumn(2)]
        public string Label { get; set; } = null!;
    }
}