using ERP.Data;
using ERP.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace ERP.Controllers
{
    public class AccountController(ERPDbContext context) : Controller
    {
        private readonly ERPDbContext _context = context;

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }


        [HttpPost]
        public async Task<IActionResult> Register(User user)
        {
            if (!ModelState.IsValid)
                return View(user);

            bool exists = await _context.Users.AnyAsync(u =>
                u.UserName == user.UserName || u.Email == user.Email);

            if (exists)
            {
                ModelState.AddModelError("", "Username or Email already exists");
                return View(user);
            }

            user.PasswordHash = user.PasswordHash;
            user.Role = user.Role;
            user.IsActive = true;
            user.CreatedDate = DateTime.Now;

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return RedirectToAction("Login");
        }


        [HttpPost]
        public async Task<IActionResult> Login(LoginVM model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var passwordHash = model.Password;

            var user = await _context.Users
                .FirstOrDefaultAsync(u =>
                    u.UserName == model.UserName &&
                    u.PasswordHash == passwordHash &&
                    u.IsActive);

            if (user == null)
            {
                ModelState.AddModelError("", "Invalid username or password");
                return View(model);
            }

            var claims = new List<Claim>
            {
                new(ClaimTypes.Name, user.UserName),
                new(ClaimTypes.Role, user.Role),
                new("UserId", user.UserId.ToString())
            };

            var identity = new ClaimsIdentity(
                claims, CookieAuthenticationDefaults.AuthenticationScheme);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(identity));

            

            return user.Role switch
            {
                "Admin" => RedirectToAction("Dashboard", "Admin"),
                "Applicant" => RedirectToAction("Jobs", "Applicant"),
                "HR" => RedirectToAction("Dashboard", "HR"),
                "Employee" => RedirectToAction("Dashboard", "Employee"),
                _ => RedirectToAction("Login"),
                
            };
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync();
            return RedirectToAction("Login");
        }

        public IActionResult AccessDenied()
        {
            return View();
        }


    }
}
