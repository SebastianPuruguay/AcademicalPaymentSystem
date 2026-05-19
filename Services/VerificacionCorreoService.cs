using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using CURSO_INTERCULTURALIDAD.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace CURSO_INTERCULTURALIDAD.Services
{
    public sealed class VerificacionCorreoService : IVerificacionCorreoService
    {
        private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

        private readonly IHttpClientFactory _httpClientFactory;
        private readonly VerificacionCorreoOptions _options;
        private readonly ILogger<VerificacionCorreoService> _logger;

        public VerificacionCorreoService(
            IHttpClientFactory httpClientFactory,
            IOptions<VerificacionCorreoOptions> options,
            ILogger<VerificacionCorreoService> logger)
        {
            _httpClientFactory = httpClientFactory;
            _options = options.Value;
            _logger = logger;
        }

        public Task<ResultadoVerificacionCorreo> SolicitarCodigoAsync(string correo, string nombreCompleto, CancellationToken cancellationToken = default)
        {
            return PostAsync(
                _options.SolicitarCodigoUrl,
                new
                {
                    email = correo,
                    nombre = string.IsNullOrWhiteSpace(nombreCompleto) ? "Alumno" : nombreCompleto.Trim()
                },
                "solicitar codigo de verificacion por correo",
                cancellationToken);
        }

        public Task<ResultadoVerificacionCorreo> ValidarCodigoAsync(string correo, string codigo, CancellationToken cancellationToken = default)
        {
            return PostAsync(
                _options.ValidarCodigoUrl,
                new
                {
                    email = correo,
                    code = codigo
                },
                "validar codigo de verificacion por correo",
                cancellationToken);
        }

        private async Task<ResultadoVerificacionCorreo> PostAsync(
            string url,
            object payload,
            string operationName,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                return new ResultadoVerificacionCorreo
                {
                    Exito = false,
                    StatusCode = StatusCodes.Status500InternalServerError,
                    Mensaje = "No se configuro el servicio de verificacion por correo."
                };
            }

            try
            {
                using var client = _httpClientFactory.CreateClient();
                using var request = new HttpRequestMessage(HttpMethod.Post, url);
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                if (!string.IsNullOrWhiteSpace(_options.ApiKey))
                {
                    request.Headers.TryAddWithoutValidation("x-api-key", _options.ApiKey);
                }

                var jsonPayload = JsonSerializer.Serialize(payload, JsonOptions);
                request.Content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

                using var response = await client.SendAsync(request, cancellationToken);
                var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
                var parsed = ParseResponse(responseBody);

                if (!response.IsSuccessStatusCode)
                {
                    return new ResultadoVerificacionCorreo
                    {
                        Exito = false,
                        StatusCode = (int)response.StatusCode,
                        Mensaje = parsed.Error ?? parsed.Message ?? "No se pudo completar la verificacion por correo."
                    };
                }

                return new ResultadoVerificacionCorreo
                {
                    Exito = true,
                    StatusCode = (int)response.StatusCode,
                    Mensaje = parsed.Message ?? "Operacion completada correctamente."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al {OperationName}.", operationName);
                return new ResultadoVerificacionCorreo
                {
                    Exito = false,
                    StatusCode = StatusCodes.Status500InternalServerError,
                    Mensaje = "No se pudo conectar con el servicio de verificacion por correo."
                };
            }
        }

        private static VerificacionCorreoApiResponse ParseResponse(string responseBody)
        {
            if (string.IsNullOrWhiteSpace(responseBody))
            {
                return new VerificacionCorreoApiResponse();
            }

            try
            {
                return JsonSerializer.Deserialize<VerificacionCorreoApiResponse>(responseBody, JsonOptions) ?? new VerificacionCorreoApiResponse();
            }
            catch (JsonException)
            {
                return new VerificacionCorreoApiResponse();
            }
        }

        private sealed class VerificacionCorreoApiResponse
        {
            public string? Message { get; set; }
            public string? Error { get; set; }
        }
    }
}
