using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Application.Services.Interface;
using Data.DTOs.Account;
using Data.Entities.Order;
using Data.Entities.User;
using Data.Repository;
using Microsoft.EntityFrameworkCore;

namespace Application.Services.Implementation;

public class UserService : IUserService
{
    private readonly IRepository<User> _userRepository;
    private readonly IRepository<Order> _orderRepository;

    public UserService(IRepository<User> userRepository,IRepository<Order> orderRepository)
    {
        _userRepository = userRepository;
        _orderRepository = orderRepository;
    }

    public async Task CreateUser(RegisterOrLoginDto registerOrLoginDto)
    {
        await _userRepository.AddEntity(new User
        {
            Username = registerOrLoginDto.Username,
            Password = BCrypt.Net.BCrypt.HashPassword(registerOrLoginDto.Password),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });
        await _userRepository.SaveChanges();
    }

    public async ValueTask DisposeAsync()
    {
        await _userRepository.DisposeAsync();
        await _orderRepository.DisposeAsync();
    }

    public async Task<bool> GetLogin(RegisterOrLoginDto signupLoginDto)
    {
        var user = await GetUserByUsername(signupLoginDto.Username);
        if (user == null || !BCrypt.Net.BCrypt.Verify(signupLoginDto.Password, user.Password))
            return false;
        return true;
    }

    public async Task<User?> GetUserByUsername(string username)
    {
        return await _userRepository.GetQuery().SingleOrDefaultAsync(u => u.Username == username);
    }

    public async Task<UserDashboardDetailDto> UserDashboardDetail(long userId)
    {
        var orders = await _orderRepository.GetQuery().Where(o => o.UserId == userId)
            .ToListAsync();
        return new UserDashboardDetailDto
        {
            User = await _userRepository.GetEntityById(userId),
            CanceledOrderCount = orders.Count(o => o.OrderState == OrderState.Canceled),
            PendingOrderCount = orders.Count(o => o.OrderState == OrderState.Pending),
            ReturnedOrderCount = orders.Count(o => o.OrderState == OrderState.Returned),
            SentOrderCount = orders.Count(o => o.OrderState == OrderState.Sent),
            PendingOrders = orders.Where(o => o.OrderState == OrderState.Pending).ToList()
        };
    }
}