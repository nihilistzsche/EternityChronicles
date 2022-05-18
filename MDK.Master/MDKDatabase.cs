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
    public interface IMdkDatabase
    {
    }

    public class MdkDatabase<T> : IMdkDatabase where T : IMdkDataSchema, new()
    {
        public MdkDatabase(MdkMaster master)
        {
            BsonClassMap.RegisterClassMap<T>();
            DbMaster = master;
        }

        public MdkMaster DbMaster { get; }

        public void Save(List<T> objects)
        {
            var db = DbMaster.Client.GetDatabase(DbMaster.DbName);
            var x = new T();
            var collection = db.GetCollection<T>(x.CollectionName);
            collection.InsertMany(objects);
        }

        public List<T> Load()
        {
            var db = DbMaster.Client.GetDatabase(DbMaster.DbName);
            var x = new T();
            var collection = db.GetCollection<T>(x.CollectionName);
            return collection.AsQueryable().ToList();
        }
    }
}