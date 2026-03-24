import { useEffect, useState } from "react";
import { useAuth } from "../context/AuthContext";

const API = import.meta.env.VITE_API_URL ?? "http://localhost:5179";

interface VoteTopic {
  topicId: number;
  title: string;
  description?: string;
  endTime: string;
  hasVoted: boolean;
  options: string[];
}

export default function VotePage() {
  const { token } = useAuth();
  const [topics, setTopics] = useState<VoteTopic[]>([]);
  const [loading, setLoading] = useState(true);
  const [fetchError, setFetchError] = useState("");
  const [voted, setVoted] = useState<Record<number, string>>({});
  const [submitting, setSubmitting] = useState<number | null>(null);
  const [message, setMessage] = useState<{ id: number; text: string; ok: boolean } | null>(null);

  useEffect(() => { fetchTopics(); }, [token]);

  const fetchTopics = async () => {
    setLoading(true);
    setFetchError("");
    try {
      const res = await fetch(`${API}/api/votes/topics`, {
        headers: { Authorization: `Bearer ${token}` },
      });
      if (res.ok) {
        setTopics(await res.json());
      } else {
        setFetchError("載入失敗，請確認後端服務已啟動");
        loadMockTopics();
      }
    } catch {
      setFetchError("後端未連線，顯示模擬資料");
      loadMockTopics();
    } finally {
      setLoading(false);
    }
  };

  const loadMockTopics = () => {
    setTopics([
      {
        topicId: 1,
        title: "2026 年度管委會選舉",
        description: "請投票選出新任管委會成員",
        endTime: "2026-04-01T00:00:00Z",
        hasVoted: false,
        options: ["候選人甲", "候選人乙", "候選人丙"],
      },
      {
        topicId: 2,
        title: "社區停車場改建方案",
        description: "討論地下停車場擴建或平面停車場改建",
        endTime: "2026-03-30T00:00:00Z",
        hasVoted: true,
        options: ["地下擴建", "平面改建", "維持現狀"],
      },
    ]);
  };

  const handleVote = async (topicId: number, option: string) => {
    setSubmitting(topicId);
    setMessage(null);
    try {
      const res = await fetch(`${API}/api/votes/cast`, {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
          Authorization: `Bearer ${token}`,
        },
        body: JSON.stringify({ topicId, option }),
      });
      if (res.ok) {
        setVoted((prev) => ({ ...prev, [topicId]: option }));
        setMessage({ id: topicId, text: `✅ 已成功投票：${option}`, ok: true });
      } else {
        const body = await res.json().catch(() => ({}));
        setMessage({ id: topicId, text: (body as { message?: string }).message ?? "投票失敗", ok: false });
      }
    } catch {
      setMessage({ id: topicId, text: "網路連線異常，投票未儲存，請重試", ok: false });
    } finally {
      setSubmitting(null);
    }
  };

  if (loading) {
    return <div className="text-slate-500 text-sm">載入中...</div>;
  }

  return (
    <div>
      <h2 className="text-2xl font-bold text-slate-800 mb-6">🗳️ 數位投票</h2>

      {fetchError && (
        <p className="text-sm text-amber-600 bg-amber-50 border border-amber-200 rounded-lg px-4 py-3 mb-4">
          ⚠️ {fetchError}
        </p>
      )}

      {topics.length === 0 ? (
        <p className="text-slate-500">目前沒有進行中的投票議題。</p>
      ) : (
        <div className="space-y-6">
          {topics.map((topic) => {
            const alreadyVoted = topic.hasVoted || voted[topic.topicId] !== undefined;
            const myChoice = voted[topic.topicId];
            const isExpired = new Date(topic.endTime) < new Date();

            return (
              <div key={topic.topicId} className="bg-white rounded-xl shadow p-6">
                <div className="flex justify-between items-start mb-2">
                  <h3 className="text-lg font-semibold text-slate-800">{topic.title}</h3>
                  {isExpired ? (
                    <span className="text-xs bg-slate-200 text-slate-600 px-2 py-1 rounded-full">
                      已截止
                    </span>
                  ) : (
                    <span className="text-xs bg-green-100 text-green-700 px-2 py-1 rounded-full">
                      進行中
                    </span>
                  )}
                </div>
                {topic.description && (
                  <p className="text-sm text-slate-500 mb-4">{topic.description}</p>
                )}
                <p className="text-xs text-slate-400 mb-4">
                  截止時間：{new Date(topic.endTime).toLocaleString("zh-TW")}
                </p>

                {alreadyVoted ? (
                  <div className="bg-green-50 border border-green-200 rounded-lg px-4 py-3 text-sm text-green-700">
                    ✅ 您已完成投票{myChoice ? `（${myChoice}）` : ""}
                  </div>
                ) : (
                  <div className="flex flex-wrap gap-2">
                    {topic.options.map((opt) => (
                      <button
                        key={opt}
                        onClick={() => handleVote(topic.topicId, opt)}
                        disabled={submitting === topic.topicId || isExpired}
                        className="px-4 py-2 bg-blue-600 text-white rounded-lg text-sm hover:bg-blue-700 disabled:opacity-50 transition-colors"
                      >
                        {submitting === topic.topicId ? "投票中..." : opt}
                      </button>
                    ))}
                  </div>
                )}

                {message?.id === topic.topicId && (
                  <p className={`mt-3 text-sm ${message.ok ? "text-green-600" : "text-red-500"}`}>
                    {message.text}
                  </p>
                )}
              </div>
            );
          })}
        </div>
      )}
    </div>
  );
}
