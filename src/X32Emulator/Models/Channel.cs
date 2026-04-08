namespace X32Emulator.Models;

public class ChannelConfig
{
    public string Name { get; set; } = "";
    public int Color { get; set; } = 0;
    public int Icon { get; set; } = 0;
    public int Source { get; set; } = 0;
}

public class ChannelPreamp
{
    public float Trim { get; set; } = 0.5f;
    public bool PhantomPower { get; set; } = false;
    public bool Invert { get; set; } = false;
    public int Source { get; set; } = 0;
    public bool HighPassFilter { get; set; } = false;
    public float HpfFreq { get; set; } = 0.25f;
}

public class GateComp
{
    public bool On { get; set; } = false;
    public string Mode { get; set; } = "gate";
    public float Threshold { get; set; } = 0.5f;
    public float Range { get; set; } = 0.5f;
    public float Attack { get; set; } = 0.5f;
    public float Hold { get; set; } = 0.5f;
    public float Release { get; set; } = 0.5f;
    public int KeySource { get; set; } = 0;
    public float FilterFreq { get; set; } = 0.5f;
    public bool FilterOn { get; set; } = false;
}

public class Dynamics
{
    public bool On { get; set; } = false;
    public string Mode { get; set; } = "comp";
    public float Threshold { get; set; } = 0.5f;
    public float Ratio { get; set; } = 0.5f;
    public float Attack { get; set; } = 0.5f;
    public float Hold { get; set; } = 0.5f;
    public float Release { get; set; } = 0.5f;
    public float Gain { get; set; } = 0.5f;
    public int KneeMode { get; set; } = 0;
    public int EnvMode { get; set; } = 0;
    public int KeySource { get; set; } = 0;
    public bool MixOn { get; set; } = false;
    public float Mix { get; set; } = 1.0f;
    public bool AutoGain { get; set; } = false;
}

public class EqBand
{
    public string Type { get; set; } = "PEQ";
    public float Freq { get; set; } = 0.5f;
    public float Gain { get; set; } = 0.5f;
    public float Q { get; set; } = 0.5f;
}

public class EqSection
{
    public bool On { get; set; } = false;
    public EqBand[] Bands { get; set; } = Enumerable.Range(0, 4).Select(_ => new EqBand()).ToArray();
}

public class ChannelMix
{
    public bool On { get; set; } = true;
    public float Fader { get; set; } = 0.75f;
    public float Pan { get; set; } = 0.5f;
    public bool Mono { get; set; } = false;
    public BusSend[] BusSends { get; set; } = Enumerable.Range(0, 16).Select(_ => new BusSend()).ToArray();
    public MatrixSend[] MatrixSends { get; set; } = Enumerable.Range(0, 6).Select(_ => new MatrixSend()).ToArray();
}

public class BusSend
{
    public bool On { get; set; } = false;
    public float Level { get; set; } = 0.75f;
    public float Pan { get; set; } = 0.5f;
    public string Type { get; set; } = "LCR";
    public bool PanFollow { get; set; } = false;
    public string SendType { get; set; } = "POST";
}

public class MatrixSend
{
    public bool On { get; set; } = false;
    public float Level { get; set; } = 0.75f;
}

public class Channel
{
    public ChannelConfig Config { get; set; } = new();
    public ChannelPreamp Preamp { get; set; } = new();
    public GateComp Gate { get; set; } = new();
    public Dynamics Dyn { get; set; } = new();
    public EqSection Eq { get; set; } = new();
    public ChannelMix Mix { get; set; } = new();
    public int[] DcaAssign { get; set; } = new int[8];
    public int MuteAssign { get; set; } = 0;
}
