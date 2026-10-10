using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using ECommerce.Application.DTOs.Auth;
using ECommerce.Application.Interfaces.Repositories;
using ECommerce.Application.Interfaces.Services;
using ECommerce.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace ECommerce.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole<Guid>> _roleManager;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEmailService _emailService;
    private readonly IConfiguration _configuration;

    public AuthService(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole<Guid>> roleManager,
        IUnitOfWork unitOfWork,
        IEmailService emailService,
        IConfiguration configuration)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _unitOfWork = unitOfWork;
        _emailService = emailService;
        _configuration = configuration;
    }

    public async Task<AuthResponseDto> RegisterAsync(RegisterDto dto)
    {
        // ✅ Validation مبكر قبل ما نعمل أي حاجة في الداتابيز
        if (dto.Role == "Customer" && string.IsNullOrWhiteSpace(dto.Address))
        {
            return new AuthResponseDto { Success = false, Message = "العنوان مطلوب لحساب العميل" };
        }

        if (dto.Role == "Seller" && string.IsNullOrWhiteSpace(dto.StoreName))
        {
            return new AuthResponseDto { Success = false, Message = "اسم المتجر مطلوب لحساب البائع" };
        }

        var existingUser = await _userManager.FindByEmailAsync(dto.Email);
        if (existingUser is not null)
        {
            return new AuthResponseDto { Success = false, Message = "الإيميل ده مستخدم بالفعل" };
        }

        var user = new ApplicationUser
        {
            UserName = dto.Email,
            Email = dto.Email,
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            PhoneNumber = dto.PhoneNumber
        };

        var result = await _userManager.CreateAsync(user, dto.Password);
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            return new AuthResponseDto { Success = false, Message = errors };
        }

        if (!await _roleManager.RoleExistsAsync(dto.Role))
        {
            await _roleManager.CreateAsync(new IdentityRole<Guid>(dto.Role));
        }

        await _userManager.AddToRoleAsync(user, dto.Role);

        // ✅ إنشاء Customer أو Seller حسب الـ Role
        if (dto.Role == "Customer")
        {
            var customer = new Customer
            {
                UserId = user.Id,
                Address = dto.Address!
            };
            await _unitOfWork.Customers.AddAsync(customer);
        }
        else if (dto.Role == "Seller")
        {
            var seller = new Seller
            {
                UserId = user.Id,
                StoreName = dto.StoreName!,
                Rating = 0
            };
            await _unitOfWork.Sellers.AddAsync(seller);
        }

        await _unitOfWork.SaveChangesAsync();

        // ✅ إرسال إيميل ترحيب - Fire and forget عشان مبطئش الـ Response
        _ = _emailService.SendEmailAsync(
            user.Email!,
            "أهلًا بيك/ي في متجرنا 🎉",
            $"<h2>أهلًا {user.FirstName}!</h2><p>تم إنشاء حسابك بنجاح.</p>");

        return await GenerateAuthResponseAsync(user, "تم إنشاء الحساب بنجاح");
    }

    public async Task<AuthResponseDto> LoginAsync(LoginDto dto)
    {
        var user = await _userManager.FindByEmailAsync(dto.Email);
        if (user is null || !await _userManager.CheckPasswordAsync(user, dto.Password))
        {
            return new AuthResponseDto { Success = false, Message = "الإيميل أو الباسورد غلط" };
        }

        if (!user.IsActive)
        {
            return new AuthResponseDto { Success = false, Message = "الحساب ده متوقف" };
        }

        return await GenerateAuthResponseAsync(user, "تم تسجيل الدخول بنجاح");
    }

    public async Task<AuthResponseDto> RefreshTokenAsync(RefreshTokenRequestDto dto)
    {
        var storedTokens = await _unitOfWork.RefreshTokens.FindAsync(rt => rt.Token == dto.RefreshToken);
        var storedToken = storedTokens.FirstOrDefault();

        if (storedToken is null || !storedToken.IsActive)
        {
            return new AuthResponseDto { Success = false, Message = "الجلسة منتهية، برجاء تسجيل الدخول مرة أخرى" };
        }

        var user = await _userManager.FindByIdAsync(storedToken.UserId.ToString());
        if (user is null || !user.IsActive)
        {
            return new AuthResponseDto { Success = false, Message = "الحساب غير موجود أو متوقف" };
        }

        // ✅ نلغي الـ Refresh Token القديم (Rotation - ميزة أمان إضافية)
        storedToken.IsRevoked = true;
        _unitOfWork.RefreshTokens.Update(storedToken);

        return await GenerateAuthResponseAsync(user, "تم تجديد الجلسة بنجاح");
    }

    public async Task<bool> RevokeTokenAsync(string refreshToken)
    {
        var storedTokens = await _unitOfWork.RefreshTokens.FindAsync(rt => rt.Token == refreshToken);
        var storedToken = storedTokens.FirstOrDefault();

        if (storedToken is null || !storedToken.IsActive) return false;

        storedToken.IsRevoked = true;
        _unitOfWork.RefreshTokens.Update(storedToken);
        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    // ✅ Helper موحد - بيولد Access Token + Refresh Token مع بعض
    private async Task<AuthResponseDto> GenerateAuthResponseAsync(ApplicationUser user, string message)
    {
        var (accessToken, expiresAt) = await GenerateJwtTokenAsync(user);

        var refreshToken = new RefreshToken
        {
            UserId = user.Id,
            Token = GenerateSecureRefreshToken(),
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            IsRevoked = false
        };

        await _unitOfWork.RefreshTokens.AddAsync(refreshToken);
        await _unitOfWork.SaveChangesAsync();

        return new AuthResponseDto
        {
            Success = true,
            Message = message,
            Token = accessToken,
            ExpiresAt = expiresAt,
            RefreshToken = refreshToken.Token
        };
    }

    // ✅ توليد Random String آمن (مش JWT - بس Token عشوائي طويل)
    private static string GenerateSecureRefreshToken()
    {
        var randomBytes = RandomNumberGenerator.GetBytes(64);
        return Convert.ToBase64String(randomBytes);
    }

    private async Task<(string Token, DateTime ExpiresAt)> GenerateJwtTokenAsync(ApplicationUser user)
    {
        var roles = await _userManager.GetRolesAsync(user);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Email, user.Email!),
            new(ClaimTypes.GivenName, user.FirstName)
        };

        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var expiryMinutes = int.Parse(_configuration["Jwt:ExpiryMinutes"] ?? "60");
        var expiresAt = DateTime.UtcNow.AddMinutes(expiryMinutes);

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: expiresAt,
            signingCredentials: creds);

        return (new JwtSecurityTokenHandler().WriteToken(token), expiresAt);

    }
    public async Task<AuthResponseDto> ForgotPasswordAsync(ForgotPasswordDto dto)
    {
        var user = await _userManager.FindByEmailAsync(dto.Email);

        // ✅ نفس الرسالة سواء الإيميل موجود أو لأ - حماية من Enumeration Attack
        const string genericMessage = "لو الإيميل ده مسجل عندنا، هيوصلك رابط إعادة تعيين كلمة المرور";

        if (user is null)
        {
            return new AuthResponseDto { Success = true, Message = genericMessage };
        }

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        var encodedToken = Uri.EscapeDataString(token);
        var encodedEmail = Uri.EscapeDataString(user.Email!);

        var clientBaseUrl = _configuration["ClientApp:BaseUrl"];
        var resetLink = $"{clientBaseUrl}/reset-password?email={encodedEmail}&token={encodedToken}";

        _ = _emailService.SendEmailAsync(
            user.Email!,
            "إعادة تعيين كلمة المرور 🔑",
            $"<p>مرحبًا {user.FirstName}،</p><p>اضغط على الرابط ده عشان تعيد تعيين كلمة المرور بتاعتك:</p><p><a href='{resetLink}'>إعادة تعيين كلمة المرور</a></p><p>لو انت مطلبتش الطلب ده، تجاهل الإيميل ده ببساطة.</p>");

        return new AuthResponseDto { Success = true, Message = genericMessage };
    }

    public async Task<AuthResponseDto> ResetPasswordAsync(ResetPasswordDto dto)
    {
        var user = await _userManager.FindByEmailAsync(dto.Email);
        if (user is null)
        {
            return new AuthResponseDto { Success = false, Message = "الرابط غير صالح أو منتهي الصلاحية" };
        }

        var result = await _userManager.ResetPasswordAsync(user, dto.Token, dto.NewPassword);
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            return new AuthResponseDto { Success = false, Message = errors };
        }

        return new AuthResponseDto { Success = true, Message = "تم تغيير كلمة المرور بنجاح، يمكنك تسجيل الدخول الآن" };
    }
    public async Task<AuthResponseDto> ChangePasswordAsync(Guid userId, ChangePasswordDto dto)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user is null)
            return new AuthResponseDto { Success = false, Message = "المستخدم غير موجود" };

        var result = await _userManager.ChangePasswordAsync(user, dto.CurrentPassword, dto.NewPassword);
        if (!result.Succeeded)
        {
            var errors = string.Join("، ", result.Errors.Select(TranslateIdentityError));
            return new AuthResponseDto { Success = false, Message = errors };
        }

        // ✅ نلغي كل الجلسات القديمة (Refresh Tokens)، ونصدر جلسة جديدة للجهاز الحالي
        var oldTokens = await _unitOfWork.RefreshTokens.FindAsync(rt => rt.UserId == user.Id && !rt.IsRevoked);
        foreach (var token in oldTokens)
        {
            token.IsRevoked = true;
            _unitOfWork.RefreshTokens.Update(token);
        }

        return await GenerateAuthResponseAsync(user, "تم تغيير كلمة المرور بنجاح");
    }

    private static string TranslateIdentityError(IdentityError error) => error.Code switch
    {
        "PasswordMismatch" => "كلمة المرور الحالية غير صحيحة",
        "PasswordTooShort" => "كلمة المرور قصيرة جدًا",
        "PasswordRequiresDigit" => "كلمة المرور يجب أن تحتوي على رقم",
        "PasswordRequiresLower" => "كلمة المرور يجب أن تحتوي على حرف صغير",
        "PasswordRequiresUpper" => "كلمة المرور يجب أن تحتوي على حرف كبير",
        "PasswordRequiresNonAlphanumeric" => "كلمة المرور يجب أن تحتوي على رمز خاص مثل @ أو #",
        _ => error.Description
    };
}