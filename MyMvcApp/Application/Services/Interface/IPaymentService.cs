using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Data.Entities.Order;

namespace Application.Services.Interface
{
    public interface IPaymentService : IAsyncDisposable
    {
        Task<long> SavePaymentRecord(SuccessPaymentResult dto);
    }
}