﻿using Mint.Server.Commands;
using Terraria;
using Terraria.Initializers;
using Terraria.Localization;

namespace Mint.Core;

public static class MintServer
{
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    internal static AssemblyManager AssemblyManager;

    internal static MintConfig Config;
    internal static ConfigUtils ConfigUtils = new ConfigUtils("core");

    public static DatabaseUtils DatabaseUtils { get; private set; }

    public static CommandsManager Commands { get; private set; } = new CommandsManager();
    public static NetworkHandler Network { get; private set; } = new NetworkHandler();
    public static PlayersManager Players { get; private set; } = new PlayersManager();

    public static DatabaseCollection<Account> AccountsCollection { get; private set; }
    public static DatabaseCollection<Group> GroupsCollection { get; private set; }

#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

    static void Main(string[] args)
    {
        AssemblyManager = new AssemblyManager();
        AssemblyManager.SetupResolving();
        AssemblyManager.LoadModules();

        if (!Directory.Exists("data"))
            Directory.CreateDirectory("data");

        Config = ConfigUtils.GetConfig<MintConfig>();
        DatabaseUtils = new DatabaseUtils();

        AccountsCollection = DatabaseUtils.GetDatabase<Account>();       
        GroupsCollection = DatabaseUtils.GetDatabase<Group>();

        Prepare(args, true);

        CoreCommands.Register();
        CoreCommands.Invoke();
        
        Players.Initialize();
        Network.Initialize();

        // TileFix removes caching 
        TileFix.Fix();

        AssemblyManager.InvokeSetup();

        AssemblyManager.InvokeInitialize();
        StartServer();
    }

#region Terraria Server Startup
    static void Prepare(string[] args, bool monoArgs = true)
    {
        // 🌿🌿🌿🌿🌿🌿🌿🌿
        //     |
        //   \ | /    04:20
        //  __\|/__
        // 🌿🌿🌿🌿🌿🌿🌿🌿

        Thread.CurrentThread.Name = "Main Thread";
        if (monoArgs) args = Terraria.Utils.ConvertMonoArgsToDotNet(args);
        Program.LaunchParameters = Terraria.Utils.ParseArguements(args);
        Program.SavePath = Path.Combine("data");
        ThreadPool.SetMinThreads(8, 8);
        Program.InitializeConsoleOutput();
        Program.SetupLogging();
        //Platform.Get<IWindowService>().SetQuickEditEnabled(false);
        Terraria.Main.dedServ = true;
        LanguageManager.Instance.SetLanguage(GameCulture.DefaultCulture);
    }

    static void StartServer()
    {
        using var main = new Terraria.Main();
        Lang.InitializeLegacyLocalization();
        LaunchInitializer.LoadParameters(main);
        main.DedServ();

        main.Run();
    }
#endregion
}