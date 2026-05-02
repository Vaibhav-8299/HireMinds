using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using HireMindsAPI.DTOs;
using HireMindsAPI.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace HireMindsAPI.Services
{
    public class AuthService
    {
        private readonly AppDbContext _db;
        private readonly IConfiguration _config;
        private readonly IEmailService _emailService;

        public AuthService(AppDbContext db, IConfiguration config, IEmailService emailService)
        {
            _db = db;
            _config = config;
            _emailService = emailService;
        }

        // ==================== REGISTER ====================
        public async Task<string> RegisterAsync(RegisterDto dto)
        {
            var existingUser = await _db.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
            if (existingUser != null)
            {
                // If the user exists but is not verified, we could just resend the OTP.
                // For simplicity, we just say it exists. (A better approach is to resend OTP if IsEmailVerified == false).
                throw new ArgumentException("A user with this email already exists.");
            }

            if (dto.Role != "Candidate" && dto.Role != "Recruiter")
            {
                throw new ArgumentException("Role must be 'Candidate' or 'Recruiter'.");
            }

            var passwordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);
            
            // Generate 4-digit OTP
            var random = new Random();
            string otp = random.Next(1000, 9999).ToString();

            var user = new User
            {
                FullName = dto.FullName,
                Email = dto.Email,
                PasswordHash = passwordHash,
                Role = dto.Role,
                CreatedAt = DateTime.UtcNow,
                IsEmailVerified = false,
                EmailOtp = otp,
                OtpExpiry = DateTime.UtcNow.AddMinutes(10)
            };

            _db.Users.Add(user);
            await _db.SaveChangesAsync();

            if (dto.Role == "Candidate")
            {
                var profile = new Candidateprofile
                {
                    UserId = user.Id
                };
                _db.Candidateprofiles.Add(profile);
                await _db.SaveChangesAsync();
            }

            // Send Email
            string emailBody = $@"
                <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; padding: 20px; background-color: #f4f4f4;'>
                    <div style='background-color: white; padding: 30px; border-radius: 8px; box-shadow: 0 4px 6px rgba(0,0,0,0.1);'>
                        <h2 style='color: #4F46E5; text-align: center;'>Welcome to HireMinds!</h2>
                        <p style='color: #333; font-size: 16px;'>Hi {user.FullName},</p>
                        <p style='color: #333; font-size: 16px;'>Thank you for registering. Please use the following 4-digit code to verify your email address. This code will expire in 10 minutes.</p>
                        <div style='background-color: #EEF2FF; padding: 15px; text-align: center; border-radius: 6px; margin: 20px 0;'>
                            <span style='font-size: 32px; font-weight: bold; letter-spacing: 5px; color: #4F46E5;'>{otp}</span>
                        </div>
                        <p style='color: #777; font-size: 14px; text-align: center;'>If you did not request this, please ignore this email.</p>
                    </div>
                </div>";

            await _emailService.SendEmailAsync(user.Email, "Verify your HireMinds Account", emailBody);

            return "Registration successful. Please verify your email.";
        }

        // ==================== VERIFY EMAIL ====================
        public async Task<LoginResponseDto> VerifyEmailAsync(string email, string otp)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null) throw new ArgumentException("User not found.");

            if (user.IsEmailVerified == true) throw new ArgumentException("Email is already verified.");
            
            if (user.EmailOtp != otp) throw new ArgumentException("Invalid OTP.");
            
            if (user.OtpExpiry < DateTime.UtcNow) throw new ArgumentException("OTP has expired. Please register again."); // A real app would have a ResendOTP endpoint

            // Mark as verified
            user.IsEmailVerified = true;
            user.EmailOtp = null;
            user.OtpExpiry = null;
            await _db.SaveChangesAsync();

            // Auto-login after verification
            var token = GenerateJwtToken(user);
            return new LoginResponseDto
            {
                Token = token,
                Role = user.Role,
                FullName = user.FullName
            };
        }

        // ==================== LOGIN ====================
        public async Task<LoginResponseDto> LoginAsync(LoginDto dto)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
            if (user == null)
            {
                throw new ArgumentException("Invalid email or password.");
            }

            if (user.IsEmailVerified == false)
            {
                throw new ArgumentException("Please verify your email before logging in.");
            }

            var isPasswordValid = BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash);
            if (!isPasswordValid)
            {
                throw new ArgumentException("Invalid email or password.");
            }

            var token = GenerateJwtToken(user);

            return new LoginResponseDto
            {
                Token = token,
                Role = user.Role,
                FullName = user.FullName
            };
        }

        // ==================== GENERATE JWT TOKEN ====================
        private string GenerateJwtToken(User user)
        {
            var key = _config["Jwt:Key"]!;
            var issuer = _config["Jwt:Issuer"]!;
            var audience = _config["Jwt:Audience"]!;
            var expiryMinutes = int.Parse(_config["Jwt:ExpiryInMinutes"] ?? "60");

            var claims = new[]
            {
                new Claim("UserId", user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Name, user.FullName),
                new Claim(ClaimTypes.Role, user.Role)
            };

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(expiryMinutes),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
