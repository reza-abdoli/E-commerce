using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Application.Responses;
using Application.Services.Interface;
using Data.DTOs.Account;
using Data.Entities.Order;
using Data.Entities.User;
using Data.Repository;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Application.Services.Implementation;

public class UserService : IUserService
{
    private readonly IRepository<User> _userRepository;
    private readonly IRepository<Order> _orderRepository;
    private readonly ILogger<UserService> _logger;

    public UserService(
        IRepository<User> userRepository,
        IRepository<Order> orderRepository,
        ILogger<UserService> logger)
    {
        _userRepository = userRepository;
        _orderRepository = orderRepository;
        _logger = logger;
    }

    public Task<UserDashboardDetailDto> CheckUserFilledProfile(long userId)
    {
        throw new NotImplementedException();
    }

    public async Task CreateUser(RegisterOrLoginDto registerOrLoginDto)
    {
        var user = new User
        {
            Username = registerOrLoginDto.Username,
            Password = BCrypt.Net.BCrypt.HashPassword(registerOrLoginDto.Password),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _userRepository.AddEntity(user);
        await _userRepository.SaveChanges();
        _logger.LogInformation("New user created. UserId: {UserId}, Username: {Username}", user.Id, user.Username);
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
        {
            _logger.LogWarning("Failed login attempt. Username: {Username}", signupLoginDto.Username);
            return false;
        }
        _logger.LogInformation("User logged in. UserId: {UserId}, Username: {Username}", user.Id, user.Username);
        return true;
    }

    public async Task<User?> GetUserByUsername(string username)
    {
        return await _userRepository.GetQuery().SingleOrDefaultAsync(u => u.Username == username);
    }

    public async Task<bool> IsProfileDetailCompleted(long userId)
    {
        var user = await _userRepository.GetEntityById(userId);
        if (user.MobileNumber == null || user.FullName == null || user.Address == null ||
         user.PostCode == null || user.UserCity == null)
            return false;
        return true;
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

    public async Task<EditUserInfoDTO> GetEditUserDetail(long userId)
    {
        var user = await _userRepository.GetEntityById(userId);
        return new EditUserInfoDTO
        {
            UserId = userId,
            MobileNumber = user.MobileNumber,
            FullName = user.FullName,
            Email = user.Email,
            Address = user.Address,
            PostCode = user.PostCode,
            UserCity = user.UserCity
        };
    }

    public async Task EditUserDetail(EditUserInfoDTO dto)
    {
        var user = await _userRepository.GetEntityById(dto.UserId);

        var oldValues = $"FullName: {user.FullName}, Email: {user.Email}, Mobile: {user.MobileNumber}, Address: {user.Address}, PostCode: {user.PostCode}, City: {user.UserCity}";

        user.MobileNumber = dto.MobileNumber;
        user.FullName = dto.FullName;
        user.Email = dto.Email;
        user.Address = dto.Address;
        user.PostCode = dto.PostCode;
        user.UserCity = dto.UserCity;
        _userRepository.UpdateEntity(user);
        await _userRepository.SaveChanges();

        var newValues = $"FullName: {user.FullName}, Email: {user.Email}, Mobile: {user.MobileNumber}, Address: {user.Address}, PostCode: {user.PostCode}, City: {user.UserCity}";

        _logger.LogInformation(
            "User profile updated. UserId: {UserId}. Old values -> {OldValues}. New values -> {NewValues}",
            dto.UserId, oldValues, newValues);
    }
    public async Task DepositBalance(DepositBalanceDto dto)
    {
        if (dto.Amount <= 0)
        {
            throw new ArgumentException("Deposit amount must be greater than zero.", nameof(dto.Amount));
        }

        int affectedRows = await _userRepository.GetQuery()
            .Where(u => u.Id == dto.Id)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(u => u.Balance, u => u.Balance + dto.Amount));

        if (affectedRows == 0)
        {
            throw new InvalidOperationException($"User with ID {dto.Id} was not found.");
        }

        _logger.LogInformation("Balance deposited atomically. UserId: {UserId}, Amount: {Amount}", dto.Id, dto.Amount);
    }

    public async Task<int> GetUserBalance(long userId)
    {
        var user = await _userRepository.GetEntityById(userId);
        return user.Balance;
    }

    public async Task WithdrawBalance(long userId, int amount)
    {
        // Atomic update at database level to prevent race conditions & negative balance
        int affectedRows = await _userRepository.GetQuery()
            .Where(u => u.Id == userId && u.Balance >= amount)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(u => u.Balance, u => u.Balance - amount));

        if (affectedRows == 0)
        {
            var userExists = await _userRepository.GetQuery().AnyAsync(u => u.Id == userId);

            if (!userExists)
            {
                throw new InvalidOperationException($"User with ID {userId} was not found.");
            }

            throw new InvalidOperationException("Insufficient wallet balance for this transaction.");
        }

        _logger.LogInformation("Balance withdrawn atomically. UserId: {UserId}, Amount: {Amount}", userId, amount);
    }
}