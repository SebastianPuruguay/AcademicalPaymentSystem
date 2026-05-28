using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using CURSO_INTERCULTURALIDAD.Models;
using CURSO_INTERCULTURALIDAD.Services;
using Microsoft.AspNetCore.Mvc;

namespace CURSO_INTERCULTURALIDAD.Controllers
{
    public class InscripcionCISController : Controller
    {
        private const long CursoDiplomadoAnestesiologiaId = 217;
        private const string CursoDiplomadoAnestesiologiaProfesion = "MEDICINA HUMANA";
        private const string CursoDiplomadoAnestesiologiaEspecialidad = "ANESTESIOLOGIA";
        private const string SessionPendingVerificationDniKey = "seguimiento_codigo_dni";
        private const string SessionPendingVerificationCorreoKey = "seguimiento_codigo_correo";
        private const string SessionAuthorizedDniKey = "seguimiento_auth_dni";
        private const string SessionAuthorizedCorreoKey = "seguimiento_auth_correo";

        private readonly IDniService _dniService;
        private readonly ICursoInscripcionRepository _cursoRepository;
        private readonly ICorreoConfirmacionService _correoConfirmacionService;
        private readonly IVerificacionCorreoService _verificacionCorreoService;
        private readonly IPagoIzipayService _pagoIzipayService;
        private readonly ILogger<InscripcionCISController> _logger;

        public InscripcionCISController(
            IDniService dniService,
            ICursoInscripcionRepository cursoRepository,
            ICorreoConfirmacionService correoConfirmacionService,
            IVerificacionCorreoService verificacionCorreoService,
            IPagoIzipayService pagoIzipayService,
            ILogger<InscripcionCISController> logger)
        {
            _dniService = dniService;
            _cursoRepository = cursoRepository;
            _correoConfirmacionService = correoConfirmacionService;
            _verificacionCorreoService = verificacionCorreoService;
            _pagoIzipayService = pagoIzipayService;
            _logger = logger;
        }

        public async Task<IActionResult> Index(long? cursoId)
        {
            try
            {
                var cursos = await _cursoRepository.ObtenerCursosPagadosAsync();
                var cursoSeleccionado = cursoId.HasValue
                    ? cursos.FirstOrDefault(curso => curso.IdActividad == cursoId.Value)
                    : null;

                return View(new PaginaInscripcionCursosViewModel
                {
                    Cursos = cursos,
                    CursoSeleccionado = cursoSeleccionado,
                    CursoSeleccionadoId = cursoSeleccionado?.IdActividad,
                    BannerPredeterminadoUrl = Url.Content("~/Head_Interculturalidad.jpg"),
                    MensajeError = cursoId.HasValue && cursoSeleccionado is null
                        ? "El curso solicitado no esta disponible. Escoge uno de la lista."
                        : null
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "No se pudieron cargar los cursos pagados.");

                return View(new PaginaInscripcionCursosViewModel
                {
                    Cursos = [],
                    CursoSeleccionado = null,
                    CursoSeleccionadoId = null,
                    BannerPredeterminadoUrl = Url.Content("~/Head_Interculturalidad.jpg"),
                    MensajeError = "No se pudo cargar la oferta de cursos. Revise la conexion a la base de datos y vuelva a intentarlo."
                });
            }
        }

        [HttpGet]
        public async Task<IActionResult> Seguimiento(string? dni, string? correo, string? mensajeError, string? mensajeInfo, CancellationToken cancellationToken)
        {
            SeguimientoAlumnoDashboard? dashboard = null;
            var dniNormalizado = NormalizarDocumento(dni);
            var correoNormalizado = NormalizarCorreo(correo);

            if (!string.IsNullOrWhiteSpace(dniNormalizado))
            {
                if (string.Equals(HttpContext.Session.GetString(SessionAuthorizedDniKey), dniNormalizado, StringComparison.OrdinalIgnoreCase))
                {
                    dashboard = await ConstruirEstadoSeguimientoAsync(dniNormalizado, cancellationToken);
                    if (dashboard is null)
                    {
                        mensajeError = "No se encontro una inscripcion asociada a ese documento.";
                    }
                }
                else if (string.IsNullOrWhiteSpace(mensajeError))
                {
                    mensajeInfo ??= "Ingresa tu documento de identidad y el correo registrado para recibir un codigo de verificacion y cursos inscritos, cuotas pagadas y pendientes, y seguimiento.";
                }
            }

            return View(new SeguimientoPortalViewModel
            {
                DniConsultado = dniNormalizado,
                CorreoConsultado = dashboard?.Correo ?? correoNormalizado,
                Dashboard = dashboard,
                MensajeError = mensajeError,
                MensajeInfo = mensajeInfo
            });
        }

        [HttpPost]
        public async Task<IActionResult> Registrar()
        {
            var lectura = await LeerInputRegistroAsync();
            var input = lectura.Input;

            if (input is null)
            {
                _logger.LogWarning(
                    "Registro de inscripcion sin datos validos. Motivo: {Motivo}. ContentType: {ContentType}. ContentLength: {ContentLength}. BodyPreview: {BodyPreview}",
                    lectura.Motivo,
                    Request.ContentType,
                    Request.ContentLength,
                    lectura.BodyPreview);

                return BadRequest(new
                {
                    exito = false,
                    mensaje = $"No se recibieron los datos de inscripcion ({lectura.Motivo}). Actualice la pagina e intente nuevamente."
                });
            }

            CursoPagadoResumen? curso = null;
            decimal? costoFinalCalculado = null;

            if (input.CursoId > 0)
            {
                curso = await _cursoRepository.ObtenerCursoPagadoPorIdAsync(input.CursoId);
                if (curso is not null)
                {
                    var esInsnsbPrevalidacion = string.Equals(input.TipoInstitucion, "INSNSB", StringComparison.OrdinalIgnoreCase);
                    costoFinalCalculado = PagoCursoHelper.CalcularCostoFinal(curso, esInsnsbPrevalidacion);

                    if (costoFinalCalculado <= 0m)
                    {
                        input.NumeroCuotas = 0;
                        ModelState.Remove(nameof(FormularioInscripcionCursoInput.NumeroCuotas));
                    }
                }
            }

            ModelState.Clear();
            TryValidateModel(input);

            if (!ModelState.IsValid)
            {
                return BadRequest(new { exito = false, mensaje = ObtenerPrimerErrorDeModelo() });
            }

            var mensajeValidacion = ValidarFormulario(input, curso, costoFinalCalculado);
            if (!string.IsNullOrWhiteSpace(mensajeValidacion))
            {
                return BadRequest(new { exito = false, mensaje = mensajeValidacion });
            }

            try
            {
                if (curso is null)
                {
                    return BadRequest(new
                    {
                        exito = false,
                        mensaje = $"El curso seleccionado ({input.CursoId}) no esta disponible para inscripcion. Verifique que tenga se_cobra configurado como Si o No y que este visible en formulario."
                    });
                }

                var esInsnsb = string.Equals(input.TipoInstitucion, "INSNSB", StringComparison.OrdinalIgnoreCase);
                var costoFinal = costoFinalCalculado ?? PagoCursoHelper.CalcularCostoFinal(curso, esInsnsb);
                var requierePago = PagoCursoHelper.RequierePago(curso, costoFinal);
                var numeroCuotas = requierePago
                    ? PagoCursoHelper.ResolverNumeroCuotas(curso, costoFinal, input.NumeroCuotas)
                    : 0;

                input.CostoFinal = costoFinal;
                input.NumeroCuotas = numeroCuotas;

                var cronograma = PagoCursoHelper.GenerarCronogramaRigido(
                    curso,
                    costoFinal,
                    numeroCuotas,
                    tokenFactory: () => Convert.ToHexString(RandomNumberGenerator.GetBytes(12)),
                    urlFactory: token => Url.Action("PagoDemo", "InscripcionCIS", new { token }) ?? $"/InscripcionCIS/PagoDemo?token={Uri.EscapeDataString(token)}");

                var resultado = await _cursoRepository.RegistrarInscripcionAsync(input, cronograma);
                if (!resultado.Exito || !resultado.IdInscripcion.HasValue)
                {
                    return BadRequest(new { exito = false, mensaje = resultado.Mensaje });
                }

                await EnviarCorreoConfirmacionRegistroAsync(input, curso, cancellationToken: CancellationToken.None);

                var numeroDocumento = NormalizarDocumento(input.NumeroDocumento);
                var primeraCuotaPendiente = cronograma
                    .OrderBy(cuota => cuota.NumeroCuota)
                    .FirstOrDefault(cuota =>
                        !string.Equals(cuota.Estado, "PAGADO", StringComparison.OrdinalIgnoreCase)
                        && !string.IsNullOrWhiteSpace(cuota.TokenPagoPasarela));
                var pagoRedirectUrl = primeraCuotaPendiente is null
                    ? null
                    : ConstruirUrlPagoInicial(primeraCuotaPendiente.TokenPagoPasarela!);

                return Ok(new
                {
                    exito = true,
                    mensaje = !requierePago
                        ? $"Inscripcion exitosa en {resultado.NombreCurso}."
                        : $"Inscripcion registrada en {resultado.NombreCurso}. Te estamos redirigiendo al pago de la {(numeroCuotas > 1 ? "primera" : "unica")} cuota.",
                    idInscripcion = resultado.IdInscripcion,
                    esCursoGratuito = !requierePago,
                    requierePago,
                    costoFinal,
                    numeroCuotas,
                    estadoPagoGeneral = PagoCursoHelper.CalcularEstadoGeneral(cronograma, costoFinal),
                    pagoRedirectUrl,
                    pagoRedirectLabel = numeroCuotas > 1 ? "Ir ahora a pagar la primera cuota" : "Ir ahora a pagar la cuota",
                    seguimientoUrl = Url.Action("Seguimiento", "InscripcionCIS", new
                    {
                        dni = numeroDocumento,
                        correo = NormalizarCorreo(input.Correo)
                    })
                });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Validacion de negocio al registrar la inscripcion.");
                return BadRequest(new { exito = false, mensaje = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado al registrar la inscripcion.");
                return StatusCode(500, new
                {
                    exito = false,
                    mensaje = "Recuerda que la inscripción es única por DNI/Correo electrónico. Si ya te inscribiste y quieres realizar un pago pendiente, ingresa a Seguimiento con tu DNI y correo registrado para continuar el proceso de pago."
                });
            }
        }

        private async Task<LecturaRegistroInput> LeerInputRegistroAsync()
        {
            if (Request.HasFormContentType)
            {
                var form = Request.Form;
                if (form.Count == 0)
                {
                    return new LecturaRegistroInput(null, "formulario sin campos", string.Empty);
                }

                return new LecturaRegistroInput(ConstruirInputDesdeFormulario(form), "formulario recibido", string.Empty);
            }

            Request.EnableBuffering();
            Request.Body.Position = 0;

            using var reader = new StreamReader(Request.Body, Encoding.UTF8, detectEncodingFromByteOrderMarks: false, leaveOpen: true);
            var rawBody = await reader.ReadToEndAsync();
            Request.Body.Position = 0;

            if (string.IsNullOrWhiteSpace(rawBody))
            {
                return new LecturaRegistroInput(null, "body vacio", string.Empty);
            }

            try
            {
                var input = JsonSerializer.Deserialize<FormularioInscripcionCursoInput>(
                    rawBody,
                    new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                return input is null
                    ? new LecturaRegistroInput(null, "json sin datos", RecortarDiagnostico(rawBody))
                    : new LecturaRegistroInput(input, "json recibido", string.Empty);
            }
            catch (JsonException ex)
            {
                _logger.LogWarning(ex, "No se pudo deserializar el registro de inscripcion.");
                return new LecturaRegistroInput(null, "json invalido", RecortarDiagnostico(rawBody));
            }
        }

        private sealed record LecturaRegistroInput(FormularioInscripcionCursoInput? Input, string Motivo, string BodyPreview);

        private static FormularioInscripcionCursoInput ConstruirInputDesdeFormulario(IFormCollection form)
        {
            return new FormularioInscripcionCursoInput
            {
                CursoId = TryLong(form["CursoId"].ToString(), out var cursoId) ? cursoId : 0,
                TipoDocumento = form["TipoDocumento"].ToString(),
                NumeroDocumento = form["NumeroDocumento"].ToString(),
                Nombres = form["Nombres"].ToString(),
                Apellidos = form["Apellidos"].ToString(),
                Correo = form["Correo"].ToString(),
                Celular = form["Celular"].ToString(),
                CodigoPais = string.IsNullOrWhiteSpace(form["CodigoPais"].ToString()) ? "+51" : form["CodigoPais"].ToString(),
                Pais = string.IsNullOrWhiteSpace(form["Pais"].ToString()) ? "PERU" : form["Pais"].ToString(),
                Region = EmptyToNull(form["Region"].ToString()),
                Profesion = form["Profesion"].ToString(),
                Especialidad = EmptyToNull(form["Especialidad"].ToString()),
                TipoInstitucion = form["TipoInstitucion"].ToString(),
                NombreInstitucion = EmptyToNull(form["NombreInstitucion"].ToString()),
                MedioComunicacion = form["MedioComunicacion"].ToString(),
                OtroMedioComunicacion = EmptyToNull(form["OtroMedioComunicacion"].ToString()),
                CodigoInsnsb = EmptyToNull(form["CodigoInsnsb"].ToString()),
                CondicionLaboralInsnsb = EmptyToNull(form["CondicionLaboralInsnsb"].ToString()),
                CostoFinal = TryDecimal(form["CostoFinal"].ToString(), out var costoFinal) ? costoFinal : null,
                NumeroCuotas = TryInt(form["NumeroCuotas"].ToString(), out var numeroCuotas) ? numeroCuotas : null,
                AceptaTratamientoDatos = ParseBool(form["AceptaTratamientoDatos"].ToString()),
                AsistiraPresencialPrimerDia = string.IsNullOrWhiteSpace(form["AsistiraPresencialPrimerDia"].ToString())
                    ? null
                    : ParseBool(form["AsistiraPresencialPrimerDia"].ToString())
            };
        }

        [HttpPost]
        public async Task<IActionResult> SolicitarCodigoSeguimiento([FromBody] SolicitudCodigoSeguimientoInput input, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { exito = false, mensaje = ObtenerPrimerErrorDeModelo() });
            }

            var dni = NormalizarDocumento(input.DniAlumno);
            var correo = NormalizarCorreo(input.Correo);
            var estado = await ConstruirEstadoSeguimientoAsync(dni, cancellationToken);
            if (estado is null)
            {
                return NotFound(new { exito = false, mensaje = "No se encontro una inscripcion asociada a ese documento." });
            }

            if (!CorreoCoincideConDashboard(estado.Correo, correo))
            {
                return BadRequest(new { exito = false, mensaje = "El correo ingresado no coincide con el correo registrado para ese documento." });
            }

            var nombreCompleto = $"{estado.Nombres} {estado.Apellidos}".Trim();
            var resultado = await _verificacionCorreoService.SolicitarCodigoAsync(correo, nombreCompleto, cancellationToken);
            if (!resultado.Exito)
            {
                return StatusCode(resultado.StatusCode, new { exito = false, mensaje = resultado.Mensaje });
            }

            HttpContext.Session.SetString(SessionPendingVerificationDniKey, dni);
            HttpContext.Session.SetString(SessionPendingVerificationCorreoKey, correo);
            HttpContext.Session.Remove(SessionAuthorizedDniKey);
            HttpContext.Session.Remove(SessionAuthorizedCorreoKey);

            return Ok(new
            {
                exito = true,
                mensaje = $"Te enviamos un codigo de verificacion a {MascarearCorreo(correo)}. Revisa tu bandeja y spam."
            });
        }

        [HttpPost]
        public async Task<IActionResult> ValidarCodigoSeguimiento([FromBody] ValidacionCodigoSeguimientoInput input, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { exito = false, mensaje = ObtenerPrimerErrorDeModelo() });
            }

            var dni = NormalizarDocumento(input.DniAlumno);
            var correo = NormalizarCorreo(input.Correo);
            var codigo = (input.Codigo ?? string.Empty).Trim();
            var dniPendiente = HttpContext.Session.GetString(SessionPendingVerificationDniKey);
            var correoPendiente = HttpContext.Session.GetString(SessionPendingVerificationCorreoKey);

            if (string.IsNullOrWhiteSpace(dniPendiente) || string.IsNullOrWhiteSpace(correoPendiente))
            {
                return BadRequest(new { exito = false, mensaje = "No hay una verificacion activa. Solicita un codigo nuevo." });
            }

            if (!string.Equals(dniPendiente, dni, StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest(new { exito = false, mensaje = "El codigo solicitado corresponde a otro documento." });
            }

            if (!string.Equals(correoPendiente, correo, StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest(new { exito = false, mensaje = "Debes validar el mismo correo al que se envio el codigo." });
            }

            var estado = await ConstruirEstadoSeguimientoAsync(dni, cancellationToken);
            if (estado is null)
            {
                return NotFound(new { exito = false, mensaje = "No se encontro una inscripcion asociada a ese documento." });
            }

            if (!CorreoCoincideConDashboard(estado.Correo, correo))
            {
                return BadRequest(new { exito = false, mensaje = "El correo ingresado no coincide con el correo registrado para ese documento." });
            }

            var resultado = await _verificacionCorreoService.ValidarCodigoAsync(correo, codigo, cancellationToken);
            if (!resultado.Exito)
            {
                return StatusCode(resultado.StatusCode, new { exito = false, mensaje = resultado.Mensaje });
            }

            HttpContext.Session.SetString(SessionAuthorizedDniKey, dni);
            HttpContext.Session.SetString(SessionAuthorizedCorreoKey, correo);
            HttpContext.Session.Remove(SessionPendingVerificationDniKey);
            HttpContext.Session.Remove(SessionPendingVerificationCorreoKey);

            return Ok(new
            {
                exito = true,
                redirectUrl = Url.Action("Seguimiento", "InscripcionCIS", new { dni, correo })
            });
        }

        [HttpGet]
        public async Task<IActionResult> IniciarPagoIzipay(string token, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                TempData["PagoDemoError"] = "No se recibio un token de pago valido.";
                return RedirectToAction(nameof(Seguimiento));
            }

            var pago = await _cursoRepository.ObtenerPagoDemoAsync(token);
            if (pago is null)
            {
                TempData["PagoDemoError"] = "No se encontro una cuota asociada a ese token.";
                return RedirectToAction(nameof(Seguimiento));
            }

            if (!pago.PuedePagarAhora)
            {
                TempData["PagoDemoError"] = pago.BloqueoPagoMensaje ?? "La cuota seleccionada aún no puede pagarse.";
                return Redirect(pago.VolverUrl ?? Url.Action(nameof(Seguimiento), new { dni = pago.NumeroDocumento }) ?? "/InscripcionCIS/Seguimiento");
            }

            if (pago.IdPagoIzipay.HasValue)
            {
                var estadoExistente = await ConsultarYGuardarEstadoIzipayAsync(pago.IdPagoIzipay.Value, cancellationToken);
                if (EstaPagoConfirmado(estadoExistente))
                {
                    return RedirectToAction(nameof(Seguimiento), new
                    {
                        dni = pago.NumeroDocumento,
                        mensajeInfo = "El pago ya fue confirmado por IziPay. Actualizamos tu cuota."
                    });
                }

                if (!string.IsNullOrWhiteSpace(pago.UrlPagoIzipay))
                {
                    return Redirect(pago.UrlPagoIzipay);
                }
            }

            var reserva = await _cursoRepository.ReservarCreacionPagoIzipayAsync(token);
            pago = reserva.Pago ?? pago;

            if (pago.IdPagoIzipay.HasValue && !string.IsNullOrWhiteSpace(pago.UrlPagoIzipay))
            {
                return Redirect(pago.UrlPagoIzipay);
            }

            if (reserva.EnProceso)
            {
                TempData["PagoDemoError"] = "Estamos generando tu enlace de pago. Espera unos segundos y vuelve a intentarlo para evitar crear una boleta duplicada.";
                return RedirectToAction(nameof(PagoDemo), new { token, pagoConfirmado = false });
            }

            if (!reserva.ReservadoParaCrear)
            {
                TempData["PagoDemoError"] = "No se pudo reservar la cuota para generar el enlace de pago. Actualiza la pagina e intenta nuevamente.";
                return RedirectToAction(nameof(PagoDemo), new { token, pagoConfirmado = false });
            }

            var resultado = await _pagoIzipayService.CrearPagoAsync(pago, cancellationToken);
            if (resultado.Exitoso && !string.IsNullOrWhiteSpace(resultado.UrlPago))
            {
                await _cursoRepository.GuardarPagoIzipayAsync(token, resultado);
                return Redirect(resultado.UrlPago);
            }

            TempData["PagoDemoError"] = resultado.Mensaje;
            return RedirectToAction(nameof(PagoDemo), new { token, pagoConfirmado = false });
        }

        [HttpGet]
        public async Task<IActionResult> PagoDemo(string token, bool pagoConfirmado = false)
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                return View(new PagoDemoViewModel { MensajeError = "No se recibio un token de pago." });
            }

            var modelo = await _cursoRepository.ObtenerPagoDemoAsync(token);
            if (modelo is null)
            {
                return View(new PagoDemoViewModel { MensajeError = "No se encontro una cuota asociada a ese token." });
            }

            return View(new PagoDemoViewModel
            {
                MensajeError = TempData["PagoDemoError"] as string ?? modelo.MensajeError,
                PagoConfirmado = pagoConfirmado,
                IdInscripcion = modelo.IdInscripcion,
                CursoId = modelo.CursoId,
                NombreCurso = modelo.NombreCurso,
                Nombres = modelo.Nombres,
                Apellidos = modelo.Apellidos,
                NumeroDocumento = modelo.NumeroDocumento,
                TipoDocumento = modelo.TipoDocumento,
                Correo = modelo.Correo,
                Celular = modelo.Celular,
                Pais = modelo.Pais,
                Region = modelo.Region,
                CodigoPais = modelo.CodigoPais,
                CodigoCentroCosto = modelo.CodigoCentroCosto,
                NumeroCuotasTotal = modelo.NumeroCuotasTotal,
                NumeroCuota = modelo.NumeroCuota,
                Monto = modelo.Monto,
                FechaVencimiento = modelo.FechaVencimiento,
                Estado = modelo.Estado,
                FechaPagoReal = modelo.FechaPagoReal,
                Token = modelo.Token,
                IdPagoIzipay = modelo.IdPagoIzipay,
                UrlPagoIzipay = modelo.UrlPagoIzipay,
                EstadoIzipay = modelo.EstadoIzipay,
                CodigoAutorizacionIzipay = modelo.CodigoAutorizacionIzipay,
                NumeroComprobanteIzipay = modelo.NumeroComprobanteIzipay,
                UrlComprobantePdfIzipay = modelo.UrlComprobantePdfIzipay,
                VolverUrl = modelo.VolverUrl,
                PuedePagarAhora = modelo.PuedePagarAhora,
                BloqueoPagoMensaje = modelo.BloqueoPagoMensaje
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmarPagoDemo(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                return RedirectToAction(nameof(PagoDemo), new { token, pagoConfirmado = false });
            }

            try
            {
                var estadoActualizado = await _cursoRepository.MarcarCuotaComoPagadaAsync(token);
                if (estadoActualizado is not null)
                {
                    HttpContext.Session.SetString(SessionAuthorizedDniKey, estadoActualizado.NumeroDocumento);
                    return RedirectToAction(nameof(PagoDemo), new { token, pagoConfirmado = true });
                }
            }
            catch (InvalidOperationException ex)
            {
                TempData["PagoDemoError"] = ex.Message;
            }

            return RedirectToAction(nameof(PagoDemo), new { token, pagoConfirmado = false });
        }

        [HttpPost]
        public async Task<IActionResult> ConsultarDni([FromBody] ConsultaDniRequest request)
        {
            try
            {
                _logger.LogInformation("Iniciando consulta de DNI: {Dni}", request?.Dni);

                if (string.IsNullOrWhiteSpace(request?.Dni) || request.Dni.Length != 8)
                {
                    return BadRequest(new { success = false, message = "El DNI debe tener 8 digitos." });
                }

                var resultado = await _dniService.ConsultarDniAsync(request.Dni);

                return Ok(new
                {
                    success = true,
                    nombres = resultado.nombres,
                    apellidos = resultado.apellidos
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error consultando DNI: {Dni}", request?.Dni);
                return StatusCode(500, new
                {
                    success = false,
                    message = "Error al consultar el DNI. Puede completar los datos manualmente."
                });
            }
        }

        private async Task<SeguimientoAlumnoDashboard?> ConstruirEstadoSeguimientoAsync(string numeroDocumento, CancellationToken cancellationToken = default)
        {
            var documento = NormalizarDocumento(numeroDocumento);
            if (string.IsNullOrWhiteSpace(documento))
            {
                return null;
            }

            var dashboard = await _cursoRepository.ObtenerSeguimientoAlumnoAsync(documento);
            if (dashboard is null)
            {
                return null;
            }

            if (await SincronizarPagosPendientesIzipayAsync(dashboard, cancellationToken))
            {
                dashboard = await _cursoRepository.ObtenerSeguimientoAlumnoAsync(documento);
                if (dashboard is null)
                {
                    return null;
                }
            }

            foreach (var cursoPago in dashboard.CursosPagados)
            {
                foreach (var cuota in cursoPago.Cuotas)
                {
                    if (cuota.PuedePagarAhora && !string.Equals(cuota.Estado, "PAGADO", StringComparison.OrdinalIgnoreCase) && !string.IsNullOrWhiteSpace(cuota.TokenPagoPasarela))
                    {
                        cuota.UrlPagoPasarela = _pagoIzipayService.EstaConfigurado
                            ? Url.Action("IniciarPagoIzipay", "InscripcionCIS", new { token = cuota.TokenPagoPasarela })
                                ?? $"/InscripcionCIS/IniciarPagoIzipay?token={Uri.EscapeDataString(cuota.TokenPagoPasarela)}"
                            : Url.Action("PagoDemo", "InscripcionCIS", new { token = cuota.TokenPagoPasarela })
                                ?? $"/InscripcionCIS/PagoDemo?token={Uri.EscapeDataString(cuota.TokenPagoPasarela)}";
                    }
                    else
                    {
                        cuota.UrlPagoPasarela = null;
                    }
                }
            }

            return dashboard;
        }

        private async Task<bool> SincronizarPagosPendientesIzipayAsync(SeguimientoAlumnoDashboard dashboard, CancellationToken cancellationToken)
        {
            if (!_pagoIzipayService.EstaConfigurado)
            {
                return false;
            }

            var idsPendientes = dashboard.CursosPagados
                .SelectMany(curso => curso.Cuotas)
                .Where(cuota => cuota.IdPagoIzipay.HasValue
                    && (!string.Equals(cuota.Estado, "PAGADO", StringComparison.OrdinalIgnoreCase)
                        || string.IsNullOrWhiteSpace(cuota.UrlComprobantePdfIzipay)
                        || string.IsNullOrWhiteSpace(cuota.NumeroComprobanteIzipay)))
                .Select(cuota => cuota.IdPagoIzipay!.Value)
                .Distinct()
                .ToList();

            var huboPagosConfirmados = false;
            foreach (var idPagoIzipay in idsPendientes)
            {
                var estado = await ConsultarYGuardarEstadoIzipayAsync(idPagoIzipay, cancellationToken);
                huboPagosConfirmados = huboPagosConfirmados || HuboActualizacionVisibleIzipay(estado);
            }

            return huboPagosConfirmados;
        }

        private async Task<RespuestaEstadoPagoIzipay?> ConsultarYGuardarEstadoIzipayAsync(long idPagoIzipay, CancellationToken cancellationToken)
        {
            var estado = await _pagoIzipayService.ConsultarEstadoAsync(idPagoIzipay, cancellationToken);
            if (!estado.Exitoso)
            {
                _logger.LogWarning("IziPay no pudo consultar el estado del pago {IdPagoIzipay}: {Mensaje}", idPagoIzipay, estado.Mensaje);
                return estado;
            }

            await _cursoRepository.ActualizarEstadoPagoIzipayAsync(estado);

            return estado;
        }

        private static bool EstaPagoConfirmado(RespuestaEstadoPagoIzipay? estado)
        {
            return estado is not null
                && (estado.YaPago || string.Equals(estado.Estado, "PAGADO", StringComparison.OrdinalIgnoreCase));
        }

        private static bool HuboActualizacionVisibleIzipay(RespuestaEstadoPagoIzipay? estado)
        {
            return EstaPagoConfirmado(estado)
                || string.Equals(estado?.Estado, "CON COMPROBANTE", StringComparison.OrdinalIgnoreCase)
                || !string.IsNullOrWhiteSpace(estado?.NumeroComprobante)
                || !string.IsNullOrWhiteSpace(estado?.UrlComprobantePdf);
        }

        private async Task EnviarCorreoConfirmacionRegistroAsync(
            FormularioInscripcionCursoInput input,
            CursoPagadoResumen curso,
            CancellationToken cancellationToken)
        {
            try
            {
                var resultado = await _correoConfirmacionService.EnviarAsync(
                    input.Correo,
                    input.Nombres,
                    input.Apellidos,
                    curso.IdActividad,
                    curso.NombreActividad,
                    cancellationToken);

                if (!resultado.Exito)
                {
                    _logger.LogWarning("No se pudo enviar el correo de confirmacion del registro {CursoId}: {Mensaje}", curso.IdActividad, resultado.Mensaje);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error enviando correo de confirmacion del registro {CursoId}.", curso.IdActividad);
            }
        }

        private string ConstruirUrlPagoInicial(string tokenPagoPasarela)
        {
            return _pagoIzipayService.EstaConfigurado
                ? Url.Action("IniciarPagoIzipay", "InscripcionCIS", new { token = tokenPagoPasarela })
                    ?? $"/InscripcionCIS/IniciarPagoIzipay?token={Uri.EscapeDataString(tokenPagoPasarela)}"
                : Url.Action("PagoDemo", "InscripcionCIS", new { token = tokenPagoPasarela })
                    ?? $"/InscripcionCIS/PagoDemo?token={Uri.EscapeDataString(tokenPagoPasarela)}";
        }

        private string? ValidarFormulario(FormularioInscripcionCursoInput input, CursoPagadoResumen? curso, decimal? costoFinalCalculado)
        {
            if (!input.AceptaTratamientoDatos)
            {
                return "Debe aceptar el tratamiento de datos para continuar.";
            }

            if (string.Equals(input.TipoDocumento, "DNI", StringComparison.OrdinalIgnoreCase) &&
                input.NumeroDocumento.Trim().Length != 8)
            {
                return "El DNI debe tener exactamente 8 digitos.";
            }

            if (curso?.SolicitaConfirmacionPresencialPrimerDia == true && !input.AsistiraPresencialPrimerDia.HasValue)
            {
                return "Debe confirmar si contara con asistencia presencial al primer dia de clases.";
            }

            var esInsnsb = string.Equals(input.TipoInstitucion, "INSNSB", StringComparison.OrdinalIgnoreCase);
            var requiereCodigoInsnsb = esInsnsb &&
                curso is not null &&
                PagoCursoHelper.RequiereCodigoInsnsb(
                    curso,
                    costoFinalCalculado ?? PagoCursoHelper.CalcularCostoFinal(curso, esInsnsb: true));

            if (requiereCodigoInsnsb &&
                string.IsNullOrWhiteSpace(input.CodigoInsnsb))
            {
                return "Para personal INSNSB debe ingresar un codigo unico valido.";
            }

            if (esInsnsb &&
                string.IsNullOrWhiteSpace(input.CondicionLaboralInsnsb))
            {
                return "Para personal INSNSB debe indicar si es Locador, Nombrado o CAS.";
            }

            if (!esInsnsb &&
                string.IsNullOrWhiteSpace(input.NombreInstitucion))
            {
                return "Debe indicar el nombre de la institucion de procedencia.";
            }

            if (string.Equals(input.MedioComunicacion, "OTRO", StringComparison.OrdinalIgnoreCase) &&
                string.IsNullOrWhiteSpace(input.OtroMedioComunicacion))
            {
                return "Debe detallar el otro medio por el que se entero del curso.";
            }

            if (input.CursoId == CursoDiplomadoAnestesiologiaId)
            {
                var profesion = NormalizarSeleccion(input.Profesion);
                var especialidad = NormalizarSeleccion(input.Especialidad);
                if (!string.Equals(profesion, CursoDiplomadoAnestesiologiaProfesion, StringComparison.OrdinalIgnoreCase) ||
                    !string.Equals(especialidad, CursoDiplomadoAnestesiologiaEspecialidad, StringComparison.OrdinalIgnoreCase))
                {
                    return "Para este diplomado solo se permite la profesion MEDICINA HUMANA y la especialidad ANESTESIOLOGIA.";
                }
            }

            return null;
        }

        private string ObtenerPrimerErrorDeModelo()
        {
            return ModelState.Values
                .SelectMany(value => value.Errors)
                .Select(error => string.IsNullOrWhiteSpace(error.ErrorMessage) ? "Revise los datos ingresados." : error.ErrorMessage)
                .FirstOrDefault() ?? "Revise los datos ingresados.";
        }

        private static string NormalizarDocumento(string? numeroDocumento)
        {
            return (numeroDocumento ?? string.Empty).Trim().ToUpperInvariant();
        }

        private static string NormalizarCorreo(string? correo)
        {
            return (correo ?? string.Empty).Trim().ToLowerInvariant();
        }

        private static string? EmptyToNull(string? value)
        {
            var normalized = value?.Trim();
            return string.IsNullOrWhiteSpace(normalized) ? null : normalized;
        }

        private static bool ParseBool(string? value)
        {
            return string.Equals(value, "true", StringComparison.OrdinalIgnoreCase)
                || string.Equals(value, "on", StringComparison.OrdinalIgnoreCase)
                || string.Equals(value, "1", StringComparison.OrdinalIgnoreCase)
                || string.Equals(value, "Si", StringComparison.OrdinalIgnoreCase);
        }

        private static bool TryLong(string? value, out long result)
        {
            return long.TryParse(value, out result);
        }

        private static bool TryInt(string? value, out int result)
        {
            return int.TryParse(value, out result);
        }

        private static bool TryDecimal(string? value, out decimal result)
        {
            return decimal.TryParse(value, System.Globalization.NumberStyles.Number, System.Globalization.CultureInfo.InvariantCulture, out result)
                || decimal.TryParse(value, out result);
        }

        private static string RecortarDiagnostico(string? value)
        {
            var normalized = value?.Replace("\r", " ", StringComparison.Ordinal).Replace("\n", " ", StringComparison.Ordinal).Trim();
            if (string.IsNullOrWhiteSpace(normalized))
            {
                return string.Empty;
            }

            return normalized.Length <= 500 ? normalized : normalized[..500];
        }

        private static string NormalizarSeleccion(string? value)
        {
            var normalized = (value ?? string.Empty).Trim().ToUpperInvariant().Normalize(NormalizationForm.FormD);
            var builder = new StringBuilder(normalized.Length);

            foreach (var character in normalized)
            {
                if (CharUnicodeInfo.GetUnicodeCategory(character) != UnicodeCategory.NonSpacingMark)
                {
                    builder.Append(character);
                }
            }

            return builder.ToString().Normalize(NormalizationForm.FormC);
        }

        private static bool CorreoCoincideConDashboard(string? correoRegistrado, string correoIngresado)
        {
            return string.Equals(NormalizarCorreo(correoRegistrado), NormalizarCorreo(correoIngresado), StringComparison.OrdinalIgnoreCase);
        }

        private static string MascarearCorreo(string correo)
        {
            var correoNormalizado = NormalizarCorreo(correo);
            var separatorIndex = correoNormalizado.IndexOf('@');
            if (separatorIndex <= 1)
            {
                return correoNormalizado;
            }

            var usuario = correoNormalizado[..separatorIndex];
            var dominio = correoNormalizado[separatorIndex..];
            var visible = usuario.Length <= 2 ? usuario[..1] : usuario[..2];
            return $"{visible}***{dominio}";
        }
    }
}
