using DataAccessLayer.Entities;
using MongoDB.Driver;

namespace DataAccessLayer.RepositoryInterfaces
{
    public interface IOrdersRepository
    {
        Task<IEnumerable<Order>> GetOrders();
        Task<IEnumerable<Order?>> GetOrders(FilterDefinition<Order> filter);
        Task<Order?> GetOrderByCondition(FilterDefinition<Order> filter);
        Task<Order?> AddOrder(Order order);
        Task<Order?> UpdateOrder(Order order);   
        Task<bool> DeleteOrder(Guid id);
    }
}
