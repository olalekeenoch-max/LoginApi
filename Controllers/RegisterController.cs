using LoginApi.Models;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace LoginApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RegisterController : ControllerBase
    {

        // TASK 2 — REGISTER USER
        [HttpPost]
        public IActionResult Register(RegisterRequest request)
        {
            string emailPattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";

            if (!Regex.IsMatch(request.Email, emailPattern))
            {
                return BadRequest(new { message = "Invalid email format" });
            }

            string passwordPattern =
            @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[\W_]).{8,}$";

            if (!Regex.IsMatch(request.Password, passwordPattern))
            {
                return BadRequest(new
                {
                    message = "Password must contain uppercase, lowercase, number, special character and be 8+ characters"
                });
            }

            string filePath =
            Path.Combine(Directory.GetCurrentDirectory(), "Data", "users.json");

            if (!System.IO.File.Exists(filePath))
            {
                return StatusCode(500, "User database not found");
            }

            string jsonData =
            System.IO.File.ReadAllText(filePath);

            var users =
            JsonSerializer.Deserialize<List<RegisterRequest>>(jsonData)
            ?? new List<RegisterRequest>();

            if (users.Any(u => u.Email == request.Email))
            {
                return BadRequest(new { message = "Email already exists" });
            }

            request.IsActive = true;

            users.Add(request);

            string updatedJson =
            JsonSerializer.Serialize(users,
            new JsonSerializerOptions
            {
                WriteIndented = true
            });

            System.IO.File.WriteAllText(filePath, updatedJson);

            return Ok(new { message = "Registration successful" });
        }



        // TASK 3 — GET USERS
        [HttpGet]
        public IActionResult GetUsers(int pageNumber = 1, int pageSize = 5)
        {
            string filePath =
            Path.Combine(Directory.GetCurrentDirectory(), "Data", "users.json");

            if (!System.IO.File.Exists(filePath))
            {
                return StatusCode(500, "User database not found");
            }

            string jsonData =
            System.IO.File.ReadAllText(filePath);

            var users =
            JsonSerializer.Deserialize<List<RegisterRequest>>(jsonData)
            ?? new List<RegisterRequest>();

            var activeUsers =
            users.Where(u => u.IsActive == true);

            var pagedUsers =
            activeUsers
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize);

            var result = new
            {
                TotalUsers = activeUsers.Count(),
                CurrentPage = pageNumber,
                PageSize = pageSize,
                Users = pagedUsers
            };

            return Ok(result);
        }



        // TASK 4 — UPDATE PASSWORD
        [HttpPut]
        [Route("UpdatePassword")]
        public IActionResult UpdatePassword(UpdatePasswordRequest request)
        {
            string filePath =
            Path.Combine(Directory.GetCurrentDirectory(), "Data", "users.json");

            if (!System.IO.File.Exists(filePath))
            {
                return StatusCode(500, "User database not found");
            }

            string jsonData =
            System.IO.File.ReadAllText(filePath);

            var users =
            JsonSerializer.Deserialize<List<RegisterRequest>>(jsonData)
            ?? new List<RegisterRequest>();

            var user =
            users.FirstOrDefault(u => u.Email == request.Email);

            if (user == null)
            {
                return NotFound(new { message = "User not found" });
            }

            if (user.Password != request.OldPassword)
            {
                return BadRequest(new { message = "Old password is incorrect" });
            }

            if (request.OldPassword == request.NewPassword)
            {
                return BadRequest(new
                {
                    message = "New password cannot be the same as old password"
                });
            }

            string passwordPattern =
            @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[\W_]).{8,}$";

            if (!Regex.IsMatch(request.NewPassword, passwordPattern))
            {
                return BadRequest(new
                {
                    message = "Password must contain uppercase, lowercase, number, special character and be 8+ characters"
                });
            }

            user.Password = request.NewPassword;

            string updatedJson =
            JsonSerializer.Serialize(users,
            new JsonSerializerOptions
            {
                WriteIndented = true
            });

            System.IO.File.WriteAllText(filePath, updatedJson);

            return Ok(new { message = "Password updated successfully" });
        }

    }
}