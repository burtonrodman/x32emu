using System.Text;
using System.Buffers.Binary;

namespace X32Emulator.Osc;

public static class OscSerializer
{
    public static byte[] Serialize(OscMessage msg)
    {
        var buf = new List<byte>();
        WriteString(buf, msg.Address);

        var tags = new StringBuilder(",");
        foreach (var arg in msg.Arguments)
            tags.Append(arg.Type switch
            {
                OscArgType.Int => 'i',
                OscArgType.Float => 'f',
                OscArgType.String => 's',
                OscArgType.Blob => 'b',
                OscArgType.True => 'T',
                OscArgType.False => 'F',
                OscArgType.Nil => 'N',
                OscArgType.Inf => 'I',
                _ => 'i'
            });
        WriteString(buf, tags.ToString());

        foreach (var arg in msg.Arguments)
        {
            switch (arg.Type)
            {
                case OscArgType.Int:
                    var ib = new byte[4];
                    BinaryPrimitives.WriteInt32BigEndian(ib, arg.AsInt());
                    buf.AddRange(ib);
                    break;
                case OscArgType.Float:
                    var fb = new byte[4];
                    BinaryPrimitives.WriteSingleBigEndian(fb, arg.AsFloat());
                    buf.AddRange(fb);
                    break;
                case OscArgType.String:
                    WriteString(buf, arg.AsString());
                    break;
                case OscArgType.Blob:
                    var blob = arg.AsBlob();
                    var lenBytes = new byte[4];
                    BinaryPrimitives.WriteInt32BigEndian(lenBytes, blob.Length);
                    buf.AddRange(lenBytes);
                    buf.AddRange(blob);
                    int pad = (4 - (blob.Length % 4)) % 4;
                    for (int i = 0; i < pad; i++) buf.Add(0);
                    break;
            }
        }
        return buf.ToArray();
    }

    private static void WriteString(List<byte> buf, string s)
    {
        var bytes = Encoding.ASCII.GetBytes(s);
        buf.AddRange(bytes);
        buf.Add(0);
        int pad = (4 - ((bytes.Length + 1) % 4)) % 4;
        for (int i = 0; i < pad; i++) buf.Add(0);
    }

    public static OscMessage? Deserialize(byte[] data)
    {
        try
        {
            int pos = 0;
            var address = ReadString(data, ref pos);
            if (string.IsNullOrEmpty(address)) return null;

            var msg = new OscMessage { Address = address };

            if (pos >= data.Length) return msg;

            var typeTags = ReadString(data, ref pos);
            if (string.IsNullOrEmpty(typeTags) || !typeTags.StartsWith(',')) return msg;

            foreach (char tag in typeTags.Skip(1))
            {
                switch (tag)
                {
                    case 'i':
                        if (pos + 4 > data.Length) return msg;
                        msg.Arguments.Add(new OscArg(BinaryPrimitives.ReadInt32BigEndian(data.AsSpan(pos, 4))));
                        pos += 4;
                        break;
                    case 'f':
                        if (pos + 4 > data.Length) return msg;
                        msg.Arguments.Add(new OscArg(BinaryPrimitives.ReadSingleBigEndian(data.AsSpan(pos, 4))));
                        pos += 4;
                        break;
                    case 's':
                        msg.Arguments.Add(new OscArg(ReadString(data, ref pos)));
                        break;
                    case 'b':
                        if (pos + 4 > data.Length) return msg;
                        int blen = BinaryPrimitives.ReadInt32BigEndian(data.AsSpan(pos, 4));
                        pos += 4;
                        if (pos + blen > data.Length) return msg;
                        var blob = data[pos..(pos + blen)];
                        msg.Arguments.Add(new OscArg(blob));
                        pos += blen;
                        int bpad = (4 - (blen % 4)) % 4;
                        pos += bpad;
                        break;
                    case 'T':
                        msg.Arguments.Add(OscArg.True);
                        break;
                    case 'F':
                        msg.Arguments.Add(OscArg.False);
                        break;
                    case 'N':
                        msg.Arguments.Add(OscArg.Nil);
                        break;
                    case 'I':
                        msg.Arguments.Add(OscArg.Inf);
                        break;
                }
            }
            return msg;
        }
        catch
        {
            return null;
        }
    }

    private static string ReadString(byte[] data, ref int pos)
    {
        int start = pos;
        while (pos < data.Length && data[pos] != 0) pos++;
        var s = Encoding.ASCII.GetString(data, start, pos - start);
        pos++;
        int rem = pos % 4;
        if (rem != 0) pos += 4 - rem;
        return s;
    }
}
