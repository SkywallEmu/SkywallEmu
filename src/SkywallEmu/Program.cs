using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using SkywallEmu.Utils.Configuration;

namespace SkywallEmu;

class Program
{
    static async Task Main(string[] args)
    {
        var builder = Host.CreateEmptyApplicationBuilder(null);

        builder.Configuration.AddJsonFile("appsettings.json");

        // Register the loaded configuration in the AppSettings accessor class
        AppSettings.SetConfigurationManager(builder.Configuration);

        var app = builder.Build();

        await app.RunAsync();
    }
}
