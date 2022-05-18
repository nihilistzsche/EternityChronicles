// CommandInterpreter.cs in EternityChronicles/DragonMUD
// 
// Copyright (C) 2022 Michael Tindal (nihilistzsche AT gmail DOT com)
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.

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
                           Name = commandAttribute.Name,
                           Method = method,
                           Flags = commandAttribute.Flags,
                           Help =
                           {
                               ["short"] = commandAttribute.ShortHelp,
                               ["long"] = commandAttribute.LongHelp
                           },
                           Owner = logic
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
            var numArgs = command.Method.GetParameters().Length;
            if (!onlyFlagsAndLevel)
            {
                var numOpt = (from p in command.Method.GetParameters() where p.HasDefaultValue select p).Count();
                if (parts.Length < numArgs - numOpt)
                {
                    coordinator.SendMessage($"#{numArgs} arguments expected, #{parts.Length} gotten.");
                    return false;
                }
            }

            var failedFlags = (from flag in command.Flags
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
                if (info.Help["short"] == null || info.Help["short"] == string.Empty)
                {
                    coordinator.SendMessage("Help not available for command.");
                    if (Logic != null && Logic.IsRepeating())
                        Logic.RepeatCommands(coordinator);
                    return;
                }

                coordinator.SendMessage(info.Help["short"]);
                return;
            }

            if (!ValidateInput(info, coordinator, false, parts))
            {
                coordinator.SendMessage("Unknown command entered.");
                return;
            }

            var xargs = new List<object>(parts) { [0] = coordinator };
            if (xargs.Count() > info.Method.GetParameters().Length)
            {
                var zargs = xargs.Skip(info.Method.GetParameters().Length)
                                 .Take(xargs.Count() - info.Method.GetParameters().Length);
                xargs = xargs.Take(info.Method.GetParameters().Length).ToList();
                xargs.Add(string.Join(" ", zargs));
            }

            Dynamic.InvokeMember(info.Owner, info.Method.Name, xargs);
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

            if (info.Help["long"] == null || info.Help["long"] == string.Empty)
            {
                if (info.Help["short"] != null && info.Help["short"] != string.Empty)
                    coordinator.SendMessage(info.Help["short"]);
                else
                    coordinator.SendMessage("Help unavailable for command.");
            }
            else
            {
                using var fs =
                    new FileStream($"$(BundleDir)/lib/help/#{info.Help["long"]}".ReplaceAllVariables(), FileMode.Open,
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
                                        : $"Command #{info.Name}, Optional Arguments: #{(from p in info.Method.GetParameters() where p.HasDefaultValue select p).Count()}, Flags Required: #{string.Join("", "", info.Flags)}");
        }

        public class CommandInfo
        {
            public List<string> Flags;

            public Dictionary<string, string> Help;

            public MethodInfo Method;

            public int MinLevel;
            public string Name;

            public dynamic Owner;
        }
    }
}