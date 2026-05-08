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

    private static AuthController BuildController(Mock<IAuthService> svc, int? authenticatedUserId = null)
    {
        var controller = new AuthController(svc.Object);
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
        var svc = new Mock<IAuthService>();
        svc.Setup(s => s.RegisterAsync(It.IsAny<RegisterInput>(), It.IsAny<CancellationToken>())).ReturnsAsync(user);
        var controller = BuildController(svc);
        var input = new RegisterInput { Email = user.Email, Password = "Password1!", FullName = user.FullName, PhoneNumber = "+1234567890" };

        var result = await controller.Register(input, CancellationToken.None);

        var created = Assert.IsType<CreatedAtActionResult>(result.Result);
        Assert.Equal(201, created.StatusCode);
        Assert.IsType<UserType>(created.Value);
    }

    [Fact]
    public async Task Register_CallsAuthService_WithInput()
    {
        var svc = new Mock<IAuthService>();
        svc.Setup(s => s.RegisterAsync(It.IsAny<RegisterInput>(), It.IsAny<CancellationToken>())).ReturnsAsync(MakeUser());
        var controller = BuildController(svc);
        var input = new RegisterInput { Email = "user@example.com", Password = "Password1!", FullName = "John Doe", PhoneNumber = "+1234567890" };

        await controller.Register(input, CancellationToken.None);

        svc.Verify(s => s.RegisterAsync(input, It.IsAny<CancellationToken>()), Times.Once);
    }

    // ── Login ─────────────────────────────────────────────────────────────────

    [Fact]
    public async Task Login_ValidCredentials_Returns200WithTokenAndUser()
    {
        var svc = new Mock<IAuthService>();
        svc.Setup(s => s.LoginAsync(It.IsAny<LoginInput>(), It.IsAny<CancellationToken>()))
           .ReturnsAsync(new LoginResponse { Token = "jwt-token", User = MakeUser() });
        var controller = BuildController(svc);

        var result = await controller.Login(new LoginInput { Email = "user@example.com", Password = "Password1!" }, CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<LoginResponse>(ok.Value);
        Assert.Equal("jwt-token", response.Token);
    }

    [Fact]
    public async Task Login_CallsAuthService_WithInput()
    {
        var svc = new Mock<IAuthService>();
        svc.Setup(s => s.LoginAsync(It.IsAny<LoginInput>(), It.IsAny<CancellationToken>()))
           .ReturnsAsync(new LoginResponse { Token = "t", User = MakeUser() });
        var controller = BuildController(svc);
        var input = new LoginInput { Email = "user@example.com", Password = "Password1!" };

        await controller.Login(input, CancellationToken.None);

        svc.Verify(s => s.LoginAsync(input, It.IsAny<CancellationToken>()), Times.Once);
    }

    // ── UpdateProfile ─────────────────────────────────────────────────────────

    [Fact]
    public async Task UpdateProfile_ValidToken_Returns200WithUpdatedUser()
    {
        var updated = MakeUser(); updated.FullName = "Jane Doe";
        var svc = new Mock<IAuthService>();
        svc.Setup(s => s.UpdateProfileAsync(1, It.IsAny<UpdateProfileInput>(), It.IsAny<CancellationToken>())).ReturnsAsync(updated);
        var controller = BuildController(svc, authenticatedUserId: 1);

        var result = await controller.UpdateProfile(new UpdateProfileInput { FullName = "Jane Doe" }, CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var user = Assert.IsType<UserType>(ok.Value);
        Assert.Equal("Jane Doe", user.FullName);
    }

    [Fact]
    public async Task UpdateProfile_MissingClaim_Returns401()
    {
        var svc = new Mock<IAuthService>();
        var controller = BuildController(svc, authenticatedUserId: null);

        var result = await controller.UpdateProfile(new UpdateProfileInput { FullName = "X" }, CancellationToken.None);

        Assert.IsType<UnauthorizedResult>(result.Result);
        svc.Verify(s => s.UpdateProfileAsync(It.IsAny<int>(), It.IsAny<UpdateProfileInput>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UpdateProfile_CallsAuthService_WithParsedUserId()
    {
        var svc = new Mock<IAuthService>();
        svc.Setup(s => s.UpdateProfileAsync(3, It.IsAny<UpdateProfileInput>(), It.IsAny<CancellationToken>())).ReturnsAsync(MakeUser(3));
        var controller = new AuthController(svc.Object)
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

        svc.Verify(s => s.UpdateProfileAsync(3, It.IsAny<UpdateProfileInput>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    // ── ForgotPassword ────────────────────────────────────────────────────────

    [Fact]
    public async Task ForgotPassword_ValidEmail_Returns200WithMessage()
    {
        var svc = new Mock<IAuthService>();
        svc.Setup(s => s.ForgotPasswordAsync(It.IsAny<ForgotPasswordInput>(), It.IsAny<CancellationToken>()))
           .ReturnsAsync(new ForgotPasswordResponse { Message = "If the email is registered, a reset token has been issued.", ResetToken = "token123" });
        var controller = BuildController(svc);

        var result = await controller.ForgotPassword(new ForgotPasswordInput { Email = "user@example.com" }, CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<ForgotPasswordResponse>(ok.Value);
        Assert.NotNull(response.Message);
    }

    [Fact]
    public async Task ForgotPassword_UnknownEmail_StillReturns200()
    {
        var svc = new Mock<IAuthService>();
        svc.Setup(s => s.ForgotPasswordAsync(It.IsAny<ForgotPasswordInput>(), It.IsAny<CancellationToken>()))
           .ReturnsAsync(new ForgotPasswordResponse { Message = "If the email is registered, a reset token has been issued." });
        var controller = BuildController(svc);

        var result = await controller.ForgotPassword(new ForgotPasswordInput { Email = "nobody@example.com" }, CancellationToken.None);

        Assert.IsType<OkObjectResult>(result.Result);
    }

    // ── ResetPassword ─────────────────────────────────────────────────────────

    [Fact]
    public async Task ResetPassword_ValidToken_Returns200WithSuccess()
    {
        var svc = new Mock<IAuthService>();
        svc.Setup(s => s.ResetPasswordAsync(It.IsAny<ResetPasswordInput>(), It.IsAny<CancellationToken>()))
           .ReturnsAsync(new OperationResult { Success = true, Message = "Password has been reset" });
        var controller = BuildController(svc);

        var result = await controller.ResetPassword(new ResetPasswordInput { Token = "valid-token", NewPassword = "NewPass1!" }, CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<OperationResult>(ok.Value);
        Assert.True(response.Success);
    }

    [Fact]
    public async Task ResetPassword_CallsAuthService_WithInput()
    {
        var svc = new Mock<IAuthService>();
        svc.Setup(s => s.ResetPasswordAsync(It.IsAny<ResetPasswordInput>(), It.IsAny<CancellationToken>()))
           .ReturnsAsync(new OperationResult { Success = true });
        var controller = BuildController(svc);
        var input = new ResetPasswordInput { Token = "tok", NewPassword = "NewPass1!" };

        await controller.ResetPassword(input, CancellationToken.None);

        svc.Verify(s => s.ResetPasswordAsync(input, It.IsAny<CancellationToken>()), Times.Once);
    }

    // ── ToggleUserStatus ──────────────────────────────────────────────────────

    [Fact]
    public async Task ToggleUserStatus_ValidId_Returns200WithOperationResult()
    {
        var svc = new Mock<IAuthService>();
        svc.Setup(s => s.ToggleUserStatusAsync(5, It.IsAny<CancellationToken>()))
           .ReturnsAsync(new OperationResult { Success = true, Message = "User account deactivated" });
        var controller = BuildController(svc);

        var result = await controller.ToggleUserStatus(5, CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<OperationResult>(ok.Value);
        Assert.True(response.Success);
    }

    [Fact]
    public async Task ToggleUserStatus_CallsAuthService_WithId()
    {
        var svc = new Mock<IAuthService>();
        svc.Setup(s => s.ToggleUserStatusAsync(7, It.IsAny<CancellationToken>()))
           .ReturnsAsync(new OperationResult { Success = true });
        var controller = BuildController(svc);

        await controller.ToggleUserStatus(7, CancellationToken.None);

        svc.Verify(s => s.ToggleUserStatusAsync(7, CancellationToken.None), Times.Once);
    }
}
