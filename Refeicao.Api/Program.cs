using Refeicao.Api.Config;
using Refeicao.Core.Logging;

var builder = WebApplication.CreateBuilder(args);
builder.ConfigurarSerilog();
builder.ConfigureServices();

var app = builder.Build();
app.ConfigurePipeline();

app.Run();

public partial class Program;
