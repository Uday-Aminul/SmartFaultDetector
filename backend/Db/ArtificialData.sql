USE SmartFaultDetectorDb;
GO

-- Optional: clear old data first
-- TRUNCATE TABLE Training.TrainingSamples;
-- GO

DECLARE @i INT = 1;
DECLARE @v FLOAT, @c FLOAT, @label VARCHAR(20);

WHILE @i <= 100
BEGIN
    -- Normal: 11.5–12.1 V, 0.38–0.45 A
    SET @v = 11.5 + (ABS(CHECKSUM(NEWID())) % 600) / 1000.0;
    SET @c = 0.38 + (ABS(CHECKSUM(NEWID())) % 70) / 1000.0;
    INSERT INTO Training.TrainingSamples (SegmentId, VoltageRms, CurrentRms, Label)
    VALUES (@i, @v, @c, 'Normal');

    -- ShortCircuit: 0.1–5.5 V, 3.5–5.0 A
    SET @v = 0.1 + (ABS(CHECKSUM(NEWID())) % 5400) / 1000.0;
    SET @c = 3.5 + (ABS(CHECKSUM(NEWID())) % 1500) / 1000.0;
    INSERT INTO Training.TrainingSamples (SegmentId, VoltageRms, CurrentRms, Label)
    VALUES (@i, @v, @c, 'ShortCircuit');

    -- OpenCircuit: 0.0–0.1 V, 0.0–0.01 A
    SET @v = (ABS(CHECKSUM(NEWID())) % 100) / 1000.0;
    SET @c = (ABS(CHECKSUM(NEWID())) % 10) / 1000.0;
    INSERT INTO Training.TrainingSamples (SegmentId, VoltageRms, CurrentRms, Label)
    VALUES (@i, @v, @c, 'OpenCircuit');

    -- Overload: 11.2–11.9 V, 1.8–2.3 A
    SET @v = 11.2 + (ABS(CHECKSUM(NEWID())) % 700) / 1000.0;
    SET @c = 1.8 + (ABS(CHECKSUM(NEWID())) % 500) / 1000.0;
    INSERT INTO Training.TrainingSamples (SegmentId, VoltageRms, CurrentRms, Label)
    VALUES (@i, @v, @c, 'Overload');

    SET @i = @i + 1;
END;
GO

SELECT Label, COUNT(*) AS SampleCount
FROM Training.TrainingSamples
GROUP BY Label;
GO