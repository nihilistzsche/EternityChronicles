using System.Collections.Generic;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;

namespace MDK.Master
{
	public interface IMDKDatabase
	{
	}

	public class MDKDatabase<T> : IMDKDatabase where T:IMDKDataSchema,new()
	{
		public MDKMaster DBMaster { get; }

		public MDKDatabase(MDKMaster master) {
			BsonClassMap.RegisterClassMap<T>();
			DBMaster = master;
		}

		public void Save(List<T> objects) {
			var db = DBMaster.Client.GetDatabase(DBMaster.DBName);
			var x = new T();
			var collection = db.GetCollection<T>(x.CollectionName);
			collection.InsertMany(objects);
		}

		public List<T> Load()
		{
			var db = DBMaster.Client.GetDatabase(DBMaster.DBName);
			var x = new T();
			var collection = db.GetCollection<T>(x.CollectionName);
			return collection.AsQueryable().ToList();
		}	
	}
}
