using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using SyweachWeb.Models;

namespace SyweachWeb.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly SyweachWeb.Services.IDataService _dataService;

    public HomeController(ILogger<HomeController> logger, SyweachWeb.Services.IDataService dataService)
    {
        _logger = logger;
        _dataService = dataService;
    }

    public IActionResult Index()
    {
        var model = new SyweachWeb.ViewModels.HomeViewModel
        {
            Projects = _dataService.GetProjects(),
            Skills = _dataService.GetSkills(),
            Version = _dataService.GetVersion()
        };
        return View(model);
    }

    [HttpPost]
    public IActionResult SetLanguage(string culture, string returnUrl)
    {
        Response.Cookies.Append(
            Microsoft.AspNetCore.Localization.CookieRequestCultureProvider.DefaultCookieName,
            Microsoft.AspNetCore.Localization.CookieRequestCultureProvider.MakeCookieValue(new Microsoft.AspNetCore.Localization.RequestCulture(culture)),
            new Microsoft.AspNetCore.Http.CookieOptions { 
                Expires = DateTimeOffset.UtcNow.AddYears(1),
                IsEssential = true,
                SameSite = SameSiteMode.Strict 
            }
        );

        return LocalRedirect(returnUrl ?? "/");
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
