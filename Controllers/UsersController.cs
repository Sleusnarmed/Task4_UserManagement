using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using Task4_UserManagement.Data;
using Task4_UserManagement.Models;


namespace Task4_UserManagement.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController(UserIndexContext context) : ControllerBase
    {
        private readonly UserIndexContext _context = context;
        private readonly IPasswordHasher<User> _passwordHasher = new PasswordHasher<User>();

        // GET: api/users/5
        [HttpGet("{id}")]
        public async Task<ActionResult<UserResponse>> GetUser(int id) =>
            await _context.Users.FindAsync(id) is { } user
                ? Ok(MapToResponse(user))
                : NotFound();

        // POST: api/users/signup
        [HttpPost("signup")]
        public async Task<ActionResult<UserResponse>> SignUp(SignUpRequest request)
        {
            if (await _context.Users.AnyAsync(u => u.Email.ToLower() == request.Email))
                return Conflict("Email already exists.");

            var user = new User
            {
                Name = request.Name,
                Email = request.Email,
                PasswordHash = _passwordHasher.HashPassword(new User(), request.Password),
                RegistrationTime = DateTime.UtcNow,
                Status = "active"
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetUser), new { id = user.Id }, MapToResponse(user));
        }

        // POST: api/users/login
        [HttpPost("login")]
        public async Task<ActionResult<LoginResponse>> Login(LoginRequest request)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == request.Email);
            if (user is null || _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.Password)
                != PasswordVerificationResult.Success)
                return Unauthorized("Invalid credentials.");

            user.LastLogin = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return Ok(new LoginResponse(user.Id, user.Name, user.Status));
        }

        // PATCH: api/users/5/block
        [HttpPatch("{id}/block")]
        public async Task<IActionResult> BlockUser(int id) =>
            await UpdateUserStatus(id, "blocked");

        // DELETE: api/users/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            return await _context.Users.FindAsync(id) is { } user
                ? await DeleteAndSave(user)
                : NotFound();
        }

        private async Task<IActionResult> UpdateUserStatus(int id, string status)
        {
            if (await _context.Users.FindAsync(id) is not { } user)
                return NotFound();

            user.Status = status;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        private async Task<IActionResult> DeleteAndSave(User user)
        {
            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        private static UserResponse MapToResponse(User user) => new(
            user.Id,
            user.Name,
            user.Email,
            user.Status
        );
    }

    // Record types for immutability and value-based equality
    public record SignUpRequest(
        [property: Required, EmailAddress] string Email,
        [property: Required, MinLength(6)] string Password,
        [property: Required, MaxLength(100)] string Name);

    public record LoginRequest(string Email, string Password);

    public record LoginResponse(int Id, string Name, string? Status);

    public record UserResponse(int Id, string Name, string Email, string Status);
}