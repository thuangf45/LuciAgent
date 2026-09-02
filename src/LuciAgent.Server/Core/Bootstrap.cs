using LuciAgent.Server.Database;
using LuciferCore.Attributes;
using LuciferCore.Main;
using Microsoft.EntityFrameworkCore;

namespace LuciAgent.Server.Core;

public static class Bootstrap
{
    [Bootstrap]
    internal static void Initialize()
    {
        Lucifer.SetModelT<DbContext, AppDBContext>();

        Lucifer.SetModelT<DbContextOptions>(() =>
        {
            var optionsBuilder = new DbContextOptionsBuilder<AppDBContext>();
            optionsBuilder.UseSqlServer(DBConfig.GetConnectionString("SqlServer"), sqlOptions =>
            {
                sqlOptions.CommandTimeout(DBConfig.CommandTimeout);
                sqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 5,
                    maxRetryDelay: TimeSpan.FromSeconds(30),
                    errorNumbersToAdd: null);
            });

            return optionsBuilder.Options;
        });

    }
}
