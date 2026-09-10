using Refeicao.Api.Endpoints;
using Serilog;

namespace Refeicao.Api.Config;

public static class AppConfiguration
{
    public static void ConfigurePipeline(this WebApplication app)
    {
        app.UseExceptionHandler();

        app.UseSerilogRequestLogging(options =>
        {
            options.MessageTemplate =
                "HTTP {RequestMethod} {RequestPath} respondeu {StatusCode} em {Elapsed:0.0000} ms para {RemoteIpAddress}";

            options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
            {
                diagnosticContext.Set("RemoteIpAddress", httpContext.Connection.RemoteIpAddress?.ToString());
            };
        });

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseCors(BuilderConfiguration.CorsPolicyName);

        app.UseRateLimiter();

        app.UseAuthentication();
        app.UseAuthorization();

        app.MapAuthEndpoints();
        app.MapUsuariosEndpoints();
        app.MapFuncionariosEndpoints();
        app.MapAcompanhamentoEndpoints();
        app.MapCardapioEndpoints();
        app.MapRefeicaoEndpoints();
    }
}
