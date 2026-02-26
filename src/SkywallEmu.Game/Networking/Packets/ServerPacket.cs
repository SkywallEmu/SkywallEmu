// This file is part of the SkywallEmu project and is published under the
// GPL 3.0. See LICENSE for further information.

using System.Buffers;
using System.Buffers.Binary;

namespace SkywallEmu.Game.Networking.Packets;

public abstract class ServerPacket : IDisposable
{
    private readonly ArrayPool<byte> _bufferPool = ArrayPool<byte>.Shared;
    private const int BufferResizeCapacity = 1024;

    private byte[] _buffer;
    private int _currentBytePosition;
    private int _currentBitPosition;

    public ReadOnlyMemory<byte> Payload => _buffer.AsMemory(0, _currentBytePosition);

    public ServerPacket(int opcode, int initialBufferSize)
    {
        _buffer = _bufferPool.Rent(initialBufferSize);
    }

    ~ServerPacket()
    {
        Dispose();
    }

    public abstract void Serialize();

    /// <summary>
    /// Retrieves a memory segment of the given size and advances the position within the buffer
    /// </summary>
    /// <param name="size">The requested buffer size</param>
    /// <returns>A span of the buffer that data can be written to</returns>
    private Span<byte> GetSpan(int size)
    {
        // The buffer is too small to fit in the requested size. Increase the buffer size and copy the data
        if (_buffer.Length + _currentBytePosition < size)
        {
            byte[] newBuffer = _bufferPool.Rent(_buffer.Length + Math.Max(size, BufferResizeCapacity));
            Buffer.BlockCopy(_buffer, 0, newBuffer, 0, _currentBytePosition);
            _bufferPool.Return(_buffer);
            _buffer = newBuffer;
        }

        Span<byte> buffer = _buffer.AsSpan(_currentBytePosition, size);
        _currentBytePosition += size;

        return buffer;
    }

    protected void WriteInt8(sbyte value)
    {
        GetSpan(sizeof(sbyte))[0] = (byte)value;
    }

    protected void WriteUInt8(byte value)
    {
        GetSpan(sizeof(byte))[0] = value;
    }

    protected void WriteInt16(short value)
    {
        BinaryPrimitives.WriteInt16LittleEndian(GetSpan(sizeof(short)), value);
    }

    protected void WriteUInt16(ushort value)
    {
        BinaryPrimitives.WriteUInt16LittleEndian(GetSpan(sizeof(ushort)), value);
    }

    protected void WriteInt32(int value)
    {
        BinaryPrimitives.WriteInt32LittleEndian(GetSpan(sizeof(int)), value);
    }

    protected void WriteUInt32(uint value)
    {
        BinaryPrimitives.WriteUInt32LittleEndian(GetSpan(sizeof(uint)), value);
    }

    protected void WriteInt64(long value)
    {
        BinaryPrimitives.WriteInt64LittleEndian(GetSpan(sizeof(long)), value);
    }

    protected void WriteUInt64(ulong value)
    {
        BinaryPrimitives.WriteUInt64LittleEndian(GetSpan(sizeof(ulong)), value);
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        _bufferPool.Return(_buffer);
    }
}
