using System.Net.Http.Json;
using System.Text.Json;
using CURSO_INTERCULTURALIDAD.Models;
using Microsoft.Extensions.Options;

namespace CURSO_INTERCULTURALIDAD.Services
{
    public sealed class PagoIzipayService : IPagoIzipayService
    {
        private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

        private readonly IHttpClientFactory _httpClientFactory;
        private readonly PagoIzipayOptions _options;
        private readonly ILogger<PagoIzipayService> _logger;

        public PagoIzipayService(
            IHttpClientFactory httpClientFactory,
            IOptions<PagoIzipayOptions> options,
            ILogger<PagoIzipayService> logger)
        {
            _httpClientFactory = httpClientFactory;
            _options = options.Value;
            _logger = logger;
        }

        public bool EstaConfigurado =>
            !string.IsNullOrWhiteSpace(_options.CrearPagoUrl) &&
            !string.IsNullOrWhiteSpace(_options.ApiKey);

        public SolicitudCrearPagoIzipay ConstruirSolicitud(PagoDemoViewModel pago)
        {
            ArgumentNullException.ThrowIfNull(pago);

            var totalCuotas = Math.Max(pago.NumeroCuotasTotal, pago.NumeroCuota);
            var referencia = $"INS-{pago.IdInscripcion}-C{pago.NumeroCuota:D2}";
            var departamento = ResolverDepartamento(pago);
            var ciudad = ResolverCiudad(pago);

            return new SolicitudCrearPagoIzipay
            {
                IdReferencia = referencia,
                CodigoExterno = referencia,
                TituloPasarela = pago.NombreCurso,
                InformacionAdicional = $"{totalCuotas} {(totalCuotas == 1 ? "Cuota" : "Cuotas")}",
                InformacionExtra = $"Cuota {pago.NumeroCuota} de {totalCuotas}",
                Nombres = pago.Nombres,
                Apellidos = pago.Apellidos,
                Email = pago.Correo,
                Celular = ResolverCelular(pago),
                Direccion = _options.DireccionPredeterminada,
                Ciudad = ciudad,
                Departamento = departamento,
                Pais = ResolverPaisIso(pago.Pais),
                PostalCode = _options.PostalCodePredeterminado,
                Monto = decimal.Round(pago.Monto, 2, MidpointRounding.AwayFromZero),
                TipoDocumento = ResolverTipoDocumento(pago.TipoDocumento),
                NroDocumento = pago.NumeroDocumento,
                Descripcion = $"{_options.DescripcionPredeterminada} - {pago.NombreCurso} - Cuota {pago.NumeroCuota}",
                CodigoCPMS = (pago.CodigoCentroCosto ?? string.Empty).Trim(),
                ApiKey = _options.ApiKey
            };
        }

        public string? ConstruirUrlResumenPago(long idPagoIzipay)
        {
            if (idPagoIzipay <= 0)
            {
                return null;
            }

            var crearUrl = (_options.CrearPagoUrl ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(crearUrl))
            {
                return null;
            }

            const string apiCrearSegment = "/api/pagoizipay/crear";
            var index = crearUrl.LastIndexOf(apiCrearSegment, StringComparison.OrdinalIgnoreCase);
            var baseUrl = index >= 0
                ? crearUrl[..index]
                : crearUrl.EndsWith("/crear", StringComparison.OrdinalIgnoreCase)
                    ? crearUrl[..^"/crear".Length]
                    : crearUrl.TrimEnd('/');

            return $"{baseUrl.TrimEnd('/')}/PagoIziPayViews/ResumenPago?idPagoIziPay={idPagoIzipay}";
        }

        public async Task<RespuestaCrearPagoIzipay> CrearPagoAsync(PagoDemoViewModel pago, CancellationToken cancellationToken = default)
        {
            if (!EstaConfigurado)
            {
                return new RespuestaCrearPagoIzipay
                {
                    Exitoso = false,
                    Mensaje = "No se configuro el servicio de pago IziPay.",
                    CodigoError = "CONFIG_MISSING"
                };
            }

            var solicitud = ConstruirSolicitud(pago);
            if (string.IsNullOrWhiteSpace(solicitud.CodigoCPMS))
            {
                return new RespuestaCrearPagoIzipay
                {
                    Exitoso = false,
                    Mensaje = "El curso no tiene CodigoCPMS configurado. Complete tabla_central.codigo_centro_costo antes de generar el enlace de pago.",
                    CodigoError = "CODIGO_CPMS_MISSING"
                };
            }

            var client = _httpClientFactory.CreateClient("PagoIzipay");

            try
            {
                using var response = await client.PostAsJsonAsync(_options.CrearPagoUrl, solicitud, JsonOptions, cancellationToken);
                var contenido = await response.Content.ReadAsStringAsync(cancellationToken);
                var resultado = DeserializarRespuesta<RespuestaCrearPagoIzipay>(contenido);

                if (!response.IsSuccessStatusCode)
                {
                    return new RespuestaCrearPagoIzipay
                    {
                        Exitoso = false,
                        Mensaje = ResolverMensajeErrorHttp(
                            resultado?.Mensaje,
                            contenido,
                            $"IziPay devolvio HTTP {(int)response.StatusCode} al crear el pago. Verifique la URL configurada y los datos enviados."),
                        CodigoError = string.IsNullOrWhiteSpace(resultado?.CodigoError)
                            ? $"HTTP_{(int)response.StatusCode}"
                            : resultado.CodigoError
                    };
                }

                if (resultado is null)
                {
                    return new RespuestaCrearPagoIzipay
                    {
                        Exitoso = false,
                        Mensaje = "IziPay devolvio una respuesta no valida al crear el pago.",
                        CodigoError = "INVALID_JSON_RESPONSE"
                    };
                }

                if (resultado.Exitoso && string.IsNullOrWhiteSpace(resultado.UrlPago))
                {
                    return new RespuestaCrearPagoIzipay
                    {
                        Exitoso = false,
                        Mensaje = "IziPay creo el pago, pero no devolvio una URL de pago.",
                        CodigoError = "URL_PAGO_MISSING",
                        IdPagoIziPay = resultado.IdPagoIziPay
                    };
                }

                return resultado;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creando el pago IziPay para la referencia {Referencia} en {CrearPagoUrl}.", solicitud.IdReferencia, _options.CrearPagoUrl);
                return new RespuestaCrearPagoIzipay
                {
                    Exitoso = false,
                    Mensaje = "No se pudo conectar con el servicio de pago IziPay.",
                    CodigoError = "HTTP_EXCEPTION"
                };
            }
        }

        public async Task<RespuestaEstadoPagoIzipay> ConsultarEstadoAsync(long idPagoIzipay, CancellationToken cancellationToken = default)
        {
            if (!EstaConfigurado)
            {
                return new RespuestaEstadoPagoIzipay
                {
                    Exitoso = false,
                    Mensaje = "No se configuro el servicio de consulta de pago IziPay.",
                    CodigoError = "CONFIG_MISSING",
                    IdPagoIziPay = idPagoIzipay
                };
            }

            var estadoUrl = ConstruirEstadoPagoUrl(idPagoIzipay);
            var client = _httpClientFactory.CreateClient("PagoIzipay");

            try
            {
                using var response = await client.GetAsync(estadoUrl, cancellationToken);
                var contenido = await response.Content.ReadAsStringAsync(cancellationToken);
                var resultado = DeserializarRespuesta<RespuestaEstadoPagoIzipay>(contenido);

                if (!response.IsSuccessStatusCode)
                {
                    return new RespuestaEstadoPagoIzipay
                    {
                        Exitoso = false,
                        Mensaje = ResolverMensajeErrorHttp(
                            resultado?.Mensaje,
                            contenido,
                            $"IziPay devolvio HTTP {(int)response.StatusCode} al consultar el estado del pago."),
                        CodigoError = string.IsNullOrWhiteSpace(resultado?.CodigoError)
                            ? $"HTTP_{(int)response.StatusCode}"
                            : resultado.CodigoError,
                        IdPagoIziPay = idPagoIzipay
                    };
                }

                return resultado ?? new RespuestaEstadoPagoIzipay
                {
                    Exitoso = false,
                    Mensaje = "IziPay devolvio una respuesta no valida al consultar el estado del pago.",
                    CodigoError = "INVALID_JSON_RESPONSE",
                    IdPagoIziPay = idPagoIzipay
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error consultando el estado IziPay para el pago {IdPagoIzipay}.", idPagoIzipay);
                return new RespuestaEstadoPagoIzipay
                {
                    Exitoso = false,
                    Mensaje = "No se pudo conectar con el servicio de estado IziPay.",
                    CodigoError = "HTTP_EXCEPTION",
                    IdPagoIziPay = idPagoIzipay
                };
            }
        }

        private static T? DeserializarRespuesta<T>(string contenido)
            where T : class
        {
            if (string.IsNullOrWhiteSpace(contenido))
            {
                return null;
            }

            try
            {
                return JsonSerializer.Deserialize<T>(contenido, JsonOptions);
            }
            catch (JsonException)
            {
                return null;
            }
        }

        private static string ResolverMensajeErrorHttp(string? mensajeIzipay, string contenido, string mensajePredeterminado)
        {
            if (!string.IsNullOrWhiteSpace(mensajeIzipay))
            {
                return mensajeIzipay;
            }

            var contenidoRecortado = RecortarContenido(contenido);
            return string.IsNullOrWhiteSpace(contenidoRecortado)
                ? mensajePredeterminado
                : $"{mensajePredeterminado} Respuesta: {contenidoRecortado}";
        }

        private static string RecortarContenido(string contenido)
        {
            var texto = (contenido ?? string.Empty).Trim();
            if (texto.Length == 0)
            {
                return string.Empty;
            }

            const int maxLength = 220;
            return texto.Length <= maxLength
                ? texto
                : $"{texto[..maxLength]}...";
        }

        private string ConstruirEstadoPagoUrl(long idPagoIzipay)
        {
            var baseUrl = ResolverEstadoPagoBaseUrl();
            var separator = baseUrl.Contains('?', StringComparison.Ordinal) ? '&' : '?';
            return $"{baseUrl}{separator}idPagoIziPay={idPagoIzipay}&token={Uri.EscapeDataString(_options.ApiKey)}";
        }

        private string ResolverEstadoPagoBaseUrl()
        {
            if (!string.IsNullOrWhiteSpace(_options.EstadoPagoUrl))
            {
                return _options.EstadoPagoUrl.Trim();
            }

            var crearUrl = (_options.CrearPagoUrl ?? string.Empty).Trim();
            return crearUrl.EndsWith("/crear", StringComparison.OrdinalIgnoreCase)
                ? $"{crearUrl[..^"/crear".Length]}/estado"
                : $"{crearUrl.TrimEnd('/')}/estado";
        }

        private string ResolverCelular(PagoDemoViewModel pago)
        {
            var raw = (pago.Celular ?? string.Empty).Trim();
            var codigoPais = ResolverCodigoPais(pago.CodigoPais);
            if (string.IsNullOrWhiteSpace(raw))
            {
                return $"{codigoPais}000000000";
            }

            var digitos = new string(raw.Where(char.IsDigit).ToArray());
            if (raw.StartsWith('+') && !string.IsNullOrWhiteSpace(digitos))
            {
                return $"+{digitos}";
            }

            var digitosCodigoPais = new string(codigoPais.Where(char.IsDigit).ToArray());
            if (!string.IsNullOrWhiteSpace(digitosCodigoPais)
                && digitos.StartsWith(digitosCodigoPais, StringComparison.Ordinal)
                && digitos.Length > digitosCodigoPais.Length)
            {
                return $"+{digitos}";
            }

            return $"{codigoPais}{digitos}";
        }

        private static string ResolverCodigoPais(string? codigoPais)
        {
            var digitos = new string((codigoPais ?? string.Empty).Where(char.IsDigit).ToArray());
            return string.IsNullOrWhiteSpace(digitos)
                ? "+51"
                : $"+{digitos[..Math.Min(digitos.Length, 5)]}";
        }

        private string ResolverDepartamento(PagoDemoViewModel pago)
        {
            return string.IsNullOrWhiteSpace(pago.Region)
                ? _options.DepartamentoPredeterminado
                : pago.Region.Trim();
        }

        private string ResolverCiudad(PagoDemoViewModel pago)
        {
            return string.IsNullOrWhiteSpace(pago.Region)
                ? _options.CiudadPredeterminada
                : pago.Region.Trim();
        }

        private string ResolverPaisIso(string? pais)
        {
            if (string.IsNullOrWhiteSpace(pais))
            {
                return _options.PaisPredeterminado;
            }

            var normalizado = pais.Trim().ToUpperInvariant();
            if (normalizado is "PERU" or "PE")
            {
                return "PE";
            }

            if (normalizado.Length == 2)
            {
                return normalizado;
            }

            return _options.PaisPredeterminado;
        }

        private static string ResolverTipoDocumento(string? tipoDocumento)
        {
            var valor = (tipoDocumento ?? string.Empty).Trim().ToUpperInvariant();
            return valor switch
            {
                "PASAPORTE" => "PASAPORTE",
                "CE" => "CE",
                _ => "DNI"
            };
        }
    }
}
