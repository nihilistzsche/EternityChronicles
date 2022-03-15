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