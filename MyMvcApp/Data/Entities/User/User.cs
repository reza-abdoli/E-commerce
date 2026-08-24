using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Data.Entities.User;

public class User : BaseEntity
{
    public string Username { get; set; }
    public string Password { get; set; }
    public string Role { get; set; } = "User";
    public int Balance { get; set; } = 0;


    //----------------------------------------------
    public string? MobileNumber { get; set; }
    public string? FullName { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }
    public string? PostCode { get; set; }
    public string? UserCity { get; set; }
}