using LoginApi.Models;
using Microsoft.AspNetCore.Mvc;
using ProjecteE.Data;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace LoginApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RegisterController : ControllerBase
    {
        private readonly ApplicationDbContext _db;

        public RegisterController(ApplicationDbContext db)
        {
            _db = db;
        }

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

            // check existing email in database (with fallback to JSON file if DB is unavailable)
            bool emailExists = false;
            try
            {
                emailExists = _db.RegisterRequests.Any(u => u.Email == request.Email);
            }
            catch
            {
                // fallback to JSON file check
                try
                {
                    var filePath = System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), "Data", "users.json");
                    if (System.IO.File.Exists(filePath))
                    {
                        var jsonData = System.IO.File.ReadAllText(filePath);
                        var users = System.Text.Json.JsonSerializer.Deserialize<System.Collections.Generic.List<RegisterRequest>>(jsonData)
                                    ?? new System.Collections.Generic.List<RegisterRequest>();
                        emailExists = users.Any(u => u.Email == request.Email);
                    }
                }
                catch
                {
                    // ignore fallback errors
                }
            }

            if (emailExists)
            {
                return BadRequest(new { message = "Email already exists" });
            }

            request.IsActive = true;

            try
            {
                _db.RegisterRequests.Add(request);
                _db.SaveChanges();
                return Ok(new { message = "Registration successful" });
            }
            catch
            {
                // fallback to appending to JSON file when DB write fails
                try
                {
                    var filePath = System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), "Data", "users.json");
                    var users = new System.Collections.Generic.List<RegisterRequest>();
                    if (System.IO.File.Exists(filePath))
                    {
                        var jsonData = System.IO.File.ReadAllText(filePath);
                        users = System.Text.Json.JsonSerializer.Deserialize<System.Collections.Generic.List<RegisterRequest>>(jsonData)
                                ?? new System.Collections.Generic.List<RegisterRequest>();
                    }

                    users.Add(request);
                    var updatedJson = System.Text.Json.JsonSerializer.Serialize(users, new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
                    System.IO.File.WriteAllText(filePath, updatedJson);
                    return Ok(new { message = "Registration successful (saved to JSON fallback)" });
                }
                catch
                {
                    return StatusCode(500, new { message = "Unable to save registration" });
                }
            }
        }



        // TASK 3 — GET USERS
        [HttpGet]
        public IActionResult GetUsers(int pageNumber = 1, int pageSize = 5)
        {
            var activeUsersQuery = _db.RegisterRequests.AsNoTracking().Where(u => u.IsActive == true);

            var total = activeUsersQuery.Count();

            var users = activeUsersQuery
                .OrderBy(u => u.Id)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var result = new
            {
                TotalUsers = total,
                CurrentPage = pageNumber,
                PageSize = pageSize,
                Users = users
            };

            return Ok(result);
        }



        // TASK 4 — UPDATE PASSWORD
        [HttpPut]
        [Route("UpdatePassword")]
        public IActionResult UpdatePassword(UpdatePasswordRequest request)
        {
            var user = _db.RegisterRequests.FirstOrDefault(u => u.Email == request.Email);

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
            _db.SaveChanges();

            return Ok(new { message = "Password updated successfully" });
        }

    }
}