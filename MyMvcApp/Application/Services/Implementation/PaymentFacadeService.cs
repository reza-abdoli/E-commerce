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

        public async Task<bool> PayOrderWithWalletAsync(long userId, int totalCartPrice, long orderId)
        {
            await using var transaction = await _unitOfWork.BeginTransactionAsync();

            try
            {
                await _userService.WithdrawBalance(userId, totalCartPrice);
                // Create a direct Pending/Success transaction record for audit log
                var pendingTx = await _paymentService.CreatePendingTransactionAsync(userId, orderId, totalCartPrice, $"WALLET_{Guid.NewGuid():N}");
                await _paymentService.UpdateTransactionStatusAsync(pendingTx.Id, PaymentStatus.Success);

                // Pass paymentTx.Id as third parameter
                await _orderService.UpdatePaidOrder(orderId, userId, pendingTx.Id);


                await _unitOfWork.CommitTransactionAsync();

                _logger.LogInformation("Wallet payment processed successfully for UserId: {UserId}, OrderId: {OrderId}",
                    userId, orderId);
                return true;
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                _logger.LogError(ex, "Failed to process wallet payment for UserId: {UserId}, OrderId: {OrderId}. Error: {Message}",
                    userId, orderId, ex.Message);
                return false;
            }
        }

        public async Task<bool> FinalizeOnlineOrderPaymentAsync(PaymentTransaction paymentTx)
        {
            await using var transaction = await _unitOfWork.BeginTransactionAsync();

            try
            {
                // Update transaction status from Pending to Success
                await _paymentService.UpdateTransactionStatusAsync(paymentTx.Id, PaymentStatus.Success);

                // Pass paymentTx.Id as third parameter
                await _orderService.UpdatePaidOrder(paymentTx.OrderId!.Value, paymentTx.UserId, paymentTx.Id);

                await _unitOfWork.CommitTransactionAsync();

                _logger.LogInformation("Online payment processed successfully for UserId: {UserId}, OrderId: {OrderId}",
                    paymentTx.UserId, paymentTx.OrderId);
                return true;
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();

                _logger.LogWarning(ex,
                    "Online payment order finalization failed for OrderId: {OrderId}, UserId: {UserId}. Refunding amount to user wallet.",
                    paymentTx.OrderId, paymentTx.UserId);

                // Fallback: Charge user's wallet if order processing failed after bank payment
                try
                {
                    await using var refundTransaction = await _unitOfWork.BeginTransactionAsync();

                    await _paymentService.UpdateTransactionStatusAsync(paymentTx.Id, PaymentStatus.Success);

                    await _userService.DepositBalance(new DepositBalanceDto
                    {
                        Id = paymentTx.UserId,
                        Amount = paymentTx.Amount
                    });

                    await _unitOfWork.CommitTransactionAsync();

                    _logger.LogInformation(
                        "Successfully refunded paid order amount to wallet for UserId: {UserId}, Amount: {Amount}",
                        paymentTx.UserId, paymentTx.Amount);
                }
                catch (Exception refundEx)
                {
                    _logger.LogCritical(refundEx,
                        "CRITICAL: Failed to refund paid amount to user wallet after order processing failure. UserId: {UserId}, OrderId: {OrderId}",
                        paymentTx.UserId, paymentTx.OrderId);
                }

                return false;
            }
        }

        public async Task<bool> DepositWalletBalanceAsync(PaymentTransaction paymentTx)
        {
            await using var transaction = await _unitOfWork.BeginTransactionAsync();

            try
            {
                // Update transaction status to Success
                await _paymentService.UpdateTransactionStatusAsync(paymentTx.Id, PaymentStatus.Success);

                // Add balance to user wallet
                await _userService.DepositBalance(new DepositBalanceDto
                {
                    Id = paymentTx.UserId,
                    Amount = paymentTx.Amount
                });

                await _unitOfWork.CommitTransactionAsync();

                _logger.LogInformation("Wallet balance deposited successfully for UserId: {UserId}, Amount: {Amount}",
                    paymentTx.UserId, paymentTx.Amount);

                return true;
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();

                _logger.LogError(ex, "Failed to deposit wallet balance for UserId: {UserId}, Amount: {Amount}. Error: {Message}",
                    paymentTx.UserId, paymentTx.Amount, ex.Message);

                return false;
            }
        }
    }
}