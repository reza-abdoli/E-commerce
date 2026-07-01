using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Application.Services.Interface;
using Data.DTOs.Order;
using Data.Entities;
using Data.Entities.Order;
using Data.Entities.User;
using Data.Repository;
using Microsoft.EntityFrameworkCore;

namespace Application.Services.Implementation
{
    public class OrderService : IOrderService
    {
        private readonly IRepository<User> _userRepository;
        private readonly IRepository<Order> _orderRepository;
        private readonly IRepository<OrderDetail> _orderDetailRepository;
        private readonly IRepository<Product> _productRepository;
        private readonly IRepository<ProductVariant> _variantRepository;
        // private readonly IRepository<PaymentRecord> _recordRepository;
        private readonly IRepository<ProductSell> _productSellRepository;


        public OrderService(IRepository<User> userRepository, IRepository<Order> orderRepository, IRepository<OrderDetail> orderDetailRepository, IRepository<Product> productRepository
        , IRepository<ProductVariant> variantRepository, IRepository<ProductSell> productSellRepository)
        {
            _userRepository = userRepository;
            _orderRepository = orderRepository;
            _orderDetailRepository = orderDetailRepository;
            _productRepository = productRepository;
            _variantRepository = variantRepository;
            // _recordRepository = recordRepository;
            _productSellRepository = productSellRepository;
        }

        public async ValueTask DisposeAsync()
        {
            await _userRepository.DisposeAsync();
            await _orderDetailRepository.DisposeAsync();
            await _orderRepository.DisposeAsync();
            await _productRepository.DisposeAsync();
            await _variantRepository.DisposeAsync();
            // await _recordRepository.DisposeAsync();
            await _productSellRepository.DisposeAsync();
        }


        public async Task AddProductToOrder(SubmitOrderDetailDto dto)
        {
            var orderId = await this.GetOrCreateOrderForUser(dto.UserId);
            if (await CheckOrderDetailExists(dto.UserId, dto.ProductVariantId))
            {
                var orderDetail = await _orderDetailRepository.GetQuery()
                .Where(o => o.OrderId == orderId && o.ProductVariantId == dto.ProductVariantId)
                .SingleOrDefaultAsync();
                orderDetail.Count += dto.Count;
                _orderDetailRepository.UpdateEntity(orderDetail);
                await _orderDetailRepository.SaveChanges();
            }
            else
            {
                var product = await _productRepository.GetEntityById(dto.ProductId);
                var newOrderDetail = new OrderDetail
                {
                    OrderId = orderId,
                    ProductVariantId = dto.ProductVariantId,
                    ProductPrice = product.Price,
                    Count = dto.Count,
                    TotalPrice = dto.Count * product.Price
                };
                await _orderDetailRepository.AddEntity(newOrderDetail);
                await _orderDetailRepository.SaveChanges();
            }

        }

        public async Task<bool> CheckOrderDetailExists(long userId, long productVariantId)
        {
            return await _orderRepository.GetQuery().Include(o => o.OrderDetails)
            .Where(o => o.UserId == userId)
            .AnyAsync(o => o.OrderDetails.Any(od => od.ProductVariantId == productVariantId));
        }
        public async Task<long> GetOrCreateOrderForUser(long userId)
        {
            var order = await _orderRepository.GetQuery()
            .Where(o => o.UserId == userId && o.OrderState == OrderState.Open)
            .FirstOrDefaultAsync();
            if (order != null)
            {
                return order.Id;
            }
            var newOrder = new Order
            {
                UserId = userId,
                TotalPrice = 0,
                OrderState = OrderState.Open
            };
            await _orderRepository.AddEntity(newOrder);
            await _orderRepository.SaveChanges();
            return newOrder.Id;
        }

        public async Task ChangeOrderDetailCount(long orderDetailId, int count)
        {
            var orderDetail = await _orderDetailRepository.GetEntityById(orderDetailId);

            if (count == 0)
            {
                _orderDetailRepository.DeleteEntityPermanently(orderDetail);
                await _orderDetailRepository.SaveChanges();
            }
            else
            {
                orderDetail.Count = count;
                _orderDetailRepository.UpdateEntity(orderDetail);
                await _orderDetailRepository.SaveChanges();
            }
        }

        public async Task<OpenOrderDto?> UserOpenOrderDetail(long userId)
        {
            var order = await _orderRepository.GetQuery()
                .FirstOrDefaultAsync(d => d.UserId == userId && d.OrderState == OrderState.Open);
            if (order == null) return null;

            return new OpenOrderDto
            {
                User = await _userRepository.GetEntityById(userId),
                Order = order,
                OrderDetails = await _orderDetailRepository.GetQuery()
                    .Include(d => d.ProductVariant)
                    .ThenInclude(v => v.Product)
                    .Where(d => d.OrderId == order.Id).ToListAsync()
            };
        }

    }
}