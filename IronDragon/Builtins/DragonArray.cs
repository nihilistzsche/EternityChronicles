// DragonArray.cs in EternityChronicles/IronDragon
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
using IronDragon.Runtime;

// <copyright file="DragonDictionary.cs" Company="Michael Tindal">
// Copyright 2011-2013 Michael Tindal
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
// -----------------------------------------------------------------------

namespace IronDragon.Builtins
{
    /// <summary>
    ///     TODO: Update summary.
    /// </summary>
    [DragonExport("Array")]
    public class DragonArray : List<dynamic>
    {
        public DragonArray()
        {
        }

        public DragonArray(IEnumerable<dynamic> array) : base(array)
        {
        }

        [DragonExport("<<")]
        public void ArrayAdd(dynamic val)
        {
            Add(val);
        }

        /*
		def to_ary {
			return self
		}
		
		def to_a {
			Array.arrayWithArray(self)
		}
		
		def &(ary) {
			if(!ary.is?(:NSArray)) {
				return self
			}
			res = []
			for(obj in ary) {
				if(self === obj && !(res === obj)) {
					res << obj
				}
			}
			return res
		}
		
		def *(obj) {
			if(obj.is?(:NSNumber)) {
				res = []
				for(i = 0; i < obj; i += 1) {
					res << self
				}
				return res
			} else if(obj.is?(:NSString)) {
				return self.join(obj)
			}
			return nil
		}
		
		def +(ary) {
			if(ary.is?(:NSArray)) {
				return [] << self << ary
			}
			return nil
		}
		
		def -(ary) {
			if(!ary.is?(:NSArray)) {
				return self
			}
			res = []
			for(obj in self) {
				if(!(ary === obj)) {
					res << obj
				}
			}
			return res
		}
		
		def join(str) {
			return self.componentsJoinedByString(str)
		}
		
		def <<(rhs) {
			if(rhs.is?(:NSArray)) {
				self.addObjectsFromArray(rhs)
			} else {
				self.addPossiblyNullObject(rhs)
			}
			return self
		}
		
		def [](i,e=nil) {
			if(e) {
				if(e < 0) {
					e = self.count + e;
				}
				return self[i..e];
			} else {
				if(i.is?(:NSArray)) {
					ret = [];
					for(x = 0; x < i.count; x+=1) {
						ret << self.objectAtIndex(i.objectAtIndex(x));
					};
					return ret;
				} else if(i >= 0) {
					return self.objectAtIndex(i)
				} else {
					return self.objectAtIndex(self.count + i);
				};
			};
		}
		
		def []=(i,v) {
			self.setObject(v,atIndex:i)
				return self.objectAtIndex(i)
		}
		
		def <=>(ary) {
			if(!ary.is?(:NSArray)) {
				return 1
			}
			oal = ary.length > self.length
			for(i = 0; i < (oal ? self.length : ary.length); i += 1) {
				res = self[i] <=> ary[i]
				return res if res
			}
			return 0 if (self.length == ary.length) or 1 if !oal or -1
		}
		
		def ===(rhs) {
			return self.containsObject(rhs);
		}
		
		def each {
			for(obj in self) {
				yield obj;
			};
			return self;
		};
		
		def map {
			results = [];
			for(obj in self) {
				tmp = yield(obj);
				results << tmp if tmp;
			};
			return results;
		};
		
		def mapi {
			results = [];
			for(i = 0; i < self.count; i+=1) {
				tmp = yield(obj,i);
				results << tmp if tmp;
			};
			return results;
		};
		
		def select(&block) {
			selected = [];
			self.each { |x| selected << x if block(x); };
			return selected;
		};
		
		def reduce(block,from:initial) {
			result = initial;
			enumerator = self.objectEnumerator;
			result = block(result,object) while ((object = enumerator.nextObject));
			return result;
		};
		
		def reduceLeft(block,from:initial) {
			result = initial;
			for(i = self.count - 1; i >= 0; i-=1) {
				result = block(result,self[i]);
			};
			return result;
		};
		
		def flatten! {
			flattenAry = ^(ary) {
				flat = [];
				for(obj in ary) {
					if(obj.is?(:NSArray)) {
						for(_obj in flattenAry(obj)) {
							flat << _obj;
						}
					} else {
						flat << obj;
					}
				}
			}
			
			self = flattenAry(self);
		}
		
		def flatten {
			flattenAry = ^(ary) {
				flat = [];
				for(obj in ary) {
					if(obj.is?(:NSArray)) {
						for(_obj in flattenAry(obj)) {
							flat << _obj;
						}
					} else {
						flat << obj;
					}
				}
			}
			
			Array.arrayWithArray(flattenAry(self));
		}
		*/
    }
}