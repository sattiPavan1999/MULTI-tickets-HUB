using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using AdminBFF.Core.DTOs;
using AdminBFF.Core.Services;
using AdminBFF.Endpoints.Controllers;

namespace AdminBFF.Tests.Controllers;

public class AdminUserControllerTests
{
    private static AdminUserController BuildController(Mock<IIdentityService> svc, string? bearerToken = "test-token")
    {
        var accessor = new Mock<IHttpContextAccessor>();
        var httpContext = new DefaultHttpContext();
        if (bearerToken is not null)
            httpContext.Request.Headers.Authorization = $"Bearer {bearerToken}";
        accessor.Setup(a => a.HttpContext).Returns(httpContext);

        return new AdminUserController(svc.Object, accessor.Object);
    }

    [Fact]
    public async Task ToggleUserStatus_Returns200WithOperationResult()
    {
        var svc = new Mock<IIdentityService>();
        svc.Setup(s => s.ToggleUserStatusAsync(3, It.IsAny<string>()))
           .ReturnsAsync(new OperationResult { Success = true, Message = "User account deactivated" });
        var controller = BuildController(svc);

        var result = await controller.ToggleUserStatus(3);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<OperationResult>(ok.Value);
        response.Success.Should().BeTrue();
    }

    [Fact]
    public async Task ToggleUserStatus_ForwardsToken_ToIdentityService()
    {
        var svc = new Mock<IIdentityService>();
        svc.Setup(s => s.ToggleUserStatusAsync(It.IsAny<int>(), It.IsAny<string>()))
           .ReturnsAsync(new OperationResult { Success = true });
        var controller = BuildController(svc, "my-jwt-token");

        await controller.ToggleUserStatus(5);

        svc.Verify(s => s.ToggleUserStatusAsync(5, "my-jwt-token"), Times.Once);
    }
}
