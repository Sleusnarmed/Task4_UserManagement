using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Task4_UserManagement.Models;
using Task4_UserManagement.Data;
using Microsoft.EntityFrameworkCore;
namespace Task4_UserManagement.Pages;

public class AdminPanelModel : PageModel
{
    private readonly UserIndexContext _context;

    public List<User> Users { get; set; } = new();

    public AdminPanelModel(UserIndexContext context)
    {
        _context = context;
    }

    public async Task OnGetAsync()
    {
        Users = await _context.Users.ToListAsync();
    }

    public async Task<IActionResult> OnPostBlockUsersAsync(int[] selectedUserIds)
    {
        await UpdateUserStatus(selectedUserIds, "blocked");
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostUnblockUsersAsync(int[] selectedUserIds)
    {
        await UpdateUserStatus(selectedUserIds, "active");
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostDeleteUsersAsync(int[] selectedUserIds)
    {
        var users = await _context.Users
            .Where(u => selectedUserIds.Contains(u.Id))
            .ToListAsync();

        _context.Users.RemoveRange(users);
        await _context.SaveChangesAsync();
        return RedirectToPage();
    }

    private async Task UpdateUserStatus(int[] userIds, string status)
    {
        var users = await _context.Users
            .Where(u => userIds.Contains(u.Id))
            .ToListAsync();

        foreach (var user in users)
        {
            user.Status = status;
        }
        await _context.SaveChangesAsync();
    }
}
