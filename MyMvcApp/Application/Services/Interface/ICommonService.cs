using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Data.DTOs.Common;
using Data.Entities.Common;

namespace Application.Services.Interface
{
    public interface ICommonService : IAsyncDisposable
    {
        Task CreateBanner(CreateBannerDto dto);

        Task<List<BannerItemDto>> GetHomeBanners();

        Task<List<Banner>> GetAllBanners();

    }
}