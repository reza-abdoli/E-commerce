using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace Presentation.Areas.Admin.Views.Product
{
    public class CreateFeature : PageModel
    {
        private readonly ILogger<CreateFeature> _logger;

        public CreateFeature(ILogger<CreateFeature> logger)
        {
            _logger = logger;
        }

        public void OnGet()
        {
        }
    }
}