using Microsoft.AspNetCore.Builder;
using Serilog;

namespace Refeicao.Core.Logging;

public static class SerilogConfiguration
{
    public static void ConfigurarSerilog(this WebApplicationBuilder builder)
    {
        builder.Host.UseSerilog((contexto, configuracaoLog) =>
        {
            configuracaoLog
                .MinimumLevel.Information()
                .Enrich.FromLogContext()
                .WriteTo.Console()
                .WriteTo.File(
                    "logs/refeicao-.log",
                    rollingInterval: RollingInterval.Day,
                    retainedFileCountLimit: 14);
        });
    }
}
