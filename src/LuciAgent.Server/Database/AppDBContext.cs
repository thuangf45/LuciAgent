// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// * Author:      Nguyen Minh Thuan (thuangf45)
// * License:     AGPL-3.0-only
// * LinkedIn:    https://www.linkedin.com/in/thuangf45
// * NuGet:       https://www.nuget.org/profiles/thuangf45
// * Portfolio:   https://thuangf45.github.io
// * Github:      https://github.com/thuangf45
// * Blog:        https://dev.to/thuangf45
// * Contact:     kingnemacc@gmail.com
// * Copyright (c) 2026 thuangf45. All rights reserved.
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

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
