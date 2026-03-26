import React, { useEffect, useState } from "react";
import { useNavigate } from "react-router-dom";
import {
  getDashboard,
  updateOccupancy,
  updateSpecial,
  addEvent,
  deleteEvent,
  getBusinessReservations,
} from "../services/api";

export default function BusinessDashboard() {
  const [data, setData] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  const [activeTab, setActiveTab] = useState("overview");
  const navigate = useNavigate();

  // Occupancy update
  const [occupancyPercent, setOccupancyPercent] = useState(50);
  const [occupancyMsg, setOccupancyMsg] = useState(null);

  // Special update
  const [specialMsg, setSpecialMsg] = useState("");
  const [specialResult, setSpecialResult] = useState(null);

  // Event form
  const [eventForm, setEventForm] = useState({
    title: "", description: "", category: "Müzik",
    startDate: "", endDate: "", ticketPrice: 0,
    capacity: 50, isAgeRestricted: false, minAge: 18,
  });
  const [eventMsg, setEventMsg] = useState(null);

  // Reservations
  const [reservations, setReservations] = useState([]);
  const [resDate, setResDate] = useState("");

  useEffect(() => {
    const token = localStorage.getItem("beach_token");
    if (!token) { navigate("/login"); return; }

    getDashboard()
      .then((res) => {
        setData(res.data);
        setOccupancyPercent(res.data.beach?.occupancyPercent || 0);
        setSpecialMsg(res.data.beach?.todaySpecial || "");
        setReservations(res.data.todayReservations || []);
      })
      .catch((err) => {
        if (err.response?.status === 401) navigate("/login");
        else setError("Dashboard yüklenemedi.");
      })
      .finally(() => setLoading(false));
  }, [navigate]);

  const handleUpdateOccupancy = () => {
    updateOccupancy(occupancyPercent)
      .then((res) => {
        setOccupancyMsg(`✅ Doluluk güncellendi: %${res.data.percent} (${res.data.level})`);
        setData((prev) => ({
          ...prev,
          stats: { ...prev.stats, occupancyPercent, occupancyLevel: res.data.level },
          beach: { ...prev.beach, occupancyPercent },
        }));
      })
      .catch(() => setOccupancyMsg("❌ Güncelleme başarısız."));
  };

  const handleUpdateSpecial = () => {
    updateSpecial(specialMsg)
      .then(() => setSpecialResult("✅ Günlük özel güncellendi!"))
      .catch(() => setSpecialResult("❌ Hata oluştu."));
  };

  const handleAddEvent = () => {
    addEvent(eventForm)
      .then(() => {
        setEventMsg("✅ Etkinlik eklendi!");
        setEventForm({ title: "", description: "", category: "Müzik", startDate: "", endDate: "", ticketPrice: 0, capacity: 50, isAgeRestricted: false, minAge: 18 });
        getDashboard().then((r) => setData(r.data));
      })
      .catch(() => setEventMsg("❌ Etkinlik eklenemedi."));
  };

  const handleDeleteEvent = (eventId) => {
    if (!window.confirm("Etkinliği silmek istiyor musunuz?")) return;
    deleteEvent(eventId)
      .then(() => setData((prev) => ({ ...prev, upcomingEvents: prev.upcomingEvents.filter((e) => e.id !== eventId) })))
      .catch(console.error);
  };

  const handleFetchReservations = () => {
    getBusinessReservations(resDate || undefined)
      .then((res) => setReservations(res.data))
      .catch(console.error);
  };

  if (loading) return <div className="loading"><div className="spinner" /></div>;
  if (error) return <div className="page"><div className="error-box">{error}</div></div>;
  if (!data) return null;

  const { beach, stats } = data;

  return (
    <div className="page">
      <div style={{ display: "flex", justifyContent: "space-between", alignItems: "flex-start", marginBottom: 24 }}>
        <div>
          <h1 className="page-title" style={{ marginBottom: 4 }}>
            {beach.name}
          </h1>
          <p style={{ color: "var(--text-muted)" }}>İşletme Paneli</p>
        </div>
        <span
          style={{
            padding: "6px 16px",
            borderRadius: 20,
            background: beach.isOpen ? "rgba(34,197,94,0.1)" : "rgba(239,68,68,0.1)",
            color: beach.isOpen ? "#16a34a" : "#dc2626",
            fontWeight: 600,
            fontSize: "0.85rem",
          }}
        >
          {beach.isOpen ? "✅ Açık" : "❌ Kapalı"}
        </span>
      </div>

      {/* Stats Row */}
      <div style={{ display: "grid", gridTemplateColumns: "repeat(4, 1fr)", gap: 16, marginBottom: 32 }}>
        {[
          { label: "Bugün Rez.", value: stats.todayReservationCount, icon: "📅" },
          { label: "Kişi", value: stats.todayPersonCount, icon: "👥" },
          { label: "Doluluk", value: `%${stats.occupancyPercent}`, icon: "📊" },
          { label: "Seviye", value: stats.occupancyLevel, icon: "🏖️" },
        ].map((s) => (
          <div key={s.label} className="card" style={{ padding: "20px", textAlign: "center" }}>
            <p style={{ fontSize: "1.5rem", marginBottom: 4 }}>{s.icon}</p>
            <p style={{ fontSize: "1.4rem", fontFamily: "Playfair Display, serif", color: "var(--ocean)", fontWeight: 700 }}>
              {s.value}
            </p>
            <p style={{ fontSize: "0.78rem", color: "var(--text-muted)", textTransform: "uppercase", letterSpacing: "0.05em" }}>
              {s.label}
            </p>
          </div>
        ))}
      </div>

      {/* Tabs */}
      <div className="tabs">
        {["overview", "occupancy", "reservations", "events"].map((tab) => (
          <button key={tab} className={`tab ${activeTab === tab ? "active" : ""}`} onClick={() => setActiveTab(tab)}>
            {tab === "overview" && "📋 Genel"}
            {tab === "occupancy" && "📊 Doluluk"}
            {tab === "reservations" && "📅 Rezervasyonlar"}
            {tab === "events" && "🎉 Etkinlikler"}
          </button>
        ))}
      </div>

      {/* ── Overview ── */}
      {activeTab === "overview" && (
        <div className="grid-2">
          {/* Doluluk Bar */}
          <div className="card" style={{ padding: 24 }}>
            <h3 style={{ color: "var(--ocean)", marginBottom: 16 }}>Anlık Doluluk</h3>
            <div style={{ marginBottom: 8 }}>
              <div className="occupancy-bar" style={{ height: 12 }}>
                <div
                  className="occupancy-fill"
                  style={{
                    width: `${beach.occupancyPercent}%`,
                    background: beach.occupancyPercent > 70 ? "var(--coral)" : beach.occupancyPercent > 40 ? "#f59e0b" : "#22c55e",
                  }}
                />
              </div>
            </div>
            <p style={{ textAlign: "right", fontWeight: 700, color: "var(--ocean)" }}>%{beach.occupancyPercent}</p>
          </div>

          {/* Today Special */}
          <div className="card" style={{ padding: 24 }}>
            <h3 style={{ color: "var(--ocean)", marginBottom: 16 }}>Günlük Özel</h3>
            {beach.todaySpecial ? (
              <div style={{ background: "var(--foam)", borderRadius: "var(--radius-sm)", padding: 12, fontSize: "0.9rem", color: "var(--text)" }}>
                ✨ {beach.todaySpecial}
              </div>
            ) : (
              <p style={{ color: "var(--text-muted)", fontSize: "0.88rem" }}>Bugün için özel mesaj girilmemiş.</p>
            )}
          </div>
        </div>
      )}

      {/* ── Occupancy ── */}
      {activeTab === "occupancy" && (
        <div style={{ display: "grid", gap: 24, maxWidth: 560 }}>
          <div className="card" style={{ padding: 28 }}>
            <h3 style={{ color: "var(--ocean)", marginBottom: 20 }}>Doluluk Güncelle</h3>

            <div className="form-group">
              <label className="form-label">Doluluk Yüzdesi: %{occupancyPercent}</label>
              <input
                type="range"
                min="0"
                max="100"
                value={occupancyPercent}
                onChange={(e) => setOccupancyPercent(parseInt(e.target.value))}
                style={{ width: "100%", accentColor: "var(--ocean)" }}
              />
              <div style={{ display: "flex", justifyContent: "space-between", fontSize: "0.78rem", color: "var(--text-muted)", marginTop: 4 }}>
                <span>Boş</span><span>Orta</span><span>Dolu</span>
              </div>
            </div>

            {occupancyMsg && (
              <div className={occupancyMsg.startsWith("✅") ? "success-box" : "error-box"} style={{ marginBottom: 16 }}>
                {occupancyMsg}
              </div>
            )}

            <button className="btn btn-primary" onClick={handleUpdateOccupancy}>
              Güncelle
            </button>
          </div>

          <div className="card" style={{ padding: 28 }}>
            <h3 style={{ color: "var(--ocean)", marginBottom: 20 }}>Günlük Özel Mesaj</h3>
            <div className="form-group">
              <label className="form-label">Mesaj (ör: Happy hour 17:00-19:00)</label>
              <textarea
                className="form-input"
                rows={3}
                value={specialMsg}
                onChange={(e) => setSpecialMsg(e.target.value)}
                placeholder="Bugün için özel mesajınızı girin..."
              />
            </div>
            {specialResult && (
              <div className={specialResult.startsWith("✅") ? "success-box" : "error-box"} style={{ marginBottom: 16 }}>
                {specialResult}
              </div>
            )}
            <button className="btn btn-primary" onClick={handleUpdateSpecial}>Kaydet</button>
          </div>
        </div>
      )}

      {/* ── Reservations ── */}
      {activeTab === "reservations" && (
        <div>
          <div style={{ display: "flex", gap: 12, alignItems: "center", marginBottom: 20 }}>
            <input
              type="date"
              className="form-input"
              style={{ width: 200 }}
              value={resDate}
              onChange={(e) => setResDate(e.target.value)}
            />
            <button className="btn btn-outline btn-sm" onClick={handleFetchReservations}>
              Getir
            </button>
          </div>

          {reservations.length === 0 ? (
            <p style={{ color: "var(--text-muted)" }}>Rezervasyon bulunamadı.</p>
          ) : (
            <div style={{ overflowX: "auto" }}>
              <table style={{ width: "100%", borderCollapse: "collapse", fontSize: "0.88rem" }}>
                <thead>
                  <tr style={{ background: "var(--foam)" }}>
                    {["Kod", "Ad", "Telefon", "Tarih", "Kişi", "Şezlong", "Tutar", "Durum"].map((h) => (
                      <th key={h} style={{ padding: "10px 12px", textAlign: "left", fontWeight: 600, color: "var(--text-muted)", fontSize: "0.78rem", textTransform: "uppercase" }}>{h}</th>
                    ))}
                  </tr>
                </thead>
                <tbody>
                  {reservations.map((r) => (
                    <tr key={r.id} style={{ borderBottom: "1px solid rgba(10,61,98,0.06)" }}>
                      <td style={{ padding: "12px", fontFamily: "monospace", color: "var(--coral)", fontWeight: 600 }}>{r.confirmationCode}</td>
                      <td style={{ padding: "12px" }}>{r.userName}</td>
                      <td style={{ padding: "12px" }}>{r.userPhone}</td>
                      <td style={{ padding: "12px" }}>{new Date(r.reservationDate).toLocaleDateString("tr-TR")}</td>
                      <td style={{ padding: "12px", textAlign: "center" }}>{r.personCount}</td>
                      <td style={{ padding: "12px", textAlign: "center" }}>{r.sunbedCount}</td>
                      <td style={{ padding: "12px" }}>{r.totalPrice > 0 ? `${r.totalPrice}₺` : "-"}</td>
                      <td style={{ padding: "12px" }}>
                        <span className={`badge ${r.status === "Confirmed" ? "badge-green" : r.status === "Cancelled" ? "badge-coral" : "badge-yellow"}`}>
                          {r.status}
                        </span>
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          )}
        </div>
      )}

      {/* ── Events ── */}
      {activeTab === "events" && (
        <div style={{ display: "grid", gridTemplateColumns: "1fr 1fr", gap: 24 }}>
          {/* Add Event Form */}
          <div className="card" style={{ padding: 24 }}>
            <h3 style={{ color: "var(--ocean)", marginBottom: 20 }}>Etkinlik Ekle</h3>

            {eventMsg && (
              <div className={eventMsg.startsWith("✅") ? "success-box" : "error-box"} style={{ marginBottom: 16 }}>
                {eventMsg}
              </div>
            )}

            <div className="form-group">
              <label className="form-label">Başlık</label>
              <input className="form-input" value={eventForm.title} onChange={(e) => setEventForm({ ...eventForm, title: e.target.value })} />
            </div>

            <div className="form-group">
              <label className="form-label">Açıklama</label>
              <textarea className="form-input" rows={3} value={eventForm.description} onChange={(e) => setEventForm({ ...eventForm, description: e.target.value })} />
            </div>

            <div style={{ display: "grid", gridTemplateColumns: "1fr 1fr", gap: 12 }}>
              <div className="form-group">
                <label className="form-label">Kategori</label>
                <select className="form-input" value={eventForm.category} onChange={(e) => setEventForm({ ...eventForm, category: e.target.value })}>
                  {["Müzik", "Spor", "Eğlence", "Yemek", "Dans", "Çocuk"].map((c) => (
                    <option key={c}>{c}</option>
                  ))}
                </select>
              </div>
              <div className="form-group">
                <label className="form-label">Kapasite</label>
                <input className="form-input" type="number" value={eventForm.capacity} onChange={(e) => setEventForm({ ...eventForm, capacity: parseInt(e.target.value) })} />
              </div>
              <div className="form-group">
                <label className="form-label">Başlangıç</label>
                <input className="form-input" type="datetime-local" value={eventForm.startDate} onChange={(e) => setEventForm({ ...eventForm, startDate: e.target.value })} />
              </div>
              <div className="form-group">
                <label className="form-label">Bitiş</label>
                <input className="form-input" type="datetime-local" value={eventForm.endDate} onChange={(e) => setEventForm({ ...eventForm, endDate: e.target.value })} />
              </div>
              <div className="form-group">
                <label className="form-label">Bilet Fiyatı (₺)</label>
                <input className="form-input" type="number" value={eventForm.ticketPrice} onChange={(e) => setEventForm({ ...eventForm, ticketPrice: parseFloat(e.target.value) })} />
              </div>
            </div>

            <label style={{ display: "flex", alignItems: "center", gap: 8, fontSize: "0.88rem", cursor: "pointer", marginBottom: 16 }}>
              <input
                type="checkbox"
                checked={eventForm.isAgeRestricted}
                onChange={(e) => setEventForm({ ...eventForm, isAgeRestricted: e.target.checked })}
              />
              Yaş kısıtlaması var (18+)
            </label>

            <button className="btn btn-primary" style={{ width: "100%", justifyContent: "center" }} onClick={handleAddEvent}>
              Etkinlik Ekle
            </button>
          </div>

          {/* Upcoming Events */}
          <div>
            <h3 style={{ color: "var(--ocean)", marginBottom: 16 }}>Yaklaşan Etkinlikler</h3>
            {(!data.upcomingEvents || data.upcomingEvents.length === 0) ? (
              <p style={{ color: "var(--text-muted)" }}>Etkinlik yok.</p>
            ) : (
              <div style={{ display: "flex", flexDirection: "column", gap: 12 }}>
                {data.upcomingEvents.map((ev) => (
                  <div key={ev.id} className="card" style={{ padding: 16, display: "flex", justifyContent: "space-between", alignItems: "flex-start" }}>
                    <div>
                      <p style={{ fontWeight: 600, color: "var(--ocean)", marginBottom: 4 }}>{ev.title}</p>
                      <p style={{ fontSize: "0.82rem", color: "var(--text-muted)" }}>
                        {new Date(ev.startDate).toLocaleDateString("tr-TR")} • {ev.category}
                      </p>
                    </div>
                    <button
                      onClick={() => handleDeleteEvent(ev.id)}
                      style={{ background: "none", border: "none", color: "var(--coral)", cursor: "pointer", fontSize: "1rem" }}
                    >
                      🗑️
                    </button>
                  </div>
                ))}
              </div>
            )}
          </div>
        </div>
      )}
    </div>
  );
}
