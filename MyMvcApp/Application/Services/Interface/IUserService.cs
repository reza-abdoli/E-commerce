using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Application.Responses;
using Data.DTOs.Account;
using Data.DTOs.Product;
using Data.Entities.Order;
using Data.Entities.User;

namespace Application.Services.Interface;

public interface IUserService : IAsyncDisposable
{
    Task<User?> GetUserByUsername(string username);
    Task CreateUser(RegisterOrLoginDto registerOrLoginDto);
    Task<bool> GetLogin(RegisterOrLoginDto registerOrLoginDto);

    Task<UserDashboardDetailDto> UserDashboardDetail(long userId);
    Task<bool> IsProfileDetailCompleted(long userId);
    Task<EditUserInfoDTO> GetEditUserDetail(long userId);
    Task EditUserDetail(EditUserInfoDTO dto);

    Task DepositBalance(DepositBalanceDto dto);
    Task<int> GetUserBalance(long userId);
    Task WithdrawBalance(long userId, int amount);

}