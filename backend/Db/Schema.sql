Create Database SmartFaultDetectorDb;
GO

Use SmartFaultDetectorDb
GO

Create Schema Training;
GO

Create Table Training.TrainingSamples(
	Id Int Primary Key Identity(1001, 1),
	SegmentId Int Not Null,
	RecordedAt DateTime2 Not Null Default SYSUTCDATETIME(),
	VoltageRms Float Not Null,
	CurrentRms Float Not Null,
	Label varchar(20) Not Null,
	Constraint CheckTrainingSamplesLabel
		Check(Label In ('Normal', 'ShortCircuit', 'OpenCircuit', 'Overload'))
)
GO

Create Schema Prediction;
GO

Create Table Prediction.Readings(
	Id Int Primary Key Identity(7001, 1),
	RecordedAt DateTime2 Not Null Default SYSUTCDATETIME(),
	VoltageRms Float Not Null,
	CurrentRms Float Not Null,
	PredictedLabel varchar(20) Not Null,
	Confidence float Not Null
)