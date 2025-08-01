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

    public async Task<IActionResult> OnGetAsync(string sortBy = "id", bool asc = true)
    {
        Users = await SortUsers(_context.Users, sortBy, asc).ToListAsync();
        return Page();
    }

    private static IQueryable<User> SortUsers(IQueryable<User> users, string sortBy, bool asc)
    {
        return sortBy switch
        {
            "name" => asc ? users.OrderBy(u => u.Name) : users.OrderByDescending(u => u.Name),
            "email" => asc ? users.OrderBy(u => u.Email) : users.OrderByDescending(u => u.Email),
            "lastSeen" => asc ? users.OrderBy(u => u.LastLogin) : users.OrderByDescending(u => u.LastLogin),
            "registerTime" => asc ? users.OrderBy(u => u.RegistrationTime) : users.OrderByDescending(u => u.RegistrationTime),
            "status" => asc ? users.OrderBy(u => u.Status) : users.OrderByDescending(u => u.Status),
            _ => users.OrderBy(u => u.Id)
        };
    }

    public async Task<IActionResult> OnPostBlockUsersAsync(int[] selectedUserIds) =>
        await ProcessUser(selectedUserIds, "blocked");

    public async Task<IActionResult> OnPostUnblockUsersAsync(int[] selectedUserIds) =>
        await ProcessUser(selectedUserIds, "active");

    public async Task<IActionResult> OnPostDeleteUsersAsync(int[] selectedUserIds)
    {
        _context.Users.RemoveRange(await GetUserIds(selectedUserIds));
        await _context.SaveChangesAsync();
        return RedirectToPage();
    }

    private async Task<IActionResult> ProcessUser(int[] userIds, string status)
    {
        await UpdateStatus(userIds, status);
        return RedirectToPage();
    }

    private async Task UpdateStatus(int[] userIds, string status)
    {
        var users = await GetUserIds(userIds);
        users.ForEach(u => u.Status = status);
        await _context.SaveChangesAsync();
    }

    private async Task<List<User>> GetUserIds(int[] userIds) =>
        await _context.Users.Where(u => userIds.Contains(u.Id)).ToListAsync();

    public static string GetTime(DateTime? date) =>
        date.HasValue ? CalculateTime(DateTime.UtcNow - date.Value) : "Just Now";

    private static string CalculateTime(TimeSpan timeSpan) => Math.Abs(timeSpan.TotalSeconds) switch
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