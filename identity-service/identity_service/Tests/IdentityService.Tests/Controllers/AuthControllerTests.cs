using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Bogus;
using IdentityService.Core.DTOs;
using IdentityService.Core.Services;
using IdentityService.Endpoints.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace IdentityService.Tests.Controllers;

public class AuthControllerTests
{
    private static readonly Faker Fake = new();

    private static UserType MakeUser(int id = 1) => new()
    {
        Id = id,
        Email = Fake.Internet.Email(),
        FullName = Fake.Name.FullName(),
        PhoneNumber = "+1234567890",
        Role = "User",
        CreatedAt = DateTime.UtcNow
    };

    private static AuthController BuildController(
        Mock<IAuthService>? authSvc = null,
        Mock<IUserAccountService>? userSvc = null,
        Mock<IPasswordService>? passSvc = null,
        int? authenticatedUserId = null)
    {
        var controller = new AuthController(
            (authSvc ?? new Mock<IAuthService>()).Object,
            (userSvc ?? new Mock<IUserAccountService>()).Object,
            (passSvc ?? new Mock<IPasswordService>()).Object);

        var claims = authenticatedUserId.HasValue
            ? new List<Claim> { new(ClaimTypes.NameIdentifier, authenticatedUserId.Value.ToString()) }
            : new List<Claim>();

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = claims.Count > 0
                    ? new ClaimsPrincipal(new ClaimsIdentity(claims, "Test"))
                    : new ClaimsPrincipal()
            }
        };
        return controller;
    }

    // ── Register ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task Register_ValidInput_Returns201WithUser()
    {
        var user = MakeUser();
        var authSvc = new Mock<IAuthService>();
        authSvc.Setup(s => s.RegisterAsync(It.IsAny<RegisterInput>(), It.IsAny<CancellationToken>())).ReturnsAsync(user);
        var controller = BuildController(authSvc: authSvc);
        var input = new RegisterInput { Email = user.Email, Password = "Password1!", FullName = user.FullName, PhoneNumber = "+1234567890" };

        var result = await controller.Register(input, CancellationToken.None);

        var created = Assert.IsType<CreatedAtActionResult>(result.Result);
        Assert.Equal(201, created.StatusCode);
        Assert.IsType<UserType>(created.Value);
    }

    [Fact]
    public async Task Register_CallsAuthService_WithInput()
    {
        var authSvc = new Mock<IAuthService>();
        authSvc.Setup(s => s.RegisterAsync(It.IsAny<RegisterInput>(), It.IsAny<CancellationToken>())).ReturnsAsync(MakeUser());
        var controller = BuildController(authSvc: authSvc);
        var input = new RegisterInput { Email = "user@example.com", Password = "Password1!", FullName = "John Doe", PhoneNumber = "+1234567890" };

        await controller.Register(input, CancellationToken.None);

        authSvc.Verify(s => s.RegisterAsync(input, It.IsAny<CancellationToken>()), Times.Once);
    }

    // ── Login ─────────────────────────────────────────────────────────────────

    [Fact]
    public async Task Login_ValidCredentials_Returns200WithTokenAndUser()
    {
        var authSvc = new Mock<IAuthService>();
        authSvc.Setup(s => s.LoginAsync(It.IsAny<LoginInput>(), It.IsAny<CancellationToken>()))
               .ReturnsAsync(new LoginResponse { Token = "jwt-token", User = MakeUser() });
        var controller = BuildController(authSvc: authSvc);

        var result = await controller.Login(new LoginInput { Email = "user@example.com", Password = "Password1!" }, CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<LoginResponse>(ok.Value);
        Assert.Equal("jwt-token", response.Token);
    }

    [Fact]
    public async Task Login_CallsAuthService_WithInput()
    {
        var authSvc = new Mock<IAuthService>();
        authSvc.Setup(s => s.LoginAsync(It.IsAny<LoginInput>(), It.IsAny<CancellationToken>()))
               .ReturnsAsync(new LoginResponse { Token = "t", User = MakeUser() });
        var controller = BuildController(authSvc: authSvc);
        var input = new LoginInput { Email = "user@example.com", Password = "Password1!" };

        await controller.Login(input, CancellationToken.None);

        authSvc.Verify(s => s.LoginAsync(input, It.IsAny<CancellationToken>()), Times.Once);
    }

    // ── UpdateProfile ─────────────────────────────────────────────────────────

    [Fact]
    public async Task UpdateProfile_ValidToken_Returns200WithUpdatedUser()
    {
        var updated = MakeUser(); updated.FullName = "Jane Doe";
        var userSvc = new Mock<IUserAccountService>();
        userSvc.Setup(s => s.UpdateProfileAsync(1, It.IsAny<UpdateProfileInput>(), It.IsAny<CancellationToken>())).ReturnsAsync(updated);
        var controller = BuildController(userSvc: userSvc, authenticatedUserId: 1);

        var result = await controller.UpdateProfile(new UpdateProfileInput { FullName = "Jane Doe" }, CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var user = Assert.IsType<UserType>(ok.Value);
        Assert.Equal("Jane Doe", user.FullName);
    }

    [Fact]
    public async Task UpdateProfile_MissingClaim_Returns401()
    {
        var controller = BuildController(authenticatedUserId: null);

        var result = await controller.UpdateProfile(new UpdateProfileInput { FullName = "X" }, CancellationToken.None);

        Assert.IsType<UnauthorizedResult>(result.Result);
    }

    [Fact]
    public async Task UpdateProfile_CallsUserService_WithParsedUserId()
    {
        var userSvc = new Mock<IUserAccountService>();
        userSvc.Setup(s => s.UpdateProfileAsync(3, It.IsAny<UpdateProfileInput>(), It.IsAny<CancellationToken>())).ReturnsAsync(MakeUser(3));
        var controller = new AuthController(
            new Mock<IAuthService>().Object,
            userSvc.Object,
            new Mock<IPasswordService>().Object)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(
                        new[] { new Claim(JwtRegisteredClaimNames.Sub, "3") }, "Test"))
                }
            }
        };

        await controller.UpdateProfile(new UpdateProfileInput { FullName = "New" }, CancellationToken.None);

        userSvc.Verify(s => s.UpdateProfileAsync(3, It.IsAny<UpdateProfileInput>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    // ── ForgotPassword ────────────────────────────────────────────────────────

    [Fact]
    public async Task ForgotPassword_ValidEmail_Returns200WithMessage()
    {
        var passSvc = new Mock<IPasswordService>();
        passSvc.Setup(s => s.ForgotPasswordAsync(It.IsAny<ForgotPasswordInput>(), It.IsAny<CancellationToken>()))
               .ReturnsAsync(new ForgotPasswordResponse { Message = "If the email is registered, a reset token has been issued.", ResetToken = "token123" });
        var controller = BuildController(passSvc: passSvc);

        var result = await controller.ForgotPassword(new ForgotPasswordInput { Email = "user@example.com" }, CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<ForgotPasswordResponse>(ok.Value);
        Assert.NotNull(response.Message);
    }

    [Fact]
    public async Task ForgotPassword_UnknownEmail_StillReturns200()
    {
        var passSvc = new Mock<IPasswordService>();
        passSvc.Setup(s => s.ForgotPasswordAsync(It.IsAny<ForgotPasswordInput>(), It.IsAny<CancellationToken>()))
               .ReturnsAsync(new ForgotPasswordResponse { Message = "If the email is registered, a reset token has been issued." });
        var controller = BuildController(passSvc: passSvc);

        var result = await controller.ForgotPassword(new ForgotPasswordInput { Email = "nobody@example.com" }, CancellationToken.None);

        Assert.IsType<OkObjectResult>(result.Result);
    }

    // ── ResetPassword ─────────────────────────────────────────────────────────

    [Fact]
    public async Task ResetPassword_ValidToken_Returns200WithSuccess()
    {
        var passSvc = new Mock<IPasswordService>();
        passSvc.Setup(s => s.ResetPasswordAsync(It.IsAny<ResetPasswordInput>(), It.IsAny<CancellationToken>()))
               .ReturnsAsync(new OperationResult { Success = true, Message = "Password has been reset" });
        var controller = BuildController(passSvc: passSvc);

        var result = await controller.ResetPassword(new ResetPasswordInput { Token = "valid-token", NewPassword = "NewPass1!" }, CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<OperationResult>(ok.Value);
        Assert.True(response.Success);
    }

    [Fact]
    public async Task ResetPassword_CallsPasswordService_WithInput()
    {
        var passSvc = new Mock<IPasswordService>();
        passSvc.Setup(s => s.ResetPasswordAsync(It.IsAny<ResetPasswordInput>(), It.IsAny<CancellationToken>()))
               .ReturnsAsync(new OperationResult { Success = true });
        var controller = BuildController(passSvc: passSvc);
        var input = new ResetPasswordInput { Token = "tok", NewPassword = "NewPass1!" };

        await controller.ResetPassword(input, CancellationToken.None);

        passSvc.Verify(s => s.ResetPasswordAsync(input, It.IsAny<CancellationToken>()), Times.Once);
    }

    // ── ToggleUserStatus ──────────────────────────────────────────────────────

    [Fact]
    public async Task ToggleUserStatus_ValidId_Returns200WithOperationResult()
    {
        var userSvc = new Mock<IUserAccountService>();
        userSvc.Setup(s => s.ToggleUserStatusAsync(5, It.IsAny<CancellationToken>()))
               .ReturnsAsync(new OperationResult { Success = true, Message = "User account deactivated" });
        var controller = BuildController(userSvc: userSvc);

        var result = await controller.ToggleUserStatus(5, CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<OperationResult>(ok.Value);
        Assert.True(response.Success);
    }

    [Fact]
    public async Task ToggleUserStatus_CallsUserService_WithId()
    {
        var userSvc = new Mock<IUserAccountService>();
        userSvc.Setup(s => s.ToggleUserStatusAsync(7, It.IsAny<CancellationToken>()))
               .ReturnsAsync(new OperationResult { Success = true });
        var controller = BuildController(userSvc: userSvc);

        await controller.ToggleUserStatus(7, CancellationToken.None);

        userSvc.Verify(s => s.ToggleUserStatusAsync(7, CancellationToken.None), Times.Once);
    }
}
