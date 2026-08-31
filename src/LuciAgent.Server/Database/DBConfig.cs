using LuciferCore.Attributes;
using LuciferCore.Main;
using LuciferCore.Utf8;

namespace LuciAgent.Server.Database;

public static class DBConfig
{
    [Config("DBHost", "localhost")]
    public static string Host { get; set; } = string.Empty;

    [Config("DBPort", "1433")]
    public static int Port { get; set; } = default;

    [Config("DBUser", "sa")]
    public static string User { get; set; } = string.Empty;

    [Config("DBPassword", "svcntt")]
    public static string Password { get; set; } = string.Empty;

    [Config("DBName", "MyShop")]
    public static string Database { get; set; } = string.Empty;

    [Config("DBTrustServerCertificate", "true")]
    public static bool TrustServerCertificate { get; set; } = default;

    [Config("DBConnectTimeout", "15")]
    public static int ConnectTimeout { get; set; } = default;

    [Config("DBCommandTimeout", "30")]
    public static int CommandTimeout { get; set; } = default;

    [Config("DBMaxPoolSize", "100")]
    public static int MaxPoolSize { get; set; } = default;

    [Config("DBAppName", "LuciferServer")]
    public static string ApplicationName { get; set; } = "LuciferServer";

    public static string GetConnectionString(string provider)
    {
        if (string.IsNullOrEmpty(provider))
        {
            return string.Empty;
        }

        using var builder = Lucifer.Rent<Utf8Builder>();

        switch (provider.ToLowerInvariant())
        {
            case "sqlite":
                builder.Append("Data Source="u8).Append<char>(Database).Append(";"u8)
                       .Append("Cache=Shared;"u8)
                       .Append("Mode=ReadWriteCreate;"u8);
                break;

            case "sqlserver":
                builder.Append("Server="u8).Append<char>(Host).Append(","u8).Append(Port).Append(";"u8)
                       .Append("Database="u8).Append<char>(Database).Append(";"u8)
                       .Append("User="u8).Append<char>(User).Append(";"u8)
                       .Append("Password="u8).Append<char>(Password).Append(";"u8)
                       .Append("TrustServerCertificate="u8).Append(TrustServerCertificate).Append(";"u8)
                       .Append("Connect Timeout="u8).Append(ConnectTimeout).Append(";"u8)
                       .Append("Max Pool Size="u8).Append(MaxPoolSize).Append(";"u8)
                       .Append("Application Name="u8).Append<char>(ApplicationName).Append(";"u8);
                break;

            case "postgresql":
                builder.Append("Host="u8).Append<char>(Host).Append(";"u8)
                       .Append("Port="u8).Append(Port).Append(";"u8)
                       .Append("Database="u8).Append<char>(Database).Append(";"u8)
                       .Append("Username="u8).Append<char>(User).Append(";"u8)
                       .Append("Password="u8).Append<char>(Password).Append(";"u8)
                       .Append("Command Timeout="u8).Append(CommandTimeout).Append(";"u8)
                       .Append("Max Pool Size="u8).Append(MaxPoolSize).Append(";"u8)
                       .Append("Application Name="u8).Append<char>(ApplicationName).Append(";"u8);
                break;

            case "mysql":
                builder.Append("Server="u8).Append<char>(Host).Append(";"u8)
                       .Append("Port="u8).Append(Port).Append(";"u8)
                       .Append("Database="u8).Append<char>(Database).Append(";"u8)
                       .Append("User="u8).Append<char>(User).Append(";"u8)
                       .Append("Password="u8).Append<char>(Password).Append(";"u8)
                       .Append("Connection Timeout="u8).Append(ConnectTimeout).Append(";"u8)
                       .Append("Max Pool Size="u8).Append(MaxPoolSize).Append(";"u8)
                       .Append("Application Name="u8).Append<char>(ApplicationName).Append(";"u8);
                break;

            case "oracle":
                builder.Append("Data Source="u8).Append<char>(Host).Append(";"u8)
                       .Append("User Id="u8).Append<char>(User).Append(";"u8)
                       .Append("Password="u8).Append<char>(Password).Append(";"u8)
                       .Append("Connection Timeout="u8).Append(ConnectTimeout).Append(";"u8)
                       .Append("Max Pool Size="u8).Append(MaxPoolSize).Append(";"u8)
                       .Append("Application Name="u8).Append<char>(ApplicationName).Append(";"u8);
                break;

            case "mongodb":
                builder.Append("mongodb://"u8).Append<char>(User).Append(":"u8).Append<char>(Password).Append("@"u8)
                       .Append<char>(Host).Append(":"u8).Append(Port).Append("/"u8)
                       .Append<char>(Database).Append("?"u8)
                       .Append("maxPoolSize="u8).Append(MaxPoolSize).Append("&"u8)
                       .Append("connectTimeoutMS="u8).Append(ConnectTimeout * 1000).Append("&"u8)
                       .Append("appName="u8).Append<char>(ApplicationName);
                break;

            case "redis":
                builder.Append("redis://"u8).Append<char>(Host).Append(":"u8).Append(Port).Append("/"u8)
                       .Append("?password="u8).Append<char>(Password).Append("&"u8)
                       .Append("connectTimeout="u8).Append(ConnectTimeout * 1000).Append("&"u8)
                       .Append("maxPoolSize="u8).Append(MaxPoolSize);
                break;

            default:
                return string.Empty;
        }

        return builder.ToString();
    }
}
