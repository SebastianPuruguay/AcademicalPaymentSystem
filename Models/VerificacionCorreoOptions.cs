namespace CURSO_INTERCULTURALIDAD.Models
{
    public sealed class VerificacionCorreoOptions
    {
        public string SolicitarCodigoUrl { get; set; } = string.Empty;
        public string ValidarCodigoUrl { get; set; } = string.Empty;
        public string? ApiKey { get; set; }
    }
}
