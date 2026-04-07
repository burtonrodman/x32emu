namespace X32Emulator.Models;

public class DcaConfig
{
    public string Name { get; set; } = "";
    public int Color { get; set; } = 0;
    public int Icon { get; set; } = 0;
}

public class DCA
{
    public DcaConfig Config { get; set; } = new();
    public bool On { get; set; } = true;
    public float Fader { get; set; } = 0.75f;
}
