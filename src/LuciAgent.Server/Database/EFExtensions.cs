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
