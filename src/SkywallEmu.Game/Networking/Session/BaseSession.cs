// This file is part of the SkywallEmu project and is published under the
// GPL 3.0. See LICENSE for further information.

using System.Buffers;
using System.IO.Pipelines;
using System.Net.Sockets;
using System.Threading.Channels;
using SkywallEmu.Game.Networking.Packets;

namespace SkywallEmu.Game.Networking.Session;

public abstract class BaseSession
{
    private readonly CancellationTokenSource _cancellationTokenSource = new();
    private readonly Channel<ServerPacket> _packetQueue = Channel.CreateUnbounded<ServerPacket>();
    private readonly TcpClient _tcpClient;

    public BaseSession(TcpClient tcpClient)
    {
        _tcpClient = tcpClient;
        _ = ReadFromStreamAsync();
        _ = WriteToStreamAsync();
    }

    public void Close()
    {
        if (_cancellationTokenSource.IsCancellationRequested)
            return;

        _cancellationTokenSource.Cancel();
        _cancellationTokenSource.Dispose();
    }

    private async Task ReadFromStreamAsync()
    {
        try
        {
            PipeReader pipeReader = PipeReader.Create(_tcpClient.GetStream());

            while (!_cancellationTokenSource.IsCancellationRequested)
            {
                ReadResult result = await pipeReader.ReadAsync(_cancellationTokenSource.Token);
                if (result.IsCanceled)
                {
                    Close();
                    return;
                }

                ReadOnlySequence<byte> buffer = result.Buffer;
                SequenceReader<byte> reader = new(buffer);

                DataReceived(ref reader);
                pipeReader.AdvanceTo(reader.Position, buffer.End);

                if (result.IsCompleted)
                {
                    Close();
                    return;
                }
            }
        }
        catch (OperationCanceledException)
        {
        }
    }

    private async Task WriteToStreamAsync()
    {
        try
        {
            PipeWriter pipeWriter = PipeWriter.Create(_tcpClient.GetStream());

            while (!_cancellationTokenSource.IsCancellationRequested)
            {
                await _packetQueue.Reader.WaitToReadAsync(_cancellationTokenSource.Token);
                while (_packetQueue.Reader.TryRead(out ServerPacket? packet))
                {
                    try
                    {
                        packet.Serialize();
                        ReadOnlyMemory<byte> payload = packet.Payload;
                        ReadOnlyMemory<byte> header = BuildPacketHeader(ref payload);

                        await pipeWriter.WriteAsync(header, _cancellationTokenSource.Token);
                        await pipeWriter.WriteAsync(payload, _cancellationTokenSource.Token);
                    }
                    finally
                    {
                        packet.Dispose();
                    }
                }

                await pipeWriter.FlushAsync(_cancellationTokenSource.Token);
            }
        }
        catch (OperationCanceledException)
        {
        }
    }


    protected abstract void DataReceived(ref SequenceReader<byte> sequenceReader);

    protected abstract ReadOnlyMemory<byte> BuildPacketHeader(ref ReadOnlyMemory<byte> payload);
}
