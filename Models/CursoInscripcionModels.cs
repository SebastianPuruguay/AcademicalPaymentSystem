using System.ComponentModel.DataAnnotations;

namespace CURSO_INTERCULTURALIDAD.Models
{
    public sealed class CursoPagadoResumen
    {
        public long IdActividad { get; set; }
        public string NombreActividad { get; set; } = string.Empty;
        public string SeCobra { get; set; } = "No";
        public string VisibleEnFormulario { get; set; } = "Si";
        public string FuePagadoHistorico { get; set; } = "No";
        public string? Modalidad { get; set; }
        public string? FechaInicio { get; set; }
        public string? FechaFin { get; set; }
        public string? TextoFechasDuracion { get; set; }
        public string? HorarioTexto { get; set; }
        public string? LugarActividad { get; set; }
        public string? Coordinador { get; set; }
        public string? TextoDirigidoA { get; set; }
        public decimal? CostoBase { get; set; }
        public decimal? CostoPersonalInsnsb { get; set; }
        public int? MaxCuotas { get; set; }
        public IReadOnlyList<int> OpcionesCuotas { get; set; } = [];
        public int? MaxInscritos { get; set; }
        public IReadOnlyList<string> FechasPagoCuotas { get; set; } = [];
        public string? CodigoCentroCosto { get; set; }
        public string? CelularContactoActividad { get; set; }
        public string? UrlBannerWeb { get; set; }
        public string? UrlProgramaWeb { get; set; }
        public bool RestriccionInscripcionUnica { get; set; }
        public bool SolicitaConfirmacionPresencialPrimerDia { get; set; }
        public bool EsGratisGlobal => string.Equals(SeCobra, "No", StringComparison.OrdinalIgnoreCase);
        public bool TieneLogicaCobro => string.Equals(SeCobra, "Si", StringComparison.OrdinalIgnoreCase);
        public bool RequierePago(decimal costoFinal) => TieneLogicaCobro && costoFinal > 0m;
    }

    public sealed class PaginaInscripcionCursosViewModel
    {
        public IReadOnlyList<CursoPagadoResumen> Cursos { get; init; } = [];
        public CursoPagadoResumen? CursoSeleccionado { get; init; }
        public long? CursoSeleccionadoId { get; init; }
        public string? MensajeError { get; init; }
        public string BannerPredeterminadoUrl { get; init; } = string.Empty;
    }

    public sealed class FormularioInscripcionCursoInput
    {
        [Range(1, long.MaxValue, ErrorMessage = "Debe seleccionar un curso.")]
        public long CursoId { get; set; }

        [Required(ErrorMessage = "Debe indicar el tipo de documento.")]
        [StringLength(30)]
        public string TipoDocumento { get; set; } = "DNI";

        [Required(ErrorMessage = "Debe ingresar el número de documento.")]
        [StringLength(20)]
        public string NumeroDocumento { get; set; } = string.Empty;

        [Required(ErrorMessage = "Debe ingresar los nombres.")]
        [StringLength(150)]
        public string Nombres { get; set; } = string.Empty;

        [Required(ErrorMessage = "Debe ingresar los apellidos.")]
        [StringLength(150)]
        public string Apellidos { get; set; } = string.Empty;

        [Required(ErrorMessage = "Debe ingresar el correo.")]
        [EmailAddress(ErrorMessage = "El correo no es válido.")]
        [StringLength(300)]
        public string Correo { get; set; } = string.Empty;

        [Required(ErrorMessage = "Debe ingresar el celular.")]
        [StringLength(30)]
        public string Celular { get; set; } = string.Empty;

        [Required(ErrorMessage = "Debe ingresar el codigo de pais del celular.")]
        [StringLength(8)]
        [RegularExpression(@"^\+\d{1,5}$", ErrorMessage = "El codigo de pais debe tener el formato +51.")]
        public string CodigoPais { get; set; } = "+51";

        [Required(ErrorMessage = "Debe indicar el país.")]
        [StringLength(100)]
        public string Pais { get; set; } = "PERU";

        [StringLength(150)]
        public string? Region { get; set; }

        [Required(ErrorMessage = "Debe indicar la profesión.")]
        [StringLength(150)]
        public string Profesion { get; set; } = string.Empty;

        [StringLength(150)]
        public string? Especialidad { get; set; }

        [Required(ErrorMessage = "Debe indicar la institución de procedencia.")]
        [StringLength(30)]
        public string TipoInstitucion { get; set; } = string.Empty;

        [StringLength(250)]
        public string? NombreInstitucion { get; set; }

        [Required(ErrorMessage = "Debe indicar cómo se enteró del curso.")]
        [StringLength(150)]
        public string MedioComunicacion { get; set; } = string.Empty;

        [StringLength(150)]
        public string? OtroMedioComunicacion { get; set; }

        [StringLength(50)]
        public string? CodigoInsnsb { get; set; }

        [StringLength(30)]
        public string? CondicionLaboralInsnsb { get; set; }

        [Range(0, 999999.99, ErrorMessage = "El costo final calculado no es valido.")]
        public decimal? CostoFinal { get; set; }

        [Range(1, 24, ErrorMessage = "El numero de cuotas debe estar entre 1 y 24.")]
        public int? NumeroCuotas { get; set; }

        public bool AceptaTratamientoDatos { get; set; }

        public bool? AsistiraPresencialPrimerDia { get; set; }
    }

    public sealed class ResultadoRegistroInscripcion
    {
        public bool Exito { get; init; }
        public string Mensaje { get; init; } = string.Empty;
        public long? IdInscripcion { get; init; }
        public string? NombreCurso { get; init; }
        public decimal CostoFinal { get; init; }
        public int NumeroCuotas { get; init; }
        public string EstadoPagoGeneral { get; init; } = "SIN_DEFINIR";
        public bool RequierePago { get; init; }
    }

    public sealed class CuotaPagoProgramada
    {
        public int NumeroCuota { get; init; }
        public decimal Monto { get; init; }
        public DateTime FechaVencimiento { get; init; }
        public string Estado { get; set; } = "PENDIENTE";
        public DateTime? FechaPagoReal { get; set; }
        public string? TokenPagoPasarela { get; set; }
        public string? UrlPagoPasarela { get; set; }
        public long? IdPagoIzipay { get; set; }
        public string? UrlPagoIzipay { get; set; }
        public string? EstadoIzipay { get; set; }
        public string? CodigoAutorizacionIzipay { get; set; }
        public string? NumeroComprobanteIzipay { get; set; }
        public string? UrlComprobantePdfIzipay { get; set; }
        public bool PuedePagarAhora { get; set; }
        public string? BloqueoPagoMensaje { get; set; }
    }

    public sealed class InscripcionAlumnoResumen
    {
        public long IdInscripcion { get; init; }
        public long CursoId { get; init; }
        public string NombreCurso { get; init; } = string.Empty;
        public string NumeroDocumento { get; init; } = string.Empty;
        public string Nombres { get; init; } = string.Empty;
        public string Apellidos { get; init; } = string.Empty;
        public string Correo { get; init; } = string.Empty;
        public string Celular { get; init; } = string.Empty;
        public string TipoInstitucion { get; init; } = string.Empty;
        public string? NombreInstitucion { get; init; }
        public DateTime FechaRegistro { get; init; }
    }

    public sealed class SeguimientoAlumnoEstado
    {
        public long IdInscripcion { get; init; }
        public long CursoId { get; init; }
        public string NumeroDocumento { get; init; } = string.Empty;
        public string NombreCurso { get; init; } = string.Empty;
        public string Nombres { get; init; } = string.Empty;
        public string Apellidos { get; init; } = string.Empty;
        public string Correo { get; init; } = string.Empty;
        public string Celular { get; init; } = string.Empty;
        public DateTime FechaRegistro { get; init; }
        public string TipoCurso { get; init; } = "GRATUITO";
        public decimal CostoFinal { get; init; }
        public int NumeroCuotas { get; init; }
        public string EstadoGeneral { get; set; } = "PENDIENTE";
        public int CuotasPendientes { get; set; }
        public List<CuotaPagoProgramada> Cuotas { get; init; } = [];
    }

    public sealed class HistorialCursoAlumnoItem
    {
        public long IdInscripcion { get; init; }
        public long CursoId { get; init; }
        public string NombreCurso { get; init; } = string.Empty;
        public DateTime FechaRegistro { get; init; }
        public string TipoCurso { get; init; } = "GRATUITO";
        public decimal CostoFinal { get; init; }
        public int NumeroCuotas { get; init; }
        public string EstadoPagoGeneral { get; init; } = "PAGADO";
    }

    public sealed class SeguimientoAlumnoDashboard
    {
        public string NumeroDocumento { get; init; } = string.Empty;
        public string Nombres { get; init; } = string.Empty;
        public string Apellidos { get; init; } = string.Empty;
        public string Correo { get; init; } = string.Empty;
        public string Celular { get; init; } = string.Empty;
        public IReadOnlyList<SeguimientoAlumnoEstado> CursosPagados { get; init; } = [];
        public IReadOnlyList<HistorialCursoAlumnoItem> HistorialCursos { get; init; } = [];
    }

    public sealed class SeguimientoPortalViewModel
    {
        public string? MensajeError { get; init; }
        public string? MensajeInfo { get; init; }
        public string? DniConsultado { get; init; }
        public string? CorreoConsultado { get; init; }
        public bool MostrarDashboard => Dashboard is not null;
        public SeguimientoAlumnoDashboard? Dashboard { get; init; }
    }

    public sealed class SolicitudCodigoSeguimientoInput
    {
        [Required(ErrorMessage = "Ingrese su documento de identidad.")]
        [StringLength(20)]
        [RegularExpression(@"^[A-Za-z0-9-]{3,20}$", ErrorMessage = "Ingrese un documento valido de 3 a 20 caracteres.")]
        public string DniAlumno { get; set; } = string.Empty;

        [Required(ErrorMessage = "Ingrese su correo registrado.")]
        [EmailAddress(ErrorMessage = "Ingrese un correo valido.")]
        [StringLength(300)]
        public string Correo { get; set; } = string.Empty;
    }

    public sealed class ValidacionCodigoSeguimientoInput
    {
        [Required(ErrorMessage = "Ingrese su documento de identidad.")]
        [StringLength(20)]
        [RegularExpression(@"^[A-Za-z0-9-]{3,20}$", ErrorMessage = "Ingrese un documento valido de 3 a 20 caracteres.")]
        public string DniAlumno { get; set; } = string.Empty;

        [Required(ErrorMessage = "Ingrese su correo registrado.")]
        [EmailAddress(ErrorMessage = "Ingrese un correo valido.")]
        [StringLength(300)]
        public string Correo { get; set; } = string.Empty;

        [Required(ErrorMessage = "Ingrese el codigo de verificacion.")]
        [StringLength(6, MinimumLength = 6, ErrorMessage = "El codigo de verificacion debe tener 6 digitos.")]
        public string Codigo { get; set; } = string.Empty;
    }

    public sealed class PagoDemoViewModel
    {
        public string? MensajeError { get; init; }
        public bool PagoConfirmado { get; init; }
        public long IdInscripcion { get; init; }
        public long CursoId { get; init; }
        public string NombreCurso { get; init; } = string.Empty;
        public string Nombres { get; init; } = string.Empty;
        public string Apellidos { get; init; } = string.Empty;
        public string NombreAlumno => $"{Nombres} {Apellidos}".Trim();
        public string NumeroDocumento { get; init; } = string.Empty;
        public string TipoDocumento { get; init; } = string.Empty;
        public string Correo { get; init; } = string.Empty;
        public string Celular { get; init; } = string.Empty;
        public string? Pais { get; init; }
        public string? Region { get; init; }
        public string? CodigoPais { get; init; }
        public string? CodigoCentroCosto { get; init; }
        public int NumeroCuotasTotal { get; init; }
        public int NumeroCuota { get; init; }
        public decimal Monto { get; init; }
        public DateTime FechaVencimiento { get; init; }
        public string Estado { get; init; } = string.Empty;
        public DateTime? FechaPagoReal { get; init; }
        public string Token { get; init; } = string.Empty;
        public long? IdPagoIzipay { get; init; }
        public string? UrlPagoIzipay { get; init; }
        public string? EstadoIzipay { get; init; }
        public string? CodigoAutorizacionIzipay { get; init; }
        public string? NumeroComprobanteIzipay { get; init; }
        public string? UrlComprobantePdfIzipay { get; init; }
        public string? VolverUrl { get; init; }
        public bool PuedePagarAhora { get; init; }
        public string? BloqueoPagoMensaje { get; init; }
    }

    public sealed class ReservaPagoIzipayResultado
    {
        public string EstadoReserva { get; init; } = "NO_DISPONIBLE";
        public PagoDemoViewModel? Pago { get; init; }
        public bool ReservadoParaCrear => string.Equals(EstadoReserva, "RESERVADO", StringComparison.OrdinalIgnoreCase);
        public bool EnProceso => string.Equals(EstadoReserva, "EN_PROCESO", StringComparison.OrdinalIgnoreCase);
    }

    public sealed class AdminSeguimientoPagoAlumnoItem
    {
        public long IdInscripcion { get; init; }
        public long CursoId { get; init; }
        public string NombreCurso { get; init; } = string.Empty;
        public string NumeroDocumento { get; init; } = string.Empty;
        public string Nombres { get; init; } = string.Empty;
        public string Apellidos { get; init; } = string.Empty;
        public string Correo { get; init; } = string.Empty;
        public string Celular { get; init; } = string.Empty;
        public DateTime FechaRegistro { get; init; }
        public decimal CostoFinal { get; init; }
        public int NumeroCuotas { get; init; }
        public int CuotasPagadas { get; set; }
        public int CuotasPendientes { get; set; }
        public decimal MontoPagado { get; set; }
        public decimal MontoPendiente { get; set; }
        public string EstadoPagoGeneral { get; set; } = "PENDIENTE";
        public List<CuotaPagoProgramada> Cuotas { get; init; } = [];
    }

    public sealed class AdminSeguimientoPagosResumen
    {
        public int TotalInscripciones { get; init; }
        public int ConDeudaPendiente { get; init; }
        public int CompletamentePagados { get; init; }
        public decimal MontoPagado { get; init; }
        public decimal MontoPendiente { get; init; }
        public IReadOnlyList<AdminSeguimientoPagoAlumnoItem> Alumnos { get; init; } = [];
    }

    public sealed class LoginAdminViewModel
    {
        [Required(ErrorMessage = "Ingrese el usuario del panel.")]
        public string Usuario { get; set; } = string.Empty;

        [Required(ErrorMessage = "Ingrese la contraseña del panel.")]
        [DataType(DataType.Password)]
        public string Clave { get; set; } = string.Empty;

        public string? MensajeError { get; set; }
    }

    public sealed class GeneracionCodigosInput
    {
        [Range(1, long.MaxValue, ErrorMessage = "Debe seleccionar un curso.")]
        public long CursoId { get; set; }

        [Required(ErrorMessage = "Debe registrar el documento del personal INSNSB.")]
        [StringLength(20)]
        [RegularExpression(@"^[A-Za-z0-9-]{3,20}$", ErrorMessage = "El documento asignado debe tener entre 3 y 20 caracteres alfanumericos.")]
        public string DniAsignado { get; set; } = string.Empty;

        [StringLength(120)]
        public string? GeneradoPor { get; set; }

        [StringLength(250)]
        public string? Observacion { get; set; }
    }

    public sealed class CursoConfiguracionAdminInput
    {
        [Range(1, long.MaxValue, ErrorMessage = "Debe seleccionar un curso.")]
        public long CursoId { get; set; }

        [Required(ErrorMessage = "Debe registrar el nombre del curso.")]
        [StringLength(300)]
        public string NombreActividad { get; set; } = string.Empty;

        [StringLength(2000)]
        public string? TextoDirigidoA { get; set; }

        [StringLength(120)]
        public string? Modalidad { get; set; }

        [StringLength(250)]
        public string? TextoFechasDuracion { get; set; }

        [StringLength(250)]
        public string? HorarioTexto { get; set; }

        [StringLength(250)]
        public string? LugarActividad { get; set; }

        [Range(0, 999999.99, ErrorMessage = "La tarifa general no es valida.")]
        public decimal? CostoBase { get; set; }

        [Range(0, 999999.99, ErrorMessage = "La tarifa INSNSB no es valida.")]
        public decimal? CostoPersonalInsnsb { get; set; }

        [Range(0, 24, ErrorMessage = "El maximo de cuotas debe estar entre 0 y 24.")]
        public int? MaxCuotas { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "El cupo referencial no es valido.")]
        public int? MaxInscritos { get; set; }

        [StringLength(200)]
        public string? OpcionesCuotas { get; set; }

        [StringLength(1000)]
        public string? FechasPagoCuotas { get; set; }

        [StringLength(50)]
        [RegularExpression(@"^[A-Za-z0-9._-]{1,50}$", ErrorMessage = "El codigo CPMS solo puede contener letras, numeros, punto, guion o guion bajo.")]
        public string? CodigoCentroCosto { get; set; }

        [StringLength(30)]
        [RegularExpression(@"^\+?[0-9 ]{6,30}$", ErrorMessage = "El celular debe contener solo numeros, espacios y opcionalmente el signo +.")]
        public string? CelularContactoActividad { get; set; }

        [StringLength(1000)]
        [Url(ErrorMessage = "Ingrese una URL valida para el programa.")]
        public string? UrlProgramaWeb { get; set; }

        [StringLength(1000)]
        [Url(ErrorMessage = "Ingrese una URL valida para el banner.")]
        public string? UrlBannerWeb { get; set; }

        [Required(ErrorMessage = "Debe indicar si el curso seguira visible en el formulario.")]
        [RegularExpression("^(Si|No)$", ErrorMessage = "La visibilidad del formulario debe ser Si o No.")]
        public string VisibleEnFormulario { get; set; } = "Si";

        [Required(ErrorMessage = "Debe indicar si aplica inscripcion unica.")]
        [RegularExpression("^(Si|No)$", ErrorMessage = "La restriccion de inscripcion unica debe ser Si o No.")]
        public string RestriccionInscripcionUnica { get; set; } = "No";

        [Required(ErrorMessage = "Debe indicar si se preguntara asistencia presencial al primer dia.")]
        [RegularExpression("^(Si|No)$", ErrorMessage = "La confirmacion presencial debe ser Si o No.")]
        public string SolicitaConfirmacionPresencialPrimerDia { get; set; } = "No";
    }

    public sealed class CodigoInsnsbAdminItem
    {
        public long IdCodigo { get; set; }
        public string Codigo { get; set; } = string.Empty;
        public string? DniAsignado { get; set; }
        public string Estado { get; set; } = string.Empty;
        public DateTime FechaGeneracion { get; set; }
        public DateTime? FechaUso { get; set; }
        public string? UsadoPorDocumento { get; set; }
        public string? UsadoPorCorreo { get; set; }
        public string? Observacion { get; set; }
    }

    public sealed class ResumenCodigosInsnsb
    {
        public int Total { get; set; }
        public int Disponibles { get; set; }
        public int Usados { get; set; }
        public IReadOnlyList<CodigoInsnsbAdminItem> Codigos { get; init; } = [];
    }

    public sealed class PanelAdminCodigosViewModel
    {
        public IReadOnlyList<CursoPagadoResumen> Cursos { get; init; } = [];
        public long? CursoSeleccionadoId { get; init; }
        public string? CursoSeleccionadoNombre { get; init; }
        public int TotalCodigos { get; init; }
        public int CodigosDisponibles { get; init; }
        public int CodigosUsados { get; init; }
        public IReadOnlyList<CodigoInsnsbAdminItem> Codigos { get; init; } = [];
        public IReadOnlyList<string> CodigosGenerados { get; init; } = [];
        public string? MensajeExito { get; init; }
        public string? MensajeError { get; init; }
        public GeneracionCodigosInput Formulario { get; init; } = new();
        public CursoConfiguracionAdminInput ConfiguracionCurso { get; init; } = new();
        public CursoPagadoResumen? VistaPreviaCurso { get; init; }
        public AdminSeguimientoPagosResumen SeguimientoPagos { get; init; } = new();
    }

    public sealed class AdminCursoConfiguracionViewModel
    {
        public CursoPagadoResumen? CursoSeleccionado { get; init; }
        public CursoConfiguracionAdminInput ConfiguracionCurso { get; init; } = new();
        public CursoPagadoResumen? VistaPreviaCurso { get; init; }
        public string? MensajeExito { get; init; }
        public string? MensajeError { get; init; }
    }

    public sealed class ReporteInscritosCursoResumen
    {
        public long CursoId { get; init; }
        public string NombreCurso { get; init; } = string.Empty;
        public int TotalInscritos { get; init; }
        public int Internos { get; init; }
        public int Externos { get; init; }
        public int Pagados { get; init; }
        public int PagadosParciales { get; init; }
        public int Pendientes { get; init; }
        public int SinCobro { get; init; }
        public bool SolicitaConfirmacionPresencialPrimerDia { get; init; }
        public int AsistiranPresencialPrimerDia { get; init; }
        public int NoAsistiranPresencialPrimerDia { get; init; }
        public int SinRespuestaPresencialPrimerDia { get; init; }
        public decimal MontoPagado { get; init; }
        public decimal MontoDeuda { get; init; }
        public decimal MontoTotal { get; init; }
    }

    public sealed class ReporteInscritosAgrupacionItem
    {
        public string Categoria { get; init; } = string.Empty;
        public int TotalInscritos { get; init; }
        public int Internos { get; init; }
        public int Externos { get; init; }
        public int Pagados { get; init; }
        public int PagadosParciales { get; init; }
        public int Pendientes { get; init; }
        public int SinCobro { get; init; }
        public decimal MontoPagado { get; init; }
        public decimal MontoDeuda { get; init; }
        public decimal MontoTotal { get; init; }
    }

    public sealed class ReporteInscritosCursoDetalle
    {
        public bool CursoTieneCobro { get; set; }
        public ReporteInscritosCursoResumen Resumen { get; init; } = new();
        public IReadOnlyList<ReporteInscritosAgrupacionItem> PorPais { get; init; } = [];
        public IReadOnlyList<ReporteInscritosAgrupacionItem> PorRegion { get; init; } = [];
        public IReadOnlyList<ReporteInscritosAgrupacionItem> PorInstitucion { get; init; } = [];
        public IReadOnlyList<ReporteInscritosAgrupacionItem> PorTipoParticipante { get; init; } = [];
    }

    public sealed class ReporteInscritoGeneralItem
    {
        public long IdInscripcion { get; init; }
        public long CursoId { get; init; }
        public string NombreCurso { get; init; } = string.Empty;
        public string EstadoReportePago { get; init; } = "PENDIENTE";
        public string EstadoReportePagoEtiqueta { get; init; } = "Pendiente";
        public string TipoParticipante { get; init; } = string.Empty;
        public string TipoDocumento { get; init; } = string.Empty;
        public string NumeroDocumento { get; init; } = string.Empty;
        public string Nombres { get; init; } = string.Empty;
        public string Apellidos { get; init; } = string.Empty;
        public string Correo { get; init; } = string.Empty;
        public string CodigoPais { get; init; } = string.Empty;
        public string Celular { get; init; } = string.Empty;
        public string Pais { get; init; } = string.Empty;
        public string Region { get; init; } = string.Empty;
        public string Profesion { get; init; } = string.Empty;
        public string Especialidad { get; init; } = string.Empty;
        public string Institucion { get; init; } = string.Empty;
        public string InstitucionProcedencia { get; init; } = string.Empty;
        public string CondicionLaboralInsnsb { get; init; } = string.Empty;
        public string MedioComunicacion { get; init; } = string.Empty;
        public bool? AsistiraPresencialPrimerDia { get; init; }
        public string AsistiraPresencialPrimerDiaEtiqueta => AsistiraPresencialPrimerDia.HasValue
            ? (AsistiraPresencialPrimerDia.Value ? "Si" : "No")
            : "Sin respuesta";
        public DateTime FechaRegistro { get; init; }
        public decimal CostoFinal { get; init; }
        public int NumeroCuotas { get; init; }
        public int CuotasPagadas { get; init; }
        public int CuotasPendientes { get; init; }
        public decimal MontoPagado { get; init; }
        public decimal MontoPendiente { get; init; }
        public decimal MontoTotal { get; init; }
        public DateTime? FechaUltimoPago { get; init; }
    }

    public sealed class ReporteInscritosViewModel
    {
        public IReadOnlyList<CursoPagadoResumen> Cursos { get; init; } = [];
        public long? CursoSeleccionadoId { get; init; }
        public string? MensajeError { get; init; }
        public ReporteInscritosCursoDetalle? Detalle { get; init; }
    }

    public sealed class ReportePagantesItem
    {
        public long IdInscripcion { get; init; }
        public long CursoId { get; init; }
        public string NombreCurso { get; init; } = string.Empty;
        public string NumeroDocumento { get; init; } = string.Empty;
        public string Nombres { get; init; } = string.Empty;
        public string Apellidos { get; init; } = string.Empty;
        public string Correo { get; init; } = string.Empty;
        public string Celular { get; init; } = string.Empty;
        public string InstitucionProcedencia { get; init; } = string.Empty;
        public bool? AsistiraPresencialPrimerDia { get; init; }
        public string AsistiraPresencialPrimerDiaEtiqueta => AsistiraPresencialPrimerDia.HasValue
            ? (AsistiraPresencialPrimerDia.Value ? "Si" : "No")
            : "Sin respuesta";
        public DateTime FechaRegistro { get; init; }
        public DateTime? FechaUltimoPago { get; init; }
        public decimal CostoFinal { get; init; }
        public int NumeroCuotas { get; init; }
        public int CuotasPagadas { get; init; }
        public decimal MontoPagado { get; init; }
        public decimal MontoPendiente { get; init; }
        public string EstadoPagoGeneral { get; init; } = "PENDIENTE";
        public string EstadoReportePago { get; init; } = "PENDIENTE";
        public string EstadoReportePagoEtiqueta { get; init; } = "Pendiente";
    }

    public sealed class ReportePagantesDetalle
    {
        public long CursoId { get; init; }
        public string NombreCurso { get; init; } = string.Empty;
        public string FiltroEstadoPago { get; init; } = "todos";
        public string Busqueda { get; init; } = string.Empty;
        public int TotalInscritos { get; init; }
        public int TotalPagantes { get; init; }
        public int Pagados { get; init; }
        public int PagadosParciales { get; init; }
        public int Pendientes { get; init; }
        public int SinCobro { get; init; }
        public bool SolicitaConfirmacionPresencialPrimerDia { get; init; }
        public int AsistiranPresencialPrimerDia { get; init; }
        public int NoAsistiranPresencialPrimerDia { get; init; }
        public int SinRespuestaPresencialPrimerDia { get; init; }
        public decimal MontoPagadoTotal { get; init; }
        public decimal MontoPendienteTotal { get; init; }
        public decimal MontoComprometidoTotal { get; init; }
        public IReadOnlyList<ReportePagantesItem> Items { get; init; } = [];
    }

    public sealed class ReportePagantesViewModel
    {
        public IReadOnlyList<CursoPagadoResumen> Cursos { get; init; } = [];
        public long? CursoSeleccionadoId { get; init; }
        public string FiltroEstadoPago { get; init; } = "todos";
        public string Busqueda { get; init; } = string.Empty;
        public string? MensajeError { get; init; }
        public ReportePagantesDetalle? Detalle { get; init; }
    }
}
