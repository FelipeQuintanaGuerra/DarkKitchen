using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Repository;

namespace RepositoriesTests;

public abstract class RepositoryTestBase
{
    protected AppDbContext Context = null!;
    private SqliteConnection _connection = null!;

    [TestInitialize]
    public void BaseInitialize()
    {
        _connection = new SqliteConnection("Data Source=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(_connection)
            .Options;

        Context = new AppDbContext(options);
        Context.Database.EnsureCreated();
    }

    [TestCleanup]
    public void BaseCleanup()
    {
        Context.Database.EnsureDeleted();
        Context.Dispose();
        _connection.Dispose();
    }
}