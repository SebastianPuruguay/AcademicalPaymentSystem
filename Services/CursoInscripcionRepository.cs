using System.Data;
using System.Security.Cryptography;
using System.Text.Json;
using CURSO_INTERCULTURALIDAD.Models;
using Microsoft.Data.SqlClient;

namespace CURSO_INTERCULTURALIDAD.Services
{
    public sealed class CursoInscripcionRepository : ICursoInscripcionRepository
    {
        private const string NombreInstitucionInsnsb = "INSTITUTO NACIONAL DE SALUD DEL NINO SAN BORJA";
        private readonly IConfiguration _configuration;
        private readonly ILogger<CursoInscripcionRepository> _logger;

        public CursoInscripcionRepository(IConfiguration configuration, ILogger<CursoInscripcionRepository> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<IReadOnlyList<CursoPagadoResumen>> ObtenerCursosPagadosAsync()
        {
            await using var connection = CreateConnection();
            await connection.OpenAsync();
            return await ObtenerCursosAsync(connection, transaction: null, soloPublicos: true, cursoId: null);
        }

        public async Task<IReadOnlyList<CursoPagadoResumen>> ObtenerCursosAdminAsync()
        {
            await using var connection = CreateConnection();
            await connection.OpenAsync();
            return await ObtenerCursosAsync(connection, transaction: null, soloPublicos: false, cursoId: null);
        }

        public async Task<CursoPagadoResumen?> ObtenerCursoPagadoPorIdAsync(long cursoId)
        {
            await using var connection = CreateConnection();
            await connection.OpenAsync();
            var cursos = await ObtenerCursosAsync(connection, transaction: null, soloPublicos: true, cursoId: cursoId);
            return cursos.FirstOrDefault();
        }

        public async Task GuardarConfiguracionCursoAsync(CursoConfiguracionAdminInput input)
        {
            await using var connection = CreateConnection();
            await connection.OpenAsync();

            var tieneVisibleEnFormulario = await ColumnExistsAsync(connection, "dbo.tabla_central", "visible_en_formulario", transaction: null);
            var tieneFuePagadoHistorico = await ColumnExistsAsync(connection, "dbo.tabla_central", "fue_pagado_historico", transaction: null);

            if (!tieneVisibleEnFormulario || !tieneFuePagadoHistorico)
            {
                throw new InvalidOperationException("Faltan columnas de configuracion del curso. Ejecuta el patch SQL para agregar visible_en_formulario y fue_pagado_historico.");
            }

            await using var command = CreateTextCommand(@"
UPDATE dbo.tabla_central
SET nombre_actividad = @NombreActividad,
    texto_dirigidoa = @TextoDirigidoA,
    modalidad = @Modalidad,
    texto_fechas_duracion = @TextoFechasDuracion,
    horario_texto = @HorarioTexto,
    lugar_actividad = @LugarActividad,
    costo_base = @CostoBase,
    costo_personal_insnsb = @CostoPersonalInsnsb,
    max_cuotas = @MaxCuotas,
    opciones_cuotas = @OpcionesCuotas,
    max_inscritos = @MaxInscritos,
    fechas_pago_cuotas = @FechasPagoCuotas,
    codigo_centro_costo = @CodigoCentroCosto,
    celular_contacto_actividad = @CelularContactoActividad,
    url_banner_web = @UrlBannerWeb,
    url_programa_web = @UrlProgramaWeb,
    restriccion_inscripcion_unica = @RestriccionInscripcionUnica,
    visible_en_formulario = @VisibleEnFormulario,
    fue_pagado_historico = CASE
        WHEN UPPER(LTRIM(RTRIM(ISNULL(fue_pagado_historico, '')))) = 'SI' THEN 'Si'
        WHEN UPPER(LTRIM(RTRIM(ISNULL(se_cobra, '')))) = 'SI' THEN 'Si'
        WHEN ISNULL(@CostoBase, 0) > 0 OR ISNULL(@CostoPersonalInsnsb, 0) > 0 THEN 'Si'
        WHEN EXISTS (
            SELECT 1
            FROM dbo.inscripciones i
            WHERE i.code_curso = @CursoId
              AND ISNULL(i.costo_final, 0) > 0
        ) THEN 'Si'
        WHEN EXISTS (
            SELECT 1
            FROM dbo.deudas_cuotas dc
            INNER JOIN dbo.inscripciones i
                ON i.id_inscripcion = dc.id_inscripcion
            WHERE i.code_curso = @CursoId
        ) THEN 'Si'
        ELSE 'No'
    END
WHERE id_actividad = @CursoId;

SELECT @@ROWCOUNT;",
                connection);

            command.Parameters.Add(new SqlParameter("@CursoId", SqlDbType.BigInt) { Value = input.CursoId });
            command.Parameters.Add(new SqlParameter("@NombreActividad", SqlDbType.NVarChar, 300) { Value = NormalizarTexto(input.NombreActividad, uppercase: false) });
            command.Parameters.Add(new SqlParameter("@TextoDirigidoA", SqlDbType.NVarChar, -1) { Value = DbValue(NormalizarTexto(input.TextoDirigidoA, uppercase: false)) });
            command.Parameters.Add(new SqlParameter("@Modalidad", SqlDbType.NVarChar, 120) { Value = DbValue(NormalizarTexto(input.Modalidad, uppercase: false)) });
            command.Parameters.Add(new SqlParameter("@TextoFechasDuracion", SqlDbType.NVarChar, 250) { Value = DbValue(NormalizarTexto(input.TextoFechasDuracion, uppercase: false)) });
            command.Parameters.Add(new SqlParameter("@HorarioTexto", SqlDbType.NVarChar, 250) { Value = DbValue(NormalizarTexto(input.HorarioTexto, uppercase: false)) });
            command.Parameters.Add(new SqlParameter("@LugarActividad", SqlDbType.NVarChar, 250) { Value = DbValue(NormalizarTexto(input.LugarActividad, uppercase: false)) });
            command.Parameters.Add(new SqlParameter("@CostoBase", SqlDbType.Decimal) { Precision = 10, Scale = 2, Value = DbValue(input.CostoBase) });
            command.Parameters.Add(new SqlParameter("@CostoPersonalInsnsb", SqlDbType.Decimal) { Precision = 10, Scale = 2, Value = DbValue(input.CostoPersonalInsnsb) });
            command.Parameters.Add(new SqlParameter("@MaxCuotas", SqlDbType.Int) { Value = DbValue(input.MaxCuotas) });
            command.Parameters.Add(new SqlParameter("@OpcionesCuotas", SqlDbType.NVarChar, -1) { Value = DbValue(NormalizarTexto(input.OpcionesCuotas, uppercase: false)) });
            command.Parameters.Add(new SqlParameter("@MaxInscritos", SqlDbType.Int) { Value = DbValue(input.MaxInscritos) });
            command.Parameters.Add(new SqlParameter("@FechasPagoCuotas", SqlDbType.NVarChar, -1) { Value = DbValue(NormalizarTexto(input.FechasPagoCuotas, uppercase: false)) });
            command.Parameters.Add(new SqlParameter("@CodigoCentroCosto", SqlDbType.VarChar, 50) { Value = DbValue(NormalizarTexto(input.CodigoCentroCosto, uppercase: false)) });
            command.Parameters.Add(new SqlParameter("@CelularContactoActividad", SqlDbType.NVarChar, 30) { Value = DbValue(NormalizarTexto(input.CelularContactoActividad, uppercase: false)) });
            command.Parameters.Add(new SqlParameter("@UrlBannerWeb", SqlDbType.NVarChar, 1000) { Value = DbValue(NormalizarTexto(input.UrlBannerWeb, uppercase: false)) });
            command.Parameters.Add(new SqlParameter("@UrlProgramaWeb", SqlDbType.NVarChar, 1000) { Value = DbValue(NormalizarTexto(input.UrlProgramaWeb, uppercase: false)) });
            command.Parameters.Add(new SqlParameter("@RestriccionInscripcionUnica", SqlDbType.NVarChar, 2) { Value = NormalizarSiNo(input.RestriccionInscripcionUnica, "No") });
            command.Parameters.Add(new SqlParameter("@VisibleEnFormulario", SqlDbType.NVarChar, 2) { Value = NormalizarSiNo(input.VisibleEnFormulario, "Si") });

            var affectedRows = Convert.ToInt32(await command.ExecuteScalarAsync());
            if (affectedRows != 1)
            {
                throw new InvalidOperationException("No se encontro el curso que intentas actualizar.");
            }
        }

        public async Task<ResultadoRegistroInscripcion> RegistrarInscripcionAsync(
            FormularioInscripcionCursoInput input,
            IReadOnlyList<CuotaPagoProgramada> cuotas)
        {
            await using var connection = CreateConnection();
            await connection.OpenAsync();
            await using var transaction = (SqlTransaction)await connection.BeginTransactionAsync();

            try
            {
                var curso = await ObtenerCursoPagadoAsync(input.CursoId, connection, transaction);
                if (curso is null)
                {
                    await transaction.RollbackAsync();
                    return new ResultadoRegistroInscripcion
                    {
                        Exito = false,
                        Mensaje = "El curso seleccionado no esta disponible para inscripcion."
                    };
                }

                if (curso.RestriccionInscripcionUnica &&
                    await ExisteInscripcionDuplicadaAsync(input.CursoId, input.NumeroDocumento, input.Correo, connection, transaction))
                {
                    await transaction.RollbackAsync();
                    return new ResultadoRegistroInscripcion
                    {
                        Exito = false,
                        Mensaje = "Ya existe una inscripcion registrada con ese documento o correo para este curso."
                    };
                }

                long? codigoId = null;
                if (EsInstitucionInsnsb(input.TipoInstitucion))
                {
                    codigoId = await ObtenerCodigoDisponibleAsync(
                        input.CursoId,
                        input.CodigoInsnsb,
                        input.NumeroDocumento,
                        connection,
                        transaction);

                    if (!codigoId.HasValue)
                    {
                        await transaction.RollbackAsync();
                        return new ResultadoRegistroInscripcion
                        {
                            Exito = false,
                            Mensaje = "El codigo INSNSB no existe, ya fue usado o no corresponde al DNI registrado."
                        };
                    }
                }

                var idInscripcion = await InsertarInscripcionAsync(input, connection, transaction);

                foreach (var cuota in cuotas)
                {
                    await InsertarDeudaCuotaAsync(idInscripcion, cuota, connection, transaction);
                }

                if (codigoId.HasValue)
                {
                    await MarcarCodigoComoUsadoAsync(codigoId.Value, idInscripcion, input.NumeroDocumento, input.Correo, connection, transaction);
                }

                await transaction.CommitAsync();

                var costoFinal = input.CostoFinal ?? 0m;
                return new ResultadoRegistroInscripcion
                {
                    Exito = true,
                    IdInscripcion = idInscripcion,
                    NombreCurso = curso.NombreActividad,
                    Mensaje = "La inscripcion se registro correctamente.",
                    CostoFinal = costoFinal,
                    NumeroCuotas = input.NumeroCuotas ?? 0,
                    EstadoPagoGeneral = cuotas.Count == 0 ? "PAGADO" : "PENDIENTE",
                    RequierePago = costoFinal > 0m
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error registrando inscripcion para el curso {CursoId}", input.CursoId);
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<SeguimientoAlumnoDashboard?> ObtenerSeguimientoAlumnoAsync(string numeroDocumento)
        {
            var documento = NormalizarDocumento(numeroDocumento);
            if (string.IsNullOrWhiteSpace(documento))
            {
                return null;
            }

            await using var connection = CreateConnection();
            await connection.OpenAsync();

            await using var command = CreateStoredProcedure("dbo.sp_docencia_obtener_seguimiento_alumno", connection);
            command.Parameters.Add(new SqlParameter("@NumeroDocumento", SqlDbType.NVarChar, 20) { Value = documento });

            await using var reader = await command.ExecuteReaderAsync();

            var cursosPagados = new List<SeguimientoAlumnoEstado>();
            var historial = new List<HistorialCursoAlumnoItem>();
            string numeroDocumentoAlumno = documento;
            string nombres = string.Empty;
            string apellidos = string.Empty;
            string correo = string.Empty;
            string celular = string.Empty;

            while (await reader.ReadAsync())
            {
                if (string.IsNullOrWhiteSpace(numeroDocumentoAlumno))
                {
                    numeroDocumentoAlumno = GetNullableString(reader, "documento_identidad") ?? numeroDocumentoAlumno;
                }

                if (string.IsNullOrWhiteSpace(nombres))
                {
                    nombres = GetNullableString(reader, "nombres") ?? nombres;
                }

                if (string.IsNullOrWhiteSpace(apellidos))
                {
                    apellidos = GetNullableString(reader, "apellidos") ?? apellidos;
                }

                if (string.IsNullOrWhiteSpace(correo))
                {
                    correo = GetNullableString(reader, "correo") ?? correo;
                }

                if (string.IsNullOrWhiteSpace(celular))
                {
                    celular = GetNullableString(reader, "celular") ?? celular;
                }

                var historialItem = new HistorialCursoAlumnoItem
                {
                    IdInscripcion = reader.GetInt64(reader.GetOrdinal("id_inscripcion")),
                    CursoId = reader.GetInt64(reader.GetOrdinal("code_curso")),
                    NombreCurso = GetNullableString(reader, "nombre_actividad") ?? string.Empty,
                    FechaRegistro = reader.GetDateTime(reader.GetOrdinal("fecha_registro")),
                    TipoCurso = GetNullableString(reader, "tipo_curso") ?? "GRATUITO",
                    CostoFinal = reader.GetDecimal(reader.GetOrdinal("costo_final")),
                    NumeroCuotas = reader.GetInt32(reader.GetOrdinal("numero_cuotas")),
                    EstadoPagoGeneral = GetNullableString(reader, "estado_pago_general") ?? "PAGADO"
                };

                historial.Add(historialItem);

                if (!string.Equals(historialItem.TipoCurso, "PAGADO", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                cursosPagados.Add(new SeguimientoAlumnoEstado
                {
                    IdInscripcion = historialItem.IdInscripcion,
                    CursoId = historialItem.CursoId,
                    NumeroDocumento = numeroDocumentoAlumno,
                    NombreCurso = historialItem.NombreCurso,
                    Nombres = nombres,
                    Apellidos = apellidos,
                    Correo = correo,
                    Celular = celular,
                    FechaRegistro = historialItem.FechaRegistro,
                    TipoCurso = historialItem.TipoCurso,
                    CostoFinal = historialItem.CostoFinal,
                    NumeroCuotas = historialItem.NumeroCuotas,
                    EstadoGeneral = historialItem.EstadoPagoGeneral,
                    CuotasPendientes = reader.GetInt32(reader.GetOrdinal("cuotas_pendientes")),
                    Cuotas = []
                });
            }

            if (historial.Count == 0)
            {
                return null;
            }

            if (await reader.NextResultAsync())
            {
                var cursosPorInscripcion = cursosPagados.ToDictionary(item => item.IdInscripcion);
                while (await reader.ReadAsync())
                {
                    var idInscripcion = reader.GetInt64(reader.GetOrdinal("id_inscripcion"));
                    if (!cursosPorInscripcion.TryGetValue(idInscripcion, out var cursoPago))
                    {
                        continue;
                    }

                    cursoPago.Cuotas.Add(new CuotaPagoProgramada
                    {
                        NumeroCuota = reader.GetInt32(reader.GetOrdinal("numero_cuota")),
                        Monto = reader.GetDecimal(reader.GetOrdinal("monto")),
                        FechaVencimiento = reader.GetDateTime(reader.GetOrdinal("fecha_vencimiento")),
                        Estado = GetNullableString(reader, "estado") ?? "PENDIENTE",
                        FechaPagoReal = GetNullableDateTime(reader, "fecha_pago_real"),
                        TokenPagoPasarela = GetNullableString(reader, "token_pago_pasarela"),
                        IdPagoIzipay = HasColumn(reader, "id_pago_izipay") ? GetNullableInt64(reader, "id_pago_izipay") : null,
                        UrlPagoIzipay = HasColumn(reader, "url_pago_izipay") ? GetNullableString(reader, "url_pago_izipay") : null,
                        EstadoIzipay = HasColumn(reader, "estado_izipay") ? GetNullableString(reader, "estado_izipay") : null,
                        CodigoAutorizacionIzipay = HasColumn(reader, "codigo_autorizacion_izipay") ? GetNullableString(reader, "codigo_autorizacion_izipay") : null,
                        NumeroComprobanteIzipay = HasColumn(reader, "numero_comprobante_izipay") ? GetNullableString(reader, "numero_comprobante_izipay") : null,
                        UrlComprobantePdfIzipay = HasColumn(reader, "url_comprobante_pdf_izipay") ? GetNullableString(reader, "url_comprobante_pdf_izipay") : null
                    });
                }
            }

            foreach (var cursoPago in cursosPagados)
            {
                PagoCursoHelper.AplicarReglaPagoSecuencial(cursoPago.Cuotas);
            }

            return new SeguimientoAlumnoDashboard
            {
                NumeroDocumento = numeroDocumentoAlumno,
                Nombres = nombres,
                Apellidos = apellidos,
                Correo = correo,
                Celular = celular,
                CursosPagados = cursosPagados
                    .OrderByDescending(item => item.FechaRegistro)
                    .ThenByDescending(item => item.IdInscripcion)
                    .ToList(),
                HistorialCursos = historial
                    .OrderByDescending(item => item.FechaRegistro)
                    .ThenByDescending(item => item.IdInscripcion)
                    .ToList()
            };
        }

        public async Task<ResumenCodigosInsnsb> ObtenerResumenCodigosAsync(long cursoId)
        {
            await using var connection = CreateConnection();
            await connection.OpenAsync();

            await using var command = CreateStoredProcedure("dbo.sp_docencia_obtener_resumen_codigos_insnsb", connection);
            command.Parameters.Add(new SqlParameter("@CursoId", SqlDbType.BigInt) { Value = cursoId });

            await using var reader = await command.ExecuteReaderAsync();

            var resumen = new ResumenCodigosInsnsb();
            if (await reader.ReadAsync())
            {
                resumen.Total = Convert.ToInt32(reader.GetValue(reader.GetOrdinal("total")));
                resumen.Disponibles = reader.IsDBNull(reader.GetOrdinal("disponibles")) ? 0 : Convert.ToInt32(reader.GetValue(reader.GetOrdinal("disponibles")));
                resumen.Usados = reader.IsDBNull(reader.GetOrdinal("usados")) ? 0 : Convert.ToInt32(reader.GetValue(reader.GetOrdinal("usados")));
            }

            var codigos = new List<CodigoInsnsbAdminItem>();
            if (await reader.NextResultAsync())
            {
                while (await reader.ReadAsync())
                {
                    codigos.Add(new CodigoInsnsbAdminItem
                    {
                        IdCodigo = reader.GetInt64(reader.GetOrdinal("id_codigo")),
                        Codigo = reader.GetString(reader.GetOrdinal("codigo")),
                        DniAsignado = GetNullableString(reader, "dni_asignado"),
                        Estado = reader.GetString(reader.GetOrdinal("estado")),
                        FechaGeneracion = reader.GetDateTime(reader.GetOrdinal("fecha_generacion")),
                        FechaUso = GetNullableDateTime(reader, "fecha_uso"),
                        UsadoPorDocumento = GetNullableString(reader, "usado_por_documento"),
                        UsadoPorCorreo = GetNullableString(reader, "usado_por_correo"),
                        Observacion = GetNullableString(reader, "observacion")
                    });
                }
            }

            return new ResumenCodigosInsnsb
            {
                Total = resumen.Total,
                Disponibles = resumen.Disponibles,
                Usados = resumen.Usados,
                Codigos = codigos
            };
        }

        public async Task<AdminSeguimientoPagosResumen> ObtenerSeguimientoPagosAdminAsync(long cursoId)
        {
            await using var connection = CreateConnection();
            await connection.OpenAsync();

            await using var command = CreateStoredProcedure("dbo.sp_docencia_obtener_seguimiento_pagos_admin", connection);
            command.Parameters.Add(new SqlParameter("@CursoId", SqlDbType.BigInt) { Value = cursoId });

            await using var reader = await command.ExecuteReaderAsync();

            var alumnos = new List<AdminSeguimientoPagoAlumnoItem>();
            var montoPagadoTotal = 0m;
            var montoPendienteTotal = 0m;

            while (await reader.ReadAsync())
            {
                var item = new AdminSeguimientoPagoAlumnoItem
                {
                    IdInscripcion = reader.GetInt64(reader.GetOrdinal("id_inscripcion")),
                    CursoId = reader.GetInt64(reader.GetOrdinal("code_curso")),
                    NombreCurso = GetNullableString(reader, "nombre_actividad") ?? string.Empty,
                    NumeroDocumento = GetNullableString(reader, "documento_identidad") ?? string.Empty,
                    Nombres = GetNullableString(reader, "nombres") ?? string.Empty,
                    Apellidos = GetNullableString(reader, "apellidos") ?? string.Empty,
                    Correo = GetNullableString(reader, "correo") ?? string.Empty,
                    Celular = GetNullableString(reader, "celular") ?? string.Empty,
                    FechaRegistro = reader.GetDateTime(reader.GetOrdinal("fecha_registro")),
                    CostoFinal = reader.GetDecimal(reader.GetOrdinal("costo_final")),
                    NumeroCuotas = reader.GetInt32(reader.GetOrdinal("numero_cuotas")),
                    CuotasPagadas = reader.GetInt32(reader.GetOrdinal("cuotas_pagadas")),
                    CuotasPendientes = reader.GetInt32(reader.GetOrdinal("cuotas_pendientes")),
                    MontoPagado = reader.GetDecimal(reader.GetOrdinal("monto_pagado")),
                    MontoPendiente = reader.GetDecimal(reader.GetOrdinal("monto_pendiente")),
                    EstadoPagoGeneral = GetNullableString(reader, "estado_pago_general") ?? "PENDIENTE"
                };

                montoPagadoTotal += item.MontoPagado;
                montoPendienteTotal += item.MontoPendiente;
                alumnos.Add(item);
            }

            if (await reader.NextResultAsync())
            {
                var alumnosPorInscripcion = alumnos.ToDictionary(item => item.IdInscripcion);
                while (await reader.ReadAsync())
                {
                    var idInscripcion = reader.GetInt64(reader.GetOrdinal("id_inscripcion"));
                    if (!alumnosPorInscripcion.TryGetValue(idInscripcion, out var alumno))
                    {
                        continue;
                    }

                    alumno.Cuotas.Add(new CuotaPagoProgramada
                    {
                        NumeroCuota = reader.GetInt32(reader.GetOrdinal("numero_cuota")),
                        Monto = reader.GetDecimal(reader.GetOrdinal("monto")),
                        FechaVencimiento = reader.GetDateTime(reader.GetOrdinal("fecha_vencimiento")),
                        Estado = GetNullableString(reader, "estado") ?? "PENDIENTE",
                        FechaPagoReal = GetNullableDateTime(reader, "fecha_pago_real"),
                        TokenPagoPasarela = GetNullableString(reader, "token_pago_pasarela"),
                        IdPagoIzipay = HasColumn(reader, "id_pago_izipay") ? GetNullableInt64(reader, "id_pago_izipay") : null,
                        UrlPagoIzipay = HasColumn(reader, "url_pago_izipay") ? GetNullableString(reader, "url_pago_izipay") : null,
                        EstadoIzipay = HasColumn(reader, "estado_izipay") ? GetNullableString(reader, "estado_izipay") : null,
                        CodigoAutorizacionIzipay = HasColumn(reader, "codigo_autorizacion_izipay") ? GetNullableString(reader, "codigo_autorizacion_izipay") : null,
                        NumeroComprobanteIzipay = HasColumn(reader, "numero_comprobante_izipay") ? GetNullableString(reader, "numero_comprobante_izipay") : null,
                        UrlComprobantePdfIzipay = HasColumn(reader, "url_comprobante_pdf_izipay") ? GetNullableString(reader, "url_comprobante_pdf_izipay") : null
                    });
                }
            }

            foreach (var alumno in alumnos)
            {
                PagoCursoHelper.AplicarReglaPagoSecuencial(alumno.Cuotas);
            }

            return new AdminSeguimientoPagosResumen
            {
                TotalInscripciones = alumnos.Count,
                ConDeudaPendiente = alumnos.Count(item => item.CuotasPendientes > 0),
                CompletamentePagados = alumnos.Count(item => string.Equals(item.EstadoPagoGeneral, "PAGADO", StringComparison.OrdinalIgnoreCase)),
                MontoPagado = montoPagadoTotal,
                MontoPendiente = montoPendienteTotal,
                Alumnos = alumnos
                    .OrderByDescending(item => item.FechaRegistro)
                    .ThenByDescending(item => item.IdInscripcion)
                .ToList()
            };
        }

        public async Task<ReporteInscritosCursoDetalle> ObtenerReporteInscritosCursoAsync(long cursoId)
        {
            await using var connection = CreateConnection();
            await connection.OpenAsync();

            await using var command = CreateStoredProcedure("dbo.sp_docencia_obtener_reporte_inscritos_curso", connection);
            command.Parameters.Add(new SqlParameter("@CursoId", SqlDbType.BigInt) { Value = cursoId });

            await using var reader = await command.ExecuteReaderAsync();

            var resumen = new ReporteInscritosCursoResumen { CursoId = cursoId };
            if (await reader.ReadAsync())
            {
                resumen = new ReporteInscritosCursoResumen
                {
                    CursoId = reader.GetInt64(reader.GetOrdinal("code_curso")),
                    NombreCurso = GetNullableString(reader, "nombre_actividad") ?? string.Empty,
                    TotalInscritos = Convert.ToInt32(reader.GetValue(reader.GetOrdinal("total_inscritos"))),
                    Internos = Convert.ToInt32(reader.GetValue(reader.GetOrdinal("internos"))),
                    Externos = Convert.ToInt32(reader.GetValue(reader.GetOrdinal("externos"))),
                    Pagados = Convert.ToInt32(reader.GetValue(reader.GetOrdinal("pagados"))),
                    PagadosParciales = Convert.ToInt32(reader.GetValue(reader.GetOrdinal("pagados_parciales"))),
                    Pendientes = Convert.ToInt32(reader.GetValue(reader.GetOrdinal("pendientes"))),
                    SinCobro = Convert.ToInt32(reader.GetValue(reader.GetOrdinal("sin_cobro"))),
                    MontoPagado = Convert.ToDecimal(reader.GetValue(reader.GetOrdinal("monto_pagado"))),
                    MontoDeuda = Convert.ToDecimal(reader.GetValue(reader.GetOrdinal("monto_deuda"))),
                    MontoTotal = Convert.ToDecimal(reader.GetValue(reader.GetOrdinal("monto_total")))
                };
            }

            var porPais = new List<ReporteInscritosAgrupacionItem>();
            var porRegion = new List<ReporteInscritosAgrupacionItem>();
            var porInstitucion = new List<ReporteInscritosAgrupacionItem>();
            var porTipoParticipante = new List<ReporteInscritosAgrupacionItem>();

            if (await reader.NextResultAsync())
            {
                porPais = await LeerAgrupacionReporteAsync(reader);
            }

            if (await reader.NextResultAsync())
            {
                porRegion = await LeerAgrupacionReporteAsync(reader);
            }

            if (await reader.NextResultAsync())
            {
                porInstitucion = await LeerAgrupacionReporteAsync(reader);
            }

            if (await reader.NextResultAsync())
            {
                porTipoParticipante = await LeerAgrupacionReporteAsync(reader);
            }

            return new ReporteInscritosCursoDetalle
            {
                Resumen = resumen,
                PorPais = porPais,
                PorRegion = porRegion,
                PorInstitucion = porInstitucion,
                PorTipoParticipante = porTipoParticipante
            };
        }

        public async Task<IReadOnlyList<ReporteInscritoGeneralItem>> ObtenerReporteInscritosGeneralAsync(long cursoId)
        {
            await using var connection = CreateConnection();
            await connection.OpenAsync();

            await using var command = CreateStoredProcedure("dbo.sp_docencia_obtener_reporte_inscritos_general", connection);
            command.Parameters.Add(new SqlParameter("@CursoId", SqlDbType.BigInt) { Value = cursoId });

            var items = new List<ReporteInscritoGeneralItem>();
            await using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                items.Add(new ReporteInscritoGeneralItem
                {
                    IdInscripcion = reader.GetInt64(reader.GetOrdinal("id_inscripcion")),
                    CursoId = reader.GetInt64(reader.GetOrdinal("code_curso")),
                    NombreCurso = GetNullableString(reader, "nombre_actividad") ?? string.Empty,
                    EstadoReportePago = GetNullableString(reader, "estado_reporte_pago") ?? "PENDIENTE",
                    EstadoReportePagoEtiqueta = GetNullableString(reader, "estado_reporte_pago_etiqueta") ?? "Pendiente",
                    TipoParticipante = GetNullableString(reader, "tipo_participante") ?? string.Empty,
                    TipoDocumento = GetNullableString(reader, "tipo_documento") ?? string.Empty,
                    NumeroDocumento = GetNullableString(reader, "documento_identidad") ?? string.Empty,
                    Nombres = GetNullableString(reader, "nombres") ?? string.Empty,
                    Apellidos = GetNullableString(reader, "apellidos") ?? string.Empty,
                    Correo = GetNullableString(reader, "correo") ?? string.Empty,
                    CodigoPais = GetNullableString(reader, "codigo_pais") ?? string.Empty,
                    Celular = GetNullableString(reader, "celular") ?? string.Empty,
                    Pais = GetNullableString(reader, "pais") ?? string.Empty,
                    Region = GetNullableString(reader, "region") ?? string.Empty,
                    Profesion = GetNullableString(reader, "profesion") ?? string.Empty,
                    Especialidad = GetNullableString(reader, "especialidad") ?? string.Empty,
                    Institucion = GetNullableString(reader, "institucion") ?? string.Empty,
                    InstitucionProcedencia = GetNullableString(reader, "institucion_procedencia") ?? string.Empty,
                    CondicionLaboralInsnsb = GetNullableString(reader, "condicion_laboral_insnsb") ?? string.Empty,
                    MedioComunicacion = GetNullableString(reader, "medio_comunicacion") ?? string.Empty,
                    FechaRegistro = reader.GetDateTime(reader.GetOrdinal("fecha_registro")),
                    CostoFinal = Convert.ToDecimal(reader.GetValue(reader.GetOrdinal("costo_final"))),
                    NumeroCuotas = Convert.ToInt32(reader.GetValue(reader.GetOrdinal("numero_cuotas"))),
                    CuotasPagadas = Convert.ToInt32(reader.GetValue(reader.GetOrdinal("cuotas_pagadas"))),
                    CuotasPendientes = Convert.ToInt32(reader.GetValue(reader.GetOrdinal("cuotas_pendientes"))),
                    MontoPagado = Convert.ToDecimal(reader.GetValue(reader.GetOrdinal("monto_pagado"))),
                    MontoPendiente = Convert.ToDecimal(reader.GetValue(reader.GetOrdinal("monto_pendiente"))),
                    MontoTotal = Convert.ToDecimal(reader.GetValue(reader.GetOrdinal("monto_total"))),
                    FechaUltimoPago = GetNullableDateTime(reader, "fecha_ultimo_pago")
                });
            }

            return items;
        }

        public async Task<IReadOnlyList<string>> GenerarCodigosAsync(GeneracionCodigosInput input)
        {
            await using var connection = CreateConnection();
            await connection.OpenAsync();
            await using var transaction = (SqlTransaction)await connection.BeginTransactionAsync();

            try
            {
                var curso = await ObtenerCursoPagadoAsync(input.CursoId, connection, transaction);
                if (curso is null)
                {
                    throw new InvalidOperationException("El curso seleccionado no existe o no esta marcado como actividad pagada.");
                }

                string? codigoGenerado = null;

                for (var intento = 0; intento < 12 && codigoGenerado is null; intento++)
                {
                    var codigo = GenerarCodigo(input.CursoId);

                    try
                    {
                        await InsertarCodigoAsync(input, codigo, connection, transaction);
                        codigoGenerado = codigo;
                    }
                    catch (SqlException ex) when (ex.Number is 2601 or 2627)
                    {
                        _logger.LogWarning("Colision de codigo {Codigo}. Reintentando.", codigo);
                    }
                    catch (SqlException ex) when (ex.Number >= 50000)
                    {
                        throw new InvalidOperationException(ex.Message, ex);
                    }
                }

                if (codigoGenerado is null)
                {
                    throw new InvalidOperationException("No se pudo generar un codigo unico para el DNI indicado. Intente nuevamente.");
                }

                await transaction.CommitAsync();
                return [codigoGenerado];
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generando codigo INSNSB para el curso {CursoId}", input.CursoId);
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task EliminarCodigoInsnsbAsync(long cursoId, long codigoId)
        {
            await using var connection = CreateConnection();
            await connection.OpenAsync();

            await using var command = CreateTextCommand(@"
DELETE FROM dbo.codigos_insnsb_inscripcion
WHERE id_codigo = @CodigoId
  AND id_actividad = @CursoId;",
                connection);

            command.Parameters.Add(new SqlParameter("@CursoId", SqlDbType.BigInt) { Value = cursoId });
            command.Parameters.Add(new SqlParameter("@CodigoId", SqlDbType.BigInt) { Value = codigoId });

            var affectedRows = await command.ExecuteNonQueryAsync();
            if (affectedRows != 1)
            {
                throw new InvalidOperationException("No se encontro el codigo indicado para el curso seleccionado.");
            }
        }

        public async Task EliminarInscripcionAsync(long cursoId, long idInscripcion)
        {
            await using var connection = CreateConnection();
            await connection.OpenAsync();
            await using var transaction = (SqlTransaction)await connection.BeginTransactionAsync();

            try
            {
                await using (var validarCommand = CreateTextCommand(@"
SELECT COUNT(1)
FROM dbo.inscripciones
WHERE id_inscripcion = @IdInscripcion
  AND code_curso = @CursoId;",
                    connection,
                    transaction))
                {
                    validarCommand.Parameters.Add(new SqlParameter("@CursoId", SqlDbType.BigInt) { Value = cursoId });
                    validarCommand.Parameters.Add(new SqlParameter("@IdInscripcion", SqlDbType.BigInt) { Value = idInscripcion });

                    var existe = Convert.ToInt32(await validarCommand.ExecuteScalarAsync()) == 1;
                    if (!existe)
                    {
                        throw new InvalidOperationException("No se encontro la inscripcion indicada para el curso seleccionado.");
                    }
                }

                await using (var limpiarCodigoCommand = CreateTextCommand(@"
UPDATE dbo.codigos_insnsb_inscripcion
SET estado = 'Disponible',
    fecha_uso = NULL,
    usado_por_documento = NULL,
    usado_por_correo = NULL,
    id_inscripcion = NULL
WHERE id_actividad = @CursoId
  AND id_inscripcion = @IdInscripcion;",
                    connection,
                    transaction))
                {
                    limpiarCodigoCommand.Parameters.Add(new SqlParameter("@CursoId", SqlDbType.BigInt) { Value = cursoId });
                    limpiarCodigoCommand.Parameters.Add(new SqlParameter("@IdInscripcion", SqlDbType.BigInt) { Value = idInscripcion });
                    await limpiarCodigoCommand.ExecuteNonQueryAsync();
                }

                await using (var deudasCommand = CreateTextCommand(@"
DELETE FROM dbo.deudas_cuotas
WHERE id_inscripcion = @IdInscripcion;",
                    connection,
                    transaction))
                {
                    deudasCommand.Parameters.Add(new SqlParameter("@IdInscripcion", SqlDbType.BigInt) { Value = idInscripcion });
                    await deudasCommand.ExecuteNonQueryAsync();
                }

                await using (var inscripcionCommand = CreateTextCommand(@"
DELETE FROM dbo.inscripciones
WHERE id_inscripcion = @IdInscripcion
  AND code_curso = @CursoId;",
                    connection,
                    transaction))
                {
                    inscripcionCommand.Parameters.Add(new SqlParameter("@CursoId", SqlDbType.BigInt) { Value = cursoId });
                    inscripcionCommand.Parameters.Add(new SqlParameter("@IdInscripcion", SqlDbType.BigInt) { Value = idInscripcion });

                    var affectedRows = await inscripcionCommand.ExecuteNonQueryAsync();
                    if (affectedRows != 1)
                    {
                        throw new InvalidOperationException("No se pudo eliminar la inscripcion indicada.");
                    }
                }

                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<PagoDemoViewModel?> ObtenerPagoDemoAsync(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                return null;
            }

            await using var connection = CreateConnection();
            await connection.OpenAsync();

            await using var command = CreateStoredProcedure("dbo.sp_docencia_obtener_pago_demo_por_token", connection);
            command.Parameters.Add(new SqlParameter("@TokenPagoPasarela", SqlDbType.VarChar, 120) { Value = token.Trim() });

            await using var reader = await command.ExecuteReaderAsync();
            if (!await reader.ReadAsync())
            {
                return null;
            }

            return MapPagoDemo(reader, token.Trim());
        }

        public async Task<PagoDemoViewModel?> ObtenerPagoDemoPorIdPagoIzipayAsync(long idPagoIzipay)
        {
            await using var connection = CreateConnection();
            await connection.OpenAsync();

            await using var command = CreateTextCommand(@"
SELECT TOP (1)
    d.id_inscripcion,
    i.code_curso,
    tc.nombre_actividad,
    tc.codigo_centro_costo,
    i.nombres,
    i.apellidos,
    CAST(ISNULL(i.correo, '') AS VARCHAR(300)) AS correo,
    CAST(ISNULL(i.celular, '') AS NVARCHAR(MAX)) AS celular,
    CAST(ISNULL(i.tipo_documento, '') AS NVARCHAR(MAX)) AS tipo_documento,
    UPPER(LTRIM(RTRIM(i.documento_identidad))) AS documento_identidad,
    CAST(ISNULL(i.pais, '') AS VARCHAR(100)) AS pais,
    CAST(ISNULL(i.region, '') AS NVARCHAR(MAX)) AS region,
    CAST(ISNULL(i.codigo_pais, '') AS NVARCHAR(MAX)) AS codigo_pais,
    ISNULL(i.numero_cuotas, 0) AS numero_cuotas_total,
    d.numero_cuota,
    d.monto,
    d.fecha_vencimiento,
    d.estado,
    d.fecha_pago_real,
    d.token_pago_pasarela,
    d.id_pago_izipay,
    d.url_pago_izipay,
    d.estado_izipay,
    d.codigo_autorizacion_izipay,
    d.numero_comprobante_izipay,
    d.url_comprobante_pdf_izipay,
    CASE
        WHEN d.estado = 'PAGADO' THEN 0
        WHEN EXISTS
        (
            SELECT 1
            FROM dbo.deudas_cuotas prev
            WHERE prev.id_inscripcion = d.id_inscripcion
              AND prev.numero_cuota < d.numero_cuota
              AND prev.estado <> 'PAGADO'
        ) THEN 0
        ELSE 1
    END AS puede_pagar_ahora,
    CASE
        WHEN d.estado = 'PAGADO' THEN NULL
        WHEN EXISTS
        (
            SELECT 1
            FROM dbo.deudas_cuotas prev
            WHERE prev.id_inscripcion = d.id_inscripcion
              AND prev.numero_cuota < d.numero_cuota
              AND prev.estado <> 'PAGADO'
        ) THEN 'Debes pagar primero la cuota anterior.'
        ELSE NULL
    END AS bloqueo_pago_mensaje
FROM dbo.deudas_cuotas d
INNER JOIN dbo.inscripciones i ON i.id_inscripcion = d.id_inscripcion
INNER JOIN dbo.tabla_central tc ON tc.id_actividad = i.code_curso
WHERE d.id_pago_izipay = @IdPagoIzipay;",
                connection);
            command.Parameters.Add(new SqlParameter("@IdPagoIzipay", SqlDbType.BigInt) { Value = idPagoIzipay });

            await using var reader = await command.ExecuteReaderAsync();
            if (!await reader.ReadAsync())
            {
                return null;
            }

            var token = GetNullableString(reader, "token_pago_pasarela") ?? string.Empty;
            return MapPagoDemo(reader, token);
        }

        public async Task GuardarPagoIzipayAsync(string token, RespuestaCrearPagoIzipay resultado)
        {
            if (string.IsNullOrWhiteSpace(token) || !resultado.IdPagoIziPay.HasValue)
            {
                return;
            }

            await using var connection = CreateConnection();
            await connection.OpenAsync();

            await using var command = CreateStoredProcedure("dbo.sp_docencia_guardar_pago_izipay_por_token", connection);
            command.Parameters.Add(new SqlParameter("@TokenPagoPasarela", SqlDbType.VarChar, 120) { Value = token.Trim() });
            command.Parameters.Add(new SqlParameter("@IdPagoIziPay", SqlDbType.BigInt) { Value = resultado.IdPagoIziPay.Value });
            command.Parameters.Add(new SqlParameter("@UrlPago", SqlDbType.NVarChar, 500) { Value = DbValue(resultado.UrlPago) });
            command.Parameters.Add(new SqlParameter("@Mensaje", SqlDbType.NVarChar, 500) { Value = DbValue(resultado.Mensaje) });

            await command.ExecuteNonQueryAsync();
        }

        public async Task<string?> ActualizarEstadoPagoIzipayAsync(RespuestaEstadoPagoIzipay estadoPago)
        {
            if (!estadoPago.IdPagoIziPay.HasValue)
            {
                return null;
            }

            await using var connection = CreateConnection();
            await connection.OpenAsync();

            try
            {
                await using var command = CreateStoredProcedure("dbo.sp_docencia_actualizar_estado_pago_izipay", connection);
                AgregarParametrosActualizarEstadoPagoIzipay(command, estadoPago, incluirComprobante: true);

                var result = await command.ExecuteScalarAsync();
                return result is null || result == DBNull.Value ? null : Convert.ToString(result);
            }
            catch (SqlException ex) when (ex.Number == 8144)
            {
                _logger.LogWarning(ex, "El SP de actualizacion IziPay aún no admite los nuevos campos de comprobante. Se ejecutara modo compatible.");

                await using var command = CreateStoredProcedure("dbo.sp_docencia_actualizar_estado_pago_izipay", connection);
                AgregarParametrosActualizarEstadoPagoIzipay(command, estadoPago, incluirComprobante: false);

                var result = await command.ExecuteScalarAsync();
                return result is null || result == DBNull.Value ? null : Convert.ToString(result);
            }
        }

        public async Task<SeguimientoAlumnoDashboard?> MarcarCuotaComoPagadaAsync(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                return null;
            }

            string? numeroDocumento = null;

            await using (var connection = CreateConnection())
            {
                await connection.OpenAsync();
                await using var transaction = (SqlTransaction)await connection.BeginTransactionAsync();

                try
                {
                    await using var command = CreateStoredProcedure("dbo.sp_docencia_marcar_cuota_pagada", connection, transaction);
                    command.Parameters.Add(new SqlParameter("@TokenPagoPasarela", SqlDbType.VarChar, 120) { Value = token.Trim() });

                    var result = await command.ExecuteScalarAsync();
                    numeroDocumento = result is null ? null : Convert.ToString(result);

                    if (string.IsNullOrWhiteSpace(numeroDocumento))
                    {
                        await transaction.RollbackAsync();
                        return null;
                    }

                    await transaction.CommitAsync();
                }
                catch (SqlException ex) when (ex.Number >= 50000)
                {
                    await transaction.RollbackAsync();
                    throw new InvalidOperationException(ex.Message, ex);
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            }

            return await ObtenerSeguimientoAlumnoAsync(numeroDocumento);
        }

        private SqlConnection CreateConnection()
        {
            var connectionString = _configuration.GetConnectionString("DbUditd");
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new InvalidOperationException("No se configuro la cadena de conexion 'DbUditd'.");
            }

            return new SqlConnection(connectionString);
        }

        private static SqlCommand CreateStoredProcedure(string procedureName, SqlConnection connection, SqlTransaction? transaction = null)
        {
            var command = transaction is null
                ? new SqlCommand(procedureName, connection)
                : new SqlCommand(procedureName, connection, transaction);

            command.CommandType = CommandType.StoredProcedure;
            return command;
        }

        private static SqlCommand CreateTextCommand(string sql, SqlConnection connection, SqlTransaction? transaction = null)
        {
            var command = transaction is null
                ? new SqlCommand(sql, connection)
                : new SqlCommand(sql, connection, transaction);

            command.CommandType = CommandType.Text;
            return command;
        }

        private static async Task<bool> ColumnExistsAsync(SqlConnection connection, string tableName, string columnName, SqlTransaction? transaction)
        {
            await using var command = CreateTextCommand(@"
SELECT CASE
    WHEN EXISTS (
        SELECT 1
        FROM sys.columns
        WHERE object_id = OBJECT_ID(@TableName)
          AND name = @ColumnName
    ) THEN 1
    ELSE 0
END;",
                connection,
                transaction);

            command.Parameters.Add(new SqlParameter("@TableName", SqlDbType.NVarChar, 256) { Value = tableName });
            command.Parameters.Add(new SqlParameter("@ColumnName", SqlDbType.NVarChar, 128) { Value = columnName });

            var result = await command.ExecuteScalarAsync();
            return result is not null && Convert.ToInt32(result) == 1;
        }

        private static async Task<IReadOnlyList<CursoPagadoResumen>> ObtenerCursosAsync(
            SqlConnection connection,
            SqlTransaction? transaction,
            bool soloPublicos,
            long? cursoId)
        {
            var tieneVisibleEnFormulario = await ColumnExistsAsync(connection, "dbo.tabla_central", "visible_en_formulario", transaction);
            var tieneFuePagadoHistorico = await ColumnExistsAsync(connection, "dbo.tabla_central", "fue_pagado_historico", transaction);

            await using var command = CreateTextCommand(
                ConstruirConsultaCursos(soloPublicos, cursoId.HasValue, tieneVisibleEnFormulario, tieneFuePagadoHistorico),
                connection,
                transaction);

            if (cursoId.HasValue)
            {
                command.Parameters.Add(new SqlParameter("@CursoId", SqlDbType.BigInt) { Value = cursoId.Value });
            }

            var cursos = new List<CursoPagadoResumen>();
            await using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                cursos.Add(MapCurso(reader));
            }

            return cursos;
        }

        private static string ConstruirConsultaCursos(
            bool soloPublicos,
            bool filtrarPorCursoId,
            bool tieneVisibleEnFormulario,
            bool tieneFuePagadoHistorico)
        {
            const string EsPagadoActual = @"
(
    UPPER(LTRIM(RTRIM(ISNULL(tc.se_cobra, '')))) = 'SI'
    OR (
        tc.se_cobra IS NULL
        AND (
            ISNULL(tc.costo_base, 0) > 0
            OR ISNULL(tc.costo_personal_insnsb, 0) > 0
        )
    )
)";

            const string TieneTarifasHistoricas = @"
(
    ISNULL(tc.costo_base, 0) > 0
    OR ISNULL(tc.costo_personal_insnsb, 0) > 0
)";

            const string TieneMovimientosPagados = @"
EXISTS (
    SELECT 1
    FROM dbo.inscripciones i
    WHERE i.code_curso = tc.id_actividad
      AND ISNULL(i.costo_final, 0) > 0
)";

            const string TieneCronogramaPagos = @"
EXISTS (
    SELECT 1
    FROM dbo.deudas_cuotas dc
    INNER JOIN dbo.inscripciones i
        ON i.id_inscripcion = dc.id_inscripcion
    WHERE i.code_curso = tc.id_actividad
)";

            var seCobraExpr = $@"
CASE
    WHEN UPPER(LTRIM(RTRIM(ISNULL(tc.se_cobra, '')))) = 'SI' THEN 'Si'
    WHEN UPPER(LTRIM(RTRIM(ISNULL(tc.se_cobra, '')))) = 'NO' THEN 'No'
    WHEN ISNULL(tc.costo_base, 0) > 0 OR ISNULL(tc.costo_personal_insnsb, 0) > 0 THEN 'Si'
    ELSE 'No'
END";

            var visibleExpr = tieneVisibleEnFormulario
                ? @"
CASE
    WHEN UPPER(LTRIM(RTRIM(ISNULL(tc.visible_en_formulario, '')))) = 'NO' THEN 'No'
    ELSE 'Si'
END"
                : "N'Si'";

            var historicoExpr = tieneFuePagadoHistorico
                ? @"
CASE
    WHEN UPPER(LTRIM(RTRIM(ISNULL(tc.fue_pagado_historico, '')))) = 'SI' THEN 'Si'
    WHEN ISNULL(tc.costo_base, 0) > 0 OR ISNULL(tc.costo_personal_insnsb, 0) > 0 THEN 'Si'
    ELSE 'No'
END"
                : $@"
CASE
    WHEN {EsPagadoActual} OR {TieneTarifasHistoricas} OR {TieneMovimientosPagados} OR {TieneCronogramaPagos} THEN 'Si'
    ELSE 'No'
END";

            var condicionWhere = soloPublicos
                ? EsPagadoActual
                : $@"
(
    {EsPagadoActual}
    OR {TieneTarifasHistoricas}
    OR {TieneMovimientosPagados}
    OR {TieneCronogramaPagos}
    {(tieneFuePagadoHistorico ? "OR UPPER(LTRIM(RTRIM(ISNULL(tc.fue_pagado_historico, '')))) = 'SI'" : string.Empty)}
)";

            if (soloPublicos && tieneVisibleEnFormulario)
            {
                condicionWhere += @"
AND (
    tc.visible_en_formulario IS NULL
    OR UPPER(LTRIM(RTRIM(tc.visible_en_formulario))) = 'SI'
)";
            }

            if (filtrarPorCursoId)
            {
                condicionWhere += @"
AND tc.id_actividad = @CursoId";
            }

            return $@"
SELECT
    tc.id_actividad,
    tc.nombre_actividad,
    {seCobraExpr} AS se_cobra,
    {visibleExpr} AS visible_en_formulario,
    {historicoExpr} AS fue_pagado_historico,
    tc.modalidad,
    tc.fecha_inicio,
    tc.fecha_fin,
    tc.texto_fechas_duracion,
    tc.horario_texto,
    tc.lugar_actividad,
    tc.coordinador,
    tc.texto_dirigidoa,
    tc.costo_base,
    tc.costo_personal_insnsb,
    tc.max_cuotas,
    tc.opciones_cuotas,
    tc.max_inscritos,
    tc.fechas_pago_cuotas,
    tc.codigo_centro_costo,
    tc.celular_contacto_actividad,
    tc.url_banner_web,
    tc.url_programa_web,
    tc.restriccion_inscripcion_unica
FROM dbo.tabla_central tc
WHERE {condicionWhere}
ORDER BY tc.id_actividad DESC;";
        }

        private static CursoPagadoResumen MapCurso(SqlDataReader reader)
        {
            return new CursoPagadoResumen
            {
                IdActividad = reader.GetInt64(reader.GetOrdinal("id_actividad")),
                NombreActividad = reader.GetString(reader.GetOrdinal("nombre_actividad")),
                SeCobra = HasColumn(reader, "se_cobra")
                    ? GetNullableString(reader, "se_cobra") ?? "No"
                    : "No",
                VisibleEnFormulario = HasColumn(reader, "visible_en_formulario")
                    ? GetNullableString(reader, "visible_en_formulario") ?? "Si"
                    : "Si",
                FuePagadoHistorico = HasColumn(reader, "fue_pagado_historico")
                    ? GetNullableString(reader, "fue_pagado_historico") ?? "No"
                    : "No",
                Modalidad = GetNullableString(reader, "modalidad"),
                FechaInicio = GetNullableString(reader, "fecha_inicio"),
                FechaFin = GetNullableString(reader, "fecha_fin"),
                TextoFechasDuracion = GetNullableString(reader, "texto_fechas_duracion"),
                HorarioTexto = GetNullableString(reader, "horario_texto"),
                LugarActividad = GetNullableString(reader, "lugar_actividad"),
                Coordinador = GetNullableString(reader, "coordinador"),
                TextoDirigidoA = GetNullableString(reader, "texto_dirigidoa"),
                CostoBase = GetNullableDecimal(reader, "costo_base"),
                CostoPersonalInsnsb = GetNullableDecimal(reader, "costo_personal_insnsb"),
                MaxCuotas = GetNullableInt(reader, "max_cuotas"),
                OpcionesCuotas = ParseOpcionesCuotas(HasColumn(reader, "opciones_cuotas")
                    ? GetNullableString(reader, "opciones_cuotas")
                    : null),
                MaxInscritos = GetNullableInt(reader, "max_inscritos"),
                FechasPagoCuotas = ParseFechasPagoCuotas(HasColumn(reader, "fechas_pago_cuotas")
                    ? GetNullableString(reader, "fechas_pago_cuotas")
                    : null),
                CodigoCentroCosto = HasColumn(reader, "codigo_centro_costo")
                    ? GetNullableString(reader, "codigo_centro_costo")
                    : null,
                CelularContactoActividad = HasColumn(reader, "celular_contacto_actividad")
                    ? GetNullableString(reader, "celular_contacto_actividad")
                    : null,
                UrlBannerWeb = GetNullableString(reader, "url_banner_web"),
                UrlProgramaWeb = GetNullableString(reader, "url_programa_web"),
                RestriccionInscripcionUnica = string.Equals(
                    GetNullableString(reader, "restriccion_inscripcion_unica"),
                    "Si",
                    StringComparison.OrdinalIgnoreCase)
            };
        }

        private static async Task<CursoPagadoResumen?> ObtenerCursoPagadoAsync(long cursoId, SqlConnection connection, SqlTransaction? transaction)
        {
            var cursos = await ObtenerCursosAsync(connection, transaction, soloPublicos: true, cursoId: cursoId);
            return cursos.FirstOrDefault();
        }

        private static async Task<bool> ExisteInscripcionDuplicadaAsync(
            long cursoId,
            string numeroDocumento,
            string correo,
            SqlConnection connection,
            SqlTransaction transaction)
        {
            await using var command = CreateStoredProcedure("dbo.sp_docencia_verificar_inscripcion_duplicada", connection, transaction);
            command.Parameters.Add(new SqlParameter("@CursoId", SqlDbType.BigInt) { Value = cursoId });
            command.Parameters.Add(new SqlParameter("@NumeroDocumento", SqlDbType.NVarChar, 20) { Value = NormalizarDocumento(numeroDocumento) });
            command.Parameters.Add(new SqlParameter("@Correo", SqlDbType.VarChar, 300) { Value = NormalizarTexto(correo) });

            var result = await command.ExecuteScalarAsync();
            return result is not null && Convert.ToInt32(result) == 1;
        }

        private static async Task<long?> ObtenerCodigoDisponibleAsync(
            long cursoId,
            string? codigo,
            string numeroDocumento,
            SqlConnection connection,
            SqlTransaction transaction)
        {
            await using var command = CreateStoredProcedure("dbo.sp_docencia_obtener_codigo_insnsb_disponible", connection, transaction);
            command.Parameters.Add(new SqlParameter("@CursoId", SqlDbType.BigInt) { Value = cursoId });
            command.Parameters.Add(new SqlParameter("@Codigo", SqlDbType.NVarChar, 50) { Value = NormalizarCodigo(codigo) });
            command.Parameters.Add(new SqlParameter("@NumeroDocumento", SqlDbType.NVarChar, 20) { Value = NormalizarDocumento(numeroDocumento) });

            var result = await command.ExecuteScalarAsync();
            return result is null ? null : Convert.ToInt64(result);
        }

        private static async Task<long> InsertarInscripcionAsync(
            FormularioInscripcionCursoInput input,
            SqlConnection connection,
            SqlTransaction transaction)
        {
            await using var command = CreateStoredProcedure("dbo.sp_docencia_registrar_inscripcion", connection, transaction);
            var pais = NormalizarTexto(input.Pais, uppercase: true);
            var tipoInstitucion = NormalizarTexto(input.TipoInstitucion, uppercase: true);
            var codigoPais = NormalizarCodigoPais(input.CodigoPais);
            if (string.IsNullOrWhiteSpace(codigoPais) && string.Equals(pais, "PERU", StringComparison.OrdinalIgnoreCase))
            {
                codigoPais = "+51";
            }

            command.Parameters.Add(new SqlParameter("@CursoId", SqlDbType.BigInt) { Value = input.CursoId });
            command.Parameters.Add(new SqlParameter("@Extranjero", SqlDbType.BigInt) { Value = string.Equals(pais, "PERU", StringComparison.OrdinalIgnoreCase) ? 0 : 1 });
            command.Parameters.Add(new SqlParameter("@TipoDocumento", SqlDbType.NVarChar, 30) { Value = NormalizarTexto(input.TipoDocumento, uppercase: true) });
            command.Parameters.Add(new SqlParameter("@NumeroDocumento", SqlDbType.NVarChar, 20) { Value = NormalizarDocumento(input.NumeroDocumento) });
            command.Parameters.Add(new SqlParameter("@Nombres", SqlDbType.NVarChar, -1) { Value = NormalizarTexto(input.Nombres, uppercase: true) });
            command.Parameters.Add(new SqlParameter("@Apellidos", SqlDbType.NVarChar, -1) { Value = NormalizarTexto(input.Apellidos, uppercase: true) });
            command.Parameters.Add(new SqlParameter("@Correo", SqlDbType.VarChar, 300) { Value = NormalizarTexto(input.Correo) });
            command.Parameters.Add(new SqlParameter("@Pais", SqlDbType.VarChar, 100) { Value = pais });
            command.Parameters.Add(new SqlParameter("@Region", SqlDbType.NVarChar, -1) { Value = DbValue(NormalizarTexto(input.Region, uppercase: true)) });
            command.Parameters.Add(new SqlParameter("@CodigoPais", SqlDbType.NVarChar, -1) { Value = DbValue(codigoPais) });
            command.Parameters.Add(new SqlParameter("@Celular", SqlDbType.NVarChar, -1) { Value = NormalizarTexto(input.Celular) });
            command.Parameters.Add(new SqlParameter("@Profesion", SqlDbType.VarChar, 200) { Value = NormalizarTexto(input.Profesion, uppercase: true) });
            command.Parameters.Add(new SqlParameter("@Especialidad", SqlDbType.NVarChar, -1) { Value = DbValue(NormalizarTexto(input.Especialidad, uppercase: true)) });
            command.Parameters.Add(new SqlParameter("@InstitucionProcedencia", SqlDbType.NVarChar, -1) { Value = TraducirInstitucionProcedencia(tipoInstitucion) });
            command.Parameters.Add(new SqlParameter("@Institucion", SqlDbType.NVarChar, -1) { Value = tipoInstitucion });
            command.Parameters.Add(new SqlParameter("@NombreInstitucion", SqlDbType.NVarChar, -1)
            {
                Value = DbValue(EsInstitucionInsnsb(tipoInstitucion)
                    ? NombreInstitucionInsnsb
                    : NormalizarTexto(input.NombreInstitucion, uppercase: true))
            });
            command.Parameters.Add(new SqlParameter("@CondicionLaboralInsnsb", SqlDbType.NVarChar, 30)
            {
                Value = DbValue(EsInstitucionInsnsb(tipoInstitucion)
                    ? NormalizarTexto(input.CondicionLaboralInsnsb, uppercase: true)
                    : null)
            });
            command.Parameters.Add(new SqlParameter("@MedioComunicacion", SqlDbType.NVarChar, -1) { Value = NormalizarTexto(input.MedioComunicacion, uppercase: true) });
            command.Parameters.Add(new SqlParameter("@OtroMedio", SqlDbType.NVarChar, -1) { Value = DbValue(NormalizarTexto(input.OtroMedioComunicacion, uppercase: true)) });
            command.Parameters.Add(new SqlParameter("@Autoriza", SqlDbType.BigInt) { Value = input.AceptaTratamientoDatos ? 1 : 0 });
            command.Parameters.Add(new SqlParameter("@CostoFinal", SqlDbType.Decimal) { Precision = 10, Scale = 2, Value = input.CostoFinal ?? 0m });
            command.Parameters.Add(new SqlParameter("@NumeroCuotas", SqlDbType.Int) { Value = input.NumeroCuotas ?? 0 });

            var result = await command.ExecuteScalarAsync();
            return Convert.ToInt64(result);
        }

        private static async Task InsertarDeudaCuotaAsync(
            long idInscripcion,
            CuotaPagoProgramada cuota,
            SqlConnection connection,
            SqlTransaction transaction)
        {
            await using var command = CreateStoredProcedure("dbo.sp_docencia_registrar_deuda_cuota", connection, transaction);
            command.Parameters.Add(new SqlParameter("@IdInscripcion", SqlDbType.BigInt) { Value = idInscripcion });
            command.Parameters.Add(new SqlParameter("@NumeroCuota", SqlDbType.Int) { Value = cuota.NumeroCuota });
            command.Parameters.Add(new SqlParameter("@Monto", SqlDbType.Decimal) { Precision = 10, Scale = 2, Value = cuota.Monto });
            command.Parameters.Add(new SqlParameter("@FechaVencimiento", SqlDbType.Date) { Value = cuota.FechaVencimiento.Date });
            command.Parameters.Add(new SqlParameter("@Estado", SqlDbType.VarChar, 20) { Value = NormalizarTexto(cuota.Estado, uppercase: true) });
            command.Parameters.Add(new SqlParameter("@FechaPagoReal", SqlDbType.DateTime) { Value = DbValue(cuota.FechaPagoReal) });
            command.Parameters.Add(new SqlParameter("@TokenPagoPasarela", SqlDbType.VarChar, 120) { Value = DbValue(cuota.TokenPagoPasarela) });

            await command.ExecuteNonQueryAsync();
        }

        private static async Task MarcarCodigoComoUsadoAsync(
            long codigoId,
            long idInscripcion,
            string numeroDocumento,
            string correo,
            SqlConnection connection,
            SqlTransaction transaction)
        {
            await using var command = CreateStoredProcedure("dbo.sp_docencia_marcar_codigo_insnsb_usado", connection, transaction);
            command.Parameters.Add(new SqlParameter("@CodigoId", SqlDbType.BigInt) { Value = codigoId });
            command.Parameters.Add(new SqlParameter("@IdInscripcion", SqlDbType.BigInt) { Value = idInscripcion });
            command.Parameters.Add(new SqlParameter("@NumeroDocumento", SqlDbType.NVarChar, 20) { Value = NormalizarDocumento(numeroDocumento) });
            command.Parameters.Add(new SqlParameter("@Correo", SqlDbType.VarChar, 300) { Value = NormalizarTexto(correo) });

            var result = await command.ExecuteScalarAsync();
            var updated = result is null ? 0 : Convert.ToInt32(result);
            if (updated != 1)
            {
                throw new InvalidOperationException("No se pudo bloquear el codigo INSNSB generado para la inscripcion.");
            }
        }

        private static async Task InsertarCodigoAsync(
            GeneracionCodigosInput input,
            string codigo,
            SqlConnection connection,
            SqlTransaction transaction)
        {
            await using var command = CreateStoredProcedure("dbo.sp_docencia_generar_codigo_insnsb_asignado", connection, transaction);
            command.Parameters.Add(new SqlParameter("@CursoId", SqlDbType.BigInt) { Value = input.CursoId });
            command.Parameters.Add(new SqlParameter("@Codigo", SqlDbType.NVarChar, 50) { Value = codigo });
            command.Parameters.Add(new SqlParameter("@DniAsignado", SqlDbType.NVarChar, 20) { Value = NormalizarDocumento(input.DniAsignado) });
            command.Parameters.Add(new SqlParameter("@GeneradoPor", SqlDbType.NVarChar, 120) { Value = DbValue(input.GeneradoPor) });
            command.Parameters.Add(new SqlParameter("@Observacion", SqlDbType.NVarChar, 250) { Value = DbValue(input.Observacion) });

            await command.ExecuteNonQueryAsync();
        }

        private static void AgregarParametrosActualizarEstadoPagoIzipay(SqlCommand command, RespuestaEstadoPagoIzipay estadoPago, bool incluirComprobante)
        {
            command.Parameters.Add(new SqlParameter("@IdPagoIziPay", SqlDbType.BigInt) { Value = estadoPago.IdPagoIziPay!.Value });
            command.Parameters.Add(new SqlParameter("@Exitoso", SqlDbType.Bit) { Value = estadoPago.Exitoso });
            command.Parameters.Add(new SqlParameter("@Mensaje", SqlDbType.NVarChar, 500) { Value = DbValue(estadoPago.Mensaje) });
            command.Parameters.Add(new SqlParameter("@EstadoGeneral", SqlDbType.Int) { Value = DbValue(estadoPago.EstadoGeneral) });
            command.Parameters.Add(new SqlParameter("@Estado", SqlDbType.VarChar, 30) { Value = DbValue(estadoPago.Estado) });
            command.Parameters.Add(new SqlParameter("@YaPago", SqlDbType.Bit) { Value = estadoPago.YaPago });
            command.Parameters.Add(new SqlParameter("@Cip", SqlDbType.NVarChar, 50) { Value = DbValue(estadoPago.Cip) });
            command.Parameters.Add(new SqlParameter("@NumeroOrden", SqlDbType.NVarChar, 100) { Value = DbValue(estadoPago.NumeroOrden) });
            command.Parameters.Add(new SqlParameter("@NumeroTransaccion", SqlDbType.NVarChar, 120) { Value = DbValue(estadoPago.NumeroTransaccion) });
            command.Parameters.Add(new SqlParameter("@FechaCreacion", SqlDbType.DateTime) { Value = DbValue(estadoPago.FechaCreacion) });

            if (!incluirComprobante)
            {
                return;
            }

            command.Parameters.Add(new SqlParameter("@CodigoAutorizacion", SqlDbType.NVarChar, 50) { Value = DbValue(estadoPago.CodigoAutorizacion) });
            command.Parameters.Add(new SqlParameter("@NumeroComprobante", SqlDbType.NVarChar, 50) { Value = DbValue(estadoPago.NumeroComprobante) });
            command.Parameters.Add(new SqlParameter("@UrlComprobantePdf", SqlDbType.NVarChar, 1000) { Value = DbValue(estadoPago.UrlComprobantePdf) });
        }

        private static async Task<List<ReporteInscritosAgrupacionItem>> LeerAgrupacionReporteAsync(SqlDataReader reader)
        {
            var items = new List<ReporteInscritosAgrupacionItem>();
            while (await reader.ReadAsync())
            {
                items.Add(new ReporteInscritosAgrupacionItem
                {
                    Categoria = GetNullableString(reader, "categoria") ?? "SIN REGISTRO",
                    TotalInscritos = Convert.ToInt32(reader.GetValue(reader.GetOrdinal("total_inscritos"))),
                    Internos = Convert.ToInt32(reader.GetValue(reader.GetOrdinal("internos"))),
                    Externos = Convert.ToInt32(reader.GetValue(reader.GetOrdinal("externos"))),
                    Pagados = Convert.ToInt32(reader.GetValue(reader.GetOrdinal("pagados"))),
                    PagadosParciales = Convert.ToInt32(reader.GetValue(reader.GetOrdinal("pagados_parciales"))),
                    Pendientes = Convert.ToInt32(reader.GetValue(reader.GetOrdinal("pendientes"))),
                    SinCobro = Convert.ToInt32(reader.GetValue(reader.GetOrdinal("sin_cobro"))),
                    MontoPagado = Convert.ToDecimal(reader.GetValue(reader.GetOrdinal("monto_pagado"))),
                    MontoDeuda = Convert.ToDecimal(reader.GetValue(reader.GetOrdinal("monto_deuda"))),
                    MontoTotal = Convert.ToDecimal(reader.GetValue(reader.GetOrdinal("monto_total")))
                });
            }

            return items;
        }

        private static string GenerarCodigo(long cursoId)
        {
            var suffix = Convert.ToHexString(RandomNumberGenerator.GetBytes(4));
            return $"INS-{cursoId % 10000:D4}-{suffix}";
        }

        private static bool EsInstitucionInsnsb(string? tipoInstitucion)
        {
            return string.Equals(tipoInstitucion?.Trim(), "INSNSB", StringComparison.OrdinalIgnoreCase);
        }

        private static string TraducirInstitucionProcedencia(string tipoInstitucion)
        {
            return tipoInstitucion switch
            {
                "INSNSB" => NombreInstitucionInsnsb,
                "IPRESS" => "IPRESS",
                "UNIVERSIDAD" => "UNIVERSIDAD",
                _ => "OTRA ORGANIZACION"
            };
        }

        private static string NormalizarTexto(string? value, bool uppercase = false)
        {
            var sanitized = (value ?? string.Empty).Trim();
            return uppercase ? sanitized.ToUpperInvariant() : sanitized;
        }

        private static string NormalizarSiNo(string? value, string fallback)
        {
            return string.Equals(NormalizarTexto(value), "No", StringComparison.OrdinalIgnoreCase)
                ? "No"
                : string.Equals(NormalizarTexto(value), "Si", StringComparison.OrdinalIgnoreCase)
                    ? "Si"
                    : fallback;
        }

        private static string NormalizarDocumento(string? value)
        {
            return NormalizarTexto(value, uppercase: true);
        }

        private static string NormalizarCodigo(string? value)
        {
            return NormalizarTexto(value, uppercase: true);
        }

        private static string NormalizarCodigoPais(string? value)
        {
            var digits = new string((value ?? string.Empty).Where(char.IsDigit).ToArray());
            return string.IsNullOrWhiteSpace(digits) ? string.Empty : $"+{digits[..Math.Min(digits.Length, 5)]}";
        }

        private static object DbValue(object? value)
        {
            return value switch
            {
                null => DBNull.Value,
                string text when string.IsNullOrWhiteSpace(text) => DBNull.Value,
                string text => text.Trim(),
                _ => value
            };
        }

        private static string? GetNullableString(SqlDataReader reader, string columnName)
        {
            var ordinal = reader.GetOrdinal(columnName);
            return reader.IsDBNull(ordinal) ? null : Convert.ToString(reader.GetValue(ordinal));
        }

        private static DateTime? GetNullableDateTime(SqlDataReader reader, string columnName)
        {
            var ordinal = reader.GetOrdinal(columnName);
            return reader.IsDBNull(ordinal) ? null : reader.GetDateTime(ordinal);
        }

        private static decimal? GetNullableDecimal(SqlDataReader reader, string columnName)
        {
            var ordinal = reader.GetOrdinal(columnName);
            return reader.IsDBNull(ordinal) ? null : Convert.ToDecimal(reader.GetValue(ordinal));
        }

        private static int? GetNullableInt(SqlDataReader reader, string columnName)
        {
            var ordinal = reader.GetOrdinal(columnName);
            return reader.IsDBNull(ordinal) ? null : Convert.ToInt32(reader.GetValue(ordinal));
        }

        private static long? GetNullableInt64(SqlDataReader reader, string columnName)
        {
            var ordinal = reader.GetOrdinal(columnName);
            return reader.IsDBNull(ordinal) ? null : Convert.ToInt64(reader.GetValue(ordinal));
        }

        private static IReadOnlyList<string> ParseFechasPagoCuotas(string? rawValue)
        {
            if (string.IsNullOrWhiteSpace(rawValue))
            {
                return [];
            }

            try
            {
                var fechas = JsonSerializer.Deserialize<List<string>>(rawValue.Trim());
                return fechas?
                    .Where(value => !string.IsNullOrWhiteSpace(value))
                    .Select(value => value.Trim())
                    .ToList() ?? [];
            }
            catch (JsonException)
            {
                return rawValue
                    .Split([',', ';', '|'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                    .ToList();
            }
        }

        private static IReadOnlyList<int> ParseOpcionesCuotas(string? rawValue)
        {
            if (string.IsNullOrWhiteSpace(rawValue))
            {
                return [];
            }

            try
            {
                var opciones = JsonSerializer.Deserialize<List<int>>(rawValue.Trim());
                return NormalizarOpcionesCuotas(opciones);
            }
            catch (JsonException)
            {
                var opciones = rawValue
                    .Split([',', ';', '|'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                    .Select(value => int.TryParse(value, out var opcion) ? opcion : 0);

                return NormalizarOpcionesCuotas(opciones);
            }
        }

        private static IReadOnlyList<int> NormalizarOpcionesCuotas(IEnumerable<int>? opciones)
        {
            return (opciones ?? [])
                .Where(opcion => opcion > 0 && opcion <= 24)
                .Distinct()
                .OrderBy(opcion => opcion)
                .ToList();
        }

        private static bool HasColumn(SqlDataReader reader, string columnName)
        {
            for (var index = 0; index < reader.FieldCount; index++)
            {
                if (string.Equals(reader.GetName(index), columnName, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        private static PagoDemoViewModel MapPagoDemo(SqlDataReader reader, string token)
        {
            return new PagoDemoViewModel
            {
                IdInscripcion = reader.GetInt64(reader.GetOrdinal("id_inscripcion")),
                CursoId = reader.GetInt64(reader.GetOrdinal("code_curso")),
                NombreCurso = GetNullableString(reader, "nombre_actividad") ?? string.Empty,
                Nombres = GetNullableString(reader, "nombres") ?? string.Empty,
                Apellidos = GetNullableString(reader, "apellidos") ?? string.Empty,
                NumeroDocumento = GetNullableString(reader, "documento_identidad") ?? string.Empty,
                TipoDocumento = GetNullableString(reader, "tipo_documento") ?? string.Empty,
                Correo = GetNullableString(reader, "correo") ?? string.Empty,
                Celular = GetNullableString(reader, "celular") ?? string.Empty,
                Pais = GetNullableString(reader, "pais"),
                Region = GetNullableString(reader, "region"),
                CodigoPais = GetNullableString(reader, "codigo_pais"),
                CodigoCentroCosto = HasColumn(reader, "codigo_centro_costo")
                    ? GetNullableString(reader, "codigo_centro_costo")
                    : null,
                NumeroCuotasTotal = reader.GetInt32(reader.GetOrdinal("numero_cuotas_total")),
                NumeroCuota = reader.GetInt32(reader.GetOrdinal("numero_cuota")),
                Monto = reader.GetDecimal(reader.GetOrdinal("monto")),
                FechaVencimiento = reader.GetDateTime(reader.GetOrdinal("fecha_vencimiento")),
                Estado = GetNullableString(reader, "estado") ?? "PENDIENTE",
                FechaPagoReal = GetNullableDateTime(reader, "fecha_pago_real"),
                Token = token,
                IdPagoIzipay = HasColumn(reader, "id_pago_izipay") ? GetNullableInt64(reader, "id_pago_izipay") : null,
                UrlPagoIzipay = HasColumn(reader, "url_pago_izipay") ? GetNullableString(reader, "url_pago_izipay") : null,
                EstadoIzipay = HasColumn(reader, "estado_izipay") ? GetNullableString(reader, "estado_izipay") : null,
                CodigoAutorizacionIzipay = HasColumn(reader, "codigo_autorizacion_izipay") ? GetNullableString(reader, "codigo_autorizacion_izipay") : null,
                NumeroComprobanteIzipay = HasColumn(reader, "numero_comprobante_izipay") ? GetNullableString(reader, "numero_comprobante_izipay") : null,
                UrlComprobantePdfIzipay = HasColumn(reader, "url_comprobante_pdf_izipay") ? GetNullableString(reader, "url_comprobante_pdf_izipay") : null,
                VolverUrl = $"/InscripcionCIS/Seguimiento?dni={Uri.EscapeDataString(GetNullableString(reader, "documento_identidad") ?? string.Empty)}",
                PuedePagarAhora = !reader.IsDBNull(reader.GetOrdinal("puede_pagar_ahora")) && Convert.ToInt32(reader.GetValue(reader.GetOrdinal("puede_pagar_ahora"))) == 1,
                BloqueoPagoMensaje = GetNullableString(reader, "bloqueo_pago_mensaje")
            };
        }
    }
}
