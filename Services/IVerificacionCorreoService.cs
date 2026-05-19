namespace CURSO_INTERCULTURALIDAD.Services
{
    public interface IVerificacionCorreoService
    {
        Task<ResultadoVerificacionCorreo> SolicitarCodigoAsync(string correo, string nombreCompleto, CancellationToken cancellationToken = default);
        Task<ResultadoVerificacionCorreo> ValidarCodigoAsync(string correo, string codigo, CancellationToken cancellationToken = default);
    }

    public sealed class ResultadoVerificacionCorreo
    {
        public bool Exito { get; init; }
        public string Mensaje { get; init; } = string.Empty;
        public int StatusCode { get; init; } = 200;
    }
}
