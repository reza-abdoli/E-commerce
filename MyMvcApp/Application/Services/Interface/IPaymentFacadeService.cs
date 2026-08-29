using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Data.DTOs.Account;
using Data.Entities.Order;

namespace Application.Services.Interface
{
    public interface IPaymentFacadeService
    {
        Task<bool> PayOrderWithWalletAsync(long userId, int totalCartPrice, long orderId);
        Task<bool> FinalizeOnlineOrderPaymentAsync(PaymentTransaction transaction);
        Task<bool> DepositWalletBalanceAsync(PaymentTransaction transaction);
    }
}