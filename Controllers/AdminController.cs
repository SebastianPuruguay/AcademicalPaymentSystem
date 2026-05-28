using System.Globalization;
using System.Net;
using System.Text;
using CURSO_INTERCULTURALIDAD.Models;
using CURSO_INTERCULTURALIDAD.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace CURSO_INTERCULTURALIDAD.Controllers
{
    public class AdminController : Controller
    {
        private const string AdminSessionKey = "AdminPanel.Authenticated";
        private readonly ICursoInscripcionRepository _cursoRepository;
        private readonly IPagoIzipayService _pagoIzipayService;
        private readonly AdminPanelOptions _adminOptions;
        private readonly ILogger<AdminController> _logger;

        public AdminController(
            ICursoInscripcionRepository cursoRepository,
            IPagoIzipayService pagoIzipayService,
            IOptions<AdminPanelOptions> adminOptions,
            ILogger<AdminController> logger)
        {
            _cursoRepository = cursoRepository;
            _pagoIzipayService = pagoIzipayService;
            _adminOptions = adminOptions.Value;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Login()
        {
            if (EstaAutenticado())
            {
                return RedirectToAction(nameof(Index));
            }

            ViewData["Title"] = "Panel Admin";
            return View(new LoginAdminViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Login(LoginAdminViewModel model)
        {
            ViewData["Title"] = "Panel Admin";

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            if (string.IsNullOrWhiteSpace(_adminOptions.Password))
            {
                model.MensajeError = "Configure `AdminPanel.Password` en appsettings antes de usar el panel.";
                return View(model);
            }

            var usuarioValido = string.Equals(model.Usuario?.Trim(), _adminOptions.Username, StringComparison.Ordinal);
            var claveValida = string.Equals(model.Clave, _adminOptions.Password, StringComparison.Ordinal);

            if (!usuarioValido || !claveValida)
            {
                model.MensajeError = "Credenciales invalidas.";
                return View(model);
            }

            HttpContext.Session.SetString(AdminSessionKey, "1");
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Logout()
        {
            HttpContext.Session.Remove(AdminSessionKey);
            return RedirectToAction(nameof(Login));
        }

        [HttpGet]
        public async Task<IActionResult> Index(long? cursoId)
        {
            if (!EstaAutenticado())
            {
                return RedirectToAction(nameof(Login));
            }

            ViewData["Title"] = "Panel Admin";

            try
            {
                return View(await ConstruirDashboardAsync(cursoId));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cargando el panel admin.");
                return View(new PanelAdminCodigosViewModel
                {
                    MensajeError = "No se pudo cargar el panel. Verifique la conexion SQL y que exista la tabla de codigos."
                });
            }
        }

        [HttpGet]
        public async Task<IActionResult> ReporteInscritos(long? cursoId)
        {
            if (!EstaAutenticado())
            {
                return RedirectToAction(nameof(Login));
            }

            ViewData["Title"] = "Reporte de inscritos";

            try
            {
                return View(await ConstruirReporteInscritosAsync(cursoId));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cargando el reporte de inscritos.");
                return View(new ReporteInscritosViewModel
                {
                    MensajeError = "No se pudo cargar el reporte. Verifique la conexion SQL y que existan las tablas de inscripciones y deudas."
                });
            }
        }

        [HttpGet]
        public async Task<IActionResult> ReportePagantes(long? cursoId, string? estadoPago, string? busqueda)
        {
            if (!EstaAutenticado())
            {
                return RedirectToAction(nameof(Login));
            }

            ViewData["Title"] = "Reporte de pagantes";

            try
            {
                return View(await ConstruirReportePagantesAsync(cursoId, estadoPago, busqueda));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cargando el reporte de pagantes.");
                return View(new ReportePagantesViewModel
                {
                    MensajeError = "No se pudo cargar el reporte de pagantes. Verifique la conexion SQL y vuelva a intentarlo."
                });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SincronizarPagosIzipay(long cursoId)
        {
            if (!EstaAutenticado())
            {
                return RedirectToAction(nameof(Login));
            }

            try
            {
                var seguimiento = await _cursoRepository.ObtenerSeguimientoPagosAdminAsync(cursoId);
                var resultado = await SincronizarPagosPendientesIzipayConResumenAsync(seguimiento);
                var dashboard = await ConstruirDashboardAsync(cursoId);

                if (resultado.Consultados == 0)
                {
                    return View("Index", CopiarDashboard(
                        dashboard,
                        null,
                        "No se encontraron boletas IziPay pendientes de sincronizar para este curso."));
                }

                var mensaje = $"Sincronizacion IziPay completada: {resultado.Consultados} boleta(s) consultada(s), {resultado.PagosDetectados} pago(s) detectado(s)";
                if (resultado.Fallidos > 0)
                {
                    mensaje += $", {resultado.Fallidos} sin respuesta correcta de IziPay";
                }

                mensaje += ".";

                return View("Index", CopiarDashboard(dashboard, null, mensaje));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sincronizando pagos IziPay para el curso {CursoId}.", cursoId);
                var dashboard = await ConstruirDashboardAsync(cursoId);
                return View("Index", CopiarDashboard(
                    dashboard,
                    "No se pudo sincronizar con IziPay. Verifique la conectividad con el servicio y vuelva a intentarlo.",
                    null));
            }
        }

        [HttpGet]
        public async Task<IActionResult> EditarCurso(long cursoId)
        {
            if (!EstaAutenticado())
            {
                return RedirectToAction(nameof(Login));
            }

            ViewData["Title"] = "Editar curso";

            try
            {
                return View(await ConstruirVistaConfiguracionCursoAsync(cursoId));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cargando la configuracion publica del curso {CursoId}.", cursoId);
                return View(new AdminCursoConfiguracionViewModel
                {
                    MensajeError = "No se pudo cargar la configuracion del curso. Verifique la conexion SQL y vuelva a intentarlo."
                });
            }
        }

        [HttpGet]
        public async Task<IActionResult> DescargarReporteInscritosExcel(long? cursoId)
        {
            if (!EstaAutenticado())
            {
                return RedirectToAction(nameof(Login));
            }

            var reporte = await ConstruirReporteInscritosAsync(cursoId);
            if (reporte.Detalle is null)
            {
                return RedirectToAction(nameof(ReporteInscritos), new { cursoId });
            }

            var contenido = ConstruirReporteHtmlExportable(reporte.Detalle, "Excel");
            var bytes = Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(contenido)).ToArray();
            return File(bytes, "application/vnd.ms-excel", ConstruirNombreArchivo(reporte.Detalle, "xls"));
        }

        [HttpGet]
        public async Task<IActionResult> DescargarReporteInscritosWord(long? cursoId)
        {
            if (!EstaAutenticado())
            {
                return RedirectToAction(nameof(Login));
            }

            var reporte = await ConstruirReporteInscritosAsync(cursoId);
            if (reporte.Detalle is null)
            {
                return RedirectToAction(nameof(ReporteInscritos), new { cursoId });
            }

            var contenido = ConstruirReporteHtmlExportable(reporte.Detalle, "Word");
            var bytes = Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(contenido)).ToArray();
            return File(bytes, "application/msword", ConstruirNombreArchivo(reporte.Detalle, "doc"));
        }

        [HttpGet]
        public async Task<IActionResult> DescargarReporteInscritosGeneralExcel(long? cursoId)
        {
            if (!EstaAutenticado())
            {
                return RedirectToAction(nameof(Login));
            }

            var reporte = await ConstruirReporteInscritosAsync(cursoId);
            if (reporte.Detalle is null || !reporte.CursoSeleccionadoId.HasValue)
            {
                return RedirectToAction(nameof(ReporteInscritos), new { cursoId });
            }

            var items = await _cursoRepository.ObtenerReporteInscritosGeneralAsync(reporte.CursoSeleccionadoId.Value);
            var contenido = ConstruirReporteInscritosGeneralHtmlExportable(reporte.Detalle, items);
            var bytes = Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(contenido)).ToArray();
            return File(bytes, "application/vnd.ms-excel", ConstruirNombreArchivoGeneralInscritos(reporte.Detalle, "xls"));
        }

        [HttpGet]
        public async Task<IActionResult> DescargarReportePagantesExcel(long? cursoId, string? estadoPago, string? busqueda)
        {
            if (!EstaAutenticado())
            {
                return RedirectToAction(nameof(Login));
            }

            var reporte = await ConstruirReportePagantesAsync(cursoId, estadoPago, busqueda);
            if (reporte.Detalle is null)
            {
                return RedirectToAction(nameof(ReportePagantes), new { cursoId, estadoPago, busqueda });
            }

            var contenido = ConstruirReportePagantesHtmlExportable(reporte.Detalle, "Excel");
            var bytes = Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(contenido)).ToArray();
            return File(bytes, "application/vnd.ms-excel", ConstruirNombreArchivoPagantes(reporte.Detalle, "xls"));
        }

        [HttpGet]
        public async Task<IActionResult> DescargarReportePagantesWord(long? cursoId, string? estadoPago, string? busqueda)
        {
            if (!EstaAutenticado())
            {
                return RedirectToAction(nameof(Login));
            }

            var reporte = await ConstruirReportePagantesAsync(cursoId, estadoPago, busqueda);
            if (reporte.Detalle is null)
            {
                return RedirectToAction(nameof(ReportePagantes), new { cursoId, estadoPago, busqueda });
            }

            var contenido = ConstruirReportePagantesHtmlExportable(reporte.Detalle, "Word");
            var bytes = Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(contenido)).ToArray();
            return File(bytes, "application/msword", ConstruirNombreArchivoPagantes(reporte.Detalle, "doc"));
        }

        [HttpGet]
        public async Task<IActionResult> ReporteInscritosPdf(long? cursoId)
        {
            if (!EstaAutenticado())
            {
                return RedirectToAction(nameof(Login));
            }

            ViewData["Title"] = "Reporte de inscritos PDF";

            try
            {
                return View("ReporteInscritosImprimir", await ConstruirReporteInscritosAsync(cursoId));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cargando la version imprimible del reporte de inscritos.");
                return View("ReporteInscritosImprimir", new ReporteInscritosViewModel
                {
                    MensajeError = "No se pudo cargar la version imprimible del reporte."
                });
            }
        }

        [HttpGet]
        public async Task<IActionResult> ReportePagantesPdf(long? cursoId, string? estadoPago, string? busqueda)
        {
            if (!EstaAutenticado())
            {
                return RedirectToAction(nameof(Login));
            }

            ViewData["Title"] = "Reporte de pagantes PDF";

            try
            {
                return View("ReportePagantesImprimir", await ConstruirReportePagantesAsync(cursoId, estadoPago, busqueda));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cargando la version imprimible del reporte de pagantes.");
                return View("ReportePagantesImprimir", new ReportePagantesViewModel
                {
                    MensajeError = "No se pudo cargar la version imprimible del reporte de pagantes."
                });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GenerarCodigos([Bind(Prefix = "Formulario")] GeneracionCodigosInput input)
        {
            if (!EstaAutenticado())
            {
                return RedirectToAction(nameof(Login));
            }

            ViewData["Title"] = "Panel Admin";

            if (!ModelState.IsValid)
            {
                var vistaConError = await ConstruirDashboardAsync(input.CursoId);
                return View("Index", CopiarDashboard(
                    vistaConError,
                    ModelState.Values
                        .SelectMany(value => value.Errors)
                        .Select(error => error.ErrorMessage)
                        .FirstOrDefault() ?? "Revise los datos ingresados.",
                    null,
                    input));
            }

            try
            {
                var codigosGenerados = await _cursoRepository.GenerarCodigosAsync(input);
                var dashboard = await ConstruirDashboardAsync(input.CursoId);

                return View("Index", CopiarDashboard(
                    dashboard,
                    null,
                    $"Se genero el codigo {codigosGenerados.FirstOrDefault()} para el documento {input.DniAsignado}.",
                    input,
                    codigosGenerados));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generando codigos para el curso {CursoId}", input.CursoId);
                var dashboard = await ConstruirDashboardAsync(input.CursoId);

                return View("Index", CopiarDashboard(dashboard, ex.Message, null, input));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EliminarCodigo(long cursoId, long codigoId)
        {
            if (!EstaAutenticado())
            {
                return RedirectToAction(nameof(Login));
            }

            ViewData["Title"] = "Panel Admin";

            try
            {
                await _cursoRepository.EliminarCodigoInsnsbAsync(cursoId, codigoId);
                var dashboard = await ConstruirDashboardAsync(cursoId);
                return View("Index", CopiarDashboard(
                    dashboard,
                    null,
                    "El codigo INSNSB se elimino correctamente."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error eliminando el codigo {CodigoId} del curso {CursoId}.", codigoId, cursoId);
                var dashboard = await ConstruirDashboardAsync(cursoId);
                return View("Index", CopiarDashboard(
                    dashboard,
                    ex.Message,
                    null));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EliminarInscripcion(long cursoId, long idInscripcion)
        {
            if (!EstaAutenticado())
            {
                return RedirectToAction(nameof(Login));
            }

            ViewData["Title"] = "Panel Admin";

            try
            {
                await _cursoRepository.EliminarInscripcionAsync(cursoId, idInscripcion);
                var dashboard = await ConstruirDashboardAsync(cursoId);
                return View("Index", CopiarDashboard(
                    dashboard,
                    null,
                    "La inscripcion y su plan de cuotas se eliminaron correctamente."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error eliminando la inscripcion {IdInscripcion} del curso {CursoId}.", idInscripcion, cursoId);
                var dashboard = await ConstruirDashboardAsync(cursoId);
                return View("Index", CopiarDashboard(
                    dashboard,
                    ex.Message,
                    null));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> VistaPreviaCurso([Bind(Prefix = "ConfiguracionCurso")] CursoConfiguracionAdminInput input)
        {
            if (!EstaAutenticado())
            {
                return RedirectToAction(nameof(Login));
            }

            ViewData["Title"] = "Editar curso";

            var vistaPrevia = ConstruirCursoPreview(input);
            if (!ModelState.IsValid)
            {
                return View("EditarCurso", await ConstruirVistaConfiguracionCursoAsync(
                    input.CursoId,
                    input,
                    vistaPrevia,
                    ModelState.Values
                        .SelectMany(value => value.Errors)
                        .Select(error => error.ErrorMessage)
                        .FirstOrDefault() ?? "Revise los datos del curso antes de generar la vista previa.",
                    null));
            }

            return View("EditarCurso", await ConstruirVistaConfiguracionCursoAsync(
                input.CursoId,
                input,
                vistaPrevia,
                null,
                "Vista previa actualizada. Aún no se guardaron cambios en la base de datos."));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GuardarConfiguracionCurso([Bind(Prefix = "ConfiguracionCurso")] CursoConfiguracionAdminInput input)
        {
            if (!EstaAutenticado())
            {
                return RedirectToAction(nameof(Login));
            }

            ViewData["Title"] = "Editar curso";

            var vistaPrevia = ConstruirCursoPreview(input);
            if (!ModelState.IsValid)
            {
                return View("EditarCurso", await ConstruirVistaConfiguracionCursoAsync(
                    input.CursoId,
                    input,
                    vistaPrevia,
                    ModelState.Values
                        .SelectMany(value => value.Errors)
                        .Select(error => error.ErrorMessage)
                        .FirstOrDefault() ?? "Revise los datos del curso antes de guardar.",
                    null));
            }

            try
            {
                await _cursoRepository.GuardarConfiguracionCursoAsync(input);
                return View("EditarCurso", await ConstruirVistaConfiguracionCursoAsync(
                    input.CursoId,
                    mensajeExito: "La configuracion publica del curso se actualizo correctamente."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error guardando la configuracion publica del curso {CursoId}", input.CursoId);
                return View("EditarCurso", await ConstruirVistaConfiguracionCursoAsync(
                    input.CursoId,
                    input,
                    vistaPrevia,
                    ex.Message,
                    null));
            }
        }

        private async Task<ReporteInscritosViewModel> ConstruirReporteInscritosAsync(long? cursoId)
        {
            var cursos = await _cursoRepository.ObtenerCursosAdminAsync();
            var cursoSeleccionadoId = cursoId ?? cursos.FirstOrDefault()?.IdActividad;
            if (!cursoSeleccionadoId.HasValue)
            {
                return new ReporteInscritosViewModel
                {
                    Cursos = cursos,
                    MensajeError = "No hay cursos disponibles para reportar."
                };
            }

            var detalle = await _cursoRepository.ObtenerReporteInscritosCursoAsync(cursoSeleccionadoId.Value);
            detalle.CursoTieneCobro = cursos
                .FirstOrDefault(curso => curso.IdActividad == cursoSeleccionadoId.Value)
                ?.TieneLogicaCobro == true;
            return new ReporteInscritosViewModel
            {
                Cursos = cursos,
                CursoSeleccionadoId = cursoSeleccionadoId,
                Detalle = detalle
            };
        }

        private async Task<ReportePagantesViewModel> ConstruirReportePagantesAsync(long? cursoId, string? estadoPago = null, string? busqueda = null)
        {
            var cursos = await _cursoRepository.ObtenerCursosAdminAsync();
            var cursoSeleccionadoId = cursoId ?? cursos.FirstOrDefault()?.IdActividad;
            var filtroEstado = NormalizarFiltroEstadoPago(estadoPago);
            var textoBusqueda = (busqueda ?? string.Empty).Trim();
            if (!cursoSeleccionadoId.HasValue)
            {
                return new ReportePagantesViewModel
                {
                    Cursos = cursos,
                    FiltroEstadoPago = filtroEstado,
                    Busqueda = textoBusqueda,
                    MensajeError = "No hay cursos disponibles para reportar."
                };
            }

            var seguimiento = await _cursoRepository.ObtenerSeguimientoPagosAdminAsync(cursoSeleccionadoId.Value);
            if (await SincronizarPagosPendientesIzipayAsync(seguimiento))
            {
                seguimiento = await _cursoRepository.ObtenerSeguimientoPagosAdminAsync(cursoSeleccionadoId.Value);
            }

            var inscritos = await _cursoRepository.ObtenerReporteInscritosGeneralAsync(cursoSeleccionadoId.Value);
            var todosItems = inscritos
                .Select(alumno => new ReportePagantesItem
                {
                    IdInscripcion = alumno.IdInscripcion,
                    CursoId = alumno.CursoId,
                    NombreCurso = alumno.NombreCurso,
                    NumeroDocumento = alumno.NumeroDocumento,
                    Nombres = alumno.Nombres,
                    Apellidos = alumno.Apellidos,
                    Correo = alumno.Correo,
                    Celular = alumno.Celular,
                    InstitucionProcedencia = alumno.InstitucionProcedencia,
                    AsistiraPresencialPrimerDia = alumno.AsistiraPresencialPrimerDia,
                    FechaRegistro = alumno.FechaRegistro,
                    FechaUltimoPago = alumno.FechaUltimoPago,
                    CostoFinal = alumno.CostoFinal,
                    NumeroCuotas = alumno.NumeroCuotas,
                    CuotasPagadas = alumno.CuotasPagadas,
                    MontoPagado = alumno.MontoPagado,
                    MontoPendiente = alumno.MontoPendiente,
                    EstadoPagoGeneral = alumno.EstadoReportePago == "PAGADO" ? "PAGADO" : "PENDIENTE",
                    EstadoReportePago = alumno.EstadoReportePago,
                    EstadoReportePagoEtiqueta = alumno.EstadoReportePagoEtiqueta
                })
                .ToList();

            var items = todosItems
                .Where(item => DebeIncluirPorFiltroEstado(item, filtroEstado))
                .Where(item => CoincideBusquedaReporte(item, textoBusqueda))
                .OrderByDescending(item => item.MontoPagado)
                .ThenByDescending(item => item.MontoPendiente)
                .ThenBy(item => item.Apellidos)
                .ThenBy(item => item.Nombres)
                .ToList();
            var itemsConPago = items
                .Where(EsInscritoConPagoParaReporte)
                .ToList();
            var cursoActual = cursos.FirstOrDefault(curso => curso.IdActividad == cursoSeleccionadoId.Value);

            return new ReportePagantesViewModel
            {
                Cursos = cursos,
                CursoSeleccionadoId = cursoSeleccionadoId,
                FiltroEstadoPago = filtroEstado,
                Busqueda = textoBusqueda,
                Detalle = new ReportePagantesDetalle
                {
                    CursoId = cursoSeleccionadoId.Value,
                    NombreCurso = cursoActual?.NombreActividad ?? string.Empty,
                    FiltroEstadoPago = filtroEstado,
                    Busqueda = textoBusqueda,
                    TotalInscritos = todosItems.Count,
                    TotalPagantes = todosItems.Count(item => item.EstadoReportePago is "PAGADO" or "PAGADO_PARCIAL"),
                    Pagados = todosItems.Count(item => item.EstadoReportePago == "PAGADO"),
                    PagadosParciales = todosItems.Count(item => item.EstadoReportePago == "PAGADO_PARCIAL"),
                    Pendientes = todosItems.Count(item => item.EstadoReportePago == "PENDIENTE"),
                    SinCobro = todosItems.Count(item => item.EstadoReportePago == "SIN_COBRO"),
                    SolicitaConfirmacionPresencialPrimerDia = cursoActual?.SolicitaConfirmacionPresencialPrimerDia == true,
                    AsistiranPresencialPrimerDia = todosItems.Count(item => item.AsistiraPresencialPrimerDia == true),
                    NoAsistiranPresencialPrimerDia = todosItems.Count(item => item.AsistiraPresencialPrimerDia == false),
                    SinRespuestaPresencialPrimerDia = todosItems.Count(item => !item.AsistiraPresencialPrimerDia.HasValue),
                    MontoPagadoTotal = itemsConPago.Sum(item => item.MontoPagado),
                    MontoPendienteTotal = itemsConPago.Sum(item => item.MontoPendiente),
                    MontoComprometidoTotal = itemsConPago.Sum(item => item.CostoFinal),
                    Items = items
                }
            };
        }

        private static bool EsInscritoConPagoParaReporte(ReportePagantesItem item)
        {
            return item.EstadoReportePago is "PAGADO" or "PAGADO_PARCIAL";
        }

        private static string NormalizarFiltroEstadoPago(string? estadoPago)
        {
            var value = (estadoPago ?? "todos").Trim().ToLowerInvariant();
            return value switch
            {
                "pagado" or "completo" => "pagado",
                "parcial" or "fraccionado" or "pagado_parcial" => "parcial",
                "pendiente" or "sin_pago" => "pendiente",
                "sin_cobro" => "sin_cobro",
                _ => "todos"
            };
        }

        private static bool DebeIncluirPorFiltroEstado(ReportePagantesItem item, string filtroEstado)
        {
            return filtroEstado switch
            {
                "pagado" => item.EstadoReportePago == "PAGADO",
                "parcial" => item.EstadoReportePago == "PAGADO_PARCIAL",
                "pendiente" => item.EstadoReportePago == "PENDIENTE",
                "sin_cobro" => item.EstadoReportePago == "SIN_COBRO",
                _ => true
            };
        }

        private static bool CoincideBusquedaReporte(ReportePagantesItem item, string busqueda)
        {
            if (string.IsNullOrWhiteSpace(busqueda))
            {
                return true;
            }

            var haystack = string.Join(" ", item.Nombres, item.Apellidos, item.NumeroDocumento, item.Celular, item.Correo, item.InstitucionProcedencia)
                .ToUpperInvariant();
            return haystack.Contains(busqueda.ToUpperInvariant(), StringComparison.Ordinal);
        }

        private static string ObtenerEtiquetaFiltroEstadoPago(string filtro)
        {
            return filtro switch
            {
                "pagado" => "Pagado",
                "parcial" => "Pagado parcial",
                "pendiente" => "Pendiente",
                "sin_cobro" => "Sin cobro",
                _ => "Todos"
            };
        }

        private static string ConstruirReporteHtmlExportable(ReporteInscritosCursoDetalle detalle, string destino)
        {
            var resumen = detalle.Resumen;
            var mostrarSinCobro = resumen.SinCobro > 0;
            var cursoTieneCobro = detalle.CursoTieneCobro;
            var builder = new StringBuilder();
            builder.AppendLine("<!DOCTYPE html>");
            builder.AppendLine("<html><head><meta charset=\"utf-8\">");
            builder.AppendLine("<style>");
            builder.AppendLine("body{font-family:Arial,sans-serif;color:#1f2933}");
            builder.AppendLine("h1{font-size:22px;margin-bottom:4px} h2{font-size:16px;margin-top:24px}");
            builder.AppendLine("table{border-collapse:collapse;width:100%;margin-top:8px}");
            builder.AppendLine("th,td{border:1px solid #b8c2cc;padding:7px;font-size:12px}");
            builder.AppendLine("th{background:#edf2f7;text-align:left}");
            builder.AppendLine(".num{text-align:right}");
            builder.AppendLine("</style>");
            builder.AppendLine("</head><body>");
            builder.AppendLine($"<h1>Reporte de inscritos por curso</h1>");
            builder.AppendLine($"<p><strong>Curso:</strong> {Html(resumen.NombreCurso)}</p>");
            builder.AppendLine($"<p><strong>Generado:</strong> {DateTime.Now:dd/MM/yyyy HH:mm} | <strong>Formato:</strong> {Html(destino)}</p>");

            builder.AppendLine("<h2>Resumen general</h2>");
            builder.AppendLine("<table><thead><tr><th>Indicador</th><th class=\"num\">Valor</th></tr></thead><tbody>");
            builder.AppendLine($"<tr><td>Total registros</td><td class=\"num\">{resumen.TotalInscritos}</td></tr>");
            if (cursoTieneCobro)
            {
                builder.AppendLine($"<tr><td>Inscritos</td><td class=\"num\">{resumen.Pagados + resumen.PagadosParciales}</td></tr>");
                builder.AppendLine($"<tr><td>Pre-inscritos</td><td class=\"num\">{resumen.Pendientes}</td></tr>");
            }
            builder.AppendLine($"<tr><td>Internos INSNSB</td><td class=\"num\">{resumen.Internos}</td></tr>");
            builder.AppendLine($"<tr><td>Externos</td><td class=\"num\">{resumen.Externos}</td></tr>");
            if (resumen.SolicitaConfirmacionPresencialPrimerDia)
            {
                builder.AppendLine($"<tr><td>Asistiran presencialmente al primer dia</td><td class=\"num\">{resumen.AsistiranPresencialPrimerDia}</td></tr>");
                builder.AppendLine($"<tr><td>No asistiran presencialmente al primer dia</td><td class=\"num\">{resumen.NoAsistiranPresencialPrimerDia}</td></tr>");
                
            }
            if (cursoTieneCobro)
            {
                builder.AppendLine($"<tr><td>Pagado</td><td class=\"num\">{resumen.Pagados}</td></tr>");
                builder.AppendLine($"<tr><td>Pagado parcial</td><td class=\"num\">{resumen.PagadosParciales}</td></tr>");
                builder.AppendLine($"<tr><td>Pendiente</td><td class=\"num\">{resumen.Pendientes}</td></tr>");
                if (mostrarSinCobro)
                {
                    builder.AppendLine($"<tr><td>Sin cobro</td><td class=\"num\">{resumen.SinCobro}</td></tr>");
                }
                builder.AppendLine($"<tr><td>Monto pagado</td><td class=\"num\">{Money(resumen.MontoPagado)}</td></tr>");
                builder.AppendLine($"<tr><td>Deuda pendiente con pago</td><td class=\"num\">{Money(resumen.MontoDeuda)}</td></tr>");
                builder.AppendLine($"<tr><td>Total comprometido con pago</td><td class=\"num\">{Money(resumen.MontoTotal)}</td></tr>");
            }
            builder.AppendLine("</tbody></table>");

            AppendReporteTable(builder, "Inscritos por pais", detalle.PorPais, mostrarSinCobro, cursoTieneCobro);
            AppendReporteTable(builder, "Inscritos por region", detalle.PorRegion, mostrarSinCobro, cursoTieneCobro);
            AppendReporteTable(builder, "Inscritos por institucion de procedencia", detalle.PorInstitucion, mostrarSinCobro, cursoTieneCobro);
            AppendReporteTable(builder, "Internos vs externos", detalle.PorTipoParticipante, mostrarSinCobro, cursoTieneCobro);

            builder.AppendLine("</body></html>");
            return builder.ToString();
        }

        private static string ConstruirReportePagantesHtmlExportable(ReportePagantesDetalle detalle, string destino)
        {
            var mostrarSinCobro = detalle.SinCobro > 0;
            var cursoPreguntaPresencialidad = detalle.SolicitaConfirmacionPresencialPrimerDia;
            var builder = new StringBuilder();
            builder.AppendLine("<!DOCTYPE html>");
            builder.AppendLine("<html><head><meta charset=\"utf-8\">");
            builder.AppendLine("<style>");
            builder.AppendLine("body{font-family:Arial,sans-serif;color:#1f2933}");
            builder.AppendLine("h1{font-size:22px;margin-bottom:4px} h2{font-size:16px;margin-top:24px}");
            builder.AppendLine("table{border-collapse:collapse;width:100%;margin-top:8px}");
            builder.AppendLine("th,td{border:1px solid #b8c2cc;padding:7px;font-size:12px}");
            builder.AppendLine("th{background:#edf2f7;text-align:left}");
            builder.AppendLine(".num{text-align:right}");
            builder.AppendLine("</style>");
            builder.AppendLine("</head><body>");
            builder.AppendLine("<h1>Reporte de estado de pagos por curso</h1>");
            builder.AppendLine($"<p><strong>Curso:</strong> {Html(string.IsNullOrWhiteSpace(detalle.NombreCurso) ? "Curso seleccionado" : detalle.NombreCurso)}</p>");
            builder.AppendLine($"<p><strong>Generado:</strong> {DateTime.Now:dd/MM/yyyy HH:mm} | <strong>Formato:</strong> {Html(destino)} | <strong>Filtro:</strong> {Html(ObtenerEtiquetaFiltroEstadoPago(detalle.FiltroEstadoPago))}</p>");
            if (!string.IsNullOrWhiteSpace(detalle.Busqueda))
            {
                builder.AppendLine($"<p><strong>Busqueda:</strong> {Html(detalle.Busqueda)}</p>");
            }

            builder.AppendLine("<h2>Resumen general</h2>");
            builder.AppendLine("<table><thead><tr><th>Indicador</th><th class=\"num\">Valor</th></tr></thead><tbody>");
            builder.AppendLine($"<tr><td>Total registros</td><td class=\"num\">{detalle.TotalInscritos}</td></tr>");
            builder.AppendLine($"<tr><td>Inscritos</td><td class=\"num\">{detalle.Pagados + detalle.PagadosParciales}</td></tr>");
            builder.AppendLine($"<tr><td>Pre-inscritos</td><td class=\"num\">{detalle.Pendientes}</td></tr>");
            builder.AppendLine($"<tr><td>Total con pago</td><td class=\"num\">{detalle.TotalPagantes}</td></tr>");
            builder.AppendLine($"<tr><td>Pagado</td><td class=\"num\">{detalle.Pagados}</td></tr>");
            builder.AppendLine($"<tr><td>Pagado parcial</td><td class=\"num\">{detalle.PagadosParciales}</td></tr>");
            builder.AppendLine($"<tr><td>Pendiente</td><td class=\"num\">{detalle.Pendientes}</td></tr>");
            if (mostrarSinCobro)
            {
                builder.AppendLine($"<tr><td>Sin cobro</td><td class=\"num\">{detalle.SinCobro}</td></tr>");
            }
            if (cursoPreguntaPresencialidad)
            {
                builder.AppendLine($"<tr><td>Asistiran presencialmente al primer dia</td><td class=\"num\">{detalle.AsistiranPresencialPrimerDia}</td></tr>");
                builder.AppendLine($"<tr><td>No asistiran presencialmente al primer dia</td><td class=\"num\">{detalle.NoAsistiranPresencialPrimerDia}</td></tr>");
                builder.AppendLine($"<tr><td>Sin respuesta presencial</td><td class=\"num\">{detalle.SinRespuestaPresencialPrimerDia}</td></tr>");
            }
            builder.AppendLine($"<tr><td>Monto pagado</td><td class=\"num\">{Money(detalle.MontoPagadoTotal)}</td></tr>");
            builder.AppendLine($"<tr><td>Monto pendiente con pago</td><td class=\"num\">{Money(detalle.MontoPendienteTotal)}</td></tr>");
            builder.AppendLine($"<tr><td>Monto comprometido con pago</td><td class=\"num\">{Money(detalle.MontoComprometidoTotal)}</td></tr>");
            builder.AppendLine("</tbody></table>");

            builder.AppendLine("<h2>Detalle de inscritos segun estado de pago</h2>");
            builder.AppendLine("<table>");
            builder.Append("<thead><tr><th>Alumno</th><th>DNI / Documento</th><th>Celular</th><th>Correo</th><th>Institucion de procedencia</th>");
            if (cursoPreguntaPresencialidad)
            {
                builder.Append("<th>Asistencia presencial primer dia</th>");
            }
            builder.AppendLine("<th>Fecha registro</th><th>Ultimo pago</th><th>Estado reporte</th><th class=\"num\">Monto pagado</th><th class=\"num\">Monto pendiente</th><th class=\"num\">Cuotas</th></tr></thead>");
            builder.AppendLine("<tbody>");

            if (detalle.Items.Count == 0)
            {
                builder.AppendLine($"<tr><td colspan=\"{(cursoPreguntaPresencialidad ? 12 : 11)}\">No hay inscritos que coincidan con el filtro seleccionado.</td></tr>");
            }
            else
            {
                foreach (var item in detalle.Items)
                {
                    builder.AppendLine("<tr>");
                    builder.AppendLine($"<td>{Html($"{item.Nombres} {item.Apellidos}".Trim())}</td>");
                    builder.AppendLine($"<td>{Html(item.NumeroDocumento)}</td>");
                    builder.AppendLine($"<td>{Html(string.IsNullOrWhiteSpace(item.Celular) ? "-" : item.Celular)}</td>");
                    builder.AppendLine($"<td>{Html(string.IsNullOrWhiteSpace(item.Correo) ? "-" : item.Correo)}</td>");
                    builder.AppendLine($"<td>{Html(string.IsNullOrWhiteSpace(item.InstitucionProcedencia) ? "-" : item.InstitucionProcedencia)}</td>");
                    if (cursoPreguntaPresencialidad)
                    {
                        builder.AppendLine($"<td>{Html(item.AsistiraPresencialPrimerDiaEtiqueta)}</td>");
                    }
                    builder.AppendLine($"<td>{item.FechaRegistro:dd/MM/yyyy HH:mm}</td>");
                    builder.AppendLine($"<td>{(item.FechaUltimoPago.HasValue ? item.FechaUltimoPago.Value.ToString("dd/MM/yyyy HH:mm") : "-")}</td>");
                    builder.AppendLine($"<td>{Html(item.EstadoReportePagoEtiqueta)}</td>");
                    builder.AppendLine($"<td class=\"num\">{Money(item.MontoPagado)}</td>");
                    builder.AppendLine($"<td class=\"num\">{Money(item.MontoPendiente)}</td>");
                    builder.AppendLine($"<td class=\"num\">{item.CuotasPagadas}/{item.NumeroCuotas}</td>");
                    builder.AppendLine("</tr>");
                }
            }

            builder.AppendLine("</tbody></table>");
            builder.AppendLine("</body></html>");
            return builder.ToString();
        }

        private static string ConstruirReporteInscritosGeneralHtmlExportable(ReporteInscritosCursoDetalle detalle, IReadOnlyList<ReporteInscritoGeneralItem> items)
        {
            var resumen = detalle.Resumen;
            var cursoTieneCobro = detalle.CursoTieneCobro;
            var builder = new StringBuilder();
            builder.AppendLine("<!DOCTYPE html>");
            builder.AppendLine("<html><head><meta charset=\"utf-8\">");
            builder.AppendLine("<style>");
            builder.AppendLine("body{font-family:Arial,sans-serif;color:#1f2933}");
            builder.AppendLine("table{border-collapse:collapse;width:100%;margin-top:8px}");
            builder.AppendLine("th,td{border:1px solid #b8c2cc;padding:7px;font-size:12px}");
            builder.AppendLine("th{background:#edf2f7;text-align:left}");
            builder.AppendLine(".num{text-align:right}");
            builder.AppendLine("</style>");
            builder.AppendLine("</head><body>");
            builder.AppendLine("<h1>Lista general de inscritos</h1>");
            builder.AppendLine($"<p><strong>Curso:</strong> {Html(resumen.NombreCurso)}</p>");
            builder.AppendLine($"<p><strong>Generado:</strong> {DateTime.Now:dd/MM/yyyy HH:mm}</p>");
            builder.AppendLine("<table>");
            builder.Append("<thead><tr><th>ID inscripcion</th><th>Curso</th>");
            if (cursoTieneCobro)
            {
                builder.Append("<th>Estado reporte</th>");
            }
            builder.Append("<th>Tipo participante</th><th>Tipo documento</th><th>Documento</th><th>Nombres</th><th>Apellidos</th><th>Correo</th><th>Codigo pais</th><th>Celular</th><th>Pais</th><th>Region</th><th>Profesion</th><th>Especialidad</th><th>Institucion</th><th>Institucion procedencia</th><th>Condicion INSNSB</th><th>Medio comunicacion</th>");
            if (resumen.SolicitaConfirmacionPresencialPrimerDia)
            {
                builder.Append("<th>Asistencia presencial primer dia</th>");
            }
            builder.Append("<th>Fecha registro</th>");
            if (cursoTieneCobro)
            {
                builder.Append("<th class=\"num\">Costo final</th><th class=\"num\">Nro cuotas</th><th class=\"num\">Cuotas pagadas</th><th class=\"num\">Cuotas pendientes</th><th class=\"num\">Monto pagado</th><th class=\"num\">Monto pendiente</th><th class=\"num\">Monto total</th><th>Ultimo pago</th>");
            }
            builder.AppendLine("</tr></thead>");
            builder.AppendLine("<tbody>");

            if (items.Count == 0)
            {
                var columnasBase = resumen.SolicitaConfirmacionPresencialPrimerDia ? 21 : 20;
                builder.AppendLine($"<tr><td colspan=\"{(cursoTieneCobro ? columnasBase + 8 : columnasBase)}\">No hay inscritos para este curso.</td></tr>");
            }
            else
            {
                foreach (var item in items)
                {
                    builder.AppendLine("<tr>");
                    builder.AppendLine($"<td>{item.IdInscripcion}</td>");
                    builder.AppendLine($"<td>{Html(item.NombreCurso)}</td>");
                    if (cursoTieneCobro)
                    {
                        builder.AppendLine($"<td>{Html(item.EstadoReportePagoEtiqueta)}</td>");
                    }
                    builder.AppendLine($"<td>{Html(item.TipoParticipante)}</td>");
                    builder.AppendLine($"<td>{Html(item.TipoDocumento)}</td>");
                    builder.AppendLine($"<td>{Html(item.NumeroDocumento)}</td>");
                    builder.AppendLine($"<td>{Html(item.Nombres)}</td>");
                    builder.AppendLine($"<td>{Html(item.Apellidos)}</td>");
                    builder.AppendLine($"<td>{Html(item.Correo)}</td>");
                    builder.AppendLine($"<td>{Html(item.CodigoPais)}</td>");
                    builder.AppendLine($"<td>{Html(item.Celular)}</td>");
                    builder.AppendLine($"<td>{Html(item.Pais)}</td>");
                    builder.AppendLine($"<td>{Html(item.Region)}</td>");
                    builder.AppendLine($"<td>{Html(item.Profesion)}</td>");
                    builder.AppendLine($"<td>{Html(item.Especialidad)}</td>");
                    builder.AppendLine($"<td>{Html(item.Institucion)}</td>");
                    builder.AppendLine($"<td>{Html(item.InstitucionProcedencia)}</td>");
                    builder.AppendLine($"<td>{Html(item.CondicionLaboralInsnsb)}</td>");
                    builder.AppendLine($"<td>{Html(item.MedioComunicacion)}</td>");
                    if (resumen.SolicitaConfirmacionPresencialPrimerDia)
                    {
                        builder.AppendLine($"<td>{Html(item.AsistiraPresencialPrimerDiaEtiqueta)}</td>");
                    }
                    builder.AppendLine($"<td>{item.FechaRegistro:dd/MM/yyyy HH:mm}</td>");
                    if (cursoTieneCobro)
                    {
                        builder.AppendLine($"<td class=\"num\">{item.CostoFinal:N2}</td>");
                        builder.AppendLine($"<td class=\"num\">{item.NumeroCuotas}</td>");
                        builder.AppendLine($"<td class=\"num\">{item.CuotasPagadas}</td>");
                        builder.AppendLine($"<td class=\"num\">{item.CuotasPendientes}</td>");
                        builder.AppendLine($"<td class=\"num\">{item.MontoPagado:N2}</td>");
                        builder.AppendLine($"<td class=\"num\">{item.MontoPendiente:N2}</td>");
                        builder.AppendLine($"<td class=\"num\">{item.MontoTotal:N2}</td>");
                        builder.AppendLine($"<td>{(item.FechaUltimoPago.HasValue ? item.FechaUltimoPago.Value.ToString("dd/MM/yyyy HH:mm") : "-")}</td>");
                    }
                    builder.AppendLine("</tr>");
                }
            }

            builder.AppendLine("</tbody></table>");
            builder.AppendLine("</body></html>");
            return builder.ToString();
        }

        private static void AppendReporteTable(StringBuilder builder, string titulo, IReadOnlyList<ReporteInscritosAgrupacionItem> items, bool mostrarSinCobro, bool cursoTieneCobro)
        {
            builder.AppendLine($"<h2>{Html(titulo)}</h2>");
            builder.AppendLine("<table>");
            builder.Append("<thead><tr><th>Categoria</th><th class=\"num\">Total</th>");
            if (cursoTieneCobro)
            {
                builder.Append("<th class=\"num\">Inscritos</th><th class=\"num\">Pre-inscritos</th>");
            }

            builder.Append("<th class=\"num\">Internos</th><th class=\"num\">Externos</th>");
            if (cursoTieneCobro)
            {
                builder.Append("<th class=\"num\">Pagado</th><th class=\"num\">Pagado parcial</th><th class=\"num\">Pendiente</th>");
                if (mostrarSinCobro)
                {
                    builder.Append("<th class=\"num\">Sin cobro</th>");
                }

                builder.Append("<th class=\"num\">Monto pagado</th><th class=\"num\">Deuda con pago</th><th class=\"num\">Comprometido S/</th>");
            }

            builder.AppendLine("</tr></thead>");
            builder.AppendLine("<tbody>");

            if (items.Count == 0)
            {
                builder.AppendLine($"<tr><td colspan=\"{(cursoTieneCobro ? (mostrarSinCobro ? 13 : 12) : 4)}\">No hay datos para mostrar.</td></tr>");
            }
            else
            {
                foreach (var item in items)
                {
                    builder.AppendLine("<tr>");
                    builder.AppendLine($"<td>{Html(item.Categoria)}</td>");
                    builder.AppendLine($"<td class=\"num\">{item.TotalInscritos}</td>");
                    if (cursoTieneCobro)
                    {
                        builder.AppendLine($"<td class=\"num\">{item.Pagados + item.PagadosParciales}</td>");
                        builder.AppendLine($"<td class=\"num\">{item.Pendientes}</td>");
                    }

                    builder.AppendLine($"<td class=\"num\">{item.Internos}</td>");
                    builder.AppendLine($"<td class=\"num\">{item.Externos}</td>");
                    if (cursoTieneCobro)
                    {
                        builder.AppendLine($"<td class=\"num\">{item.Pagados}</td>");
                        builder.AppendLine($"<td class=\"num\">{item.PagadosParciales}</td>");
                        builder.AppendLine($"<td class=\"num\">{item.Pendientes}</td>");
                        if (mostrarSinCobro)
                        {
                            builder.AppendLine($"<td class=\"num\">{item.SinCobro}</td>");
                        }

                        builder.AppendLine($"<td class=\"num\">{Money(item.MontoPagado)}</td>");
                        builder.AppendLine($"<td class=\"num\">{Money(item.MontoDeuda)}</td>");
                        builder.AppendLine($"<td class=\"num\">{Money(item.MontoTotal)}</td>");
                    }
                    builder.AppendLine("</tr>");
                }
            }

            builder.AppendLine("</tbody></table>");
        }

        private static string ConstruirNombreArchivo(ReporteInscritosCursoDetalle detalle, string extension)
        {
            var baseName = string.IsNullOrWhiteSpace(detalle.Resumen.NombreCurso)
                ? $"reporte_inscritos_{detalle.Resumen.CursoId}"
                : $"reporte_inscritos_{detalle.Resumen.CursoId}_{detalle.Resumen.NombreCurso}";
            var safeName = new string(baseName
                .Select(character => char.IsLetterOrDigit(character) ? character : '_')
                .ToArray())
                .Trim('_');

            return $"{safeName}_{DateTime.Now:yyyyMMdd_HHmm}.{extension}";
        }

        private static string ConstruirNombreArchivoGeneralInscritos(ReporteInscritosCursoDetalle detalle, string extension)
        {
            var baseName = string.IsNullOrWhiteSpace(detalle.Resumen.NombreCurso)
                ? $"lista_general_inscritos_{detalle.Resumen.CursoId}"
                : $"lista_general_inscritos_{detalle.Resumen.CursoId}_{detalle.Resumen.NombreCurso}";
            var safeName = new string(baseName
                .Select(character => char.IsLetterOrDigit(character) ? character : '_')
                .ToArray())
                .Trim('_');

            return $"{safeName}_{DateTime.Now:yyyyMMdd_HHmm}.{extension}";
        }

        private static string ConstruirNombreArchivoPagantes(ReportePagantesDetalle detalle, string extension)
        {
            var baseName = string.IsNullOrWhiteSpace(detalle.NombreCurso)
                ? $"reporte_pagantes_{detalle.CursoId}"
                : $"reporte_pagantes_{detalle.CursoId}_{detalle.NombreCurso}";
            var safeName = new string(baseName
                .Select(character => char.IsLetterOrDigit(character) ? character : '_')
                .ToArray())
                .Trim('_');

            return $"{safeName}_{DateTime.Now:yyyyMMdd_HHmm}.{extension}";
        }

        private static string Html(string? value)
        {
            return WebUtility.HtmlEncode(value ?? string.Empty);
        }

        private static string Money(decimal value)
        {
            return value == 0m
                ? "--"
                : $"S/ {value.ToString("N2", CultureInfo.GetCultureInfo("es-PE"))}";
        }

        private bool EstaAutenticado()
        {
            return string.Equals(HttpContext.Session.GetString(AdminSessionKey), "1", StringComparison.Ordinal);
        }

        private async Task<AdminCursoConfiguracionViewModel> ConstruirVistaConfiguracionCursoAsync(
            long cursoId,
            CursoConfiguracionAdminInput? configuracionCurso = null,
            CursoPagadoResumen? vistaPreviaCurso = null,
            string? mensajeError = null,
            string? mensajeExito = null)
        {
            var cursos = await _cursoRepository.ObtenerCursosAdminAsync();
            var cursoSeleccionado = cursos.FirstOrDefault(curso => curso.IdActividad == cursoId);

            if (cursoSeleccionado is null)
            {
                return new AdminCursoConfiguracionViewModel
                {
                    MensajeError = "El curso solicitado no esta disponible para edicion."
                };
            }

            return new AdminCursoConfiguracionViewModel
            {
                CursoSeleccionado = cursoSeleccionado,
                ConfiguracionCurso = configuracionCurso ?? MapearConfiguracionCurso(cursoSeleccionado),
                VistaPreviaCurso = vistaPreviaCurso ?? cursoSeleccionado,
                MensajeError = mensajeError,
                MensajeExito = mensajeExito
            };
        }

        private async Task<PanelAdminCodigosViewModel> ConstruirDashboardAsync(
            long? cursoId,
            CursoConfiguracionAdminInput? configuracionCurso = null,
            CursoPagadoResumen? vistaPreviaCurso = null)
        {
            var cursos = await _cursoRepository.ObtenerCursosAdminAsync();
            var cursoSeleccionadoId = cursoId ?? cursos.FirstOrDefault()?.IdActividad;
            var cursoSeleccionado = cursos.FirstOrDefault(curso => curso.IdActividad == cursoSeleccionadoId);

            if (cursoSeleccionadoId is null || cursoSeleccionado is null)
            {
                return new PanelAdminCodigosViewModel
                {
                    Cursos = cursos,
                    CursoSeleccionadoId = null,
                    MensajeError = cursos.Count == 0
                        ? "No hay cursos pagados con inscripcion activa para administrar."
                        : "Seleccione un curso para administrar sus codigos.",
                    Formulario = new GeneracionCodigosInput(),
                    ConfiguracionCurso = new CursoConfiguracionAdminInput()
                };
            }

            var resumen = await _cursoRepository.ObtenerResumenCodigosAsync(cursoSeleccionadoId.Value);
            var seguimientoPagos = await _cursoRepository.ObtenerSeguimientoPagosAdminAsync(cursoSeleccionadoId.Value);
            if (await SincronizarPagosPendientesIzipayAsync(seguimientoPagos))
            {
                seguimientoPagos = await _cursoRepository.ObtenerSeguimientoPagosAdminAsync(cursoSeleccionadoId.Value);
            }

            return new PanelAdminCodigosViewModel
            {
                Cursos = cursos,
                CursoSeleccionadoId = cursoSeleccionadoId,
                CursoSeleccionadoNombre = cursoSeleccionado.NombreActividad,
                TotalCodigos = resumen.Total,
                CodigosDisponibles = resumen.Disponibles,
                CodigosUsados = resumen.Usados,
                Codigos = resumen.Codigos,
                SeguimientoPagos = seguimientoPagos,
                Formulario = new GeneracionCodigosInput
                {
                    CursoId = cursoSeleccionadoId.Value
                },
                ConfiguracionCurso = configuracionCurso ?? MapearConfiguracionCurso(cursoSeleccionado),
                VistaPreviaCurso = vistaPreviaCurso ?? cursoSeleccionado
            };
        }

        private static PanelAdminCodigosViewModel CopiarDashboard(
            PanelAdminCodigosViewModel baseModel,
            string? mensajeError,
            string? mensajeExito,
            GeneracionCodigosInput? formulario = null,
            IReadOnlyList<string>? codigosGenerados = null,
            CursoConfiguracionAdminInput? configuracionCurso = null,
            CursoPagadoResumen? vistaPreviaCurso = null)
        {
            return new PanelAdminCodigosViewModel
            {
                Cursos = baseModel.Cursos,
                CursoSeleccionadoId = baseModel.CursoSeleccionadoId,
                CursoSeleccionadoNombre = baseModel.CursoSeleccionadoNombre,
                TotalCodigos = baseModel.TotalCodigos,
                CodigosDisponibles = baseModel.CodigosDisponibles,
                CodigosUsados = baseModel.CodigosUsados,
                Codigos = baseModel.Codigos,
                SeguimientoPagos = baseModel.SeguimientoPagos,
                Formulario = formulario ?? baseModel.Formulario,
                ConfiguracionCurso = configuracionCurso ?? baseModel.ConfiguracionCurso,
                VistaPreviaCurso = vistaPreviaCurso ?? baseModel.VistaPreviaCurso,
                MensajeError = mensajeError,
                MensajeExito = mensajeExito,
                CodigosGenerados = codigosGenerados ?? []
            };
        }

        private static CursoConfiguracionAdminInput MapearConfiguracionCurso(CursoPagadoResumen curso)
        {
            return new CursoConfiguracionAdminInput
            {
                CursoId = curso.IdActividad,
                NombreActividad = curso.NombreActividad,
                TextoDirigidoA = curso.TextoDirigidoA,
                Modalidad = curso.Modalidad,
                TextoFechasDuracion = curso.TextoFechasDuracion,
                HorarioTexto = curso.HorarioTexto,
                LugarActividad = curso.LugarActividad,
                CostoBase = curso.CostoBase,
                CostoPersonalInsnsb = curso.CostoPersonalInsnsb,
                MaxCuotas = curso.MaxCuotas,
                MaxInscritos = curso.MaxInscritos,
                OpcionesCuotas = curso.OpcionesCuotas.Count == 0
                    ? null
                    : $"[{string.Join(",", curso.OpcionesCuotas)}]",
                FechasPagoCuotas = curso.FechasPagoCuotas.Count == 0
                    ? null
                    : $"[\"{string.Join("\",\"", curso.FechasPagoCuotas)}\"]",
                CodigoCentroCosto = curso.CodigoCentroCosto,
                CelularContactoActividad = curso.CelularContactoActividad,
                UrlProgramaWeb = curso.UrlProgramaWeb,
                UrlBannerWeb = curso.UrlBannerWeb,
                VisibleEnFormulario = NormalizarSiNo(curso.VisibleEnFormulario, "Si"),
                RestriccionInscripcionUnica = curso.RestriccionInscripcionUnica ? "Si" : "No",
                SolicitaConfirmacionPresencialPrimerDia = curso.SolicitaConfirmacionPresencialPrimerDia ? "Si" : "No"
            };
        }

        private static CursoPagadoResumen ConstruirCursoPreview(CursoConfiguracionAdminInput input)
        {
            return new CursoPagadoResumen
            {
                IdActividad = input.CursoId,
                NombreActividad = input.NombreActividad?.Trim() ?? string.Empty,
                VisibleEnFormulario = NormalizarSiNo(input.VisibleEnFormulario, "Si"),
                FuePagadoHistorico = "Si",
                Modalidad = input.Modalidad?.Trim(),
                TextoFechasDuracion = input.TextoFechasDuracion?.Trim(),
                HorarioTexto = input.HorarioTexto?.Trim(),
                LugarActividad = input.LugarActividad?.Trim(),
                TextoDirigidoA = input.TextoDirigidoA?.Trim(),
                CostoBase = input.CostoBase,
                CostoPersonalInsnsb = input.CostoPersonalInsnsb,
                MaxCuotas = input.MaxCuotas,
                OpcionesCuotas = ParseOpcionesCuotas(input.OpcionesCuotas),
                MaxInscritos = input.MaxInscritos,
                FechasPagoCuotas = ParseListaTexto(input.FechasPagoCuotas),
                CodigoCentroCosto = input.CodigoCentroCosto?.Trim(),
                CelularContactoActividad = input.CelularContactoActividad?.Trim(),
                UrlBannerWeb = input.UrlBannerWeb?.Trim(),
                UrlProgramaWeb = input.UrlProgramaWeb?.Trim(),
                RestriccionInscripcionUnica = string.Equals(input.RestriccionInscripcionUnica, "Si", StringComparison.OrdinalIgnoreCase),
                SolicitaConfirmacionPresencialPrimerDia = string.Equals(input.SolicitaConfirmacionPresencialPrimerDia, "Si", StringComparison.OrdinalIgnoreCase)
            };
        }

        private static IReadOnlyList<int> ParseOpcionesCuotas(string? rawValue)
        {
            return (rawValue ?? string.Empty)
                .Replace("[", string.Empty, StringComparison.Ordinal)
                .Replace("]", string.Empty, StringComparison.Ordinal)
                .Split([',', ';', '|', '\r', '\n'], StringSplitOptions.RemoveEmptyEntries)
                .Select(segment => segment.Trim())
                .Select(segment => int.TryParse(segment, out var cuota) ? cuota : 0)
                .Where(cuota => cuota > 0)
                .Distinct()
                .OrderBy(cuota => cuota)
                .ToList();
        }

        private static IReadOnlyList<string> ParseListaTexto(string? rawValue)
        {
            return (rawValue ?? string.Empty)
                .Replace("[", string.Empty, StringComparison.Ordinal)
                .Replace("]", string.Empty, StringComparison.Ordinal)
                .Replace("\"", string.Empty, StringComparison.Ordinal)
                .Split([',', ';', '|', '\r', '\n'], StringSplitOptions.RemoveEmptyEntries)
                .Select(segment => segment.Trim())
                .Where(segment => !string.IsNullOrWhiteSpace(segment))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        private static string NormalizarSiNo(string? value, string fallback)
        {
            return string.Equals(value?.Trim(), "No", StringComparison.OrdinalIgnoreCase)
                ? "No"
                : string.Equals(value?.Trim(), "Si", StringComparison.OrdinalIgnoreCase)
                    ? "Si"
                    : fallback;
        }

        private async Task<bool> SincronizarPagosPendientesIzipayAsync(AdminSeguimientoPagosResumen seguimientoPagos)
        {
            var resultado = await SincronizarPagosPendientesIzipayConResumenAsync(seguimientoPagos);
            return resultado.HuboCambiosVisibles;
        }

        private async Task<(int Consultados, int PagosDetectados, int Fallidos, bool HuboCambiosVisibles)> SincronizarPagosPendientesIzipayConResumenAsync(AdminSeguimientoPagosResumen seguimientoPagos)
        {
            if (!_pagoIzipayService.EstaConfigurado)
            {
                return (0, 0, 0, false);
            }

            var idsPendientes = seguimientoPagos.Alumnos
                .SelectMany(alumno => alumno.Cuotas)
                .Where(cuota => cuota.IdPagoIzipay.HasValue
                    && (!string.Equals(cuota.Estado, "PAGADO", StringComparison.OrdinalIgnoreCase)
                        || string.IsNullOrWhiteSpace(cuota.UrlComprobantePdfIzipay)
                        || string.IsNullOrWhiteSpace(cuota.NumeroComprobanteIzipay)))
                .Select(cuota => cuota.IdPagoIzipay!.Value)
                .Distinct()
                .ToList();

            var huboCambiosVisibles = false;
            var consultados = 0;
            var pagosDetectados = 0;
            var fallidos = 0;
            foreach (var idPagoIzipay in idsPendientes)
            {
                consultados++;
                var estado = await _pagoIzipayService.ConsultarEstadoAsync(idPagoIzipay);
                if (!estado.Exitoso)
                {
                    fallidos++;
                    _logger.LogWarning("IziPay no pudo consultar el estado del pago {IdPagoIzipay}: {Mensaje}", idPagoIzipay, estado.Mensaje);
                    continue;
                }

                await _cursoRepository.ActualizarEstadoPagoIzipayAsync(estado);
                var pagoDetectado = estado.YaPago
                    || string.Equals(estado.Estado, "PAGADO", StringComparison.OrdinalIgnoreCase)
                    || string.Equals(estado.Estado, "CON COMPROBANTE", StringComparison.OrdinalIgnoreCase)
                    || !string.IsNullOrWhiteSpace(estado.NumeroComprobante)
                    || !string.IsNullOrWhiteSpace(estado.UrlComprobantePdf);

                if (pagoDetectado)
                {
                    pagosDetectados++;
                    huboCambiosVisibles = true;
                }
            }

            return (consultados, pagosDetectados, fallidos, huboCambiosVisibles);
        }
    }
}
