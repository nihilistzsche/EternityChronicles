using XDL;

namespace EternityChronicles.Core.Data
{
    public class Greeting : IDataSchema<Greeting>
    {
        private string _text;

        // ReSharper disable once ConvertToAutoProperty
        public string Text
        {
            get => _text;

            set => _text = value;
        }

        public string DataType => "greeting";

        public string GetKeyForTag(string tag)
        {
            return tag == "text" ? "_text" : null;
        }

        public string GetKeyForAttribute(string attribute, string tag)
        {
            return null;
        }

        public DataLoadHandler GetLoadHandlerForKey(string key)
        {
            return null;
        }
    }
}