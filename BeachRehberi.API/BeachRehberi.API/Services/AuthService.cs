using BeachRehberi.API.Data;
using BeachRehberi.API.Models;
using Microsoft.EntityFrameworkCore;

namespace BeachRehberi.API.Services;

public class AuthService : IAuthService {
    private readonly BeachDbContext _db;
    public AuthService(BeachDbContext db) { _db = db; }

    public async Task<AuthResponse?> LoginAsync(string email, string password) {
        var user = await _db.BusinessUsers.FirstOrDefaultAsync(u => u.Email == email);
        if (user == null || user.PasswordHash != password) return null; // Gerçekte hashing olmalı
        return new AuthResponse { Email = user.Email, Token = "jwt-mock-token", Role = user.Role };
    }

    public async Task<BusinessUser?> RegisterAsync(RegisterRequest request) {
        if (await _db.BusinessUsers.AnyAsync(u => u.Email == request.Email)) return null;
        var user = new BusinessUser { 
            Email = request.Email, 
            PasswordHash = request.Password, 
            BusinessName = request.BusinessName,
            BeachId = request.BeachId,
            CreatedAt = DateTime.UtcNow 
        };
        _db.BusinessUsers.Add(user);
        await _db.SaveChangesAsync();
        return user;
    }
}