// CommandInterpreter.cs
// 
// Copyright (C) 2022 Michael Tindal (nihilistzsche AT gmail DOT com)
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//    http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using CSLog;
using DragonMUD.Interpreters.Logic;
using DragonMUD.Network;
using Dynamitey;
using XDL;

namespace DragonMUD.Interpreters
{
    public class CommandInterpreter : BaseInterpreter
    {
        public CommandInterpreter()
        {
            AdditionalLogics = new List<CommandInterpreterLogic>();
            RegisterLogicInternal(this);
        }

        public CommandInterpreterLogic Logic { get; private set; }

        public List<CommandInterpreterLogic> AdditionalLogics { get; }

        public Dictionary<string, CommandInfo> RegisteredCommands { get; private set; }

        private static bool VerifyMethodSignature(MethodInfo method)
        {
            var args = method.GetParameters();
            if (!args.Any())
                return false;
            return args[0].ParameterType == typeof(ConnectionCoordinator);
        }

        public void Test(out int x)
        {
            x = 17;
        }

        private void RegisterCommand(dynamic logic, MethodInfo method)
        {
            if (!VerifyMethodSignature(method))
            {
                Log.LogMessage("dragonmud", LogLevel.Debug,
                               $"Method #{method.Name} from logic {logic.GetType().Name} does not have a valid signature, and will not be registered.  DragonMUD.Network.ConnectionCoordinator must always be the first argument type.");
                return;
            }

            var commandAttribute = method.GetCustomAttribute<CommandAttribute>();

            var info = new CommandInfo
                       {
                           name = commandAttribute.Name,
                           method = method,
                           flags = commandAttribute.Flags,
                           help =
                           {
                               ["short"] = commandAttribute.ShortHelp,
                               ["long"] = commandAttribute.LongHelp
                           },
                           owner = logic
                       };

            RegisteredCommands.Add(commandAttribute.Name, info);
            commandAttribute.Aliases.ForEach(alias => { RegisteredCommands.Add(alias, info); });
        }

        protected void RegisterLogicInternal(object logic, bool replaceDefault = false)
        {
            if (logic is CommandInterpreterLogic logic1)
            {
                if (replaceDefault)
                {
                    if (Logic != null && !AdditionalLogics.Contains(Logic)) AdditionalLogics.Add(Logic);

                    if (AdditionalLogics.Contains(logic1)) AdditionalLogics.Remove(logic1);

                    Logic = logic1;
                    if (Logic != null) Logic.Interpreter = this;
                }
                else
                {
                    if (!AdditionalLogics.Contains(logic1)) AdditionalLogics.Add(logic1);
                }
            }

            var commandMethods =
                from method in logic.GetType()
                                    .GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                where method.GetCustomAttribute<CommandAttribute>() != null
                select method;

            foreach (var commandMethod in commandMethods) RegisterCommand(logic, commandMethod);
        }

        private bool ValidateInput(CommandInfo command, ConnectionCoordinator coordinator, bool onlyFlagsAndLevel,
                                   string[] parts)
        {
            var numArgs = command.method.GetParameters().Length;
            if (!onlyFlagsAndLevel)
            {
                var numOpt = (from p in command.method.GetParameters() where p.HasDefaultValue select p).Count();
                if (parts.Length < numArgs - numOpt)
                {
                    coordinator.SendMessage($"#{numArgs} arguments expected, #{parts.Length} gotten.");
                    return false;
                }
            }

            var failedFlags = (from flag in command.flags
                               where !coordinator.IsFlagSet(flag) || !coordinator["current-character"]
                                         ?.IsFlagSet(flag)
                               select flag).Any();

            if (failedFlags)
                return false;

            // Check character here
            return true;
        }

        private CommandInfo FindCommand(string name)
        {
            return (from kvp in RegisteredCommands
                    where kvp.Key.StartsWith(name, StringComparison.InvariantCulture)
                    select kvp.Value).First();
        }

        public override void Interpret(ConnectionCoordinator coordinator, InputEventArgs input)
        {
            var regex = new Regex(@"\s");
            var parts = regex.Split(input.Input.ToLower());
            var info = FindCommand(parts[0].ToLower());

            if (info == null)
            {
                coordinator.SendMessage("Unknown command entered.");
                return;
            }

            if (parts.Length > 1 && parts[1] == "-help" && ValidateInput(info, coordinator, true, parts))
            {
                if (info.help["short"] == null || info.help["short"] == string.Empty)
                {
                    coordinator.SendMessage("Help not available for command.");
                    if (Logic != null && Logic.IsRepeating())
                        Logic.RepeatCommands(coordinator);
                    return;
                }

                coordinator.SendMessage(info.help["short"]);
                return;
            }

            if (!ValidateInput(info, coordinator, false, parts))
            {
                coordinator.SendMessage("Unknown command entered.");
                return;
            }

            var xargs = new List<object>(parts) { [0] = coordinator };
            if (xargs.Count() > info.method.GetParameters().Length)
            {
                var zargs = xargs.Skip(info.method.GetParameters().Length)
                                 .Take(xargs.Count() - info.method.GetParameters().Length);
                xargs = xargs.Take(info.method.GetParameters().Length).ToList();
                xargs.Add(string.Join(" ", zargs));
            }

            Dynamic.InvokeMember(info.owner, info.method.Name, xargs);
        }

        [Command("help", "", 1, "", "Displays long help for the given command.")]
        public void HelpCommand(ConnectionCoordinator coordinator, string command = null)
        {
            if (command == null)
            {
                Logic?.DisplayHelp(coordinator);
                return;
            }

            var info = FindCommand(command);
            if (info == null || !ValidateInput(info, coordinator, true, new string[] { }))
            {
                coordinator.SendMessage("Unknown command.");
                return;
            }

            if (info.help["long"] == null || info.help["long"] == string.Empty)
            {
                if (info.help["short"] != null && info.help["short"] != string.Empty)
                    coordinator.SendMessage(info.help["short"]);
                else
                    coordinator.SendMessage("Help unavailable for command.");
            }
            else
            {
                using var fs =
                    new FileStream($"$(BundleDir)/lib/help/#{info.help["long"]}".ReplaceAllVariables(), FileMode.Open,
                                   FileAccess.Read);
                var bytes = new byte[fs.Length];
                fs.Read(bytes, 0, (int)fs.Length);
                coordinator.SendMessage(Encoding.UTF8.GetString(bytes));
            }
        }

        [Command("displaycommand", "", 1, "staff")]
        public void DisplayCommandCommand(ConnectionCoordinator coordinator, string command)
        {
            var info = FindCommand(command);
            coordinator.SendMessage(info == null
                                        ? "No command found."
                                        : $"Command #{info.name}, Optional Arguments: #{(from p in info.method.GetParameters() where p.HasDefaultValue select p).Count()}, Flags Required: #{string.Join("", "", info.flags)}");
        }

        public class CommandInfo
        {
            public List<string> flags;

            public Dictionary<string, string> help;

            public MethodInfo method;

            public int minLevel;
            public string name;

            public dynamic owner;
        }
    }
}