using System.Collections.Concurrent;
using CURSO_INTERCULTURALIDAD.Models;

namespace CURSO_INTERCULTURALIDAD.Services
{
    public sealed class AlumnoSeguimientoStore : IAlumnoSeguimientoStore
    {
        private readonly ConcurrentDictionary<string, SeguimientoAlumnoEstado> _estadosPorDocumento = new(StringComparer.OrdinalIgnoreCase);

        public void Guardar(SeguimientoAlumnoEstado estado)
        {
            _estadosPorDocumento.AddOrUpdate(
                estado.NumeroDocumento,
                _ => ClonarEstado(estado),
                (_, __) => ClonarEstado(estado));
        }

        public SeguimientoAlumnoEstado? ObtenerPorDocumento(string numeroDocumento)
        {
            return _estadosPorDocumento.TryGetValue(numeroDocumento, out var estado)
                ? ClonarEstado(estado)
                : null;
        }

        public PagoDemoViewModel? ObtenerPagoDemo(string token)
        {
            foreach (var estado in _estadosPorDocumento.Values)
            {
                var cuota = estado.Cuotas.FirstOrDefault(item => string.Equals(item.TokenPagoPasarela, token, StringComparison.OrdinalIgnoreCase));
                if (cuota is null)
                {
                    continue;
                }

                return new PagoDemoViewModel
                {
                    IdInscripcion = estado.IdInscripcion,
                    CursoId = estado.CursoId,
                    NombreCurso = estado.NombreCurso,
                    Nombres = estado.Nombres,
                    Apellidos = estado.Apellidos,
                    NumeroDocumento = estado.NumeroDocumento,
                    TipoDocumento = "DNI",
                    Correo = estado.Correo,
                    Celular = estado.Celular,
                    NumeroCuotasTotal = estado.NumeroCuotas,
                    NumeroCuota = cuota.NumeroCuota,
                    Monto = cuota.Monto,
                    FechaVencimiento = cuota.FechaVencimiento,
                    Estado = cuota.Estado,
                    FechaPagoReal = cuota.FechaPagoReal,
                    Token = token,
                    VolverUrl = $"/InscripcionCIS/Seguimiento?dni={Uri.EscapeDataString(estado.NumeroDocumento)}"
                };
            }

            return null;
        }

        public bool MarcarCuotaComoPagada(string token, out SeguimientoAlumnoEstado? estadoActualizado)
        {
            foreach (var estado in _estadosPorDocumento.Values)
            {
                lock (estado)
                {
                    var cuota = estado.Cuotas.FirstOrDefault(item => string.Equals(item.TokenPagoPasarela, token, StringComparison.OrdinalIgnoreCase));
                    if (cuota is null)
                    {
                        continue;
                    }

                    cuota.Estado = "PAGADO";
                    cuota.FechaPagoReal = DateTime.Now;
                    estado.EstadoGeneral = PagoCursoHelper.CalcularEstadoGeneral(estado.Cuotas, estado.CostoFinal);
                    estadoActualizado = ClonarEstado(estado);
                    return true;
                }
            }

            estadoActualizado = null;
            return false;
        }

        private static SeguimientoAlumnoEstado ClonarEstado(SeguimientoAlumnoEstado estado)
        {
            return new SeguimientoAlumnoEstado
            {
                IdInscripcion = estado.IdInscripcion,
                CursoId = estado.CursoId,
                NumeroDocumento = estado.NumeroDocumento,
                NombreCurso = estado.NombreCurso,
                Nombres = estado.Nombres,
                Apellidos = estado.Apellidos,
                Correo = estado.Correo,
                Celular = estado.Celular,
                CostoFinal = estado.CostoFinal,
                NumeroCuotas = estado.NumeroCuotas,
                EstadoGeneral = estado.EstadoGeneral,
                Cuotas = estado.Cuotas
                    .Select(cuota => new CuotaPagoProgramada
                    {
                        NumeroCuota = cuota.NumeroCuota,
                        Monto = cuota.Monto,
                        FechaVencimiento = cuota.FechaVencimiento,
                        Estado = cuota.Estado,
                        FechaPagoReal = cuota.FechaPagoReal,
                        TokenPagoPasarela = cuota.TokenPagoPasarela,
                        UrlPagoPasarela = cuota.UrlPagoPasarela
                    })
                    .ToList()
            };
        }
    }
}
