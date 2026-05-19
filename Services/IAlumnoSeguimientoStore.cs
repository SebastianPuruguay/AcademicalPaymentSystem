using CURSO_INTERCULTURALIDAD.Models;

namespace CURSO_INTERCULTURALIDAD.Services
{
    public interface IAlumnoSeguimientoStore
    {
        void Guardar(SeguimientoAlumnoEstado estado);
        SeguimientoAlumnoEstado? ObtenerPorDocumento(string numeroDocumento);
        PagoDemoViewModel? ObtenerPagoDemo(string token);
        bool MarcarCuotaComoPagada(string token, out SeguimientoAlumnoEstado? estadoActualizado);
    }
}
