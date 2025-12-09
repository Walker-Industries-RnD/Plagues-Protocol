using MagicOnion.Server;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using XRUIOS.Linux.PublicAccountDataHandler;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenUnixSocket("/var/run/xruios/publicacc.sock", o => o.Protocols = HttpProtocols.Http2);
    // Or for testing: options.ListenLocalhost(5000, o => o.Protocols = HttpProtocols.Http2);
});

builder.Services.AddHttpContextAccessor();
builder.Services.AddSingleton<Worker>();
builder.Services.AddHostedService<Worker>();
builder.Services.AddMagicOnion();

var app = builder.Build();

app.MapMagicOnionService<PublicAccService>();

app.Run();