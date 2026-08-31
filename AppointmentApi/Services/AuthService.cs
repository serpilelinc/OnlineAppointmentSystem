using AppointmentApi.Data;
using AppointmentApi.DTOs;
using AppointmentApi.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using AppointmentApi.Exceptions;
using AppointmentApi.UnitOfWork;

namespace AppointmentApi.Services
{
    public class AuthService
    {
        private readonly AppDbContext _context;
        private readonly PasswordHasher<User> _passwordHasher;
        private readonly IConfiguration _configuration;
        private readonly EmailService _emailService;
        private readonly IUnitOfWork _unitOfWork;

        public AuthService(
            AppDbContext context,
            IConfiguration configuration,
            EmailService emailService,
            IUnitOfWork unitOfWork)
        {
            _context = context;
            _configuration = configuration;
            _emailService = emailService;
            _unitOfWork = unitOfWork;
            _passwordHasher = new PasswordHasher<User>();
        }

        /// <summary>
        /// Yeni bir kullanıcının (Müşterinin) sisteme kayıt olması işlemini gerçekleştirir.
        /// </summary>
        /// <param name="dto">Kayıt ekranından gelen ad, soyad, e-posta ve şifre bilgileri.</param>
        /// <returns>Başarılı olursa üretilen JWT token'ı ve kullanıcı bilgilerini döner.</returns>
        public async Task<AuthResponseDto> RegisterAsync(RegisterDto dto)
        {
            // 1. Gelen e-posta adresindeki boşlukları temizleyip küçük harfe çeviriyoruz.
            var email = dto.Email.Trim().ToLower();

            // 2. Veritabanında bu e-posta adresiyle kayıtlı başka bir kullanıcı var mı kontrol ediyoruz.
            var emailExists = await _context.Users
                .AnyAsync(x => x.Email == email);

            if (emailExists)
            {
                // Eğer e-posta kullanımda ise hata (Exception) fırlatıp işlemi durduruyoruz.
                throw new ConflictException("Bu e-posta adresi zaten kayıtlı.");
            }

            // 3. Yeni bir kullanıcı nesnesi (User entity) oluşturuyoruz.
            // Varsayılan rol her zaman "Customer" (Müşteri) olarak atanır.
            var user = new User
            {
                FullName = dto.FullName.Trim(),
                Email = email,
                Role = "Customer"
            };

            // 4. ŞİFRE GÜVENLİĞİ: Kullanıcının girdiği düz şifreyi (örneğin: "123456") 
            // veritabanına olduğu gibi kaydetmiyoruz! PasswordHasher kullanarak geri döndürülemez
            // bir Hash (Karmaşık metin) haline getiriyoruz.
            user.PasswordHash = _passwordHasher.HashPassword(user, dto.Password);

            // 5. Kullanıcıyı Entity Framework üzerinden veritabanına ekliyoruz.
            _context.Users.Add(user);
            
            // 6. Güvenli oturum yönetimi için (Beni Hatırla özelliği) bir Refresh Token oluşturuyoruz.
            var refreshToken = GenerateRefreshToken();
            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7); // Token 7 gün geçerli.
            
            // 7. Tüm bu değişiklikleri veritabanına kaydediyoruz.
            await _unitOfWork.SaveChangesAsync();

            // 8. Ön yüze (Web veya Mobil) döndürülecek yanıtı hazırlıyoruz.
            // İşlem bitiminde kullanıcıya, sisteme giriş yapabilmesi için bir JWT (Json Web Token) üretiyoruz.
            return new AuthResponseDto
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                Role = user.Role,
                Token = GenerateJwtToken(user),
                RefreshToken = refreshToken
            };
        }

        /// <summary>
        /// Kullanıcının e-posta ve şifre ile sisteme giriş (Oturum açma) işlemini gerçekleştirir.
        /// </summary>
        /// <param name="dto">Giriş formundan gelen e-posta ve şifre bilgileri.</param>
        /// <returns>Bilgiler doğruysa JWT Token döner, yanlışsa null döner.</returns>
        public async Task<AuthResponseDto?> LoginAsync(LoginDto dto)
        {
            var email = dto.Email.Trim().ToLower();

            // 1. Veritabanından bu e-postaya sahip bir kullanıcı (User) arıyoruz.
            var user = await _context.Users
                .FirstOrDefaultAsync(x => x.Email == email);

            // Eğer böyle bir e-posta yoksa null döndürüp işlemi bitiriyoruz.
            if (user == null)
            {
                return null;
            }

            // 2. ŞİFRE DOĞRULAMA: Kullanıcının forma girdiği düz şifre ile, 
            // veritabanındaki Hash'lenmiş (şifrelenmiş) şifreyi karşılaştırıyoruz.
            var result = _passwordHasher.VerifyHashedPassword(
                user,
                user.PasswordHash,
                dto.Password
            );

            // Eğer şifreler eşleşmiyorsa (Failed), işlemi sonlandırıp null dönüyoruz.
            if (result == PasswordVerificationResult.Failed)
            {
                return null;
            }

            // 3. Giriş başarılı! Kullanıcının oturum süresini uzatmak için yeni bir Refresh Token üretiyoruz.
            var refreshToken = GenerateRefreshToken();
            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
            
            // Refresh Token'ı veritabanında güncelliyoruz.
            await _unitOfWork.SaveChangesAsync();

            // 4. API'ye yapılan diğer isteklerde (Randevu alma vb.) kullanılmak üzere 
            // kullanıcının kimlik kartı niteliğindeki JWT Token'ı üretip Ön Yüze (Web/Mobil) dönüyoruz.
            // Ayrıca kullanıcının kişisel ayarlarını (Telefon, Bildirim tercihleri) da pakete ekliyoruz.
            return new AuthResponseDto
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                Role = user.Role,
                Token = GenerateJwtToken(user),
                RefreshToken = refreshToken,
                Phone = user.Phone,
                EmailNotificationsEnabled = user.EmailNotificationsEnabled,
                SmsNotificationsEnabled = user.SmsNotificationsEnabled
            };
        }
        public async Task<AuthResponseDto> CreateStaffUserAsync(
    CreateStaffUserDto dto)
        {
            var staff = await _context.Staffs
                .FirstOrDefaultAsync(x => x.Id == dto.StaffId);

            if (staff == null)
            {
                throw new NotFoundException(
    "Personel bulunamadı."
);
            }

            if (staff.UserId != null)
            {
                throw new ConflictException(
    "Bu personel için zaten bir kullanıcı hesabı oluşturulmuş."
);
            }

            var email = staff.Email.Trim().ToLower();

            var emailExists = await _context.Users
                .AnyAsync(x => x.Email == email);

            if (emailExists)
            {
                throw new ConflictException(
    "Bu e-posta adresiyle kayıtlı bir kullanıcı zaten var."
);
            }

            await _unitOfWork.BeginTransactionAsync();
            try
            {
                var user = new User
                {
                    FullName = staff.FullName,
                    Email = email,
                    Role = "Staff"
                };

                user.PasswordHash =
                    _passwordHasher.HashPassword(
                        user,
                        dto.Password
                    );

                _context.Users.Add(user);

                await _unitOfWork.SaveChangesAsync();

                staff.UserId = user.Id;

                var refreshToken = GenerateRefreshToken();
                user.RefreshToken = refreshToken;
                user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
                
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitAsync();

                return new AuthResponseDto
                {
                    Id = user.Id,
                    FullName = user.FullName,
                    Email = user.Email,
                    Role = user.Role,
                    Token = GenerateJwtToken(user),
                    RefreshToken = refreshToken,
                    Phone = user.Phone,
                    EmailNotificationsEnabled = user.EmailNotificationsEnabled,
                    SmsNotificationsEnabled = user.SmsNotificationsEnabled
                };
            }
            catch
            {
                await _unitOfWork.RollbackAsync();
                throw;
            }
        }
        public async Task<bool> StaffHasUserAccountAsync(int staffId)
        {
            var staff = await _context.Staffs
                .FirstOrDefaultAsync(x => x.Id == staffId);

            if (staff == null)
            {
                throw new Exception("Personel bulunamadı.");
            }

            return staff.UserId != null;
        }
        private string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using var rng = System.Security.Cryptography.RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }

        private string GenerateJwtToken(User user)
        {
            var key = _configuration["Jwt:Key"]
                ?? throw new InvalidOperationException("JWT Key bulunamadı.");

            var issuer = _configuration["Jwt:Issuer"];
            var audience = _configuration["Jwt:Audience"];

            var expireMinutes =
                int.TryParse(
                    _configuration["Jwt:ExpireMinutes"],
                    out var minutes)
                    ? minutes
                    : 120;

            var claims = new List<Claim>
    {
        new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
        new Claim(ClaimTypes.Name, user.FullName),
        new Claim(ClaimTypes.Email, user.Email),
        new Claim(ClaimTypes.Role, user.Role),
        new Claim("Phone", user.Phone ?? ""),
        new Claim("EmailNotificationsEnabled", user.EmailNotificationsEnabled.ToString()),
        new Claim("SmsNotificationsEnabled", user.SmsNotificationsEnabled.ToString())
    };

            var securityKey =
                new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(key)
                );

            var credentials =
                new SigningCredentials(
                    securityKey,
                    SecurityAlgorithms.HmacSha256
                );

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(expireMinutes),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler()
                .WriteToken(token);
        }

        public async Task<AuthResponseDto?> RefreshTokenAsync(TokenApiDto dto)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.RefreshToken == dto.RefreshToken);

            if (user == null || user.RefreshTokenExpiryTime <= DateTime.UtcNow)
            {
                return null;
            }

            var newAccessToken = GenerateJwtToken(user);
            var newRefreshToken = GenerateRefreshToken();

            user.RefreshToken = newRefreshToken;
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
            await _unitOfWork.SaveChangesAsync();

            return new AuthResponseDto
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                Role = user.Role,
                Token = newAccessToken,
                RefreshToken = newRefreshToken
            };
        }

        public async Task<bool> ForgotPasswordAsync(ForgotPasswordDto dto, string resetUrlBase)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
            if (user == null) return true; // Güvenlik sebebiyle email var mı yok mu belli etmemek için.

            var resetToken = GenerateRefreshToken(); // Rastgele güvenli token üretmek için aynı metodu kullanabiliriz
            user.ResetPasswordToken = resetToken;
            user.ResetPasswordTokenExpiry = DateTime.UtcNow.AddHours(1);
            await _unitOfWork.SaveChangesAsync();

            var encodedToken = Uri.EscapeDataString(resetToken);
            var resetLink = $"{resetUrlBase}?token={encodedToken}&email={user.Email}";
            
            await _emailService.SendEmailAsync(user.Email, "RandevuPlus - Şifre Sıfırlama Talebi", 
                $"<div style='font-family: Arial, sans-serif; padding: 20px;'>" +
                $"<h2>Şifre Sıfırlama</h2>" +
                $"<p>Merhaba {user.FullName}, şifrenizi sıfırlamak için aşağıdaki butona tıklayın:</p>" +
                $"<a href='{resetLink}' style='background-color: #0d6efd; color: white; padding: 10px 20px; text-decoration: none; border-radius: 5px; display: inline-block; margin-top: 10px;'>Şifremi Sıfırla</a>" +
                $"<p style='margin-top: 20px; font-size: 12px; color: gray;'>Eğer bu talebi siz yapmadıysanız bu e-postayı dikkate almayın.</p></div>");

            return true;
        }

        public async Task<bool> ResetPasswordAsync(ResetPasswordDto dto)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email && u.ResetPasswordToken == dto.Token);

            if (user == null || user.ResetPasswordTokenExpiry <= DateTime.UtcNow)
            {
                return false;
            }

            user.PasswordHash = _passwordHasher.HashPassword(user, dto.NewPassword);
            user.ResetPasswordToken = null;
            user.ResetPasswordTokenExpiry = null;

            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// Kullanıcının kendi şifresini güvenli bir şekilde değiştirmesini sağlar.
        /// </summary>
        public async Task<bool> ChangePasswordAsync(int userId, string currentPassword, string newPassword)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return false;

            // 1. Eski şifreyi doğrula
            var result = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, currentPassword);
            if (result == PasswordVerificationResult.Failed)
            {
                // Eski şifre yanlışsa işlem iptal
                return false; 
            }

            // 2. Yeni şifreyi Hash'leyerek kaydet
            user.PasswordHash = _passwordHasher.HashPassword(user, newPassword);
            await _unitOfWork.SaveChangesAsync();

            return true;
        }
    }
}