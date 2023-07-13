using APIContagem.Models;

namespace APIContagem.Endpoints;

public static class EndpointsContagem
{
    public static RouteHandlerBuilder MapContador(this WebApplication app, string endpoint) =>
        app.MapGet(endpoint, (Contador contador) =>
        {
            int valorAtualContador;
            lock (contador)
            {
                contador.Incrementar();
                valorAtualContador = contador.ValorAtual;
            }
            app.Logger.LogInformation($"Contador - Valor atual: {valorAtualContador}");

            return Results.Ok(new ResultadoContador()
            {
                ValorAtual = contador.ValorAtual,
                Local = contador.Local,
                Kernel = contador.Kernel,
                Framework = contador.Framework,
                Mensagem = $"Utilizando o endpoint {endpoint}"
            });
        })
        .Produces<ResultadoContador>()
        .Produces(StatusCodes.Status429TooManyRequests);
}