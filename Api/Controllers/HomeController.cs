using GoldPriceTracker.Domain.Entities;
using GoldPriceTracker.Infrastructure.ExternalServices.Interfaces;
using GoldPriceTracker.Shared.Contracts.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace GoldPriceTracker.Api.Controllers;



/// <summary>
/// Home controller - thin API layer.
/// Handles public pages and price display.
/// </summary>
public class HomeController : Controller
{
    private readonly IPriceService _priceService;
    private readonly ILogger<HomeController> _logger;

    public HomeController(IPriceService priceService, ILogger<HomeController> logger)
    {
        _priceService = priceService;
        _logger = logger;
    }

    public async Task<IActionResult> Index()
    {
        try
        {
            var goldPrice = await _priceService.GetGoldPriceAsync();
            var dollarPrice = await _priceService.GetDollarPriceAsync();
            
            // برای سکه، از قیمت طلا استفاده می‌کنیم
            var coinPrice = new PriceData
            {
                Name = "سکه",
                Price = goldPrice.Price * 8.13m,
                Change = goldPrice.Change * 8.13m,
                ChangePercent = goldPrice.ChangePercent,
                LastUpdate = goldPrice.LastUpdate
            };

            var model = new PriceViewModel
            {
                GoldPrice = goldPrice,
                CoinPrice = coinPrice,
                DollarPrice = dollarPrice
            };

            return View(model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "خطا در بارگذاری صفحه اصلی");
            return View(new PriceViewModel());
        }
    }
}

