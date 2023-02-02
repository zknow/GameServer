using System;
using System.IO;
using GameServer.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;

namespace GameServer;

// Doc: https://www.rocksaying.tw/archives/2019/dotNET-Core-%E7%AD%86%E8%A8%98-ASP.NET-Core-appsettings.json-%E8%88%87%E5%9F%B7%E8%A1%8C%E7%92%B0%E5%A2%83.html
public class Config
{
    public static AppSettings AppSettings;

    private static IConfiguration cinfig;

    public static void LoadConfig()
    {
        AppSettings = new AppSettings();

        var builder = new ConfigurationBuilder()
        .SetBasePath(Directory.GetCurrentDirectory())
        .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
        cinfig = builder.Build();
        cinfig.Bind(AppSettings);

        ChangeToken.OnChange(cinfig.GetReloadToken, () =>
        {
            cinfig.Bind(AppSettings);
            Console.WriteLine("Configuration changed");
        });
    }

    public static string GetValueFromKey(string key)
    {
        return cinfig.GetSection(key).Value;
    }
}