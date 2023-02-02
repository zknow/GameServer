using System.Text;

namespace GameServer.Utils;

public class PackParser
{
    public byte[] PackBuffer { get; set; }
    public int PackIndex { get; set; }

    public PackParser(byte[] rcvBuffer)
    {
        PackBuffer = rcvBuffer;
        PackIndex = 0;
    }

    public PackParser(byte[] rcvBuffer, long offset, long size)
    {
        PackBuffer = new byte[size];
        Array.Copy(rcvBuffer, offset, PackBuffer, 0, size);
        PackIndex = 0;
    }

    public bool GetBoolen()
    {
        bool result = BitConverter.ToBoolean(PackBuffer, PackIndex);
        PackIndex += 1;
        return result;
    }

    public byte GetByte()
    {
        byte result = PackBuffer[PackIndex];
        PackIndex += 1;
        return result;
    }

    public string GetString()
    {
        int len = BitConverter.ToInt32(PackBuffer, PackIndex);
        PackIndex += 4;
        string result = Encoding.UTF8.GetString(PackBuffer, PackIndex, len);
        PackIndex += len;
        return result;
    }

    public int GetInt()
    {
        int result = BitConverter.ToInt32(PackBuffer, PackIndex);
        PackIndex += 4;
        return result;
    }

    public long GetLong()
    {
        long result = BitConverter.ToInt64(PackBuffer, PackIndex);
        PackIndex += 8;
        return result;
    }
}