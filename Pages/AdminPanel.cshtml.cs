using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace Task4_UserManagement.Pages;

public class AdminPanelModel : PageModel
{
    private readonly ILogger<AdminPanelModel> _logger;

    public AdminPanelModel(ILogger<AdminPanelModel> logger)
    {
        _logger = logger;
    }

    public void OnGet()
    {
        _logger.LogInformation("Admin panel loaded.");
    }
}
