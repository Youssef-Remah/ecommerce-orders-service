using BusinessLogicLayer.DTOs;
using DataAccessLayer.Entities;
using MongoDB.Driver;

namespace BusinessLogicLayer.ServiceInterfaces
{
    public interface IOrdersService
    {
        Task<List<OrderResponse?>> GetOrders();
        Task<List<OrderResponse?>> GetOrders(FilterDefinition<Order> filter);
        Task<OrderResponse?> GetOrderByCondition(FilterDefinition<Order> filter);
        Task<OrderResponse?> AddOrder(OrderAddRequest orderAddRequest);
        Task<OrderResponse?> UpdateOrder(OrderUpdateRequest orderUpdateRequest);
        Task<bool> DeleteOrder(Guid orderId);
    }
}
