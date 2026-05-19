# QA Inscripcion y Seguimiento Movil

Fecha: 2026-04-10

## Alcance

- Vista de seleccion de curso
- Vista de inscripcion del alumno
- Vista de seguimiento con verificacion por correo
- Comportamiento responsive en celular

## Cambios validados

- Botones principales y secundarios alineados en desktop y movil
- Links de navegacion con apariencia consistente
- Inputs y selects con altura tactil comoda
- Tablas adaptadas a tarjetas en pantallas pequenas
- Confirmacion doble de correo en el formulario
- Seguimiento por DNI + correo + codigo de verificacion

## Smoke Checks Ejecutados

- PASS: Seguimiento carga
- PASS: Seguimiento incluye campo `correoSeguimiento`
- PASS: Seguimiento incluye boton `requestVerificationCodeButton`
- PASS: Index seleccion carga
- PASS: Index formulario carga con un `cursoId` real
- PASS: Index formulario incluye campo `confirmacionCorreo`
- PASS: Index formulario incluye seccion `paymentPlanSection`
- PASS: `dotnet build`

## Casos Manuales Recomendados

1. Celular, curso pagado externo
- Seleccionar curso con costo general mayor a cero
- Completar formulario
- Confirmar que aparece plan de pagos
- Verificar que la tabla de cuotas se vea como tarjetas en pantalla pequena

2. Celular, curso gratuito para INSNSB
- Seleccionar curso con tarifa externa mayor a cero y tarifa INSNSB igual a cero
- Elegir `INSNSB`
- Ingresar codigo INSNSB valido
- Confirmar que no aparece el plan de cuotas
- Confirmar que registra directamente la inscripcion

3. Confirmacion de correo
- Ingresar dos correos distintos
- Confirmar que el formulario bloquea el avance
- Corregir el correo y verificar que permite continuar

4. Seguimiento por correo
- Ingresar DNI y correo registrado
- Solicitar codigo
- Validar codigo de 6 digitos
- Confirmar ingreso al dashboard

5. Pago demo desde seguimiento
- Abrir un curso con cuota pendiente
- Confirmar que solo la siguiente cuota habilitada tenga boton de pago
- Verificar que una cuota posterior quede bloqueada hasta pagar la anterior
