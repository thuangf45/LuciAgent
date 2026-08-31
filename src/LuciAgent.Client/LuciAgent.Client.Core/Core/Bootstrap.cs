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

using LuciAgent.Client.Core.Contract;
using LuciAgent.Client.Core.Service;
using LuciferCore.Attributes;
using LuciferCore.Main;

namespace LuciAgent.Client.Core.Core;

public static class Bootstrap
{
    [ConsoleCommand("init", "Initialize the app server.")]
    internal static void Initialize()
    {


        Lucifer.SetModelI<IPayment, PayPalPay>();
    }
}
