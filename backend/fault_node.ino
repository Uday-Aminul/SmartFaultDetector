// ESP32 fault node: cycles relay states, samples AC voltage/current (RMS),
// and POSTs to the backend once per second.
//
// MODE SWITCH (GPIO 4, INPUT_PULLUP):
//   switch/jumper to GND  -> TRAINING mode -> POST /api/TrainingSamples (with label + segmentId)
//   pin left open         -> LIVE mode     -> POST /api/Readings (voltage + current only)

#include <WiFi.h>
#include <HTTPClient.h>
#include <Preferences.h>
#include <math.h>

// ---------------- CONFIG ----------------
const char *WIFI_SSID = "YOUR_WIFI_NAME";
const char *WIFI_PASS = "YOUR_WIFI_PASSWORD";
const char *SERVER = "http://192.168.0.100:5138"; // your PC's local IP + API port

// Pins (ADC1 pins only: ADC2 does not work while Wi-Fi is on)
const int PIN_VOLTAGE = 34; // ZMPT101B output
const int PIN_CURRENT = 35; // ACS712 output (through a divider, max 3.3 V)
const int PIN_MODE = 4;     // mode switch to GND
const int PIN_RELAY_SHORT = 26;
const int PIN_RELAY_OPEN = 27;
const int PIN_RELAY_OVER = 14;

const bool RELAY_ACTIVE_LOW = true; // most relay modules: LOW = relay ON

// Timing
const unsigned long SETTLE_MS = 3000;          // wait after switching, nothing is sent
const int SAMPLES_PER_HOLD = 10;               // samples per state per cycle
const unsigned long SAMPLE_INTERVAL_MS = 1000; // one POST per second

// Calibration: start here, then adjust until readings match your multimeter / analog meters
const double VOLTAGE_CAL = 1.0;                         // volts per mV of AC signal at the ADC
const double ACS_DIVIDER = 0.667;                       // divider ratio on the ACS712 output (10k/20k = 0.667)
const double CURRENT_CAL = 1.0 / (100.0 * ACS_DIVIDER); // ACS712-20A = 100 mV per amp

const bool RESET_SEGMENTS_ON_BOOT = false; // set true once if you clear the TrainingSamples table

// ---------------- STATE ----------------
enum State
{
  NORMAL = 0,
  SHORT_CIRCUIT,
  OPEN_CIRCUIT,
  OVERLOAD
};
const char *LABELS[] = {"Normal", "ShortCircuit", "OpenCircuit", "Overload"};

Preferences prefs;
uint32_t segmentId = 0;
int stateIdx = 0;
int samplesSent = 0;
bool settling = true;
bool lastTraining = false;
unsigned long phaseStart = 0;
unsigned long nextSample = 0;

// ---------------- RELAYS ----------------
void setRelay(int pin, bool on)
{
  digitalWrite(pin, (on == RELAY_ACTIVE_LOW) ? LOW : HIGH);
}

void allRelaysOff()
{
  setRelay(PIN_RELAY_SHORT, false);
  setRelay(PIN_RELAY_OPEN, false);
  setRelay(PIN_RELAY_OVER, false);
}

// Only one fault relay is ever on at a time.
void applyState(int idx)
{
  allRelaysOff();
  delay(50);
  switch (idx)
  {
  case SHORT_CIRCUIT:
    setRelay(PIN_RELAY_SHORT, true);
    break;
  case OPEN_CIRCUIT:
    setRelay(PIN_RELAY_OPEN, true);
    break;
  case OVERLOAD:
    setRelay(PIN_RELAY_OVER, true);
    break;
  default:
    break; // NORMAL: all off
  }
}

// ---------------- SENSING ----------------
// RMS of the AC component (mean removed) over 200 ms = 10 cycles at 50 Hz, in millivolts.
double readRmsMilliVolts(int pin)
{
  const unsigned long windowUs = 200000;
  double sum = 0, sumSq = 0;
  uint32_t n = 0;
  unsigned long start = micros();
  while (micros() - start < windowUs)
  {
    double mv = analogReadMilliVolts(pin);
    sum += mv;
    sumSq += mv * mv;
    n++;
  }
  if (n == 0)
    return 0;
  double mean = sum / n;
  double var = sumSq / n - mean * mean;
  return var > 0 ? sqrt(var) : 0;
}

// ---------------- NETWORK ----------------
int postJson(const char *path, const char *body)
{
  if (WiFi.status() != WL_CONNECTED)
    return -1;
  HTTPClient http;
  http.setTimeout(2000);
  http.begin(String(SERVER) + path);
  http.addHeader("Content-Type", "application/json");
  int code = http.POST((uint8_t *)body, strlen(body));
  http.end();
  return code;
}

// ---------------- CYCLE CONTROL ----------------
void startHold(bool training)
{
  applyState(stateIdx);
  settling = true;
  phaseStart = millis();
  samplesSent = 0;
  if (training)
  {
    segmentId++;
    prefs.putUInt("seg", segmentId);
  }
  Serial.printf("--- %s | state %s | segment %lu | settling...\n",
                training ? "TRAINING" : "LIVE", LABELS[stateIdx], (unsigned long)segmentId);
}

void restartCycle(bool training)
{
  stateIdx = NORMAL;
  startHold(training);
}

bool isTraining() { return digitalRead(PIN_MODE) == LOW; }

// ---------------- SETUP / LOOP ----------------
void setup()
{
  Serial.begin(115200);

  pinMode(PIN_MODE, INPUT_PULLUP);
  pinMode(PIN_RELAY_SHORT, OUTPUT);
  pinMode(PIN_RELAY_OPEN, OUTPUT);
  pinMode(PIN_RELAY_OVER, OUTPUT);
  allRelaysOff();

  analogSetAttenuation(ADC_11db); // 0 - 3.3 V range

  prefs.begin("fault", false);
  if (RESET_SEGMENTS_ON_BOOT)
    prefs.putUInt("seg", 0);
  segmentId = prefs.getUInt("seg", 0);

  WiFi.mode(WIFI_STA);
  WiFi.setAutoReconnect(true);
  WiFi.begin(WIFI_SSID, WIFI_PASS);
  Serial.print("Connecting to Wi-Fi");
  while (WiFi.status() != WL_CONNECTED)
  {
    delay(500);
    Serial.print(".");
  }
  Serial.printf("\nConnected. ESP32 IP: %s\n", WiFi.localIP().toString().c_str());

  lastTraining = isTraining();
  restartCycle(lastTraining);
}

void loop()
{
  bool training = isTraining();
  if (training != lastTraining)
  { // switch flipped: restart the cycle in the new mode
    lastTraining = training;
    restartCycle(training);
  }

  unsigned long now = millis();

  if (settling)
  {
    if (now - phaseStart >= SETTLE_MS)
    {
      settling = false;
      nextSample = now;
    }
    return;
  }

  if (now < nextSample)
    return;
  nextSample += SAMPLE_INTERVAL_MS;
  if (now > nextSample)
    nextSample = now + SAMPLE_INTERVAL_MS; // fell behind: resync

  double v = readRmsMilliVolts(PIN_VOLTAGE) * VOLTAGE_CAL;
  double c = readRmsMilliVolts(PIN_CURRENT) * CURRENT_CAL;

  char body[160];
  int code;
  if (training)
  {
    snprintf(body, sizeof(body),
             "{\"segmentId\":%lu,\"voltageRms\":%.3f,\"currentRms\":%.3f,\"label\":\"%s\"}",
             (unsigned long)segmentId, v, c, LABELS[stateIdx]);
    code = postJson("/api/TrainingSamples", body);
  }
  else
  {
    snprintf(body, sizeof(body), "{\"voltageRms\":%.3f,\"currentRms\":%.3f}", v, c);
    code = postJson("/api/Readings", body);
  }

  samplesSent++;
  Serial.printf("[%s] %s %d/%d  V=%.2f  I=%.3f  -> HTTP %d\n",
                training ? "TRAIN" : "LIVE", LABELS[stateIdx], samplesSent, SAMPLES_PER_HOLD, v, c, code);

  if (samplesSent >= SAMPLES_PER_HOLD)
  {
    stateIdx = (stateIdx + 1) % 4;
    startHold(training);
  }
}
