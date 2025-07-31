using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Task4_UserManagement.Models;
using Task4_UserManagement.Data;
using Microsoft.EntityFrameworkCore;
using Task4_UserManagement.Filters;

namespace Task4_UserManagement.Pages;

[TypeFilter(typeof(AuthFilter))]
public class AdminPanelModel : PageModel
{
    private readonly UserIndexContext _context;
    public List<User> Users { get; set; } = new();

    public AdminPanelModel(UserIndexContext context) => _context = context;

    public async Task<IActionResult> OnGetAsync()
    {
        Users = await _context.Users.ToListAsync();
        return Page();
    }

    public async Task<IActionResult> OnPostBlockUsersAsync(int[] selectedUserIds) => 
        await ProcessUserAction(selectedUserIds, "blocked");

    public async Task<IActionResult> OnPostUnblockUsersAsync(int[] selectedUserIds) => 
        await ProcessUserAction(selectedUserIds, "active");

    public async Task<IActionResult> OnPostDeleteUsersAsync(int[] selectedUserIds)
    {
        _context.Users.RemoveRange(await GetUsersByIds(selectedUserIds));
        await _context.SaveChangesAsync();
        return RedirectToPage();
    }

    private async Task<IActionResult> ProcessUserAction(int[] userIds, string status)
    {
        await UpdateUserStatus(userIds, status);
        return RedirectToPage();
    }

    private async Task UpdateUserStatus(int[] userIds, string status)
    {
        var users = await GetUsersByIds(userIds);
        users.ForEach(u => u.Status = status);
        await _context.SaveChangesAsync();
    }

    private async Task<List<User>> GetUsersByIds(int[] userIds) => 
        await _context.Users.Where(u => userIds.Contains(u.Id)).ToListAsync();

    public static string GetRelativeTime(DateTime? date) => 
        date.HasValue ? CalculateTimeDifference(DateTime.UtcNow - date.Value) : "just now";

    private static string CalculateTimeDifference(TimeSpan timeSpan) => Math.Abs(timeSpan.TotalSeconds) switch
    {
        < 60 => "Just now",
        < 120 => "a minute ago",
        < 3600 => $"{timeSpan.Minutes} minutes ago",
        < 7200 => "an hour ago",
        < 86400 => $"{timeSpan.Hours} hours ago",
        < 172800 => "yesterday",
        < 2592000 => $"{timeSpan.Days} days ago",
        < 31104000 => $"{timeSpan.Days / 30} months ago",
        _ => $"{timeSpan.Days / 365} years ago"
    };
}