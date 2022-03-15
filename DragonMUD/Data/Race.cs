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