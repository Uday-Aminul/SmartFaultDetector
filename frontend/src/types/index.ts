export interface Reading {
  id: number;
  recordedAt: string;
  voltageRms: number;
  currentRms: number;
  predictedLabel: string;
  confidence: number;
}

export interface AddReadingRequest {
  voltageRms: number;
  currentRms: number;
}

export interface TrainingSample {
  id: number;
  segmentId: number;
  recordedAt: string;
  voltageRms: number;
  currentRms: number;
  label: string;
}

export interface AddTrainingSampleRequest {
  segmentId: number;
  voltageRms: number;
  currentRms: number;
  label: string;
}