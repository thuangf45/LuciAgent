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

using LuciAgent.Server.Contract;
using LuciAgent.Server.Database;
using LuciAgent.Server.Service;
using LuciferCore.Attributes;
using LuciferCore.Main;
using Microsoft.EntityFrameworkCore;

namespace LuciAgent.Server.Core;

public static class Bootstrap
{
    [ConsoleCommand("init", "Initialize the app server.")]
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

        Lucifer.SetModelI<IPayment, PayPalPay>();
    }
}
