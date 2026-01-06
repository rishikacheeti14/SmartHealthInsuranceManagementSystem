using Microsoft.AspNetCore.Identity;
using SmartHealthInsurance.Api.DTOs;
using SmartHealthInsurance.Api.Enums;
using SmartHealthInsurance.Api.Helpers;
using SmartHealthInsurance.Api.Models;
using SmartHealthInsurance.Api.Repositories;

namespace SmartHealthInsurance.Api.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly IPasswordHasher<User> _passwordHasher;
        private readonly IJwtTokenHelper _jwtTokenHelper;

        public AuthService(IUserRepository userRepository, IPasswordHasher<User> passwordHasher, IJwtTokenHelper jwtTokenHelper)
        {
            _userRepository = userRepository;
            _passwordHasher = passwordHasher;
            _jwtTokenHelper = jwtTokenHelper;
        }

        public async Task<string> RegisterCustomerAsync(RegisterDto dto)
        {
            var existingUser = await _userRepository.GetByEmailAsync(dto.Email);
            if (existingUser != null)
            {

                var verify = _passwordHasher.VerifyHashedPassword(existingUser, existingUser.PasswordHash, "Welcome@123");
                
                if (verify == PasswordVerificationResult.Success)
                {
                    existingUser.FirstName = dto.FirstName;
                    existingUser.LastName = dto.LastName;
                    existingUser.PasswordHash = _passwordHasher.HashPassword(existingUser, dto.Password);
                }
                else
                {
                     throw new Exception("User already exists");
                }
            }
            else
            {
                var user = new User
                {
                    Email = dto.Email,
                    FirstName = dto.FirstName,
                    LastName = dto.LastName,
                    Role = UserRole.Customer,
                    IsActive = true
                };

                user.PasswordHash = _passwordHasher.HashPassword(user, dto.Password);

                await _userRepository.AddAsync(user);
            }

            return "Registered successfully";
        }

        public async Task<string> CreateStaffAsync(CreateUserDto dto)
        {
            if (await _userRepository.GetByEmailAsync(dto.Email) != null)
                throw new Exception("User already exists");

            if (!Enum.TryParse<UserRole>(dto.Role, true, out var parsedRole))
                throw new Exception("Invalid role");

            var user = new User
            {
                Email = dto.Email,
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                Role = parsedRole,
                IsActive = true
            };

            string password = dto.Password; 
            user.PasswordHash = _passwordHasher.HashPassword(user, password);

            await _userRepository.AddAsync(user);
            return $"{dto.Role} created successfully";
        }

        public async Task<AuthResponseDto> LoginAsync(LoginDto dto)
        {
            var user = await _userRepository.GetByEmailAsync(dto.Email);

            if (user == null)
            {
                throw new UnauthorizedAccessException("Invalid credentials");
            }

            if (!user.IsActive)
            {
                throw new UnauthorizedAccessException("Invalid credentials");
            }

            var result = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, dto.Password);

            if (result == PasswordVerificationResult.Failed)
            {
                 throw new UnauthorizedAccessException("Invalid credentials");
            }

            var token = _jwtTokenHelper.GenerateToken(user);

            return new AuthResponseDto
            {
                Token = token,
                Role = user.Role.ToString()
            };
        }
    }
}
