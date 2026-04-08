namespace X32Emulator.Models;

public class FxSlot
{
    public string Type { get; set; } = "NONE";
    public float[] Params { get; set; } = new float[16];
}

public class ShowScene
{
    public string Name { get; set; } = "";
    public string Notes { get; set; } = "";
    public int Current { get; set; } = 0;
}

public class X32Config
{
    public string Name { get; set; } = "X32RACK";
    public int Bits { get; set; } = 24;
    public int Rate { get; set; } = 48000;
    public string LinkMode { get; set; } = "on";
    public int Amixermode { get; set; } = 0;
    public int Mute { get; set; } = 0;
}

public class X32State
{
    public Channel[] Channels { get; set; } = Enumerable.Range(0, 32).Select(i =>
    {
        var ch = new Channel();
        ch.Config.Name = $"Ch {i + 1:D2}";
        return ch;
    }).ToArray();

    public Bus[] Buses { get; set; } = Enumerable.Range(0, 16).Select(i =>
    {
        var b = new Bus();
        b.Config.Name = $"Bus {i + 1:D2}";
        return b;
    }).ToArray();

    public Matrix[] Matrices { get; set; } = Enumerable.Range(0, 6).Select(i =>
    {
        var m = new Matrix();
        m.Config.Name = $"Mtx {i + 1:D2}";
        return m;
    }).ToArray();

    public DCA[] DCAs { get; set; } = Enumerable.Range(0, 8).Select(i =>
    {
        var d = new DCA();
        d.Config.Name = $"DCA {i + 1}";
        return d;
    }).ToArray();

    public FxReturn[] FxReturns { get; set; } = Enumerable.Range(0, 8).Select(i =>
    {
        var fx = new FxReturn();
        fx.Config.Name = $"FxRtn {i + 1}";
        return fx;
    }).ToArray();

    public MainMix MainStereo { get; set; } = new() { Config = new() { Name = "MainLR" } };
    public MainMix MainMono { get; set; } = new() { Config = new() { Name = "MainM" } };

    public FxSlot[] FxSlots { get; set; } = Enumerable.Range(0, 4).Select(_ => new FxSlot()).ToArray();

    public ShowScene Scene { get; set; } = new();
    public X32Config Config { get; set; } = new();

    public Dictionary<string, object> RawParams { get; set; } = new();
}
