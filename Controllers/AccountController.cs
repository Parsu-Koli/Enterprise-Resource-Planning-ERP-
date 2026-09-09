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

        #region Login

        // GET: /Account/Login
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }


        // POST: /Account/Login
        [HttpPost]
        public async Task<IActionResult> Login(LoginVM model)
        {
            if (!ModelState.IsValid)
                return View(model);


            // -----------------------------------------------------
            // Find user
            // -----------------------------------------------------

            var passwordHash = model.Password;

            var user = await _context.Users
                .FirstOrDefaultAsync(u =>
                    u.UserName == model.UserName &&
                    u.PasswordHash == passwordHash &&
                    u.IsActive);


            // -----------------------------------------------------
            // Invalid login
            // -----------------------------------------------------

            if (user == null)
            {
                ModelState.AddModelError(
                    "",
                    "Invalid username or password"
                );

                return View(model);
            }


            // -----------------------------------------------------
            // Create authentication claims
            // -----------------------------------------------------

            var claims = new List<Claim>
            {
                new(ClaimTypes.Name, user.UserName),
                new(ClaimTypes.Role, user.Role),
                new("UserId", user.UserId.ToString())
            };


            // -----------------------------------------------------
            // Create identity
            // -----------------------------------------------------

            var identity = new ClaimsIdentity(
                claims,
                CookieAuthenticationDefaults.AuthenticationScheme
            );


            // -----------------------------------------------------
            // Sign in user
            // -----------------------------------------------------

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(identity)
            );


            // -----------------------------------------------------
            // Redirect according to role
            // -----------------------------------------------------

            return user.Role switch
            {
                "Admin" =>
                    RedirectToAction("Dashboard", "Admin"),

                "Applicant" =>
                    RedirectToAction("Jobs", "Applicant"),

                "HR" =>
                    RedirectToAction("Dashboard", "HR"),

                "Employee" =>
                    RedirectToAction("Dashboard", "Employee"),

                _ =>
                    RedirectToAction("Login")
            };
        }

        #endregion


        #region Registration

        // GET: /Account/Register
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }


        // POST: /Account/Register
        [HttpPost]
        public async Task<IActionResult> Register(User user)
        {
            if (!ModelState.IsValid)
                return View(user);


            // -----------------------------------------------------
            // Check duplicate username/email
            // -----------------------------------------------------

            bool exists = await _context.Users.AnyAsync(u =>
                u.UserName == user.UserName ||
                u.Email == user.Email
            );


            if (exists)
            {
                ModelState.AddModelError(
                    "",
                    "Username or Email already exists"
                );

                return View(user);
            }


            // -----------------------------------------------------
            // Set default user information
            // -----------------------------------------------------

            user.PasswordHash = user.PasswordHash;
            user.Role = user.Role;
            user.IsActive = true;
            user.CreatedDate = DateTime.UtcNow;


            // -----------------------------------------------------
            // Save user
            // -----------------------------------------------------

            _context.Users.Add(user);

            await _context.SaveChangesAsync();


            // -----------------------------------------------------
            // Redirect to Login
            // -----------------------------------------------------

            return RedirectToAction(nameof(Login));
        }

        #endregion


        #region Logout

        // GET: /Account/Logout
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync();

            return RedirectToAction(nameof(Login));
        }

        #endregion


        #region Access Denied

        // GET: /Account/AccessDenied
        public IActionResult AccessDenied()
        {
            return View();
        }

        #endregion
    }
}