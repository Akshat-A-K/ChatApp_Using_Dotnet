using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ChatApp.Models
{
    public class Message
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = null!;

        [BsonElement("SenderId")]
        public string SenderId { get; set; } = null!;

        [BsonElement("ReceiverId")]
        public string ReceiverId { get; set; } = null!;

        [BsonElement("MessageContent")]
        public string MessageContent { get; set; } = null!;

        [BsonElement("Timestamp")]
        public DateTime Timestamp { get; set; } = DateTime.Now;
    }
}
