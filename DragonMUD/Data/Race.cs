// Race.cs in EternityChronicles/DragonMUD
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
using CSLog;
using DragonMUD.Data.Character;
using DragonMUD.StateMachine.Helpers;
using IronDragon.Runtime;
using XDL;

namespace DragonMUD.Data
{
    public class Race : BaseObject, IDataStartup, IMenu
    {
        public static readonly Stat.StatLoadType CustomLoadingContext = Stat.StatLoadType.Race;

        private static bool _initialized;

        static Race()
        {
            Races = new List<Race>();
        }

        public static List<Race> Races { get; }

        public static DataManager DataManager { get; private set; }

        public string Name { get; set; }

        public string Abbreviation { get; set; }

        public Stat Bonuses { get; set; }

        public void InitData()
        {
            InitDataInternal();
        }

        public string MenuLine => Name.Capitalize();

        public string KeyForInfo => null;

        public static DataManager SetUpDataManager()
        {
            var rl = new DataManager();
            rl.RegisterAttributesForTag("race", ("name", "Name"), ("abbr", "Abbreviation"));
            rl.RegisterTagForCustomLoading<Stat>("stattemplate", "Bonuses", CustomLoadingContext);
            return rl;
        }

        public static Race LoadRaceWithPath(string path)
        {
            DataManager ??= SetUpDataManager();

            var race = new Race();

            DataManager.LoadFromPath(path, race);

            return race;
        }

        public static Race GetRaceByName(string raceName)
        {
            return Races.FirstOrDefault(race =>
                                            string.Equals(race.Name, raceName,
                                                          StringComparison.CurrentCultureIgnoreCase) ||
                                            string.Equals(race.Abbreviation, raceName,
                                                          StringComparison.CurrentCultureIgnoreCase));
        }

        private static void InitDataInternal()
        {
            if (_initialized)
                return;

            var racesToLoad = Directory.GetFiles("$(KMRaceSourceDir)".ReplaceAllVariables());

            if (!racesToLoad.Any())
                return;

            foreach (var raceToLoad in racesToLoad)
            {
                if (Path.GetExtension(raceToLoad) != ".xml") continue;
                var race = LoadRaceWithPath($"$(KMRaceSourceDir){Path.DirectorySeparatorChar}{raceToLoad}"
                                                .ReplaceAllVariables());

                if (race.Name == null) continue;
                Log.LogMessage("dragonmud", LogLevel.Info,
                               $"Adding race {race.Name}({race.Abbreviation}) to list of races.");
                Races.Add(race);
            }

            _initialized = true;
        }
    }
}