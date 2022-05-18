// MDKMaster.cs in EternityChronicles/MDK.Master
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
using System.Diagnostics;
using System.Linq;
using CSLog;
using MongoDB.Driver;

namespace MDK.Master
{
    /// <summary>
    ///     Provides extensions to <see cref="System.Type" />.
    /// </summary>
    public static class TypeExtensions
    {
        /// <summary>
        ///     Creates a generic instance of a given type.
        /// </summary>
        /// <returns>The generic instance.</returns>
        /// <param name="type">The base type.</param>
        /// <param name="typeArgs">Generic type arguments.</param>
        /// <param name="args">Arguments for the constructor.</param>
        public static object CreateGenericInstance(this Type type, Type[] typeArgs, object[] args)
        {
            var genericTypeDefinition = type.GetGenericTypeDefinition();
            if (genericTypeDefinition == null) throw new ArgumentNullException(nameof(genericTypeDefinition));
            var constructorTypes = new List<Type>();
            args.ToList().ForEach(arg => constructorTypes.Add(arg.GetType()));
            return genericTypeDefinition.MakeGenericType(typeArgs).GetConstructor(constructorTypes.ToArray())
                                        ?.Invoke(args);
        }
    }

    public class MdkMaster
    {
        public MdkMaster(string dbName)
        {
            DbName = dbName;
            Databases = new Dictionary<Type, IMdkDatabase>();
        }

        public MongoClient Client { get; set; }

        public Process Process { get; set; }

        public Dictionary<Type, IMdkDatabase> Databases { get; }

        public string DbName { get; }

        public void StartServer(string dataDir)
        {
            var info = new ProcessStartInfo
                       {
                           FileName = "/usr/bin/mongod",
                           UseShellExecute = false,
                           RedirectStandardOutput = true,
                           Arguments = $"--dbpath \"{dataDir}\""
                       };

            Log.LogMessage("mdk", LogLevel.Info, $"Starting MongoDB server with the data directory: {dataDir}");
            Process = Process.Start(info);
            Client = new MongoClient();
        }

        public void InitializeData()
        {
            var mdkTypes = from a in AppDomain.CurrentDomain.GetAssemblies()
                           from t in a.GetTypes()
                           where t.GetInterface("IMDKDataSchema") != null
                           select t;

            foreach (var type in mdkTypes)
            {
                Databases.Add(type,
                              (IMdkDatabase)typeof(MdkDatabase<>).CreateGenericInstance(
                               new[] { type }, new object[] { this }));
            }
        }

        public List<T> Load<T>() where T : IMdkDataSchema, new()
        {
            var db = Databases[typeof(T)] as MdkDatabase<T>;
            return db?.Load();
        }

        public void Save<T>(List<T> objects) where T : IMdkDataSchema, new()
        {
            var db = Databases[typeof(T)] as MdkDatabase<T>;
            db?.Save(objects);
        }
    }
}