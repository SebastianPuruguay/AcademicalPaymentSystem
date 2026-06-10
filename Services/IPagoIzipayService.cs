using CURSO_INTERCULTURALIDAD.Models;

namespace CURSO_INTERCULTURALIDAD.Services
{
    public interface IPagoIzipayService
    {
        bool EstaConfigurado { get; }
        SolicitudCrearPagoIzipay ConstruirSolicitud(PagoDemoViewModel pago);
        string? ConstruirUrlResumenPago(long idPagoIzipay);
        Task<RespuestaCrearPagoIzipay> CrearPagoAsync(PagoDemoViewModel pago, CancellationToken cancellationToken = default);
        Task<RespuestaEstadoPagoIzipay> ConsultarEstadoAsync(long idPagoIzipay, CancellationToken cancellationToken = default);
    }
}
