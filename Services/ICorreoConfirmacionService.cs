namespace CURSO_INTERCULTURALIDAD.Services
{
    public interface ICorreoConfirmacionService
    {
        Task<ResultadoCorreoConfirmacion> EnviarAsync(
            string correo,
            string nombres,
            string apellidos,
            long cursoId,
            string cursoNombre,
            CancellationToken cancellationToken = default);
    }

    public sealed class ResultadoCorreoConfirmacion
    {
        public bool Exito { get; init; }
        public string Mensaje { get; init; } = string.Empty;
        public int StatusCode { get; init; } = 200;
    }
}
