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

        [BindProperty]
        public string Email { get; set; } = null!;

        [BindProperty]
        public string Password { get; set; } = null!;

        [BindProperty]
        public string Name { get; set; } = null!;

        public IndexModel(UserIndexContext context, IPasswordHasher<User> passwordHasher)
        {
            _context = context;
            _passwordHasher = passwordHasher;
        }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostLogin()
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == Email);
            
            if (user == null || _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, Password) != PasswordVerificationResult.Success)
            {
                ModelState.AddModelError("", "Invalid credentials");
                return Page();
            }

            // Update last login
            user.LastLogin = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            // Store user ID in session
            HttpContext.Session.SetInt32("UserId", user.Id);
            
            return RedirectToPage("/AdminPanel");
        }

        public async Task<IActionResult> OnPostSignUp()
        {
            if (await _context.Users.AnyAsync(u => u.Email.ToLower() == Email))
            {
                ModelState.AddModelError("", "Email already exists");
                return Page();
            }

            var user = new User
            {
                Name = Name,
                Email = Email,
                PasswordHash = _passwordHasher.HashPassword(new User(), Password),
                Status = "active",
                RegistrationTime = DateTime.UtcNow
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            HttpContext.Session.SetInt32("UserId", user.Id);
            return RedirectToPage("/AdminPanel");
        }
    }
}