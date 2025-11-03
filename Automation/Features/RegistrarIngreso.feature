Feature: Nuevo Ingreso

A short summary of the feature

Background: 
Given el usuario ingresa al ambiente 'https://taller2025-qa.sigesonline.com/'
When el usuario ingresa sesion con usuario 'admin@plazafer.com' y contraseña 'calidad'
And acceder al modulo 'Tesorería Y Finanzas'
And acceder al submodulo 'Ingresos/Egresos'

@RegistrarIngreso
Scenario: Registrar un ingreso válido
   When el usuario hace clic en el botón 'INGRESO'
   And completa el campo 'Autorizado por' con '11111110'
   And selecciona 'Empleado' como tipo de persona
   And completa el campo 'Pagador' con '11111110'
   And selecciona el tipo de documento 'NOTA DE INGRESO'
   And completa el importe con '100.00'
   And escribe la observación 'Pago de servicio'
   And presiona el botón 'GUARDAR'
   Then el ingreso se registra correctamente

@CP02_FaltaAutorizado
Scenario: Registro sin autorizado
   When el usuario hace clic en el botón 'INGRESO'
   And selecciona 'Empleado' como tipo de persona
   And completa el campo 'Pagador' con '11111110'
   And selecciona el tipo de documento 'NOTA DE INGRESO'
   And completa el importe con '100.00'
   And escribe la observación 'Falta autorizado'
   And presiona el botón 'GUARDAR'
   Then se muestra el mensaje de error 'Es necesario seleccionar un autorizado.'
