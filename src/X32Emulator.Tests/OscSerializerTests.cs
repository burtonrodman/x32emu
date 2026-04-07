using X32Emulator.Osc;
using Xunit;

namespace X32Emulator.Tests;

public class OscSerializerTests
{
    [Fact]
    public void RoundTrip_PreservesAddressAndArgs()
    {
        var msg = new OscMessage("/ch/01/mix/fader", new OscArg(0.75f));
        var bytes = OscSerializer.Serialize(msg);
        var result = OscSerializer.Deserialize(bytes);
        Assert.NotNull(result);
        Assert.Equal("/ch/01/mix/fader", result.Address);
        Assert.Single(result.Arguments);
        Assert.Equal(0.75f, result.Arguments[0].AsFloat(), 5);
    }

    [Fact]
    public void IntArg_SerializesAndDeserializesCorrectly()
    {
        var msg = new OscMessage("/test", new OscArg(42));
        var bytes = OscSerializer.Serialize(msg);
        var result = OscSerializer.Deserialize(bytes);
        Assert.NotNull(result);
        Assert.Equal(42, result!.Arguments[0].AsInt());
    }

    [Fact]
    public void FloatArg_SerializesAndDeserializesCorrectly()
    {
        var msg = new OscMessage("/test", new OscArg(3.14f));
        var bytes = OscSerializer.Serialize(msg);
        var result = OscSerializer.Deserialize(bytes);
        Assert.NotNull(result);
        Assert.Equal(3.14f, result!.Arguments[0].AsFloat(), 5);
    }

    [Fact]
    public void StringArg_SerializesAndDeserializesCorrectly()
    {
        var msg = new OscMessage("/test", new OscArg("hello"));
        var bytes = OscSerializer.Serialize(msg);
        var result = OscSerializer.Deserialize(bytes);
        Assert.NotNull(result);
        Assert.Equal("hello", result!.Arguments[0].AsString());
    }

    [Fact]
    public void MixedArgs_WorkCorrectly()
    {
        var msg = new OscMessage("/test", new OscArg(1), new OscArg(2.0f), new OscArg("three"));
        var bytes = OscSerializer.Serialize(msg);
        var result = OscSerializer.Deserialize(bytes);
        Assert.NotNull(result);
        Assert.Equal(3, result!.Arguments.Count);
        Assert.Equal(1, result.Arguments[0].AsInt());
        Assert.Equal(2.0f, result.Arguments[1].AsFloat(), 5);
        Assert.Equal("three", result.Arguments[2].AsString());
    }

    [Fact]
    public void XInfoMessage_SerializesCorrectly()
    {
        var msg = new OscMessage("/xinfo", new OscArg("192.168.1.1"), new OscArg("X32EMU"), new OscArg("X32RACK"), new OscArg("4.06"));
        var bytes = OscSerializer.Serialize(msg);
        var result = OscSerializer.Deserialize(bytes);
        Assert.NotNull(result);
        Assert.Equal("/xinfo", result!.Address);
        Assert.Equal(4, result.Arguments.Count);
        Assert.Equal("192.168.1.1", result.Arguments[0].AsString());
    }
}
