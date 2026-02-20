using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using SyweachWeb.Models;

namespace SyweachWeb.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly SyweachWeb.Services.IDataService _dataService;
    private readonly SyweachWeb.Services.IEmailService _emailService;

    public HomeController(ILogger<HomeController> logger, 
                          SyweachWeb.Services.IDataService dataService,
                          SyweachWeb.Services.IEmailService emailService)
    {
        _logger = logger;
        _dataService = dataService;
        _emailService = emailService;
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
    public async Task<IActionResult> SubmitContact([FromBody] ContactFormModel model)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new { message = "Geçersiz form verisi." });
        }

        try
        {
            string subject = $"İletişim Formu Mesajı: {model.Name}";
            string body = $@"
                <h3>Yeni İletişim Formu Mesajı</h3>
                <p><strong>İsim:</strong> {model.Name}</p>
                <p><strong>E-posta:</strong> {model.Email}</p>
                <p><strong>Mesaj:</strong><br/>{model.Message}</p>
            ";

            await _emailService.SendEmailAsync(subject, body);
            return Ok(new { message = "Mesajınız başarıyla gönderildi!" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "E-posta gönderimi sırasında hata oluştu.");
            return StatusCode(500, new { message = "Mesaj gönderilirken bir hata oluştu. Lütfen daha sonra tekrar deneyin." });
        }
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
