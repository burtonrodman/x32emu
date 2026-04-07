using Microsoft.AspNetCore.SignalR;
using X32Emulator.Hubs;
using X32Emulator.Services;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSignalR();
builder.Services.AddSingleton<X32StateService>();
builder.Services.AddSingleton<SubscriptionService>();
builder.Services.AddSingleton<SceneService>();
builder.Services.AddHostedService<OscServer>();
builder.Services.AddHostedService<McuServer>();
builder.Services.AddHostedService(sp => sp.GetRequiredService<SubscriptionService>());

var app = builder.Build();
app.UseStaticFiles();
app.MapHub<X32Hub>("/hub");
app.MapGet("/api/state", (X32StateService svc) => Results.Json(svc.State));
app.MapPost("/api/scenes/{name}/load", async (string name, SceneService scenes) =>
{
    scenes.LoadScene(name);
    return Results.Ok();
});
app.MapGet("/api/scenes", (SceneService scenes) => Results.Json(scenes.ListScenes()));

var stateService = app.Services.GetRequiredService<X32StateService>();
var hubContext = app.Services.GetRequiredService<Microsoft.AspNetCore.SignalR.IHubContext<X32Hub>>();
stateService.OnStateChanged += async (path, args) =>
{
    var value = args.Count > 0 ? args[0].Type switch
    {
        X32Emulator.Osc.OscArgType.Float => args[0].AsFloat().ToString(),
        X32Emulator.Osc.OscArgType.Int => args[0].AsInt().ToString(),
        X32Emulator.Osc.OscArgType.String => args[0].AsString(),
        _ => ""
    } : "";
    await hubContext.Clients.All.SendAsync("ParamChanged", path, value);
};

app.Run("http://0.0.0.0:8080");
