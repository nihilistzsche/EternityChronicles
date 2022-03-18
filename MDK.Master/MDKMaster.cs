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

    public class MDKMaster
    {
        public MDKMaster(string dbName)
        {
            DBName = dbName;
            Databases = new Dictionary<Type, IMDKDatabase>();
        }

        public MongoClient Client { get; set; }

        public Process Process { get; set; }

        public Dictionary<Type, IMDKDatabase> Databases { get; }

        public string DBName { get; }

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
                              (IMDKDatabase)typeof(MDKDatabase<>).CreateGenericInstance(
                               new[] { type }, new object[] { this }));
            }
        }

        public List<T> Load<T>() where T : IMDKDataSchema, new()
        {
            var db = Databases[typeof(T)] as MDKDatabase<T>;
            return db?.Load();
        }

        public void Save<T>(List<T> objects) where T : IMDKDataSchema, new()
        {
            var db = Databases[typeof(T)] as MDKDatabase<T>;
            db?.Save(objects);
        }
    }
}