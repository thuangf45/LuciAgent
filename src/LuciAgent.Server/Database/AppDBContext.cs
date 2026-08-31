using LuciferCore.Main;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace LuciAgent.Server.Database;

/// <summary>
/// Main DBContext of the application, responsible for managing database connections and entity configurations.
/// </summary>
public class AppDBContext : DbContext
{
    private static IEnumerable<Type>? _cachedEntityTypes;
    public AppDBContext(DbContextOptions options) : base(options) { }

    /// <summary>
    /// Configures the model by scanning for entity types with the TableAttribute 
    /// and applying configurations from the assembly.
    /// </summary>
    /// <param name="modelBuilder"></param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Scan for entity types with the TableAttribute and cache them for performance.
        _cachedEntityTypes ??= Lucifer.GetTypesWithAttribute<TableAttribute>();

        foreach (var type in _cachedEntityTypes)
        {
            modelBuilder.Entity(type);
        }

        // Apply configurations from the assembly containing the AppDBContext class.
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDBContext).Assembly);
    }
}
