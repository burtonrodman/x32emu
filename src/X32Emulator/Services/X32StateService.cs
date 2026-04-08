using X32Emulator.Models;
using X32Emulator.Osc;

namespace X32Emulator.Services;

public class X32StateService
{
    public X32State State { get; private set; } = new();

    public void RestoreState(X32State state)
    {
        State = state;
    }
    public event Action<string, List<OscArg>>? OnStateChanged;

    public OscArg? GetValue(string path)
    {
        path = path.TrimStart('/');
        var parts = path.Split('/');

        try
        {
            if (parts[0] == "ch" && parts.Length >= 2 && int.TryParse(parts[1], out int chNum))
            {
                var ch = State.Channels[chNum - 1];
                if (parts.Length >= 4)
                {
                    if (parts[2] == "mix" && parts.Length >= 5 && int.TryParse(parts[3], out int busNum) && busNum >= 1 && busNum <= 16)
                    {
                        return parts[4] switch
                        {
                            "level" => new OscArg(ch.Mix.BusSends[busNum - 1].Level),
                            "on" => new OscArg(ch.Mix.BusSends[busNum - 1].On ? 1 : 0),
                            "pan" => new OscArg(ch.Mix.BusSends[busNum - 1].Pan),
                            _ => null
                        };
                    }
                    return (parts[2], parts[3]) switch
                    {
                        ("config", "name") => new OscArg(ch.Config.Name),
                        ("config", "color") => new OscArg(ch.Config.Color),
                        ("config", "icon") => new OscArg(ch.Config.Icon),
                        ("mix", "fader") => new OscArg(ch.Mix.Fader),
                        ("mix", "on") => new OscArg(ch.Mix.On ? 1 : 0),
                        ("mix", "pan") => new OscArg(ch.Mix.Pan),
                        ("gate", "on") => new OscArg(ch.Gate.On ? 1 : 0),
                        ("dyn", "on") => new OscArg(ch.Dyn.On ? 1 : 0),
                        ("eq", "on") => new OscArg(ch.Eq.On ? 1 : 0),
                        ("preamp", "trim") => new OscArg(ch.Preamp.Trim),
                        ("preamp", "hpon") => new OscArg(ch.Preamp.HighPassFilter ? 1 : 0),
                        ("preamp", "inv") => new OscArg(ch.Preamp.Invert ? 1 : 0),
                        _ => null
                    };
                }
            }

            if (parts[0] == "bus" && parts.Length >= 2 && int.TryParse(parts[1], out int busIdx))
            {
                var bus = State.Buses[busIdx - 1];
                if (parts.Length >= 4)
                {
                    return (parts[2], parts[3]) switch
                    {
                        ("config", "name") => new OscArg(bus.Config.Name),
                        ("config", "color") => new OscArg(bus.Config.Color),
                        ("mix", "fader") => new OscArg(bus.Mix.Fader),
                        ("mix", "on") => new OscArg(bus.Mix.On ? 1 : 0),
                        ("mix", "pan") => new OscArg(bus.Mix.Pan),
                        ("dyn", "on") => new OscArg(bus.Dyn.On ? 1 : 0),
                        ("eq", "on") => new OscArg(bus.Eq.On ? 1 : 0),
                        _ => null
                    };
                }
            }

            if (parts[0] == "mtx" && parts.Length >= 2 && int.TryParse(parts[1], out int mtxIdx))
            {
                var mtx = State.Matrices[mtxIdx - 1];
                if (parts.Length >= 4)
                {
                    return (parts[2], parts[3]) switch
                    {
                        ("config", "name") => new OscArg(mtx.Config.Name),
                        ("mix", "fader") => new OscArg(mtx.Mix.Fader),
                        ("mix", "on") => new OscArg(mtx.Mix.On ? 1 : 0),
                        ("mix", "pan") => new OscArg(mtx.Mix.Pan),
                        _ => null
                    };
                }
            }

            if (parts[0] == "dca" && parts.Length >= 2 && int.TryParse(parts[1], out int dcaIdx))
            {
                var dca = State.DCAs[dcaIdx - 1];
                if (parts.Length >= 3)
                {
                    if (parts[2] == "fader") return new OscArg(dca.Fader);
                    if (parts[2] == "on") return new OscArg(dca.On ? 1 : 0);
                    if (parts.Length >= 4 && parts[2] == "config")
                    {
                        return parts[3] switch
                        {
                            "name" => new OscArg(dca.Config.Name),
                            "color" => new OscArg(dca.Config.Color),
                            _ => null
                        };
                    }
                }
            }

            if (parts[0] == "fxrtn" && parts.Length >= 2 && int.TryParse(parts[1], out int fxIdx))
            {
                var fx = State.FxReturns[fxIdx - 1];
                if (parts.Length >= 4)
                {
                    return (parts[2], parts[3]) switch
                    {
                        ("config", "name") => new OscArg(fx.Config.Name),
                        ("mix", "fader") => new OscArg(fx.Mix.Fader),
                        ("mix", "on") => new OscArg(fx.Mix.On ? 1 : 0),
                        _ => null
                    };
                }
            }

            if (parts[0] == "main" && parts.Length >= 4 && parts[1] == "st")
            {
                return (parts[2], parts[3]) switch
                {
                    ("mix", "fader") => new OscArg(State.MainStereo.Fader),
                    ("mix", "on") => new OscArg(State.MainStereo.On ? 1 : 0),
                    ("mix", "pan") => new OscArg(State.MainStereo.Pan),
                    ("config", "name") => new OscArg(State.MainStereo.Config.Name),
                    _ => null
                };
            }

            if (parts[0] == "main" && parts.Length >= 4 && parts[1] == "m")
            {
                return (parts[2], parts[3]) switch
                {
                    ("mix", "fader") => new OscArg(State.MainMono.Fader),
                    ("mix", "on") => new OscArg(State.MainMono.On ? 1 : 0),
                    _ => null
                };
            }

            if (parts[0] == "config" && parts.Length >= 2)
            {
                return parts[1] switch
                {
                    "name" => new OscArg(State.Config.Name),
                    "bits" => new OscArg(State.Config.Bits),
                    "rate" => new OscArg(State.Config.Rate),
                    _ => null
                };
            }

            var fullPath = "/" + path;
            if (State.RawParams.TryGetValue(fullPath, out var raw))
            {
                return raw switch
                {
                    float f => new OscArg(f),
                    int i => new OscArg(i),
                    string s => new OscArg(s),
                    _ => null
                };
            }
        }
        catch { }
        return null;
    }

    public void SetValue(string path, List<OscArg> args)
    {
        if (args.Count == 0) return;
        path = path.TrimStart('/');
        var parts = path.Split('/');
        var arg = args[0];
        bool handled = false;

        try
        {
            if (parts[0] == "ch" && parts.Length >= 2 && int.TryParse(parts[1], out int chNum) && chNum >= 1 && chNum <= 32)
            {
                var ch = State.Channels[chNum - 1];
                if (parts.Length >= 4)
                {
                    if (parts[2] == "mix" && parts.Length >= 5 && int.TryParse(parts[3], out int busNum) && busNum >= 1 && busNum <= 16)
                    {
                        switch (parts[4])
                        {
                            case "level": ch.Mix.BusSends[busNum - 1].Level = arg.AsFloat(); handled = true; break;
                            case "on": ch.Mix.BusSends[busNum - 1].On = arg.AsInt() != 0; handled = true; break;
                            case "pan": ch.Mix.BusSends[busNum - 1].Pan = arg.AsFloat(); handled = true; break;
                        }
                    }
                    else
                    {
                        switch ((parts[2], parts[3]))
                        {
                            case ("config", "name"): ch.Config.Name = arg.AsString(); handled = true; break;
                            case ("config", "color"): ch.Config.Color = arg.AsInt(); handled = true; break;
                            case ("config", "icon"): ch.Config.Icon = arg.AsInt(); handled = true; break;
                            case ("mix", "fader"): ch.Mix.Fader = arg.AsFloat(); handled = true; break;
                            case ("mix", "on"): ch.Mix.On = arg.AsInt() != 0; handled = true; break;
                            case ("mix", "pan"): ch.Mix.Pan = arg.AsFloat(); handled = true; break;
                            case ("gate", "on"): ch.Gate.On = arg.AsInt() != 0; handled = true; break;
                            case ("dyn", "on"): ch.Dyn.On = arg.AsInt() != 0; handled = true; break;
                            case ("eq", "on"): ch.Eq.On = arg.AsInt() != 0; handled = true; break;
                            case ("preamp", "trim"): ch.Preamp.Trim = arg.AsFloat(); handled = true; break;
                            case ("preamp", "hpon"): ch.Preamp.HighPassFilter = arg.AsInt() != 0; handled = true; break;
                            case ("preamp", "inv"): ch.Preamp.Invert = arg.AsInt() != 0; handled = true; break;
                        }
                    }
                }
            }
            else if (parts[0] == "bus" && parts.Length >= 2 && int.TryParse(parts[1], out int busIdx) && busIdx >= 1 && busIdx <= 16)
            {
                var bus = State.Buses[busIdx - 1];
                if (parts.Length >= 4)
                {
                    switch ((parts[2], parts[3]))
                    {
                        case ("config", "name"): bus.Config.Name = arg.AsString(); handled = true; break;
                        case ("config", "color"): bus.Config.Color = arg.AsInt(); handled = true; break;
                        case ("mix", "fader"): bus.Mix.Fader = arg.AsFloat(); handled = true; break;
                        case ("mix", "on"): bus.Mix.On = arg.AsInt() != 0; handled = true; break;
                        case ("mix", "pan"): bus.Mix.Pan = arg.AsFloat(); handled = true; break;
                        case ("dyn", "on"): bus.Dyn.On = arg.AsInt() != 0; handled = true; break;
                        case ("eq", "on"): bus.Eq.On = arg.AsInt() != 0; handled = true; break;
                    }
                }
            }
            else if (parts[0] == "mtx" && parts.Length >= 2 && int.TryParse(parts[1], out int mtxIdx) && mtxIdx >= 1 && mtxIdx <= 6)
            {
                var mtx = State.Matrices[mtxIdx - 1];
                if (parts.Length >= 4)
                {
                    switch ((parts[2], parts[3]))
                    {
                        case ("config", "name"): mtx.Config.Name = arg.AsString(); handled = true; break;
                        case ("mix", "fader"): mtx.Mix.Fader = arg.AsFloat(); handled = true; break;
                        case ("mix", "on"): mtx.Mix.On = arg.AsInt() != 0; handled = true; break;
                        case ("mix", "pan"): mtx.Mix.Pan = arg.AsFloat(); handled = true; break;
                    }
                }
            }
            else if (parts[0] == "dca" && parts.Length >= 2 && int.TryParse(parts[1], out int dcaIdx) && dcaIdx >= 1 && dcaIdx <= 8)
            {
                var dca = State.DCAs[dcaIdx - 1];
                if (parts.Length >= 3)
                {
                    if (parts[2] == "fader") { dca.Fader = arg.AsFloat(); handled = true; }
                    else if (parts[2] == "on") { dca.On = arg.AsInt() != 0; handled = true; }
                    else if (parts.Length >= 4 && parts[2] == "config")
                    {
                        switch (parts[3])
                        {
                            case "name": dca.Config.Name = arg.AsString(); handled = true; break;
                            case "color": dca.Config.Color = arg.AsInt(); handled = true; break;
                        }
                    }
                }
            }
            else if (parts[0] == "fxrtn" && parts.Length >= 2 && int.TryParse(parts[1], out int fxIdx) && fxIdx >= 1 && fxIdx <= 8)
            {
                var fx = State.FxReturns[fxIdx - 1];
                if (parts.Length >= 4)
                {
                    switch ((parts[2], parts[3]))
                    {
                        case ("config", "name"): fx.Config.Name = arg.AsString(); handled = true; break;
                        case ("mix", "fader"): fx.Mix.Fader = arg.AsFloat(); handled = true; break;
                        case ("mix", "on"): fx.Mix.On = arg.AsInt() != 0; handled = true; break;
                    }
                }
            }
            else if (parts[0] == "main" && parts.Length >= 4 && parts[1] == "st")
            {
                switch ((parts[2], parts[3]))
                {
                    case ("mix", "fader"): State.MainStereo.Fader = arg.AsFloat(); handled = true; break;
                    case ("mix", "on"): State.MainStereo.On = arg.AsInt() != 0; handled = true; break;
                    case ("mix", "pan"): State.MainStereo.Pan = arg.AsFloat(); handled = true; break;
                }
            }
            else if (parts[0] == "main" && parts.Length >= 4 && parts[1] == "m")
            {
                switch ((parts[2], parts[3]))
                {
                    case ("mix", "fader"): State.MainMono.Fader = arg.AsFloat(); handled = true; break;
                    case ("mix", "on"): State.MainMono.On = arg.AsInt() != 0; handled = true; break;
                }
            }
            else if (parts[0] == "config" && parts.Length >= 2)
            {
                switch (parts[1])
                {
                    case "name": State.Config.Name = arg.AsString(); handled = true; break;
                    case "bits": State.Config.Bits = arg.AsInt(); handled = true; break;
                    case "rate": State.Config.Rate = arg.AsInt(); handled = true; break;
                }
            }

            if (!handled)
            {
                var fullPath = "/" + path;
                State.RawParams[fullPath] = arg.Type switch
                {
                    OscArgType.Float => (object)arg.AsFloat(),
                    OscArgType.Int => (object)arg.AsInt(),
                    OscArgType.String => (object)arg.AsString(),
                    _ => (object)""
                };
            }

            FireChanged("/" + path, args);
        }
        catch { }
    }

    public string GetNodeChildren(string path)
    {
        path = path.TrimStart('/');
        var parts = path.Split('/');

        if (parts[0] == "ch" && parts.Length == 1)
            return string.Join(" ", Enumerable.Range(1, 32).Select(i => $"{i:D2}"));
        if (parts[0] == "ch" && parts.Length == 2)
            return "config mix gate dyn eq preamp";
        if (parts[0] == "bus" && parts.Length == 1)
            return string.Join(" ", Enumerable.Range(1, 16).Select(i => $"{i:D2}"));
        if (parts[0] == "mtx" && parts.Length == 1)
            return string.Join(" ", Enumerable.Range(1, 6).Select(i => $"{i:D2}"));
        if (parts[0] == "dca" && parts.Length == 1)
            return string.Join(" ", Enumerable.Range(1, 8));
        if (parts[0] == "fxrtn" && parts.Length == 1)
            return string.Join(" ", Enumerable.Range(1, 8).Select(i => $"{i:D2}"));
        if (parts[0] == "main")
            return "st m";
        if (parts[0] == "config")
            return "name bits rate";
        return "";
    }

    private void FireChanged(string path, List<OscArg> args)
    {
        OnStateChanged?.Invoke(path, args);
    }
}
