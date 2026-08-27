using System;
using System.Threading.Tasks;
using Application.Services.Interface;
using Data.DTOs.Account;
using Data.Entities.Order;
using Data.Repository;
using Microsoft.Extensions.Logging;

namespace Application.Services.Implementation
{
    public class PaymentFacadeService : IPaymentFacadeService
    {
        private readonly IOrderService _orderService;
        private readonly IUserService _userService;
        private readonly IPaymentService _paymentService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<PaymentFacadeService> _logger;

        public PaymentFacadeService(
            IOrderService orderService,
            IUserService userService,
            IPaymentService paymentService,
            IUnitOfWork unitOfWork,
            ILogger<PaymentFacadeService> logger)
        {
            _orderService = orderService;
            _userService = userService;
            _paymentService = paymentService;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<bool> PayOrderWithWalletAsync(long userId, int totalCartPrice, SuccessPaymentResult paymentResult)
        {
            await using var transaction = await _unitOfWork.BeginTransactionAsync();

            try
            {
                var recordId = await _paymentService.SavePaymentRecord(paymentResult);
                await _orderService.UpdatePaidOrder(paymentResult, recordId);

                await _userService.WithdrawBalance(userId, totalCartPrice);

                await _unitOfWork.CommitTransactionAsync();
                _logger.LogInformation("Wallet payment processed successfully for UserId: {UserId}, OrderId: {OrderId}",
                    userId, paymentResult.order_id);
                return true;
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                _logger.LogError(ex, "Failed to process wallet payment for UserId: {UserId}, OrderId: {OrderId}. Error: {Message}",
                    userId, paymentResult.order_id, ex.Message);
                return false;
            }
        }

        public async Task<bool> FinalizeOnlineOrderPaymentAsync(SuccessPaymentResult paymentResult)
        {
            await using var transaction = await _unitOfWork.BeginTransactionAsync();

            try
            {
                var recordId = await _paymentService.SavePaymentRecord(paymentResult);

                await _orderService.UpdatePaidOrder(paymentResult, recordId);

                await _unitOfWork.CommitTransactionAsync();
                _logger.LogInformation("Online payment processed successfully for UserId: {UserId}, OrderId: {OrderId}",
                    paymentResult.buyer_id, paymentResult.order_id);
                return true;
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();

                _logger.LogWarning(ex,
                    "Online payment order finalization failed for OrderId: {OrderId}, UserId: {UserId}. Refunding amount to user wallet.",
                    paymentResult.order_id, paymentResult.buyer_id);

                try
                {
                    await using var refundTransaction = await _unitOfWork.BeginTransactionAsync();

                    await _paymentService.SavePaymentRecord(paymentResult);

                    await _userService.DepositBalance(new DepositBalanceDto
                    {
                        Id = paymentResult.buyer_id,
                        Amount = paymentResult.amount
                    });

                    await _unitOfWork.CommitTransactionAsync();

                    _logger.LogInformation(
                        "Successfully refunded paid order amount to wallet for UserId: {UserId}, Amount: {Amount}",
                        paymentResult.buyer_id, paymentResult.amount);
                }
                catch (Exception refundEx)
                {
                    _logger.LogCritical(refundEx,
                        "CRITICAL: Failed to refund paid amount to user wallet after order processing failure. UserId: {UserId}, OrderId: {OrderId}",
                        paymentResult.buyer_id, paymentResult.order_id);
                }

                return false;
            }
        }

        public async Task<bool> DepositWalletBalanceAsync(DepositBalanceDto dto, SuccessPaymentResult paymentResult)
        {
            await using var transaction = await _unitOfWork.BeginTransactionAsync();

            try
            {
                await _paymentService.SavePaymentRecord(paymentResult);

                await _userService.DepositBalance(dto);

                await _unitOfWork.CommitTransactionAsync();
                return true;
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                _logger.LogError(ex, "Failed to deposit wallet balance for UserId: {UserId}, Amount: {Amount}. Error: {Message}",
                    dto.Id, dto.Amount, ex.Message);
                
                return false;
            }
        }
    }
}