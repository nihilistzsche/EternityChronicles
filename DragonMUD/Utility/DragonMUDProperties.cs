using System.Collections.Generic;
using System.Linq;

namespace DragonMUD.Utility
{
	public class DragonMUDProperties
	{
		public readonly Dictionary<string, dynamic> Properties = new();

		private DragonMUDProperties GetPropertyDictFromName(string name)
		{
			return Properties[name] as DragonMUDProperties ?? new DragonMUDProperties();
		}

		public dynamic this[string propertyPath]
		{
			get
			{
				if (propertyPath == string.Empty)
					return null;

				var propertyNames = propertyPath.Split('.').ToList();

				if (propertyNames.Count() == 1)
					return Properties[propertyNames[0]];

				var dict = GetPropertyDictFromName(propertyNames[0]);

				Properties[propertyNames[0]] = dict;

				propertyNames.RemoveAt(0);

				propertyPath = string.Join(".", propertyNames);

				return dict[propertyPath];
			}
			set
			{
				if (propertyPath == string.Empty)
					return;

				var propertyNames = propertyPath.Split('.').ToList();

				if (propertyNames.Count() == 1)
					Properties[propertyNames[0]] = value;

				var dict = GetPropertyDictFromName(propertyNames[0]);

				Properties[propertyNames[0]] = dict;

				propertyNames.RemoveAt(0);

				propertyPath = string.Join(".", propertyNames);

				dict[propertyPath] = value;
			}
		}
	}
}
