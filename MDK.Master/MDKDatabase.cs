// MDKDatabase.cs in EternityChronicles/MDK.Master
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