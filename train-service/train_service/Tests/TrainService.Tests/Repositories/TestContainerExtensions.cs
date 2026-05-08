using DotNet.Testcontainers.Containers;
using System.Net.Sockets;
using Testcontainers.PostgreSql;

namespace TrainService.Tests.Repositories;

public static class TestContainerExtensions
{
    public static Task<bool> WaitForPort(
        this PostgreSqlContainer container,
        TimeSpan? maxWait = null)
        => WaitForPortAsync(container, PostgreSqlBuilder.PostgreSqlPort,
            maxWait ?? TimeSpan.FromSeconds(30));

    private static async Task<bool> WaitForPortAsync(
        IContainer container, int port, TimeSpan maxWait)
    {
        var host = container.Hostname;
        var mapped = container.GetMappedPublicPort(port);
        var deadline = DateTime.UtcNow.Add(maxWait);

        while (DateTime.UtcNow < deadline)
        {
            try
            {
                using var tcp = new TcpClient();
                await tcp.ConnectAsync(host, mapped);
                return true;
            }
            catch (SocketException) { }

            await Task.Delay(100);
        }

        return false;
    }
}
