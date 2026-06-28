using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Application.Services.Interface;
using Data.DTOs.Account;
using Data.Entities.User;
using Data.Repository;
using Microsoft.EntityFrameworkCore;

namespace Application.Services.Implementation;

public class UserService : IUserService
{
    private readonly IRepository<User> _userRepository;
    public UserService(IRepository<User> userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task CreateUser(RegisterOrLoginDto registerOrLoginDto)
    {
        await _userRepository.AddEntity(new User
        {
            Username = registerOrLoginDto.Username,
            Password = registerOrLoginDto.Password,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });
        await _userRepository.SaveChanges();
    }

    public async Task<bool> GetLogin(RegisterOrLoginDto signupLoginDto)
    {
        var user = await GetUserByUsername(signupLoginDto.Username);
        if(user == null || !BCrypt.Net.BCrypt.Verify(signupLoginDto.Password, user.Password))
            return false;
        return true;
    }

    public async Task<User?> GetUserByUsername(string username)
    {
        return await _userRepository.GetQuery().SingleOrDefaultAsync(u => u.Username == username);
    }
}