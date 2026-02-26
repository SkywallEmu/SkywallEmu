using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SkywallEmu.Game.Services.Hosted;
using SkywallEmu.Game.Services.Singletons;
using SkywallEmu.Utils.Configuration;
using SkywallEmu.Utils.Services;

namespace SkywallEmu;

class Program
{
    static async Task Main(string[] args)
    {
        var builder = Host.CreateEmptyApplicationBuilder(null);

        builder.Configuration.AddJsonFile("appsettings.json", false, false);

        // Register the loaded configuration in the AppSettings accessor class
        AppSettings.SetConfigurationManager(builder.Configuration);

        builder.Services.AddSingleton<AuthenticationService>();
        builder.Services.AddSingleton<SessionService>();
        builder.Services.AddHostedService<GruntLoginService>();

        var app = builder.Build();

        ServiceAccessor.SetServiceProvider(app.Services);

        await app.RunAsync();
    }
}
