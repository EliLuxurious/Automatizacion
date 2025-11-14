Feature: Nuevo Ingreso
  A short summary of the feature

  Background:
    Given el usuario ingresa al ambiente 'http://161.132.67.82:31097/'
    When el usuario ingresa sesion con usuario 'admin@plazafer.com' y contraseña 'calidad'
    And acceder al modulo 'Tesorería Y Finanzas'
    And acceder al submodulo 'Ingresos/Egresos'

  @ValidacionesRequeridas_TodoVacio
  Scenario: Validaciones requeridas al guardar con todo vacío
    When el usuario hace clic en el botón 'INGRESO'
    And selecciona '-' como tipo de persona
    And completa el campo 'Autorizado por' con ''
    And completa el campo 'Pagador' con ''
    And completa el campo 'Documento' con 'NOTA DE INGRESO'
    And completa el importe con '0'
    And escribe la observación 'Ninguna'
    And intenta guardar el ingreso
    Then se muestran las inconsistencias requeridas
	  | mensaje                 |
      | pagador o beneficiario. |
      | autorizado              |
      | comprobante             |
      | mayor                   |
     
  @FaltantesUnitarios
  Scenario Outline: Validaciones requeridas con faltante de cliente
    When el usuario hace clic en el botón 'INGRESO'
    And completa el campo 'Autorizado por' con 'Cliente'
    And completa el campo 'Autorizado por' con '<autorizado>'
    And completa el campo 'Pagador' con '<pagador>'
    And completa el campo 'Documento' con 'NOTA DE INGRESO'
    And completa el importe con '<importe>'
    And escribe la observación '<observacion>'
    And intenta guardar el ingreso
    Then se muestran las inconsistencias requeridas
      | mensaje  |
      | <espera> |

    Examples:
      | campo            | autorizado | pagador   | documento           | importe | observacion     | espera      |
      | Falta Autorizado |            | 11111110  | NOTA DE INGRESO     | 50.00   |                 | autorizado  |
      | Falta Pagador    | 11111110   |           | NOTA DE INGRESO     | 50.00   | Caso automático | pagador     |
      | Falta Documento  | 11111110   | 11111110  |                     | 50.00   | Caso automático | comprobante |
      | Importe = 0      | 11111110   | 11111110  | NOTA DE INGRESO     | 0.00    | Caso automático | importe     |

  @FaltantesUnitarios
  Scenario Outline: Validaciones requeridas con faltante de empleado
    When el usuario hace clic en el botón 'INGRESO'
    And completa el campo 'Autorizado por' con 'Empleado'
    And completa el campo 'Autorizado por' con '<autorizado>'
    And completa el campo 'Pagador' con '<pagador>'
    And completa el campo 'Documento' con 'NOTA DE INGRESO'
    And completa el importe con '<importe>'
    And escribe la observación '<observacion>'
    And intenta guardar el ingreso
    Then se muestran las inconsistencias requeridas
      | mensaje  |
      | <espera> |

    Examples:
      | campo            | autorizado | pagador   | documento           | importe | observacion     | espera      |
      | Falta Autorizado |            | 11111110  | NOTA DE INGRESO     | 50.00   |                 | autorizado  |
      | Falta Pagador    | 11111110   |           | NOTA DE INGRESO     | 50.00   | Caso automático | pagador     |
      | Falta Documento  | 11111110   | 11111110  |                     | 50.00   | Caso automático | comprobante |
      | Importe = 0      | 11111110   | 11111110  | NOTA DE INGRESO     | 0.00    | Caso automático | importe     |
      
      @FormatoIdentidad_Y_Congruencia
      Scenario Outline: Validaciones de formato y congruencia
        When el usuario hace clic en el botón 'INGRESO'
        And selecciona '<tipoPersona>' como tipo de persona
        And completa el campo 'Autorizado por' con '<autorizado>'
        And completa el campo 'Pagador' con '<pagador>'
        And completa el campo 'Documento' con 'NOTA DE INGRESO'
        And completa el importe con '<importe>'
        And escribe la observación 'Caso formato'
        And intenta guardar el ingreso
        Then no debe permitir registrar el ingreso
        And se muestra el mensaje de error '<mensajeEsperado>'

        Examples:
          | caso                              | tipoPersona | autorizado | pagador      | importe | mensajeEsperado    |
          | DNI pagador longitud inválida     | Cliente     | 11111110   | 123          | 10.00   | no es un Id Valido |
          | DNI pagador con caracteres no num | Cliente     | 11111110   | 41A11B11     | 10.00   | no es un Id Valido |
          | RUC proveedor con DV incorrecto   | Proveedor   | 11111110   | 20123456780  | 20.00   | no es un Id Valido |
          | Tipo pagador ≠ dato ingresado     | Empleado    | 11111110   | 20123456780  | 25.00   | no es un Id Valido |




  @HappyPath
  Scenario: Registrar un ingreso válido
    When el usuario hace clic en el botón 'INGRESO'
    And selecciona 'Empleado' como tipo de persona
    And completa el campo 'Autorizado por' con '76018810'   
    And completa el campo 'Pagador' con '76018810'           
    And completa el campo 'Documento' con 'NOTA DE INGRESO'
    And completa el importe con '65.00'
    And escribe la observación 'Ninguna'
    And presiona el botón 'GUARDAR'
    Then el ingreso se registra correctamente


    @DocChange_ReiniciaDependencias
    Scenario: Cambiar de DON a NOTA DE INGRESO reinicia dependencias
      When el usuario hace clic en el botón 'INGRESO'
      And selecciona 'Cliente' como tipo de persona
      And completa el campo 'Autorizado por' con '11111110'
      And completa el campo 'Pagador' con '11111110'
      And selecciona el tipo de documento 'DON'
      And escribe la observación 'Obs DON'
      And selecciona el tipo de documento 'NOTA DE INGRESO'
      And completa el importe con '30'
      And intenta guardar el ingreso
      Then el ingreso se registra correctamente

    @DON_ObservacionRequerida
    Scenario: DON exige observación
      When el usuario hace clic en el botón 'INGRESO'
      And selecciona 'Cliente' como tipo de persona
      And completa el campo 'Autorizado por' con '11111110'
      And completa el campo 'Pagador' con '11111110'
      And selecciona el tipo de documento 'DON'
      And completa el importe con '20'
      And intenta guardar el ingreso
      Then se muestra el mensaje de error 'comprobante'

    @Obs_Larga_SeValidaOTrunca
    Scenario: NOTA DE INGRESO con observación de 600 caracteres
      When el usuario hace clic en el botón 'INGRESO'
      And selecciona 'Cliente' como tipo de persona
      And completa el campo 'Autorizado por' con '11111110'
      And completa el campo 'Pagador' con '11111110'
      And selecciona el tipo de documento 'NOTA DE INGRESO'
      And completa el importe con '30'
      And escribe una observación de '600' caracteres
      And presiona el botón 'GUARDAR'
      Then el ingreso se registra correctamente

    @Importe_TresDecimales
    Scenario: Importe 10.999 se redondea/normaliza
      When el usuario hace clic en el botón 'INGRESO'
      And selecciona 'Cliente' como tipo de persona
      And completa el campo 'Autorizado por' con '11111110'
      And completa el campo 'Pagador' con '11111110'
      And selecciona el tipo de documento 'NOTA DE INGRESO'
      And completa el importe con '10.999'
      Then el importe formateado es '11.00'
      And presiona el botón 'GUARDAR'
      And escribe la observación 'Ninguna'
      Then el ingreso se registra correctamente

    @Importe_Negativo
    Scenario: Importe negativo no permite registrar
      When el usuario hace clic en el botón 'INGRESO'
      And selecciona 'Cliente' como tipo de persona
      And completa el campo 'Autorizado por' con '11111110'
      And completa el campo 'Pagador' con '11111110'
      And selecciona el tipo de documento 'NOTA DE INGRESO'
      And completa el importe con '-1.00'
      And escribe la observación 'Negativo'
      And intenta guardar el ingreso
      Then se muestra el mensaje de error 'importe sea mayor a 0'

      @Obs_LongitudMaxima
      Scenario: NOTA DE INGRESO con observación de 600 caracteres muestra error
        When el usuario hace clic en el botón 'INGRESO'
        And selecciona 'Cliente' como tipo de persona
        And completa el campo 'Autorizado por' con '11111110'
        And completa el campo 'Pagador' con '11111110'
        And selecciona el tipo de documento 'NOTA DE INGRESO'
        And completa el importe con '30'
        And escribe una observación de '600' caracteres
        And intenta guardar el ingreso
        Then se muestra el mensaje de error 'Error al guardar el ingreso / egreso'
        And no se registra el ingreso

      @Importe_TresDecimales_Bloqueo
      Scenario: Importe con más de 2 decimales no permite registrar
        When el usuario hace clic en el botón 'INGRESO'
        And selecciona 'Cliente' como tipo de persona
        And completa el campo 'Autorizado por' con '11111110'
        And completa el campo 'Pagador' con '11111110'
        And selecciona el tipo de documento 'NOTA DE INGRESO'
        And completa el importe con '10.999'
        And escribe la observación 'Caso 3 decimales'
        And intenta guardar el ingreso
        Then no debe permitir registrar el ingreso
        And se muestra el mensaje de error 'importe solo permite 2 decimales'

      @Importe_Alto_Limite
      Scenario: Importe valor límite alto 999999.99 se registra correctamente
        When el usuario hace clic en el botón 'INGRESO'
        And selecciona 'Cliente' como tipo de persona
        And completa el campo 'Autorizado por' con '11111110'
        And completa el campo 'Pagador' con '11111110'
        And selecciona el tipo de documento 'NOTA DE INGRESO'
        And completa el importe con '999999.99'
        And escribe la observación 'Límite alto'
        And presiona el botón 'GUARDAR'
        Then el importe formateado es '999999.99'
        And el ingreso se registra correctamente

      @Importe_Comas
      Scenario: Importe con separador de miles/comas no se acepta
        When el usuario hace clic en el botón 'INGRESO'
        And selecciona 'Cliente' como tipo de persona
        And completa el campo 'Autorizado por' con '11111110'
        And completa el campo 'Pagador' con '11111110'
        And selecciona el tipo de documento 'NOTA DE INGRESO'
        And completa el importe con '10,00'
        And escribe la observación 'Con coma'
        And intenta guardar el ingreso
        Then no debe permitir registrar el ingreso
        And se muestra el mensaje de error 'formato de importe inválido'

      @Importe_ConEspaciosYLetras
      Scenario: Importe con espacios y letras no se acepta
        When el usuario hace clic en el botón 'INGRESO'
        And selecciona 'Cliente' como tipo de persona
        And completa el campo 'Autorizado por' con '11111110'
        And completa el campo 'Pagador' con '11111110'
        And selecciona el tipo de documento 'NOTA DE INGRESO'
        And completa el importe con ' 50 a '
        And escribe la observación 'Importe inválido'
        And intenta guardar el ingreso
        Then no debe permitir registrar el ingreso
        And se muestra el mensaje de error 'formato de importe inválido'

      @Rol_SinPermiso
      Scenario: Usuario con rol Revisor no puede crear ingresos
        When el usuario ingresa sesion con usuario 'admin@plazafer.com' y contraseña 'calidad'
        And acceder al modulo 'Tesorería Y Finanzas'
        And acceder al submodulo 'Ingresos/Egresos'
        And el usuario hace clic en el botón 'INGRESO'
        And completa el campo 'Autorizado por' con '11111110'
        And completa el campo 'Pagador' con '11111110'
        And selecciona el tipo de documento 'NOTA DE INGRESO'
        And completa el importe con '50.00'
        And escribe la observación 'Intento revisor'
        And intenta guardar el ingreso
        Then se muestra el mensaje de error 'El valor 0 para la entidad actor negocio externo no es un Id Valido'
        And no se registra el ingreso

      @Sesion_Expirada
      Scenario: Sesión expirada al guardar muestra error de servidor
        When el usuario hace clic en el botón 'INGRESO'
        And selecciona 'Cliente' como tipo de persona
        And completa el campo 'Autorizado por' con '11111110'
        And completa el campo 'Pagador' con '11111110'
        And selecciona el tipo de documento 'NOTA DE INGRESO'
        And completa el importe con '40.00'
        And escribe la observación 'Sesión expirada'
        And deja expirar la sesión
        And intenta guardar el ingreso
        Then se muestra el mensaje de error 'Error de servidor en la aplicación'
        And no se registra el ingreso

      @Autorizado_Inactivo
      Scenario: Autorizado inactivo no permite registrar ingreso
        When el usuario hace clic en el botón 'INGRESO'
        And selecciona 'Empleado' como tipo de persona
        And completa el campo 'Autorizado por' con '99999999'   
        And completa el campo 'Pagador' con '11111110'
        And selecciona el tipo de documento 'NOTA DE INGRESO'
        And completa el importe con '25.00'
        And escribe la observación 'Autorizado inactivo'
        And intenta guardar el ingreso
        Then se muestra el mensaje de error 'El valor 0 para la entidad actor negocio externo no es un Id Valido'
        And no se registra el ingreso

      @Pagador_ParticionesEquivalencia
      Scenario Outline: Validaciones de pagador según tipo y documento
        When el usuario hace clic en el botón 'INGRESO'
        And selecciona '<tipoPersona>' como tipo de persona
        And completa el campo 'Autorizado por' con '11111110'
        And completa el campo 'Pagador' con '<pagador>'
        And selecciona el tipo de documento '<documento>'
        And completa el importe con '<importe>'
        And escribe la observación 'Caso pagador'
        And intenta guardar el ingreso
        Then <resultadoRegistro>
        And se muestra el mensaje '<mensajeEsperado>'

        Examples:
          | caso                                        | tipoPersona | pagador      | documento        | importe | resultadoRegistro                          | mensajeEsperado                               |
          | Pagador Proveedor con RUC válido           | Proveedor   | 20123456789  | NOTA DE INGRESO | 30.00   | el ingreso se registra correctamente       | La Operación se realizo con éxito             |
          | Pagador Proveedor con RUC con DV incorrecto| Proveedor   | 20123456780  | NOTA DE INGRESO | 30.00   | no debe permitir registrar el ingreso      | no es un Id Valido                            |
          | Pagador Empleado - DNI válido              | Empleado    | 44556677     | REC-ING         | 15.00   | el ingreso se registra correctamente       | La Operación se realizo con éxito             |
          | Tipo pagador incongruente con identificador| Empleado    | 20123456789  | NOTA DE INGRESO | 25.00   | no debe permitir registrar el ingreso      | no es un Id Valido                            |
          | Pagador vacío                              | Cliente     |              | NOTA DE INGRESO | 20.00   | no debe permitir registrar el ingreso      | Es necesario seleccionar un pagador.          |

      @Importe_ParticionesEquivalencia
      Scenario Outline: Validaciones de importe según rango y formato
        When el usuario hace clic en el botón 'INGRESO'
        And selecciona 'Cliente' como tipo de persona
        And completa el campo 'Autorizado por' con '11111110'
        And completa el campo 'Pagador' con '11111110'
        And selecciona el tipo de documento 'NOTA DE INGRESO'
        And completa el importe con '<importe>'
        And escribe la observación 'Caso importe'
        And intenta guardar el ingreso
        Then <resultadoRegistro>
        And se muestra el mensaje '<mensajeEsperado>'

        Examples:
          | caso                         | importe          | resultadoRegistro                          | mensajeEsperado                               |
          | Importe válido mínimo > 0    | 0.01             | el ingreso se registra correctamente       | La Operación se realizo con éxito             |
          | Importe entero válido        | 100              | el ingreso se registra correctamente       | La Operación se realizo con éxito             |
          | Importe con 2 decimales      | 49.90            | el ingreso se registra correctamente       | La Operación se realizo con éxito             |
          | Importe con más de 2 decimales| 10.999          | no debe permitir registrar el ingreso      | importe solo permite 2 decimales              |
          | Importe igual a 0            | 0                | no debe permitir registrar el ingreso      | Es necesario que el importe sea mayor a 0.    |
          | Importe negativo             | -5               | no debe permitir registrar el ingreso      | Es necesario que el importe sea mayor a 0.    |
          | Importe muy alto             | 999999999999.99  | no debe permitir registrar el ingreso      | Error al intentar guardar el ingreso de dinero|
          | Importe con separador de miles| 1,000.50        | no debe permitir registrar el ingreso      | formato de importe inválido                   |

      @Observacion_ParticionesEquivalencia
      Scenario Outline: Validaciones de observación según documento y longitud
        When el usuario hace clic en el botón 'INGRESO'
        And selecciona 'Cliente' como tipo de persona
        And completa el campo 'Autorizado por' con '11111110'
        And completa el campo 'Pagador' con '11111110'
        And selecciona el tipo de documento '<documento>'
        And completa el importe con '30'
        And escribe la observación '<observacion>'
        And intenta guardar el ingreso
        Then <resultadoRegistro>
        And se muestra el mensaje '<mensajeEsperado>'

        Examples:
          | caso                               | documento        | observacion      | resultadoRegistro                          | mensajeEsperado                               |
          | Observación vacía con NOTA         | NOTA DE INGRESO  |                  | no debe permitir registrar el ingreso      | Es necesario ingresar la observacion.         |
          | Observación obligatoria para DOC   | DOC              |                  | no debe permitir registrar el ingreso      | Es necesario ingresar la observacion.         |
          | Observación longitud máxima (500)  | NOTA DE INGRESO  | texto de 500     | no debe permitir registrar el ingreso      | Error al guardar el ingreso / egreso          |
          | Observación excede longitud (600)  | NOTA DE INGRESO  | texto de 600     | no debe permitir registrar el ingreso      | Error al guardar el ingreso / egreso          |
          | Flujo válido cliente con obs normal| NOTA DE INGRESO  | Ninguna          | el ingreso se registra correctamente       | La Operación se realizo con éxito             |

      @Pagador_ParticionesEquivalencia
      Scenario Outline: Validaciones de pagador según tipo y formato
        When el usuario hace clic en el botón 'INGRESO'
        And selecciona '<tipoPersona>' como tipo de persona
        And completa el campo 'Autorizado por' con '11111110'
        And completa el campo 'Pagador' con '<pagador>'
        And selecciona el tipo de documento 'NOTA DE INGRESO'
        And completa el importe con '20.00'
        And escribe la observación 'Caso pagador'
        And intenta guardar el ingreso
        Then <resultadoRegistro>
        And se muestra el mensaje '<mensajeEsperado>'

        Examples:
          | caso                                   | tipoPersona | pagador       | resultadoRegistro                          | mensajeEsperado                               |
          | Pagador Cliente DNI con espacios       | Cliente     |  41111111     | no debe permitir registrar el ingreso      | no es un Id Valido                            |
          | Pagador DNI 7 dígitos                  | Cliente     | 2345677       | no debe permitir registrar el ingreso      | no es un Id Valido                            |
          | Pagador DNI 8 dígitos válido           | Cliente     | 23456789      | el ingreso se registra correctamente       | La Operación se realizo con éxito             |
          | Pagador DNI 9 dígitos                  | Cliente     | 234567899     | no debe permitir registrar el ingreso      | no es un Id Valido                            |
          | Pagador Proveedor RUC 10 dígitos       | Proveedor   | 2012345678    | no debe permitir registrar el ingreso      | no es un Id Valido                            |
          | Pagador Proveedor RUC 11 válido        | Proveedor   | 20123456789   | el ingreso se registra correctamente       | La Operación se realizo con éxito             |
          | Pagador Proveedor RUC 12 dígitos       | Proveedor   | 201234567890  | no debe permitir registrar el ingreso      | no es un Id Valido                            |
          | Pagador Proveedor RUC DV incorrecto    | Proveedor   | 20123456780   | no debe permitir registrar el ingreso      | no es un Id Valido                            |
          | Pagador vacío                          | Cliente     |               | no debe permitir registrar el ingreso      | Es necesario seleccionar un pagador.          |

      @Importe_ParticionesEquivalencia
      Scenario Outline: Validaciones de importe de rango y formato
        When el usuario hace clic en el botón 'INGRESO'
        And selecciona 'Cliente' como tipo de persona
        And completa el campo 'Autorizado por' con '11111110'
        And completa el campo 'Pagador' con '11111110'
        And selecciona el tipo de documento 'NOTA DE INGRESO'
        And completa el importe con '<importe>'
        And escribe la observación 'Caso importe'
        And intenta guardar el ingreso
        Then <resultadoRegistro>
        And se muestra el mensaje '<mensajeEsperado>'

        Examples:
          | caso                        | importe           | resultadoRegistro                          | mensajeEsperado                               |
          | Importe -0.01               | -0.01             | no debe permitir registrar el ingreso      | Es necesario que el importe sea mayor a 0.    |
          | Importe 0.00                | 0.00              | no debe permitir registrar el ingreso      | Es necesario que el importe sea mayor a 0.    |
          | Importe mínimo válido 0.01  | 0.01              | el ingreso se registra correctamente       | La Operación se realizo con éxito             |
          | Importe 1.00 válido         | 1.00              | el ingreso se registra correctamente       | La Operación se realizo con éxito             |
          | Importe 10.994              | 10.994            | no debe permitir registrar el ingreso      | importe solo permite 2 decimales              |
          | Importe 10.995              | 10.995            | no debe permitir registrar el ingreso      | importe solo permite 2 decimales              |
          | Importe válido alto 999999.98| 999999.98        | el ingreso se registra correctamente       | La Operación se realizo con éxito             |
          | Importe fuera de rango      | 99999999999.99    | no debe permitir registrar el ingreso      | Error al intentar guardar el ingreso de dinero|
          | Importe parámetro fuera rango| 1000000000000.00 | no debe permitir registrar el ingreso      | El valor de parámetro está fuera del intervalo|
          | Importe con miles           | 1,000.00          | no debe permitir registrar el ingreso      | formato de importe inválido                   |

      @Observacion_ParticionesEquivalencia
      Scenario Outline: Validaciones de observación según longitud y documento
        When el usuario hace clic en el botón 'INGRESO'
        And selecciona 'Cliente' como tipo de persona
        And completa el campo 'Autorizado por' con '11111110'
        And completa el campo 'Pagador' con '11111110'
        And selecciona el tipo de documento '<documento>'
        And completa el importe con '30'
        And escribe la observación '<observacion>'
        And intenta guardar el ingreso
        Then <resultadoRegistro>
        And se muestra el mensaje '<mensajeEsperado>'

        Examples:
          | caso                                  | documento        | observacion   | resultadoRegistro                          | mensajeEsperado                               |
          | Observación 0 NOTA                    | NOTA DE INGRESO  |               | no debe permitir registrar el ingreso      | Es necesario ingresar la observacion.         |
          | Observación 1 carácter                | NOTA DE INGRESO  | A             | el ingreso se registra correctamente       | La Operación se realizo con éxito             |
          | Observación 500 caracteres            | NOTA DE INGRESO  | texto de 500  | no debe permitir registrar el ingreso      | Error al guardar el ingreso / egreso          |
          | Observación 501 caracteres            | NOTA DE INGRESO  | texto de 501  | no debe permitir registrar el ingreso      | Error al guardar el ingreso / egreso          |
          | Observación vacía con DOC             | DOC              |               | no debe permitir registrar el ingreso      | Es necesario ingresar la observacion.         |
          | Observación vacía NOTA (repetida)     | NOTA DE INGRESOS |               | no debe permitir registrar el ingreso      | Es necesario ingresar la observacion.         |
          | Observación 1 NOTA (repetida)         | NOTA DE INGRESOS | A             | el ingreso se registra correctamente       | La Operación se realizo con éxito             |

      @OrdenTab
      Scenario: Orden correcto de tabulación en el formulario
        When el usuario hace clic en el botón 'INGRESO'
        And recorre los campos usando la tecla TAB
        Then el foco avanza en el orden correcto y termina en el botón 'GUARDAR'

      @DobleClick_Guardar
      Scenario: Doble clic en Guardar solo crea un registro
        When el usuario hace clic en el botón 'INGRESO'
        And completa todos los campos obligatorios con datos válidos
        And hace doble clic rápido en el botón 'GUARDAR'
        Then se muestra el mensaje 'La Operación se realizo con éxito'
        And solo se crea un ingreso en el listado

      @Cambio_Cliente_A_Proveedor
      Scenario: Cambio de tipo de Cliente a Proveedor limpia el pagador
        When el usuario hace clic en el botón 'INGRESO'
        And selecciona 'Cliente' como tipo de persona
        And completa el campo 'Pagador' con '23456789'
        And cambia el tipo de persona a 'Proveedor'
        Then el campo 'Pagador' queda vacío
        And el resto de campos se mantiene sin cambios

      @ST_FormularioValido
      Scenario: Formulario válido habilita Guardar
        When el usuario hace clic en el botón 'INGRESO'
        And selecciona 'Cliente' como tipo de persona
        And completa el campo 'Autorizado por' con '11111110'
        And completa el campo 'Pagador' con '11111110'
        And completa el campo 'Documento' con 'NOTA DE INGRESO'
        And completa el importe con '50.00'
        And escribe la observación 'Caso válido'
        Then el botón 'GUARDAR' está habilitado

      @ST_GuardarDeshabilitaBoton
      Scenario: Guardar deshabilita el botón
        When el usuario hace clic en el botón 'INGRESO'
        And selecciona 'Cliente' como tipo de persona
        And completa el campo 'Autorizado por' con '11111110'
        And completa el campo 'Pagador' con '11111110'
        And completa el campo 'Documento' con 'NOTA DE INGRESO'
        And completa el importe con '50.00'
        And escribe la observación 'Caso doble clic'
        And presiona el botón 'GUARDAR'
        And presiona nuevamente el botón 'GUARDAR'
        Then el botón 'GUARDAR' se deshabilita
        And solo se registra un ingreso

      @ST_ExitoYCierre
      Scenario: Éxito y cierre vuelve al listado
        When el usuario hace clic en el botón 'INGRESO'
        And selecciona 'Cliente' como tipo de persona
        And completa el campo 'Autorizado por' con '11111110'
        And completa el campo 'Pagador' con '11111110'
        And completa el campo 'Documento' con 'NOTA DE INGRESO'
        And completa el importe con '65.00'
        And escribe la observación 'Ninguna'
        And presiona el botón 'GUARDAR'
        Then se muestra el mensaje 'La operación se guardo con éxito'
        And el usuario vuelve al listado de ingresos
        And el nuevo ingreso aparece en el listado

      @ST_CancelarDesdeParcial
      Scenario: Cancelar formulario parcial no registra
        When el usuario hace clic en el botón 'INGRESO'
        And completa el campo 'Autorizado por' con '11111110'
        And presiona el botón 'CANCELAR'
        Then el formulario se cierra
        And no se registra ningún ingreso

      @ST_CancelarDesdeValido
      Scenario: Cancelar formulario válido borra datos
        When el usuario hace clic en el botón 'INGRESO'
        And selecciona 'Cliente' como tipo de persona
        And completa el campo 'Autorizado por' con '11111110'
        And completa el campo 'Pagador' con '11111110'
        And completa el campo 'Documento' con 'NOTA DE INGRESO'
        And completa el importe con '50.00'
        And escribe la observación 'Cancelar válido'
        And presiona el botón 'CANCELAR'
        Then el formulario se cierra sin registrar
        And al volver a abrir, los campos aparecen vacíos

      @ST_ErrorSistema
      Scenario: Error del sistema al guardar no registra
        When el usuario hace clic en el botón 'INGRESO'
        And selecciona 'Cliente' como tipo de persona
        And completa el campo 'Autorizado por' con 'DNI_INACTIVO'
        And completa el campo 'Pagador' con '11111110'
        And completa el campo 'Documento' con 'NOTA DE INGRESO'
        And completa el importe con '50.00'
        And escribe la observación 'Autorizado inactivo'
        And presiona el botón 'GUARDAR'
        Then se muestra un mensaje de error de sistema
        And no se registra el ingreso

      @ST_TimeoutGuardar
      Scenario: Timeout al guardar y reintento exitoso
        When el usuario hace clic en el botón 'INGRESO'
        And selecciona 'Cliente' como tipo de persona
        And completa el campo 'Autorizado por' con '11111110'
        And completa el campo 'Pagador' con '11111110'
        And completa el campo 'Documento' con 'NOTA DE INGRESO'
        And completa el importe con '50.00'
        And escribe la observación 'Timeout'
        And se produce un timeout al presionar 'GUARDAR'
        And el usuario presiona el botón 'REINTENTAR'
        Then se muestra el mensaje 'La operación se guardo con éxito'
        And solo se registra un ingreso

      @ST_CambioTipoPagador
      Scenario: Cambiar Cliente a Proveedor limpia pagador
        When el usuario hace clic en el botón 'INGRESO'
        And selecciona 'Cliente' como tipo de persona
        And completa el campo 'Pagador' con '41111111'
        And cambia el tipo de persona a 'Proveedor'
        Then el campo 'Pagador' queda vacío

      @ST_DocNotaRequiereObs
      Scenario: NOTA DE INGRESO sin observación muestra inconsistencia
        When el usuario hace clic en el botón 'INGRESO'
        And selecciona 'Cliente' como tipo de persona
        And completa el campo 'Autorizado por' con '11111110'
        And completa el campo 'Pagador' con '11111110'
        And completa el campo 'Documento' con 'NOTA DE INGRESO'
        And completa el importe con '20.00'
        And escribe la observación ''
        And intenta guardar el ingreso
        Then se muestra el mensaje de error 'es necesario escribir una observacion'

      @ST_CorregirObs
      Scenario: Corregir observación y guardar
        When el usuario hace clic en el botón 'INGRESO'
        And selecciona 'Cliente' como tipo de persona
        And completa el campo 'Autorizado por' con '11111110'
        And completa el campo 'Pagador' con '11111110'
        And completa el campo 'Documento' con 'NOTA DE INGRESO'
        And completa el importe con '20.00'
        And escribe la observación 'correccion'
        And presiona el botón 'GUARDAR'
        Then se muestra el mensaje 'La Operación se realizo con éxito'

      @ST_ImporteCeroBloqueo
      Scenario: Importe igual a 0 bloquea registro
        When el usuario hace clic en el botón 'INGRESO'
        And selecciona 'Cliente' como tipo de persona
        And completa el campo 'Autorizado por' con '11111110'
        And completa el campo 'Pagador' con '11111110'
        And completa el campo 'Documento' con 'NOTA DE INGRESO'
        And completa el importe con '0.00'
        And escribe la observación 'Cero'
        And intenta guardar el ingreso
        Then se muestra el mensaje 'Es necesario que el importe sea mayor a 0.'

      @ST_ImporteMinimoValido
      Scenario: Importe 0.01 permite guardar
        When el usuario hace clic en el botón 'INGRESO'
        And selecciona 'Cliente' como tipo de persona
        And completa el campo 'Autorizado por' con '11111110'
        And completa el campo 'Pagador' con '11111110'
        And completa el campo 'Documento' con 'NOTA DE INGRESO'
        And completa el importe con '0.01'
        And escribe la observación 'Minimo'
        And presiona el botón 'GUARDAR'
        Then se muestra el mensaje 'La operación se guardo con éxito'

      @ST_ConsolidacionListado
      Scenario: Consolidación con listado después de registrar
        When el usuario registra un ingreso válido
        And el usuario actualiza el listado de ingresos del día
        Then el nuevo ingreso aparece en el listado

      @ST_RecuperacionErrorSistema
      Scenario: Recuperación tras ErrorSistema
        When el usuario hace clic en el botón 'INGRESO'
        And completa el formulario con datos válidos
        And se produce un error de sistema al guardar
        Then se muestra un mensaje de error
        And no se registra el ingreso

      @ST_SalirYVolver
      Scenario: Salir del diálogo y volver sin guardar
        When el usuario hace clic en el botón 'INGRESO'
        And completa el campo 'Autorizado por' con '11111110'
        And completa el campo 'Pagador' con '11111110'
        And cierra el formulario sin guardar
        And vuelve a abrir el formulario de ingreso
        Then los campos aparecen vacíos
        And no se ha registrado ningún ingreso

      @ST_AbrirDetalleDesdeListado
      Scenario: Abrir detalle desde listado
        When el usuario registra un ingreso válido
        And el usuario va al listado de ingresos
        And hace clic en la lupa del último registro
        Then se muestran los datos registrados del ingreso

      @ST_DosFormulariosSimultaneos
      Scenario: Dos formularios simultáneos no duplican registro
        When el usuario abre dos formularios de ingreso en ventanas distintas
        And completa ambos formularios con datos válidos
        And presiona 'GUARDAR' casi al mismo tiempo en ambas ventanas
        Then solo se registra un ingreso
        And no se genera un duplicado por numeración

      @ST_SesionExpiraGuardando
      Scenario: Sesión expira mientras está en Guardando
        When el usuario hace clic en el botón 'INGRESO'
        And completa el formulario con datos válidos
        And la sesión expira antes de guardar
        And el usuario presiona el botón 'GUARDAR'
        Then se cancela la operación
        And se redirige al login
        And no se registra el ingreso

      @ST_MascaraDNIAlFoco
      Scenario: Campo DNI aplica máscara y bloquea letras
        When el usuario hace clic en el botón 'INGRESO'
        And hace foco en el campo 'Autorizado por'
        And intenta escribir números y letras
        Then la máscara de DNI solo permite números
        And las letras y caracteres inválidos son bloqueados

      @Validaciones_Combinadas
      Scenario Outline: Combinaciones de faltantes con resto correcto
        When el usuario hace clic en el botón 'INGRESO'
        And selecciona 'Cliente' como tipo de persona
        And completa el campo 'Autorizado por' con '<autorizado>'
        And completa el campo 'Pagador' con '<pagador>'
        And completa el campo 'Documento' con '<documento>'
        And completa el importe con '50.00'
        And escribe la observación 'Ninguna'
        And intenta guardar el ingreso
        Then se muestran las inconsistencias requeridas
          | mensaje                |
          | <mensaje1>             |
          | <mensaje2>             |

        Examples:
          | caso                               | autorizado | pagador  | documento       | mensaje1                                   | mensaje2                     |
          | Falta Autorizado y Pagador         |           |          | NOTA DE INGRESO | Es necesario seleccionar un pagador o beneficiario. | Es necesario seleccionar un autorizado. |
          | Falta Autorizado y Documento       |           | 11111110 |                 | Es necesario seleccionar un autorizado.   | Es necesario seleccionar un comprobante. |
          | Falta Pagador y Documento          | 11111110  |          |                 | Es necesario seleccionar un pagador o beneficiario. | Es necesario seleccionar un comprobante. |

      @Montos_Formato
      Scenario Outline: Validaciones de formato de importe
        When el usuario hace clic en el botón 'INGRESO'
        And selecciona 'Cliente' como tipo de persona
        And completa el campo 'Autorizado por' con '11111110'
        And completa el campo 'Pagador' con '11111110'
        And completa el campo 'Documento' con 'NOTA DE INGRESO'
        And completa el importe con '<importe>'
        And escribe la observación 'Caso importe'
        And intenta guardar el ingreso
        Then se muestra el mensaje de error '<mensajeEsperado>'

        Examples:
          | caso                               | importe   | mensajeEsperado                              |
          | Más de 2 decimales                 | 10.999    | No permite más de 2 decimales                |
          | Importe negativo                   | -1.00     | No permite usar el signo de -                |
          | Importe muy pequeño redondea a 0   | 0.004     | Es necesario que el importe sea mayor a 0.   |
          | Importe con coma                   | 10,00     | No se permiten comas en el importe           |
          | Importe con espacios y letras      |  50 a     | No se permiten letras en el importe          |

      @Observacion_Longitud
      Scenario Outline: Validaciones de observación
        When el usuario hace clic en el botón 'INGRESO'
        And selecciona 'Cliente' como tipo de persona
        And completa el campo 'Autorizado por' con '11111110'
        And completa el campo 'Pagador' con '11111110'
        And completa el campo 'Documento' con 'NOTA DE INGRESO'
        And completa el importe con '30.00'
        And escribe la observación '<observacion>'
        And intenta guardar el ingreso
        Then se muestra el mensaje de error '<mensajeEsperado>'

        Examples:
          | caso                        | observacion       | mensajeEsperado                                   |
          | Observación vacía           |                   | Es necesario ingresar la observacion.             |
          | Observación mínima          | A                 | La Operación se realizo con éxito                 |
          | Observación 500 caracteres  | TEXTO_500         | Error al guardar el ingreso / egreso              |
          | Observación 501 caracteres  | TEXTO_501         | Error al guardar el ingreso / egreso              |

      @Observacion_Longitud
Scenario Outline: Validaciones para Observacion según longitud
        When el usuario hace clic en el botón 'INGRESO'
        And selecciona 'Cliente' como tipo de persona
        And completa el campo 'Autorizado por' con '11111110'
        And completa el campo 'Pagador' con '11111110'
        And completa el campo 'Documento' con 'NOTA DE INGRESO'
        And completa el importe con '30.00'
        And escribe la observación '<observacion>'
        And intenta guardar el ingreso
        Then se muestra el mensaje de error '<mensajeEsperado>'

        Examples:
          | caso                        | observacion       | mensajeEsperado                                   |
          | Observación vacía           |                   | Es necesario ingresar la observacion.             |
          | Observación mínima          | A                 | La Operación se realizo con éxito                 |
          | Observación 500 caracteres  | TEXTO_500         | Error al guardar el ingreso / egreso              |
          | Observación 501 caracteres  | TEXTO_501         | Error al guardar el ingreso / egreso              |
