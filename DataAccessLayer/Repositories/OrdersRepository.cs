using DataAccessLayer.Entities;
using DataAccessLayer.RepositoryInterfaces;
using MongoDB.Driver;

namespace DataAccessLayer.Repositories
{
    public class OrdersRepository : IOrdersRepository
    {
        private readonly IMongoCollection<Order> _orders;
        private readonly string collectionName = "orders";

        public OrdersRepository(IMongoDatabase mongoDatabase)
        {
            _orders = mongoDatabase.GetCollection<Order>(collectionName);
        }

        public async Task<Order?> AddOrder(Order order)
        {
            order.OrderId = Guid.NewGuid();
            order._id = order.OrderId;

            foreach (var orderitem in order.OrderItems)
            {
                orderitem._id = Guid.NewGuid();
            }
            await _orders.InsertOneAsync(order);
            
            return order;
        }

        public async Task<bool> DeleteOrder(Guid id)
        {
            var filter = Builders<Order>.Filter.Eq(temp => temp.OrderId, id);

            var order = (await _orders.FindAsync(filter)).FirstOrDefault();

            if(order == null) return false;

            var result = await _orders.DeleteOneAsync(filter);

            return result.DeletedCount > 0;
        }

        public async Task<Order?> GetOrderByCondition(FilterDefinition<Order> filter)
        {
            return (await _orders.FindAsync(filter)).FirstOrDefault();
        }

        public async Task<IEnumerable<Order>> GetOrders()
        {
            return (await _orders.FindAsync(Builders<Order>.Filter.Empty)).ToList();
        }

        public async Task<IEnumerable<Order?>> GetOrders(FilterDefinition<Order> filter)
        {
            return (await _orders.FindAsync(filter)).ToList();
        }

        public async Task<Order?> UpdateOrder(Order order)
        {
            var filter = Builders<Order>.Filter.Eq(temp => temp.OrderId, order.OrderId);

            var orderToUpdate = (await _orders.FindAsync(filter)).FirstOrDefault();

            if (orderToUpdate == null) return null;

            order._id = orderToUpdate._id;

            var res = await _orders.ReplaceOneAsync(filter, order);

            return order;
        }
    }
}
