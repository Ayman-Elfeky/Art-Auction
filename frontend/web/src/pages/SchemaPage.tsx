const API_BASE = import.meta.env.VITE_API_URL ?? "http://localhost:5161";

export function SchemaPage() {
  return (
    <section>
      <h2>Database Schema</h2>
      <p>Live HTML schema document served by backend.</p>
      <div className="schema-frame-wrap">
        <iframe className="schema-frame" src={`${API_BASE}/api/schema`} title="Database Schema" />
      </div>
    </section>
  );
}
