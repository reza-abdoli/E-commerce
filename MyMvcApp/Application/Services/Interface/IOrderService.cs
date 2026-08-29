using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Data.DTOs.Order;
using Data.Entities;
using Data.Entities.Order;

namespace Application.Services.Interface
{
    public interface IOrderService : IAsyncDisposable
    {
        Task<long> AddProductToOrder(SubmitOrderDetailDto dto);

        Task<long> GetOrCreateOrderForUser(long userId);
        Task<bool> CheckOrderDetailExists(long userId, long productVariantId);
        Task<bool> ChangeOrderDetailCount(long orderDetailId, int count);

        Task<OpenOrderDto?> UserOpenOrderDetail(long userId);
        Task<OrderDetail?> GetCartItemForVariant(long userId, long productVariantId);

        Task UpdatePaidOrder(long orderId, long userId, long paymentTransactionId);
        Task<List<Order>> GetUserPaidOrders(long userId);
        Task<FilterOrderDto> FilterOrders(FilterOrderDto filter);
        Task<OrderDetailDto> OrderDetail(long orderId);

        Task ProcessOrder(long orderId, OrderState orderState);
    }
}