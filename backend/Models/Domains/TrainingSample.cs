using System;
using System.Collections.Generic;

namespace backend.Models;

public partial class TrainingSample
{
    public int Id { get; set; }

    public int SegmentId { get; set; }

    public DateTime RecordedAt { get; set; }

    public double VoltageRms { get; set; }

    public double CurrentRms { get; set; }

    public string Label { get; set; } = null!;
}
