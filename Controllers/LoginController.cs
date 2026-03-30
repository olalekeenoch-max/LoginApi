using System.Linq;
using LoginApi.Models;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace LoginApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LoginController : ControllerBase
    {
        [HttpPost]
        public IActionResult Login(LoginRequest request)
        {
            // Validate email format
            string emailPattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";

            // Validate password format
            string passwordPattern = @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[\W_]).{8,}$";

            if (!Regex.IsMatch(request.Email, emailPattern))
            {
                return BadRequest(new { message = "Invalid email format" });
            }

            if (!Regex.IsMatch(request.Password, passwordPattern))
            {
                return BadRequest(new
                {
                    message = "Password must contain uppercase, lowercase, number, special character and be 8+ characters"
                });
            }

            // Read users from JSON file
            string filePath = Path.Combine(Directory.GetCurrentDirectory(), "Data", "users.json");

            if (!System.IO.File.Exists(filePath))
            {
                return StatusCode(500, "User database not found");
            }

            string jsonData = System.IO.File.ReadAllText(filePath);

            var users = JsonSerializer.Deserialize<List<RegisterRequest>>(jsonData);

            // check if user exists
            var user = users.FirstOrDefault(u =>
                u.Email == request.Email &&
                u.Password == request.Password);

            if (user == null)
            {
                return Unauthorized(new { message = "Invalid email or password" });
            }

            return Ok(new { message = "Login successful" });
        }
    }
}

