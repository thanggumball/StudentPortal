using StudentPortal.Common.DTOs.Auth;
using StudentPortal.Common.DTOs.User;
using StudentPortal.Common.Enums;
using StudentPortal.Repository.Entities;
using StudentPortal.Repository.Interfaces;
using StudentPortal.Service.Interfaces;

namespace StudentPortal.Service.Implementations;

public class AuthService : IAuthService
{
    private const string DefaultStudentRole = "Student";

    private readonly IUserRepository _userRepository;
    private readonly IRoleRepository _roleRepository;
    private readonly IUnitOfWork _unitOfWork;

    public AuthService(
        IUserRepository userRepository,
        IRoleRepository roleRepository,
        IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _roleRepository = roleRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<UserResponse> RegisterAsync(
        RegisterRequest request,
        CancellationToken ct = default)
    {
        var emailExists = await _userRepository.ExistsByEmailAsync(
            request.Email,
            ct);

        if (emailExists)
        {
            throw new InvalidOperationException(
                "An account with this email already exists.");
        }

        var userNameExists = await _userRepository.FindByUserNameAsync(
            request.UserName,
            ct);

        if (userNameExists is not null)
        {
            throw new InvalidOperationException(
                "An account with this username already exists.");
        }

        var role = await _roleRepository.FindByNameAsync(
            DefaultStudentRole,
            ct);

        if (role is null)
        {
            throw new InvalidOperationException(
                "Default Student role was not found.");
        }

        var user = new User
        {
            Email = request.Email,
            UserName = request.UserName,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            FullName = request.FullName,
            StudentCode = request.StudentCode,
            RoleId = role.Id,
            Role = role,
            Status = UserStatus.Active,
            IsDeleted = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _userRepository.AddAsync(user, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return new UserResponse
        {
            Id = user.Id,
            Email = user.Email,
            UserName = user.UserName,
            FullName = user.FullName,
            StudentCode = user.StudentCode,
            RoleName = role.Name,
            Status = user.Status,
            AvatarUrl = user.AvatarUrl,
            LastLoginAt = user.LastLoginAt,
            CreatedAt = user.CreatedAt
        };
    }

    public Task<LoginResponse> LoginAsync(
        LoginRequest request,
        CancellationToken ct = default)
        => throw new NotImplementedException();

    public Task<LoginResponse> RefreshTokenAsync(
        RefreshTokenRequest request,
        CancellationToken ct = default)
        => throw new NotImplementedException();

    public Task LogoutAsync(
        LogoutRequest request,
        CancellationToken ct = default)
        => throw new NotImplementedException();

    public Task ChangePasswordAsync(
        Guid userId,
        ChangePasswordRequest request,
        CancellationToken ct = default)
        => throw new NotImplementedException();
}