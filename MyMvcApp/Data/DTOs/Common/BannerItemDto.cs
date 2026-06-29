using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Data.Entities.Common;
using Microsoft.AspNetCore.Http;

namespace Data.DTOs.Common;

public class BannerItemDto
{
    public string Title { get; set; }
    public string Url { get; set; }
    public string ImageName { get; set; }
    public int Order { get; set; }
    public BannerLocation BannerLocation { get; set; }
}
