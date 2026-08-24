using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Application.Responses
{
    public class BalanceActionResponse
    {
        public bool Successful { get; set; }
        public string? ErrorMessage { get; set; }
    }
}