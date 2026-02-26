// This file is part of the SkywallEmu project and is published under the
// GPL 3.0. See LICENSE for further information.

using System.Net;
using System.Net.Sockets;
using SkywallEmu.Utils.Configuration;
using Microsoft.Extensions.Hosting;

namespace SkywallEmu.Game.Services.Hosted;

/// <summary>
/// A hosted service which handles the Grunt protocol login process
/// </summary>
public class GruntLoginService : IHostedService
{
    private CancellationTokenSource _cancellationTokenSource = new();

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        _ = ListenForConnectionsAsync();
        _ = ProcessLoginRequestsAsync();

        return Task.CompletedTask;
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        _cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        await _cancellationTokenSource.CancelAsync();
        _cancellationTokenSource.Dispose();
    }

    private async Task ListenForConnectionsAsync()
    {
        if (!AppSettings.TryGetConfigValue("GruntIpEndPoint", out string? ipEndPoint))
            throw new OperationCanceledException("No configuration for key 'GruntIpEndPoint' has been found. " +
                                                 "GruntLoginService cannot be started");

        if (!IPEndPoint.TryParse(ipEndPoint, out IPEndPoint? endpoint))
            throw new OperationCanceledException("Configuration value for key 'GruntIpEndPoint' is not a valid IPEndPoint address.");

        TcpListener listener = new(endpoint);
        listener.Start();

        try
        {
            while (!_cancellationTokenSource.IsCancellationRequested)
            {
                await listener.AcceptTcpClientAsync(_cancellationTokenSource.Token);
            }

            listener.Stop();
        }
        catch (OperationCanceledException)
        {
            listener.Stop();
        }
    }

    private async Task ProcessLoginRequestsAsync()
    {
        while (!_cancellationTokenSource.IsCancellationRequested)
        {
            try
            {
                await Task.Delay(1000, _cancellationTokenSource.Token);

            }
            catch (OperationCanceledException)
            {
            }
        }
    }
}
