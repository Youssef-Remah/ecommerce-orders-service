using AutoMapper;
using BusinessLogicLayer.DTOs;
using BusinessLogicLayer.HttpClients;
using BusinessLogicLayer.ServiceInterfaces;
using DataAccessLayer.Entities;
using DataAccessLayer.RepositoryInterfaces;
using FluentValidation;
using FluentValidation.Results;
using MongoDB.Driver;

namespace BusinessLogicLayer.Services
{
    public class OrdersService(
                         IOrdersRepository ordersRepository,
                         IMapper mapper,
                         IValidator<OrderAddRequest> orderAddRequestValidator,
                         IValidator<OrderItemAddRequst> orderItemAddRequestValidator,
                         IValidator<OrderUpdateRequest> orderUpdateRequestValidator,
                         IValidator<OrderItemUpdateRequest> orderItemUpdateRequestValidator,
                         UsersMicroserviceClient usersMicroserviceClient) : IOrdersService
    {
        public async Task<OrderResponse?> AddOrder(OrderAddRequest orderAddRequest)
        {
            ArgumentNullException.ThrowIfNull(orderAddRequest);

            ValidationResult orderAddRequestValidationResult = await orderAddRequestValidator.ValidateAsync(orderAddRequest);
            if (!orderAddRequestValidationResult.IsValid)
            {
                string errors = string.Join(", ", orderAddRequestValidationResult.Errors.Select(temp => temp.ErrorMessage));
                throw new ArgumentException(errors);
            }

            foreach (OrderItemAddRequst orderItemAddRequest in orderAddRequest.OrderItems)
            {
                ValidationResult orderItemAddRequestValidationResult = await orderItemAddRequestValidator.ValidateAsync(orderItemAddRequest);

                if (!orderItemAddRequestValidationResult.IsValid)
                {
                    string errors = string.Join(", ", orderItemAddRequestValidationResult.Errors.Select(temp => temp.ErrorMessage));
                    throw new ArgumentException(errors);
                }
            }

            var user = await usersMicroserviceClient.GetUserByUserId(orderAddRequest.UserId) ?? throw new ArgumentException("Invalid User Id");

            Order orderInput = mapper.Map<Order>(orderAddRequest);

            foreach (OrderItem orderItem in orderInput.OrderItems)
            {
                orderItem.TotalPrice = orderItem.Quantity * orderItem.UnitPrice;
            }
            orderInput.TotalBill = orderInput.OrderItems.Sum(temp => temp.TotalPrice);

            Order? addedOrder = await ordersRepository.AddOrder(orderInput);

            if (addedOrder == null)
            {
                return null;
            }

            OrderResponse addedOrderResponse = mapper.Map<OrderResponse>(addedOrder);

            return addedOrderResponse;
        }

        public async Task<bool> DeleteOrder(Guid orderId)
        {
            FilterDefinition<Order> filter = Builders<Order>.Filter.Eq(temp => temp.OrderId, orderId);
            Order? existingOrder = await ordersRepository.GetOrderByCondition(filter);

            if (existingOrder == null)
            {
                return false;
            }

            bool isDeleted = await ordersRepository.DeleteOrder(orderId);
            return isDeleted;
        }

        public async Task<OrderResponse?> GetOrderByCondition(FilterDefinition<Order> filter)
        {
            Order? order = await ordersRepository.GetOrderByCondition(filter);
            if (order == null)
                return null;

            OrderResponse orderResponse = mapper.Map<OrderResponse>(order);
            return orderResponse;
        }

        public async Task<List<OrderResponse?>> GetOrders()
        {
            IEnumerable<Order?> orders = await ordersRepository.GetOrders();
            IEnumerable<OrderResponse?> orderResponses = mapper.Map<IEnumerable<OrderResponse>>(orders);

            return orderResponses.ToList();
        }

        public async Task<List<OrderResponse?>> GetOrders(FilterDefinition<Order> filter)
        {
            IEnumerable<Order?> orders = await ordersRepository.GetOrders(filter);
            IEnumerable<OrderResponse?> orderResponses = mapper.Map<IEnumerable<OrderResponse>>(orders);

            return orderResponses.ToList();
        }

        public async Task<OrderResponse?> UpdateOrder(OrderUpdateRequest orderUpdateRequest)
        {
            ArgumentNullException.ThrowIfNull(orderUpdateRequest);

            ValidationResult orderUpdateRequestValidationResult = await orderUpdateRequestValidator.ValidateAsync(orderUpdateRequest);
            if (!orderUpdateRequestValidationResult.IsValid)
            {
                string errors = string.Join(", ", orderUpdateRequestValidationResult.Errors.Select(temp => temp.ErrorMessage));
                throw new ArgumentException(errors);
            }

            foreach (OrderItemUpdateRequest orderItemUpdateRequest in orderUpdateRequest.OrderItems)
            {
                ValidationResult orderItemUpdateRequestValidationResult = await orderItemUpdateRequestValidator.ValidateAsync(orderItemUpdateRequest);

                if (!orderItemUpdateRequestValidationResult.IsValid)
                {
                    string errors = string.Join(", ", orderItemUpdateRequestValidationResult.Errors.Select(temp => temp.ErrorMessage));
                    throw new ArgumentException(errors);
                }
            }

            var user = await usersMicroserviceClient.GetUserByUserId(orderUpdateRequest.UserId) ?? throw new ArgumentException("Invalid User Id");

            Order orderInput = mapper.Map<Order>(orderUpdateRequest);

            foreach (OrderItem orderItem in orderInput.OrderItems)
            {
                orderItem.TotalPrice = orderItem.Quantity * orderItem.UnitPrice;
            }
            orderInput.TotalBill = orderInput.OrderItems.Sum(temp => temp.TotalPrice);


            Order? updatedOrder = await ordersRepository.UpdateOrder(orderInput);

            if (updatedOrder == null)
            {
                return null;
            }

            OrderResponse updatedOrderResponse = mapper.Map<OrderResponse>(updatedOrder);

            return updatedOrderResponse;
        }
    }
}
