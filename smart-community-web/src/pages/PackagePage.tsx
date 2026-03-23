import { useEffect, useState } from "react";
import { useAuth } from "../context/AuthContext";

const API = import.meta.env.VITE_API_URL ?? "http://localhost:5179";

interface Package {
  packageId: number;
  carrierName: string;
  arrivalDate: string;
  pickupDate?: string;
  status: "Pending" | "PickedUp";
}

const STATUS_LABEL: Record<Package["status"], string> = {
  Pending: "待領取",
  PickedUp: "已領取",
};

export default function PackagePage() {
  const { token } = useAuth();
  const [packages, setPackages] = useState<Package[]>([]);
  const [loading, setLoading] = useState(true);
  const [fetchError, setFetchError] = useState("");
  const [confirmingId, setConfirmingId] = useState<number | null>(null);

  useEffect(() => { fetchPackages(); }, [token]);

  const fetchPackages = async () => {
    setLoading(true);
    setFetchError("");
    try {
      const res = await fetch(`${API}/api/packages`, {
        headers: { Authorization: `Bearer ${token}` },
      });
      if (res.ok) {
        setPackages(await res.json());
      } else {
        setFetchError("載入失敗，請確認後端服務已啟動");
        loadMockPackages();
      }
    } catch {
      setFetchError("後端未連線，顯示模擬資料");
      loadMockPackages();
    } finally {
      setLoading(false);
    }
  };

  const loadMockPackages = () => {
    setPackages([
      { packageId: 1, carrierName: "黑貓宅急便", arrivalDate: "2026-03-20T10:00:00Z", status: "Pending" },
      { packageId: 2, carrierName: "統一速達", arrivalDate: "2026-03-18T14:00:00Z", pickupDate: "2026-03-19T09:00:00Z", status: "PickedUp" },
      { packageId: 3, carrierName: "順豐速運", arrivalDate: "2026-03-22T11:00:00Z", status: "Pending" },
    ]);
  };

  const handlePickup = async (packageId: number) => {
    setConfirmingId(packageId);
    try {
      const res = await fetch(`${API}/api/packages/${packageId}/pickup`, {
        method: "POST",
        headers: { Authorization: `Bearer ${token}` },
      });
      if (res.ok) {
        setPackages((prev) =>
          prev.map((p) =>
            p.packageId === packageId
              ? { ...p, status: "PickedUp" as const, pickupDate: new Date().toISOString() }
              : p
          )
        );
      }
    } catch {
      // 降級：直接更新 UI
      setPackages((prev) =>
        prev.map((p) =>
          p.packageId === packageId
            ? { ...p, status: "PickedUp" as const, pickupDate: new Date().toISOString() }
            : p
        )
      );
    } finally {
      setConfirmingId(null);
    }
  };

  const pending = packages.filter((p) => p.status === "Pending");
  const pickedUp = packages.filter((p) => p.status === "PickedUp");

  if (loading) return <div className="text-slate-500 text-sm">載入中...</div>;

  return (
    <div>
      <h2 className="text-2xl font-bold text-slate-800 mb-6">📦 包裹通知</h2>

      {fetchError && (
        <p className="text-sm text-amber-600 bg-amber-50 border border-amber-200 rounded-lg px-4 py-3 mb-4">
          ⚠️ {fetchError}
        </p>
      )}

      {/* 待領取 */}
      <div className="bg-white rounded-xl shadow p-6 mb-6">
        <div className="flex items-center gap-2 mb-4">
          <h3 className="text-base font-semibold text-slate-700">待領取</h3>
          {pending.length > 0 && (
            <span className="text-xs bg-red-100 text-red-600 px-2 py-0.5 rounded-full font-medium">
              {pending.length} 件
            </span>
          )}
        </div>

        {pending.length === 0 ? (
          <p className="text-sm text-slate-400">目前沒有待領取的包裹。</p>
        ) : (
          <div className="space-y-3">
            {pending.map((pkg) => (
              <div
                key={pkg.packageId}
                className="flex items-center justify-between border border-orange-100 rounded-lg px-4 py-3 bg-orange-50"
              >
                <div>
                  <p className="text-sm font-medium text-slate-800">{pkg.carrierName}</p>
                  <p className="text-xs text-slate-500">
                    到達：{new Date(pkg.arrivalDate).toLocaleString("zh-TW")}
                  </p>
                </div>
                <button
                  onClick={() => handlePickup(pkg.packageId)}
                  disabled={confirmingId === pkg.packageId}
                  className="text-sm bg-green-600 text-white px-4 py-1.5 rounded-lg hover:bg-green-700 disabled:opacity-50 transition-colors"
                >
                  {confirmingId === pkg.packageId ? "確認中..." : "確認領取"}
                </button>
              </div>
            ))}
          </div>
        )}
      </div>

      {/* 已領取紀錄 */}
      <div className="bg-white rounded-xl shadow p-6">
        <h3 className="text-base font-semibold text-slate-700 mb-4">已領取紀錄</h3>
        {pickedUp.length === 0 ? (
          <p className="text-sm text-slate-400">尚無領取紀錄。</p>
        ) : (
          <div className="space-y-3">
            {pickedUp.map((pkg) => (
              <div
                key={pkg.packageId}
                className="flex items-center justify-between border border-slate-100 rounded-lg px-4 py-3 bg-slate-50"
              >
                <div>
                  <p className="text-sm font-medium text-slate-800">{pkg.carrierName}</p>
                  <p className="text-xs text-slate-500">
                    到達：{new Date(pkg.arrivalDate).toLocaleString("zh-TW")}
                    {pkg.pickupDate &&
                      ` ／ 領取：${new Date(pkg.pickupDate).toLocaleString("zh-TW")}`}
                  </p>
                </div>
                <span className="text-xs bg-slate-200 text-slate-600 px-2 py-1 rounded-full">
                  {STATUS_LABEL[pkg.status]}
                </span>
              </div>
            ))}
          </div>
        )}
      </div>
    </div>
  );
}
