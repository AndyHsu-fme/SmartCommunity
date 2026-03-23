# 智慧社區管理系統 — Copilot 工作指令

## 專案概覽

本專案為「智慧社區物聯管理系統」，採增量開發模式。

| 層級 | 技術 |
|---|---|
| 後端 API | ASP.NET Core Web API，**.NET 10**，C# |
| ORM | Entity Framework Core 10.0.5 + SQL Server |
| 前端 | React 19 + TypeScript + Vite 8 + Tailwind CSS 4 |
| 測試 | xUnit + EF Core InMemory |

---

## 目錄結構

```
SmartCommunity/
├── SmartCommunityApi/          # 後端 Web API
│   ├── Controllers/            # AuthController, VoteController, AdminController,
│   │                           # PackageController, ReservationController, PaymentController
│   ├── Services/               # IXxxService 介面 + XxxService 實作（共 7 組）
│   ├── Models/                 # Entity 類別（含 Enums/）
│   ├── DTOs/                   # 請求/回應資料傳輸物件（7 Request + 8 Response）
│   ├── Data/                   # SmartCommunityDbContext (Fluent API)
│   ├── Migrations/             # EF Core Migration（InitialCreate 已建立）
│   ├── Program.cs              # DI 註冊、JWT、CORS、Controllers
│   └── appsettings.json        # Jwt、Admin、HashToken、ConnectionStrings
├── SmartCommunityApi.Tests/    # 單元測試
│   ├── Helpers/DbContextFactory.cs  # 每測試獨立 InMemory DB
│   └── *Tests.cs               # xUnit 測試類別（共 4 檔）
└── smart-community-web/        # 前端 React 專案
    └── src/
        ├── components/         # Layout.tsx、Sidebar.tsx
        ├── pages/              # *Page.tsx（6 頁面，均已串接真實 API）
        ├── context/            # AuthContext.tsx（JWT + localStorage）
        └── App.tsx             # React Router 路由設定
```

---

## 建置與執行指令

### 後端

```bash
# 還原套件並建置
cd SmartCommunityApi
dotnet restore
dotnet build

# 啟動（HTTP: http://localhost:5179）
dotnet run

# 執行測試
cd SmartCommunityApi.Tests
dotnet test

# EF Core Migration（已有 InitialCreate）
# 若 dotnet-ef 未安裝：
dotnet tool install --global dotnet-ef --version 10.0.5
# 新增 migration：
dotnet ef migrations add <MigrationName>
# 套用至資料庫（若 Debug DLL 被鎖，先 build 到暫存目錄）：
dotnet build -o "C:\Temp\SmartCommunityEf"
Copy-Item "C:\Temp\SmartCommunityEf\SmartCommunityApi.dll" "bin\Debug\net10.0\" -Force
dotnet ef database update --no-build
```

### 前端

```bash
cd smart-community-web

# 安裝相依套件
npm install

# 開發伺服器（通常為 http://localhost:5173，port 被占時自動遞增）
# PowerShell 需先執行：Set-ExecutionPolicy -Scope Process -ExecutionPolicy Bypass
npm run dev

# 型別檢查 + 打包
npm run build

# Lint
npm run lint
```

### CORS 注意事項
前端 port 若非 5173（例如 5174），需在 `Program.cs` 的 CORS policy 補充對應 origin，否則 API 呼叫會被瀏覽器封鎖。

---

## 架構原則

1. **Interface-based**：所有 Service 必須先定義 `I` 開頭的 Interface，再實作。
2. **Async/Await**：所有資料庫操作一律使用非同步方法（`XxxAsync`）。
3. **Repository Pattern**：目前 Service 直接注入 DbContext，未來擴充時抽出 Repository 層。
4. **DTO 隔離**：Controller 只接受/回傳 DTO，不直接暴露 Entity Model。
5. **DI 生命週期**：Service 一律以 `AddScoped` 在 `Program.cs` 注冊。

---

## 安全性規範（強制）

- **密碼**：一律使用 `BCrypt.Net.BCrypt.HashPassword()` 雜湊，禁止明文儲存。
- **JWT**：所有需驗證的 API 加上 `[Authorize]` 屬性，管理員功能需額外檢查 `isAdmin` Claim。
- **匿名性（核心）**：
  - `AnonymousBallots` 表刻意無 `UserId` 欄位與外鍵。
  - 所有與 `AnonymousBallots` 相關的 API 回傳值**嚴禁包含任何 UserId 資訊**。
  - 防重複投票由 `VoteStatus` 表的複合唯一索引 `(TopicId, UserId)` 保障，與投票內容完全分離。
- **HashToken**：`AnonymousBallots.HashToken` 由 HMAC-SHA256 單向雜湊產生，無法反查 UserId。

---

## 資料模型核心設計

```
User ──< VoteStatus >── VoteTopic ──< AnonymousBallot（無 UserId）
User ──< Package
User ──< FacilityReservation >── Facility
```

### VoteTopic 特別說明

`VoteTopic.OptionsJson` 欄位存放 JSON 序列化的選項陣列，例如：
```json
["贊成", "反對", "棄權"]
```
使用 `System.Text.Json.JsonSerializer` 序列化/反序列化。

### Enum 儲存方式

`PackageStatus` 與 `ReservationStatus` 在資料庫中以字串儲存（`HasConversion<string>()`）。

---

## 匿名投票核心邏輯（CastVoteAsync）

```
1. db.Database.BeginTransactionAsync()
2. 查 VoteStatus：AnyAsync(TopicId == x && UserId == y)
3. 若已投 → return CastVoteResult.AlreadyVoted（不 throw）
4. 新增 VoteStatus（標記已投）
5. 新增 AnonymousBallot（OptionSelected + HashToken，無 UserId）
6. SaveChangesAsync → CommitAsync
7. catch → RollbackAsync → rethrow
```

---

## API 設計規範

| 路由前綴 | 說明 | 授權 |
|---|---|---|
| `/api/auth/*` | 登入 | 公開 |
| `/api/votes/*` | 住戶投票功能 | `[Authorize]` |
| `/api/packages/*` | 包裹通知 | `[Authorize]` |
| `/api/reservations/*` | 公設預約 | `[Authorize]` |
| `/api/payments/*` | 管理費支付 | `[Authorize]` |
| `/api/admin/*` | 管理後台 | `[Authorize]` + `isAdmin == "true"` Claim |

### 各 Controller 端點清單

#### AuthController `/api/auth`
- `POST /login` — 登入，回傳 JWT token

#### VoteController `/api/votes`
- `GET /topics` — 取得目前有效投票主題（含 HasVoted 狀態）
- `POST /cast` — 投票 `{ topicId, option }`

#### PackageController `/api/packages`
- `GET /` — 取得目前登入住戶的包裹清單
- `POST /{packageId}/pickup` — 標記包裹已領取

#### ReservationController `/api/reservations`
- `GET /facilities` — 取得所有可預約設施
- `GET /availability?facilityId=&start=&end=` — 查詢時段是否可預約
- `GET /` — 取得目前登入住戶的預約紀錄
- `POST /` — 新增預約 `{ facilityId, startTime, endTime }`
- `POST /{reservationId}/cancel` — 取消預約

#### PaymentController `/api/payments`
- `POST /create-order` — 建立支付訂單 `{ amount, description }`，回傳 `orderId`

#### AdminController `/api/admin`（需 `isAdmin == "true"`）
- `GET /vote-topics` — 取得所有投票主題（含結果統計）
- `POST /vote-topics` — 建立新投票主題 `{ title, description, endTime, options[] }`
- `GET /users` — 取得所有住戶
- `POST /users` — 新增住戶 `{ unitNumber, userName, password, isAdmin }`
- `POST /packages` — 新增包裹通知 `{ userId, carrierName, arrivalDate }`

Admin Token 判斷方式：
```csharp
string.Equals(User.FindFirstValue("isAdmin"), "true", StringComparison.OrdinalIgnoreCase)
```

CurrentUserId 取得方式（各 Controller）：
```csharp
private int CurrentUserId => int.TryParse(User.FindFirstValue("sub") ?? User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : 0;
```

---

## 固定 Admin 帳號（開發階段）

| 設定 | 值（位於 appsettings.json） |
|---|---|
| 門牌號碼 | `ADMIN` |
| 密碼 | `Admin@2026` |
| JWT UserId | `0` |

**注意**：固定帳號不存入資料庫，由 `AuthService` 直接比對 `config["Admin:*"]`。

---

## 前端規範

- **API Base URL**：各頁面頂部定義 `const API = "http://localhost:5179"`。
- **Token 傳遞**：`Authorization: Bearer ${token}`（由 `useAuth()` 取得）。
- **AuthContext**：`user` 物件含 `{ userId, userName, unitNumber, isAdmin }`，token 存於 `localStorage`（key：`sc_token`）。
- **Admin 路由保護**：`AdminPage` 以 `user?.isAdmin` 判斷，非管理員自動跳回 `/vote`。
- **Layout 保護**：`Layout.tsx` 未登入時自動跳轉 `/login`。
- **API 失敗退化**：各頁面 API 呼叫失敗時顯示 `⚠️` 警告訊息並回退模擬資料，確保 UI 可預覽。
- **PowerShell 執行策略**：啟動前端前執行 `Set-ExecutionPolicy -Scope Process -ExecutionPolicy Bypass`。
- **CORS 多 port**：前端 port 若因被占用而遞增（如 5174），需在 `Program.cs` CORS policy 加入對應 origin。

### 前端路由清單

| 路徑 | 元件 | 說明 |
|---|---|---|
| `/login` | `LoginPage` | 公開頁面，無 Layout 包裝 |
| `/vote` | `VotePage` | 住戶投票（預設首頁） |
| `/reservation` | `ReservationPage` | 公設預約 |
| `/package` | `PackagePage` | 包裹通知 |
| `/payment` | `PaymentPage` | 管理費支付 |
| `/admin` | `AdminPage` | 管理後台（僅 isAdmin） |

---

## 測試規範

- 每個測試方法使用 `DbContextFactory.Create()`，傳入唯一 GUID 作 DB 名稱，確保測試間隔離。
- 測試使用 `EF Core InMemory`，不依賴 SQL Server。
- 測試案例命名格式：`動作\_狀況描述\_預期結果`（英文 PascalCase）。
- 目前測試覆蓋：`UserTests`、`VoteTests`、`PackageTests`、`FacilityReservationTests`。

---

## 尚待實作

- [ ] EF Core `dotnet ef database update` 套用至 SQL Server（InitialCreate Migration 已建立）
- [ ] Admin 前端頁面加入住戶管理 UI（呼叫 `GET/POST /api/admin/users`）
- [ ] `INotificationService` 完整實作（目前為 stub，僅記錄 log）
- [ ] `IPaymentService` 完整實作（目前為 stub，產生假 OrderId）
- [ ] 管理費紀錄資料庫 Model（目前 PaymentPage 使用模擬資料）
