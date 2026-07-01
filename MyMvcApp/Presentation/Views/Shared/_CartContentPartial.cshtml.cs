using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace Presentation.Views.Shared
{
    public class _CartContentPartial : PageModel
    {
        private readonly ILogger<_CartContentPartial> _logger;

        public _CartContentPartial(ILogger<_CartContentPartial> logger)
        {
            _logger = logger;
        }

        public void OnGet()
        {
        }
    }
}