namespace X32Emulator.Osc;

public enum OscArgType { Int, Float, String, Blob, True, False, Nil, Inf }

public class OscArg
{
    public OscArgType Type { get; }
    public object? Value { get; }

    public OscArg(int v) { Type = OscArgType.Int; Value = v; }
    public OscArg(float v) { Type = OscArgType.Float; Value = v; }
    public OscArg(string v) { Type = OscArgType.String; Value = v; }
    public OscArg(byte[] v) { Type = OscArgType.Blob; Value = v; }
    private OscArg(OscArgType t) { Type = t; Value = null; }

    public static OscArg True => new(OscArgType.True);
    public static OscArg False => new(OscArgType.False);
    public static OscArg Nil => new(OscArgType.Nil);
    public static OscArg Inf => new(OscArgType.Inf);

    public int AsInt() => (int)(Value ?? 0);
    public float AsFloat() => (float)(Value ?? 0f);
    public string AsString() => (string)(Value ?? "");
    public byte[] AsBlob() => (byte[])(Value ?? Array.Empty<byte>());
    public bool AsBool() => Type == OscArgType.True || (Type == OscArgType.Int && AsInt() != 0);
}

public class OscMessage
{
    public string Address { get; set; } = "";
    public List<OscArg> Arguments { get; set; } = new();

    public OscMessage() { }
    public OscMessage(string address, params OscArg[] args)
    {
        Address = address;
        Arguments.AddRange(args);
    }
}
