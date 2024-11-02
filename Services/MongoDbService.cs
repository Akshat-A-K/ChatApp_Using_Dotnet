using MongoDB.Driver;
using Microsoft.Extensions.Configuration;
using ChatApp.Models;  // Ensure this namespace matches your project

namespace ChatApp.Services
{
    public class MongoDbService
    {
        private readonly IMongoCollection<User> _usersCollection;
        private readonly IMongoCollection<Message> _messagesCollection;

        public MongoDbService(IConfiguration config)
        {
            var client = new MongoClient(config["ConnectionStrings:MongoDbConnection"]);
            var database = client.GetDatabase(config["DatabaseSettings:DatabaseName"]);

            _usersCollection = database.GetCollection<User>("Users");
            _messagesCollection = database.GetCollection<Message>("Messages");
        }

        public async Task<List<User>> GetUsersAsync() => await _usersCollection.Find(_ => true).ToListAsync();
        public async Task<List<Message>> GetMessagesAsync() => await _messagesCollection.Find(_ => true).ToListAsync();
        
        public async Task<User> GetUserByIDAsync(string id)
        {
            return await _usersCollection.Find(user => user.Id == id).FirstOrDefaultAsync();
        }
        public async Task<User> GetUserByUsernameAsync(string username)
        {
            return await _usersCollection.Find(user => user.Username == username).FirstOrDefaultAsync();
        }

        public async Task<List<User>> GetAllUsersExceptAsync(string currentUserId)
        {
            return await _usersCollection.Find(user => user.Id != currentUserId).ToListAsync();
        }
        public async Task CreateUserAsync(User user)
        {
            await _usersCollection.InsertOneAsync(user);
        }

        public async Task<List<Message>> GetMessagesBetweenUsersAsync(string senderId, string receiverId)
        {
            var filter = Builders<Message>.Filter.Or(
                Builders<Message>.Filter.And(
                    Builders<Message>.Filter.Eq(m => m.SenderId, senderId),
                    Builders<Message>.Filter.Eq(m => m.ReceiverId, receiverId)
                ),
                Builders<Message>.Filter.And(
                    Builders<Message>.Filter.Eq(m => m.SenderId, receiverId),
                    Builders<Message>.Filter.Eq(m => m.ReceiverId, senderId)
                )
            );

            return await _messagesCollection.Find(filter).SortBy(m => m.Timestamp).ToListAsync();
        }

        public async Task CreateMessageAsync(Message message)
        {
            await _messagesCollection.InsertOneAsync(message);
        }

        // Add additional methods for creating, updating, and deleting users/messages
    }
}