using Microsoft.EntityFrameworkCore;
using SmartCommunityApi.Data;

namespace SmartCommunityApi.Tests.Helpers;

/// <summary>
/// 為每個測試建立獨立的 InMemory 資料庫，避免測試間互相干擾。
/// </summary>
public static class DbContextFactory
{
    public static SmartCommunityDbContext Create(string? dbName = null)
    {
        var options = new DbContextOptionsBuilder<SmartCommunityDbContext>()
            .UseInMemoryDatabase(dbName ?? Guid.NewGuid().ToString())
            .Options;

        return new SmartCommunityDbContext(options);
    }
}
