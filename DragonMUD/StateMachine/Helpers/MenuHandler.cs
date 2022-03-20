// MenuHandler.cs in EternityChronicles/DragonMUD
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
using System.Linq;
using System.Reflection;
using CSLog;
using DragonMUD.Network;
using ImpromptuInterface;
using IronDragon.Runtime;

namespace DragonMUD.StateMachine.Helpers
{
    public class MenuHandler
    {
        public MenuHandler(params dynamic[] items) : this("Please make a choice from the following selections:>", items)
        {
        }

        public MenuHandler(string message, params dynamic[] items)
        {
            Message = message;
            Items = items.ToList();
            RealItems = new List<dynamic>();
        }

        public List<dynamic> Items { get; set; }

        private List<dynamic> RealItems { get; }

        public string Message { get; }

        public void DisplayMenu(ConnectionCoordinator coordinator, string sortKey = null)
        {
            if (sortKey != null)
                Items = Items.OrderBy(item =>
                                      {
                                          var memberInfo = item.GetType()
                                                               .GetField(
                                                                         sortKey,
                                                                         BindingFlags.Instance | BindingFlags.Public |
                                                                         BindingFlags.NonPublic);
                                          if (memberInfo != null)
                                              return memberInfo
                                                  .GetValue(item);
                                          return null;
                                      }).ToList();

            coordinator.SendMessage(Message);
            RealItems.Clear();
            for (var i = 1; i < Items.Count(); ++i)
            {
                var item = Items[i];
                RealItems.Add(item);
                if (item is not IMenu)
                {
                    if (item is string)
                    {
                        var m = new string(item);
                        var mParts = m.Split(' ');
                        mParts[0] = mParts[0].Capitalize();
                        m = string.Join(" ", mParts);
                        var anon = new { MenuLine = mParts };
                        Items[i] = anon.ActLike<IMenu>();
                    }
                    else
                    {
                        Log.LogMessage("dragonmud", LogLevel.Info,
                                       "Non-conforming menu item.  Your user will see a broken menu and will not be able to progress.");
                        return;
                    }
                }

                coordinator.SendMessage($"`#`c[`G#{i}`c] `w#{Items[i].MenuLine}`x");
            }

            coordinator.SendMessage("`@");
            coordinator.SendMessage(Items[0].KeyForInfo != null
                                        ? $"Please make your selection (`c1`x - `c#{Items.Count()}`x) or type info <selection> for more information:"
                                        : $"Please make your selection (`c1`x - `c#{Items.Count()}`x):");
        }

        public dynamic GetSelection(ConnectionCoordinator coordinator, string selection, string sortKey = null)
        {
            if (sortKey != null)
                Items = Items.OrderBy(item =>
                                      {
                                          var memberInfo = item.GetType()
                                                               .GetField(
                                                                         sortKey,
                                                                         BindingFlags.Instance | BindingFlags.Public |
                                                                         BindingFlags.NonPublic);
                                          if (memberInfo != null)
                                              return memberInfo
                                                  .GetValue(item);
                                          return null;
                                      }).ToList();
            coordinator.SetFlag("no-message");
            var sel = -1;
            try
            {
                sel = int.Parse(selection);
            }
            catch (FormatException)
            {
            }

            if (sel > Items.Count() || sel < 1)
            {
                if (!selection.StartsWith("info"))
                {
                    coordinator.SendMessage("\n\rInvalid selection.\n\r");
                }
                else
                {
                    var infoMakeup = selection.Split(' ');
                    if (infoMakeup.Count() > 1)
                    {
                        var xsel = -1;
                        try
                        {
                            xsel = int.Parse(infoMakeup[1]);
                        }
                        catch (FormatException)
                        {
                        }

                        if (xsel > Items.Count() || xsel < 1)
                        {
                            coordinator.SendMessage("\n\rInvalid selection.\n\r");
                        }
                        else
                        {
                            var item = RealItems[xsel - 1];
                            if (item.KeyForInfo != null)
                            {
                                var info = item.GetType()
                                               .GetField(item.KeyForInfo,
                                                         BindingFlags.Instance | BindingFlags.Public |
                                                         BindingFlags.NonPublic)
                                               .GetValue(item);
                                coordinator.SendMessage($"\n\r#{info}\n\r");
                            }
                            else
                            {
                                coordinator.SendMessage("\n\rNo info available for given selection.\n\r");
                            }
                        }
                    }
                    else
                    {
                        coordinator.SendMessage("\n\rUsage:  info <num>\n\r");
                    }
                }

                return null;
            }

            coordinator["menu"] = null;
            coordinator.ClearFlag("no-message");
            return RealItems[sel - 1];
        }
    }
}