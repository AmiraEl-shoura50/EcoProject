using ECommerce.Application.DTOs.Auth;

namespace ECommerce.Application.Interfaces.Services;

public interface IAuthService
{
    Task<AuthResponseDto> RegisterAsync(RegisterDto dto);
    Task<AuthResponseDto> LoginAsync(LoginDto dto);
    Task<AuthResponseDto> RefreshTokenAsync(RefreshTokenRequestDto dto); // ✅ جديد
    Task<bool> RevokeTokenAsync(string refreshToken); // ✅ جديد - للـ Logout
    Task<AuthResponseDto> ForgotPasswordAsync(ForgotPasswordDto dto);
    Task<AuthResponseDto> ResetPasswordAsync(ResetPasswordDto dto);

    Task<AuthResponseDto> ChangePasswordAsync(Guid userId, ChangePasswordDto dto);
}