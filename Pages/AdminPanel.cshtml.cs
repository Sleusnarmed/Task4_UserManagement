using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Task4_UserManagement.Models;
using Task4_UserManagement.Data;
using Microsoft.EntityFrameworkCore;
using System;

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

    public static string GetRelativeTime(DateTime? date)
    {
        if (!date.HasValue) return "Just Now";
        var timeSpan = DateTime.UtcNow - date.Value; 
        double delta = Math.Abs(timeSpan.TotalSeconds);
        return delta switch
        {
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
}