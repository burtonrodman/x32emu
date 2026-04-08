using X32Emulator.Models;
using X32Emulator.Services;
using X32Emulator.Osc;
using Xunit;

namespace X32Emulator.Tests;

public class X32StateTests
{
    [Fact]
    public void X32State_InitializesWithCorrectDefaultValues()
    {
        var state = new X32State();
        Assert.Equal(32, state.Channels.Length);
        Assert.Equal(16, state.Buses.Length);
        Assert.Equal(6, state.Matrices.Length);
        Assert.Equal(8, state.DCAs.Length);
        Assert.Equal(8, state.FxReturns.Length);
    }

    [Fact]
    public void ChannelNames_DefaultCorrectly()
    {
        var state = new X32State();
        Assert.Equal("Ch 01", state.Channels[0].Config.Name);
        Assert.Equal("Ch 16", state.Channels[15].Config.Name);
        Assert.Equal("Ch 32", state.Channels[31].Config.Name);
    }

    [Fact]
    public void DefaultFaderValues_Are075()
    {
        var state = new X32State();
        Assert.Equal(0.75f, state.Channels[0].Mix.Fader);
        Assert.Equal(0.75f, state.Buses[0].Mix.Fader);
        Assert.Equal(0.75f, state.MainStereo.Fader);
    }

    [Fact]
    public void GetValue_ReturnsCorrectChannelFader()
    {
        var service = new X32StateService();
        var result = service.GetValue("/ch/01/mix/fader");
        Assert.NotNull(result);
        Assert.Equal(OscArgType.Float, result!.Type);
        Assert.Equal(0.75f, result.AsFloat(), 5);
    }

    [Fact]
    public void SetValue_UpdatesStateCorrectly()
    {
        var service = new X32StateService();
        service.SetValue("/ch/01/mix/fader", new List<OscArg> { new OscArg(0.5f) });
        var result = service.GetValue("/ch/01/mix/fader");
        Assert.NotNull(result);
        Assert.Equal(0.5f, result!.AsFloat(), 5);
    }

    [Fact]
    public void SetValue_FiresOnStateChanged()
    {
        var service = new X32StateService();
        string? changedPath = null;
        service.OnStateChanged += (path, args) => changedPath = path;
        service.SetValue("/ch/01/mix/fader", new List<OscArg> { new OscArg(0.5f) });
        Assert.Equal("/ch/01/mix/fader", changedPath);
    }
}
