import { useEffect, useState } from "react";
import { useNavigate } from "react-router-dom";
import { useAuth } from "../context/AuthContext";

const API = import.meta.env.VITE_API_URL ?? "http://localhost:5179";

// ── 型別定義 ──────────────────────────────────────────────
interface VoteOptionResult { option: string; count: number; percentage: number; }
interface VoteTopicWithResults {
  topicId: number; title: string; description?: string;
  endTime: string; isExpired: boolean; totalVotes: number;
  options: string[]; results: VoteOptionResult[];
}
interface CreateTopicForm { title: string; description: string; endTime: string; options: string; }

interface UserDto { userId: number; unitNumber: string; userName: string; isAdmin: boolean; }
interface CreateUserForm { unitNumber: string; userName: string; password: string; isAdmin: boolean; }

interface CreatePackageForm { userId: string; carrierName: string; arrivalDate: string; }

type Tab = "vote" | "users" | "packages";

export default function AdminPage() {
  const { user, token } = useAuth();
  const navigate = useNavigate();
  const [activeTab, setActiveTab] = useState<Tab>("vote");

  // ── 投票管理狀態 ──────────────────────────────────────
  const [topics, setTopics] = useState<VoteTopicWithResults[]>([]);
  const [topicsLoading, setTopicsLoading] = useState(true);
  const [topicsFetchError, setTopicsFetchError] = useState("");
  const [form, setForm] = useState<CreateTopicForm>({ title: "", description: "", endTime: "", options: "" });
  const [creating, setCreating] = useState(false);
  const [createMsg, setCreateMsg] = useState("");

  // ── 住戶管理狀態 ──────────────────────────────────────
  const [users, setUsers] = useState<UserDto[]>([]);
  const [usersLoading, setUsersLoading] = useState(false);
  const [usersFetched, setUsersFetched] = useState(false);
  const [usersFetchError, setUsersFetchError] = useState("");
  const [userForm, setUserForm] = useState<CreateUserForm>({ unitNumber: "", userName: "", password: "", isAdmin: false });
  const [creatingUser, setCreatingUser] = useState(false);
  const [createUserMsg, setCreateUserMsg] = useState("");

  // ── 包裹通知狀態 ──────────────────────────────────────
  const [pkgForm, setPkgForm] = useState<CreatePackageForm>({ userId: "", carrierName: "", arrivalDate: "" });
  const [creatingPkg, setCreatingPkg] = useState(false);
  const [createPkgMsg, setCreatePkgMsg] = useState("");

  useEffect(() => {
    if (!user?.isAdmin) { navigate("/vote"); return; }
    fetchTopics();
  }, [user]);

  // 切換頁籤時載入住戶資料
  useEffect(() => {
    if (activeTab === "users" && !usersFetched) fetchUsers();
  }, [activeTab]);

  // ── 投票管理 ──────────────────────────────────────────
  const fetchTopics = async () => {
    setTopicsLoading(true); setTopicsFetchError("");
    try {
      const res = await fetch(`${API}/api/admin/vote-topics`, { headers: { Authorization: `Bearer ${token}` } });
      if (res.ok) setTopics(await res.json());
      else { setTopicsFetchError("載入失敗"); loadMockTopics(); }
    } catch { setTopicsFetchError("後端未連線，顯示模擬資料"); loadMockTopics(); }
    finally { setTopicsLoading(false); }
  };

  const loadMockTopics = () => setTopics([
    { topicId: 1, title: "2026 年度管委會選舉", description: "請投票選出新任管委會成員",
      endTime: "2026-04-01T00:00:00Z", isExpired: false, totalVotes: 42, options: ["候選人甲","候選人乙","候選人丙"],
      results: [{ option:"候選人甲",count:20,percentage:47.6 },{ option:"候選人乙",count:15,percentage:35.7 },{ option:"候選人丙",count:7,percentage:16.7 }] },
    { topicId: 2, title: "停車場改建方案", endTime: "2026-03-20T00:00:00Z", isExpired: true, totalVotes: 88,
      options: ["地下擴建","平面改建","維持現狀"],
      results: [{ option:"地下擴建",count:50,percentage:56.8 },{ option:"平面改建",count:25,percentage:28.4 },{ option:"維持現狀",count:13,percentage:14.8 }] },
  ]);

  const handleCreate = async (e: React.FormEvent) => {
    e.preventDefault(); setCreateMsg("");
    const optionList = form.options.split(",").map(o => o.trim()).filter(Boolean);
    if (optionList.length < 2) { setCreateMsg("請輸入至少 2 個選項（以逗號分隔）"); return; }
    setCreating(true);
    try {
      const res = await fetch(`${API}/api/admin/vote-topics`, {
        method: "POST",
        headers: { "Content-Type": "application/json", Authorization: `Bearer ${token}` },
        body: JSON.stringify({ title: form.title, description: form.description || null, endTime: new Date(form.endTime).toISOString(), options: optionList }),
      });
      if (res.ok) { setCreateMsg("✅ 投票議題已建立"); setForm({ title:"", description:"", endTime:"", options:"" }); fetchTopics(); }
      else setCreateMsg("建立失敗，請重試");
    } catch { setCreateMsg("⚠️ 後端未連線，無法建立"); }
    finally { setCreating(false); }
  };

  // ── 住戶管理 ──────────────────────────────────────────
  const fetchUsers = async () => {
    setUsersLoading(true); setUsersFetchError("");
    try {
      const res = await fetch(`${API}/api/admin/users`, { headers: { Authorization: `Bearer ${token}` } });
      if (res.ok) { setUsers(await res.json()); setUsersFetched(true); }
      else setUsersFetchError("載入失敗，請確認後端服務已啟動");
    } catch { setUsersFetchError("後端未連線"); }
    finally { setUsersLoading(false); }
  };

  const handleCreateUser = async (e: React.FormEvent) => {
    e.preventDefault(); setCreateUserMsg("");
    setCreatingUser(true);
    try {
      const res = await fetch(`${API}/api/admin/users`, {
        method: "POST",
        headers: { "Content-Type": "application/json", Authorization: `Bearer ${token}` },
        body: JSON.stringify({ unitNumber: userForm.unitNumber, userName: userForm.userName, password: userForm.password, isAdmin: userForm.isAdmin }),
      });
      if (res.ok) {
        const dto: UserDto = await res.json();
        setUsers(prev => [...prev, dto]);
        setCreateUserMsg("✅ 住戶已新增");
        setUserForm({ unitNumber:"", userName:"", password:"", isAdmin: false });
      } else {
        const txt = await res.text();
        setCreateUserMsg(`新增失敗：${txt || res.statusText}`);
      }
    } catch { setCreateUserMsg("⚠️ 後端未連線，無法新增"); }
    finally { setCreatingUser(false); }
  };

  // ── 包裹通知 ──────────────────────────────────────────
  const handleCreatePackage = async (e: React.FormEvent) => {
    e.preventDefault(); setCreatePkgMsg("");
    setCreatingPkg(true);
    try {
      const res = await fetch(`${API}/api/admin/packages`, {
        method: "POST",
        headers: { "Content-Type": "application/json", Authorization: `Bearer ${token}` },
        body: JSON.stringify({ userId: Number(pkgForm.userId), carrierName: pkgForm.carrierName, arrivalDate: new Date(pkgForm.arrivalDate).toISOString() }),
      });
      if (res.ok) { setCreatePkgMsg("✅ 包裹通知已建立"); setPkgForm({ userId:"", carrierName:"", arrivalDate:"" }); }
      else setCreatePkgMsg("建立失敗，請重試");
    } catch { setCreatePkgMsg("⚠️ 後端未連線，無法建立"); }
    finally { setCreatingPkg(false); }
  };

  // ── 畫面渲染 ──────────────────────────────────────────
  const TABS: { key: Tab; label: string }[] = [
    { key: "vote",     label: "🗳️ 投票管理" },
    { key: "users",    label: "👥 住戶管理" },
    { key: "packages", label: "📦 包裹通知" },
  ];

  return (
    <div>
      <h2 className="text-2xl font-bold text-slate-800 mb-2">⚙️ 後台管理</h2>
      <p className="text-sm text-slate-500 mb-6">管理投票議題、住戶帳號與包裹通知</p>

      {/* 頁籤 */}
      <div className="flex gap-1 mb-6 border-b border-slate-200">
        {TABS.map(t => (
          <button
            key={t.key}
            onClick={() => setActiveTab(t.key)}
            className={`px-4 py-2 text-sm font-medium rounded-t-lg transition-colors ${
              activeTab === t.key
                ? "bg-white border border-b-white border-slate-200 text-blue-600 -mb-px"
                : "text-slate-500 hover:text-slate-700"
            }`}
          >
            {t.label}
          </button>
        ))}
      </div>

      {/* ── 投票管理頁籤 ── */}
      {activeTab === "vote" && (
        <div>
          {/* 建立投票議題 */}
          <div className="bg-white rounded-xl shadow p-6 mb-8">
            <h3 className="text-base font-semibold text-slate-700 mb-4">新增投票議題</h3>
            <form onSubmit={handleCreate} className="grid grid-cols-1 gap-4 sm:grid-cols-2">
              <div className="sm:col-span-2">
                <label className="block text-sm font-medium text-slate-700 mb-1">議題標題</label>
                <input type="text" required value={form.title} onChange={e => setForm({...form, title: e.target.value})}
                  placeholder="例：2026 年度管委會選舉"
                  className="w-full border border-slate-300 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500" />
              </div>
              <div className="sm:col-span-2">
                <label className="block text-sm font-medium text-slate-700 mb-1">說明（選填）</label>
                <textarea value={form.description} onChange={e => setForm({...form, description: e.target.value})}
                  rows={2} placeholder="投票說明內容..."
                  className="w-full border border-slate-300 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500" />
              </div>
              <div>
                <label className="block text-sm font-medium text-slate-700 mb-1">截止時間</label>
                <input type="datetime-local" required value={form.endTime} onChange={e => setForm({...form, endTime: e.target.value})}
                  className="w-full border border-slate-300 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500" />
              </div>
              <div>
                <label className="block text-sm font-medium text-slate-700 mb-1">投票選項（逗號分隔）</label>
                <input type="text" required value={form.options} onChange={e => setForm({...form, options: e.target.value})}
                  placeholder="贊成, 反對, 棄權"
                  className="w-full border border-slate-300 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500" />
              </div>
              {createMsg && (
                <p className={`sm:col-span-2 text-sm ${createMsg.startsWith("✅") ? "text-green-600" : "text-red-500"}`}>{createMsg}</p>
              )}
              <div>
                <button type="submit" disabled={creating}
                  className="bg-blue-600 text-white px-6 py-2 rounded-lg text-sm font-medium hover:bg-blue-700 disabled:opacity-50 transition-colors">
                  {creating ? "建立中..." : "建立議題"}
                </button>
              </div>
            </form>
          </div>

          {/* 投票統計列表 */}
          {topicsFetchError && (
            <p className="text-sm text-amber-600 bg-amber-50 border border-amber-200 rounded-lg px-4 py-3 mb-4">
              ⚠️ {topicsFetchError}（以下為模擬資料）
            </p>
          )}
          <h3 className="text-base font-semibold text-slate-700 mb-4">投票統計（共 {topics.length} 筆）</h3>
          {topicsLoading ? <div className="text-slate-500 text-sm">載入中...</div> : (
            <div className="space-y-6">
              {topics.map(topic => (
                <div key={topic.topicId} className="bg-white rounded-xl shadow p-6">
                  <div className="flex justify-between items-start mb-4">
                    <div>
                      <h4 className="text-lg font-semibold text-slate-800">{topic.title}</h4>
                      {topic.description && <p className="text-sm text-slate-500 mt-1">{topic.description}</p>}
                      <p className="text-xs text-slate-400 mt-1">截止：{new Date(topic.endTime).toLocaleString("zh-TW")}</p>
                    </div>
                    <div className="text-right ml-4 flex-shrink-0">
                      <span className={`text-xs px-2 py-1 rounded-full ${topic.isExpired ? "bg-slate-100 text-slate-500" : "bg-green-100 text-green-700"}`}>
                        {topic.isExpired ? "已截止" : "進行中"}
                      </span>
                      <p className="text-3xl font-bold text-slate-800 mt-1">{topic.totalVotes}</p>
                      <p className="text-xs text-slate-400">總票數</p>
                    </div>
                  </div>
                  <div className="space-y-3">
                    {topic.results.map(r => (
                      <div key={r.option}>
                        <div className="flex justify-between text-sm mb-1">
                          <span className="text-slate-700 font-medium">{r.option}</span>
                          <span className="text-slate-500 tabular-nums">{r.count} 票（{r.percentage}%）</span>
                        </div>
                        <div className="w-full bg-slate-100 rounded-full h-2.5">
                          <div className="bg-blue-500 h-2.5 rounded-full transition-all duration-500" style={{ width: `${r.percentage}%` }} />
                        </div>
                      </div>
                    ))}
                  </div>
                </div>
              ))}
            </div>
          )}
        </div>
      )}

      {/* ── 住戶管理頁籤 ── */}
      {activeTab === "users" && (
        <div>
          {/* 新增住戶表單 */}
          <div className="bg-white rounded-xl shadow p-6 mb-8">
            <h3 className="text-base font-semibold text-slate-700 mb-4">新增住戶</h3>
            <form onSubmit={handleCreateUser} className="grid grid-cols-1 gap-4 sm:grid-cols-2">
              <div>
                <label className="block text-sm font-medium text-slate-700 mb-1">門牌號碼</label>
                <input type="text" required value={userForm.unitNumber} onChange={e => setUserForm({...userForm, unitNumber: e.target.value})}
                  placeholder="例：A-101"
                  className="w-full border border-slate-300 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500" />
              </div>
              <div>
                <label className="block text-sm font-medium text-slate-700 mb-1">姓名</label>
                <input type="text" required value={userForm.userName} onChange={e => setUserForm({...userForm, userName: e.target.value})}
                  placeholder="例：王小明"
                  className="w-full border border-slate-300 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500" />
              </div>
              <div>
                <label className="block text-sm font-medium text-slate-700 mb-1">初始密碼</label>
                <input type="password" required minLength={6} value={userForm.password} onChange={e => setUserForm({...userForm, password: e.target.value})}
                  placeholder="至少 6 個字元"
                  className="w-full border border-slate-300 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500" />
              </div>
              <div className="flex items-center gap-3 pt-5">
                <input type="checkbox" id="isAdmin" checked={userForm.isAdmin} onChange={e => setUserForm({...userForm, isAdmin: e.target.checked})}
                  className="w-4 h-4 text-blue-600 border-gray-300 rounded" />
                <label htmlFor="isAdmin" className="text-sm font-medium text-slate-700">管理員權限</label>
              </div>
              {createUserMsg && (
                <p className={`sm:col-span-2 text-sm ${createUserMsg.startsWith("✅") ? "text-green-600" : "text-red-500"}`}>{createUserMsg}</p>
              )}
              <div className="sm:col-span-2">
                <button type="submit" disabled={creatingUser}
                  className="bg-blue-600 text-white px-6 py-2 rounded-lg text-sm font-medium hover:bg-blue-700 disabled:opacity-50 transition-colors">
                  {creatingUser ? "新增中..." : "新增住戶"}
                </button>
              </div>
            </form>
          </div>

          {/* 住戶列表 */}
          {usersFetchError && (
            <p className="text-sm text-amber-600 bg-amber-50 border border-amber-200 rounded-lg px-4 py-3 mb-4">⚠️ {usersFetchError}</p>
          )}
          <h3 className="text-base font-semibold text-slate-700 mb-4">住戶列表（共 {users.length} 人）</h3>
          {usersLoading ? <div className="text-slate-500 text-sm">載入中...</div> : (
            users.length === 0 ? (
              <p className="text-sm text-slate-400">尚無住戶資料。</p>
            ) : (
              <div className="bg-white rounded-xl shadow overflow-hidden">
                <table className="w-full text-sm">
                  <thead className="bg-slate-50 text-slate-600 text-xs uppercase">
                    <tr>
                      <th className="px-4 py-3 text-left">門牌</th>
                      <th className="px-4 py-3 text-left">姓名</th>
                      <th className="px-4 py-3 text-left">身分</th>
                      <th className="px-4 py-3 text-left">UID</th>
                    </tr>
                  </thead>
                  <tbody className="divide-y divide-slate-100">
                    {users.map(u => (
                      <tr key={u.userId} className="hover:bg-slate-50">
                        <td className="px-4 py-3 font-medium text-slate-800">{u.unitNumber}</td>
                        <td className="px-4 py-3 text-slate-700">{u.userName}</td>
                        <td className="px-4 py-3">
                          <span className={`text-xs px-2 py-1 rounded-full ${u.isAdmin ? "bg-purple-100 text-purple-700" : "bg-slate-100 text-slate-600"}`}>
                            {u.isAdmin ? "管理員" : "住戶"}
                          </span>
                        </td>
                        <td className="px-4 py-3 text-slate-400 tabular-nums">{u.userId}</td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            )
          )}
        </div>
      )}

      {/* ── 包裹通知頁籤 ── */}
      {activeTab === "packages" && (
        <div>
          <div className="bg-white rounded-xl shadow p-6">
            <h3 className="text-base font-semibold text-slate-700 mb-4">新增包裹到達通知</h3>
            <form onSubmit={handleCreatePackage} className="grid grid-cols-1 gap-4 sm:grid-cols-2">
              <div>
                <label className="block text-sm font-medium text-slate-700 mb-1">住戶 UID</label>
                <input type="number" required min={1} value={pkgForm.userId} onChange={e => setPkgForm({...pkgForm, userId: e.target.value})}
                  placeholder="輸入住戶 UserId"
                  className="w-full border border-slate-300 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500" />
                <p className="text-xs text-slate-400 mt-1">可在「住戶管理」頁籤查閱 UserId</p>
              </div>
              <div>
                <label className="block text-sm font-medium text-slate-700 mb-1">物流公司</label>
                <input type="text" required value={pkgForm.carrierName} onChange={e => setPkgForm({...pkgForm, carrierName: e.target.value})}
                  placeholder="例：黑貓宅急便"
                  className="w-full border border-slate-300 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500" />
              </div>
              <div>
                <label className="block text-sm font-medium text-slate-700 mb-1">到達日期</label>
                <input type="date" required value={pkgForm.arrivalDate} onChange={e => setPkgForm({...pkgForm, arrivalDate: e.target.value})}
                  className="w-full border border-slate-300 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500" />
              </div>
              {createPkgMsg && (
                <p className={`sm:col-span-2 text-sm ${createPkgMsg.startsWith("✅") ? "text-green-600" : "text-red-500"}`}>{createPkgMsg}</p>
              )}
              <div className="sm:col-span-2">
                <button type="submit" disabled={creatingPkg}
                  className="bg-blue-600 text-white px-6 py-2 rounded-lg text-sm font-medium hover:bg-blue-700 disabled:opacity-50 transition-colors">
                  {creatingPkg ? "建立中..." : "建立通知"}
                </button>
              </div>
            </form>
          </div>
        </div>
      )}
    </div>
  );
}
