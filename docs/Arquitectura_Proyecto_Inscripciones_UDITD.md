# Arquitectura tecnica - Sistema de Inscripciones UDITD

## 1. Resumen ejecutivo

El proyecto `CURSO_INTERCULTURALIDAD` es una aplicacion web ASP.NET Core MVC orientada a gestionar inscripciones de alumnos en cursos, diplomados y actividades academicas de la UDITD/INSNSB. La aplicacion cubre el flujo publico de inscripcion, seguimiento del alumno, generacion de enlaces de pago IziPay, administracion de cursos, codigos internos INSNSB, seguimiento de pagos y reportes exportables.

La solucion esta construida sobre:

- ASP.NET Core MVC con Razor Views.
- .NET `net10.0`.
- SQL Server como persistencia principal.
- Procedimientos almacenados para operaciones criticas de negocio.
- Integraciones HTTP externas para DNI, correo, verificacion por codigo e IziPay.
- Bootstrap, CSS propio y JavaScript vanilla/jQuery para la experiencia de usuario.

La arquitectura es deliberadamente simple: controlador MVC, servicios de dominio/integracion, repositorio SQL y vistas Razor. Esto facilita mantenimiento por un equipo pequeno, sin introducir capas innecesarias.

## 2. Objetivo funcional

El sistema permite:

- Publicar cursos pagados o historicamente pagados.
- Mostrar cursos disponibles en un formulario publico.
- Registrar alumnos internos INSNSB y externos.
- Validar DNI mediante servicio externo.
- Calcular costo final segun condicion del alumno.
- Resolver cuotas permitidas por curso.
- Generar cronograma rigido de pagos.
- Crear enlaces de pago IziPay por cuota.
- Consultar estado de pago IziPay.
- Bloquear pago de cuotas posteriores si no se pago la cuota anterior.
- Permitir seguimiento del alumno mediante correo y codigo de verificacion.
- Administrar cursos, codigos INSNSB, inscritos y pagos desde panel admin.
- Exportar reportes en Excel, Word y PDF/imprimible.

## 3. Estructura de carpetas

```text
Controllers/
  AdminController.cs
  InscripcionCISController.cs
  HomeController.cs

Models/
  CursoInscripcionModels.cs
  PagoIzipayModels.cs
  ApiConfig.cs
  AdminPanelOptions.cs
  CorreoConfirmacionOptions.cs
  VerificacionCorreoOptions.cs
  ConsultaDniRequest.cs

Services/
  CursoInscripcionRepository.cs
  PagoCursoHelper.cs
  PagoIzipayService.cs
  CorreoConfirmacionService.cs
  VerificacionCorreoService.cs
  DniService.cs
  Interfaces...

Views/
  InscripcionCIS/
  Admin/
  Shared/

wwwroot/
  css/
  js/
  lib/

sql/
  PATCH_BDUDITD.sql
  20260518_PATCH_REPORTES_INSCRITOS_PAGOS_SP.sql
  scripts historicos y patches

docs/
  documentacion tecnica, QA y control de cambios
```

## 4. Componentes principales

### 4.1 `InscripcionCISController`

Controlador publico de alumnos.

Responsabilidades:

- Renderizar el formulario publico de inscripcion.
- Registrar inscripciones.
- Calcular y generar cronograma de pagos.
- Enviar correo de confirmacion.
- Solicitar y validar codigo de seguimiento por correo.
- Mostrar seguimiento de cursos y deudas del alumno.
- Iniciar flujo de pago IziPay.
- Consultar DNI.
- Soportar pago demo/local.

Rutas principales:

```text
GET  /InscripcionCIS/Index
POST /InscripcionCIS/Registrar
GET  /InscripcionCIS/Seguimiento
POST /InscripcionCIS/SolicitarCodigoSeguimiento
POST /InscripcionCIS/ValidarCodigoSeguimiento
GET  /InscripcionCIS/IniciarPagoIzipay
GET  /InscripcionCIS/PagoDemo
POST /InscripcionCIS/ConfirmarPagoDemo
POST /InscripcionCIS/ConsultarDni
```

### 4.2 `AdminController`

Controlador administrativo.

Responsabilidades:

- Login/logout admin por sesion.
- Panel de codigos INSNSB y seguimiento de pagos.
- Generacion y eliminacion de codigos.
- Eliminacion controlada de inscripciones.
- Edicion de informacion publica del curso.
- Vista previa antes de guardar cambios del curso.
- Reporte de inscritos.
- Reporte de estado de pagos.
- Exportaciones Excel, Word y PDF/imprimir.

Rutas principales:

```text
GET  /Admin/Login
POST /Admin/Login
POST /Admin/Logout
GET  /Admin/Index
GET  /Admin/EditarCurso
POST /Admin/VistaPreviaCurso
POST /Admin/GuardarConfiguracionCurso
POST /Admin/GenerarCodigos
POST /Admin/EliminarCodigo
POST /Admin/EliminarInscripcion
GET  /Admin/ReporteInscritos
GET  /Admin/ReportePagantes
GET  /Admin/DescargarReporteInscritosExcel
GET  /Admin/DescargarReporteInscritosWord
GET  /Admin/DescargarReporteInscritosGeneralExcel
GET  /Admin/DescargarReportePagantesExcel
GET  /Admin/DescargarReportePagantesWord
GET  /Admin/ReporteInscritosPdf
GET  /Admin/ReportePagantesPdf
```

## 5. Capa de servicios

### 5.1 `CursoInscripcionRepository`

Repositorio central de acceso a SQL Server. Encapsula operaciones de lectura/escritura sobre cursos, inscripciones, codigos INSNSB, deudas, cuotas, reportes y pagos IziPay.

Puntos importantes:

- Usa `Microsoft.Data.SqlClient`.
- Ejecuta procedimientos almacenados para operaciones criticas.
- Algunas operaciones administrativas simples usan SQL parametrizado local.
- Maneja transacciones para registro de inscripcion, generacion de codigos y eliminacion.
- Mapea resultados SQL a modelos fuertemente tipados.

Procedimientos almacenados consumidos:

```text
sp_docencia_obtener_seguimiento_alumno
sp_docencia_obtener_resumen_codigos_insnsb
sp_docencia_obtener_seguimiento_pagos_admin
sp_docencia_obtener_reporte_inscritos_curso
sp_docencia_obtener_reporte_inscritos_general
sp_docencia_obtener_pago_demo_por_token
sp_docencia_guardar_pago_izipay_por_token
sp_docencia_actualizar_estado_pago_izipay
sp_docencia_marcar_cuota_pagada
sp_docencia_verificar_inscripcion_duplicada
sp_docencia_obtener_codigo_insnsb_disponible
sp_docencia_registrar_inscripcion
sp_docencia_registrar_deuda_cuota
sp_docencia_marcar_codigo_insnsb_usado
sp_docencia_generar_codigo_insnsb_asignado
```

### 5.2 `PagoCursoHelper`

Servicio estatico de reglas de negocio de pagos.

Responsabilidades:

- Calcular costo final segun alumno interno/externo.
- Resolver numero de cuotas permitido.
- Leer opciones de cuotas configuradas.
- Generar cronograma rigido.
- Validar fechas de pago.
- Calcular estado general: `PAGADO`, `PAGADO_PARCIAL`, `PENDIENTE`.
- Aplicar regla secuencial: no se paga cuota N si cuota N-1 sigue pendiente.

Reglas clave:

- Si el costo final es `0`, no se generan cuotas.
- Si el curso tiene `opciones_cuotas`, solo se aceptan esas opciones.
- Si el curso tiene fechas configuradas, el numero de cuotas no puede superar el numero de fechas.
- La primera cuota debe vencer hoy o dentro de las proximas 48 horas.
- Ninguna fecha de cuota puede superar la fecha fin del curso.

### 5.3 `PagoIzipayService`

Encapsula la comunicacion HTTP con IziPay.

Responsabilidades:

- Construir payload de creacion de pago.
- Enviar `POST` a endpoint de creacion.
- Guardar `idPagoIziPay` y URL de pago devuelta.
- Consultar estado del pago con `idPagoIziPay`.
- Normalizar celular, pais y tipo de documento.
- Enviar `CodigoCPMS` desde `codigo_centro_costo`.

Campos relevantes del payload:

```text
idReferencia
codigoExterno
tituloPasarela
informacionAdicional
informacionExtra
nombres
apellidos
email
celular
monto
tipoDocumento
nroDocumento
descripcion
CodigoCPMS
apiKey
```

### 5.4 `CorreoConfirmacionService`

Servicio HTTP para enviar correo de confirmacion de inscripcion.

Observacion operativa:

- Actualmente el correo puede enviarse al registrar la inscripcion, no necesariamente despues de confirmar pago IziPay.
- Si se desea enviar solo despues del primer pago, se requiere callback/webhook de IziPay o proceso automatico de conciliacion que consulte estados y dispare correo.

### 5.5 `VerificacionCorreoService`

Servicio HTTP para solicitar y validar codigo de verificacion por correo.

Se usa para acceso al portal de seguimiento del alumno, reemplazando OTP SMS.

### 5.6 `DniService`

Servicio de consulta externa de DNI. Consume configuracion `ApiConfig` y ayuda a autocompletar datos del alumno.

## 6. Modelo de dominio

Los modelos se concentran principalmente en `CursoInscripcionModels.cs`.

Grupos principales:

- Cursos: `CursoPagadoResumen`, `CursoConfiguracionAdminInput`.
- Inscripcion: `FormularioInscripcionCursoInput`, `ResultadoRegistroInscripcion`.
- Pagos: `CuotaPagoProgramada`, `PagoDemoViewModel`.
- Seguimiento alumno: `SeguimientoAlumnoDashboard`, `HistorialCursoAlumnoItem`, `SeguimientoPortalViewModel`.
- Admin: `PanelAdminCodigosViewModel`, `AdminSeguimientoPagoAlumnoItem`, `ResumenCodigosInsnsb`.
- Reportes: `ReporteInscritosCursoDetalle`, `ReporteInscritoGeneralItem`, `ReportePagantesDetalle`.

## 7. Base de datos

### 7.1 Tablas principales

#### `tabla_central`

Tabla de actividades academicas/cursos.

Campos relevantes para esta aplicacion:

```text
id_actividad
nombre_actividad
fecha_inicio
fecha_fin
costo_base
costo_personal_insnsb
se_cobra
max_cuotas
opciones_cuotas
fechas_pago_cuotas
codigo_centro_costo
celular_contacto_actividad
visible_en_formulario
fue_pagado_historico
max_inscritos
url_programa_web
url_banner
```

#### `inscripciones`

Tabla transaccional de alumnos inscritos/pre-inscritos.

Campos relevantes:

```text
id_inscripcion
code_curso
tipo_documento
documento_identidad
nombres
apellidos
correo
codigo_pais
celular
pais
region
profesion
especialidad
institucion
institucion_procedencia
nombre_institucion
condicion_laboral_insnsb
medio_comunicacion
fecha_registro
costo_final
numero_cuotas
```

#### `deudas_cuotas`

Tabla de deuda y cronograma de pagos por inscripcion.

Campos relevantes:

```text
id_deuda_cuota
id_inscripcion
numero_cuota
monto
fecha_vencimiento
estado
fecha_pago_real
token_pago_pasarela
id_pago_izipay
url_pago_izipay
estado_izipay
cip_izipay
numero_orden_izipay
numero_transaccion_izipay
codigo_autorizacion_izipay
numero_comprobante_izipay
url_comprobante_pdf_izipay
```

#### `codigos_insnsb_inscripcion`

Tabla de codigos generados por admin para personal INSNSB.

Campos relevantes:

```text
id_codigo
id_actividad
codigo
dni_asignado
estado
fecha_generacion
fecha_uso
usado_por_documento
usado_por_correo
id_inscripcion
observacion
activo
```

### 7.2 Patches SQL

Scripts relevantes:

```text
sql/PATCH_BDUDITD.sql
sql/20260429_PATCH_UNICO_BDUDITD_INSCRIPCIONES_TABLAS_SPS.sql
sql/20260518_PATCH_REPORTES_INSCRITOS_PAGOS_SP.sql
sql/LIMPIAR_MOVIMIENTOS.sql
```

`20260518_PATCH_REPORTES_INSCRITOS_PAGOS_SP.sql` es un patch especifico solo para reportes. No modifica tablas ni datos; solo crea/actualiza:

```text
sp_docencia_obtener_reporte_inscritos_curso
sp_docencia_obtener_reporte_inscritos_general
```

## 8. Flujo de inscripcion

1. Alumno ingresa al formulario publico.
2. Selecciona curso.
3. Completa datos personales, documento, correo, celular, pais, region, profesion, institucion y condicion.
4. Si es personal INSNSB, valida codigo generado por admin para su documento.
5. El backend calcula costo final:
   - Externo: `costo_base`.
   - INSNSB: `costo_personal_insnsb`.
6. Si costo final es `0`, se registra inscripcion sin cuotas.
7. Si costo final es mayor a `0`, se valida numero de cuotas permitido.
8. Se genera cronograma rigido de pagos.
9. Se registra inscripcion y deudas en BD.
10. Se envia correo de confirmacion.
11. Para cursos pagados, el sistema puede redirigir al pago de primera/unica cuota.

## 9. Flujo de pagos

1. Cada cuota tiene token interno `token_pago_pasarela`.
2. El alumno inicia pago desde seguimiento o redireccion post-inscripcion.
3. La app construye solicitud IziPay con datos del alumno, curso y cuota.
4. IziPay devuelve:

```json
{
  "exitoso": true,
  "mensaje": "Pago creado correctamente",
  "urlPago": "...",
  "idPagoIziPay": 18,
  "codigoError": null
}
```

5. La app guarda `id_pago_izipay` y `url_pago_izipay`.
6. El alumno es redirigido a la URL de pago.
7. Al volver o refrescar seguimiento, la app consulta estado IziPay.
8. Si `yaPago = true`, se marca cuota como pagada.
9. Si hay comprobante, se almacenan numero y URL para consulta administrativa.

Estados de cuota:

```text
PENDIENTE
PAGADO
```

Estados generales:

```text
PENDIENTE        -> no pago ninguna cuota
PAGADO_PARCIAL   -> pago una o mas cuotas, pero aun tiene deuda
PAGADO           -> pago completo o curso sin costo
```

## 10. Reglas de reportes

### 10.1 Reporte de inscritos

Objetivo: vista consolidada de alumnos por curso.

Conceptos:

```text
Inscritos     = Pagado + Pagado parcial
Pre-inscritos = Pendiente
Sin cobro     = solo se muestra si existe al menos un alumno con costo S/ 0
```

Desgloses:

- Pais.
- Region.
- Institucion de procedencia.
- Internos vs externos.

Exportaciones:

- Excel.
- Word.
- PDF/imprimir.
- Lista general Excel con columnas ampliadas.

### 10.2 Reporte de estado de pagos

Objetivo: seguimiento operativo de pagos por alumno.

Filtros:

- Todos.
- Pagado.
- Pagado parcial.
- Pendiente.
- Sin cobro, solo si aplica.

Busqueda:

- Documento.
- Nombre/apellidos.
- Correo.
- Celular.
- Institucion.

Columnas clave:

- Alumno.
- Documento.
- Celular.
- Correo.
- Institucion de procedencia.
- Registro.
- Ultimo pago.
- Estado reporte.
- Monto pagado.
- Monto pendiente.
- Cuotas.

## 11. Panel administrativo

El panel admin permite:

- Seleccionar curso.
- Generar codigos INSNSB por documento.
- Ver resumen de codigos disponibles/usados.
- Eliminar codigos.
- Ver seguimiento de alumnos y pagos.
- Expandir detalle de cuotas.
- Eliminar inscripciones con confirmacion.
- Editar configuracion publica del curso.
- Ver vista previa antes de guardar.
- Acceder a reportes.

La autenticacion admin se basa en sesion y credenciales de configuracion. Para produccion se recomienda mover secretos a variables de entorno o un secret manager.

## 12. Configuracion

Las secciones de configuracion principales son:

```json
{
  "ConnectionStrings": {
    "DbUditd": "..."
  },
  "AdminPanel": {
    "Username": "...",
    "Password": "..."
  },
  "ApiConfig": {
    "Usuario": "...",
    "Clave": "...",
    "EndpointToken": "..."
  },
  "CorreoConfirmacion": {
    "Url": "...",
    "ApiKey": "..."
  },
  "VerificacionCorreo": {
    "SolicitarCodigoUrl": "...",
    "ValidarCodigoUrl": "...",
    "ApiKey": "..."
  },
  "PagoIzipay": {
    "CrearPagoUrl": "...",
    "EstadoPagoUrl": "...",
    "ApiKey": "...",
    "PermitirCertificadoInseguro": false
  }
}
```

Importante:

- No versionar credenciales reales.
- No publicar `appsettings.json` con secretos en repositorios externos.
- En produccion, preferir variables de entorno o configuracion protegida del servidor.

## 13. Seguridad

Medidas existentes:

- Sesion HTTP-only para admin.
- Validacion de formularios en modelos.
- Antiforgery token en formularios administrativos.
- Consultas SQL parametrizadas.
- Uso de procedimientos almacenados para flujos criticos.
- Verificacion por codigo de correo para seguimiento.
- No se permite pagar una cuota si la anterior no fue pagada.

Riesgos y mejoras recomendadas:

- Reemplazar credenciales admin estaticas por autenticacion institucional.
- Mover secretos fuera de `appsettings.json`.
- Implementar auditoria para eliminacion de inscripciones/codigos.
- Agregar logging estructurado con correlation id.
- Agregar job de conciliacion automatica IziPay.
- Agregar webhook/callback IziPay para confirmar pagos sin depender de refresco manual.
- Agregar pruebas automatizadas de reglas de cuotas.

## 14. Despliegue

Comandos usados para validar:

```powershell
dotnet build -c Release /p:UseAppHost=false
dotnet publish -c Release /p:UseAppHost=false
```

Salida de publicacion:

```text
bin/Release/net10.0/publish/
```

Recomendaciones:

- Publicar solo artefactos necesarios.
- Excluir `.vs/`, `bin/`, `obj/`, `artifacts/` y ZIPs antiguos del codigo fuente.
- Si se modifica `wwwroot/js/index.js`, verificar cache del navegador y archivos precomprimidos `.gz`/`.br` si el hosting los sirve.
- Ejecutar scripts SQL antes de desplegar codigo que dependa de SPs nuevos.

## 15. Operacion y monitoreo

Escenarios que el equipo debe monitorear:

- Error al crear pago IziPay.
- Curso sin `codigo_centro_costo`.
- Cuotas con fechas insuficientes para las opciones permitidas.
- Alumnos pendientes que nunca pagan primera cuota.
- Diferencias entre estado local y estado real IziPay.
- Fallos en envio de correo de confirmacion.
- Fallos en codigo de verificacion por correo.

## 16. Criterios de calidad funcional

Casos criticos a probar antes de publicar:

1. Alumno externo con curso pagado en una cuota.
2. Alumno externo con curso pagado en varias cuotas permitidas.
3. Alumno INSNSB con codigo valido y costo cero.
4. Alumno INSNSB con codigo invalido.
5. Alumno que intenta pagar cuota 2 sin pagar cuota 1.
6. Creacion exitosa de pago IziPay.
7. Consulta de estado IziPay con `yaPago = true`.
8. Reporte inscritos con pagado, pagado parcial y pendiente.
9. Reporte pagantes filtrando por estado.
10. Lista general Excel de inscritos.
11. Edicion admin de curso y vista previa.
12. Eliminacion admin de codigo e inscripcion con confirmacion.

## 17. Convenciones de estado

Estados de reporte:

```text
Pagado          -> Pago completo
Pagado parcial  -> Pago una o mas cuotas, pero mantiene deuda
Pendiente       -> Sin pago
Sin cobro       -> Costo final igual a 0
```

Estados de sistema:

```text
PAGADO
PAGADO_PARCIAL
PENDIENTE
SIN_COBRO
```

## 18. Recomendaciones senior

### Corto plazo

- Ejecutar patch especifico de reportes en produccion antes de usar las vistas nuevas.
- Validar que todos los cursos pagados tengan `codigo_centro_costo`.
- Validar `opciones_cuotas` y `fechas_pago_cuotas` por curso.
- Rotar credenciales expuestas en archivos de configuracion.

### Mediano plazo

- Crear un servicio de conciliacion programada para IziPay.
- Auditar eventos administrativos.
- Centralizar configuracion sensible fuera del repositorio.
- Agregar pruebas unitarias para `PagoCursoHelper`.
- Agregar pruebas de integracion para `CursoInscripcionRepository`.

### Largo plazo

- Migrar autenticacion admin a identidad institucional.
- Separar capa de reportes en vistas SQL o SPs versionados formalmente.
- Implementar dashboard de conciliacion financiera.
- Incorporar observabilidad con logs estructurados y metricas de pagos.

## 19. Archivos clave para mantenimiento

```text
Controllers/InscripcionCISController.cs
Controllers/AdminController.cs
Services/CursoInscripcionRepository.cs
Services/PagoCursoHelper.cs
Services/PagoIzipayService.cs
Models/CursoInscripcionModels.cs
Views/InscripcionCIS/Index.cshtml
Views/InscripcionCIS/Seguimiento.cshtml
Views/Admin/Index.cshtml
Views/Admin/ReporteInscritos.cshtml
Views/Admin/ReportePagantes.cshtml
wwwroot/js/index.js
wwwroot/js/seguimiento.js
wwwroot/css/index.css
wwwroot/css/admin.css
sql/PATCH_BDUDITD.sql
sql/20260518_PATCH_REPORTES_INSCRITOS_PAGOS_SP.sql
```

## 20. Glosario

```text
INSNSB
  Instituto Nacional de Salud del Nino San Borja.

UDITD
  Unidad responsable de docencia/tecnologia en el contexto de la aplicacion.

IziPay
  Pasarela de pagos institucional.

CodigoCPMS
  Codigo de centro de costo enviado a IziPay desde tabla_central.codigo_centro_costo.

Pre-inscrito
  Alumno registrado con deuda pendiente y sin pagos realizados.

Inscrito
  Alumno con pago completo o pago parcial.

Sin cobro
  Alumno cuyo costo final es 0.
```

