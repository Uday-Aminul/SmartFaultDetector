using System;
using System.Collections.Generic;
namespace backend.Models;

public partial class Reading
{
    public int Id { get; set; }
    public DateTime RecordedAt { get; set; }
    public double VoltageRms { get; set; }
    public double CurrentRms { get; set; }
    public string PredictedLabel { get; set; } = null!;
    public double Confidence { get; set; }
}
