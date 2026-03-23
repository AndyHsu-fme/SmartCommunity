import { useEffect, useState } from "react";
import { useAuth } from "../context/AuthContext";

const API = import.meta.env.VITE_API_URL ?? "http://localhost:5179";

interface Facility {
  facilityId: number;
  name: string;
  maxCapacity: number;
}

interface Reservation {
  reservationId: number;
  facilityId: number;
  facilityName: string;
  startTime: string;
  endTime: string;
  status: "Pending" | "Confirmed" | "Cancelled";
}

const STATUS_LABEL: Record<Reservation["status"], string> = {
  Pending: "待確認",
  Confirmed: "已確認",
  Cancelled: "已取消",
};

const STATUS_COLOR: Record<Reservation["status"], string> = {
  Pending: "bg-yellow-100 text-yellow-700",
  Confirmed: "bg-green-100 text-green-700",
  Cancelled: "bg-slate-100 text-slate-500",
};

export default function ReservationPage() {
  const { token } = useAuth();
  const [facilities, setFacilities] = useState<Facility[]>([]);
  const [reservations, setReservations] = useState<Reservation[]>([]);
  const [loading, setLoading] = useState(true);
  const [fetchError, setFetchError] = useState("");

  // 表單狀態
  const [facilityId, setFacilityId] = useState("");
  const [startTime, setStartTime] = useState("");
  const [endTime, setEndTime] = useState("");
  const [formError, setFormError] = useState("");
  const [submitting, setSubmitting] = useState(false);

  useEffect(() => { fetchAll(); }, [token]);

  const fetchAll = async () => {
    setLoading(true);
    setFetchError("");
    try {
      const [facRes, resRes] = await Promise.all([
        fetch(`${API}/api/reservations/facilities`, { headers: { Authorization: `Bearer ${token}` } }),
        fetch(`${API}/api/reservations`,            { headers: { Authorization: `Bearer ${token}` } }),
      ]);
      if (facRes.ok) setFacilities(await facRes.json());
      if (resRes.ok) setReservations(await resRes.json());
      if (!facRes.ok || !resRes.ok) {
        setFetchError("載入失敗，請確認後端服務已啟動");
        loadMockData();
      }
    } catch {
      setFetchError("後端未連線，顯示模擬資料");
      loadMockData();
    } finally {
      setLoading(false);
    }
  };

  const loadMockData = () => {
    setFacilities([
      { facilityId: 1, name: "B1 健身房", maxCapacity: 10 },
      { facilityId: 2, name: "頂樓游泳池", maxCapacity: 20 },
      { facilityId: 3, name: "社區交誼廳", maxCapacity: 30 },
      { facilityId: 4, name: "羽球場", maxCapacity: 4 },
    ]);
    setReservations([
      { reservationId: 1, facilityId: 1, facilityName: "B1 健身房", startTime: "2026-03-25T09:00:00Z", endTime: "2026-03-25T10:00:00Z", status: "Confirmed" },
    ]);
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setFormError("");

    if (new Date(startTime) >= new Date(endTime)) {
      setFormError("結束時間必須晚於開始時間");
      return;
    }

    setSubmitting(true);
    try {
      const res = await fetch(`${API}/api/reservations`, {
        method: "POST",
        headers: { "Content-Type": "application/json", Authorization: `Bearer ${token}` },
        body: JSON.stringify({
          facilityId: Number(facilityId),
          startTime: new Date(startTime).toISOString(),
          endTime:   new Date(endTime).toISOString(),
        }),
      });
      if (res.ok) {
        const dto = await res.json();
        setReservations((prev) => [...prev, dto]);
        setFacilityId(""); setStartTime(""); setEndTime("");
      } else if (res.status === 409) {
        setFormError("該時段已被預約，請選擇其他時間");
      } else {
        setFormError("預約失敗，請稍後再試");
      }
    } catch {
      setFormError("後端未連線，無法預約");
    } finally {
      setSubmitting(false);
    }
  };

  const handleCancel = async (reservationId: number) => {
    try {
      await fetch(`${API}/api/reservations/${reservationId}/cancel`, {
        method: "POST",
        headers: { Authorization: `Bearer ${token}` },
      });
    } catch { /* ignore */ }
    setReservations((prev) =>
      prev.map((r) =>
        r.reservationId === reservationId ? { ...r, status: "Cancelled" as const } : r
      )
    );
  };

  if (loading) return <div className="text-slate-500 text-sm">載入中...</div>;

  return (
    <div>
      <h2 className="text-2xl font-bold text-slate-800 mb-6">🏊 公設預約</h2>

      {fetchError && (
        <p className="text-sm text-amber-600 bg-amber-50 border border-amber-200 rounded-lg px-4 py-3 mb-4">
          ⚠️ {fetchError}
        </p>
      )}

      {/* 預約表單 */}
      <div className="bg-white rounded-xl shadow p-6 mb-8">
        <h3 className="text-base font-semibold text-slate-700 mb-4">新增預約</h3>
        <form onSubmit={handleSubmit} className="grid grid-cols-1 gap-4 sm:grid-cols-2">
          <div className="sm:col-span-2">
            <label className="block text-sm font-medium text-slate-700 mb-1">公設設施</label>
            <select
              value={facilityId}
              onChange={(e) => setFacilityId(e.target.value)}
              required
              className="w-full border border-slate-300 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500"
            >
              <option value="">請選擇設施</option>
              {facilities.map((f) => (
                <option key={f.facilityId} value={f.facilityId}>
                  {f.name}（容量 {f.maxCapacity} 人）
                </option>
              ))}
            </select>
          </div>
          <div>
            <label className="block text-sm font-medium text-slate-700 mb-1">開始時間</label>
            <input
              type="datetime-local"
              value={startTime}
              onChange={(e) => setStartTime(e.target.value)}
              required
              className="w-full border border-slate-300 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500"
            />
          </div>
          <div>
            <label className="block text-sm font-medium text-slate-700 mb-1">結束時間</label>
            <input
              type="datetime-local"
              value={endTime}
              onChange={(e) => setEndTime(e.target.value)}
              required
              className="w-full border border-slate-300 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500"
            />
          </div>
          {formError && (
            <p className="sm:col-span-2 text-sm text-red-600">{formError}</p>
          )}
          <div className="sm:col-span-2">
            <button
              type="submit"
              disabled={submitting}
              className="bg-blue-600 text-white px-6 py-2 rounded-lg text-sm font-medium hover:bg-blue-700 disabled:opacity-50 transition-colors"
            >
              {submitting ? "預約中..." : "送出預約"}
            </button>
          </div>
        </form>
      </div>

      {/* 我的預約列表 */}
      <div className="bg-white rounded-xl shadow p-6">
        <h3 className="text-base font-semibold text-slate-700 mb-4">我的預約紀錄</h3>
        {reservations.length === 0 ? (
          <p className="text-sm text-slate-400">尚無預約紀錄。</p>
        ) : (
          <div className="space-y-3">
            {reservations.map((r) => (
              <div
                key={r.reservationId}
                className="flex items-center justify-between border border-slate-100 rounded-lg px-4 py-3 bg-slate-50"
              >
                <div>
                  <p className="text-sm font-medium text-slate-800">{r.facilityName}</p>
                  <p className="text-xs text-slate-500">
                    {new Date(r.startTime).toLocaleString("zh-TW")} –{" "}
                    {new Date(r.endTime).toLocaleString("zh-TW")}
                  </p>
                </div>
                <div className="flex items-center gap-3">
                  <span className={`text-xs px-2 py-1 rounded-full ${STATUS_COLOR[r.status]}`}>
                    {STATUS_LABEL[r.status]}
                  </span>
                  {r.status !== "Cancelled" && (
                    <button
                      onClick={() => handleCancel(r.reservationId)}
                      className="text-xs text-red-500 hover:text-red-700 transition-colors"
                    >
                      取消
                    </button>
                  )}
                </div>
              </div>
            ))}
          </div>
        )}
      </div>
    </div>
  );
}
