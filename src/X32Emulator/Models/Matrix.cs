namespace X32Emulator.Models;

public class MatrixConfig
{
    public string Name { get; set; } = "";
    public int Color { get; set; } = 0;
    public int Icon { get; set; } = 0;
}

public class MatrixMix
{
    public bool On { get; set; } = true;
    public float Fader { get; set; } = 0.75f;
    public float Pan { get; set; } = 0.5f;
    public bool Mono { get; set; } = false;
}

public class Matrix
{
    public MatrixConfig Config { get; set; } = new();
    public Dynamics Dyn { get; set; } = new();
    public EqSection Eq { get; set; } = new();
    public MatrixMix Mix { get; set; } = new();
}
