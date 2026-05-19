# Control de Cambios Posteriores al Modulo de Reportes

Fecha de corte: 2026-04-24

Base comparativa: ultima subida a produccion identificada por el equipo, correspondiente al modulo **Reporte de inscritos por curso**.

## 1. Funcionalidades / Modificaciones incluidas - por Modulos

### 1.1 Modulo Admin - Reportes

- QA2025-0001 – Se mantuvo el **reporte de inscritos por curso** dentro del panel admin.
- QA2025-0002 – Se habilito la **descarga del reporte en Excel, Word y PDF / imprimir**.
- QA2025-0003 – El reporte quedo preparado para operar en entorno local sin depender obligatoriamente de un SP adicional de despliegue para ese modulo.

### 1.2 Modulo Admin - Gestion de cursos visibles al alumno

- QA2025-0004 – Se incorporo la posibilidad de **ocultar cursos del formulario publico** sin perderlos del panel admin.
- QA2025-0005 – Se agrego la logica para conservar cursos como **historicos en admin**, aun si ya no deben mostrarse al alumno.
- QA2025-0006 – Se implemento una pantalla dedicada de **edicion de ficha publica del curso**, con administracion de:
  - nombre del curso
  - texto introductorio
  - modalidad
  - fechas / duracion
  - horario
  - lugar
  - tarifa general
  - costo INSNSB para la logica interna
  - cupo referencial
  - opciones de cuotas
  - fechas de cuotas
  - codigo CPMS
  - celular de contacto por actividad
  - URL del programa
  - URL del banner
  - visibilidad en formulario
  - restriccion de inscripcion unica
- QA2025-0007 – La edicion ya no se muestra dentro del dashboard principal, sino en una **pagina separada** para no sobrecargar la operacion diaria.
- QA2025-0008 – Se agrego una **vista previa** antes de guardar, reutilizando el formato del formulario publico.

### 1.3 Modulo Admin - Seguimiento de pagos

- QA2025-0009 – El seguimiento de pagos en admin ahora incluye:
  - **buscador** por alumno, correo, documento, celular, estado y curso
  - **orden por columnas** desde los encabezados de la tabla
  - mantenimiento del boton **Mas detalle** para revisar cuotas
- QA2025-0010 – Se mantiene la consulta de estado de cuotas e integracion con IziPay.
- QA2025-0011 – El admin conserva la opcion de **ver el comprobante PDF** cuando IziPay ya devolvio `url_comprobante_pdf_izipay`, como canal de contingencia si el correo al alumno falla.

### 1.4 Modulo Alumno - Seguimiento y pagos

- QA2025-0012 – El acceso a seguimiento sigue funcionando con **documento + correo + codigo de verificacion por correo**.
- QA2025-0013 – Se ajusto el sistema para admitir documentos mayores a 8 caracteres en escenarios administrativos y de seguimiento, cubriendo **CE y Pasaporte**.
- QA2025-0014 – En el seguimiento del alumno:
  - ya no se muestra el boton de descarga del PDF del comprobante
  - se informa que la boleta/comprobante **sera enviada automaticamente por correo**
- QA2025-0015 – En el flujo de pago demo / resumen de pago:
  - ya no se expone el PDF al alumno
  - se mantiene solo el estado del pago y los datos de referencia

### 1.5 Modulo Integracion IziPay

- QA2025-0016 – Se consolido el envio de `CodigoCPMS` hacia IziPay a partir de `tabla_central.codigo_centro_costo`.
- QA2025-0017 – Se guardan y consultan mas datos del estado de pago IziPay:
  - `idPagoIziPay`
  - URL de pago
  - estado general / estado textual
  - CIP
  - numero de orden
  - numero de transaccion
  - fecha de creacion
  - codigo de autorizacion
  - numero de comprobante
  - URL del comprobante PDF
- QA2025-0018 – La logica funcional actual respeta el flujo operativo acordado:
  - **alumno**: recibe el comprobante por correo
  - **admin**: puede verlo desde panel si IziPay ya lo publico

### 1.6 Modulo Formulario publico de inscripcion

- QA2025-0019 – La tarjeta de contacto para tarifa INSNSB paso a depender de un dato por actividad, tomado desde base de datos.
- QA2025-0020 – Se incorporo soporte de administracion para `url_banner_web` desde la edicion del curso.

## 2. Tablas y campos creados o modificados - Base de datos

### 2.1 Tabla `dbo.tabla_central`

Campos incorporados / utilizados en el tramo posterior al modulo de reportes:

| Campo | Tipo | Uso funcional |
| --- | --- | --- |
| `codigo_centro_costo` | `VARCHAR(50)` | Se envia a IziPay como `CodigoCPMS`. |
| `celular_contacto_actividad` | `NVARCHAR(30)` | Telefono mostrado segun actividad en el formulario publico. |
| `visible_en_formulario` | `NVARCHAR(2)` | Controla si el curso aparece o no en el formulario publico. Valores esperados: `Si` / `No`. |
| `fue_pagado_historico` | `NVARCHAR(2)` | Mantiene el curso visible en admin como historico aunque se retire del formulario publico. Valores esperados: `Si` / `No`. |

Restricciones / defaults asociados:

- `DF_tabla_central_visible_en_formulario`
- `CK_tabla_central_visible_en_formulario`
- `DF_tabla_central_fue_pagado_historico`
- `CK_tabla_central_fue_pagado_historico`

Notas:

- `url_banner_web` ya venia siendo consumido por la UI, pero ahora tambien queda incorporado en la pantalla de edicion admin.
- `url_programa_web` sigue siendo usado y ahora es editable desde admin.

### 2.2 Tabla `dbo.deudas_cuotas`

Campos de integracion y trazabilidad IziPay visibles en el tramo actual:

| Campo | Tipo | Uso funcional |
| --- | --- | --- |
| `id_pago_izipay` | `BIGINT` | Identificador del pago creado en IziPay. |
| `url_pago_izipay` | `NVARCHAR(500)` | URL de resumen / pasarela de pago. |
| `estado_izipay` | `VARCHAR(30)` | Estado textual reportado por IziPay. |
| `cip_izipay` | `NVARCHAR(50)` | CIP del pago si aplica. |
| `numero_orden_izipay` | `NVARCHAR(100)` | Numero de orden en IziPay. |
| `numero_transaccion_izipay` | `NVARCHAR(120)` | Numero de transaccion devuelto por IziPay. |
| `fecha_creacion_izipay` | `DATETIME` | Fecha de creacion del registro en IziPay. |
| `fecha_ultima_consulta_izipay` | `DATETIME` | Marca de la ultima consulta de estado. |
| `codigo_autorizacion_izipay` | `NVARCHAR(50)` | Codigo de autorizacion del pago. |
| `numero_comprobante_izipay` | `NVARCHAR(50)` | Numero del comprobante emitido. |
| `url_comprobante_pdf_izipay` | `NVARCHAR(1000)` | Ruta PDF del comprobante para contingencia administrativa. |

Indice asociado:

- `UX_deudas_cuotas_id_pago_izipay`

## 3. Documentos sustento de Control de calidad - Funcionalidades

### 3.1 Documentos existentes en el repositorio

- [QA_Inscripcion_Mobile_2026-04-10.md](c:/Users/spuruguay/source/repos/CURSO_INTERCULTURALIDAD/docs/QA_Inscripcion_Mobile_2026-04-10.md)
  - Sustenta QA responsive y flujo base de inscripcion / seguimiento movil.
- [Reporte_Tablas_Principales_Inscripcion.xlsx](c:/Users/spuruguay/source/repos/CURSO_INTERCULTURALIDAD/docs/Reporte_Tablas_Principales_Inscripcion.xlsx)
  - Sustento de inventario / estructura de tablas principales. No corresponde a QA funcional, pero sirve como respaldo tecnico.

### 3.2 Documento agregado para este corte

- [QA_Funcionalidades_Post_Reporte_2026-04-24.md](c:/Users/spuruguay/source/repos/CURSO_INTERCULTURALIDAD/docs/QA_Funcionalidades_Post_Reporte_2026-04-24.md)
  - Casos de QA recomendados para:
    - visibilidad publica vs historico admin
    - edicion de ficha publica
    - preview del curso
    - busqueda y orden en seguimiento admin
    - comprobante por correo al alumno
    - PDF visible solo para admin
    - soporte para CE / Pasaporte

## 4. Procedimientos Almacenados y funciones - Base de datos

### 4.1 Procedimientos almacenados identificados como impactados / utilizados en este tramo

#### Catalogos / cursos

- `dbo.sp_docencia_obtener_cursos_pagados`
- `dbo.sp_docencia_obtener_curso_pagado_por_id`

Responsabilidad:

- exponer cursos visibles al formulario publico
- devolver `visible_en_formulario`, `fue_pagado_historico`, `codigo_centro_costo`, `celular_contacto_actividad`

#### Seguimiento y pagos

- `dbo.sp_docencia_obtener_seguimiento_alumno`
- `dbo.sp_docencia_obtener_pago_demo_por_token`
- `dbo.sp_docencia_guardar_pago_izipay_por_token`
- `dbo.sp_docencia_actualizar_estado_pago_izipay`
- `dbo.sp_docencia_marcar_cuota_pagada`
- `dbo.sp_docencia_obtener_seguimiento_pagos_admin`

Responsabilidad:

- recuperar dashboard del alumno
- recuperar resumen de pago por token
- guardar `idPagoIziPay` y URL de pago
- actualizar estado IziPay y datos de comprobante
- reflejar estado de cuotas pagadas
- poblar seguimiento de pagos en admin

#### Codigos INSNSB

- `dbo.sp_docencia_obtener_resumen_codigos_insnsb`

Responsabilidad:

- mantener visibilidad operativa del modulo de codigos unicos en admin

### 4.2 Funciones T-SQL

- **No se identifican funciones SQL Server nuevas o modificadas** en este corte dentro de los scripts versionados del repositorio.

## 5. Scripts de base de datos relacionados

Los scripts principales asociados a este tramo son:

- [20260410_actualizar_tablas_inscripcion_pago.sql](c:/Users/spuruguay/source/repos/CURSO_INTERCULTURALIDAD/sql/20260410_actualizar_tablas_inscripcion_pago.sql)
- [20260410_sp_inscripcion_catalogos.sql](c:/Users/spuruguay/source/repos/CURSO_INTERCULTURALIDAD/sql/20260410_sp_inscripcion_catalogos.sql)
- [20260410_sp_inscripcion_seguimiento.sql](c:/Users/spuruguay/source/repos/CURSO_INTERCULTURALIDAD/sql/20260410_sp_inscripcion_seguimiento.sql)
- [PATCH_BDUDITD.sql](c:/Users/spuruguay/source/repos/CURSO_INTERCULTURALIDAD/sql/PATCH_BDUDITD.sql)

## 6. Observacion de control de cambios

Este documento consolida el **delta funcional y tecnico posterior al hito de Reporte de inscritos**. Si se requiere una version para comite o jefatura, se puede convertir esta base en:

- acta de pase a produccion
- matriz de impacto por modulo
- formato Word institucional
