using Microsoft.AspNetCore.Mvc;
using GoldPriceTracker.Services;
using GoldPriceTracker.Models;

namespace GoldPriceTracker.Controllers;

public class HomeController : Controller
{
    private readonly IPriceService _priceService;
    private readonly IAuthService _authService;
    private readonly ILogger<HomeController> _logger;

    public HomeController(
        IPriceService priceService, 
        IAuthService authService,
        ILogger<HomeController> logger)
    {
        _priceService = priceService;
        _authService = authService;
        _logger = logger;
    }

    public async Task<IActionResult> Index()
    {
        try
        {
            var goldPrice = await _priceService.GetGoldPriceAsync();
            var dollarPrice = await _priceService.GetDollarPriceAsync();
            
            // برای سکه، از قیمت طلا استفاده می‌کنیم (می‌توانید API جداگانه اضافه کنید)
            var coinPrice = new PriceData
            {
                Name = "سکه",
                Price = goldPrice.Price * 8.13m, // تقریباً 8.13 گرم
                Change = goldPrice.Change * 8.13m,
                ChangePercent = goldPrice.ChangePercent,
                LastUpdate = goldPrice.LastUpdate
            };

            var model = new PriceViewModel
            {
                GoldPrice = goldPrice,
                CoinPrice = coinPrice,
                DollarPrice = dollarPrice,
                LoginModel = new LoginViewModel()
            };

            return View(model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "خطا در بارگذاری صفحه اصلی");
            return View(new PriceViewModel());
        }
    }

    [HttpGet]
    public IActionResult Login()
    {
        return View(new LoginViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        // Get price data for the view
        var goldPrice = await _priceService.GetGoldPriceAsync();
        var dollarPrice = await _priceService.GetDollarPriceAsync();
        var coinPrice = new PriceData
        {
            Name = "سکه",
            Price = goldPrice.Price * 8.13m,
            Change = goldPrice.Change * 8.13m,
            ChangePercent = goldPrice.ChangePercent,
            LastUpdate = goldPrice.LastUpdate
        };

        var viewModel = new PriceViewModel
        {
            GoldPrice = goldPrice,
            CoinPrice = coinPrice,
            DollarPrice = dollarPrice,
            LoginModel = model
        };

        if (!ModelState.IsValid)
        {
            return View("Index", viewModel);
        }

        // Authenticate user
        var loginRequest = new LoginRequest
        {
            EmailOrUsername = model.EmailOrUsername,
            Password = model.Password,
            RememberMe = model.RememberMe
        };

        var authResponse = await _authService.LoginAsync(loginRequest);

        if (!authResponse.Success)
        {
            ModelState.AddModelError("", authResponse.Message ?? "خطا در ورود");
            return View("Index", viewModel);
        }

        // Store token in cookie or session
        if (!string.IsNullOrEmpty(authResponse.Token))
        {
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = authResponse.ExpiresAt
            };

            Response.Cookies.Append("AuthToken", authResponse.Token, cookieOptions);

            if (!string.IsNullOrEmpty(authResponse.RefreshToken))
            {
                Response.Cookies.Append("RefreshToken", authResponse.RefreshToken, cookieOptions);
            }
        }

        // Redirect to dashboard after successful login
        return RedirectToAction("Index");
    }

    [HttpPost]
    public async Task<IActionResult> Logout()
    {
        Response.Cookies.Delete("AuthToken");
        Response.Cookies.Delete("RefreshToken");
        return RedirectToAction("Index");
    }
}





