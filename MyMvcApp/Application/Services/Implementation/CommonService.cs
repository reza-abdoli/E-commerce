using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Application.Extensions;
using Application.Services.Interface;
using Data.DTOs.Common;
using Data.Entities.Common;
using Data.Repository;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace Application.Services.Implementation;


public class CommonService : ICommonService
{
    private readonly IRepository<Banner> _bannerRepository;
    private readonly IMemoryCache _cache;
    private readonly ILogger<CommonService> _logger;

    public CommonService(IRepository<Banner> bannerRepository, IMemoryCache cache, ILogger<CommonService> logger)
    {
        _bannerRepository = bannerRepository;
        _cache = cache;
        _logger = logger;
    }
    public async Task CreateBanner(CreateBannerDto dto)
    {
        var imageName = Guid.NewGuid().ToString("N") + Path.GetExtension(dto.ImageFile.FileName);
        var res = dto.ImageFile.AddImageToServer(imageName, PathExtension.BannerServer, 300, 150, PathExtension.BannerThumbServer);
        // if (!res) return false;
        var newBanner = new Banner
        {
            BannerLocation = dto.BannerLocation,
            Order = dto.Order,
            Url = dto.Url,
            Title = dto.Title,
            ImageName = imageName
        };

        await _bannerRepository.AddEntity(newBanner);
        await _bannerRepository.SaveChanges();

        _logger.LogInformation("Banner created. BannerId: {BannerId}, Title: {Title}, Location: {BannerLocation}", newBanner.Id, newBanner.Title, newBanner.BannerLocation);

        var cachedBanners = _cache.Get<List<BannerItemDto>>("AllBanners");
        if (cachedBanners == null) return;

        var newBannerDto = new BannerItemDto
        {
            Title = newBanner.Title,
            Url = newBanner.Url,
            ImageName = newBanner.ImageName,
            Order = newBanner.Order,
            BannerLocation = newBanner.BannerLocation
        };

        cachedBanners.Add(newBannerDto);
        cachedBanners = cachedBanners.OrderBy(b => b.Order).ToList();
        _cache.Set("AllBanners", cachedBanners, TimeSpan.FromDays(1));

    }

    public async ValueTask DisposeAsync()
    {
        await _bannerRepository.DisposeAsync();
    }

    public async Task<List<Banner>> GetAllBanners()
    {
        return await _bannerRepository.GetQuery().ToListAsync();
    }

    public async Task<List<BannerItemDto>> GetHomeBanners()
    {
        var banners = await _cache.GetOrCreateAsync("AllBanners", async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(1);

                var bannerEntities = await _bannerRepository.GetQuery()
                    .OrderBy(b => b.Order)
                    .Select(b => new BannerItemDto
                    {
                        Title = b.Title,
                        Url = b.Url,
                        ImageName = b.ImageName,
                        Order = b.Order,
                        BannerLocation = b.BannerLocation
                    })
                    .ToListAsync();

                    return bannerEntities;
            });
            return banners != null ? banners : new List<BannerItemDto>();
    }
}