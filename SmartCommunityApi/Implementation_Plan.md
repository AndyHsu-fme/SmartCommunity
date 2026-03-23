智慧社區物聯管理系統 - 實作指令集 (Master Prompt)
1. 專案概要 (Project Context)
本專案為「智慧社區管理系統」，採用增量開發模式。

後端: .NET 8 Web API, C#

資料庫: MS SQL (Entity Framework Core)

前端: React (TypeScript) + Tailwind CSS

架構原則: Repository Pattern, Interface-based, 異步處理 (Async/Await)

2. 核心資料模型實作 (Domain Models)
Copilot 指令: 請根據以下 SQL 結構，在 /Models 資料夾中產生對應的 C# 類別 (Entity Classes)，並設定 EF Core 的 Fluent API 關聯。

資料庫 Schema 定義
Users: UserId (PK), UnitNumber, UserName, PasswordHash, IsAdmin

VoteTopics: TopicId (PK), Title, Description, EndTime

VoteStatus: StatusId (PK), TopicId, UserId, VotedAt (Unique: TopicId + UserId)

AnonymousBallots: BallotId (PK), TopicId, OptionSelected, HashToken (不含 UserId)

Packages: PackageId (PK), UserId, CarrierName, ArrivalDate, PickupDate, Status(Enum)

Facilities: FacilityId (PK), Name, MaxCapacity

FacilityReservations: ReservationId (PK), FacilityId, UserId, StartTime, EndTime, Status

3. 第一階段：匿名投票邏輯 (Core Logic)
Copilot 指令: 請在 /Services/VoteService.cs 實作 CastVoteAsync 方法。

輸入: userId, topicId, option

邏輯:

開啟 IDbContextTransaction。

檢查 VoteStatus 是否已存在該用戶對該主題的紀錄。

若無，在 VoteStatus 新增一筆紀錄（標記已投過）。

在 AnonymousBallots 新增一筆紀錄（僅含選項，斷開與用戶關聯）。

提交事務，確保兩者同時成功。

4. 第二階段：增量功能擴充介面 (Extensibility)
Copilot 指令: 請定義以下 Interface 以利未來擴充功能。

INotificationService: 包含 SendNotification(userId, message)。

IPaymentService: 包含 CreatePaymentOrder(amount, description)。

IReservationService: 包含 CheckAvailability(facilityId, start, end)。

5. 前端功能模組 (Frontend Framework)
Copilot 指令: 請使用 React + Tailwind CSS 建立一個具備 Sidebar 的 Layout 組件。

功能選單: 數位投票、公設預約、包裹通知、管理費支付。

狀態管理: 使用 useState 或 Context API 處理住戶登入狀態。

6. 安全性與防弊規範 (Security Policy)
密碼: 使用 BCrypt 進行雜湊。

驗證: 使用 JWT (JSON Web Token)。

匿名性: 程式碼中嚴禁在 AnonymousBallots 相關的 API 回傳值中洩漏任何 UserId 資訊。