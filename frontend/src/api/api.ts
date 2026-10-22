import axios from 'axios';
import type { AddReadingRequest, AddTrainingSampleRequest } from '../types';

const BASE_URL = import.meta.env.VITE_API_URL ?? "http://localhost:5138";

const api = axios.create({
  baseURL: BASE_URL,
  headers: {
    'Content-Type': 'application/json',
  },
});

export const readingsApi = {
  getAll: () => api.get('/api/Readings'),
  getLatest: () => api.get(`/api/Readings/latest`),
  create: (data:AddReadingRequest) => api.post('/api/Readings', data),
};

export const trainingSamplesApi = {
  create: (data:AddTrainingSampleRequest) => api.post('/api/TrainingSamples', data),
};