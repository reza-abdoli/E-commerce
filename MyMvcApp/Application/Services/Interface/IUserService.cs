using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Data.DTOs.Account;
using Data.DTOs.Product;
using Data.Entities.User;

namespace Application.Services.Interface;

public interface IUserService
{
    Task<User?> GetUserByUsername(string username);
    Task CreateUser(RegisterOrLoginDto registerOrLoginDto);
    Task<bool> GetLogin(RegisterOrLoginDto registerOrLoginDto);

}