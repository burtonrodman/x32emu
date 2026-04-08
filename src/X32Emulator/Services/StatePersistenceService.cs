using System.Text.Json;
using System.Text.Json.Serialization;
using X32Emulator.Models;
using X32Emulator.Osc;

namespace X32Emulator.Services;

public class StatePersistenceService : IHostedService
{
    private const string StateFilePath = "/data/scenes/current_state.json";

    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        WriteIndented = true,
        Converters = { new RawParamValueConverter() }
    };

    private readonly X32StateService _stateService;
    private Timer? _debounceTimer;
    private readonly object _timerLock = new();

    public StatePersistenceService(X32StateService stateService)
    {
        _stateService = stateService;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        LoadState();
        _stateService.OnStateChanged += OnStateChanged;
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _stateService.OnStateChanged -= OnStateChanged;
        lock (_timerLock)
        {
            _debounceTimer?.Dispose();
            _debounceTimer = null;
        }
        SaveState();
        return Task.CompletedTask;
    }

    private void OnStateChanged(string path, List<OscArg> args)
    {
        lock (_timerLock)
        {
            _debounceTimer?.Dispose();
            _debounceTimer = new Timer(_ => SaveState(), null, TimeSpan.FromSeconds(2), Timeout.InfiniteTimeSpan);
        }
    }

    private void LoadState()
    {
        try
        {
            if (!File.Exists(StateFilePath)) return;
            var json = File.ReadAllText(StateFilePath);
            var state = JsonSerializer.Deserialize<X32State>(json, SerializerOptions);
            if (state != null)
                _stateService.RestoreState(state);
        }
        catch { }
    }

    private void SaveState()
    {
        try
        {
            var dir = Path.GetDirectoryName(StateFilePath)!;
            Directory.CreateDirectory(dir);
            var json = JsonSerializer.Serialize(_stateService.State, SerializerOptions);
            var tmpPath = StateFilePath + ".tmp";
            File.WriteAllText(tmpPath, json);
            File.Move(tmpPath, StateFilePath, overwrite: true);
        }
        catch { }
    }
}

internal class RawParamValueConverter : JsonConverter<object>
{
    public override object? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return reader.TokenType switch
        {
            JsonTokenType.Number when reader.TryGetInt32(out int i) => (object)i,
            JsonTokenType.Number => reader.GetSingle(),
            JsonTokenType.String => reader.GetString()!,
            JsonTokenType.True => (object)1,
            JsonTokenType.False => (object)0,
            _ => ""
        };
    }

    public override void Write(Utf8JsonWriter writer, object value, JsonSerializerOptions options)
    {
        switch (value)
        {
            case int i: writer.WriteNumberValue(i); break;
            case float f: writer.WriteNumberValue(f); break;
            case string s: writer.WriteStringValue(s); break;
            default: writer.WriteStringValue(value?.ToString() ?? ""); break;
        }
    }
}
