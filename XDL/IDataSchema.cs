// IDataSchema.cs in EternityChronicles/XDL
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

namespace XDL
{
    public delegate object DataLoadHandler(object node);

    public interface IDataSchema<T>
    {
        string DataType { get; }

        string GetKeyForTag(string tag);

        string GetKeyForAttribute(string attribute, string tag);

        DataLoadHandler GetLoadHandlerForKey(string key);
    }
}