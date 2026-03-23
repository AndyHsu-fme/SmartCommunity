import { NavLink } from "react-router-dom";
import { useAuth } from "../context/AuthContext";

const menuItems = [
  { to: "/vote",        label: "數位投票",   icon: "🗳️" },
  { to: "/reservation", label: "公設預約",   icon: "🏊" },
  { to: "/package",     label: "包裹通知",   icon: "📦" },
  { to: "/payment",     label: "管理費支付", icon: "💳" },
];

export default function Sidebar() {
  const { user, logout } = useAuth();

  return (
    <aside className="w-64 min-h-screen bg-slate-800 text-white flex flex-col shadow-lg">
      {/* 標題區 */}
      <div className="p-6 border-b border-slate-700">
        <h1 className="text-xl font-bold tracking-wide">🏢 智慧社區</h1>
        {user ? (
          <div className="mt-2">
            <p className="text-sm font-medium text-white">{user.userName}</p>
            <p className="text-xs text-slate-400">{user.unitNumber}</p>
            {user.isAdmin && (
              <span className="inline-block mt-1 text-xs bg-amber-500 text-white px-2 py-0.5 rounded-full">
                管理員
              </span>
            )}
          </div>
        ) : (
          <p className="text-sm text-slate-400 mt-1">尚未登入</p>
        )}
      </div>

      {/* 導覽選單 */}
      <nav className="flex-1 p-4 space-y-1">
        {menuItems.map((item) => (
          <NavLink
            key={item.to}
            to={item.to}
            className={({ isActive }) =>
              `flex items-center gap-3 px-4 py-3 rounded-lg transition-colors text-sm font-medium ${
                isActive
                  ? "bg-blue-600 text-white"
                  : "text-slate-300 hover:bg-slate-700 hover:text-white"
              }`
            }
          >
            <span className="text-base">{item.icon}</span>
            <span>{item.label}</span>
          </NavLink>
        ))}

        {/* 管理員後台 */}
        {user?.isAdmin && (
          <div className="mt-4 pt-4 border-t border-slate-700">
            <p className="text-xs text-slate-500 px-4 mb-2 uppercase tracking-wider">管理後台</p>
            <NavLink
              to="/admin"
              className={({ isActive }) =>
                `flex items-center gap-3 px-4 py-3 rounded-lg transition-colors text-sm font-medium ${
                  isActive
                    ? "bg-amber-600 text-white"
                    : "text-slate-300 hover:bg-slate-700 hover:text-white"
                }`
              }
            >
              <span className="text-base">⚙️</span>
              <span>後台管理</span>
            </NavLink>
          </div>
        )}
      </nav>

      {/* 底部登出 */}
      <div className="p-4 border-t border-slate-700">
        {user ? (
          <button
            onClick={logout}
            className="w-full text-sm text-slate-400 hover:text-white transition-colors py-2 rounded-lg hover:bg-slate-700"
          >
            登出
          </button>
        ) : (
          <NavLink
            to="/login"
            className="block w-full text-center text-sm text-slate-400 hover:text-white transition-colors py-2 rounded-lg hover:bg-slate-700"
          >
            登入
          </NavLink>
        )}
      </div>
    </aside>
  );
}
