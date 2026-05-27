using CURSO_INTERCULTURALIDAD.Models;

namespace CURSO_INTERCULTURALIDAD.Services
{
    public static class PagoCursoHelper
    {
        public static decimal CalcularCostoFinal(CursoPagadoResumen curso, bool esInsnsb)
        {
            if (curso.EsGratisGlobal)
            {
                return 0m;
            }

            var monto = esInsnsb
                ? (curso.CostoPersonalInsnsb ?? curso.CostoBase ?? 0m)
                : (curso.CostoBase ?? 0m);

            return decimal.Round(monto, 2, MidpointRounding.AwayFromZero);
        }

        public static bool RequierePago(CursoPagadoResumen curso, decimal costoFinal)
        {
            return curso.RequierePago(costoFinal);
        }

        public static bool RequiereCodigoInsnsb(CursoPagadoResumen curso, decimal costoFinal)
        {
            if (!RequierePago(curso, costoFinal))
            {
                return false;
            }

            var costoBase = decimal.Round(curso.CostoBase ?? 0m, 2, MidpointRounding.AwayFromZero);
            var costoPersonalInsnsb = decimal.Round(curso.CostoPersonalInsnsb ?? curso.CostoBase ?? 0m, 2, MidpointRounding.AwayFromZero);

            return costoBase != costoPersonalInsnsb;
        }

        public static int ResolverNumeroCuotas(CursoPagadoResumen curso, decimal costoFinal, int? numeroCuotasSolicitado)
        {
            if (costoFinal <= 0m)
            {
                return 0;
            }

            var opciones = ObtenerOpcionesCuotasDisponibles(curso);

            var numeroCuotas = numeroCuotasSolicitado.GetValueOrDefault();
            if (numeroCuotas <= 0)
            {
                numeroCuotas = opciones.First();
            }

            if (!opciones.Contains(numeroCuotas))
            {
                throw new InvalidOperationException($"Solo se permiten las siguientes opciones de cuotas para este curso: {FormatearOpcionesCuotas(opciones)}.");
            }

            return numeroCuotas;
        }

        public static int ObtenerMaximoCuotasDisponibles(CursoPagadoResumen curso)
        {
            return ObtenerOpcionesCuotasDisponibles(curso).Max();
        }

        public static IReadOnlyList<int> ObtenerOpcionesCuotasDisponibles(CursoPagadoResumen curso)
        {
            var fechasConfiguradas = ObtenerFechasPagoConfiguradas(curso);
            var opcionesConfiguradas = (curso.OpcionesCuotas ?? [])
                .Where(opcion => opcion > 0)
                .Distinct()
                .OrderBy(opcion => opcion)
                .ToList();

            var opciones = opcionesConfiguradas.Count > 0
                ? opcionesConfiguradas
                : Enumerable.Range(1, Math.Max(1, curso.MaxCuotas.GetValueOrDefault(1))).ToList();

            if (fechasConfiguradas.Count > 0)
            {
                opciones = opciones
                    .Where(opcion => opcion <= fechasConfiguradas.Count)
                    .ToList();
            }

            return opciones.Count > 0 ? opciones : [1];
        }

        public static IReadOnlyList<CuotaPagoProgramada> GenerarCronogramaRigido(
            CursoPagadoResumen curso,
            decimal costoFinal,
            int numeroCuotas,
            Func<string>? tokenFactory = null,
            Func<string, string?>? urlFactory = null)
        {
            var cronograma = new List<CuotaPagoProgramada>();
            if (costoFinal <= 0m || numeroCuotas <= 0)
            {
                return cronograma;
            }

            var fechasVencimiento = ResolverFechasVencimiento(curso, numeroCuotas);
            var montoBase = decimal.Round(costoFinal / numeroCuotas, 2, MidpointRounding.AwayFromZero);
            var acumulado = 0m;

            for (var cuota = 1; cuota <= numeroCuotas; cuota++)
            {
                var monto = cuota == numeroCuotas
                    ? decimal.Round(costoFinal - acumulado, 2, MidpointRounding.AwayFromZero)
                    : montoBase;

                acumulado += monto;

                var token = tokenFactory?.Invoke();
                cronograma.Add(new CuotaPagoProgramada
                {
                    NumeroCuota = cuota,
                    Monto = monto,
                    FechaVencimiento = fechasVencimiento[cuota - 1],
                    Estado = "PENDIENTE",
                    TokenPagoPasarela = token,
                    UrlPagoPasarela = token is null ? null : urlFactory?.Invoke(token)
                });
            }

            AplicarReglaPagoSecuencial(cronograma);
            return cronograma;
        }

        public static string CalcularEstadoGeneral(IReadOnlyCollection<CuotaPagoProgramada> cuotas, decimal costoFinal)
        {
            if (costoFinal <= 0m || cuotas.Count == 0)
            {
                return "PAGADO";
            }

            var pagadas = cuotas.Count(cuota => string.Equals(cuota.Estado, "PAGADO", StringComparison.OrdinalIgnoreCase));
            if (pagadas == 0)
            {
                return "PENDIENTE";
            }

            return pagadas == cuotas.Count ? "PAGADO" : "PAGADO_PARCIAL";
        }

        public static IReadOnlyList<DateTime> ObtenerFechasPagoConfiguradas(CursoPagadoResumen curso)
        {
            return (curso.FechasPagoCuotas ?? [])
                .Select(ParseFecha)
                .Where(fecha => fecha.HasValue)
                .Select(fecha => fecha!.Value.Date)
                .OrderBy(fecha => fecha)
                .ToList();
        }

        public static string FormatearOpcionesCuotas(IEnumerable<int> opciones)
        {
            var valores = opciones
                .Where(opcion => opcion > 0)
                .Distinct()
                .OrderBy(opcion => opcion)
                .Select(opcion => opcion.ToString())
                .ToList();

            return valores.Count == 0 ? "1" : string.Join(" o ", valores);
        }

        public static void AplicarReglaPagoSecuencial(IEnumerable<CuotaPagoProgramada> cuotas)
        {
            var cuotaAnteriorPagada = true;

            foreach (var cuota in cuotas.OrderBy(item => item.NumeroCuota))
            {
                var pagada = string.Equals(cuota.Estado, "PAGADO", StringComparison.OrdinalIgnoreCase);
                cuota.PuedePagarAhora = !pagada && cuotaAnteriorPagada;
                cuota.BloqueoPagoMensaje = pagada || cuotaAnteriorPagada
                    ? null
                    : "Debes pagar primero la cuota anterior.";

                if (!pagada)
                {
                    cuotaAnteriorPagada = false;
                }
            }
        }

        private static IReadOnlyList<DateTime> ResolverFechasVencimiento(CursoPagadoResumen curso, int numeroCuotas)
        {
            var fechasConfiguradas = ObtenerFechasPagoConfiguradas(curso);
            if (fechasConfiguradas.Count > 0)
            {
                if (fechasConfiguradas.Count < numeroCuotas)
                {
                    throw new InvalidOperationException("El curso no tiene suficientes fechas de pago configuradas para la cantidad de cuotas solicitada.");
                }

                var fechasSeleccionadas = fechasConfiguradas.Take(numeroCuotas).ToList();
                ValidarFechasRigidas(curso, fechasSeleccionadas);
                return fechasSeleccionadas;
            }

            var generadas = GenerarFechasRigidasFallback(curso, numeroCuotas);
            ValidarFechasRigidas(curso, generadas);
            return generadas;
        }

        private static List<DateTime> GenerarFechasRigidasFallback(CursoPagadoResumen curso, int numeroCuotas)
        {
            var hoy = DateTime.Today;
            var fechaFin = ParseFecha(curso.FechaFin) ?? hoy.AddDays(30 * Math.Max(1, numeroCuotas));
            if (fechaFin < hoy)
            {
                fechaFin = hoy;
            }

            var primeraFecha = hoy.AddDays(2);
            if (primeraFecha > fechaFin)
            {
                primeraFecha = fechaFin;
            }

            var fechas = new List<DateTime> { primeraFecha.Date };
            if (numeroCuotas == 1)
            {
                return fechas;
            }

            var diasDisponibles = Math.Max(0, (fechaFin - primeraFecha).Days);
            var separacion = Math.Max(1, diasDisponibles / Math.Max(1, numeroCuotas - 1));

            for (var cuota = 2; cuota <= numeroCuotas; cuota++)
            {
                var fecha = primeraFecha.AddDays(separacion * (cuota - 1));
                if (cuota == numeroCuotas || fecha > fechaFin)
                {
                    fecha = fechaFin;
                }

                if (fecha < fechas[^1])
                {
                    fecha = fechas[^1];
                }

                fechas.Add(fecha.Date);
            }

            return fechas;
        }

        private static void ValidarFechasRigidas(CursoPagadoResumen curso, IReadOnlyList<DateTime> fechas)
        {
            if (fechas.Count == 0)
            {
                return;
            }

            var hoy = DateTime.Today;
            var fechaMaximaPrimeraCuota = hoy.AddDays(2);
            var fechaFin = ParseFecha(curso.FechaFin);

            if (fechas[0] > fechaMaximaPrimeraCuota)
            {
                throw new InvalidOperationException("La primera cuota debe vencer hoy o dentro de las proximas 48 horas.");
            }

            for (var indice = 0; indice < fechas.Count; indice++)
            {
                if (fechaFin.HasValue && fechas[indice] > fechaFin.Value.Date)
                {
                    throw new InvalidOperationException("Las fechas de pago configuradas no pueden superar la fecha fin del curso.");
                }

                if (indice > 0 && fechas[indice] < fechas[indice - 1])
                {
                    throw new InvalidOperationException("Las fechas de pago deben estar ordenadas de forma ascendente.");
                }
            }
        }

        public static string MascarearCelular(string celular)
        {
            var limpio = new string((celular ?? string.Empty).Where(char.IsDigit).ToArray());
            if (limpio.Length <= 4)
            {
                return limpio;
            }

            return $"{limpio[..2]}***{limpio[^2..]}";
        }

        public static DateTime? ParseFecha(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }

            var formatos = new[]
            {
                "yyyy-MM-dd",
                "yyyy/MM/dd",
                "dd/MM/yyyy",
                "d/M/yyyy",
                "dd-MM-yyyy",
                "d-M-yyyy",
                "MM/dd/yyyy"
            };

            foreach (var formato in formatos)
            {
                if (DateTime.TryParseExact(
                    value.Trim(),
                    formato,
                    System.Globalization.CultureInfo.InvariantCulture,
                    System.Globalization.DateTimeStyles.None,
                    out var fecha))
                {
                    return fecha.Date;
                }
            }

            return DateTime.TryParse(value, out var fechaGenerica)
                ? fechaGenerica.Date
                : null;
        }
    }
}
