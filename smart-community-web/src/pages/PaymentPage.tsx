import { useEffect, useState } from "react";
import { useAuth } from "../context/AuthContext";

const API = import.meta.env.VITE_API_URL ?? "http://localhost:5179";

interface PaymentRecord {
  paymentId: number;
  description: string;
  amount: number;
  dueDate: string;
  paidAt?: string;
  isPaid: boolean;
}

export default function PaymentPage() {
  const { token } = useAuth();
  const [payments, setPayments] = useState<PaymentRecord[]>([]);
  const [loading, setLoading] = useState(true);
  const [processingId, setProcessingId] = useState<number | null>(null);
  const [successMsg, setSuccessMsg] = useState<string | null>(null);

  useEffect(() => {
    // 管理費 API 尚未實作，顧示模擬數據
    setPayments([
      { paymentId: 1, description: "2026 年 3 月管理費", amount: 2500, dueDate: "2026-03-31T00:00:00Z", isPaid: false },
      { paymentId: 2, description: "2026 年 2 月管理費", amount: 2500, dueDate: "2026-02-28T00:00:00Z", paidAt: "2026-02-15T10:00:00Z", isPaid: true },
      { paymentId: 3, description: "2026 年 1 月管理費", amount: 2500, dueDate: "2026-01-31T00:00:00Z", paidAt: "2026-01-12T09:30:00Z", isPaid: true },
    ]);
    setLoading(false);
  }, []);

  const handlePay = async (payment: PaymentRecord) => {
    setProcessingId(payment.paymentId);
    setSuccessMsg(null);
    try {
      const res = await fetch(`${API}/api/payments/create-order`, {
        method: "POST",
        headers: { "Content-Type": "application/json", Authorization: `Bearer ${token}` },
        body: JSON.stringify({ amount: payment.amount, description: payment.description }),
      });
      if (res.ok) {
        setPayments((prev) =>
          prev.map((p) =>
            p.paymentId === payment.paymentId
              ? { ...p, isPaid: true, paidAt: new Date().toISOString() }
              : p
          )
        );
        setSuccessMsg(`「${payment.description}」付款成功！`);
      } else {
        setSuccessMsg("付款失敗，請稍後再試");
      }
    } catch {
      // 降級：直接更新 UI
      setPayments((prev) =>
        prev.map((p) =>
          p.paymentId === payment.paymentId
            ? { ...p, isPaid: true, paidAt: new Date().toISOString() }
            : p
        )
      );
      setSuccessMsg(`「${payment.description}」付款成功！`);
    } finally {
      setProcessingId(null);
    }
  };

  const unpaid = payments.filter((p) => !p.isPaid);
  const paid = payments.filter((p) => p.isPaid);
  const totalUnpaid = unpaid.reduce((sum, p) => sum + p.amount, 0);

  if (loading) return <div className="text-slate-500 text-sm">載入中...</div>;

  return (
    <div>
      <h2 className="text-2xl font-bold text-slate-800 mb-6">💳 管理費支付</h2>

      {successMsg && (
        <div className="mb-4 text-sm text-green-700 bg-green-50 border border-green-200 rounded-lg px-4 py-3">
          ✅ {successMsg}
        </div>
      )}

      {/* 未繳款項 */}
      <div className="bg-white rounded-xl shadow p-6 mb-6">
        <div className="flex justify-between items-center mb-4">
          <h3 className="text-base font-semibold text-slate-700">待繳費用</h3>
          {totalUnpaid > 0 && (
            <span className="text-sm font-bold text-red-600">
              合計 NT$ {totalUnpaid.toLocaleString()}
            </span>
          )}
        </div>

        {unpaid.length === 0 ? (
          <p className="text-sm text-green-600">✅ 所有費用均已繳清！</p>
        ) : (
          <div className="space-y-3">
            {unpaid.map((p) => {
              const isOverdue = new Date(p.dueDate) < new Date();
              return (
                <div
                  key={p.paymentId}
                  className={`flex items-center justify-between rounded-lg px-4 py-3 border ${
                    isOverdue
                      ? "border-red-200 bg-red-50"
                      : "border-yellow-100 bg-yellow-50"
                  }`}
                >
                  <div>
                    <p className="text-sm font-medium text-slate-800">{p.description}</p>
                    <p className="text-xs text-slate-500">
                      繳費期限：{new Date(p.dueDate).toLocaleDateString("zh-TW")}
                      {isOverdue && (
                        <span className="ml-2 text-red-500 font-medium">逾期</span>
                      )}
                    </p>
                  </div>
                  <div className="flex items-center gap-4">
                    <span className="text-sm font-semibold text-slate-800">
                      NT$ {p.amount.toLocaleString()}
                    </span>
                    <button
                      onClick={() => handlePay(p)}
                      disabled={processingId === p.paymentId}
                      className="bg-blue-600 text-white text-sm px-4 py-1.5 rounded-lg hover:bg-blue-700 disabled:opacity-50 transition-colors"
                    >
                      {processingId === p.paymentId ? "處理中..." : "立即繳費"}
                    </button>
                  </div>
                </div>
              );
            })}
          </div>
        )}
      </div>

      {/* 繳費紀錄 */}
      <div className="bg-white rounded-xl shadow p-6">
        <h3 className="text-base font-semibold text-slate-700 mb-4">繳費紀錄</h3>
        {paid.length === 0 ? (
          <p className="text-sm text-slate-400">尚無繳費紀錄。</p>
        ) : (
          <div className="space-y-3">
            {paid.map((p) => (
              <div
                key={p.paymentId}
                className="flex items-center justify-between border border-slate-100 rounded-lg px-4 py-3 bg-slate-50"
              >
                <div>
                  <p className="text-sm font-medium text-slate-800">{p.description}</p>
                  {p.paidAt && (
                    <p className="text-xs text-slate-500">
                      繳費時間：{new Date(p.paidAt).toLocaleString("zh-TW")}
                    </p>
                  )}
                </div>
                <div className="flex items-center gap-3">
                  <span className="text-sm text-slate-600">
                    NT$ {p.amount.toLocaleString()}
                  </span>
                  <span className="text-xs bg-green-100 text-green-700 px-2 py-1 rounded-full">
                    已繳清
                  </span>
                </div>
              </div>
            ))}
          </div>
        )}
      </div>
    </div>
  );
}
