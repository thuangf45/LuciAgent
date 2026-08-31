using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;

namespace LuciAgent.Server.Database;

public static class EFExtensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IQueryable<T> Page<T, TKey>(
        this IQueryable<T> query,
        int pageIndex,
        int pageSize,
        Expression<Func<T, TKey>> orderBy,
        bool descending = false
        )
    {
        query = descending ? query.OrderByDescending(orderBy) : query.OrderBy(orderBy);

        return query
            .Skip(pageIndex * pageSize)
            .Take(pageSize);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void UpdateNoNull<T>(this DbContext context, T entity) where T : class
    {
        var entry = context.Entry(entity);

        if (entry.State == EntityState.Detached)
        {
            context.Set<T>().Attach(entity);
        }

        foreach (var property in entry.Metadata.GetProperties())
        {
            if (property.IsPrimaryKey()) continue;
            if (property.IsShadowProperty()) continue;

            var propEntry = entry.Property(property.Name);
            if (propEntry.CurrentValue != null)
            {
                propEntry.IsModified = true;
            }
        }
    }
}
