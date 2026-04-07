namespace X32Emulator.Models;

public class FxReturnConfig
{
    public string Name { get; set; } = "";
    public int Color { get; set; } = 0;
    public int Icon { get; set; } = 0;
}

public class FxReturnMix
{
    public bool On { get; set; } = true;
    public float Fader { get; set; } = 0.75f;
    public float Pan { get; set; } = 0.5f;
    public BusSend[] BusSends { get; set; } = Enumerable.Range(0, 16).Select(_ => new BusSend()).ToArray();
}

public class FxReturn
{
    public FxReturnConfig Config { get; set; } = new();
    public EqSection Eq { get; set; } = new();
    public FxReturnMix Mix { get; set; } = new();
}
