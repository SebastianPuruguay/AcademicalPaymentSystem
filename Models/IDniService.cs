namespace CURSO_INTERCULTURALIDAD.Models
{
    public interface IDniService
    {
        Task<(string nombres, string apellidos)> ConsultarDniAsync(string dni);
    }
}
