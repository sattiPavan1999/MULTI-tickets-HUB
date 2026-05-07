using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using IdentityService.Core.DTOs;
using IdentityService.Core.Services;
using IdentityService.Endpoints.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace IdentityService.Tests.Controllers;

public class AuthControllerTests
{
    private static UserType MakeUser(int id = 1) => new()
    {
        Id = id,
        Email = "user@example.com",
        FullName = "John Doe",
        PhoneNumber = "+1234567890",
        Role = "User",
        CreatedAt = DateTime.UtcNow
    };

    private static AuthController BuildController(
        Mock<IAuthService> svc,
        int? authenticatedUserId = null)
    {
        var controller = new AuthController(svc.Object);

        if (authenticatedUserId.HasValue)
        {
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, authenticatedUserId.Value.ToString())
            };
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(claims, "Test"))
                }
            };
        }
        else
        {
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };
        }

        return controller;
    }

    // ── Register ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task Register_ValidInput_Returns201WithUser()
    {
        var svc = new Mock<IAuthService>();
        svc.Setup(s => s.RegisterAsync(It.IsAny<RegisterInput>())).ReturnsAsync(MakeUser());
        var controller = BuildController(svc);

        var result = await controller.Register(new RegisterInput
        {
            Email = "user@example.com",
            Password = "Password1!",
            FullName = "John Doe",
            PhoneNumber = "+1234567890"
        });

        var created = Assert.IsType<CreatedAtActionResult>(result.Result);
        Assert.Equal(201, created.StatusCode);
        var user = Assert.IsType<UserType>(created.Value);
        Assert.Equal("user@example.com", user.Email);
    }

    [Fact]
    public async Task Register_CallsAuthService_WithInput()
    {
        var svc = new Mock<IAuthService>();
        svc.Setup(s => s.RegisterAsync(It.IsAny<RegisterInput>())).ReturnsAsync(MakeUser());
        var controller = BuildController(svc);
        var input = new RegisterInput
        {
            Email = "user@example.com",
            Password = "Password1!",
            FullName = "John Doe",
            PhoneNumber = "+1234567890"
        };

        await controller.Register(input);

        svc.Verify(s => s.RegisterAsync(input), Times.Once);
    }

    // ── Login ─────────────────────────────────────────────────────────────────

    [Fact]
    public async Task Login_ValidCredentials_Returns200WithTokenAndUser()
    {
        var svc = new Mock<IAuthService>();
        svc.Setup(s => s.LoginAsync(It.IsAny<LoginInput>()))
           .ReturnsAsync(new LoginResponse { Token = "jwt-token", User = MakeUser() });
        var controller = BuildController(svc);

        var result = await controller.Login(new LoginInput
        {
            Email = "user@example.com",
            Password = "Password1!"
        });

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<LoginResponse>(ok.Value);
        Assert.Equal("jwt-token", response.Token);
        Assert.NotNull(response.User);
    }

    [Fact]
    public async Task Login_CallsAuthService_WithInput()
    {
        var svc = new Mock<IAuthService>();
        svc.Setup(s => s.LoginAsync(It.IsAny<LoginInput>()))
           .ReturnsAsync(new LoginResponse { Token = "t", User = MakeUser() });
        var controller = BuildController(svc);
        var input = new LoginInput { Email = "user@example.com", Password = "Password1!" };

        await controller.Login(input);

        svc.Verify(s => s.LoginAsync(input), Times.Once);
    }

    // ── UpdateProfile ─────────────────────────────────────────────────────────

    [Fact]
    public async Task UpdateProfile_ValidToken_Returns200WithUpdatedUser()
    {
        var updated = MakeUser();
        updated.FullName = "Jane Doe";
        var svc = new Mock<IAuthService>();
        svc.Setup(s => s.UpdateProfileAsync(1, It.IsAny<UpdateProfileInput>())).ReturnsAsync(updated);
        var controller = BuildController(svc, authenticatedUserId: 1);

        var result = await controller.UpdateProfile(new UpdateProfileInput { FullName = "Jane Doe" });

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var user = Assert.IsType<UserType>(ok.Value);
        Assert.Equal("Jane Doe", user.FullName);
    }

    [Fact]
    public async Task UpdateProfile_MissingClaim_Returns401()
    {
        var svc = new Mock<IAuthService>();
        var controller = BuildController(svc, authenticatedUserId: null);

        var result = await controller.UpdateProfile(new UpdateProfileInput { FullName = "X" });

        Assert.IsType<UnauthorizedResult>(result.Result);
        svc.Verify(s => s.UpdateProfileAsync(It.IsAny<int>(), It.IsAny<UpdateProfileInput>()), Times.Never);
    }

    [Fact]
    public async Task UpdateProfile_CallsAuthService_WithParsedUserId()
    {
        var svc = new Mock<IAuthService>();
        svc.Setup(s => s.UpdateProfileAsync(3, It.IsAny<UpdateProfileInput>())).ReturnsAsync(MakeUser(3));
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, "3")
        };
        var controller = new AuthController(svc.Object)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(claims, "Test"))
                }
            }
        };

        await controller.UpdateProfile(new UpdateProfileInput { FullName = "New" });

        svc.Verify(s => s.UpdateProfileAsync(3, It.IsAny<UpdateProfileInput>()), Times.Once);
    }

    // ── ForgotPassword ────────────────────────────────────────────────────────

    [Fact]
    public async Task ForgotPassword_ValidEmail_Returns200WithMessage()
    {
        var svc = new Mock<IAuthService>();
        svc.Setup(s => s.ForgotPasswordAsync(It.IsAny<ForgotPasswordInput>()))
           .ReturnsAsync(new ForgotPasswordResponse
           {
               Message = "If the email is registered, a reset token has been issued.",
               ResetToken = "token123"
           });
        var controller = BuildController(svc);

        var result = await controller.ForgotPassword(new ForgotPasswordInput { Email = "user@example.com" });

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<ForgotPasswordResponse>(ok.Value);
        Assert.NotNull(response.Message);
    }

    [Fact]
    public async Task ForgotPassword_UnknownEmail_StillReturns200()
    {
        var svc = new Mock<IAuthService>();
        svc.Setup(s => s.ForgotPasswordAsync(It.IsAny<ForgotPasswordInput>()))
           .ReturnsAsync(new ForgotPasswordResponse
           {
               Message = "If the email is registered, a reset token has been issued."
           });
        var controller = BuildController(svc);

        var result = await controller.ForgotPassword(new ForgotPasswordInput { Email = "nobody@example.com" });

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.IsType<ForgotPasswordResponse>(ok.Value);
    }

    // ── ResetPassword ─────────────────────────────────────────────────────────

    [Fact]
    public async Task ResetPassword_ValidToken_Returns200WithSuccess()
    {
        var svc = new Mock<IAuthService>();
        svc.Setup(s => s.ResetPasswordAsync(It.IsAny<ResetPasswordInput>()))
           .ReturnsAsync(new OperationResult { Success = true, Message = "Password has been reset" });
        var controller = BuildController(svc);

        var result = await controller.ResetPassword(new ResetPasswordInput
        {
            Token = "valid-token",
            NewPassword = "NewPass1!"
        });

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<OperationResult>(ok.Value);
        Assert.True(response.Success);
    }

    [Fact]
    public async Task ResetPassword_CallsAuthService_WithInput()
    {
        var svc = new Mock<IAuthService>();
        svc.Setup(s => s.ResetPasswordAsync(It.IsAny<ResetPasswordInput>()))
           .ReturnsAsync(new OperationResult { Success = true });
        var controller = BuildController(svc);
        var input = new ResetPasswordInput { Token = "tok", NewPassword = "NewPass1!" };

        await controller.ResetPassword(input);

        svc.Verify(s => s.ResetPasswordAsync(input), Times.Once);
    }
}
