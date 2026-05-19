# QA Funcionalidades Posteriores al Modulo de Reportes

Fecha: 2026-04-24

## Alcance

- Modulo admin de edicion de ficha publica del curso
- Visibilidad del curso en formulario publico vs historico admin
- Seguimiento de pagos admin con buscador y orden por columnas
- Integracion de comprobante IziPay:
  - visible para admin
  - no expuesto al alumno
- Soporte de documentos mayores a 8 caracteres para CE / Pasaporte
- Telefono de contacto por actividad

## Estado del sustento

### Smoke tecnico ejecutado

- PASS: `dotnet build -c Release /p:UseAppHost=false`
- PASS: `dotnet publish -c Release /p:UseAppHost=false`

### Validacion manual

- Pendiente de ejecucion integral en entorno funcional con datos reales o de homologacion.

## Casos funcionales recomendados

### 1. Edicion de ficha publica del curso

1. Ingresar al panel admin.
2. Seleccionar un curso.
3. Abrir `Editar ficha publica del curso`.
4. Modificar `NombreActividad`, `TextoDirigidoA`, `Modalidad`, `TextoFechasDuracion`, `HorarioTexto` y `LugarActividad`.
5. Presionar `Vista previa`.
6. Verificar que la preview refleja exactamente los cambios.
7. Presionar `Guardar cambios`.
8. Abrir el formulario publico del mismo curso y confirmar que se visualizan los cambios persistidos.

Resultado esperado:

- La preview responde sin guardar primero.
- El guardado actualiza la ficha publica del curso.

### 2. URL de banner por actividad

1. Editar `UrlBannerWeb` del curso.
2. Generar `Vista previa`.
3. Confirmar que el banner cambia en la preview.
4. Guardar.
5. Abrir el formulario publico del curso.

Resultado esperado:

- El banner configurado se muestra en admin preview y en formulario publico.

### 3. Visibilidad publica del curso

1. Editar un curso con `VisibleEnFormulario = No`.
2. Guardar cambios.
3. Abrir el formulario publico sin `cursoId`.
4. Revisar la lista de cursos.
5. Entrar al panel admin y verificar que el curso sigue apareciendo en el selector.

Resultado esperado:

- El curso no aparece para el alumno en la lista publica.
- El curso se conserva visible en admin.

### 4. Historico admin

1. Tomar un curso con pagos / movimientos ya registrados.
2. Confirmar que se conserva en admin aunque ya no este visible en formulario.

Resultado esperado:

- El curso sigue administrable en panel y/o reportes por su condicion historica.

### 5. Celular de contacto por actividad

1. Configurar `celular_contacto_actividad`.
2. Guardar cambios.
3. Abrir el formulario del curso.

Resultado esperado:

- La tarjeta `Tarifa trabajador INSNSB` usa el telefono del curso, no un telefono fijo global.

### 6. Buscador en seguimiento admin

1. Ingresar a `Seguimiento de pagos` del panel admin.
2. Escribir fragmentos de:
   - nombre
   - apellido
   - correo
   - documento
   - celular
   - estado
3. Revisar el contador de resultados.

Resultado esperado:

- La tabla filtra filas sin recargar pagina.
- El contador cambia segun los registros visibles.

### 7. Orden por columnas en seguimiento admin

1. Hacer clic en cada encabezado:
   - Alumno
   - Documento
   - Registro
   - Estado
   - Total
   - Pagado
   - Pendiente
   - Cuotas
2. Repetir clic para alternar ascendente / descendente.

Resultado esperado:

- Las filas se reordenan correctamente.
- El detalle de cuotas permanece asociado a su fila resumen.

### 8. Detalle de cuotas y comprobante admin

1. Abrir `Mas detalle` para una inscripcion pagada.
2. Verificar si el pago ya tiene:
   - numero de comprobante
   - codigo de autorizacion
   - URL PDF

Resultado esperado:

- Si existe `url_comprobante_pdf_izipay`, el admin ve `Ver comprobante PDF`.
- Si el PDF aun no fue publicado, no se rompe la fila y sigue visible el mensaje operativo.

### 9. Comprobante no visible para el alumno

1. Abrir `Seguimiento` del alumno con una cuota ya pagada.
2. Abrir tambien `PagoDemo` con una cuota pagada.

Resultado esperado:

- El alumno no ve boton para abrir PDF.
- El sistema informa que el comprobante sera enviado automaticamente al correo.

### 10. Documento CE / Pasaporte mayor a 8 caracteres

1. En el panel admin, registrar codigo para un documento de 9 a 20 caracteres.
2. En seguimiento, consultar con documento no DNI de longitud mayor a 8.

Resultado esperado:

- No se bloquea por longitud 8.
- El flujo acepta CE o Pasaporte dentro del rango permitido.

## Riesgos / observaciones

- La logica de historico admin depende de la disponibilidad de columnas `visible_en_formulario` y `fue_pagado_historico` en base de datos.
- La visualizacion del PDF para admin depende de que IziPay efectivamente devuelva `url_comprobante_pdf_izipay`.
- El alumno no tiene descarga directa de PDF por decision funcional; la contingencia queda en correo + panel admin.

## Evidencia sugerida para cierre formal

- Captura de pantalla del dashboard admin con buscador y orden por columnas.
- Captura de pantalla de `Editar ficha publica del curso`.
- Captura de la preview de curso con banner.
- Captura del formulario publico antes y despues de ocultar un curso.
- Captura del seguimiento admin mostrando comprobante PDF para una cuota pagada.
