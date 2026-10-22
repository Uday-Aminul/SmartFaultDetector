import { useEffect, useState } from "react";
import { readingsApi } from "./api/api.ts";
import type { Reading } from "./types";

const STATE_COLORS: Record<string, string> = {
  Normal: "#2e7d32",
  ShortCircuit: "#c62828",
  OpenCircuit: "#ef6c00",
  Overload: "#6a1b9a",
};

// SQL Server datetime2 has no timezone, so the API sends UTC without a "Z".
// Add the "Z" so the browser converts it to local time.
const toLocalTime = (utc: string) =>
  new Date(utc.endsWith("Z") ? utc : utc + "Z").toLocaleTimeString();

function App() {
  const [recentReadings, setRecentReadings] = useState<Reading[]>([]);
  const [latest, setLatest] = useState<Reading | null>(null);
  const [offline, setOffline] = useState(false);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    let cancelled = false;

    const fetchData = async () => {
      try {
        const [allRes, latestRes] = await Promise.all([
          readingsApi.getAll(),
          readingsApi.getLatest(),
        ]);

        if (cancelled) return;

        // getAll returns newest-last or newest-first depending on your repository.
        // Your repo orders descending, so slice(0, 10) gives the 10 most recent.
        const data: Reading[] = allRes.data ?? [];
        setRecentReadings(data.slice(0, 10));

        // getLatest returns 204 when nothing was recorded in the last second.
        if (latestRes.status === 204 || !latestRes.data) {
          setOffline(true);
          setLatest(null);
        } else {
          setOffline(false);
          setLatest(latestRes.data);
        }

        setError(null);
      } catch (err) {
        if (cancelled) return;
        console.error(err);
        setError("Failed to reach the backend.");
        setOffline(true);
      }
    };

    fetchData();
    const interval = setInterval(fetchData, 1000);
    return () => {
      cancelled = true;
      clearInterval(interval);
    };
  }, []);

  return (
    <div
      style={{
        fontFamily: "system-ui, sans-serif",
        maxWidth: 900,
        margin: "2rem auto",
        padding: "0 1rem",
      }}
    >
      <h1>Smart Fault Detector</h1>

      {/* Current state panel */}
      <section style={{ marginBottom: "2rem" }}>
        <h2>Current State</h2>
        {error && <p style={{ color: "#c62828" }}>{error}</p>}
        {offline ? (
          <div
            style={{
              padding: "1rem",
              border: "2px solid #9e9e9e",
              borderRadius: 8,
              background: "#f5f5f5",
              color: "#616161",
              fontSize: "1.25rem",
            }}
          >
            Device Offline — no reading in the last second
          </div>
        ) : latest ? (
          <div
            style={{
              padding: "1rem",
              border: `2px solid ${STATE_COLORS[latest.predictedLabel] ?? "#000"}`,
              borderRadius: 8,
              background: "#fafafa",
            }}
          >
            <p
              style={{
                fontSize: "1.5rem",
                margin: 0,
                color: STATE_COLORS[latest.predictedLabel] ?? "#000",
              }}
            >
              {latest.predictedLabel}
            </p>
            <p style={{ margin: "0.5rem 0 0", color: "#555" }}>
              Confidence: {(latest.confidence * 100).toFixed(1)}% · V:{" "}
              {latest.voltageRms.toFixed(2)} · A: {latest.currentRms.toFixed(3)}
            </p>
          </div>
        ) : (
          <p>Loading…</p>
        )}
      </section>

      {/* Recent readings table */}
      <section>
        <h2>Recent Readings (last 10)</h2>
        <table style={{ width: "100%", borderCollapse: "collapse" }}>
          <thead>
            <tr style={{ borderBottom: "2px solid #ddd", textAlign: "left" }}>
              <th style={{ padding: "0.5rem" }}>Time</th>
              <th style={{ padding: "0.5rem" }}>Voltage (V)</th>
              <th style={{ padding: "0.5rem" }}>Current (A)</th>
              <th style={{ padding: "0.5rem" }}>Fault</th>
              <th style={{ padding: "0.5rem" }}>Confidence</th>
            </tr>
          </thead>
          <tbody>
            {recentReadings.length === 0 ? (
              <tr>
                <td
                  colSpan={5}
                  style={{
                    padding: "1rem",
                    textAlign: "center",
                    color: "#777",
                  }}
                >
                  No readings yet
                </td>
              </tr>
            ) : (
              recentReadings.map((r) => (
                <tr key={r.id} style={{ borderBottom: "1px solid #eee" }}>
                  <td style={{ padding: "0.5rem" }}>
                    {toLocalTime(r.recordedAt)}
                  </td>
                  <td style={{ padding: "0.5rem" }}>
                    {r.voltageRms.toFixed(2)}
                  </td>
                  <td style={{ padding: "0.5rem" }}>
                    {r.currentRms.toFixed(3)}
                  </td>
                  <td
                    style={{
                      padding: "0.5rem",
                      color: STATE_COLORS[r.predictedLabel] ?? "#000",
                      fontWeight: 600,
                    }}
                  >
                    {r.predictedLabel}
                  </td>
                  <td style={{ padding: "0.5rem" }}>
                    {(r.confidence * 100).toFixed(1)}%
                  </td>
                </tr>
              ))
            )}
          </tbody>
        </table>
      </section>
    </div>
  );
}

export default App;
