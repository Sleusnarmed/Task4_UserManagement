using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Identity;
using Task4_UserManagement.Models;
using Task4_UserManagement.Data;
using Microsoft.EntityFrameworkCore;

namespace Task4_UserManagement.Pages
{
    public class IndexModel : PageModel
    {
        private readonly UserIndexContext _context;
        private readonly IPasswordHasher<User> _passwordHasher;

        [BindProperty] public string Email { get; set; } = null!;
        [BindProperty] public string Password { get; set; } = null!;
        [BindProperty] public string Name { get; set; } = null!;

        public IndexModel(UserIndexContext context, IPasswordHasher<User> passwordHasher)
            => (_context, _passwordHasher) = (context, passwordHasher);

        public void OnGet() { }

        public async Task<IActionResult> OnPostLogin()
        {
            var user = await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Email.Equals(Email, StringComparison.OrdinalIgnoreCase));

            if (user is null || _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, Password) != PasswordVerificationResult.Success)
                return InvalidCredentials();

            user.LastLogin = DateTime.UtcNow;
            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            HttpContext.Session.SetInt32("UserId", user.Id);
            return RedirectToPage("/adminpanel");
        }

        public async Task<IActionResult> OnPostSignUp()
        {
            if (await _context.Users.AnyAsync(u => u.Email.Equals(Email, StringComparison.OrdinalIgnoreCase)))
                return InvalidCredentials("Email already exists");

            var user = new User
            {
                Name = Name,
                Email = Email,
                PasswordHash = _passwordHasher.HashPassword(new User(), Password),
                Status = "active",
                RegistrationTime = DateTime.UtcNow,
                LastLogin = DateTime.UtcNow
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            user.LastLogin = DateTime.UtcNow;
            HttpContext.Session.SetInt32("UserId", user.Id);
            return RedirectToPage("/adminpanel");
        }

        private IActionResult InvalidCredentials(string message = "Invalid credentials")
        {
            ModelState.AddModelError("", message);
            return Page();
        }
    }
}