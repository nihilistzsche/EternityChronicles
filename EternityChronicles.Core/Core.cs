using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Reflection;
using CSLog;
using DragonMUD.Network;
using DragonMUD.Utility;
using ECX.Core;
using ECX.Core.Loader;
using EternityChronicles.Core.Data;
using EternityChronicles.Glue;
using MDK.Master;
using XDL;

[assembly: ModuleRole("Core")]

namespace EternityChronicles.Core
{
    public class Core : ICore
    {
        private static readonly Dictionary<string, AppDomain> LoadedMudlibs = new();

        private readonly List<IGameLoopObject> _gameLoopObjects = new();
        private ModuleController _controller;

        private MDKMaster _dbMaster;

        private int _port = 7000;

        private Server _server;

        private VariableManager _varManager;

        public Tuple<SocketInformation, List<SocketInformation>> DuplicateSockets(int processID)
        {
            var serverSocket = _server.Socket.DuplicateAndClose(processID);

            var clientSockets = new List<SocketInformation>();

            foreach (var client in _server.ConnectionPool.Connections)
            {
                var clientSocket = client.Socket.DuplicateAndClose(processID);
                clientSockets.Add(clientSocket);
            }

            return new Tuple<SocketInformation, List<SocketInformation>>(serverSocket, clientSockets);
        }

        public int GetProcessID()
        {
            return Process.GetCurrentProcess().Id;
        }

        public int Main(bool softReboot, Tuple<SocketInformation, List<SocketInformation>> sockets,
                        params string[] args)
        {
            SetUpPort(args);

            _server = new Server();

            SetUpLog();
            SetUpVariables();
            SetUpModuleController();
            LoadMudLibs();
            LoadData();
            // Workflow

            var ok = InitServer(args, softReboot, sockets);

            if (!ok) return 1;

            SetUpHooks();

            while (_server.IsRunning)
            {
                foreach (var loopObject in _gameLoopObjects)
                    loopObject.OnGameLoop();
            }

            return _server.SoftReboot ? 9999 : 0;
        }

        private void SetUpPort(params string[] args)
        {
            if (!args.Any()) return;
            for (var i = 0; i < args.Count(); i++)
            {
                if (args[i] == "port" && args.Count() > i + 1)
                    _port = int.Parse(args[i + 1]);
            }
        }

        private void SetUpLog()
        {
            Log.RegisterChannel("sys");
            Log.RegisterChannel("ecdata");
            Log.DefaultMaster.AddListener(new ConsoleListener());
        }

        private void SetUpVariables()
        {
            var fileName = $"{Directory.GetCurrentDirectory()}/config/sys.conf";
            // ReSharper disable once UnusedVariable
            _varManager = new VariableManager(fileName);
        }

        private void SetUpModuleController()
        {
            _controller = new ModuleController();
            _controller.SearchPath.Add("lib/sys/bundles");
            _controller.RegisterNewRole("mudlib", typeof(IMudLib), (asm, basetype) =>
                                                                   {
                                                                       var info = _controller.GetInfoForAssembly(asm);
                                                                       if (info == null) return;
                                                                       var domain =
                                                                           _controller.GetAppDomainForAssembly(asm);
                                                                       if (domain == null) return;
                                                                       // Register data loader with Dragon here
                                                                       var baseObject =
                                                                           (IMudLib)asm.CreateInstance(
                                                                            basetype.ToString());
                                                                       if (baseObject == null) return;
                                                                       var methodName = baseObject.GetMudLibMethod();

                                                                       foreach (var method in asm.GetTypes().Select(
                                                                                 type => type.GetMethod(methodName,
                                                                                     BindingFlags.Public |
                                                                                     BindingFlags.NonPublic |
                                                                                     BindingFlags.Static)))
                                                                           method?.Invoke(null, new object[] { });

                                                                       // Load Dragon files in the mudlib here
                                                                       LoadedMudlibs.Add(info.Name, domain);
                                                                   }, asm => { });
            _controller.ModuleLoaded += (sender, args) =>
                                        {
                                            Log.LogMessage("sys", LogLevel.Info,
                                                           "[ECX] Module {0} ({1}) with roles ({2}) loaded.",
                                                           sender.Name,
                                                           sender.Version.ToString(), sender.Roles);
                                        };
        }

        private void LoadMudLibs()
        {
            var modules = _controller.SearchForModulesForRole("mudlib");

            if (!modules.Any())
            {
                Log.LogMessage("sys", LogLevel.Warning,
                               "[WARNING] A mudlib plugin could not be found.  A mudlib is required for the IronDragon bridge to function properly, please install a mudlib plugin into your plugin folder.");
            }
            else
            {
                Log.RegisterChannel("mudlib");
                _controller.LoadAllModules(modules);
            }
        }

        private void LoadData()
        {
            var greetings = ExtensibleDataLoader<Greeting>.LoadFile("lib/greeting.xml", new Greeting());

            Greeting greeting = null;

            if (greetings.Any()) greeting = greetings[0];

            if (greeting != null) _server.ConnectionPool.Greeting = greeting.Text;

            _dbMaster = new MDKMaster("EternityChronicles");
            _dbMaster.StartServer("/var/ec/db");
            _dbMaster.InitializeData();

            var races = _dbMaster.Load<Race>();

            if (races == null || !races.Any() || Race.ForcedBootstrap)

            {
                Log.LogMessage("sys", LogLevel.Info, "Races were not found in the db, loading the bootstrap file.");
                races = ExtensibleDataLoader<Race>.LoadFile("lib/bootstrap/races.xml", new Race());
                if (Race.ForcedBootstrap)
                    _dbMaster.Client.GetDatabase(_dbMaster.DBName).DropCollection(new Race().CollectionName);
                else
                    _dbMaster.Save(races);

                foreach (var race in races) Log.LogMessage("sys", LogLevel.Info, $"{race.Name}({race.Abbreviation})");
            }
            else
            {
                Log.LogMessage("sys", LogLevel.Info, "Races were found in the db.");
                foreach (var race in races) Log.LogMessage("sys", LogLevel.Info, $"{race.Name}({race.Abbreviation})");
            }
        }

        // Workflow

        private bool InitServer(string[] args, bool softReboot,
                                Tuple<SocketInformation, List<SocketInformation>> sockets)
        {
            bool res;
            if (softReboot)
            {
                Log.LogMessage("sys", LogLevel.Info, "Restarting soft reboot socket...");
                res = _server.StartServer(_port, sockets);
            }
            else
            {
                Log.LogMessage("sys", LogLevel.Info, "Starting server on port {0}...\n", _port);
                res = _server.StartServer(_port);
            }

            if (!res)
            {
                Log.LogMessage("sys", LogLevel.Info, "Error starting server, exiting.");
                return false;
            }

            _gameLoopObjects.Add(_server);
            return true;
        }

        public void SetUpHooks()
        {
            _server.ConnectionPool.WriteHooks.Add(new ColorProcessWriteHook());
            // Read hook
        }
    }
}