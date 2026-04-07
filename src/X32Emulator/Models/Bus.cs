namespace X32Emulator.Models;

public class BusConfig
{
    public string Name { get; set; } = "";
    public int Color { get; set; } = 0;
    public int Icon { get; set; } = 0;
    public string Source { get; set; } = "LR";
}

public class BusMix
{
    public bool On { get; set; } = true;
    public float Fader { get; set; } = 0.75f;
    public float Pan { get; set; } = 0.5f;
    public bool Mono { get; set; } = false;
    public MatrixSend[] MatrixSends { get; set; } = Enumerable.Range(0, 6).Select(_ => new MatrixSend()).ToArray();
}

public class Bus
{
    public BusConfig Config { get; set; } = new();
    public Dynamics Dyn { get; set; } = new();
    public EqSection Eq { get; set; } = new();
    public BusMix Mix { get; set; } = new();
    public int MuteAssign { get; set; } = 0;
}
