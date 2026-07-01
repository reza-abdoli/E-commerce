using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Data.DTOs.Order;

namespace Application.Services.Interface
{
    public interface IOrderService : IAsyncDisposable
    {
        Task AddProductToOrder(SubmitOrderDetailDto dto);

        Task<long> GetOrCreateOrderForUser(long userId);
        Task<bool> CheckOrderDetailExists(long userId, long productVariantId);
        Task ChangeOrderDetailCount(long orderDetailId, int count);

        Task<OpenOrderDto?> UserOpenOrderDetail(long userId);

    }
}