using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using Task4_UserManagement.Data;

namespace Task4_UserManagement.Filters;

public class AuthFilter(UserIndexContext db) : IAsyncPageFilter
{
    public async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
    {
        var page = context.HandlerInstance.GetType().Name;
        var session = context.HttpContext.Session;

        if (page is "IndexModel" or "RegisterModel" ||
            session.GetInt32("UserId") is int id &&
            await db.Users.FindAsync(id) is { Status: not "blocked" })
        {
            await next();
        }
        else
        {
            session.Remove("UserId");
            context.Result = new RedirectToPageResult("/Index");
        }
    }

    public Task OnPageHandlerSelectionAsync(PageHandlerSelectedContext context) => Task.CompletedTask;
}
