using SmartCommunityApi.Models;
using SmartCommunityApi.Tests.Helpers;
using Xunit;

namespace SmartCommunityApi.Tests;

public class UserTests
{
    [Fact]
    public async Task AddUser_ShouldPersistToDatabase()
    {
        // Arrange
        await using var ctx = DbContextFactory.Create();
        var user = new User
        {
            UnitNumber = "A101",
            UserName = "TestUser",
            PasswordHash = "hashed_password",
            IsAdmin = false
        };

        // Act
        ctx.Users.Add(user);
        await ctx.SaveChangesAsync();

        // Assert
        var saved = await ctx.Users.FindAsync(user.UserId);
        Assert.NotNull(saved);
        Assert.Equal("A101", saved.UnitNumber);
        Assert.Equal("TestUser", saved.UserName);
        Assert.False(saved.IsAdmin);
    }

    [Fact]
    public async Task AddAdminUser_ShouldHaveIsAdminTrue()
    {
        // Arrange
        await using var ctx = DbContextFactory.Create();
        var admin = new User
        {
            UnitNumber = "OFFICE",
            UserName = "Admin",
            PasswordHash = "hashed_admin",
            IsAdmin = true
        };

        // Act
        ctx.Users.Add(admin);
        await ctx.SaveChangesAsync();

        // Assert
        var saved = await ctx.Users.FindAsync(admin.UserId);
        Assert.NotNull(saved);
        Assert.True(saved.IsAdmin);
    }

    [Fact]
    public async Task MultipleUsers_ShouldHaveUniqueIds()
    {
        // Arrange
        await using var ctx = DbContextFactory.Create();

        // Act
        ctx.Users.AddRange(
            new User { UnitNumber = "A101", UserName = "User1", PasswordHash = "h1" },
            new User { UnitNumber = "A102", UserName = "User2", PasswordHash = "h2" }
        );
        await ctx.SaveChangesAsync();

        // Assert
        var users = ctx.Users.ToList();
        Assert.Equal(2, users.Count);
        Assert.NotEqual(users[0].UserId, users[1].UserId);
    }
}
