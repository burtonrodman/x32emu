namespace X32Emulator.Models;

public class MainMixConfig
{
    public string Name { get; set; } = "MainLR";
    public int Color { get; set; } = 0;
    public int Icon { get; set; } = 0;
}

public class MainMix
{
    public MainMixConfig Config { get; set; } = new();
    public bool On { get; set; } = true;
    public float Fader { get; set; } = 0.75f;
    public float Pan { get; set; } = 0.5f;
    public Dynamics Dyn { get; set; } = new();
    public EqSection Eq { get; set; } = new();
    public MatrixSend[] MatrixSends { get; set; } = Enumerable.Range(0, 6).Select(_ => new MatrixSend()).ToArray();
}
