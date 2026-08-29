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
using Microsoft.Extensions.Logging;

namespace Application.Services.Implementation
{
    public class OrderService : IOrderService
    {
        private readonly IRepository<User> _userRepository;
        private readonly IRepository<Order> _orderRepository;
        private readonly IRepository<OrderDetail> _orderDetailRepository;
        private readonly IRepository<Product> _productRepository;
        private readonly IRepository<ProductVariant> _variantRepository;
        private readonly IRepository<ProductSell> _productSellRepository;
        private readonly IRepository<PaymentTransaction> _paymentTransactionRepository;
        private readonly ILogger<OrderService> _logger;

        public OrderService(
            IRepository<User> userRepository,
            IRepository<Order> orderRepository,
            IRepository<OrderDetail> orderDetailRepository,
            IRepository<Product> productRepository,
            IRepository<ProductVariant> variantRepository,
            IRepository<ProductSell> productSellRepository,
            IRepository<PaymentTransaction> paymentTransactionRepository,
            ILogger<OrderService> logger)
        {
            _userRepository = userRepository;
            _orderRepository = orderRepository;
            _orderDetailRepository = orderDetailRepository;
            _productRepository = productRepository;
            _variantRepository = variantRepository;
            _productSellRepository = productSellRepository;
            _paymentTransactionRepository = paymentTransactionRepository;
            _logger = logger;
        }

        public async ValueTask DisposeAsync()
        {
            await _userRepository.DisposeAsync();
            await _orderDetailRepository.DisposeAsync();
            await _orderRepository.DisposeAsync();
            await _productRepository.DisposeAsync();
            await _variantRepository.DisposeAsync();
            await _productSellRepository.DisposeAsync();
            await _paymentTransactionRepository.DisposeAsync();
        }

        public async Task<long> AddProductToOrder(SubmitOrderDetailDto dto)
        {
            var orderId = await this.GetOrCreateOrderForUser(dto.UserId);
            var variantCount = await _variantRepository.GetQuery()
                .Where(v => v.Id == dto.ProductVariantId)
                .Select(v => v.StockCount)
                .SingleOrDefaultAsync();

            if (!(variantCount > 0))
            {
                _logger.LogWarning("Add to order failed: no stock. UserId: {UserId}, ProductVariantId: {ProductVariantId}", dto.UserId, dto.ProductVariantId);
                return 0;
            }

            if (await CheckOrderDetailExists(dto.UserId, dto.ProductVariantId))
            {
                var orderDetail = await _orderDetailRepository.GetQuery()
                    .Where(o => o.OrderId == orderId && o.ProductVariantId == dto.ProductVariantId)
                    .SingleOrDefaultAsync();

                orderDetail.Count = 1;
                _orderDetailRepository.UpdateEntity(orderDetail);
                await _orderDetailRepository.SaveChanges();

                _logger.LogInformation("Order detail updated. OrderDetailId: {OrderDetailId}, OrderId: {OrderId}, ProductVariantId: {ProductVariantId}", orderDetail.Id, orderId, dto.ProductVariantId);
                return orderDetail.Id;
            }
            else
            {
                var product = await _productRepository.GetEntityById(dto.ProductId);
                var newOrderDetail = new OrderDetail
                {
                    OrderId = orderId,
                    ProductVariantId = dto.ProductVariantId,
                    ProductPrice = product.Price,
                    Count = 1,
                    TotalPrice = product.Price
                };
                await _orderDetailRepository.AddEntity(newOrderDetail);
                await _orderDetailRepository.SaveChanges();

                _logger.LogInformation("Product added to order. OrderDetailId: {OrderDetailId}, OrderId: {OrderId}, ProductId: {ProductId}, ProductVariantId: {ProductVariantId}", newOrderDetail.Id, orderId, dto.ProductId, dto.ProductVariantId);
                return newOrderDetail.Id;
            }
        }

        public async Task<bool> CheckOrderDetailExists(long userId, long productVariantId)
        {
            return await _orderRepository.GetQuery()
                .Where(o => o.UserId == userId && o.OrderState == OrderState.Open)
                .Include(o => o.OrderDetails)
                .AnyAsync(o => o.OrderDetails.Any(od => od.ProductVariantId == productVariantId));
        }

        public async Task<long> GetOrCreateOrderForUser(long userId)
        {
            var order = await _orderRepository.GetQuery()
                .Where(o => o.UserId == userId && o.OrderState == OrderState.Open)
                .FirstOrDefaultAsync();

            if (order != null) return order.Id;

            var newOrder = new Order
            {
                UserId = userId,
                TotalPrice = 0,
                OrderState = OrderState.Open
            };
            await _orderRepository.AddEntity(newOrder);
            await _orderRepository.SaveChanges();

            _logger.LogInformation("New open order created. OrderId: {OrderId}, UserId: {UserId}", newOrder.Id, userId);
            return newOrder.Id;
        }

        public async Task<bool> ChangeOrderDetailCount(long orderDetailId, int count)
        {
            var orderDetail = await _orderDetailRepository.GetEntityById(orderDetailId);

            if (count == 0)
            {
                _orderDetailRepository.DeleteEntityPermanently(orderDetail);
                await _orderDetailRepository.SaveChanges();

                _logger.LogInformation("Order detail removed. OrderDetailId: {OrderDetailId}", orderDetailId);
            }
            else
            {
                var variantCount = await _orderDetailRepository.GetQuery()
                    .Include(o => o.ProductVariant)
                    .Where(o => o.Id == orderDetailId)
                    .Select(o => o.ProductVariant.StockCount)
                    .SingleOrDefaultAsync();

                if (variantCount < count)
                {
                    _logger.LogWarning("Order detail count change failed: not enough stock. OrderDetailId: {OrderDetailId}, RequestedCount: {RequestedCount}, StockCount: {StockCount}", orderDetailId, count, variantCount);
                    return false;
                }

                var oldCount = orderDetail.Count;
                orderDetail.Count = count;
                orderDetail.TotalPrice = orderDetail.ProductPrice * count;
                _orderDetailRepository.UpdateEntity(orderDetail);
                await _orderDetailRepository.SaveChanges();

                _logger.LogInformation("Order detail count changed. OrderDetailId: {OrderDetailId}, OldCount: {OldCount}, NewCount: {NewCount}", orderDetailId, oldCount, count);
            }
            return true;
        }

        public async Task<OrderDetail?> GetCartItemForVariant(long userId, long productVariantId)
        {
            var order = await _orderRepository.GetQuery()
                .Where(o => o.UserId == userId && o.OrderState == OrderState.Open)
                .FirstOrDefaultAsync();

            if (order == null) return null;

            return await _orderDetailRepository.GetQuery()
                .Where(d => d.OrderId == order.Id && d.ProductVariantId == productVariantId)
                .FirstOrDefaultAsync();
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
                    .Where(d => d.OrderId == order.Id)
                    .ToListAsync()
            };
        }

        public async Task UpdatePaidOrder(long orderId, long userId, long paymentTransactionId)
        {
            var order = await _orderRepository.GetQuery()
                .Include(o => o.OrderDetails)
                    .ThenInclude(d => d.ProductVariant)
                        .ThenInclude(v => v.Product)
                .FirstOrDefaultAsync(o => o.Id == orderId && o.UserId == userId);

            if (order == null)
            {
                throw new InvalidOperationException($"Order with ID {orderId} for User {userId} was not found.");
            }

            var now = DateTime.UtcNow;

            foreach (var item in order.OrderDetails)
            {
                var currentProductPrice = item.ProductVariant.Product.Price;

                item.ProductPrice = currentProductPrice;
                item.TotalPrice = currentProductPrice * item.Count;
                _orderDetailRepository.UpdateEntity(item);

                await _productSellRepository.AddEntity(new ProductSell
                {
                    ProductId = item.ProductVariant.ProductId,
                    OrderId = order.Id,
                    SellCount = item.Count,
                    SellDate = now,
                    ProductPrice = currentProductPrice
                });

                // Atomic stock deduction to prevent race conditions
                int affectedRows = await _variantRepository.GetQuery()
                    .Where(v => v.Id == item.ProductVariantId && v.StockCount >= item.Count)
                    .ExecuteUpdateAsync(setters => setters
                        .SetProperty(v => v.StockCount, v => v.StockCount - item.Count));

                if (affectedRows == 0)
                {
                    throw new InvalidOperationException($"Insufficient stock for product '{item.ProductVariant.Product.Title}'.");
                }
            }

            var user = await _userRepository.GetEntityById(userId);
            if (user != null)
            {
                order.Address = user.Address;
                order.PostCode = user.PostCode;
                order.DestinationCity = user.UserCity;
                order.UserName = user.FullName;
            }

            order.OrderState = OrderState.Paid;
            order.PaymentDate = now;
            order.PaymentTransactionId = paymentTransactionId;
            order.TotalPrice = order.OrderDetails.Sum(item => item.ProductPrice * item.Count);

            _orderRepository.UpdateEntity(order);
            await _orderRepository.SaveChanges();
        }

        public async Task<List<Order>> GetUserPaidOrders(long userId)
        {
            return await _orderRepository.GetQuery()
                .Where(o => o.UserId == userId && o.OrderState == OrderState.Paid)
                .Include(o => o.OrderDetails)
                    .ThenInclude(d => d.ProductVariant)
                        .ThenInclude(v => v.Product)
                .ToListAsync();
        }

        public async Task<FilterOrderDto> FilterOrders(FilterOrderDto filter)
        {
            var query = _orderRepository.GetQuery()
                .Include(o => o.OrderDetails)
                .OrderByDescending(d => d.CreatedAt)
                .AsQueryable();

            switch (filter.FilterOrderState)
            {
                case FilterOrderState.All:
                    break;
                case FilterOrderState.Open:
                    query = query.Where(d => d.OrderState == OrderState.Open);
                    break;
                case FilterOrderState.Paid:
                    query = query.Where(d => d.OrderState == OrderState.Paid);
                    break;
                case FilterOrderState.Sent:
                    query = query.Where(d => d.OrderState == OrderState.Sent);
                    break;
                case FilterOrderState.Canceled:
                    query = query.Where(d => d.OrderState == OrderState.Canceled);
                    break;
                case FilterOrderState.Returned:
                    query = query.Where(d => d.OrderState == OrderState.Returned);
                    break;
                case FilterOrderState.Pending:
                    query = query.Where(d => d.OrderState == OrderState.Pending);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            if (!string.IsNullOrEmpty(filter.UserName))
            {
                query = query.Where(p => EF.Functions.Like(p.UserName, $"%{filter.UserName}%"));
            }

            if (filter.TransactionId is > 0)
            {
                query = query.Where(o => o.TransactionId == filter.TransactionId);
            }

            if (filter.PaymentRecordId is > 0)
            {
                query = query.Where(o => o.PaymentTransactionId == filter.PaymentRecordId);
            }

            if (!string.IsNullOrEmpty(filter.DestinationCity))
            {
                query = query.Where(p => EF.Functions.Like(p.DestinationCity, $"%{filter.DestinationCity}%"));
            }

            if (filter.MinimumPrice is > 0)
            {
                query = query.Where(d => d.TotalPrice > filter.MinimumPrice);
            }

            if (filter.UserId is > 0)
            {
                query = query.Where(d => d.UserId == filter.UserId);
            }

            filter.Data = await query.ToListAsync();
            return filter;
        }

        public async Task<OrderDetailDto> OrderDetail(long orderId)
        {
            var data = await _orderRepository.GetEntityById(orderId);

            return new OrderDetailDto
            {
                Address = data.Address,
                CreateDate = data.CreatedAt,
                Description = data.Description,
                Id = data.Id,
                OrderState = data.OrderState,
                UserName = data.UserName,
                TotalPrice = data.TotalPrice,
                UserId = data.UserId,
                PostCode = data.PostCode,
                PostTraceCode = data.PostTraceCode,
                BankTraceCode = data.TransactionId?.ToString() ?? string.Empty,
                LastUpdateDate = data.UpdatedAt,
                DestinationCity = data.DestinationCity,
                User = await _userRepository.GetEntityById(data.UserId),
                OrderDetails = await _orderDetailRepository.GetQuery()
                    .Include(d => d.ProductVariant)
                        .ThenInclude(v => v.Product)
                    .Where(d => d.OrderId == orderId)
                    .ToListAsync(),
                PaymentTransaction = await _paymentTransactionRepository.GetQuery()
                    .FirstOrDefaultAsync(r => r.OrderId == orderId && r.Status == PaymentStatus.Success)
            };
        }

        public async Task ProcessOrder(long orderId, OrderState orderState)
        {
            var data = await _orderRepository.GetEntityById(orderId);
            var oldState = data.OrderState;
            data.OrderState = orderState;

            _orderRepository.UpdateEntity(data);
            await _orderRepository.SaveChanges();

            _logger.LogInformation("Order state changed. OrderId: {OrderId}, OldState: {OldState}, NewState: {NewState}", orderId, oldState, orderState);
        }
    }
}