using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Repository;

namespace RepositoryTests;

[TestClass]
public class GenericRepositoryTests
{
    private static readonly SqliteConnection Connection = new("Data Source=:memory:");

    private readonly DbContext _context;
    private readonly GenericRepository<EntityTest> _repository;

    public GenericRepositoryTests()
    {
        _context = BuildTestDbContext();
        _repository = new GenericRepository<EntityTest>(_context);
    }

    private static TestDbContext BuildTestDbContext()
    {
        if (Connection.State != System.Data.ConnectionState.Open)
        {
            Connection.Open();
        }

        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseSqlite(Connection)
            .Options;

        return new TestDbContext(options);
    }

    [TestInitialize]
    public void Setup()
    {
        _context.Database.EnsureCreated();
    }

    [TestCleanup]
    public void Cleanup()
    {
        _context.Database.EnsureDeleted();
    }

    [TestMethod]
    public void Add_WhenEntityIsAdded_CanBeRetrievedFromDatabase()
    {
        var entity = new EntityTest("Entity1");

        _repository.Add(entity);

        using var otherContext = BuildTestDbContext();
        var entitiesSaved = otherContext.Set<EntityTest>().ToList();

        Assert.AreEqual(1, entitiesSaved.Count);

        var entitySaved = entitiesSaved[0];
        Assert.AreEqual(entity.Id, entitySaved.Id);
        Assert.AreEqual(entity.Name, entitySaved.Name);
    }

    [TestMethod]
    public void GetBy_WhenEntityExists_ReturnsCorrectEntity()
    {
        var entity = new EntityTest("Entity1");

        using var otherContext = BuildTestDbContext();
        otherContext.Set<EntityTest>().Add(entity);
        otherContext.SaveChanges();

        var result = _repository.GetBy(e => e.Id == entity.Id);

        Assert.IsNotNull(result);
        Assert.AreEqual(entity.Id, result.Id);
        Assert.AreEqual(entity.Name, result.Name);
    }

    [TestMethod]
    public void GetBy_WhenEntityDoesNotExist_ReturnsNull()
    {
        var nonExistentId = Guid.NewGuid();

        var result = _repository.GetBy(e => e.Id == nonExistentId);

        Assert.IsNull(result);
    }

    [TestMethod]
    public void GetAllBy_WhenFilterMatches_ReturnsOnlyMatchingEntities()
    {
        using var otherContext = BuildTestDbContext();
        otherContext.Set<EntityTest>().Add(new EntityTest("Entity1"));
        otherContext.Set<EntityTest>().Add(new EntityTest("Entity2"));
        otherContext.Set<EntityTest>().Add(new EntityTest("Other"));
        otherContext.SaveChanges();

        var result = _repository.GetAllBy(e => e.Name.Contains("Entity"));

        Assert.AreEqual(2, result.Count);
        Assert.IsTrue(result.All(e => e.Name.Contains("Entity")));
    }

    [TestMethod]
    public void Update_WhenEntityIsUpdated_ChangesArePersistedInDatabase()
    {
        var entity = new EntityTest("Entity1");

        using var setupContext = BuildTestDbContext();
        setupContext.Set<EntityTest>().Add(entity);
        setupContext.SaveChanges();

        var updated = entity with { Name = "UpdatedEntity" };
        _repository.Update(updated);

        using var verifyContext = BuildTestDbContext();
        var saved = verifyContext.Set<EntityTest>().FirstOrDefault(e => e.Id == entity.Id);

        Assert.IsNotNull(saved);
        Assert.AreEqual("UpdatedEntity", saved.Name);
    }

    [TestMethod]
    public void Delete_WhenEntityIsDeleted_CanNoLongerBeRetrievedFromDatabase()
    {
        var entity = new EntityTest("Entity1");

        using var setupContext = BuildTestDbContext();
        setupContext.Set<EntityTest>().Add(entity);
        setupContext.SaveChanges();

        _repository.Delete(entity);

        using var verifyContext = BuildTestDbContext();
        var saved = verifyContext.Set<EntityTest>().FirstOrDefault(e => e.Id == entity.Id);

        Assert.IsNull(saved);
    }

    [TestMethod]
    public void GetAllBy_WhenPageSizeIsOne_ReturnsOnlyOneEntity()
    {
        using var otherContext = BuildTestDbContext();

        otherContext.Set<EntityTest>().Add(new EntityTest("Entity1"));
        otherContext.Set<EntityTest>().Add(new EntityTest("Entity2"));
        otherContext.Set<EntityTest>().Add(new EntityTest("Entity3"));

        otherContext.SaveChanges();

        var result = _repository.GetAllBy(
            e => e.Name.Contains("Entity"),
            page: 1,
            pageSize: 1);

        Assert.AreEqual(1, result.Count);
    }

    [TestMethod]
    public void GetAllBy_WhenSecondPageIsRequested_SkipsFirstPage()
    {
        using var otherContext = BuildTestDbContext();

        otherContext.Set<EntityTest>().Add(new EntityTest("Entity1"));
        otherContext.Set<EntityTest>().Add(new EntityTest("Entity2"));
        otherContext.Set<EntityTest>().Add(new EntityTest("Entity3"));
        otherContext.SaveChanges();

        var firstPage = _repository.GetAllBy(
            e => e.Name.Contains("Entity"),
            page: 1,
            pageSize: 1);

        var secondPage = _repository.GetAllBy(
            e => e.Name.Contains("Entity"),
            page: 2,
            pageSize: 1);

        Assert.AreEqual(1, firstPage.Count);
        Assert.AreEqual(1, secondPage.Count);
        Assert.AreNotEqual(firstPage[0].Id, secondPage[0].Id);
    }

    [TestMethod]
    public void GetAllBy_WhenPageIsLessThanOne_ThrowsArgumentException()
    {
        Assert.ThrowsException<ArgumentException>(() =>
            _repository.GetAllBy(e => true, page: 0, pageSize: 20));
    }


    [TestMethod]
    public void GetAllBy_WhenPageSizeIsLessThanOne_ThrowsArgumentException()
    {
        Assert.ThrowsException<ArgumentException>(() =>
            _repository.GetAllBy(e => true, page: 1, pageSize: 0));
    }
    internal sealed class TestDbContext(DbContextOptions options) : DbContext(options)
    {
        public DbSet<EntityTest> EntitiesTest { get; set; }
    }

    internal sealed record class EntityTest()
    {
        public Guid Id { get; init; } = Guid.NewGuid();

        public string Name { get; init; } = null!;

        public EntityTest(string name) : this()
        {
            Name = name;
        }
    }
}