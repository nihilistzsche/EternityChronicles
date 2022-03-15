using MongoDB.Bson;

namespace MDK.Master
{
    public interface IMDKDataSchema
    {
        ObjectId ID { get; set; }

        string CollectionName { get; }
    }
}