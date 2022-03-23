// MDKDatabase.cs in EternityChronicles/MDK.Master
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

using System.Collections.Generic;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;

namespace MDK.Master
{
    public interface IMDKDatabase
    {
    }

    public class MDKDatabase<T> : IMDKDatabase where T : IMDKDataSchema, new()
    {
        public MDKDatabase(MDKMaster master)
        {
            BsonClassMap.RegisterClassMap<T>();
            DBMaster = master;
        }

        public MDKMaster DBMaster { get; }

        public void Save(List<T> objects)
        {
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