using CURSO_INTERCULTURALIDAD.Models;

namespace CURSO_INTERCULTURALIDAD.Services
{
    public interface ICursoInscripcionRepository
    {
        Task<IReadOnlyList<CursoPagadoResumen>> ObtenerCursosPagadosAsync();
        Task<IReadOnlyList<CursoPagadoResumen>> ObtenerCursosAdminAsync();
        Task<CursoPagadoResumen?> ObtenerCursoPagadoPorIdAsync(long cursoId);
        Task GuardarConfiguracionCursoAsync(CursoConfiguracionAdminInput input);
        Task<ResultadoRegistroInscripcion> RegistrarInscripcionAsync(FormularioInscripcionCursoInput input, IReadOnlyList<CuotaPagoProgramada> cuotas);
        Task<SeguimientoAlumnoDashboard?> ObtenerSeguimientoAlumnoAsync(string numeroDocumento);
        Task<ResumenCodigosInsnsb> ObtenerResumenCodigosAsync(long cursoId);
        Task<AdminSeguimientoPagosResumen> ObtenerSeguimientoPagosAdminAsync(long cursoId);
        Task<ReporteInscritosCursoDetalle> ObtenerReporteInscritosCursoAsync(long cursoId);
        Task<IReadOnlyList<ReporteInscritoGeneralItem>> ObtenerReporteInscritosGeneralAsync(long cursoId);
        Task<IReadOnlyList<string>> GenerarCodigosAsync(GeneracionCodigosInput input);
        Task EliminarCodigoInsnsbAsync(long cursoId, long codigoId);
        Task EliminarInscripcionAsync(long cursoId, long idInscripcion);
        Task<PagoDemoViewModel?> ObtenerPagoDemoAsync(string token);
        Task<PagoDemoViewModel?> ObtenerPagoDemoPorIdPagoIzipayAsync(long idPagoIzipay);
        Task<ReservaPagoIzipayResultado> ReservarCreacionPagoIzipayAsync(string token);
        Task GuardarPagoIzipayAsync(string token, RespuestaCrearPagoIzipay resultado);
        Task<string?> ActualizarEstadoPagoIzipayAsync(RespuestaEstadoPagoIzipay estadoPago);
        Task<SeguimientoAlumnoDashboard?> MarcarCuotaComoPagadaAsync(string token);
    }
}
