using CURSO_INTERCULTURALIDAD.Models;

namespace CURSO_INTERCULTURALIDAD.Services
{
    public interface IPagoIzipayService
    {
        bool EstaConfigurado { get; }
        SolicitudCrearPagoIzipay ConstruirSolicitud(PagoDemoViewModel pago);
        Task<RespuestaCrearPagoIzipay> CrearPagoAsync(PagoDemoViewModel pago, CancellationToken cancellationToken = default);
        Task<RespuestaEstadoPagoIzipay> ConsultarEstadoAsync(long idPagoIzipay, CancellationToken cancellationToken = default);
    }
}
