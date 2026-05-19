using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using CURSO_INTERCULTURALIDAD.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace CURSO_INTERCULTURALIDAD.Services
{
    public sealed class CorreoConfirmacionService : ICorreoConfirmacionService
    {
        private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

        private readonly IHttpClientFactory _httpClientFactory;
        private readonly CorreoConfirmacionOptions _options;
        private readonly ILogger<CorreoConfirmacionService> _logger;

        public CorreoConfirmacionService(
            IHttpClientFactory httpClientFactory,
            IOptions<CorreoConfirmacionOptions> options,
            ILogger<CorreoConfirmacionService> logger)
        {
            _httpClientFactory = httpClientFactory;
            _options = options.Value;
            _logger = logger;
        }

        public async Task<ResultadoCorreoConfirmacion> EnviarAsync(
            string correo,
            string nombres,
            string apellidos,
            long cursoId,
            string cursoNombre,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(_options.Url))
            {
                return new ResultadoCorreoConfirmacion
                {
                    Exito = false,
                    StatusCode = StatusCodes.Status500InternalServerError,
                    Mensaje = "No se configuro el servicio de correo de confirmacion."
                };
            }

            try
            {
                using var client = _httpClientFactory.CreateClient();
                using var request = new HttpRequestMessage(HttpMethod.Post, _options.Url);
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                if (!string.IsNullOrWhiteSpace(_options.ApiKey))
                {
                    request.Headers.TryAddWithoutValidation("x-api-key", _options.ApiKey);
                }

                var payload = JsonSerializer.Serialize(new
                {
                    correo = correo.Trim(),
                    nombres = nombres.Trim(),
                    apellidos = apellidos.Trim(),
                    cursoId = cursoId.ToString(),
                    cursoNombre = cursoNombre.Trim(),
                    recaptchaToken = string.Empty
                }, JsonOptions);

                request.Content = new StringContent(payload, Encoding.UTF8, "application/json");

                using var response = await client.SendAsync(request, cancellationToken);
                var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    return new ResultadoCorreoConfirmacion
                    {
                        Exito = false,
                        StatusCode = (int)response.StatusCode,
                        Mensaje = string.IsNullOrWhiteSpace(responseBody)
                            ? "No se pudo enviar el correo de confirmacion."
                            : responseBody
                    };
                }

                return new ResultadoCorreoConfirmacion
                {
                    Exito = true,
                    StatusCode = (int)response.StatusCode,
                    Mensaje = "Correo de confirmacion enviado correctamente."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al enviar el correo de confirmacion del curso {CursoId}.", cursoId);
                return new ResultadoCorreoConfirmacion
                {
                    Exito = false,
                    StatusCode = StatusCodes.Status500InternalServerError,
                    Mensaje = "No se pudo conectar con el servicio de correo de confirmacion."
                };
            }
        }
    }
}
