using Microsoft.AspNetCore.SignalR;
using X32Emulator.Osc;
using X32Emulator.Services;

namespace X32Emulator.Hubs;

public class X32Hub : Hub
{
    private readonly X32StateService _stateService;

    public X32Hub(X32StateService stateService)
    {
        _stateService = stateService;
    }

    public Task SetParam(string path, string value)
    {
        List<OscArg> args;
        if (float.TryParse(value, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float f))
            args = new List<OscArg> { new OscArg(f) };
        else if (int.TryParse(value, out int i))
            args = new List<OscArg> { new OscArg(i) };
        else
            args = new List<OscArg> { new OscArg(value) };

        _stateService.SetValue(path, args);
        return Task.CompletedTask;
    }
}
