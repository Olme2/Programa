# Auditoría del sistema de gestión de pollería

**Fecha:** 2026-08-06
**Repo:** `C:\Users\Valentino\OneDrive\Desktop\Polleria\Programa` — rama `TP9` (HEAD `01b1f50`, 2026-05-11)
**Remoto:** `https://github.com/Olme2/Programa.git`
**Método:** lectura estática completa del repositorio. No se ejecutó la aplicación, no se abrió la base de datos, no se modificó ningún archivo salvo este informe.

**Alcance leído:** 100% de `Controllers/`, `Repositorios/`, `Models/`, `ViewModels/`, `Helpers/`, `Views/`, `Program.cs`, `AppDbContext.cs`, `wwwroot/css/site.css`, `wwwroot/js/site.js`, `db/*.sql`, archivos de configuración y proyecto. No se leyeron `wwwroot/lib/` (librerías de terceros sin modificar) ni `bin/`, `obj/`, `.vs/` salvo para extraer metadatos de compilación.

---

## PARTE A — Qué hace el sistema

### A1. Inventario funcional exhaustivo

Aplicación web ASP.NET Core MVC de un solo proyecto. 11 controladores, 45 acciones alcanzables. El punto de entrada por defecto es **Ventas/Alta** (`Program.cs:75-77`), no una pantalla de inicio: el sistema está pensado como caja registradora.

---

#### 1. Registrar una venta
**Qué hace:** el usuario abre el sistema y cae directamente en esta pantalla. Elige método de pago (por defecto id 1), fecha y hora (precargadas con el momento actual), y va agregando líneas. Cada línea se busca con un autocompletado que muestra `Nombre ($precio) - S: stock`; los productos sin stock aparecen en rojo y no se pueden seleccionar. Al elegir un producto se copian precio y costo actuales a campos ocultos. Se puede agregar promociones en una sección aparte. A la derecha, un panel fijo va mostrando subtotal, recargo, total y el vuelto para billetes de $1.000, $10.000 y $20.000. Hay un campo "Redondeo" en pesos (puede ser negativo) y un campo "Recargo (%)". Al guardar, se valida stock en la aplicación y otra vez en la base por trigger.
**Archivos:** `Controllers/VentasController.cs:96-201`, `Repositorios/VentaRepository.cs:106-179`, `Models/Ventas.cs`, `Views/Ventas/Alta.cshtml`, `Views/Ventas/Partials/_DetalleVentaItem.cshtml`, `Views/Ventas/Partials/_VentaPromocionItem.cshtml`.
**Estado: parcial.** El campo **Recargo (%) no se persiste nunca**. Se muestra en pantalla, entra al total que ve el cajero (`Views/Ventas/Alta.cshtml:267`: `const total = (subtotal+redondeo)*(1+recargo/100);`), pero `Ventas.CrearDesdeViewModel` (`Models/Ventas.cs:35-40`) no lo lee, no hay columna en la base y `CalcularPrecioTotal()` (`Models/Ventas.cs:135-138`) no lo aplica. La única referencia en el servidor es una línea que no hace nada: `viewModel.Recargo = decimal.Parse(viewModel.Recargo.ToString(CultureInfo.InvariantCulture), CultureInfo.InvariantCulture);` (`Controllers/VentasController.cs:119`). **Consecuencia de negocio: si se cobra un recargo por pago con tarjeta, el sistema guarda la venta sin ese recargo.**

#### 2. Listado y consulta de ventas
**Qué hace:** tabla de ventas con filtros de fecha desde/hasta, turno, método de pago y texto libre (busca en nombres de productos y promociones). Arriba, cuatro contadores: cantidad, costo, venta y ganancia, calculados en el navegador sumando las filas visibles. Los filtros se recalculan por AJAX al cambiar cualquier control. Hay un botón "Resetear".
**Archivos:** `Controllers/VentasController.cs:33-93`, `Repositorios/VentaRepository.cs:12-86`, `Views/Ventas/Index.cshtml`, `Views/Ventas/_VentasTabla.cshtml`.
**Estado: completa.**

#### 3. Modificar una venta
**Qué hace:** permite cambiar método de pago, fecha, hora, detalle, redondeo y las cantidades de cada línea. El producto de una línea existente **no** se puede cambiar (se muestra como texto plano, `Views/Ventas/Partials/_ModificarDetalleVentaItem.cshtml:14`); sí se pueden borrar líneas y agregar nuevas.
**Archivos:** `Controllers/VentasController.cs:278-368`, `Repositorios/VentaRepository.cs:128-135`, `Views/Ventas/Modificar.cshtml`.
**Estado: parcial.** El repositorio es un `Update` plano con un comentario que lo admite: *"La lógica de actualización es compleja y requiere un manejo cuidadoso de las entidades... Por ahora, implementamos la estructura y la lógica se puede añadir después"* (`Repositorios/VentaRepository.cs:130-132`). Además `Ventas.ActualizarDesdeViewModel` (`Models/Ventas.cs:49-67`) hace `LimpiarDetalles()` y vuelve a agregar todos los detalles, lo que en la base se traduce a borrar e insertar filas de `detalle_venta` — es lo que dispara los triggers de stock. No hay transacción explícita en esta ruta. **No se valida stock** en la aplicación al modificar (a diferencia del alta); sólo queda la validación del trigger.

#### 4. Eliminar una venta
**Qué hace:** pantalla de confirmación con el detalle de la venta y un botón. Al confirmar, se borra la venta; el borrado en cascada elimina detalles y promociones vendidas, y los triggers devuelven el stock.
**Archivos:** `Controllers/VentasController.cs:371-414`, `Repositorios/VentaRepository.cs:136-147`, `Views/Ventas/Eliminar.cshtml`.
**Estado: completa.**

#### 5. Buscador de productos para venta (autocompletado)
**Qué hace:** endpoint AJAX que devuelve hasta 10 productos activos, ordenados por **venta de la última semana** descendente y luego alfabético, excluyendo los ya elegidos en la misma venta, con una marca `disabled` si el stock es 0.
**Archivos:** `Controllers/VentasController.cs:219-247`.
**Estado: completa.** Nota: carga *todos* los productos con subconsultas por producto y recién después filtra en memoria (`Repositorios/ProductosRepository.cs:13-44`).

#### 6. Buscador de promociones para venta
**Archivos:** `Controllers/VentasController.cs:250-276`.
**Estado: completa**, con una regla de negocio mal aplicada (ver A3, regla 14: una promoción con fecha de inicio futura se puede vender hoy).

#### 7. Filtro por turno
**Qué hace:** al entrar al listado de ventas, el sistema mira la hora actual y preselecciona el turno: mañana si son entre 08:00 y 14:00, tarde si entre 17:30 y 22:00, "todos" fuera de esas franjas.
**Archivos:** `Controllers/VentasController.cs:48-53`, `Repositorios/VentaRepository.cs:18-32`.
**Estado: completa.**

#### 8. Filtro "efectivo" vs "virtuales"
**Qué hace:** en el desplegable de método de pago hay dos opciones agrupadas: 💵 Efectivo y 💳 Virtuales. "Virtuales" significa *todo lo que no se llame exactamente "Efectivo"*.
**Archivos:** `Repositorios/VentaRepository.cs:40-49`, `Views/Ventas/Index.cshtml:24-36`.
**Estado: completa** (con string mágico, ver A3 regla 17).

#### 9. Cálculo de vuelto
**Qué hace:** muestra cuánto hay que devolver si el cliente paga con un billete de $1.000, $10.000 o $20.000, redondeando hacia arriba al múltiplo del billete.
**Archivos:** `Views/Ventas/Alta.cshtml:116-127, 278-283`.
**Estado: completa** (sólo en el alta; la pantalla de modificar no lo tiene).

#### 10. Listado de productos
**Qué hace:** tabla con producto, proveedor, stock, costo, precio, ganancia, % de ganancia y venta semanal. Filtros: orden (alfabético / stock ascendente), búsqueda por producto o proveedor, y un switch "ver inactivos". Las coincidencias de búsqueda se resaltan en amarillo. Las filas de productos inactivos se pintan en gris.
**Archivos:** `Controllers/ProductosController.cs:19-55`, `Repositorios/ProductosRepository.cs:46-104`, `Views/Productos/index.cshtml`, `Views/Productos/_ProductosTabla.cshtml`.
**Estado: completa.**

#### 11. Alta de producto
**Archivos:** `Controllers/ProductosController.cs:57-88`, `Views/Productos/Alta.cshtml`.
**Estado: parcial — con un bug visible.** En `Views/Productos/Alta.cshtml:48` el campo Precio se rellena con el **costo**: `value="@(Model.Precio>0 ? Model.Costo : "")"`. Sólo se nota cuando el formulario vuelve por un error de validación: el precio que había escrito el usuario se reemplaza por el costo. Además `AltaProductoVM.Stock` arranca en `-1` (`ViewModels/Productos/AltaProductoViewModel.cs:17`) como truco para que el campo se vea vacío; si el usuario no lo toca, el error que ve es *"El stock no puede ser negativo ni mayor a 9.999,999"*, que no explica que el campo está vacío.

#### 12. Modificar producto
**Qué hace:** cambiar nombre, proveedor, stock, costo, precio y el switch activo/inactivo. Las etiquetas dicen "Stock (Calculado)" y "Costo (Derivado de compras)".
**Archivos:** `Controllers/ProductosController.cs:90-160`, `Views/Productos/Modificar.cshtml`.
**Estado: parcial — la protección no funciona.** El JavaScript que debía bloquear stock y costo busca `#stock-no-editable` y `#costo-no-editable` (`Views/Productos/Modificar.cshtml:96-110`), ids que **no existen** en el HTML: los inputs se generan con `asp-for="Stock"` y `asp-for="Costo"`, o sea `id="Stock"` e `id="Costo"`. Resultado: los campos son editables, el `alert` explicativo nunca aparece y el usuario puede pisar a mano el stock calculado por los triggers.

#### 13. Eliminar producto
**Qué hace:** sólo se ofrece si el producto no está referenciado en ventas, promociones ni compras. Si lo está, el botón queda gris y el mensaje sugiere desactivarlo.
**Archivos:** `Controllers/ProductosController.cs:162-204`, `Repositorios/ProductosRepository.cs:132-135`.
**Estado: completa.**

#### 14. Desactivar producto → baja en cascada de promociones
**Qué hace:** al pasar un producto de activo a inactivo, el sistema recorre las promociones vigentes que lo contienen; las que ya empezaron se desactivan poniéndoles fecha de fin = hoy, y **las que todavía no empezaron se borran**.
**Archivos:** `Controllers/ProductosController.cs:134-147`, `Repositorios/PromocionesRepository.cs:77-94`.
**Estado: completa pero silenciosa y destructiva.** El usuario no ve ningún aviso previo ni posterior de qué promociones se tocaron o borraron.

#### 15. Lista de precios para copiar (WhatsApp)
**Qué hace:** botón "📋 Copiar Lista de Precios" que arma un texto con `*PRODUCTOS*` y `*PROMOCIONES*` (los asteriscos son negrita de WhatsApp), una línea por ítem con `Nombre | $precio`, y lo copia al portapapeles.
**Archivos:** `Controllers/ProductosController.cs:205-227`, `Repositorios/ProductosRepository.cs:136-143`, `Repositorios/PromocionesRepository.cs:95-103`, `Views/Productos/index.cshtml:86-110`.
**Estado: completa.**

#### 16. Estadísticas de un producto
**Qué hace:** historial de cuánto se vendió de un producto entre dos fechas, fila por fila, con fecha, hora, cantidad y origen ("Directa" o el nombre de la promoción). Muestra el total del período.
**Archivos:** `Controllers/ProductosController.cs:229-251`, `Repositorios/ProductosRepository.cs:145-210`, `Views/Productos/Estadisticas.cshtml`.
**Estado: completa.** No incluye consumo interno ni retiros de producción, aunque esos también descuentan stock.

#### 17–20. Promociones: listado, alta, modificación, baja
**Qué hace:** una promoción es un combo con nombre, precio de venta, fecha de inicio, fecha de fin opcional y una "receta" de productos con cantidades. El listado muestra costo (suma de costos de la receta), precio, ganancia, % de ganancia, stock disponible del combo y estado activa/inactiva. El alta y la modificación calculan costo/ganancia en vivo en el navegador y rechazan un precio menor al costo. La modificación tiene dos modos: si la promoción nunca se vendió se puede editar todo; si ya se vendió, la receta queda bloqueada y sólo se editan nombre, precio y fechas.
**Archivos:** `Controllers/PromocionesController.cs` (completo), `Repositorios/PromocionesRepository.cs`, `Views/Promociones/*`.
**Estado: completa.** La validación precio ≥ costo está escrita **dos veces seguidas** en el alta (`Controllers/PromocionesController.cs:54-58` y `81-84`), la primera con el costo que mandó el navegador y la segunda releyendo los costos de la base.

#### 21–24. Compras: listado, alta, modificación, baja
**Qué hace:** registrar la mercadería que entra. Se elige proveedor, fecha, un switch "Ya fue pagada" y se cargan líneas producto/cantidad/costo unitario. El buscador de productos está **filtrado por el proveedor elegido**; si se cambia el proveedor, se vacían todas las líneas. El total se calcula en vivo. Al guardar, los triggers suben el stock y (según el trigger que vive sólo en la base, ver A3 regla 8) suman la deuda al proveedor.
**Archivos:** `Controllers/ComprasController.cs`, `Repositorios/ComprasRepository.cs`, `Views/Compras/*`.
**Estado: completa**, con la modificación implementada de forma poco convencional: si cambió cualquier línea, borra todas las filas de `detalle_compra` con SQL directo y las reinserta una por una para que se disparen los triggers (`Repositorios/ComprasRepository.cs:93-105`).

#### 25. Marcar una compra como pagada / no pagada
**Qué hace:** el switch "Ya fue pagada" en la modificación de compra ajusta la deuda del proveedor: al pasar de no pagada a pagada le resta el total de la compra; al revés, se lo suma.
**Archivos:** `db/2026-05-10-compra-pagada-trigger.sql` (trigger `tg_compra_pagada_cambio`), `Repositorios/ComprasRepository.cs:107-113`.
**Estado: completa**, pero implementada **íntegramente en la base de datos**, no en el código.

#### 26. Proveedores (ABM completo)
**Qué hace:** alta, listado, modificación y baja de proveedores con nombre, contacto libre y saldo adeudado. El listado se ordena por deuda descendente, pinta en rojo las filas con deuda, permite filtrar "sólo con deuda" / "sólo sin deuda" y buscar por nombre. No se puede borrar un proveedor que tenga productos asociados ni deuda pendiente.
**Archivos:** `Controllers/ProveedoresController.cs`, `Repositorios/ProveedoresRepository.cs`, `Views/Proveedores/*`.
**Estado: completa.**

#### 27. Métodos de pago (ABM completo)
**Archivos:** `Controllers/MetodosPagoController.cs`, `Repositorios/MetodosPagoRepository.cs`, `Views/MetodosPago/*`.
**Estado: completa.** No se puede borrar un método usado en alguna venta.

#### 28. Consumo interno
**Qué hace:** registrar mercadería que se consume el negocio (comida del personal, etc.). Se elige fecha, un detalle libre y productos con cantidad; el costo unitario se autocompleta desde el producto y queda de sólo lectura. Descuenta stock. Se lista por rango de fechas con el costo total del período, y se puede modificar y eliminar (devolviendo stock).
**Archivos:** `Controllers/ConsumoController.cs`, `Repositorios/ConsumoRepository.cs`, `Views/Consumo/*`.
**Estado: completa.** Implementado reutilizando la tabla `venta` con `tipo='consumo'` y precio 0.

#### 29. Retiro para producción
**Qué hace:** idéntico a consumo, pero para mercadería que se retira para elaborar otra cosa. Se llama "retiro de producción".
**Archivos:** `Controllers/ProduccionController.cs`, `Repositorios/ProduccionRepository.cs`, `Views/Produccion/*`.
**Estado: completa como registro de salida de stock — pero no modela la transformación.** Ver A3 regla 5: sale materia prima, no entra producto elaborado.

#### 30. Gastos
**Qué hace:** registrar gastos que no son mercadería (luz, gas, bolsas, aceite). Fecha, concepto, monto y observación. Se listan por rango de fechas con total, y se pueden borrar.
**Archivos:** `Controllers/GastosController.cs`, `Repositorios/GastosRepository.cs`, `Views/Gastos/*`.
**Estado: parcial.** No existe "modificar gasto": si se carga mal un monto hay que borrar y volver a cargar.

#### 31. Dashboard
**Qué hace:** panel con período seleccionable (última semana, último mes, 6 meses, año, personalizado). Muestra 7 indicadores (venta total, costo de ventas, consumo, producción, gastos extra, costo total, ganancia neta con % de margen), un gráfico de líneas venta vs ganancia, una dona con la distribución del costo, el top 5 de productos por facturación y los gastos del período.
**Archivos:** `Controllers/DashboardController.cs`, `Repositorios/DashboardRepository.cs`, `Views/Dashboard/Index.cshtml`.
**Estado: completa.**

#### 32–34. Home / Privacy / Error — **código muerto**
`Views/Home/Index.cshtml` sigue siendo la plantilla por defecto de ASP.NET ("Welcome / Learn about building Web apps with ASP.NET Core"), `Views/Home/Privacy.cshtml` dice literalmente *"Use this page to detail your site's privacy policy"* y `Views/Shared/Error.cshtml` está íntegramente en inglés. Ninguna de las tres es alcanzable desde el menú lateral. `Home/Error` sólo se muestra fuera de entorno de desarrollo (`Program.cs:57-61`).

---

#### Código muerto adicional (verificado con búsqueda en todo el repo)

| Elemento | Ubicación | Evidencia |
|---|---|---|
| Búsqueda AJAX de productos del `site.js` global | `wwwroot/js/site.js:40-65` | Llama a `/Productos/_BuscarProductos`, acción **que no existe**. El controlador tiene `FiltrarProductos`. Da 404 y sólo se registra en la consola del navegador. |
| `MapearAltaViewModelAEntidad` | `Controllers/ComprasController.cs:207-211` | Sin llamadores. |
| `MapearModificarViewModelAEntidad` | `Controllers/ComprasController.cs:213-225` | Sin llamadores. |
| `Compras.CrearDesdeViewModel` (método de instancia) | `Models/Compras.cs:32-36` | Sin llamadores; el controlador construye `new Compras(...)` a mano. |
| `Ventas.ActualizarDetalles` | `Models/Ventas.cs:69-111` | Sin llamadores (43 líneas de lógica de diff de detalles nunca usada). |
| `Compras.ActualizarDetalles` | `Models/Compras.cs:51-72` | Sin llamadores. |
| `Promociones.CalcularStock()` | `Models/Promociones.cs:71-74` | Sin llamadores. Usa `Math.Floor` (stock entero de combos); la versión que **sí** se usa (`VentasPromociones.CalcularStock`, `Models/VentasPromociones.cs:34-37`) no redondea. |
| `Productos.CalcularGanancia()` / `CalcularPorcentajeGanancia()` | `Models/Productos.cs:67-79` | Sólo los usa el constructor `ListarProductosVM(Productos)`, que **no se invoca en ninguna parte**. Las mismas fórmulas están reescritas en SQL en `Repositorios/ProductosRepository.cs:28-29` y `87-88`. |
| `ModificarPromocionVM.CalcularGanancia()` / `CalcularPorcentajeGanancia()` | `ViewModels/Promociones/ModificarPromocionViewModel.cs:61-68` | Sólo se llaman entre sí; la vista calcula en JavaScript. |
| `CG.CulturaUS` | `Models/Cultura.cs:10` | Sin usos. |
| `AltaVentaVM.Productos` y `.Promociones` | `ViewModels/Ventas/AltaVentaViewModel.cs:36-37` | Nunca se pueblan ni se leen. En `Views/Promociones/Alta.cshtml:10` hay incluso un `<input asp-for="Productos" type="hidden">` que serializa una lista vacía. |
| `ListarVentasVM.IdMetodo` | `ViewModels/Ventas/ListarVentasViewModel.cs:29` | Nunca se asigna. |
| `IndexVentasVM` ctors con parámetros | `ViewModels/Ventas/IndexVenta.cs:27-42` | Sin llamadores. Ídem `IndexComprasVM` (`ViewModels/Compras/IndexCompra.cs:21-32`). |
| Clases CSS `.confirm-card`, `.page-header` | `wwwroot/css/site.css:429-442, 536-554` | Definidas, ninguna vista las usa. |
| `.stat-chip .sublabel` | Usado en `Views/Dashboard/Index.cshtml:52,61,66,75,80` | **No está definido** en `site.css`: hereda estilo por defecto. |

#### Funcionalidad esbozada, no funcional

**Costo pendiente / umbral de stock.** `Productos.CostoPendiente` y `Productos.StockUmbral` existen como propiedades, están mapeadas a columnas (`AppDbContext.cs:37-38`) y tienen documentación XML: *"Costo que se activará cuando el stock baje del umbral (gestionado por el trigger de Postgres)"* (`Models/Productos.cs:12-15`). **No hay ninguna otra referencia en todo el código**: ninguna vista las muestra, ningún formulario las carga, ningún repositorio las lee. Es la mitad de una funcionalidad de costeo por lotes (cuando se agota el stock viejo, entra a regir el costo de la compra nueva). Si el trigger que la implementa existe, vive únicamente en la base de datos de producción; no está en `db/`.

---

### A2. Mapa de pantallas

Ruteo: `{controller=Ventas}/{action=Alta}` (`Program.cs:75-77`). No hay áreas ni rutas personalizadas. Toda la navegación estable pasa por la barra lateral de `Views/Shared/_Layout.cshtml`.

#### Árbol de navegación

```
[Arranque]  http://localhost:5146  ─────────────►  Ventas/Alta   (pantalla inicial)
                                                    (el ejecutable abre el navegador solo)

BARRA LATERAL (siempre visible en escritorio; oculta y sin sustituto en móvil)
│
├─ GENERAL
│  └─ 📊 Dashboard ................ /Dashboard/Index
│                                    └─ enlace "Ver todos" → /Gastos/Index
│
├─ PRINCIPAL
│  ├─ 🛒 Nueva Venta .............. /Ventas/Alta
│  │                                ├─ "Cancelar" → /Ventas/Index
│  │                                └─ [POST] → /Ventas/Index
│  └─ 📋 Ventas ................... /Ventas/Index
│                                   ├─ FAB "+" → /Ventas/Alta
│                                   ├─ "Resetear" → /Ventas/ResetearFiltros → /Ventas/Index
│                                   ├─ "Modificar" → /Ventas/Modificar/{id}
│                                   └─ "Eliminar"  → /Ventas/Eliminar/{id}  → [POST] → /Ventas/Index
│
├─ INVENTARIO
│  ├─ 📦 Productos ................ /Productos/Index
│  │                                ├─ FAB "+" → /Productos/Alta
│  │                                ├─ "Info"      → /Productos/Estadisticas/{id}
│  │                                ├─ "Modificar" → /Productos/Modificar/{id}
│  │                                ├─ "Eliminar"  → /Productos/Eliminar/{id}
│  │                                └─ "Copiar Lista de Precios" (sin navegación)
│  ├─ 🛍️ Compras .................. /Compras/Index
│  │                                ├─ FAB "+" → /Compras/Alta
│  │                                ├─ "Editar"   → /Compras/Modificar/{id}
│  │                                └─ "Eliminar" → /Compras/Eliminar/{id}
│  ├─ 🍽️ Consumo .................. /Consumo/Index
│  │                                ├─ FAB "+" → /Consumo/Alta
│  │                                ├─ "Modificar" → /Consumo/Modificar/{id}
│  │                                └─ "Eliminar" (POST directo con confirm())
│  ├─ 🏭 Producción ............... /Produccion/Index   (igual estructura que Consumo)
│  └─ 🚚 Proveedores .............. /Proveedores/Index
│                                   ├─ FAB "+" → /Proveedores/Alta
│                                   ├─ "Modificar" → /Proveedores/Modificar/{id}
│                                   └─ "Eliminar"  → /Proveedores/Eliminar/{id}
│
└─ CONFIGURACIÓN
   ├─ 🎁 Promociones .............. /Promociones/Index
   │                                ├─ FAB "+" → /Promociones/Alta
   │                                ├─ "Modificar" → /Promociones/Modificar/{id}
   │                                └─ "Eliminar"  → /Promociones/Eliminar/{id}
   ├─ 💳 Métodos de Pago .......... /MetodosPago/Index
   │                                ├─ FAB "+" → /MetodosPago/Alta
   │                                ├─ "Modificar" → /MetodosPago/Modificar/{id}
   │                                └─ "Eliminar"  → /MetodosPago/Eliminar/{id}
   └─ 💸 Gastos ................... /Gastos/Index
                                    ├─ FAB "+" → /Gastos/Alta
                                    └─ "Eliminar" (POST directo con confirm())

HUÉRFANAS (no alcanzables desde la interfaz)
   /Home/Index      — plantilla por defecto de ASP.NET, en inglés
   /Home/Privacy    — plantilla por defecto, en inglés
   /Home/Error      — sólo si el entorno no es Development
```

#### Detalle por pantalla

| Pantalla | Ruta | Muestra | Acciones |
|---|---|---|---|
| Nueva Venta | `Ventas/Alta` | Método de pago, fecha, hora, recargo %, redondeo $; líneas de producto y de promoción con subtotal; panel de resumen con subtotal, recargo, total y tres vueltos | Agregar/quitar línea, buscar producto, buscar promoción, registrar, cancelar |
| Ventas | `Ventas/Index` | Filtros (fecha desde/hasta, turno, método, texto); 4 chips (cantidad, costo, venta, ganancia); tabla N°, productos/promos, método, total, fecha y hora, detalle | Filtrar, resetear, modificar, eliminar, nueva venta |
| Modificar Venta | `Ventas/Modificar/{id}` | Igual que Alta pero con producto fijo por línea y sin vueltos | Cambiar cantidades, agregar/quitar líneas, guardar, cancelar |
| Eliminar Venta | `Ventas/Eliminar/{id}` | Ficha: id, fecha y hora, método, ítems, detalle, total | Confirmar, cancelar |
| Productos | `Productos/Index` | Filtros (orden, búsqueda, ver inactivos); tabla producto, proveedor, stock, costo, precio, ganancia, % ganancia, venta semanal | Copiar lista de precios, filtrar, resetear, estadísticas, modificar, eliminar, nuevo |
| Nuevo Producto | `Productos/Alta` | Nombre, proveedor (autocompletado), stock, costo, precio | Guardar, cancelar |
| Modificar Producto | `Productos/Modificar/{id}` | Lo anterior + switch activo | Guardar, cancelar |
| Eliminar Producto | `Productos/Eliminar/{id}` | Ficha: producto, proveedor, stock, costo, precio | Confirmar, cancelar |
| Estadísticas Producto | `Productos/Estadisticas/{id}` | Nombre y proveedor; filtro de fechas; total vendido; tabla fecha, hora, cantidad, origen | Filtrar, volver |
| Compras | `Compras/Index` | Filtros (fechas, proveedor); chips cantidad y total; tabla N°, productos, proveedor, fecha, total, estado (Pagada/Pendiente), detalle | Filtrar, resetear, editar, eliminar, nueva |
| Nueva/Modificar Compra | `Compras/Alta`, `Compras/Modificar/{id}` | Proveedor, fecha, switch pagada; líneas producto/cantidad/costo con subtotal; total | Agregar/quitar línea, guardar, cancelar |
| Eliminar Compra | `Compras/Eliminar/{id}` | Ficha con aviso de que se descontará el stock | Confirmar, cancelar |
| Consumo | `Consumo/Index` | Filtro de fechas; chips registros y costo total; tabla fecha, hora, productos, costo, detalle | Filtrar, modificar, eliminar, nuevo |
| Alta/Modificar Consumo | `Consumo/Alta`, `Consumo/Modificar/{id}` | Fecha (+hora al modificar), detalle; líneas producto/cantidad/costo (costo sólo lectura); panel resumen | Agregar/quitar línea, guardar, cancelar |
| Producción | `Produccion/*` | Idéntico a Consumo, etiquetas "retiro de producción" | Ídem |
| Proveedores | `Proveedores/Index` | Buscador y filtro de deuda; tabla proveedor, contacto, deuda | Buscar, filtrar, modificar, eliminar, nuevo |
| Alta/Modificar Proveedor | `Proveedores/Alta`, `Proveedores/Modificar/{id}` | Nombre, contacto, saldo | Guardar, cancelar |
| Promociones | `Promociones/Index` | Buscador y switch "ver inactivas"; tabla promoción, productos, stock, costo, precio, ganancia, % ganancia, inicio/fin, estado | Buscar, modificar, eliminar, nueva |
| Alta/Modificar Promoción | `Promociones/Alta`, `Promociones/Modificar/{id}` | Nombre, inicio, fin; receta de productos con cantidades; panel con precio, costo total, ganancia y % | Agregar/quitar producto, guardar, cancelar |
| Métodos de Pago | `MetodosPago/Index` + Alta/Modificar/Eliminar | Nombre del método | ABM |
| Gastos | `Gastos/Index` | Filtro de fechas; chips cantidad y total; tabla fecha, concepto, monto, observación | Filtrar, eliminar, nuevo |
| Nuevo Gasto | `Gastos/Alta` | Fecha, monto, concepto, observación | Guardar, cancelar |
| Dashboard | `Dashboard/Index` | 5 botones de período + fechas personalizadas; 7 KPIs; gráfico de líneas; dona; top 5 productos; gastos del período | Cambiar período, ver todos los gastos |

---

### A3. Reglas de negocio del rubro

Esta es la sección que más conocimiento contiene. Advertencia de partida: **una parte sustancial de las reglas no está en el código C# sino en triggers de PostgreSQL, y sólo una parte de esos triggers está versionada en el repositorio.** Ver regla 8.

---

#### Regla 1 — El stock se lleva con tres decimales, y esa es toda la unidad de medida que existe

```
// AppDbContext.cs:33
entity.Property(e => e.Stock).HasColumnName("stock").HasColumnType("numeric(7,3)").IsRequired();
// AppDbContext.cs:148  (detalle_venta)
entity.Property(e => e.Cantidad).HasColumnName("cantidad").HasColumnType("numeric(6,3)").IsRequired();
// AppDbContext.cs:85   (detalle_compra)
entity.Property(e => e.Cantidad).HasColumnName("cantidad").HasColumnType("numeric(7,3)").IsRequired();
```

Validación en formularios: `[Range(0.001, 999.999)]` en la cantidad de venta (`ViewModels/DetallesVentas/DetalleVentaViewModel.cs:11`), `[Range(0.001, 9999.999)]` en compra, `[Range(0, 9999.999)]` en el stock del producto.

**Qué significa para el dueño:** no hay concepto de "unidad" ni de "kilo" en ningún lado del sistema. **No existe una sola palabra "kilo", "kg", "gramo", "unidad" ni "peso" en todo el código** (verificado por búsqueda). El sistema maneja un único número con tres decimales por producto y el usuario decide implícitamente qué significa: si el producto se llama "Pollo entero" probablemente sea unidades, y si se llama "Pechuga" probablemente sean kilos. Los tres decimales existen justamente para poder vender **1,250 kg** de algo. La consecuencia práctica: un mismo producto no puede venderse a veces por unidad y a veces por peso, y ningún informe puede distinguir "vendí 12 kilos" de "vendí 12 unidades". Todo el redondeo de fracciones queda a criterio de quien tipea.

**Límite duro:** `numeric(7,3)` topea el stock en **9.999,999**. Y `[Range(0.001, 999.999)]` impide vender más de 999,999 en una sola línea.

---

#### Regla 2 — El stock lo mueve la base de datos, no la aplicación

Los tres triggers de `db/polleria.sql` son el corazón del sistema de inventario. El código C# nunca escribe la columna `stock`; sólo la lee.

**Venta** (`db/polleria.sql:67-128`, trigger `tr_actualizar_stock_venta` sobre `detalle_venta`, BEFORE INSERT/UPDATE/DELETE):

```sql
WHEN 'INSERT' THEN
    SELECT stock INTO stock_disponible FROM producto WHERE id_producto = NEW.id_producto FOR UPDATE;
    IF stock_disponible < NEW.cantidad THEN
        RAISE EXCEPTION 'Stock insuficiente para el producto (ID: %). Stock disponible: %', ...
    END IF;
    UPDATE producto SET stock = stock - NEW.cantidad WHERE id_producto = NEW.id_producto;

WHEN 'DELETE' THEN
    UPDATE producto SET stock = stock + OLD.cantidad WHERE id_producto = OLD.id_producto;

WHEN 'UPDATE' THEN   -- sólo si cambió la cantidad
    SELECT stock + OLD.cantidad INTO stock_disponible FROM producto WHERE ... FOR UPDATE;
    IF stock_disponible < NEW.cantidad THEN RAISE EXCEPTION ... END IF;
    UPDATE producto SET stock = stock + OLD.cantidad - NEW.cantidad WHERE ...;
```

**Compra** (`db/polleria.sql:10-65`, trigger `tr_actualizar_stock_compra` sobre `detalle_compra`):

```sql
WHEN 'INSERT' THEN  UPDATE producto SET stock = stock + NEW.cantidad ...
WHEN 'DELETE' THEN  -- valida antes de restar
    IF (SELECT stock FROM producto WHERE id_producto = OLD.id_producto) < OLD.cantidad THEN
        RAISE EXCEPTION 'No hay stock suficiente para anular esta compra. El producto (ID: %) ya fue utilizado en ventas.';
    END IF;
    UPDATE producto SET stock = stock - OLD.cantidad ...
WHEN 'UPDATE' THEN  UPDATE producto SET stock = stock - OLD.cantidad + NEW.cantidad ...
```

**Qué significa para el dueño:**
- Vender descuenta stock; borrar una venta lo devuelve; cambiar la cantidad ajusta la diferencia.
- Comprar suma stock; **anular una compra sólo se permite si esa mercadería todavía está en el depósito**. Si ya se vendió, el sistema se niega con el mensaje *"No hay stock suficiente para anular esta compra. El producto (ID: 37) ya fue utilizado en ventas"*. Esto es una decisión de negocio deliberada: no se puede reescribir la historia de una compra ya consumida.
- El stock **nunca puede quedar negativo**: hay además una restricción `CHECK (stock >= 0)` (`db/polleria.sql:429-431`).
- La venta usa `FOR UPDATE` (bloqueo de fila) para evitar que dos cajas vendan el último pollo a la vez. La compra **no** usa `FOR UPDATE`.

---

#### Regla 3 — Doble validación de stock: la aplicación pregunta, la base decide

Antes de guardar una venta, el repositorio recorre los detalles y consulta el stock producto por producto:

```csharp
// Repositorios/VentaRepository.cs:149-179
private void ValidarStockParaVenta(Ventas venta)
{
    foreach (var detalle in venta.DetallesVenta)
    {
        var producto = _context.Productos.Find(detalle.IdProducto);
        if (producto == null || producto.Stock < detalle.Cantidad)
            throw new InvalidOperationException($"Stock insuficiente para el producto '{...}'. Stock disponible: {producto?.Stock ?? 0}.");
    }
    foreach (var ventaPromo in venta.VentaPromociones)
    {
        var promocion = _context.Promociones.Include(p => p.DetallesPromocion).AsNoTracking()
                                .FirstOrDefault(p => p.IdPromocion == ventaPromo.IdPromocion);
        if (promocion == null) continue;
        foreach (var detallePromo in promocion.DetallesPromocion)
        {
            var producto = _context.Productos.Find(detallePromo.IdProducto);
            var cantidadRequerida = ventaPromo.Cantidad * detallePromo.Cantidad;
            if (producto == null || producto.Stock < cantidadRequerida)
                throw new InvalidOperationException($"Stock insuficiente para el producto '...' dentro de la promoción '...'. Stock disponible: {...}, requerido: {cantidadRequerida}.");
        }
    }
}
```

**Qué significa:** la validación de la aplicación existe para dar un mensaje entendible ("no te queda pechuga") en vez del error crudo del trigger. La que realmente garantiza la consistencia es la de la base. **Importante:** esta validación existe sólo en el alta. Al modificar una venta (`VentaRepository.Actualizar`) no se llama, así que el usuario recibiría el mensaje técnico del trigger de PostgreSQL.

---

#### Regla 4 — Una promoción es una receta: vender el combo descuenta cada ingrediente

Trigger `tr_modificar_stock_venta_promocion` sobre `venta_promocion` (`db/polleria.sql:130-209`):

```sql
WHEN 'INSERT' THEN
    -- primero valida TODOS los ingredientes
    FOR detalle IN SELECT id_producto, cantidad FROM detalle_promocion WHERE id_promocion = NEW.id_promocion LOOP
        IF (SELECT stock FROM producto WHERE id_producto = detalle.id_producto) < (NEW.cantidad * detalle.cantidad) THEN
            RAISE EXCEPTION 'Stock insuficiente para el producto ID % y completar la promoción.', detalle.id_producto;
        END IF;
    END LOOP;
    -- después descuenta TODOS
    FOR detalle IN ... LOOP
        UPDATE producto SET stock = stock - (NEW.cantidad * detalle.cantidad) WHERE id_producto = detalle.id_producto;
    END LOOP;
```

**Fórmula: `descuento_de_stock = cantidad_de_combos_vendidos × cantidad_del_ingrediente_en_la_receta`.**

**Qué significa para el dueño:** si el "Combo Familiar" lleva 1 pollo entero + 2 kg de papas, y vendés 3 combos, el sistema descuenta 3 pollos y 6 kg de papas. Si falta cualquiera de los dos ingredientes, **no se puede vender el combo**, aunque sobre el otro. La validación es "todo o nada", en dos pasadas (primero verifica todo, después descuenta todo), para no dejar el inventario a medio descontar.

---

#### Regla 5 — Stock disponible de un combo = el ingrediente que primero se agota

```csharp
// Repositorios/PromocionesRepository.cs:35-38   (lo que se muestra en pantalla)
Stock = p.DetallesPromocion
         .Where(d => d.Cantidad > 0)
         .Select(d => d.Producto.Stock / d.Cantidad)
         .Min(),

// Models/VentasPromociones.cs:34-37   (lo que se muestra en el buscador de la caja)
public decimal CalcularStock()
    => Promocion.DetallesPromocion.Where(d => d.Cantidad > 0)
                .Select(d => d.Producto.Stock / d.Cantidad).Min();

// Models/Promociones.cs:71-74   (versión con redondeo hacia abajo — NUNCA SE USA)
public int CalcularStock()
    => (int)_detallesPromocion.Where(d => d.Cantidad > 0)
             .Select(d => Math.Floor(d.Producto.Stock / d.Cantidad)).Min();
```

**Fórmula: `stock_del_combo = MIN(stock_del_ingrediente ÷ cantidad_en_la_receta)`.**

**Qué significa:** "cuántos combos podría armar hoy". Si tengo 5 pollos y el combo lleva 1, y tengo 3 kg de papas y el combo lleva 2, puedo armar `MIN(5/1, 3/2) = MIN(5; 1,5) = 1,5` combos. **La versión que se usa no redondea**: la pantalla muestra "1,5 combos disponibles", que no significa nada para el negocio. La versión correcta —redondear hacia abajo a 1— está escrita en `Models/Promociones.cs` y **no se llama desde ningún lado**.

**Riesgo latente:** `.Min()` sobre una lista vacía. Si una promoción se queda sin ingredientes, esta consulta revienta y con ella se cae *toda* la pantalla de Promociones **y** el buscador de promociones de la caja. Es un escenario alcanzable: la clave foránea `detalle_promocion → producto` tiene `ON DELETE CASCADE` (`db/polleria.sql:452-454`), así que borrar un producto vacía la receta de las promociones que lo contenían.

---

#### Regla 6 — El costo de un combo se recalcula todo el tiempo; el precio de venta lo fija el dueño

```csharp
// Repositorios/PromocionesRepository.cs:29-33
Costo    = p.DetallesPromocion.Sum(d => d.Producto.Costo * d.Cantidad),
Ganancia = p.Precio - p.DetallesPromocion.Sum(d => d.Producto.Costo * d.Cantidad),
PorcentajeGanancia = (costo > 0) ? (p.Precio - costo) / costo : 0,
```

**Fórmulas:**
- `costo_del_combo = Σ (costo_actual_del_producto × cantidad_en_la_receta)`
- `ganancia = precio_del_combo − costo_del_combo`
- `% ganancia = ganancia ÷ costo` (**sobre el costo, no sobre la venta** — es un *markup*, no un margen)

**Qué significa:** el costo del combo no es un número guardado: se recalcula con el costo de hoy de cada ingrediente. Si sube el pollo, la ganancia del combo baja sola en la pantalla, sin tocar nada. Esto es correcto para decidir precios y peligroso para analizar el pasado: **cambiar el costo de un producto reescribe la rentabilidad histórica de todas las promociones.**

Ojo con la inconsistencia de fórmula: acá el % de ganancia se calcula como `ganancia/costo` **sin multiplicar por 100** y se muestra con formato `{0:P2}` (`ViewModels/Promociones/ListarPromocionesViewModel.cs:18`), que ya multiplica por 100. En productos, en cambio, se calcula `100 * (precio-costo)/costo` (`Repositorios/ProductosRepository.cs:29`) y se muestra con `ToString("N2") + "%"` (`Views/Productos/_ProductosTabla.cshtml:28`). Dos caminos distintos para el mismo indicador, ambos llegan al mismo número.

---

#### Regla 7 — Un producto nunca puede tener precio menor al costo

Tres capas independientes lo impiden:

```csharp
// ViewModels/Productos/AltaProductoViewModel.cs:41-47  y  ModificarProductoViewModel.cs:45-51
if (Precio <= Costo)
    yield return new ValidationResult("El precio no puede ser menor que el costo.", [nameof(Precio)]);
```
```sql
-- db/polleria.sql:262
CONSTRAINT precio_mayor_que_costo CHECK ((precio_producto >= costo_producto))
-- db/polleria.sql:370-372
ALTER TABLE public.detalle_venta ADD CONSTRAINT costo_menor_que_precio CHECK ((costo_unitario <= precio_unitario)) NOT VALID;
```

Y para las promociones, en el controlador:

```csharp
// Controllers/PromocionesController.cs:70-84
foreach (var detalleVM in viewModel.DetallesPromocion) {
    var producto = _productosRepo.ObtenerPorId(detalleVM.IdProducto);
    if (producto != null) costoTotalReal += producto.Costo * detalleVM.Cantidad;
}
if (viewModel.Precio < costoTotalReal)
    ModelState.AddModelError("Precio", $"El precio no puede ser menor que el costo total (${costoTotalReal:N2}).");
```

**Qué significa:** el sistema no deja vender a pérdida, ni por producto ni por combo. Nótese la asimetría: en C# la regla es `Precio <= Costo` es error (**precio igual al costo también se rechaza**), mientras que en la base es `precio >= costo` (**igual se acepta**). Y en `detalle_venta` la regla se aplica a la línea de venta histórica.

**Efecto colateral no obvio:** como `detalle_venta` guarda `costo_unitario` con `CHECK (costo_unitario <= precio_unitario)`, y el consumo interno se registra como una venta con precio 0 (regla 12), esa restricción **tiene que estar deshabilitada o eliminada en la base real**, porque si no ningún consumo se podría guardar. Está marcada `NOT VALID`, lo que en PostgreSQL significa que no se validaron las filas existentes pero **sí se aplica a las nuevas**. Esto es una contradicción entre el esquema versionado y la funcionalidad de consumo: no determinable desde el repo cuál de los dos refleja la base de producción.

---

#### Regla 8 — Comprar mercadería aumenta la deuda con el proveedor · ⚠️ REGLA CRÍTICA QUE NO ESTÁ EN EL REPOSITORIO

El archivo `db/2026-05-10-compra-pagada-trigger.sql` documenta y crea un trigger que ajusta la deuda cuando se marca una compra como pagada:

```sql
-- db/2026-05-10-compra-pagada-trigger.sql:29-47
IF OLD.pagada = FALSE AND NEW.pagada = TRUE THEN
    UPDATE public.proveedor
    SET debo = debo - (SELECT COALESCE(SUM(dc.cantidad * dc.costo_unitario), 0)
                       FROM public.detalle_compra dc WHERE dc.id_compra = NEW.id_compra)
    WHERE id_proveedor = NEW.id_proveedor;
ELSIF OLD.pagada = TRUE AND NEW.pagada = FALSE THEN
    UPDATE public.proveedor SET debo = debo + (...) WHERE id_proveedor = NEW.id_proveedor;
END IF;
```

Pero su propio comentario de cabecera revela la pieza que falta:

```
-- Por qué existe este trigger:
--   El repositorio, cuando solo cambian datos de cabecera (fecha,
--   proveedor, detalle, pagada) sin modificar los productos ni
--   cantidades, hace únicamente un UPDATE sobre la tabla compra
--   sin tocar detalle_compra. Por eso el trigger existente en
--   detalle_compra (tg_actualizar_deuda_proveedor) no se dispara.
```

**`tg_actualizar_deuda_proveedor` no existe en ningún archivo del repositorio.** No está en `db/polleria.sql` (que es anterior y ni siquiera tiene la columna `pagada`), no está en el código C#, no está en ninguna migración —no hay migraciones—. Es el trigger que implementa la regla de negocio *"cargar una compra suma su total a la deuda del proveedor"*, confirmada por el mensaje de commit `79bb6b0`: *"agregue produccion, consumo, dashboard y compras suma deuda a proveedores"*.

**Fórmula (inferida): `deuda_proveedor += Σ (cantidad × costo_unitario)` de los detalles de la compra, cuando la compra no está marcada como pagada.**

**Qué significa para el dueño:** la cuenta corriente con el proveedor se lleva sola. Cargás la compra sin tildar "ya fue pagada" y la deuda sube; cuando le pagás, tildás la compra y la deuda baja. La pantalla de proveedores ordena por deuda descendente y pinta en rojo a quien tenga saldo.

**Qué significa para el proyecto:** esta regla **se pierde si se reinstala el sistema desde el repositorio**. Es el hallazgo de conocimiento de dominio más grave de la auditoría.

---

#### Regla 9 — La deuda con el proveedor nunca puede ser negativa

```sql
-- db/polleria.sql:382-384
ALTER TABLE public.proveedor ADD CONSTRAINT debo_positivo CHECK ((debo >= (0)::numeric)) NOT VALID;
```
```csharp
// ViewModels/Proveedores/AltaProveedorViewModel.cs:13
[Range(0, 9999999.99, ErrorMessage = "El saldo no puede ser negativo.")]
```

**Qué significa:** no se modela "pagué de más y el proveedor me debe". El saldo a favor no existe. Techo de deuda: `numeric(9,2)` = **$9.999.999,99**.

---

#### Regla 10 — No se puede borrar un proveedor con deuda ni con productos

```csharp
// Controllers/ProveedoresController.cs:125-134
if (!proveedorVM.EsEliminable)
    TempData["ErrorMessage"] = "No se puede eliminar el proveedor " + proveedorVM.Proveedor
        + ", es referenciado en los productos " + proveedorVM.ProductosReferenciados + ".";
if (proveedorVM.Debo > 0)
    TempData["ErrorMessage"] = "No se puede eliminar el proveedor " + proveedorVM.Proveedor
        + " ya que todavia tiene una deuda.";
```

**Qué significa:** el sistema protege la cuenta corriente: no podés hacer desaparecer a quien te fía.

---

#### Regla 11 — La caja registra tres cosas distintas en la misma tabla

```csharp
// Models/Ventas.cs:12
public string Tipo { get; set; } = "venta"; // venta | consumo | produccion

// Models/Ventas.cs:42-48
// Crea un egreso interno (consumo o produccion) que usa la infra de venta sin precio de venta
public static Ventas CrearEgreso(string tipo, short idMetodo, DateOnly fecha, TimeOnly hora,
                                 string? detalle, List<DetallesVentas> detalles)
{
    var v = new Ventas(idMetodo, fecha, hora, detalle, 0, detalles, new List<VentasPromociones>());
    v.Tipo = tipo;
    return v;
}

// Models/DetallesVentas.cs:27-31
// Para consumo (precio=0) y produccion (costo=precio=0)
public static DetallesVentas CrearParaEgreso(int idProducto, decimal cantidad, decimal costoUnitario)
    => new DetallesVentas(idProducto, cantidad, costoUnitario, 0);
```

**Qué significa para el dueño:** hay tres formas de que salga mercadería del negocio y las tres descuentan stock igual, pero cuentan distinto en las cuentas:
1. **`venta`** — se cobró. Entra a la facturación y a la ganancia.
2. **`consumo`** — se lo comió el negocio. No factura, pero **sí es costo**: entra al "Costo Total" del dashboard.
3. **`produccion`** — se retiró para elaborar. No factura y **no se cuenta como costo**; el dashboard lo muestra sólo "como referencia" (`Views/Dashboard/Index.cshtml:66`).

La razón de que producción no sume al costo es que **es un movimiento interno**: la materia prima que sale ya se contabilizó cuando se compró, y lo que se produce con ella se vende después como otro producto. Contarla sería contar el costo dos veces. Es una decisión de negocio correcta, y está documentada en una sola palabra en la vista.

**Cómo se distinguen en el listado de ventas:** `Repositorios/VentaRepository.cs:37` filtra `v.Tipo == "venta"`, así que consumos y producciones no ensucian la caja del día.

---

#### Regla 12 — El consumo y la producción se imputan a un método de pago llamado "Yo"

```csharp
// Repositorios/ConsumoRepository.cs:10-18  (idéntico en ProduccionRepository.cs:10-17)
private short ObtenerIdMetodoYo()
{
    // Usa "Yo" si existe, sino cualquier método disponible
    return _context.MetodosPago
        .OrderBy(m => m.Metodo == "Yo" ? 0 : 1)
        .ThenBy(m => m.IdMetodo)
        .Select(m => m.IdMetodo)
        .First();
}
```

Y el listado de ventas descuenta esas operaciones del conteo:

```javascript
// Views/Ventas/Index.cshtml:129-140
if (metodoPago === "Yo") {
    cantidadGeneral--;
} else {
    totalVenta += valorVenta;
    totalCosto += valorCosto;
}
...
if ($('.filtroMetodoPago option:selected').text().trim() === "Yo") {
    cantidadGeneral = cantidadConYo;   // si el usuario filtra por "Yo", los vuelve a contar
}
```

**Qué significa:** "Yo" es el método de pago ficticio del dueño. Todo lo que sale sin cobrarse queda imputado ahí. En el listado de ventas, las operaciones "Yo" **no suman a la venta ni al costo del día y no se cuentan en la cantidad**, salvo que el usuario filtre explícitamente por "Yo", en cuyo caso sí se cuentan. Es una regla real de la pollería, escrita en JavaScript, en una vista, con el nombre del método comparado como texto.

**Fragilidad:** si alguien renombra el método "Yo" desde la pantalla de Métodos de Pago, esta regla deja de aplicarse en silencio y las cuentas del día cambian. Y si no existe ningún método llamado "Yo", el consumo se imputa al método de menor id, que probablemente sea "Efectivo".

---

#### Regla 13 — El redondeo es un campo manual, en pesos, que puede ser negativo

```csharp
// ViewModels/Ventas/AltaVentaViewModel.cs:29-32
[Required(ErrorMessage = "El campo redondeo no puede estar vacio")]
[Range(-999999.99, 999999.99, ErrorMessage = "El redondeo debe estar entre -999.999,99 y 999.999,99.")]
public decimal Redondeo { get; set; } = 0;

// Models/Ventas.cs:135-138
public decimal CalcularPrecioTotal()
    => _detallesVenta.Sum(d => d.CalcularPrecio())
     + _ventaPromociones.Sum(p => p.CalcularPrecio())
     + Redondeo;
```

**Fórmula: `total_de_la_venta = Σ(precio_unitario × cantidad de cada producto) + Σ(precio_promo × cantidad de cada combo) + redondeo`.**

**Qué significa:** el cajero puede sumar o restar pesos a mano a cada venta. Se usa para redondear a un número cómodo ("son $8.437, dejámelo en $8.400" → redondeo = −37) o para un recargo/descuento puntual. Se guarda en la venta (`numeric(8,2)`), aparece en el listado y forma parte del total histórico. **Es el único mecanismo de descuento que tiene el sistema.**

Contrasta con el **Recargo (%)**, que se ve en pantalla pero no se guarda (ver A1, funcionalidad 1). El redondeo es real; el recargo es humo.

---

#### Regla 14 — Una promoción está "activa" si no tiene fecha de fin o si su fin es posterior a hoy

```csharp
// Repositorios/PromocionesRepository.cs:31
Activa = !p.Fin.HasValue || p.Fin.Value > hoy,

// Models/Promociones.cs:75-78
public void Desactivar() { Fin = DateOnly.FromDateTime(DateTime.Now); }
```

Pero la lista de precios usa otro criterio:

```csharp
// Repositorios/PromocionesRepository.cs:98-99
.Where(p => p.Inicio <= hoy && (p.Fin == null || p.Fin >= hoy))
```

**Qué significa y dónde está mal:** "desactivar" una promoción es ponerle fecha de fin = hoy, y con el criterio `Fin > hoy` deja de estar activa el mismo día. Coherente. Pero hay **dos definiciones incompatibles de "promoción vigente"** conviviendo:

| | ¿Mira `Inicio`? | Promo que termina hoy |
|---|---|---|
| `Activa` (listado, buscador de la caja) | **No** | Inactiva |
| `ObtenerPromocionesActivasParaLista` (lista de precios WhatsApp) | Sí | **Activa** |

Consecuencias reales: **(a)** una promoción con fecha de inicio en el futuro figura como "Activa" y **se puede vender hoy**, porque `BuscarPromocionesParaVenta` filtra por `p.Activa` (`Controllers/VentasController.cs:253`); **(b)** una promoción dada de baja hoy sigue apareciendo en la lista de precios que se manda por WhatsApp ese día.

Restricción en la base: `CONSTRAINT inicio_antes_que_fin CHECK ((inicio <= fin))` (`db/polleria.sql:271`). En PostgreSQL, con `fin IS NULL` el CHECK da NULL y **pasa**, que es lo buscado: promoción sin vencimiento.

---

#### Regla 15 — Una promoción ya vendida no puede cambiar de receta

```csharp
// Controllers/PromocionesController.cs:188-199
var esModificable = _promocionesRepo.PuedeSerEliminada(promocion.IdPromocion);
if (esModificable)
    promocion.ActualizarDesdeViewModel(viewModel);      // cambia todo, receta incluida
else
    promocion.ActualizarDatosGenerales(viewModel);      // sólo nombre, precio y fechas

// Repositorios/PromocionesRepository.cs:73-76
public bool PuedeSerEliminada(int id) => !_context.VentasPromociones.Any(vp => vp.IdPromocion == id);
```

Y el usuario lo ve explicado en pantalla (`Views/Promociones/Modificar.cshtml:222-224`): *"Esta promoción ya fue utilizada en ventas, por lo que su 'receta' (productos y cantidades) no puede ser alterada. Solo puedes modificar el nombre, precio y las fechas de vigencia."*

**Qué significa:** protege el histórico. Si cambiara la receta de un combo ya vendido, el costo de las ventas pasadas se recalcularía mal.

---

#### Regla 16 — Precio y costo se congelan en la línea de venta

```csharp
// AppDbContext.cs:149-150
entity.Property(e => e.CostoUnitario).HasColumnName("costo_unitario").HasColumnType("numeric(8,2)").IsRequired();
entity.Property(e => e.PrecioUnitario).HasColumnName("precio_unitario").HasColumnType("numeric(8,2)").IsRequired();
// AppDbContext.cs:166-167  (promociones vendidas)
entity.Property(e => e.CostoPromo).HasColumnName("costo_promo")...
entity.Property(e => e.PrecioPromo).HasColumnName("precio_promo")...
```

**Qué significa:** cada línea de venta guarda a cuánto se vendió y a cuánto costaba **en ese momento**. Cambiar el precio de lista de un producto no altera las ventas de ayer. Es lo correcto y está bien hecho.

**Pero hay una fuga:** al reconstruir el formulario de modificación de una venta, el sistema **descarta los valores históricos y vuelve a leer los actuales**:

```csharp
// ViewModels/DetallesVentas/DetalleVentaViewModel.cs:26-33
public DetalleVentaVM(DetallesVentas detalle)
{
    IdProducto = detalle.IdProducto;
    Cantidad   = detalle.Cantidad;
    CostoUnitario  = detalle.Producto.Costo;    // <-- costo de HOY, no el de la venta
    PrecioUnitario = detalle.Producto.Precio;   // <-- precio de HOY
}
```
Lo mismo hacen `RepoblarViewModelParaModificarVenta` (`Controllers/VentasController.cs:338-345`) y `RepoblarViewModelParaAltaVenta` (`:164-171`). **Consecuencia: si se abre y guarda una venta vieja para corregir cualquier cosa, esa venta queda revalorizada a precios de hoy.** Es una pérdida de dato histórico silenciosa.

---

#### Regla 17 — "Virtuales" es todo lo que no se llame exactamente "Efectivo"

```csharp
// Repositorios/VentaRepository.cs:42-48
// -1 = Virtuales (todo menos efectivo), -2 = solo Efectivo, >0 = método específico
if (filtro.IdMetodoPago == -1)      query = query.Where(v => v.Metodo.Metodo != "Efectivo");
else if (filtro.IdMetodoPago == -2) query = query.Where(v => v.Metodo.Metodo == "Efectivo");
else                                query = query.Where(v => v.IdMetodo == filtro.IdMetodoPago.Value);
```

**Qué significa:** al cierre del día el dueño quiere separar lo que tiene en la caja de lo que le entró al banco/billetera. La clasificación se hace comparando el **nombre** del método con la palabra "Efectivo". Nótese que el método "Yo" (consumo propio) cae dentro de "Virtuales" según este filtro. Los ids negativos −1 y −2 son valores centinela metidos en el mismo campo que los ids reales.

---

#### Regla 18 — Los turnos de la pollería son 08:00–14:00 y 17:30–22:00

```csharp
// Controllers/VentasController.cs:48-53
viewModel.Turno = horaActual switch
{
    var h when h >= new TimeOnly(8, 0)    && h <= new TimeOnly(14, 0) => IndexVentasVM.Turnos.Mañana,
    var h when h >= new TimeOnly(17, 30)  && h <= new TimeOnly(22, 0) => IndexVentasVM.Turnos.Tarde,
    _ => IndexVentasVM.Turnos.Todos
};

// Repositorios/VentaRepository.cs:18-32
case Turnos.Mañana: horaInicio = new TimeOnly(8,0);    horaFin = new TimeOnly(14,0);  break;
case Turnos.Tarde:  horaInicio = new TimeOnly(17,30);  horaFin = new TimeOnly(22,0);  break;
default:            horaInicio = new TimeOnly(0,0);    horaFin = new TimeOnly(23,59); break;
```

**Qué significa:** es el horario de atención real del local, hardcodeado en dos lugares. Cuando el cajero abre el listado de ventas, el sistema adivina en qué turno está y filtra sólo ese turno. Es el sustituto de un "cierre de caja por turno": **no hay arqueo, no hay apertura ni cierre, no hay caja como entidad** (ver A5). El turno es sólo un filtro de hora sobre la tabla de ventas.

**Detalle:** el turno "Todos" filtra `hora <= 23:59`, así que una venta registrada a las 23:59:30 queda fuera del listado.

---

#### Regla 19 — Ranking por venta de la última semana

```csharp
// Repositorios/ProductosRepository.cs:15-16, 33-42
var unaSemanaAtras = DateOnly.FromDateTime(DateTime.Now.AddDays(-7));
var hoy = DateOnly.FromDateTime(DateTime.Now);
VentaSemanal = _context.DetallesVentas
    .Where(dv => dv.IdProducto == p.IdProducto && _context.Ventas.Any(v => v.IdVenta == dv.IdVenta
                 && v.Fecha >= unaSemanaAtras && v.Fecha <= hoy))
    .Select(dv => (decimal?)dv.Cantidad)
    .Concat(_context.VentasPromociones
        .Where(vp => _context.Ventas.Any(v => v.IdVenta == vp.IdVenta && v.Fecha >= unaSemanaAtras && v.Fecha <= hoy))
        .SelectMany(vp => _context.DetallesPromociones
            .Where(dp => dp.IdPromocion == vp.IdPromocion && dp.IdProducto == p.IdProducto)
            .Select(dp => (decimal?)vp.Cantidad * dp.Cantidad)))
    .Sum() ?? 0
```

**Fórmula: `venta_semanal = Σ cantidades vendidas sueltas (últimos 7 días) + Σ (combos vendidos × cantidad en la receta)`.**

**Qué significa:** los productos que más se movieron esta semana **aparecen primero en el autocompletado de la caja** (`Controllers/VentasController.cs:235`, `.OrderByDescending(p => p.VentaSemanal)`). Es una optimización de velocidad de tipeo pensada para el mostrador: lo que más se vende, menos hay que buscarlo. **Es una de las mejores ideas de producto que tiene el sistema.** Incluye el consumo dentro de promociones, no sólo la venta suelta. No excluye los movimientos tipo "consumo"/"produccion".

---

#### Regla 20 — Reparto proporcional del precio de un combo entre sus productos (dashboard)

```csharp
// Repositorios/DashboardRepository.cs:143-155
var precioListaPromo = detallesPromo.Sum(d => d.Producto.Precio * d.Cantidad);
foreach (var detallePromo in detallesPromo)
{
    var proporcion = precioListaPromo > 0
        ? (detallePromo.Producto.Precio * detallePromo.Cantidad) / precioListaPromo
        : 1m / detallesPromo.Count;

    movimientosProductos.Add(new VM.TopProductoVM {
        Producto   = detallePromo.Producto.Producto,
        Cantidad   = ventaPromo.Cantidad * detallePromo.Cantidad,
        TotalVenta = ventaPromo.PrecioPromo * ventaPromo.Cantidad * proporcion,
    });
}
```

**Fórmula: `venta_imputada_al_producto = precio_del_combo × combos_vendidos × (precio_lista_del_ingrediente × cantidad) ÷ (precio_lista_total_de_la_receta)`.**

**Qué significa:** para el ranking "Top Productos", cuando se vende un combo hay que decidir cuánta de esa plata le corresponde a cada ingrediente. El sistema reparte **en proporción al precio de lista** de cada uno: si el pollo vale el 70% de lo que valdría comprar los ingredientes sueltos, se le imputa el 70% de lo facturado por el combo. Si no hay precios (todo en 0), reparte en partes iguales. Es una decisión de asignación de ingresos razonable y bien pensada; no está documentada en ningún otro lado.

---

#### Regla 21 — Cómo se arma la ganancia del negocio (dashboard)

```csharp
// ViewModels/Dashboard/DashboardViewModel.cs:44-47
public decimal CostoTotal => TotalCostoVentas + TotalCostoConsumo + TotalGastos;
public decimal Ganancia   => TotalVenta - CostoTotal;
public decimal MargenPct  => TotalVenta > 0 ? Math.Round(Ganancia / TotalVenta * 100, 1) : 0;
public decimal CostoPct   => TotalVenta > 0 ? Math.Round(CostoTotal / TotalVenta * 100, 1) : 0;
```

**Fórmulas:**
- `costo_total = costo_de_la_mercadería_vendida + costo_del_consumo_interno + gastos_extra`
- `ganancia_neta = facturación − costo_total`
- `margen % = ganancia ÷ facturación × 100` (sobre la venta)
- `costo % = costo_total ÷ facturación × 100`

**Qué significa:** la ganancia del negocio es lo que entró por ventas menos (lo que costó esa mercadería + lo que se comió el negocio + luz, gas, bolsas). **El retiro para producción queda deliberadamente afuera** — la etiqueta en pantalla lo dice: *"@Model.CostoPct% de la venta, sin produccion"* (`Views/Dashboard/Index.cshtml:75`). El margen se mide sobre la venta; el % de ganancia de productos y promociones se mide sobre el costo (regla 6). **Son dos indicadores distintos con nombres parecidos.**

En la serie histórica del gráfico, el costo por período incluye consumos y gastos pero **no** producción (`Repositorios/DashboardRepository.cs:77, 96, 115`: `costo = vDia.Sum(CalcularCostoTotal) + cDia + gDia`).

---

#### Regla 22 — Agrupación temporal automática del gráfico

```csharp
// Repositorios/DashboardRepository.cs:63-120
var dias = (fechaFin - fechaInicio).Days;
if (dias <= 31)        // agrupar por día
else if (dias <= 180)  // agrupar por semana, arrancando el lunes
else                   // agrupar por mes
```
```csharp
// Repositorios/DashboardRepository.cs:85
var firstMonday = fechaInicio.AddDays(-(int)fechaInicio.DayOfWeek + 1);
```

**Qué significa:** hasta un mes se ve día por día; hasta seis meses, semana por semana (semana que empieza el lunes); más allá, mes por mes.

**Bug:** `AddDays(-(int)DayOfWeek + 1)` para un **domingo** (`DayOfWeek = 0`) da `+1 día`, o sea el lunes **siguiente**, no el anterior. Si el rango arranca un domingo, ese domingo queda fuera del gráfico.

---

#### Regla 23 — Períodos de análisis del negocio

```csharp
// Controllers/DashboardController.cs:21-28
"semana"   => (hoy.AddDays(-6), hoy),
"semestre" => (hoy.AddMonths(-6), hoy),
"anio"     => (hoy.AddYears(-1), hoy),
"custom"   => (customInicio ?? hoy.AddDays(-30), customFin ?? hoy),
_          => (hoy.AddMonths(-1), hoy),   // "mes" por defecto
```

Y los períodos por defecto de las otras pantallas: consumo, producción y gastos abren con **los últimos 30 días**; estadísticas de producto con **el último mes**; ventas y compras abren con **hoy solamente**.

---

#### Regla 24 — Un producto pertenece a un único proveedor, y las compras se filtran por eso

```csharp
// AppDbContext.cs:31
entity.Property(e => e.IdProveedor).HasColumnName("id_proveedor").IsRequired();
```
```javascript
// Views/Compras/Alta.cshtml:160-164 — cambiar proveedor vacía todas las líneas
$('#IdProveedor').on('change', function () { productosContainer.empty(); ... });
```
```csharp
// Controllers/ComprasController.cs:260-272 — el buscador filtra por nombre de proveedor
var nombreProveedor = _proveedoresRepo.ObtenerListadoProveedores()
    .FirstOrDefault(pr => pr.IdProveedor == idProveedor)?.Proveedor;
if (!string.IsNullOrEmpty(nombreProveedor))
    filtrados = todos.Where(p => p.Proveedor == nombreProveedor);
```

**Qué significa:** cada producto tiene un proveedor obligatorio y una compra es siempre a un solo proveedor. No se puede comprar el mismo producto a dos proveedores distintos ni comparar precios entre ellos. Detalle de implementación frágil: el filtrado se hace **comparando el nombre del proveedor como texto**, no por id; dos proveedores con el mismo nombre se mezclarían.

---

#### Regla 25 — Duplicados prohibidos en el mismo comprobante

```csharp
// ViewModels/Ventas/AltaVentaViewModel.cs:59-76
"No se puede agregar el mismo producto más de una vez a la venta."
"No se puede agregar la misma promoción más de una vez a la venta."
```
Lo mismo en compras (`ViewModels/Compras/AltaCompraViewModel.cs:42-49`), promociones (`AltaPromocionViewModel.cs:39-45`), consumo y producción (`Controllers/ConsumoController.cs:186-192`).

**Razón técnica de fondo:** la clave primaria de los detalles es **compuesta por (comprobante, producto)** (`AppDbContext.cs:82, 116, 145, 162`), así que la base directamente no admite dos líneas del mismo producto. **Consecuencia para el negocio: no se puede vender el mismo producto en dos líneas con precios distintos** (por ejemplo, una parte al precio normal y otra con descuento).

---

#### Regla 26 — Una venta debe tener al menos un ítem

```csharp
// ViewModels/Ventas/AltaVentaViewModel.cs:53-58
if (DetallesVenta.Count == 0 && VentaPromociones.Count == 0)
    yield return new ValidationResult("La venta debe contener al menos un producto o una promoción.");
```

---

#### Regla 27 — Topes monetarios y de cantidad codificados

| Concepto | Tope | Dónde |
|---|---|---|
| Precio y costo de un producto | **$99.999,99** | `numeric(7,2)` (`AppDbContext.cs:34-35`) + `[Range(0.01, 99999.99)]` |
| Precio de una promoción | **$99.999,99** | `numeric(7,2)` (`AppDbContext.cs:107`) |
| Precio/costo de una línea de venta | $999.999,99 | `numeric(8,2)` (`AppDbContext.cs:149-150`) |
| Redondeo de una venta | ±$999.999,99 | `[Range(-999999.99, 999999.99)]` |
| Deuda a un proveedor | $9.999.999,99 | `numeric(9,2)` (`AppDbContext.cs:48`) |
| Monto de un gasto | $99.999.999,99 | `numeric(10,2)` (`AppDbContext.cs:199`) |
| Stock de un producto | 9.999,999 | `numeric(7,3)` |
| Cantidad en una línea de venta | 999,999 | `numeric(6,3)` + `[Range(0.001, 999.999)]` |

**Qué significa:** el techo de **$99.999,99 por producto** es el más apretado y es una decisión que envejece: con inflación argentina, un producto que hoy vale $30.000 llega a ese techo en pocos años, y cuando lo alcance el sistema rechazará el alta sin explicar por qué el número no entra.

---

#### Regla 28 — Cantidades y costos positivos

```sql
-- db/polleria.sql:364-381
detalle_compra: CHECK (cantidad >= 0.001), CHECK (costo_unitario >= 0.01)
detalle_venta:  CHECK (cantidad >= 0.001), CHECK (costo_unitario >= 0.01), CHECK (precio_unitario >= 0.01)
producto:       CHECK (costo_producto >= 0.01), CHECK (precio_producto >= 0.01)
promocion:      CHECK (precio_promocion >= 0.01)
venta_promocion: CHECK (costo_promo >= 0.01), CHECK (precio_promo >= 0.01)
compra:         CHECK (total >= 0.01)
```

**No se puede registrar nada gratis ni con cantidad cero.** Esto entra en conflicto directo con el consumo interno, que se guarda con `precio_unitario = 0` (regla 11) — ver la contradicción explicada en la regla 7.

---

#### Reglas que el rubro tiene y este sistema NO modela

Se listan acá porque su ausencia es, en sí misma, información de dominio:

- **Merma y descarte.** No existe. No hay ninguna forma de registrar "se me echó a perder", "se cayó al piso", "lo tiré". Lo más cercano es el consumo interno, que contablemente significa otra cosa. **No hay ninguna aparición de las palabras "merma", "pérdida" ni "descarte" en el repositorio.**
- **Rendimiento de despiece.** El módulo de "producción" registra la salida de materia prima pero **no el ingreso del producto elaborado**. Un pollo entero que se convierte en pata, pechuga y alas sale del stock y no vuelve a entrar bajo ninguna forma. No hay factor de conversión, no hay rendimiento, no hay merma de despiece. La transformación pollo → presas —el proceso central de una pollería— **no está modelada**.
- **Vencimientos, lotes y trazabilidad.** No hay fecha de vencimiento, número de lote, ni relación entre una venta y la compra de la que salió esa mercadería. El stock es un único número por producto, sin capas ni antigüedad. `CostoPendiente`/`StockUmbral` (A1, funcionalidad esbozada) es el único vestigio de un intento de costear por lote.
- **Caja, turnos y arqueo.** No existe una entidad "caja". No hay apertura, cierre, fondo fijo, retiro de efectivo ni arqueo. El "turno" es sólo un filtro de horas sobre las ventas (regla 18).
- **Pesaje.** No hay integración con balanza ni lectura de códigos de barras con peso embebido. La cantidad se tipea a mano.

---

### A4. Reportes y salidas

El sistema **no genera un solo archivo**. No hay PDF, no hay Excel, no hay CSV, no hay impresión, no hay envío de mail. Verificado por búsqueda: cero ocurrencias de `print`, `imprimir`, `pdf`, `excel`, `csv` o `export` en todo el código propio. Todas las salidas son pantallas HTML o texto al portapapeles.

| # | Salida | Formato | Contenido | Cálculo |
|---|---|---|---|---|
| 1 | **Lista de precios** | Texto plano al portapapeles vía `navigator.clipboard` | `*PRODUCTOS*` + una línea `Nombre \| $precio` por producto activo ordenado alfabéticamente; `*PROMOCIONES*` + ídem para promociones vigentes | `Controllers/ProductosController.cs:205-227`. Formato de moneda `N2` con cultura `es-AR`. Los asteriscos son la sintaxis de negrita de WhatsApp: **el destino de este reporte es un mensaje de WhatsApp a los clientes.** Es la única salida "externa" del sistema. |
| 2 | **Listado de ventas** | Tabla HTML + 4 chips | N°, productos/promos, método, total, fecha y hora, detalle. Chips: cantidad, costo, venta, ganancia | Chips calculados **en el navegador** re-parseando el texto de la columna Total y el atributo `data-costo` de cada fila (`Views/Ventas/Index.cshtml:116-147`). Excluye las operaciones con método "Yo". |
| 3 | **Listado de compras** | Tabla HTML + 2 chips | N°, productos, proveedor, fecha, total, estado, detalle. Chips: cantidad y total | Total por compra = `Σ(cantidad × costo_unitario)` calculado en SQL (`Repositorios/ComprasRepository.cs:33`); el chip suma en el navegador re-parseando el texto |
| 4 | **Listado de productos** | Tabla HTML | Producto, proveedor, stock, costo, precio, ganancia, % ganancia, venta semanal | Ganancia y % en SQL (`ProductosRepository.cs:28-29`); venta semanal según regla 19 |
| 5 | **Listado de promociones** | Tabla HTML | Promoción, productos (con cantidades), stock del combo, costo, precio, ganancia, % ganancia, vigencia, estado | Reglas 5 y 6 |
| 6 | **Listado de proveedores** | Tabla HTML | Proveedor, contacto, deuda | Deuda es la columna `debo`, mantenida por triggers (regla 8). Formato de moneda con `es-AR` explícito |
| 7 | **Estadísticas de producto** | Tabla HTML + total | Fecha, hora, cantidad, origen (Directa / nombre de la promoción) + total del período | `Repositorios/ProductosRepository.cs:145-210`. Une ventas directas con ventas dentro de promociones (`cantidad_promo × cantidad_receta`) |
| 8 | **Listado de consumo** | Tabla HTML + 2 chips | Fecha, hora, productos con cantidad, costo, detalle. Chips: registros y costo total | `Σ(costo_unitario × cantidad)` por registro |
| 9 | **Listado de producción** | Tabla HTML + 2 chips | Idéntico a consumo | Ídem |
| 10 | **Listado de gastos** | Tabla HTML + 2 chips | Fecha, concepto, monto, observación. Chips: cantidad y total | Suma directa |
| 11 | **Dashboard — KPIs** | 7 chips | Venta total, costo de ventas, consumo, producción, gastos, costo total, ganancia neta con margen % | Regla 21 |
| 12 | **Dashboard — gráfico de evolución** | Gráfico de líneas (Chart.js desde CDN) | Serie de venta y ganancia agrupada por día/semana/mes | Reglas 21 y 22 |
| 13 | **Dashboard — distribución del costo** | Gráfico de dona | Costo de ventas / consumo interno / gastos extras (se omiten los que dan 0) | `Repositorios/DashboardRepository.cs:172-177` |
| 14 | **Dashboard — Top 5 productos** | Tabla HTML | Producto, cantidad, venta imputada | Regla 20 |
| 15 | **Dashboard — gastos del período** | Tabla HTML (máx. 8 filas + "y N más") | Fecha, concepto, monto | `Repositorios/DashboardRepository.cs:54-60` |

**Lo que falta como salida y el rubro necesita:** ticket para el cliente, comprobante de venta impreso, cierre de caja del día en papel, listado de faltantes para el proveedor, y cualquier exportación que permita darle los números al contador.

---

### A5. Funcionalidad ausente que el rubro esperaría

Observaciones, no críticas. Se listan por lo que el propio modelo de datos sugiere que haría falta.

**Directamente ligado a lo que ya está modelado:**

1. **Despiece / transformación de productos.** Existe el módulo "producción" que saca materia prima del stock pero nunca ingresa el producto terminado. Falta la contraparte: "de 1 pollo entero salieron 2 patas, 2 pechugas, 2 alas y 0,3 kg de merma".
2. **Merma y descarte.** No hay forma de dar de baja mercadería perdida. Hoy el único camino es editar el stock a mano en la pantalla de producto (que, por el bug de la funcionalidad 12, sí se puede hacer), lo que deja el ajuste sin registro ni motivo.
3. **Cierre de caja / arqueo.** Existe el filtro por turno pero no la operación de cierre: contar el efectivo, compararlo con lo que dice el sistema y registrar la diferencia.
4. **Ticket / comprobante para el cliente.** No hay ninguna salida imprimible.
5. **Modificar un gasto.** Hay alta y baja, falta la edición.
6. **Registro de pagos parciales a proveedores.** La deuda sólo se cancela marcando compras completas como pagadas; no se puede registrar "le di $50.000 a cuenta".
7. **Ajuste manual de stock con motivo.** Relacionado con el punto 2: un movimiento de inventario tipeado como tal, no una edición del número.
8. **Historial de cambios de precio.** El costo y el precio se pisan; no queda registro de cuándo ni por qué cambiaron, aunque el negocio los cambia seguido.
9. **Alerta de stock bajo.** `StockUmbral` existe como campo sin uso; no hay pantalla de "productos por reponer".
10. **Pedidos / encargues.** Una pollería toma pedidos por teléfono para retirar más tarde. No hay entidad "pedido" ni estado "pendiente de retiro".
11. **Envíos / delivery.** El commit `b0716f5` dice *"por hacer filtros globales e implementar envio"*: estaba en el plan y no se hizo.
12. **Clientes.** No existen. Curiosamente, **sí existían en el proyecto del que este deriva** (`Models/Clientes.cs` en la rama TP8) y se eliminaron. Sin clientes no hay cuenta corriente de clientes, ni fiado, que es habitual en el rubro.

**De infraestructura de producto:**

13. **Login y usuarios.** No hay ninguno (ver B10). Sin usuarios no hay forma de saber quién hizo cada venta ni quién borró qué.
14. **Auditoría de cambios.** Nada registra quién modificó o eliminó una venta.
15. **Copia de seguridad.** No hay ninguna función de backup ni recordatorio.
16. **Facturación fiscal.** Ver B9: no hay absolutamente nada.
17. **Múltiples sucursales o cajas.** Ver B4: el sistema asume una máquina, un negocio, una base.

---

## PARTE B — Cómo está construido

### B1. Inventario técnico

#### Stack

| Componente | Versión exacta | Fuente | Estado |
|---|---|---|---|
| .NET / TFM | `net9.0` | `entornoPolleria.csproj:4` | Vigente |
| SDK usado en el último build registrado | **9.0.100-rc.1.24452.12** | `build_log.txt` | ⚠️ **Release candidate**, no versión final |
| Runtime al que apunta el binario compilado | `Microsoft.NETCore.App 9.0.0-rc.1.24431.7` / `Microsoft.AspNetCore.App 9.0.0-rc.1.24452.1` | `bin/Debug/net9.0/entornoPolleria.runtimeconfig.json` | ⚠️ Preview. Con *roll-forward* usará el 9.0.x instalado, pero el artefacto versionado apunta a un RC |
| ASP.NET Core MVC | Incluido en el framework | `Program.cs:35` | — |
| Entity Framework Core | **9.0.7** | `bin/Debug/net9.0/entornoPolleria.deps.json` | Vigente |
| Npgsql.EntityFrameworkCore.PostgreSQL | **9.0.4** | `entornoPolleria.csproj:12` | Vigente |
| Npgsql (driver) | **9.0.3** | `deps.json` | Vigente |
| Microsoft.EntityFrameworkCore.Design | **9.0.7** (`PrivateAssets=all`) | `entornoPolleria.csproj:13-16` | Herramienta de diseño; arrastra Roslyn 4.8 y MSBuild.Locator al `bin` |
| Newtonsoft.Json | **13.0.3** | `entornoPolleria.csproj:11` | Vigente pero **innecesario**: se usa sólo en `Helpers/TempDataExtensions.cs` para serializar TempData; `System.Text.Json` ya viene en el framework |
| PostgreSQL | **17.5** | Cabecera de `db/polleria.sql` | Vigente |
| Bootstrap | **5.1.0** (local, `wwwroot/lib`) | `wwwroot/lib/bootstrap/dist/css/bootstrap.css:3` | ⚠️ De julio 2021. La rama 5.x va por 5.3.x |
| jQuery | **3.6.0** (local) | `wwwroot/lib/jquery/dist/jquery.js:2` | ⚠️ De marzo 2021 |
| jQuery Validation | **1.20.0** (local) | `wwwroot/lib/jquery-validation/dist/jquery.validate.js:2` | ⚠️ De 2021 |
| jquery-validation-unobtrusive | **4.0.0** (local) | — | — |
| Select2 | **4.1.0-rc.0** — **desde CDN jsdelivr** | `Views/Shared/_Layout.cshtml:11,142` | ⚠️ **Release candidate de 2020, proyecto prácticamente sin mantenimiento**. Y se descarga de internet en cada carga |
| Bootstrap Icons | **1.11.3** — **desde CDN jsdelivr** | `Views/Shared/_Layout.cshtml:12` | Requiere internet |
| Chart.js | **4.4.0** — **desde CDN jsdelivr** | `Views/Dashboard/Index.cshtml:180` | Requiere internet |
| Google Fonts "Inter" | Sin versión — **desde fonts.googleapis.com** | `wwwroot/css/site.css:7` (`@import`) | Requiere internet |

**Hallazgo de dependencias:** cuatro recursos de la interfaz se descargan de internet en tiempo de ejecución (Select2 CSS y JS, Bootstrap Icons, Chart.js, la tipografía). **Un local con internet caído pierde todos los buscadores con autocompletado** — que son la forma principal de cargar una venta — **y el dashboard entero.** Para un sistema de punto de venta que corre en localhost, esto es una dependencia externa que no debería existir.

**Paquetes desactualizados o cuestionables:**
- Select2 4.1.0-rc.0: RC congelado hace años, con jQuery como dependencia dura. Es el componente más usado de toda la interfaz.
- Bootstrap 5.1.0 y jQuery 3.6.0: ~5 años de atraso, sin vulnerabilidades conocidas graves pero sin soporte.
- Newtonsoft.Json: dependencia evitable.
- `EntityFrameworkCore.Design` referenciado en un proyecto sin migraciones: mete ~15 MB de Roslyn y MSBuild en el `bin` sin usarlos.

#### Estructura de proyectos

**Un solo proyecto** (`entornoPolleria.csproj`, SDK `Microsoft.NET.Sdk.Web`), sin bibliotecas de clases, sin proyecto de tests. La solución (`entornoPolleria.sln`) contiene ese único proyecto, con un GUID a mano evidentemente falso: `{1F6EA2CD-8B8A-4B8A-8B8A-4B8A8B8A4B8A}`.

#### Conteo de archivos y líneas por capa

| Capa | Archivos | Líneas |
|---|---:|---:|
| `Views/` (Razor + CSS de layout) | 56 | 4.953 |
| `Controllers/` | 11 | 2.185 |
| `Repositorios/` (10 clases + 9 interfaces) | 19 | 1.289 |
| `ViewModels/` | 30 | 1.257 |
| `Models/` (entidades + helpers) | 13 | 621 |
| Raíz (`Program.cs`, `AppDbContext.cs`) | 2 | 292 |
| `Helpers/` | 1 | 22 |
| `wwwroot/` propio (`site.css`, `site.js`) | 2 | 692 |
| `db/` (SQL versionado) | 2 | 554 |
| **Total de código propio** | **136** | **≈ 11.865** |

Además: 468 archivos en el índice de git, de los cuales **más de 300 son binarios y basura de compilación** (`bin/`, `obj/`, `.vs/`, `wwwroot/lib/`) porque **no existe `.gitignore`**.

**Distribución del código C# puro (sin vistas):** 5.666 líneas. De ellas, ~2.185 (39%) están en controladores.

---

### B2. Modelo de datos

#### Entidades del código (11 `DbSet`, `AppDbContext.cs:9-19`)

**`Productos` → tabla `producto`** (`Models/Productos.cs`, `AppDbContext.cs:26-39`)

| Propiedad | Columna | Tipo BD | Nulo |
|---|---|---|---|
| `IdProducto` (int) | `id_producto` | integer identity | no |
| `IdProveedor` (int) | `id_proveedor` | integer | no |
| `Producto` (string) | `producto` | varchar(75) | no |
| `Stock` (decimal) | `stock` | numeric(7,3) | no |
| `Costo` (decimal) | `costo_producto` | numeric(7,2) | no |
| `Precio` (decimal) | `precio_producto` | numeric(7,2) | no |
| `Activo` (bool) | `activo` | boolean, default true | no |
| `CostoPendiente` (decimal?) | `costo_pendiente` | numeric(7,2) | sí — **sin uso** |
| `StockUmbral` (decimal?) | `stock_umbral` | numeric(7,3) | sí — **sin uso** |
| `Proveedor` (nav) | — | — | — |

**`Proveedores` → `proveedor`**: `IdProveedor` int identity, `Proveedor` varchar(75) NOT NULL, `Contacto` varchar(100) nullable (texto libre: nombre y teléfono mezclados), `Debo` numeric(9,2) NOT NULL. Navegación 1:N a `Productos`.

**`MetodosPago` → `metodo_pago`**: `IdMetodo` **short** (smallint) identity, `Metodo` varchar(50). Navegación 1:N a `Ventas`.

**`Ventas` → `venta`**: `IdVenta` **long** (bigint) identity, `IdMetodo` short FK, `Fecha` `DateOnly` → `date`, `Hora` `TimeOnly` → `time without time zone`, `Detalle` varchar(100) nullable, `Redondeo` numeric(8,2) NOT NULL, `Tipo` varchar(20) NOT NULL default `'venta'`. Colecciones: `DetallesVenta`, `VentaPromociones` (expuestas como `IReadOnlyCollection` sobre campos públicos `_detallesVenta` / `_ventaPromociones`).

**`DetallesVentas` → `detalle_venta`**: PK compuesta `(IdVenta, IdProducto)`, `Cantidad` numeric(6,3), `CostoUnitario` numeric(8,2), `PrecioUnitario` numeric(8,2). Cascada al borrar la venta.

**`VentasPromociones` → `venta_promocion`**: PK compuesta `(IdVenta, IdPromocion)`, `Cantidad` **numeric(6,2)**, `CostoPromo` numeric(7,2), `PrecioPromo` numeric(7,2). Cascada.

**`Promociones` → `promocion`**: `IdPromocion` int identity, `Promocion` varchar(75), `Precio` numeric(7,2), `Inicio` date NOT NULL, `Fin` date nullable.

**`DetallesPromociones` → `detalle_promocion`**: PK compuesta `(IdProducto, IdPromocion)`, `Cantidad` numeric(6,3). Cascada al borrar la promoción.

**`Compras` → `compra`**: `IdCompra` int **identity always**, `IdProveedor` int FK, `Fecha` date, `Detalle` varchar(100) nullable, `Pagada` boolean default false NOT NULL. **No tiene columna de total** — se calcula sumando detalles.

**`DetallesCompras` → `detalle_compra`**: PK compuesta `(IdCompra, IdProducto)`, `Cantidad` numeric(7,3), `CostoUnitario` numeric(8,2). Cascada configurada en EF.

**`Gasto` → `gasto`**: `IdGasto` int **identity always**, `Fecha` date, `Nombre` varchar(100), `Monto` numeric(10,2), `Observacion` varchar(255) nullable. **Entidad aislada: sin relaciones con nada.**

#### Diagrama de relaciones

```
proveedor 1 ──< N producto
proveedor 1 ──< N compra
compra    1 ──< N detalle_compra >── 1 producto
venta     1 ──< N detalle_venta   >── 1 producto        (venta con Tipo = venta|consumo|produccion)
venta     1 ──< N venta_promocion >── 1 promocion
promocion 1 ──< N detalle_promocion >── 1 producto
metodo_pago 1 ──< N venta
gasto     (aislada)
```

#### ⚠️ El esquema versionado y el modelo del código **divergen gravemente**

`db/polleria.sql` es el único archivo de esquema del repositorio. Dos problemas de fondo:

**(a) No es un `.sql`.** A pesar de la extensión, es un **archivo de volcado en formato *custom* de `pg_dump`** (`PostgreSQL custom database dump - v1.16-0`, magic `PGDMP` en los primeros bytes). **No se puede aplicar con `psql -f`**; requiere `pg_restore`. Además el `CREATE DATABASE` fija `LOCALE = 'Spanish_Spain.1252'` (`db/polleria.sql:8`), un locale de Windows: **restaurar ese dump en un servidor Linux falla.**

**(b) Está desactualizado.** Es anterior a los cambios de 2026. Divergencias verificadas:

| Objeto | En `db/polleria.sql` | En el modelo EF actual | Impacto |
|---|---|---|---|
| `compra.total` | `numeric(9,2) NOT NULL` con `CHECK (total >= 0.01)` | **No existe** | El código nunca escribe `total`; si la columna sigue en la base con NOT NULL, **ningún alta de compra funcionaría**. Debe haber sido eliminada en producción sin dejar rastro en el repo |
| `compra.pagada` | **No existe** | `boolean NOT NULL default false` | Columna agregada fuera del repo |
| `producto.costo_pendiente` | No existe | `numeric(7,2)` nullable | Ídem |
| `producto.stock_umbral` | No existe | `numeric(7,3)` nullable | Ídem |
| `venta.redondeo` | **No existe** | `numeric(8,2) NOT NULL` | Ídem — y es un campo central |
| `venta.tipo` | **No existe** | `varchar(20) NOT NULL default 'venta'` | Ídem — sin esta columna no existen consumo ni producción |
| Tabla `gasto` | **No existe** | Entidad completa | Módulo entero fuera del esquema versionado |
| `venta_promocion.cantidad` | `smallint` | `numeric(6,2)` | Cambio de tipo: el commit `96fedd6` menciona *"promo con decimales"* |
| `detalle_venta.costo_unitario` / `precio_unitario` | `numeric(7,2)`, `precio_unitario` **nullable** | `numeric(8,2)`, ambos `IsRequired` | Techo distinto |
| `detalle_compra.costo_unitario` | `numeric(7,2)` | `numeric(8,2)` | Techo distinto |
| Trigger `tg_actualizar_deuda_proveedor` | **No existe en ningún archivo** | Referenciado como existente en `db/2026-05-10-compra-pagada-trigger.sql:9` | **Regla de negocio perdida** |
| Trigger `tg_compra_pagada_cambio` | No | En `db/2026-05-10-compra-pagada-trigger.sql` | Único trigger versionado al día |

**No hay migraciones.** No existe carpeta `Migrations/` ni ninguna clase `DbContext` con `EnsureCreated`/`Migrate`. **El esquema de la base se administra a mano, y el repositorio no contiene una definición correcta ni completa de ese esquema.**

#### Índices

Sólo los que PostgreSQL crea automáticamente por las claves primarias (7 índices únicos) y ninguno más: no hay un solo `CREATE INDEX` en todo el repositorio. En particular **no hay índice sobre `venta.fecha`, `venta.tipo`, `compra.fecha` ni `gasto.fecha`**, que son las columnas por las que filtran todas las pantallas. Tampoco sobre las FK (`detalle_venta.id_producto`, `producto.id_proveedor`, etc.), lo que hace lentos los `Any()` de "¿se puede eliminar?".

---

### B3. Tipo de claves primarias — entidad por entidad

| Entidad | Tabla | Clave primaria | Tipo | Generación |
|---|---|---|---|---|
| `Productos` | `producto` | `IdProducto` | **`int`** | Autoincremental — `UseIdentityColumn()` (`AppDbContext.cs:30`); en el dump, `DEFAULT nextval('secuencia_productos')` |
| `Proveedores` | `proveedor` | `IdProveedor` | **`int`** | Autoincremental — `UseIdentityColumn()` (`:45`) |
| `MetodosPago` | `metodo_pago` | `IdMetodo` | **`short`** (smallint) | Autoincremental — `UseIdentityColumn()` (`:180`) |
| `Ventas` | `venta` | `IdVenta` | **`long`** (bigint) | Autoincremental — `UseIdentityColumn()` (`:132`) |
| `Compras` | `compra` | `IdCompra` | **`int`** | Autoincremental — `UseIdentityAlwaysColumn()` (`:60-62`), reforzado con `[DatabaseGenerated(DatabaseGeneratedOption.Identity)]` (`Models/Compras.cs:8-10`) |
| `Promociones` | `promocion` | `IdPromocion` | **`int`** | Autoincremental — `UseIdentityColumn()` (`:105`) |
| `Gasto` | `gasto` | `IdGasto` | **`int`** | Autoincremental — `UseIdentityAlwaysColumn()` (`:196`) |
| `DetallesVentas` | `detalle_venta` | **Compuesta** `(IdVenta, IdProducto)` | `(long, int)` | Heredada de los padres (`:145`) |
| `DetallesCompras` | `detalle_compra` | **Compuesta** `(IdCompra, IdProducto)` | `(int, int)` | Heredada (`:82`) |
| `DetallesPromociones` | `detalle_promocion` | **Compuesta** `(IdProducto, IdPromocion)` | `(int, int)` | Heredada (`:116`) |
| `VentasPromociones` | `venta_promocion` | **Compuesta** `(IdVenta, IdPromocion)` | `(long, int)` | Heredada (`:162`) |

**Resumen: no hay un solo GUID ni una sola clave natural en todo el sistema. Todas las claves raíz son enteros autoincrementales generados por secuencias locales de una base de datos.**

**Consecuencia directa para el objetivo comercial:** los datos de dos instalaciones **no pueden convivir**. La pollería A y la pollería B tendrán ambas un `producto` con `id_producto = 1`, una `venta` con `id_venta = 1`, un `proveedor` con `id_proveedor = 1`. Cualquier escenario que implique juntar datos —sincronización a la nube, base multi-inquilino, consolidación de sucursales, respaldo centralizado, o simplemente importar los datos de un cliente para depurar un problema— **exige reasignar identificadores en todas las tablas y reescribir todas las claves foráneas**, incluidas las cuatro claves primarias compuestas. No existe ninguna columna que identifique al comercio dueño del registro (ver B4).

Además hay un detalle de escala: `metodo_pago.id_metodo` es `smallint` (máximo 32.767, irrelevante) pero `venta.id_venta` es `bigint` mientras `compra.id_compra` es `int` — inconsistencia sin consecuencia práctica a corto plazo.

Y `UseIdentityAlwaysColumn()` en `compra` y `gasto` (`GENERATED ALWAYS AS IDENTITY`) **impide insertar un id explícito** sin `OVERRIDING SYSTEM VALUE`: cualquier migración o importación de datos con ids preexistentes fallará en esas dos tablas. De hecho el propio código tuvo que trabajar alrededor de esto: `compra.IdCompra = 0; // Forzar Identity Generation` (`Repositorios/ComprasRepository.cs:57`).

---

### B4. Supuestos de usuario único / comercio único

Este es el eje donde el sistema es más rígido. Ordenado por gravedad.

#### 1. No existe el concepto de "comercio" en ninguna parte

**Ninguna de las 11 tablas tiene una columna de propietario, sucursal, inquilino o empresa.** Verificado tabla por tabla en `AppDbContext.cs:26-201`. Tampoco hay filtros globales de consulta (`HasQueryFilter`), ni un `ITenantProvider`, ni nada equivalente. Toda consulta del sistema, sin excepción, lee la tabla entera.

#### 2. No existe el concepto de "usuario"

No hay entidad `Usuario`, ni sesión, ni identidad. Ninguna venta, compra, gasto o modificación registra quién la hizo. `Program.cs:73` llama a `app.UseAuthorization()` **sin haber registrado autenticación ni políticas**: es un no-op. Ver B10.

Dato relevante: **el proyecto del que este deriva sí tenía login y roles** (`Models/Usuarios.cs`, `Controllers/LoginController.cs`, `Repositorios/UsuariosRepository.cs` en la rama `TP8`, con `enum Rol { Admin, Cliente }`). Se eliminaron al reconvertir el proyecto (ver D3).

#### 3. Cadena de conexión fija a `localhost`, con credenciales fijas

```json
// appsettings.json:10-12
"ConnectionStrings": {
  "DefaultConnection": "Host=localhost;Port=5432;Database=polleria;Username=postgres;Password=1234;"
}
```
Duplicada en un segundo archivo que nadie lee:
```ini
// postgresql.conf:1-6
[PostgreSQL]
Host=localhost
Port=5432
Database=polleria
Username=postgres
Password=1234
```
Y una tercera copia versionada en `bin/Debug/net9.0/appsettings.json`.

El nombre de la base (`polleria`), el usuario (`postgres`, superusuario) y la contraseña están fijos. No hay variables de entorno, ni `User Secrets`, ni configuración por instalación.

#### 4. La aplicación se ata a una sola máquina y un solo puerto

```csharp
// Program.cs:78-89
Console.WriteLine("Iniciando sistema en http://localhost:5146...");
System.Diagnostics.Process.Start(new ProcessStartInfo("http://localhost:5146") { UseShellExecute = true });
...
app.Run("http://localhost:5146");
```

`app.Run("http://localhost:5146")` **sobreescribe cualquier configuración de URLs** y liga el servidor a **loopback**: ninguna otra máquina de la red local puede conectarse. Puerto fijo 5146, sin posibilidad de cambiarlo sin recompilar. Además el proceso **abre un navegador en el escritorio del servidor**, lo que confirma el modelo mental: la aplicación es un programa de escritorio disfrazado de web, para una PC, una caja, un usuario.

Consecuencias: no hay segunda caja, no hay una tablet en el mostrador, no hay acceso desde el celular del dueño, y el sistema no puede correr como servicio de Windows ni en un contenedor (el `Process.Start` fallaría o abriría un navegador que nadie ve — está en `try/catch`, así que no rompe).

#### 5. Claves de Data Protection en el perfil del usuario de Windows

```csharp
// Program.cs:24-25
builder.Services.AddDataProtection().SetApplicationName("PolleriaGestion");
```

No se configura persistencia de claves. En Windows, ASP.NET Core las guarda por defecto en **`%LOCALAPPDATA%\ASP.NET\DataProtection-Keys`**, es decir en la carpeta del usuario de sistema operativo que ejecuta el proceso. Esas claves cifran las cookies de TempData, que son el mecanismo de persistencia de filtros. **Si el sistema pasa a ejecutarse con otra cuenta de Windows —o como servicio— las cookies previas dejan de poder descifrarse.** También significa que el estado de la aplicación depende de un directorio derivado del usuario del sistema operativo.

#### 6. Filtros persistidos en cookies del navegador, con el conjunto de resultados adentro

`Helpers/TempDataExtensions.cs` serializa objetos completos a TempData. Como no se registra `AddSession`, el proveedor por defecto de ASP.NET Core es **`CookieTempDataProvider`**: todo va a una cookie.

```csharp
// Controllers/VentasController.cs:55-57
viewModel.Ventas = _ventaRepo.ObtenerListadoVentas(viewModel).ToList();
viewModel.MetodosPago = _metodosPagoRepo.ObtenerListadoMetodosPago().ToList();
TempData.Set("FiltrosVentas", viewModel);          // <-- serializa TODA la lista de ventas a una cookie
```
```csharp
// Controllers/ComprasController.cs:40-42
viewModel.Proveedores = _proveedoresRepo.ObtenerListadoProveedores().ToList();
viewModel.Compras = _comprasRepo.ObtenerListadoCompras(viewModel).ToList();
TempData.Set("FiltrosCompras", viewModel);         // <-- ídem con compras y proveedores
```

El `IndexVentasVM` incluye la propiedad `Ventas` (lista completa de resultados) y `MetodosPago`. Todo eso se serializa a JSON, se cifra, se codifica en base64 y se manda como cookie. **Los navegadores descartan silenciosamente las cookies de más de ~4 KB**: con más de un puñado de ventas en el rango de fechas, la persistencia de filtros deja de funcionar sin ningún aviso. El commit `710bb6d` (2026-05-10) se llama literalmente *"arregle lo de las cookies y cambios varios"*, lo que sugiere que este problema ya se manifestó.

También implica que **el estado de la interfaz vive en el navegador del cliente**, no en el servidor: dos pestañas abiertas se pisan los filtros mutuamente.

#### 7. Datos del comercio hardcodeados en la interfaz

```html
<!-- Views/Shared/_Layout.cshtml:6 -->
<title>@ViewData["Title"] — Sistema Pollería</title>
<!-- :21-24 -->
<div class="sidebar-brand-icon">🍗</div>
<div class="sidebar-brand-text">Pollería</div>
<div class="sidebar-brand-sub">Sistema de gestión</div>
```

No hay nombre del negocio, logo, dirección ni ningún dato configurable: el producto se llama "Pollería" y su logo es un emoji de pollo. **Vender esto a un segundo cliente implica que su sistema también diga "Pollería" y muestre 🍗.**

Además, los billetes de vuelto están fijos en pesos argentinos ($1.000 / $10.000 / $20.000, `Views/Ventas/Alta.cshtml:117-126`), los horarios de turno están fijos (regla 18), y las palabras "Yo" y "Efectivo" funcionan como configuración de negocio escrita en el código (reglas 12 y 17).

#### 8. Cultura fija en el arranque

```csharp
// Program.cs:11-20
var supportedCultures = new[] { new CultureInfo("en-US") };
options.DefaultRequestCulture = new RequestCulture("en-US");
```
El sistema fuerza `en-US` **para todo el proceso de las peticiones** y luego formatea a mano con `es-AR` en cada lugar que se acuerda. Ver B8.

#### 9. Sin caches estáticos ni singletons con estado

Punto a favor: todos los repositorios se registran como `Scoped` (`Program.cs:42-51`), no hay ningún `static` mutable, y los únicos `static` del sistema son `CG.CulturaES/CulturaUS` (`Models/Cultura.cs`) y los métodos de extensión de TempData, ambos inmutables y sin estado. **No hay estado compartido entre peticiones en memoria del servidor.** Esto facilita mucho cualquier futuro trabajo de multi-instancia.

---

### B5. Acoplamiento de la lógica de negocio

#### Dónde vive la lógica

La arquitectura nominal es Controller → Repository → DbContext, con ViewModels por caso de uso. **No existe una capa de servicios ni de dominio.** En la práctica la lógica está repartida en **cinco lugares distintos**, y la proporción es la parte incómoda:

| Ubicación | Qué hay ahí | Volumen aprox. |
|---|---|---|
| **Triggers de PostgreSQL** | Todo el movimiento de stock (venta, compra, promoción) y toda la cuenta corriente de proveedores | ~200 líneas de PL/pgSQL, **parcialmente no versionadas** |
| **Repositorios** | Consultas, proyecciones y todos los cálculos derivados (ganancias, %, stock de combos, venta semanal, dashboard) escritos como expresiones LINQ que se traducen a SQL | ~1.100 líneas |
| **Controladores** | Validaciones de negocio (precio vs costo de promociones, duplicados, detalles vacíos), orquestación, repoblado de formularios | ~2.185 líneas |
| **ViewModels** | Validaciones vía `IValidatableObject` y atributos, cálculos de costo de promoción | ~1.257 líneas |
| **JavaScript en las vistas** | Totales, subtotales, recargo, vueltos, ganancia y % de ganancia de promociones, exclusión de operaciones "Yo" del total del día, reindexado de los formularios dinámicos | ~1.200 líneas embebidas en `.cshtml` |
| **Entidades (`Models/`)** | Muy poco: `CalcularPrecioTotal`, `CalcularCostoTotal`, `CalcularTotal`, `CalcularCosto`. Buena parte de los métodos de dominio están muertos (ver A1) | ~200 líneas útiles |

**El JavaScript no es decoración: contiene reglas de negocio que no existen en ningún otro lado.** El cálculo del total con recargo (`Views/Ventas/Alta.cshtml:267`), el vuelto (`:278-283`) y la exclusión de las operaciones "Yo" del total del día (`Views/Ventas/Index.cshtml:129-140`) sólo existen como JavaScript dentro de archivos `.cshtml`. Si mañana se reescribe la interfaz, esas reglas se pierden salvo que alguien las lea de acá.

#### Regla por regla: ¿es código puro extraíble?

| # | Regla (de A3) | Dónde vive hoy | ¿Extraíble a una librería de clases? |
|---|---|---|---|
| 1 | Stock con 3 decimales, sin unidad | Mapeo EF + atributos de validación | **Sí**, trivial |
| 2 | Movimiento de stock por venta/compra | **Triggers PL/pgSQL** | **No**. Hay que reescribirlo en C#. La lógica es simple y está documentada, pero es una reescritura, no una extracción |
| 3 | Doble validación de stock | `VentaRepository.ValidarStockParaVenta` — acoplado al `DbContext` | **Parcial**: la regla es pura, la obtención de datos no. Extraíble con un puerto de lectura |
| 4 | Venta de combo descuenta ingredientes | **Trigger PL/pgSQL** | **No**. Reescritura |
| 5 | Stock de combo = MIN(stock/receta) | `PromocionesRepository` (LINQ traducido a SQL) + `VentasPromociones.CalcularStock` (C# puro) | **Sí** — de hecho ya existe una versión pura en `Models/Promociones.cs:71`, sin usar |
| 6 | Costo/ganancia/% de combo | Expresión LINQ dentro de la proyección del repositorio | **Sí**, la fórmula es trivial; hay que sacarla de la consulta |
| 7 | Precio ≥ costo | `IValidatableObject` en 2 ViewModels + controlador de promociones + `CHECK` en la base | **Sí**, pero está triplicada e inconsistente (`<=` vs `>=`) |
| 8 | Compra suma deuda al proveedor | **Trigger no versionado** + `tg_compra_pagada_cambio` | **No.** Hay que reconstruirla leyendo la base de producción |
| 9 | Deuda no negativa | `CHECK` + atributo `[Range]` | Sí |
| 10 | No borrar proveedor con deuda/productos | `ProveedoresController.Eliminar` — mezclada con `TempData` y `RedirectToAction` | **Parcial**: la condición es pura, la reacción es MVC |
| 11 | Tres tipos de movimiento (venta/consumo/producción) | `Models/Ventas.cs` + filtros repetidos en 4 repositorios | **Sí**, es el mejor modelado del sistema |
| 12 | Método de pago "Yo" | `ConsumoRepository`/`ProduccionRepository` (consulta) + **JavaScript de una vista** | **No tal cual.** La parte de exclusión del total está en JS |
| 13 | Redondeo manual en pesos | `Ventas.CalcularPrecioTotal()` — **C# puro** | **Sí**, ya está bien |
| 14 | Vigencia de promociones | 2 definiciones distintas en `PromocionesRepository` | **Sí**, pero hay que decidir cuál es la correcta |
| 15 | Promoción vendida no cambia receta | `PromocionesController.Modificar` (decisión) + `PromocionesRepository.PuedeSerEliminada` (consulta) | **Parcial** |
| 16 | Congelar precio/costo en la línea | Mapeo EF + constructores de entidades | **Sí** (y hay que arreglar la fuga documentada) |
| 17 | Efectivo vs virtuales | `VentaRepository` (expresión de filtro) | **Sí**, es un predicado |
| 18 | Turnos 08–14 / 17:30–22 | `VentasController` + `VentaRepository` (duplicado) | **Sí**, son constantes que deberían ser configuración |
| 19 | Venta semanal / orden del buscador | Subconsulta LINQ compleja en `ProductosRepository` | **Parcial**: es una consulta, no una regla; el criterio de orden sí es extraíble |
| 20 | Reparto proporcional del combo | `DashboardRepository.cs:143-155` — **C# puro sobre objetos ya materializados** | **Sí**, directamente |
| 21 | Ganancia neta del negocio | `DashboardVM` (propiedades calculadas) — **C# puro** | **Sí**, ya está bien |
| 22 | Agrupación temporal del gráfico | `DashboardRepository` sobre listas en memoria | **Sí** (arreglando el bug del domingo) |
| 23 | Períodos de análisis | `DashboardController.ResolverPeriodo` — método `static` puro | **Sí**, ya está aislado |
| 24 | Un producto, un proveedor | Modelo + filtro por **nombre** en el controlador | **Sí** |
| 25 | Sin duplicados en un comprobante | `IValidatableObject` en 4 ViewModels + PK compuesta | **Sí** |
| 26 | Venta con al menos un ítem | `IValidatableObject` | Sí |
| 27 | Topes monetarios | Tipos de columna + `[Range]` | Sí |
| 28 | Cantidades y costos positivos | `CHECK` de la base | Sí |

#### Veredicto numérico

Contando las 28 reglas de A3 con el mismo peso:

- **Extraíbles sin reescribir la lógica (sólo mover el código): 18 reglas — 64%.**
- **Parcialmente extraíbles** (la regla es pura pero está enredada con `DbContext`, `TempData` o `ModelState`; hay que separar consulta de decisión): **5 reglas — 18%.**
- **No extraíbles porque no están en C#** (viven en PL/pgSQL o en JavaScript de una vista, y hay que reescribirlas): **5 reglas — 18%.**

Ahora bien, ese 64% engaña si se mide sólo por conteo de reglas, porque las 5 no extraíbles son **las más importantes del sistema**: todo el movimiento de inventario y toda la cuenta corriente de proveedores. Ponderando por importancia funcional en lugar de por cantidad:

> **Aproximadamente el 60% del dominio es extraíble a una librería de clases sin reescribir la lógica. El 40% restante requiere reescritura: el 25% porque está en triggers de PostgreSQL (movimiento de stock y deuda de proveedores — la parte más crítica), el 10% porque está en JavaScript embebido en vistas (totales, recargo, vueltos, exclusión de "Yo"), y el 5% porque está enredado con la capa MVC.**

Y una salvedad que cambia el número: de esas reglas en triggers, **la de deuda de proveedores ni siquiera se puede leer del repositorio** (regla 8). Para ese 25% no alcanza con reescribir: primero hay que **recuperar la especificación desde la base de datos de producción**.

Aspectos que sí facilitan el trabajo: no hay estado estático, los repositorios están detrás de interfaces (9 de ellas), las entidades son POCOs limpios sin atributos de EF salvo `Compras`, y los ViewModels están razonablemente separados por caso de uso.

---

### B6. Dependencias del sistema de archivos

**El sistema no lee ni escribe un solo archivo fuera de la base de datos.** Verificado por búsqueda de `System.IO`, `File.`, `Path.`, `Directory.`, `Environment.` y `AppDomain` en todo el código propio: **cero ocurrencias** (la única coincidencia es `app.Environment.IsDevelopment()` en `Program.cs:57`, que no toca el disco).

Es el punto más fuerte del sistema de cara a un futuro en servidor: no hay rutas absolutas, no hay carpetas de usuario, no hay archivos adjuntos, no hay imágenes de productos, no hay logs a disco, no hay exportaciones.

Lo que sí depende del entorno de archivos, indirectamente:

| Elemento | Cómo se resuelve | Qué pasaría en un servidor remoto |
|---|---|---|
| `appsettings.json` con la cadena de conexión | Se lee del directorio del ejecutable | Habría que externalizarlo a variables de entorno o a un almacén de secretos. Hoy la contraseña está en texto plano y versionada |
| `wwwroot/` (CSS, JS, librerías) | Servido por `app.UseStaticFiles()` desde el content root | Funciona igual |
| Claves de Data Protection | **`%LOCALAPPDATA%\ASP.NET\DataProtection-Keys`** del usuario que ejecuta (por defecto, sin configurar) | ⚠️ Con más de una instancia, cada una generaría sus propias claves y las cookies de una no servirían en la otra. Hay que persistirlas a un almacén compartido |
| Apertura automática del navegador | `Process.Start("http://localhost:5146")` (`Program.cs:82`) | ⚠️ Sin escritorio no hay navegador. Está en `try/catch` y sólo escribe a consola, así que **no rompe el arranque** |
| Enlace a `~/entornoPolleria.styles.css` | Bundle de CSS *scoped* generado en compilación desde `Views/Shared/_Layout.cshtml.css` | Funciona igual |
| CDNs (Select2, Bootstrap Icons, Chart.js, Google Fonts) | Descarga desde internet en cada carga de página | Funcionaría en un servidor con salida a internet; **hoy es una dependencia de red incluso corriendo en localhost** |
| `db/*.sql` | Nunca los lee la aplicación; son artefactos para ejecutar a mano | — |

---

### B7. Concurrencia y transacciones

#### Motor y configuración

PostgreSQL 17.5 vía Npgsql 9.0.3 / EF Core 9.0.7. Configuración completa del contexto:

```csharp
// Program.cs:37-40
var conectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(options => options.UseNpgsql(conectionString));
```

No se configura: nivel de aislamiento, timeout de comandos, política de reintentos (`EnableRetryOnFailure`), pooling explícito, ni `UseQueryTrackingBehavior`. Se usan los valores por defecto: **aislamiento READ COMMITTED**, seguimiento de entidades activado, sin reintentos ante fallo transitorio de conexión. El `DbContext` es `Scoped` (uno por petición), que es lo correcto.

También queda activado el registro detallado de SQL:
```json
// appsettings.json:6, 13
"Microsoft.EntityFrameworkCore.Database": "Information",
"Npgsql.EnableVerboseLogging": true
```
Esto imprime cada sentencia SQL con sus parámetros a la consola en producción. (La clave `Npgsql.EnableVerboseLogging` fuera de la sección `Logging` no la lee nadie; la que sí tiene efecto es la de `LogLevel`.)

#### Dónde hay transacciones y dónde no

| Operación | ¿Transacción explícita? | Evidencia | Riesgo |
|---|---|---|---|
| Crear venta | **Sí** | `VentaRepository.cs:108-127` — `BeginTransaction`, valida stock, `SaveChanges`, `Commit`, `Rollback` en `catch` | Correcto |
| Modificar venta | **No** | `VentaRepository.cs:128-135` — sólo `Update` + `SaveChanges` | EF emite todo en una transacción implícita de `SaveChanges`, así que es atómico **por accidente**, no por diseño |
| Eliminar venta | **No** | `VentaRepository.cs:136-147` | Atómico por el `SaveChanges` único + cascada |
| Crear compra | **No** | `ComprasRepository.cs:53-66` | Atómico por `SaveChanges` único |
| Modificar compra | **Sí, pero mal ordenada** | `ComprasRepository.cs:70-122` | Ver abajo |
| **Eliminar compra** | **No — y son dos operaciones** | `ComprasRepository.cs:126-144`: `ExecuteSqlRaw("DELETE FROM detalle_compra...")` y después `Remove` + `SaveChanges` | ⚠️ **Si la segunda falla, quedan detalles borrados (con el stock ya descontado y la deuda ajustada) y la cabecera de compra huérfana en la base** |
| **Eliminar consumo** | **No — dos `SaveChanges` seguidos** | `ConsumoRepository.cs:96-108` | ⚠️ Si el segundo falla, el stock se devolvió pero el registro de consumo sigue existiendo, ahora vacío |
| **Eliminar producción** | **No — dos `SaveChanges` seguidos** | `ProduccionRepository.cs:96-107` | ⚠️ Ídem |
| Modificar consumo | **Sí** | `ConsumoRepository.cs:45-66` | Correcto |
| Modificar producción | **Sí** | `ProduccionRepository.cs:45-67` | Correcto |
| ABM de producto / proveedor / promoción / método / gasto | **No** (operación única) | — | Correcto: un solo `SaveChanges` |
| Desactivar producto → desactivar/borrar promociones | **No** | `ProductosController.cs:134-149` + `PromocionesRepository.DesactivarPorIdProducto` | ⚠️ Son **dos `SaveChanges` en dos repositorios distintos, sin transacción común**. Si el segundo falla, quedan promociones desactivadas de un producto que sigue activo |

**Problema de orden en `ComprasRepository.Actualizar`:**

```csharp
// Repositorios/ComprasRepository.cs:74-115
var compraOriginal = _context.Compras.AsTracking().Include(c => c.DetallesCompra)
                             .FirstOrDefault(c => c.IdCompra == idCompra);   // <-- lee ANTES
if (compraOriginal == null) throw ...;
using var transaction = _context.Database.BeginTransaction();                // <-- transacción DESPUÉS
```
La lectura del estado original queda **fuera** de la transacción: entre la lectura y el `BeginTransaction` otra operación puede haber cambiado los detalles, y la comparación `detallesIguales` decidiría mal. Con un solo usuario es inofensivo; con dos cajas, no.

Y el mecanismo de actualización es agresivo: si cambió cualquier cosa de cualquier línea, **borra todos los detalles e inserta todos de nuevo** con SQL directo (`:97-104`), lo que dispara los triggers de stock y de deuda tantas veces como líneas haya. Es correcto en resultado y caro en efectos secundarios.

#### Supuestos de "un solo escritor"

El sistema está construido para una caja. Los indicios:

1. **Servidor atado a loopback** (`Program.cs:89`): físicamente no puede haber un segundo cliente en otra máquina. Este es, hoy, el mecanismo real de exclusión mutua.
2. **Sin control de concurrencia optimista**: ninguna entidad tiene `xmin` como token de concurrencia (`IsRowVersion()` / `UseXminAsConcurrencyToken()`) — cero ocurrencias en `AppDbContext.cs`. Dos usuarios editando la misma venta: **gana el último en guardar, sin aviso**.
3. **Validación de stock TOCTOU**: `ValidarStockParaVenta` lee el stock y el trigger lo vuelve a verificar después. La ventana entre ambos existe, pero el trigger de venta usa `SELECT ... FOR UPDATE` (`db/polleria.sql:80, 111`), que **sí serializa correctamente** el descuento de stock. El trigger de compra **no usa `FOR UPDATE`**, así que dos altas de compra simultáneas del mismo producto pueden perder una suma de stock.
4. **`ConsumoRepository.ObtenerIdMetodoYo()` usa `.First()`** sin `OrderBy` determinista más allá de "Yo primero": si no hay ningún método de pago cargado, **lanza `InvalidOperationException`** ("Sequence contains no elements") y el registro de consumo falla con un error técnico.
5. **Estado de UI en cookies** (B4, punto 6): dos pestañas del mismo navegador comparten y se pisan los filtros.

#### Operaciones largas y atomicidad

Ninguna operación es larga en tiempo, pero varias son caras:

- `ProductosRepository.ObtenerListadoProductos()` ejecuta, **por cada producto**, tres subconsultas `Any()` para `EsEliminable` y una subconsulta con `Concat`+`SelectMany` para `VentaSemanal` (`:30-42`). Sin índices en las claves foráneas (B2), con 88 productos y ~180 ventas es instantáneo; con miles de ventas se degrada de forma cuadrática. **Y se ejecuta completa en cada pulsación de tecla del buscador de la caja** (`Controllers/VentasController.cs:222`, con `delay: 0` en Select2, `Views/Ventas/Alta.cshtml:157`).
- `DashboardRepository.ObtenerDashboard` trae a memoria **todas las ventas del período con todos sus detalles, productos, promociones y recetas** (`:22-30`, `.AsEnumerable()`), y después calcula en C#. Para un año de operación son decenas de miles de filas materializadas en una sola petición.
- El trigger `modificar_stock_por_venta_promocion` recorre la receta dos veces por promoción vendida y contiene un bloque de "deshacer manual" antes de lanzar la excepción (`db/polleria.sql:192-197`) que es innecesario —el `RAISE EXCEPTION` aborta la transacción y revierte todo igual— y que, si alguna vez se ejecutara fuera de una transacción, dejaría el stock mal.

---

### B8. Dinero, pesos y fechas

#### Tipos usados — el lado del servidor está bien

**No hay un solo `double` ni `float` en cálculos de dinero o de peso en todo el código C#.** Verificado por búsqueda en todos los `.cs`: la única aparición de `double` es en un atributo de validación, no en un cálculo:

```csharp
// ViewModels/Gastos/GastoViewModel.cs:14
[Range(0.01, double.MaxValue, ErrorMessage = "El monto debe ser mayor a 0")]
public decimal Monto { get; set; }
```
Es la sobrecarga estándar de `RangeAttribute` con literales `double`; el valor validado y almacenado es `decimal`. **No es un hallazgo crítico.** Todos los importes, costos, precios, cantidades y stocks son `decimal` en C# y `numeric` en PostgreSQL.

#### ⚠️ Pero el número que ve el cliente se calcula en punto flotante y nunca se guarda

Toda la aritmética que ocurre en pantalla —la que el cajero lee y le cobra al cliente— se hace en JavaScript con `parseFloat`, que es IEEE 754 de doble precisión. **34 usos de `parseFloat` en vistas y JS.** Los puntos donde eso toca dinero:

```javascript
// Views/Ventas/Alta.cshtml:250-267
const precio = parseFloat(row.find('.precio-unitario-hidden').val()) || 0;
const cantidad = parseFloat(row.find('.cantidad-producto').val()) || 0;
const subtotalProducto = (precio * cantidad);
...
const redondeo = parseFloat($('#Redondeo').val()) || 0;
const recargo = parseFloat($('#Recargo').val() || 0);
const total = (subtotal+redondeo)*(1+recargo/100);
```
```javascript
// Views/Ventas/Index.cshtml:126  — el total del día se recalcula parseando TEXTO ya formateado
const valorVenta = parseFloat(textoTotal.replace('$', '').replace(/\./g, '').replace(',', '.')) || 0;
```
```javascript
// Views/Compras/Index.cshtml:89  — ídem para compras
totalGeneral += parseFloat(textoTotal.replace('$', '').replace(/\./g, '').replace(',', '.')) || 0;
```

Consecuencias reales, en orden de gravedad:

1. **El total que ve el cajero nunca se envía al servidor.** El servidor recalcula con `decimal` a partir de las líneas. Si difieren, nadie se entera. Y **difieren siempre que haya recargo**, porque el recargo no se persiste (A3, funcionalidad 1).
2. **Los indicadores del día se calculan re-parseando texto ya formateado.** El chip "Venta" del listado de ventas toma la columna Total renderizada como `"$12.345,67"`, le saca el `$`, borra los puntos y cambia la coma por punto. Cualquier cambio de formato de moneda rompe la suma en silencio. Y sumar 200 flotantes acumula error.
3. **El % de ganancia de promociones** se calcula en JS con flotantes (`Views/Promociones/Alta.cshtml:148-150`) y en SQL con `numeric` (`PromocionesRepository.cs:33`): pueden mostrar decimales distintos en dos pantallas para el mismo combo.

#### Dónde se redondea

**Prácticamente en ningún lado, de forma explícita.** El único `Math.Round` del sistema:

```csharp
// ViewModels/Dashboard/DashboardViewModel.cs:46-47
public decimal MargenPct => TotalVenta > 0 ? Math.Round(Ganancia / TotalVenta * 100, 1) : 0;
public decimal CostoPct  => TotalVenta > 0 ? Math.Round(CostoTotal / TotalVenta * 100, 1) : 0;
```

Todo el resto del redondeo monetario lo hace **PostgreSQL al guardar**, por la precisión de la columna (`numeric(8,2)` redondea a 2 decimales al insertar, con redondeo *half-up*). No hay una política de redondeo del sistema; hay una consecuencia de los tipos de columna. El único redondeo "de negocio" es el campo manual `Redondeo` (A3 regla 13).

**Riesgo concreto:** una línea de venta con cantidad `0,333` y precio `1000,00` da un subtotal de `333,00` en el servidor; el navegador muestra `$333,00` también. Pero con precios y cantidades con más decimales, la diferencia entre lo que suma el navegador (flotante, sin redondear por línea) y lo que suma el servidor (`decimal`, sin redondear por línea) puede aparecer en el último centavo. En un negocio que cobra en efectivo y usa el campo Redondeo, es tolerable; en uno que factura, no.

#### Fechas y zonas horarias

```csharp
// Models/Ventas.cs:8-9
public DateOnly Fecha { get; set; }
public TimeOnly Hora { get; set; }
```
Mapeadas a `date` y `time without time zone` (`db/polleria.sql:305-311`).

- **No hay zonas horarias en ninguna parte.** No hay un solo `DateTimeOffset`, ni `TimeZoneInfo`, ni `DateTime.UtcNow` (verificado: **cero ocurrencias de `UtcNow`**). Todo es hora local del servidor.
- La hora "ahora" se toma con `DateTime.Now` / `DateTime.Today` en **48 lugares** distintos, repartidos entre controladores, repositorios, entidades y ViewModels. `DateTime.Now` es la hora del reloj de la máquina que corre la aplicación.
- Fecha y hora se guardan **separadas en dos columnas**, lo que obliga a filtrar con dos condiciones (`Repositorios/VentaRepository.cs:37`) y hace imposible expresar correctamente un rango que cruce la medianoche.
- Para un sistema de una sola caja en una sola ciudad, la ausencia de zonas horarias es aceptable. Para un producto vendido a varios comercios con datos consolidados, no: dos instalaciones en husos distintos producirían fechas incomparables.
- **La hora de un consumo o de una producción no la elige el usuario**: siempre se estampa `TimeOnly.FromDateTime(DateTime.Now)` (`ConsumoRepository.cs:23`, `ProduccionRepository.cs:22`) aunque el usuario haya elegido una fecha pasada. Al modificar, sí se puede corregir.

#### Cultura — inconsistente

El arranque fuerza `en-US` para todas las peticiones:
```csharp
// Program.cs:11-20
// Usamos "en-US" porque es una cultura que siempre usa '.' para los decimales.
var supportedCultures = new[] { new CultureInfo("en-US") };
options.DefaultRequestCulture = new RequestCulture("en-US");
```
La razón es válida: garantiza que el *binding* de decimales que llegan del navegador (`step="0.01"`, siempre con punto) funcione. Pero el precio es que **el formato de salida por defecto de toda la aplicación es estadounidense**, y entonces hay que forzar `es-AR` a mano en cada lugar:

```csharp
// Models/Cultura.cs:9
public static readonly CultureInfo CulturaES = new CultureInfo("es-AR");
```

**Y no se hace en todos lados.** Conviven tres estilos de formateo de moneda:

| Estilo | Resultado | Dónde (ejemplos) |
|---|---|---|
| `.ToString("N2", CG.CulturaES)` con `$` a mano | `$12.345,67` ✅ | `_VentasTabla.cshtml:29`, `_ProductosTabla.cshtml:25-27`, `Dashboard/Index.cshtml` (todos), `Gastos/Index.cshtml:53` |
| **`.ToString("C")` sin cultura** → usa la cultura de la petición = **en-US** | **`$12,345.67`** ❌ | `Views/Productos/Eliminar.cshtml:41,48`; `Views/Ventas/Eliminar.cshtml:33`; `Views/Compras/Eliminar.cshtml:33`; `Views/Proveedores/Eliminar.cshtml:34`; `Views/Promociones/Eliminar.cshtml:24,27`; `Views/Promociones/_PromocionesTabla.cshtml:23-26` |
| `.ToString("C", new CultureInfo("es-AR"))` | `$ 12.345,67` ✅ | `ViewModels/Proveedores/ListarProveedoresViewModel.cs:20-21` |

**Resultado visible: en el listado de promociones y en las seis pantallas de confirmación de borrado, los importes se muestran en formato estadounidense** (`$1,234.56`), mientras que en el resto del sistema se muestran en formato argentino (`$1.234,56`). Un usuario que confirma la eliminación de una venta de "$1.234,56" ve "$1,234.56" y puede leerlo como mil doscientos treinta y cuatro con cincuenta y seis... o como uno con veintitrés. Lo mismo pasa con el mensaje de error de promociones: `$"El precio no puede ser menor que el costo total ({costoTotal:C})."` (`Controllers/PromocionesController.cs:57`).

En JavaScript, en cambio, **todo** se formatea con `toLocaleString('es-AR', { style: 'currency', currency: 'ARS' })` de forma consistente (34 usos).

---

### B9. Huella fiscal

**No existe absolutamente nada.**

Búsqueda exhaustiva en todo el repositorio (`.cs`, `.cshtml`, `.json`, `.sql`) de: `CUIT`, `IVA`, `AFIP`, `CAE`, `factura`, `comprobante`, `ticket`, `remito`, `punto de venta`, `razón social`. **Cero coincidencias reales** (las únicas que devuelve la búsqueda son falsos positivos: `private`, `Privacy`, `Identity`).

En concreto, no hay:

- Ningún campo de CUIT, DNI o identificación tributaria, ni del comercio ni de terceros.
- Ninguna condición frente al IVA (responsable inscripto, monotributo, consumidor final), ni alícuotas, ni discriminación de impuestos. **Todos los precios son un único número sin desglose.**
- Ningún tipo de comprobante (A, B, C, X), ningún punto de venta, ninguna numeración de comprobante. El "número" que muestra el listado de ventas **se calcula en el navegador** contando filas hacia atrás (`Views/Ventas/Index.cshtml:135`: `$(this).find(".numeroVenta").text(num--);`): **no es un número de comprobante, es la posición en la tabla filtrada, y cambia si cambian los filtros.**
- Ningún CAE, código de barras fiscal, QR de AFIP, ni estructura para almacenarlos.
- Ninguna integración ni preparación para facturación electrónica.
- Ninguna entidad "cliente" a la que emitirle un comprobante.

La entidad `Proveedores` tiene un único campo de texto libre `Contacto varchar(100)` (`AppDbContext.cs:47`) donde caben nombre y teléfono mezclados; no hay CUIT ni domicilio.

**Interpretación:** el sistema es un registro interno de gestión, no un sistema de facturación. Vender esto como producto en Argentina obliga a construir toda la capa fiscal desde cero, y esa capa **condiciona el modelo de datos hacia atrás**: necesita clientes, numeración por punto de venta, inmutabilidad de comprobantes emitidos (hoy cualquier venta se edita o se borra sin dejar rastro), y desglose de IVA en cada línea.

---

### B10. Seguridad

#### Autenticación y autorización: no existen

**Cualquiera que llegue a la aplicación tiene acceso total e irrestricto a todo**: registrar y borrar ventas, editar precios y stock, borrar proveedores, ver el dashboard con la facturación completa.

- No hay ningún atributo `[Authorize]` en ningún controlador ni acción (0 ocurrencias).
- No hay `AddAuthentication`, ni esquema de cookies, ni sesión (`AddSession` no se llama).
- `Program.cs:73` llama a `app.UseAuthorization()` **sin `UseAuthentication()` y sin ningún servicio de autorización que requiera algo**: es un middleware que no hace nada.
- No hay entidad de usuario, ni roles, ni permisos.

**Único mitigante:** el servidor escucha en `http://localhost:5146` (loopback), así que sólo es accesible desde la propia máquina. La seguridad del sistema es, hoy, la seguridad física de esa PC y su sesión de Windows. En el momento en que se lo exponga a una red —que es exactamente lo que exige convertirlo en producto— **queda completamente abierto**.

Dato de contexto: el proyecto del que deriva **sí tenía login** con usuario, contraseña y roles Admin/Cliente (rama `TP8`, `Controllers/LoginController.cs`). Ese login guardaba la contraseña **en texto plano** (`Models/Usuarios.cs` de TP8: `private string contraseña;` sin hash) y comparaba credenciales directamente contra la base. Se eliminó por completo al reconvertir el proyecto. **No hay contraseñas hasheadas ni en texto plano en el sistema actual, porque no hay contraseñas de usuario en absoluto.**

#### Credenciales en el repositorio

La contraseña de la base de datos está **en texto plano y versionada en git, en tres archivos**:

```json
// appsettings.json:11
"DefaultConnection": "Host=localhost;Port=5432;Database=polleria;Username=postgres;Password=1234;"
```
```ini
// postgresql.conf:6
Password=1234
```
```
// bin/Debug/net9.0/appsettings.json  (copia compilada, también versionada)
```

Agravantes: el usuario es **`postgres`**, el superusuario del motor —no un usuario de aplicación con permisos acotados—, y la contraseña es **`1234`**. El repositorio tiene remoto en `https://github.com/Olme2/Programa.git`; **desde el repositorio no se puede determinar si ese remoto es público o privado**, pero si alguna vez fue público o llega a serlo, la credencial ya está en el historial de git y no se borra cambiando el archivo.

No hay ninguna clave de API ni token de terceros en el repositorio (el sistema no consume ningún servicio externo).

#### Protección CSRF: casi completa, con tres huecos

`Program.cs:27-33` configura antiforgery con nombre de cookie propio, `HttpOnly`, `SameSite=Strict` y `SecurePolicy.None` (razonable en HTTP local). La mayoría de los POST llevan `[ValidateAntiForgeryToken]`. **Faltan en tres:**

| Acción | Ubicación | Nota |
|---|---|---|
| `MetodosPagoController.Modificar` (POST) | `Controllers/MetodosPagoController.cs:78-79` | ⚠️ Es el único POST de un ABM sin el atributo, mientras `Alta` y `Eliminar` del mismo controlador sí lo tienen. Parece un olvido |
| `ProductosController.FiltrarProductos` (POST) | `Controllers/ProductosController.cs:37-38` | Sólo lectura; el formulario AJAX no envía token |
| `ComprasController._BuscarCompras` (POST) | `Controllers/ComprasController.cs:53-54` | Sólo lectura; ídem |

#### Inyección SQL: bien resuelta

Las tres únicas sentencias SQL escritas a mano usan **parámetros**, no concatenación:

```csharp
// Repositorios/ComprasRepository.cs:97, 101-103, 131
_context.Database.ExecuteSqlRaw("DELETE FROM detalle_compra WHERE id_compra = {0}", idCompra);
_context.Database.ExecuteSqlRaw(
    "INSERT INTO detalle_compra (id_compra, id_producto, cantidad, costo_unitario) VALUES ({0}, {1}, {2}, {3})",
    idCompra, d.IdProducto, d.Cantidad, d.CostoUnitario);
```
`ExecuteSqlRaw` con marcadores `{0}` genera parámetros reales (`DbParameter`), no interpolación de texto. **No hay ninguna consulta construida por concatenación de strings en todo el sistema.** El resto es LINQ, que siempre parametriza.

#### Cross-site scripting: sí hay, por diseño de las tablas

Varias pantallas construyen HTML en el servidor y lo emiten sin escapar. Los nombres de productos y promociones —que el usuario escribe— terminan interpretados como HTML:

```csharp
// Repositorios/VentaRepository.cs:69-74 — arma HTML con nombres de producto
ProductosYPromociones = v.VentaPromociones.Any()
    ? string.Concat(string.Join("<br>", v.VentaPromociones.Select(vp => vp.Promocion.Promocion + " x" + vp.Cantidad)), "<br>",
                    string.Join("<br>", v.DetallesVenta.Select(dv => dv.Producto.Producto + " " + dv.Cantidad)))
    : string.Join("<br>", v.DetallesVenta.Select(dv => dv.Producto.Producto + " " + dv.Cantidad));
```
```csharp
// Repositorios/PromocionesRepository.cs:30 — HtmlString: se emite sin escapar
ProductosConcatenados = new HtmlString(string.Join("<br>", p.DetallesPromocion.Select(...)))
```
Y los puntos de emisión:
- `Views/Ventas/_VentasTabla.cshtml:8-17,27` — `ResaltarCoincidencia` devuelve `HtmlString`
- `Views/Ventas/Eliminar.cshtml:27` — `@Html.Raw(Model.ProductosYPromociones)`
- `Views/Productos/_ProductosTabla.cshtml:6-15,22-23`, `Views/Promociones/_PromocionesTabla.cshtml:6-14`, `Views/Proveedores/_ProveedoresTabla.cshtml:7-15,22`
- `Views/Consumo/Index.cshtml:57` y `Views/Produccion/Index.cshtml:55` — `@Html.Raw(...)`

Un producto llamado `<img src=x onerror=alert(1)>` se ejecuta en el listado de ventas. Hoy el atacante y la víctima son la misma persona (un solo usuario local), así que el riesgo práctico es bajo. **En un escenario multi-comercio o con más de un usuario, es una vulnerabilidad real de XSS almacenado.**

#### Inyección de expresión regular / denegación de servicio

El término de búsqueda del usuario se pasa **directamente al constructor de `Regex`**, sin escapar ni acotar:

```csharp
// Views/Ventas/_VentasTabla.cshtml:14   (idéntico en _ProductosTabla:12, _PromocionesTabla:12, _ProveedoresTabla:13)
var regex = new System.Text.RegularExpressions.Regex(busqueda, RegexOptions.IgnoreCase);
string textoResaltado = regex.Replace(texto, $"<span class='highlight'>$&</span>");
```

Dos problemas: **(a)** escribir un paréntesis suelto `(` o un corchete `[` en cualquier buscador lanza `ArgumentException` al renderizar la tabla → la vista parcial falla → la tabla no se actualiza (y en dos de las cuatro pantallas eso ocurre en silencio, ver C3); **(b)** una expresión con retroceso catastrófico bloquea un hilo del servidor indefinidamente (no hay `Regex` timeout configurado).

#### Otros

- **Sin HTTPS.** `app.UseHttpsRedirection()` (`Program.cs:68`) está presente pero **no hace nada**: `app.Run("http://localhost:5146")` no expone ningún puerto HTTPS, así que el middleware registra una advertencia y deja pasar. Todo el tráfico es HTTP plano.
- **Sin validación de tipo/tamaño de entrada más allá de los atributos** de los ViewModels. Los atributos existen y son razonablemente completos (`[Required]`, `[StringLength]`, `[Range]` en casi todos los campos).
- **Sin limitación de tasa** en los endpoints de autocompletado, que se disparan con `delay: 0` en cada tecla.
- **Higiene del repositorio:** no hay `.gitignore`; están versionados `bin/`, `obj/`, `.vs/` (incluidos archivos `.suo` de Visual Studio), los binarios de un proyecto anterior (`tl2-tp6-2024-Olme2.dll/.exe/.pdb`), y `build_log.txt` / `log.text` con rutas absolutas del equipo del autor.
- **Sin registro de auditoría.** Los `ILogger` escriben a la consola del proceso; no hay archivo de log, no hay retención, y nada registra quién hizo qué (no hay usuarios).

---

### B11. Distribución, versionado y configuración

#### Cómo se empaqueta y se instala

**No hay ningún mecanismo de empaquetado ni de instalación.** No existe: perfil de publicación (`Properties/PublishProfiles/`), `Dockerfile`, `docker-compose`, script de instalación, instalador MSI/MSIX, ni configuración de CI (`.github/`, `azure-pipelines.yml`). Verificado por listado del repositorio.

El commit más reciente se llama **"borre publish para hacer el sync"** (`01b1f50`, 2026-05-11) y el anterior relevante **"hice el publish del sistema 27/03"** (`d9dc1fe`): la carpeta publicada se generaba a mano desde Visual Studio, se versionaba, y se borró para poder sincronizar el repositorio. **El artefacto que efectivamente corre en la pollería no está en el repositorio y no hay forma de reproducirlo desde acá salvo compilando.**

El binario que sí quedó versionado es la salida de **Debug**: `bin/Debug/net9.0/entornoPolleria.exe`, con fecha de compilación 2026-06-12 (posterior al último commit de código, del 2026-05-11).

#### Qué hace falta para instalarlo en una máquina nueva

1. .NET 9 (el `runtimeconfig.json` pide `9.0.0-rc.1`; con *roll-forward* sirve un 9.0.x final).
2. PostgreSQL 17, escuchando en `localhost:5432`, con el superusuario `postgres` y contraseña `1234` (o editar `appsettings.json`).
3. Una base llamada exactamente `polleria`.
4. El esquema — **y acá se rompe la cadena**: `db/polleria.sql` es un dump *custom* de `pg_dump` (requiere `pg_restore`, no `psql`), **está desactualizado** respecto del modelo actual (le faltan `venta.redondeo`, `venta.tipo`, `compra.pagada`, la tabla `gasto`, `producto.costo_pendiente` y `producto.stock_umbral`) y **le sobra** `compra.total NOT NULL`, que rompería el alta de compras. Además fija un locale de Windows (`Spanish_Spain.1252`) que impide restaurarlo en Linux.
5. Los triggers: tres están en el dump; `tg_compra_pagada_cambio` está en `db/2026-05-10-compra-pagada-trigger.sql`; y **`tg_actualizar_deuda_proveedor` no está en ninguna parte** (ver A3 regla 8).
6. Datos semilla: al menos un método de pago (si no hay ninguno, el consumo falla con excepción) y, para que la regla de "Yo" funcione, uno llamado exactamente `Yo` y otro exactamente `Efectivo`. **Nada de esto está documentado ni se crea automáticamente.**

**Conclusión: hoy no es instalable desde el repositorio.** Se puede compilar, pero no se puede construir una base funcional.

#### Número de versión

No hay ninguno definido. El `.csproj` no declara `<Version>`, `<AssemblyVersion>` ni `<InformationalVersion>`, así que el SDK usa el valor por defecto:

```csharp
// obj/Debug/net9.0/entornoPolleria.AssemblyInfo.cs (generado)
[assembly: AssemblyFileVersion("1.0.0.0")]
[assembly: AssemblyInformationalVersion("1.0.0+01b1f5066c4c53e1b4254bd79ba32ee7cb08cc03")]
[assembly: AssemblyVersion("1.0.0.0")]
```

El único dato útil es el hash de commit que SourceLink adjunta a `InformationalVersion`. **La versión no se muestra en ninguna pantalla**: el usuario no tiene forma de saber qué versión está usando, y el desarrollador no tiene forma de saber qué versión tiene instalada un cliente. El único "versionado" del producto está en los mensajes de commit (`d580f20`: *"commit PROGRAMA 1.0 TERMINADO"*).

#### Mecanismo de actualización

**No existe.** No hay comprobación de versión, ni descarga, ni notificación, ni migración de esquema automática. Actualizar significa: compilar, copiar archivos a mano sobre la carpeta del cliente, y aplicar a mano los cambios de base de datos que hagan falta —sabiendo cuáles son de memoria, porque no hay migraciones—.

#### Dónde vive la configuración

| Qué | Dónde | Editable sin recompilar |
|---|---|---|
| Cadena de conexión | `appsettings.json:10-12` (y copia en `bin/`) | Sí |
| Niveles de log | `appsettings.json:2-8` | Sí |
| `postgresql.conf` en la raíz | Duplica la cadena de conexión | **No lo lee nadie**; es un archivo huérfano |
| URL y puerto | **`Program.cs:89`, hardcodeado** | ❌ No |
| Horarios de turno | **`VentasController.cs:50-51` y `VentaRepository.cs:21-26`, hardcodeados y duplicados** | ❌ No |
| Billetes para el vuelto | **`Views/Ventas/Alta.cshtml:117-126, 268-270`** | ❌ No |
| Nombre y logo del comercio | **`_Layout.cshtml:21-24`** | ❌ No |
| Métodos "Yo" y "Efectivo" como reglas | **Código C# y JavaScript** | ❌ No |
| Cultura de la aplicación | **`Program.cs:14-19`** | ❌ No |

`appsettings.Development.json` sólo redefine niveles de log; no hay `appsettings.Production.json`.

---

### B12. Tests

**No hay tests. Ninguno.**

- No existe proyecto de tests: la solución tiene un único proyecto (`entornoPolleria.sln:6`).
- No hay referencias a xUnit, NUnit, MSTest, Moq, FluentAssertions ni ningún framework de testing en `entornoPolleria.csproj` ni en `deps.json`.
- No hay ningún archivo cuyo nombre contenga "test" fuera de `wwwroot/lib/` (verificado por búsqueda recursiva).
- No hay tests de integración, ni de base de datos, ni de interfaz, ni un solo script de prueba manual documentado.

Cobertura: **0%**. No hay nada que correr.

Consecuencia práctica para el objetivo comercial: las 28 reglas de negocio documentadas en A3 —incluidas las que gobiernan el inventario y la deuda con proveedores— **no tienen una sola prueba que verifique que siguen funcionando después de un cambio**. En un sistema donde la lógica crítica vive en triggers de base de datos parcialmente no versionados, la ausencia de tests significa que cualquier reescritura se hace a ciegas: no hay forma de comprobar que el comportamiento nuevo es equivalente al viejo.

---

## PARTE C — Cómo se ve

### C1. Inventario visual

#### Tecnología de interfaz

Razor Views del lado del servidor + Bootstrap 5.1.0 + jQuery 3.6.0 + Select2 4.1.0-rc.0 + Chart.js 4.4.0. **No hay framework de componentes de front-end** (no hay React, Vue, ni Blazor), no hay bundler, no hay TypeScript, no hay preprocesador CSS. Las páginas se renderizan enteras en el servidor y las tablas se refrescan con `$.ajax` devolviendo HTML parcial.

Hay **una hoja de estilos propia**, `wwwroot/css/site.css` (625 líneas), con un encabezado que declara la intención:

```css
/* ================================================================
   SISTEMA POLLERÍA — Design System
   Tema: Dark sidebar + warm amber accent + clean cards
   ================================================================ */
```

Y **una segunda hoja residual**, `Views/Shared/_Layout.cshtml.css` (48 líneas), que es la plantilla por defecto de ASP.NET sin tocar (`a.navbar-brand`, `.footer`, `.box-shadow`), se compila al bundle *scoped* `entornoPolleria.styles.css` y se sigue enlazando en `_Layout.cshtml:10` **aunque ninguna de sus reglas se usa**.

#### Paleta de colores exacta

**Variables CSS declaradas** (`wwwroot/css/site.css:10-39`) — este es el sistema real:

| Variable | Valor | Uso |
|---|---|---|
| `--sidebar-bg` | `#1a1f2e` | Fondo de la barra lateral y de los encabezados de tabla |
| `--sidebar-hover` | `#252d40` | Hover de la navegación |
| `--sidebar-active` | `#e67e22` | (declarada; el ítem activo usa `--accent`, mismo valor) |
| `--sidebar-text` | `#a0aec0` | Texto de la navegación |
| `--accent` | `#e67e22` | **Color de marca**: naranja ámbar. Botones primarios, FAB, foco de formularios, borde superior del total |
| `--accent-dark` | `#cf6d17` | Hover de botones primarios |
| `--accent-light` | `#fdf0e6` | Declarada, **sin uso** |
| `--bg-page` | `#f0f2f5` | Fondo de página |
| `--bg-card` | `#ffffff` | Fondo de tarjetas, topbar, tablas |
| `--border` | `#e2e8f0` | Todos los bordes |
| `--text-primary` | `#1a202c` | Texto principal |
| `--text-secondary` | `#718096` | Etiquetas de formulario; también fondo de `.btn-secondary` |
| `--text-muted` | `#a0aec0` | Etiquetas de chips, secciones de la barra lateral |
| `--success` | `#38a169` | Verde: botones de éxito, ganancia |
| `--danger` | `#e53e3e` | Rojo: botones de borrado, costos |
| `--info` | `#3182ce` | Azul |
| `--warning` | `#d69e2e` | Amarillo: botones de modificar |

**Colores hexadecimales que aparecen fuera de las variables** (rompen el sistema):

| Valor | Dónde | Qué es |
|---|---|---|
| `#2f855a` | `site.css:336` | Hover de `.btn-success` |
| `#c53030` | `site.css:339` | Hover de `.btn-danger` |
| `#b7791f` | `site.css:342` | Hover de `.btn-warning` |
| `#4a5568` | `site.css:345` | Hover de `.btn-secondary` |
| `#f7f9fc` | `site.css:286` | Hover de fila de tabla |
| `#e2e8f0` | `site.css:272` | Texto del encabezado de tabla |
| `#fafafa` | `site.css:513` | Fondo de las filas de detalle |
| `#f0fff4` / `#276749` | `site.css:418` | Alerta de éxito |
| `#fff5f5` / `#9b2c2c` | `site.css:419` | Alerta de error |
| `#fed7d7` / `#9b2c2c` | `site.css:525-526` | Fila de producto sin stock en el desplegable |
| `#fef3c7` | `site.css:505` | Amarillo del resaltado de búsqueda |
| `rgba(230,126,34,.5)` / `.6` / `.15` | `site.css:372,383,399,490` | Sombras y foco del acento |
| **`#e67e22`** | `Views/Dashboard/Index.cshtml:72,74` | ⚠️ El color de acento **escrito a mano en estilos en línea**, no vía variable |
| **`#a0aec0`** | `Views/Dashboard/Index.cshtml:191` | Color por defecto de Chart.js, a mano |
| **`#27ae60`, `rgba(39,174,96,.12)`** | `Views/Dashboard/Index.cshtml:205-206` | Verde de la serie "Venta" — **es otro verde**, distinto de `--success` (`#38a169`) |
| **`#e67e22`, `rgba(230,126,34,.1)`** | `Views/Dashboard/Index.cshtml:214-215` | Naranja de la serie "Ganancia" |
| **`#e74c3c`, `#e67e22`, `#3498db`, `#9b59b6`** | `Views/Dashboard/Index.cshtml:255` | Paleta de la dona — **rojo, azul y violeta que no existen en el sistema** (`#e74c3c` ≠ `--danger` `#e53e3e`; `#3498db` ≠ `--info` `#3182ce`) |
| `rgba(255,255,255,.05)` | `Views/Dashboard/Index.cshtml:237,240` | Grilla de los gráficos — **gris casi blanco sobre fondo blanco: prácticamente invisible.** Está pensado para un tema oscuro que la página no tiene |
| `#0077cc`, `#1b6ec2`, `#1861ac`, `#e5e5e5` | `Views/Shared/_Layout.cshtml.css:11,16-17,23-24,27,30` | ⚠️ **Azules de la plantilla por defecto de ASP.NET**, todavía enlazados. `.btn-primary` se define ahí con `#1b6ec2` y en `site.css` con `--accent`; gana el que cargue último |

**Conclusión de la paleta:** hay un sistema de diseño real y coherente en `site.css` (17 variables, bien nombradas), pero **el dashboard no lo usa**: define su propia paleta de gráficos con colores que se parecen a los del sistema sin ser los mismos, y arrastra un tema oscuro que no corresponde. Y sigue enlazada la hoja de la plantilla original de Microsoft con azules que contradicen la marca.

#### Tipografías

Una sola familia: **Inter**, importada desde Google Fonts dentro del CSS:

```css
/* wwwroot/css/site.css:7 */
@import url('https://fonts.googleapis.com/css2?family=Inter:wght@300;400;500;600;700&display=swap');
/* :46 */
font-family: 'Inter', system-ui, -apple-system, sans-serif;
```
Se importan cinco pesos (300, 400, 500, 600, 700) y se usan cuatro (400, 500, 600, 700); el 300 nunca se usa. Chart.js la redeclara a mano (`Views/Dashboard/Index.cshtml:192`). **Sin internet, todo cae al `system-ui` de reserva** y la tipografía del sistema cambia.

Tamaño base: `14px` (`site.css:47`), más chico que el 16px habitual — es una interfaz densa, pensada para mostrar muchas filas.

**Escala tipográfica** (todo en px, sin variables): 10, 10.5, 11, 12, 13, 13.5, 14, 15, 16, 17, 20, 22, 26. Trece tamaños distintos, con incrementos de medio píxel (`13.5px` para el cuerpo de las tablas y de los formularios). No es una escala, es una lista.

#### Sistema de espaciado

**No existe.** Hay variables para radio de borde (`--radius: 10px`, `--radius-sm: 6px`), sombra (`--shadow`, `--shadow-lg`) y transición (`--transition: 150ms ease`), pero **ninguna para espaciado**. Los valores están escritos a mano y no siguen una escala consistente: `2px, 4px, 5px, 6px, 7px, 8px, 9px, 10px, 11px, 12px, 14px, 16px, 18px, 20px, 24px, 28px`.

En las vistas, el espaciado se resuelve con clases de utilidad de Bootstrap (`mb-3`, `mt-4`, `g-2`, `g-3`, `g-4`, `p-2`, `py-3`, `ms-2`), mezcladas de forma irregular: por ejemplo `class="row g-2"` en unos filtros y `class="row g-3"` en otros, `mb-3` en la mayoría de los formularios y `mb-4` en algunos.

Hay además **estilos en línea** en las vistas donde faltó una clase: `style="position: sticky; top: 20px;"` (`Ventas/Alta.cshtml:96`, `Ventas/Modificar.cshtml:94` — duplicando lo que ya hace `.summary-card`), `style="flex:1 1 140px;"` repetido siete veces en el dashboard, `style="max-height:280px;"`, `style="margin-bottom:0"`.

---

### C2. Componentes de interfaz

#### Qué se repite y dónde está definido

| Componente | ¿Unificado? | Dónde |
|---|---|---|
| **Barra lateral de navegación** | ✅ Sí, uno solo | `Views/Shared/_Layout.cshtml:19-109` |
| **Barra superior** | ✅ Sí | `_Layout.cshtml:114-116` |
| **Alertas de éxito/error** | ✅ Sí, un único punto | `_Layout.cshtml:120-133`, alimentadas por `TempData["SuccessMessage"]` / `TempData["ErrorMessage"]` |
| **Botones** | ✅ Estilo unificado en CSS (`.btn`, `.btn-primary`, `.btn-success`, `.btn-danger`, `.btn-warning`, `.btn-secondary`, `.btn-outline-primary`) | `site.css:319-355` |
| **Botón flotante "+"** | ✅ Clase `.btn-float` | `site.css:357-384`, usado en 9 pantallas |
| **Panel de filtros** | ✅ Clase `.filter-panel` + `.filter-title` | `site.css:243-260`, usado en 8 pantallas — pero el **contenido** se reescribe entero en cada vista |
| **Chips de estadísticas** | ✅ Clase `.stat-chip` con modificadores `.danger/.success/.info/.warning/.accent` | `site.css:205-241`, usado en 6 pantallas. ⚠️ El dashboard usa además `.sublabel`, **que no está definido en el CSS** |
| **Tablas** | ✅ Clases `.table-wrapper` + `.table` con encabezado oscuro | `site.css:262-317` |
| **Tarjetas** | ✅ `.card` / `.card-header` / `.card-body` (sobreescribiendo Bootstrap) | `site.css:186-203` |
| **Panel de resumen lateral** | ✅ `.summary-card` / `.summary-item` / `.summary-item.total` | `site.css:444-476`, usado en Ventas, Compras, Consumo, Producción, Promociones |
| **Fila de detalle de comprobante** | ⚠️ Parcial | Clase `.detalle-item-producto` en CSS, pero **el marcado está duplicado en 7 parciales distintos** |
| **Resaltado de coincidencias de búsqueda** | ❌ **Duplicado 4 veces** | La misma función `ResaltarCoincidencia` copiada literalmente en `_VentasTabla.cshtml:8-17`, `_ProductosTabla.cshtml:6-15`, `_PromocionesTabla.cshtml:6-14`, `_ProveedoresTabla.cshtml:7-15` |
| **Formularios** | ❌ Cada pantalla los arma a mano | 20+ vistas con la misma estructura `form-group > label + input + span de validación` repetida |
| **Pantallas de confirmación de borrado** | ❌ **Seis implementaciones distintas** | `Productos/Eliminar` y `Proveedores/Eliminar` usan `<dl class="row">` suelto; `Ventas/Eliminar` y `Compras/Eliminar` usan `<div class="card bg-light col-md-8">`; `Promociones/Eliminar` usa `card` sin sombra; `MetodosPago/Eliminar` es un `<h4>` con dos botones. Existe una clase `.confirm-card` en el CSS **que ninguna de las seis usa** |
| **Modales** | ❌ No hay ninguno | Se usa `confirm()` nativo del navegador (Consumo, Producción, Gastos) o una página entera de confirmación (el resto) |
| **Inicialización de Select2** | ❌ **Duplicada 9 veces** | Bloques de 20-40 líneas casi idénticos en `Ventas/Alta`, `Ventas/Modificar`, `Compras/Alta`, `Compras/Modificar`, `Consumo/Alta`, `Consumo/Modificar`, `Produccion/Alta`, `Produccion/Modificar`, `Promociones/Alta`, `Promociones/Modificar`, `Productos/Alta`, `Productos/Modificar` |
| **Función de reindexado de filas dinámicas** | ❌ **Duplicada 8 veces** con tres variantes distintas | `Ventas/Alta:218-243`, `Ventas/Modificar:192-217`, `Compras/Alta:132-144`, `Compras/Modificar:122-137` (esta con código muerto: calcula `propertyName` y no lo usa), `Consumo/Alta:96-107`, `Consumo/Modificar:101-111`, `Promociones/Alta:162-175`, `Promociones/Modificar` |
| **El parche de foco de Select2** | ❌ **Copiado literalmente 8 veces, comentarios incluidos** | El bloque `// --- INICIO DE LA CORRECCIÓN DEFINITIVA ---` … `$(document).on('select2:open', ...)` aparece idéntico en 8 vistas. En `Ventas/Alta.cshtml` está **dos veces en el mismo archivo** (líneas 180-186 y 364-374) |

#### ¿Hay algo parecido a un sistema de diseño?

**Sí, a medias, y sólo en la capa de estilos.** `site.css` es un sistema de diseño genuino: variables semánticas, componentes con nombre (`.stat-chip`, `.filter-panel`, `.summary-card`, `.table-wrapper`, `.btn-float`), overrides consistentes de Bootstrap, y un tema visual claramente definido (barra lateral oscura, acento ámbar, tarjetas claras). El resultado visual es **coherente y bastante por encima de lo que suele verse en un sistema de este tamaño**.

**Pero no hay sistema de componentes.** Razor ofrece parciales, view components y tag helpers, y prácticamente no se usan: hay 7 parciales de fila de detalle y 5 de tabla, todos específicos de una pantalla. **Todo lo demás —formularios, confirmaciones, inicialización de widgets, lógica de filas dinámicas— se resuelve copiando y pegando entre vistas.** El síntoma más claro es el comentario "CORRECCIÓN DEFINITIVA" replicado ocho veces: cada arreglo hay que aplicarlo ocho veces, y basta con olvidar una para que una pantalla quede distinta.

Estimación: de las ~4.950 líneas de `Views/`, alrededor de **1.200 son JavaScript embebido, y más de la mitad de ese JavaScript es código duplicado** entre pantallas.

---

### C3. Comportamiento de la interfaz

#### Estados: carga, error y vacío

| Estado | ¿Se contempla? | Detalle |
|---|---|---|
| **Vacío** | ✅ **Sí, y bien** | Todas las tablas tienen su mensaje: *"No se encontraron ventas para los filtros seleccionados"* (`_VentasTabla.cshtml:53-55`), *"No hay gastos en el período seleccionado"*, *"No hay ventas de este producto en el período seleccionado"*, *"Sin ventas en el período"*, *"Sin productos aun"*, *"Sin datos en este período"* (dashboard). Es el estado mejor cubierto del sistema |
| **Carga** | ❌ **Casi no** | Sólo hay un indicador en todo el sistema: el botón "Copiar Lista de Precios" muestra un spinner (`Views/Productos/index.cshtml:88`). Las tablas que se refrescan por AJAX —ventas, compras, productos, promociones, proveedores— **no muestran nada mientras cargan**: la tabla vieja queda en pantalla y se reemplaza de golpe. En la caja, con el listado de ventas recargándose en cada tecla, eso significa que el usuario no sabe si está viendo datos actuales o viejos |
| **Error** | ⚠️ **Desigual** | Ver el detalle exhaustivo abajo |

#### Fallos silenciosos — lista completa

Punto crítico, porque el usuario final no es técnico. Se listan **todas** las rutas en las que algo puede fallar sin que aparezca nada en pantalla.

**A. Errores del servidor que sólo llegan a la consola del navegador**

| # | Qué falla | Endpoint | Dónde se pierde el error |
|---|---|---|---|
| 1 | Búsqueda dinámica de **proveedores** | `ProveedoresController._BuscarProveedores` devuelve `StatusCode(500)` (`:189-193`) | `wwwroot/js/site.js:21-24`: `error: function (err) { console.error("Error al buscar proveedores:", err); }`. **La tabla queda con los datos viejos y no pasa nada visible** |
| 2 | Búsqueda dinámica de **promociones** | `PromocionesController._BuscarPromociones` → `StatusCode(500)` (`:283-287`) | `Views/Promociones/Index.cshtml:57`: `error: function (err) { console.error("Error al buscar promociones:", err); }` |
| 3 | Autocompletado de **proveedores** (alta/modificación de producto) | `ProveedoresController.BuscarProveedores` → `StatusCode(500)` (`:213-217`) | Select2 no tiene manejador de error: el desplegable queda vacío, indistinguible de "no hay resultados" |
| 4 | Autocompletado de **productos para promoción** | `PromocionesController.BuscarProductosParaPromocion` → `StatusCode(500)` (`:315-319`) | Ídem |
| 5 | Autocompletado de **productos para venta** | `VentasController.BuscarProductosParaVenta` (`:219-247`) — **sin `try/catch`**: excepción no manejada → 500 | Ídem. **Es el buscador principal de la caja** |
| 6 | Autocompletado de **promociones para venta** | `VentasController.BuscarPromocionesParaVenta` (`:250-276`) — sin `try/catch` | Ídem |
| 7 | Autocompletado de **productos para compra** | `ComprasController.BuscarProductosParaCompra` / `BuscarProductosPorProveedor` (`:243-282`) — sin `try/catch` | Ídem |
| 8 | Autocompletado de **productos para consumo** | `ConsumoController.BuscarProductosParaConsumo` (`:154-179`) — sin `try/catch` | Ídem |
| 9 | Autocompletado de **productos para producción** | `ProduccionController.BuscarProductosParaProduccion` (`:135-160`) — sin `try/catch` | Ídem |
| 10 | Consulta de costo de producto | `ConsumoController.ObtenerCostoProducto` devuelve `NotFound()` (`:144`) | Ninguna vista llama a este endpoint; es código muerto, pero si se usara, un 404 no se mostraría |

**Los nueve buscadores con autocompletado —la forma principal de cargar cualquier comprobante— fallan sin decir nada.** Para el usuario, un error del servidor y "no hay productos que coincidan" se ven exactamente igual: un desplegable vacío.

**B. Errores que se registran en la consola del proceso, que nadie mira**

| # | Qué | Dónde |
|---|---|---|
| 11 | Fallos al crear, modificar o eliminar una compra | `Repositorios/ComprasRepository.cs:63, 119, 141`: `Console.WriteLine($"ERROR (Crear): {ex.Message} | Inner: {ex.InnerException?.Message}");` — escribe a la ventana de consola del ejecutable y relanza. El detalle de la excepción interna (que es donde está el mensaje real del trigger de PostgreSQL) **sólo existe en esa ventana** |
| 12 | Fallo al abrir el navegador al arrancar | `Program.cs:84-87`: `Console.WriteLine("No se pudo abrir el navegador automáticamente: " + ex.Message)`. Si falla, el usuario ve una consola negra y no sabe que tiene que abrir el navegador a mano |
| 13 | Todos los `_logger.LogError(...)` (24 ocurrencias) | Van al proveedor de consola por defecto. **No hay log a archivo**: si el usuario cierra la ventana, el diagnóstico se pierde |

**C. Validaciones que descartan datos sin avisar**

| # | Qué se descarta | Dónde |
|---|---|---|
| 14 | **Líneas de consumo y producción con producto o cantidad inválidos** | `ConsumoRepository.cs:26-29` y `ProduccionRepository.cs:25-28`: `.Where(d => d.IdProducto > 0 && d.Cantidad > 0)`. **Una línea mal cargada se descarta en silencio y el resto se guarda.** El usuario ve "Consumo registrado correctamente" y le falta un producto |
| 15 | **Detalles de compra con producto no encontrado** | En la modificación, `_ModificarDetalleCompraItem.cshtml:13,19` emite `name="IdProducto"` **sin índice de colección**; el reindexado a `DetallesCompra[i].IdProducto` depende enteramente del JavaScript de `reindexarFilas()`. Si ese JS no corre (error previo en la página, JS deshabilitado), **el POST llega con los nombres mal y la compra se guarda con los detalles equivocados o vacíos** |
| 16 | Promociones borradas al desactivar un producto | `PromocionesRepository.cs:83-86`: `_context.Remove(promocion)` para las promociones no iniciadas. **Borrado permanente, sin confirmación previa ni aviso posterior** |
| 17 | Costo y precio históricos de una venta al abrir "Modificar" | `DetalleVentaViewModel.cs:31-32` (ver A3 regla 16). **Se pierden datos históricos sin ningún aviso** |
| 18 | El **Recargo (%)** de toda venta | Ver A1 funcionalidad 1. **El usuario lo escribe, lo ve sumado en el total, y no se guarda** |

**D. Fallos de renderizado que rompen una pantalla en silencio**

| # | Qué | Dónde |
|---|---|---|
| 19 | Escribir `(`, `[`, `*` o `+` en cualquier buscador | `new Regex(busqueda)` sin escapar (`_VentasTabla.cshtml:14`, `_ProductosTabla.cshtml:12`, `_PromocionesTabla.cshtml:12`, `_ProveedoresTabla.cshtml:13`) lanza `ArgumentException` al renderizar la parcial → 500 → en proveedores y promociones, silencio total (casos 1 y 2) |
| 20 | Búsqueda de productos: el error **sí** se muestra, pero como texto crudo dentro de la tabla | `Views/Productos/index.cshtml:75`: `$('#tabla-productos-body').html('<tr><td colspan="9" class="text-center text-danger">Error al cargar los productos.</td></tr>')` |
| 21 | Promoción sin ingredientes → `.Min()` sobre secuencia vacía | `PromocionesRepository.cs:35-38` (ver A3 regla 5). Rompe la pantalla de Promociones **y** el autocompletado de promociones en la caja |
| 22 | Sin métodos de pago cargados → `.First()` sobre secuencia vacía | `ConsumoRepository.cs:18` / `ProduccionRepository.cs:17`. El alta de consumo falla con `InvalidOperationException`; el mensaje que llega al usuario es `"Error al registrar el consumo: Sequence contains no elements"` |
| 23 | JS de búsqueda de productos apuntando a una acción inexistente | `wwwroot/js/site.js:45` → `/Productos/_BuscarProductos` → 404 → `console.error`. Como `site.js` se carga en **todas** las páginas desde `_Layout.cshtml:143`, este 404 se dispara en cualquier pantalla que tenga un `#buscador-producto` |
| 24 | `site.js` cargado dos veces en Proveedores | `_Layout.cshtml:143` y otra vez en `Views/Proveedores/Index.cshtml:44` → los manejadores `keyup` se registran dos veces → **cada tecla dispara dos peticiones AJAX idénticas** |

**E. Fallos con aviso — para contrastar (lo que sí está bien resuelto)**

- Errores de negocio en el alta de venta: `TempData["ErrorMessage"] = ex.Message` con el mensaje de `InvalidOperationException`, que es legible (*"Stock insuficiente para el producto 'Pechuga'. Stock disponible: 2,500."*) — `VentasController.cs:138-144`.
- Fallo del listado de ventas y compras: mensaje visible *"No se pudo cargar el listado de ventas"* (`VentasController.cs:60-65`).
- Fallo del refresco AJAX de ventas y compras: `alert("Error al cargar los datos. Intente nuevamente.")` (`Ventas/Index.cshtml:112`, `Compras/Index.cshtml:80`). Es un `alert()` del navegador, feo pero visible.
- Todas las validaciones de formulario (`asp-validation-for`) se muestran junto al campo, en rojo, y son mensajes escritos en castellano.

#### Responsive

Hay tres `@media` en `site.css` y la intención existe, pero el resultado tiene un agujero grave:

```css
/* wwwroot/css/site.css:557-563 */
@media (max-width: 768px) {
  .sidebar { width: 0; overflow: hidden; }
  .main-content { margin-left: 0; }
  ...
}
```

**En pantallas de menos de 768px la barra lateral se oculta y no hay ningún sustituto: no hay botón de hamburguesa, no hay menú desplegable, no hay nada.** Es la única navegación del sistema. **En un teléfono, el usuario queda encerrado en la pantalla en la que esté**, sin forma de ir a ninguna otra salvo escribiendo la URL a mano. Verificado: no hay ningún `navbar-toggler`, ni `data-bs-toggle="offcanvas"`, ni JavaScript que muestre la barra.

El resto del comportamiento adaptativo:
- `@media (max-width: 1400px)` y `(max-width: 1100px)` reducen el ancho de la barra lateral (230px → 216px → 196px), la densidad de las tablas y el tamaño de los chips. Está bien pensado para portátiles.
- A menos de 1100px, `.summary-card` deja de ser fija (`position: static`), lo que evita que el resumen de la venta tape contenido.
- Las tablas están dentro de `.table-wrapper` con `overflow-x: auto` y `min-width: 760px` (`site.css:289-301`): **hacen scroll horizontal en vez de romper el layout**. Correcto.
- Las vistas usan clases de grilla de Bootstrap con puntos de corte (`col-lg-8`, `col-md-4`, `col-xl-2`), así que los formularios sí se reacomodan.

**Veredicto: está diseñado para monitor de escritorio (≥1100px), degrada aceptablemente en portátil, y en teléfono es inutilizable por falta de navegación.**

#### Accesibilidad y navegación por teclado

| Aspecto | Estado |
|---|---|
| `lang="es"` en el `<html>` | ✅ `_Layout.cshtml:2` |
| `<meta viewport>` | ✅ `_Layout.cshtml:5` |
| Etiquetas `<label>` asociadas a los campos | ✅ Generalizado, vía `asp-for` (que emite el `for` correcto) |
| Estructura semántica | ⚠️ Usa `<aside>` para la barra lateral, pero **no hay `<nav>`, `<main>`, `<header>`** ni ningún punto de referencia ARIA |
| `aria-label` | Sólo en los botones de cerrar de las alertas (`_Layout.cshtml:124,131`) |
| Iconos | ❌ **Todos los iconos de navegación son emojis dentro de `<span class="nav-icon">`** (🍗 📊 🛒 📋 📦 🛍️ 🍽️ 🏭 🚚 🎁 💳 💸). Un lector de pantalla los lee literalmente ("cara de pollo asado") |
| Botones sin texto | ❌ El botón de borrar fila es `&times;` sin `aria-label` (7 parciales). El botón flotante es un `+` sin descripción |
| Contraste | ⚠️ Barra lateral: texto `#a0aec0` sobre `#1a1f2e` ≈ 7:1, correcto. **Texto atenuado `--text-muted #a0aec0` sobre fondo blanco ≈ 2,3:1: no llega al mínimo AA de 4,5:1.** Se usa en todas las etiquetas de los chips y en los títulos de sección |
| Foco visible | ✅ Los campos tienen un anillo de foco ámbar bien visible (`site.css:397-401`). Los botones, no: `.btn` no define `:focus` |
| Orden de tabulación | Natural (no hay `tabindex` en ninguna parte). Pero **Select2 secuestra el foco**: hay un parche replicado 8 veces para forzar el foco al campo de búsqueda al abrir el desplegable (`Ventas/Alta.cshtml:364-374`, etc.) |
| Atajos de teclado | ❌ Ninguno. En un punto de venta, no poder cerrar una venta sin tocar el mouse es una limitación operativa real |
| Contenido dinámico | ❌ Las tablas se reemplazan por AJAX sin `aria-live`: un lector de pantalla no se entera de que cambiaron |
| `alert()` y `confirm()` nativos | Usados en Consumo, Producción, Gastos, Ventas y Compras. Accesibles, pero bloquean e interrumpen |

---

### C4. Textos y errores

#### Cómo se le habla al usuario

El tono general es **correcto, en castellano rioplatense, y dirigido a alguien no técnico**. Ejemplos representativos:

- *"No se puede eliminar el producto porque está en uso en promociones, ventas o compras. Prueba desactivandolo."* (`ProductosController.cs:175`)
- *"No se puede eliminar la promoción porque ya ha sido registrada en una o más ventas. Prueba con desactivarla colocandole la fecha de hoy como fin."* (`PromocionesController.cs:230`)
- *"No se puede eliminar el proveedor Granja San José, es referenciado en los productos Pollo entero, Pata muslo."* (`ProveedoresController.cs:127`)
- *"Esta promoción ya fue utilizada en ventas, por lo que su 'receta' (productos y cantidades) no puede ser alterada. Solo puedes modificar el nombre, precio y las fechas de vigencia."* (`Promociones/Modificar.cshtml:223`)
- *"Esta acción es permanente y no se puede deshacer. Se restaurará el stock de los productos involucrados."* (`Ventas/Eliminar.cshtml:9`)

**Estos mensajes son buenos**: explican la causa, nombran los objetos concretos que bloquean la operación y sugieren la alternativa. Es de lo mejor que tiene el sistema desde el punto de vista de producto.

Inconsistencias de estilo: se mezcla el tuteo (*"Prueba desactivandolo"*, *"Solo puedes modificar"*) con el impersonal (*"¿Está seguro que desea eliminar...?"*, *"Por favor seleccione un producto"*). Y hay faltas de ortografía visibles para el usuario: *"desactivandolo"*, *"colocandole"*, *"Solo puedes"*, *"el campo redondeo no puede estar vacio"*, *"no puede tener mas de 100 caracteres"*, *"maximo"*, *"minimo"*, y todo el módulo de Consumo y Producción está escrito **sin tildes** (*"Retiro de produccion registrado correctamente"*, *"No se puede agregar el mismo producto mas de una vez"*, *"No hay consumos en el periodo seleccionado"*, *"El stock sera devuelto"*).

#### Dónde se filtra detalle interno al usuario

| # | Mensaje que ve el usuario | Origen |
|---|---|---|
| 1 | **`"Error al registrar: " + e.Message`** | `ProduccionController.cs:62`. Si falla un trigger, el usuario lee: *"Error al registrar: 23514: new row for relation "producto" violates check constraint "stock_no_negativo""* |
| 2 | `"Error al modificar: " + e.Message` | `ProduccionController.cs:106` |
| 3 | `"Error al eliminar: " + e.Message` | `ProduccionController.cs:124`, `ConsumoController.cs:134` |
| 4 | `"Error al registrar el consumo: " + e.Message` | `ConsumoController.cs:71` |
| 5 | `"Error al modificar el consumo: " + e.Message` | `ConsumoController.cs:115` |
| 6 | `"Error: " + e.Message` | `GastosController.cs:46` |
| 7 | `"Error al eliminar: " + e.Message` | `GastosController.cs:63` |
| 8 | **`"Error al guardar la compra: " + ex.Message`** | `ComprasController.cs:107`. Aparece en el resumen de validación del formulario |
| 9 | `"Error al guardar los cambios: " + ex.Message` | `ComprasController.cs:160` |

**Nueve puntos donde el mensaje crudo de la excepción llega a la pantalla.** En el mejor caso son mensajes de los triggers, que están escritos en castellano y son entendibles (*"Stock insuficiente para el producto (ID: 37). Stock disponible: 2.500"* — aunque expone el ID interno en vez del nombre). En el peor caso son errores de PostgreSQL o de Npgsql en inglés con códigos SQLSTATE, nombres de tablas, nombres de restricciones y, si la conexión falla, **la cadena de conexión completa incluyendo el host y el usuario**.

Los mensajes de los triggers, además, **identifican los productos por ID numérico, no por nombre**: *"No hay stock suficiente para anular esta compra. El producto (ID: 37) ya fue utilizado en ventas."* El usuario no tiene forma de saber cuál es el producto 37.

En la dirección opuesta, los controladores de Ventas, Productos, Promociones, Proveedores y MetodosPago **sí** encapsulan: registran la excepción con `_logger.LogError` y muestran un mensaje genérico (*"Ocurrió un error al crear el producto"*). El problema es que ese mensaje genérico **no dice qué hacer** y el detalle técnico queda sólo en una consola que el usuario no ve.

#### Pantallas de error del sistema

`Views/Shared/Error.cshtml` está **íntegramente en inglés y en modo desarrollador**:

```html
<h1 class="text-danger">Error.</h1>
<h2 class="text-danger">An error occurred while processing your request.</h2>
<strong>Request ID:</strong> <code>@Model.RequestId</code>
<h3>Development Mode</h3>
<p>Swapping to <strong>Development</strong> environment will display more detailed information...
```

Es la plantilla original de Microsoft sin tocar. Un usuario de la pollería que llegue a esta página ve un texto en inglés que le habla de variables de entorno de ASP.NET Core. (Sólo se muestra si el entorno **no** es Development, `Program.cs:57-61`; si corre en Development —que es lo que configura `launchSettings.json`— ve directamente la **página de excepción del desarrollador con el stack trace completo y fragmentos de código fuente**.)

Lo mismo con `Views/Home/Index.cshtml` (*"Welcome — Learn about building Web apps with ASP.NET Core"*) y `Views/Home/Privacy.cshtml` (*"Use this page to detail your site's privacy policy"*).

#### Textos hardcodeados

**Todos, sin excepción.** No hay recursos de localización (`.resx`), no hay `IStringLocalizer`, no hay ningún archivo de textos. Cada cadena está escrita en el lugar donde se usa:

- Títulos y encabezados: en las vistas `.cshtml`.
- Mensajes de validación: en los atributos `[Required(ErrorMessage = "...")]` de los ViewModels (aproximadamente 60 mensajes).
- Mensajes de éxito y error: en `TempData["SuccessMessage"]` / `TempData["ErrorMessage"]` dentro de los controladores (aproximadamente 55 mensajes).
- Mensajes de la interfaz dinámica: en cadenas de JavaScript dentro de las vistas (*"Por favor seleccione un producto antes de agregar otro"*, repetido en 6 archivos con dos redacciones distintas — con y sin punto final).
- Mensajes de los triggers: en español, dentro del PL/pgSQL.

Traducir el sistema, o simplemente cambiar el vocabulario para venderlo a otro rubro (una carnicería, una verdulería), implica editar decenas de archivos a mano.

---

## PARTE D — Veredicto

### D1. ¿Funciona?

Son dos preguntas distintas y las respuestas son distintas.

---

#### D1.1 — ¿El código compila y arranca hoy?

**Compila: sí, con evidencia fuerte, aunque indirecta. Arranca: no determinable sin ejecutarlo.**

**Sobre la compilación.** No ejecuté `dotnet build` porque escribe en `bin/` y `obj/`, y esta auditoría es de sólo lectura. La evidencia disponible:

1. **El binario compilado existe y es posterior al último cambio de código.** `bin/Debug/net9.0/entornoPolleria.dll` tiene fecha **2026-06-12 01:01**; el último commit que toca código es del **2026-05-11**. Alguien compiló este código con éxito hace dos meses.
2. **El registro de compilación versionado no muestra un solo error de C#.** `build_log.txt` (UTF-16, de enero 2026) contiene 2 errores y 10 advertencias, y **todos son el mismo problema de copia de archivo**: `MSB3027` / `MSB3021`, *"No se pudo copiar apphost.exe en bin\Debug\net9.0\entornoPolleria.exe... El archivo se ha bloqueado por: entornoPolleria (24096)"*. Es decir: **la compilación de C# terminó bien y sólo falló el paso de copiar el ejecutable porque la aplicación estaba corriendo**. No hay ni un `error CS`.
3. El único mensaje del SDK es `NETSDK1057: Está usando una versión preliminar de .NET`.

Salvedad honesta: el `build_log.txt` es de enero de 2026 y el código cambió en marzo y mayo. La evidencia decisiva es el binario de junio.

**Sobre el arranque.** No se puede afirmar desde el repositorio. Requiere:
- Que exista PostgreSQL en `localhost:5432` con la base `polleria`, usuario `postgres`, contraseña `1234`.
- Que el esquema **real** de esa base coincida con el modelo de EF, cosa que el repositorio **no permite verificar** porque `db/polleria.sql` está desactualizado (B2).

Y hay un punto que sólo se puede resolver ejecutando, que señalo explícitamente porque afecta a la pantalla más usada:

```csharp
// Repositorios/VentaRepository.cs:61-83  — proyección de la consulta del listado de ventas
var ventasVM = query
    .Include(v => v.Metodo)
    .Include(v => v.DetallesVenta).ThenInclude(dv => dv.Producto)
    .Include(v => v.VentaPromociones).ThenInclude(vp => vp.Promocion)
    .Select(v => new ListarVentasVM()
    {
        ...
        Total = v.CalcularPrecioTotal(),                      // <-- método C# dentro de una proyección LINQ-to-SQL
        Costo = v.DetallesVenta.Sum(dv => dv.Cantidad * dv.CostoUnitario)   // <-- esto sí es traducible a SQL
               + v.VentaPromociones.Sum(vp => vp.Cantidad * vp.CostoPromo),
        ...
    });
```

`CalcularPrecioTotal()` es un método de instancia que recorre las colecciones en memoria (`Models/Ventas.cs:135-138`). EF Core no puede traducirlo a SQL; sólo puede resolverlo evaluándolo en el cliente, y para eso necesita que las colecciones `_detallesVenta` y `_ventaPromociones` estén pobladas — algo que **no está garantizado cuando hay una proyección a un tipo que no es la entidad**, porque en ese caso EF Core descarta los `Include`. Que la columna **Costo** de la misma proyección esté escrita como una expresión traducible mientras **Total** llama a un método sugiere que el propio autor no tenía claro el límite.

Los dos desenlaces posibles son: (a) funciona y muestra el total correcto; (b) muestra sólo el valor de `Redondeo` para cada venta. Dado que esta pantalla es de uso diario y que si mostrara casi cero en todas las filas se habría notado de inmediato, lo más probable es (a) — **pero no es determinable desde el repositorio y hay que abrir la pantalla con datos reales para confirmarlo.** Es el primer punto que verificaría al ejecutar.

---

#### D1.2 — ¿Un desconocido podría instalarlo y usarlo sin el autor presente?

**No.** Y no por un detalle, sino por seis obstáculos concretos, tres de ellos bloqueantes.

**Bloqueantes — es imposible llegar a un sistema funcional:**

1. **No se puede crear la base de datos desde el repositorio.** `db/polleria.sql` está desactualizado respecto del modelo: **le faltan** `venta.redondeo`, `venta.tipo`, `compra.pagada`, `producto.costo_pendiente`, `producto.stock_umbral` y la tabla `gasto` completa; y **le sobra** `compra.total NOT NULL CHECK (total >= 0.01)`, columna que el código actual nunca escribe, con lo cual **todo alta de compra fallaría**. Sin migraciones y sin un script actualizado, no hay forma de derivar el esquema correcto salvo leyendo `AppDbContext.cs` y escribiéndolo a mano.

2. **Falta una regla de negocio completa.** El trigger `tg_actualizar_deuda_proveedor` —el que hace que comprar mercadería aumente la deuda con el proveedor— **no está en ningún archivo del repositorio**, sólo se lo menciona en un comentario (`db/2026-05-10-compra-pagada-trigger.sql:9`). Una instalación nueva tendría el módulo de proveedores mostrando siempre deuda cero. La única fuente de esa regla es la base de datos que está corriendo en la pollería.

3. **El archivo de esquema ni siquiera se puede aplicar como se espera.** `db/polleria.sql` no es SQL: es un archivo binario en formato *custom* de `pg_dump` (verificado: magic `PGDMP`, `PostgreSQL custom database dump - v1.16-0`). `psql -f db/polleria.sql` falla. Hay que saber que se usa `pg_restore`. Y su `CREATE DATABASE` fija `LOCALE = 'Spanish_Spain.1252'`, que **no existe en Linux**.

**Serios — se puede sortear, pero nadie lo adivina:**

4. **Datos semilla no documentados y necesarios para que el sistema no falle.** Hace falta al menos un método de pago cargado o el alta de consumo lanza `InvalidOperationException: Sequence contains no elements` (`ConsumoRepository.cs:18`). Y para que las reglas de negocio funcionen tienen que existir métodos llamados **exactamente** `"Yo"` (imputación de consumo interno y exclusión del total del día) y **exactamente** `"Efectivo"` (filtro efectivo/virtuales). Nada de esto está escrito en ninguna parte: son reglas 12 y 17 de A3, descubiertas leyendo el código.

5. **No hay documentación de ningún tipo.** No hay `README`, ni `INSTALL`, ni notas, ni comentarios de instalación. Cero archivos `.md` en el repositorio antes de este informe. Los mensajes de commit son la única documentación existente, y son bitácora personal (*"Commit qcyo"*, *"commit intermedio para gemini"*, *"COMMIT TERMINADO POR FIN"*).

6. **No hay artefacto instalable.** No hay publicación, instalador, contenedor ni script. Hay que tener el SDK de .NET 9 y compilar. La carpeta publicada existió y se borró (`01b1f50`, *"borre publish para hacer el sync"*).

**Menores — afectan al uso, no a la instalación:**

7. **Configuración crítica hardcodeada**: puerto 5146, `localhost`, horarios de turno, billetes de vuelto, nombre "Pollería" y logo 🍗. Un segundo cliente recibiría un sistema que dice el nombre del rubro y muestra un emoji de pollo, atado a una sola PC.
8. **Dependencia de internet en tiempo de ejecución**: sin conexión se caen todos los buscadores con autocompletado (Select2 desde CDN) y el dashboard entero (Chart.js desde CDN). Un corte de internet deja al negocio sin poder cargar una venta.
9. **Sin usuarios ni contraseña**: quien tenga acceso a esa PC tiene acceso total, incluido el borrado de ventas y la facturación completa.

**Resumen:** una persona con conocimientos de .NET y PostgreSQL, con el código delante y varias horas, podría reconstruir un esquema funcional leyendo `AppDbContext.cs` — pero **no podría reconstruir la regla de la deuda de proveedores**, porque no está escrita en ninguna parte. Sin el autor o sin acceso a la base de producción, esa parte del sistema se pierde.

---

### D2. Tabla de reusabilidad

Una fila por módulo. Sin optimismo.

| Módulo | Categoría | Razón (una línea) |
|---|---|---|
| **Modelo de datos conceptual** (entidades, relaciones, PK compuestas de detalle) | Reusable con cambios menores | El modelo del dominio es correcto y está bien normalizado; sólo hay que agregar columna de comercio y cambiar el tipo de las claves. |
| **Tipos de claves primarias** (`int` autoincremental en todo) | El concepto sirve pero hay que rehacerlo | Impide convivencia de datos de dos instalaciones; hay que migrar a GUID o clave compuesta con comercio. |
| **Esquema SQL versionado** (`db/polleria.sql`) | **Descartable** | Formato equivocado, desactualizado, con columnas que rompen el código actual y locale de Windows. Hay que regenerarlo desde cero. |
| **Triggers de stock** (venta, compra, promoción) | El concepto sirve pero hay que rehacerlo | La lógica es correcta y está documentada, pero atada a PL/pgSQL: no es testeable, no es portable y no se puede reutilizar desde otra plataforma. |
| **Trigger de deuda de proveedores** | El concepto sirve pero hay que rehacerlo | **Además hay que recuperarlo primero**: no está en el repositorio. |
| **Reglas de negocio del rubro** (las 28 de A3) | Reusable tal cual — **como conocimiento** | El activo más valioso del proyecto. Documentadas acá, se pueden implementar en cualquier stack. |
| **Módulo Ventas** (alta con búsqueda, promociones, redondeo, vuelto) | Reusable con cambios menores | El flujo de caja está bien pensado y probado en uso real; hay que persistir el recargo y arreglar la revalorización al modificar. |
| **Listado de ventas con filtros** (turno, efectivo/virtual, texto) | Reusable con cambios menores | Buenos filtros; los totales calculados re-parseando texto en el navegador hay que rehacerlos en el servidor. |
| **Módulo Productos** (ABM, filtros, orden por venta semanal) | Reusable con cambios menores | Sólido; dos bugs puntuales (precio que se llena con el costo, bloqueo de stock/costo que no funciona). |
| **Módulo Promociones** (receta, costeo, stock del combo, bloqueo si ya se vendió) | Reusable con cambios menores | Muy buen modelado; hay que unificar las dos definiciones contradictorias de "vigente". |
| **Módulo Compras** | Reusable con cambios menores | Correcto de cara al usuario; el `DELETE`+`INSERT` con SQL crudo en la modificación hay que rehacerlo. |
| **Módulo Proveedores + cuenta corriente** | Reusable con cambios menores | El ABM está bien; la deuda depende del trigger perdido. |
| **Módulos Consumo y Producción** | Reusable con cambios menores | Buena idea (tres tipos de movimiento en una tabla); Producción está incompleto conceptualmente (falta el ingreso del elaborado). |
| **Módulo Gastos** | Reusable tal cual | Simple, aislado, sin dependencias. Sólo falta la edición. |
| **Módulo Métodos de Pago** | Reusable tal cual | ABM trivial y correcto. |
| **Dashboard** (KPIs, series, reparto proporcional, top productos) | Reusable con cambios menores | Los cálculos son buenos y el reparto proporcional del combo es valioso; hay que arreglar el bug de la semana que arranca en domingo y evitar materializar todo en memoria. |
| **Lista de precios para WhatsApp** | Reusable tal cual | 20 líneas, resuelve una necesidad real del negocio. |
| **Estadísticas por producto** | Reusable con cambios menores | Correcto; debería incluir consumo y producción. |
| **Capa de repositorios** (9 interfaces + 10 clases) | El concepto sirve pero hay que rehacerlo | Las interfaces son un buen punto de partida, pero devuelven ViewModels de presentación en lugar de tipos de dominio: el acoplamiento está invertido. |
| **Entidades de dominio** (`Models/`) | Reusable con cambios menores | POCOs limpios; buena parte de sus métodos está muerta y hay que decidir qué lógica sube ahí. |
| **ViewModels + validaciones** | Reusable con cambios menores | Las validaciones con `IValidatableObject` son correctas y están en castellano; atadas a `System.ComponentModel.DataAnnotations` y a MVC. |
| **Controladores** | El concepto sirve pero hay que rehacerlo | 2.185 líneas con lógica de negocio mezclada con orquestación, repoblado de formularios y `TempData`; los flujos son correctos pero el código no se reaprovecha. |
| **`Program.cs` / arranque** | **Descartable** | Puerto y host fijos, apertura de navegador, `UseAuthorization` sin autenticación, `UseHttpsRedirection` inoperante, cultura forzada a `en-US`. |
| **Configuración** (`appsettings.json`, `postgresql.conf`) | **Descartable** | Credenciales de superusuario en texto plano, versionadas, duplicadas en tres archivos, uno de ellos huérfano. |
| **Hoja de estilos `site.css`** | Reusable tal cual | Es un sistema de diseño real, coherente y bien construido. El mejor activo técnico del proyecto después de las reglas de negocio. |
| **`Views/Shared/_Layout.cshtml`** | Reusable con cambios menores | Buena estructura; hay que hacer configurables la marca y agregar navegación en móvil. |
| **`Views/Shared/_Layout.cshtml.css`** | **Descartable** | Plantilla por defecto de ASP.NET sin usar, con azules que contradicen la paleta. |
| **JavaScript embebido en vistas** (~1.200 líneas) | El concepto sirve pero hay que rehacerlo | Contiene reglas de negocio reales (totales, recargo, vuelto, exclusión de "Yo") duplicadas hasta 8 veces entre archivos. |
| **`wwwroot/js/site.js`** | **Descartable** | La mitad apunta a un endpoint inexistente; la otra mitad se duplica al cargarse dos veces en Proveedores. |
| **Vistas de Home y Error** | **Descartable** | Plantillas de Microsoft en inglés, sin tocar. |
| **`Helpers/TempDataExtensions`** | **Descartable** | Serializa conjuntos de resultados completos a cookies del navegador; el mecanismo entero hay que reemplazarlo. |
| **Tests** | — | No hay. |
| **Empaquetado y despliegue** | — | No hay. |

**Recuento:** 5 módulos reusables tal cual, 14 con cambios menores, 8 a rehacer conservando el concepto, 7 descartables.

---

### D3. Comparación con el otro sistema

**Sí hay indicios, y son concluyentes: este sistema no es un proyecto nuevo, es la reconversión de un trabajo práctico universitario del mismo autor.**

#### Evidencia

**1. El historial de git empieza en otro proyecto.** Los primeros 19 commits (2024-11-13 al 2024-12-20) construyen un sistema completamente distinto:

```
704ac90 2024-11-13  commit index producto
7c1e640 2024-11-15  commit falta controller y vista de presupuestos
9a67eef 2024-12-11  commit tp6 terminado
3189aa9 2024-12-12  Commit Controller cliente terminado, falta modificar las vistas
9993607 2024-12-20  Implementacion de modelo usuario
e259cae 2024-12-20  Implementacion de repositorio, interfaz, viewmodel de login y cosas en el program
6ebbf19 2024-12-20  Commit implementadas vista de login y controller de login
9e375d0 2024-12-20  TP8 TERMINADO
d32fbdf 2024-12-20  Commit try-catch y loggers agregados, TP TERMINADO
```
El primer commit de la pollería es **`0752aa7`, del 2025-07-15**: *"Commit productos y proveedores hechos, models armados"* — siete meses después.

**2. Las ramas conservan el nombre del origen académico:** `TP7`, `TP8`, `TP9` (la rama de trabajo actual), además de `main` y `Cursor_1`. "TP" es "trabajo práctico".

**3. El nombre del proyecto original sigue en el repositorio.** El proyecto se llamaba **`tl2-tp6-2024-Olme2`** ("Taller de Lenguajes 2, TP6, 2024"). Restos versionados hoy:
- `tl2-tp6-2024-Olme2.csproj.user` en la raíz (idéntico byte a byte a `entornoPolleria.csproj.user`)
- `bin/Debug/net9.0/tl2-tp6-2024-Olme2.dll`, `.exe`, `.pdb`, `.deps.json`, `.runtimeconfig.json`
- `.vs/ProjectEvaluation/tl2-tp6-2024-olme2.*` y `.vs/tl2-tp6-2024-Olme2/` completo

**4. Qué había en el proyecto anterior** (rama `TP8`): un sistema de **presupuestos** con `Clientes`, `Productos`, `Presupuestos`, `PresupuestosDetalle`, `Usuarios` y **login con roles**, sobre **SQLite** (`Tienda.db` versionado en esa rama).

**5. Qué se conservó estructuralmente.** El `diff TP8..TP9` muestra que se borró todo el dominio anterior y se creó el nuevo, pero **el andamiaje se heredó tal cual**: la organización `Controllers/` + `Repositorios/` con interfaces + `ViewModels/` + `Views/`, la convención de nombres (`AltaXViewModel`, `ModificarXViewModel`, `ListarXViewModel`, `IXRepository`), el patrón `CrearDesdeViewModel` / `ActualizarDesdeViewModel` en las entidades, el uso de `TempData["SuccessMessage"]` / `TempData["ErrorMessage"]`, y el par `Views/Home/Index.cshtml` + `Privacy.cshtml` + `Error.cshtml` **sin tocar desde la plantilla original**. Tres archivos figuran como `M` (modificados) y no como añadidos: `Controllers/HomeController.cs`, `Controllers/ProductosController.cs`, `Repositorios/ProductosRepository.cs`, `Models/Productos.cs`, `Program.cs` — es decir, **`Productos` es la única entidad que sobrevivió del proyecto anterior y fue transformada**.

**6. Lo que se perdió en la transición:** el login, los usuarios y los roles (`Models/Usuarios.cs` con `enum Rol { Admin, Cliente }`, `Controllers/LoginController.cs`, `Repositorios/UsuariosRepository.cs`, `ViewModels/LoginViewModel.cs`, `Views/Login/`), y la entidad `Clientes`. Ambas cosas están hoy en la lista de funcionalidad ausente (A5).

**7. Indicios de asistencia de IA en el desarrollo.** No es "otro sistema", pero es relevante para entender el código: hay una rama llamada **`Cursor_1`** con el commit *"Commit creacion de CRUD de ventas hecho por cursor"*, un commit en `TP9` titulado *"commit intermedio para gemini"* (`f9635f2`) y otro *"commit intermedio para que entienda el gemini"* (`445620e`). Esto explica varios patrones observados: comentarios del tipo `// --- INICIO DE LA CORRECCIÓN DEFINITIVA ---` y `// ¡Aquí está la magia!`, bloques marcados `// --- INICIO DE LA CORRECCIÓN ---` / `// --- FIN DE LA CORRECCIÓN ---` (`PromocionesController.cs:48-60, 143-146`), validaciones duplicadas dejadas una al lado de la otra (`PromocionesController.cs:54-84`), comentarios en inglés aislados (`// --- CRITICAL FIX: Ensure index realignment before submit ---`, `Compras/Alta.cshtml:97`), y funciones de ayuda generadas y nunca conectadas (`MapearAltaViewModelAEntidad`).

#### Sobre "el otro sistema del mismo autor"

El enunciado de la auditoría menciona **otro sistema que se está evaluando en paralelo**. **En este repositorio no hay ninguna referencia a él**: no hay proyectos compartidos, ni paquetes locales, ni submódulos, ni código con nombres de otro dominio, ni referencias a rutas de otro proyecto (más allá de las de `tl2-tp6-2024-Olme2`, que es el ancestro de este mismo). Si existe código compartido con ese otro sistema, **no es determinable desde este repositorio**; habría que comparar los dos árboles de archivos directamente.

Lo que sí se puede afirmar: **si el otro sistema también nació de un TP de la misma materia, es muy probable que comparta el mismo andamiaje** (organización de carpetas, convenciones de nombres `AltaX`/`ModificarX`/`ListarX`, patrón repositorio con interfaces, `TempData` para mensajes, plantilla `Home`/`Privacy`/`Error` sin tocar). Eso sería una buena noticia para un producto común: dos sistemas con la misma estructura se pueden unificar sobre una base compartida con mucho menos trabajo que dos sistemas ajenos.

---

### D4. Los tres hallazgos que más condicionan el futuro

---

#### 1. Las reglas de negocio más importantes están en la base de datos, y una de ellas ni siquiera está en el repositorio

**Qué es.** Todo el movimiento de inventario —descuento por venta, aumento por compra, descuento por combo, validación de stock insuficiente, prohibición de anular una compra ya consumida— vive en tres funciones PL/pgSQL de PostgreSQL (`db/polleria.sql:10-209`). La cuenta corriente de proveedores vive en dos triggers más, y **uno de ellos, `tg_actualizar_deuda_proveedor`, no existe en ningún archivo del repositorio**: sólo se lo menciona en un comentario que explica por qué hizo falta escribir otro trigger para cubrir el caso que aquél no atrapaba (`db/2026-05-10-compra-pagada-trigger.sql:4-10`).

**Por qué condiciona todo.** Cualquier plan que implique tocar la persistencia —sincronización con la nube, cambio de motor, multi-inquilino, offline-first, una API— tropieza inmediatamente con esto:
- La lógica **no se puede testear**: no hay forma de escribir una prueba unitaria de un trigger.
- La lógica **no es portable**: atada a PostgreSQL y a PL/pgSQL.
- La lógica **es invisible desde el código**: un desarrollador que lea todo el C# no encuentra en ninguna parte dónde se descuenta el stock. El código lo menciona sólo en comentarios (*"Los triggers de la BD se ejecutarán aquí"*, `VentaRepository.cs:116`).
- Y sobre todo: **hay una regla de negocio en producción cuya única copia existente es la base de datos del cliente.** Si esa base se pierde, o si se instala el sistema en otra máquina desde el repositorio, la funcionalidad de deuda con proveedores desaparece sin que nadie se dé cuenta hasta que un proveedor reclame.

**Consecuencia para el plan de producto:** el primer trabajo, antes de cualquier decisión de arquitectura, es **extraer el esquema completo y todos los triggers de la base de producción** (`pg_dump --schema-only`) y versionarlos. Recién con eso a la vista se puede evaluar cuánto cuesta subir esa lógica a C#. Mientras no exista esa extracción, **cualquier estimación sobre este sistema es una estimación sobre información incompleta**.

---

#### 2. Todas las claves primarias son enteros autoincrementales y no existe ninguna noción de comercio

**Qué es.** Las 7 entidades raíz usan `int`/`long`/`short` autoincremental generado por secuencias locales de PostgreSQL (B3); las 4 tablas de detalle usan claves compuestas construidas sobre esos enteros. **Ninguna de las 11 tablas tiene una columna que identifique a qué negocio pertenece el registro.** No hay filtros globales de consulta, no hay usuarios, no hay sesión, y la cadena de conexión apunta a una base fija llamada `polleria` en `localhost`.

**Por qué condiciona todo.** Este es el punto que decide, hoy, qué arquitecturas son posibles y cuáles no:
- **Dos instalaciones no pueden convivir en ninguna base común.** La pollería A y la pollería B tienen ambas `producto 1`, `venta 1`, `proveedor 1`. Consolidar, sincronizar o migrar exige reasignar identificadores en cascada por todas las tablas, incluidas las cuatro claves primarias compuestas.
- **`UseIdentityAlwaysColumn()` en `compra` y `gasto`** (`AppDbContext.cs:62, 196`) **prohíbe insertar identificadores explícitos** sin `OVERRIDING SYSTEM VALUE`: cualquier importación de datos con ids preexistentes falla en esas dos tablas.
- **Sin columna de propietario y sin usuarios, no hay ningún camino corto a multi-inquilino.** No es que falte configurar un filtro: falta la columna, en las 11 tablas, y falta el concepto de quién está usando el sistema.
- La aplicación además está **físicamente atada a una máquina**: `app.Run("http://localhost:5146")` sólo escucha en loopback y el proceso abre un navegador en el escritorio del servidor (`Program.cs:82-89`).

**La decisión que fuerza:** o se acepta el modelo "una instalación aislada por cliente, sin datos compartidos jamás" —que es lo que el sistema es hoy y limita el producto a venta e instalación manual, sin nube, sin respaldo centralizado, sin soporte remoto—, o **se cambia el tipo de todas las claves primarias antes de que existan datos de más de un cliente**. Cuanto más tarde se tome esa decisión, más caro es: cada instalación nueva agrega un conjunto de identificadores que después hay que reconciliar.

---

#### 3. No hay tests, no hay migraciones, no hay documentación y no hay artefacto instalable: cada cliente nuevo es trabajo manual del autor

**Qué es.** Cuatro ausencias que se refuerzan entre sí:
- **Cero tests** (B12). Ninguna de las 28 reglas de negocio tiene una prueba.
- **Cero migraciones** (B2). El esquema se administra a mano; el único archivo de esquema del repositorio está en formato equivocado, desactualizado, y contiene una columna (`compra.total NOT NULL`) que rompería el alta de compras.
- **Cero documentación**. No hay un `README`. No hay ninguna nota sobre cómo instalar, qué datos semilla hacen falta, ni que tienen que existir métodos de pago llamados exactamente `"Yo"` y `"Efectivo"` para que dos reglas de negocio funcionen.
- **Cero empaquetado** (B11). No hay publicación, instalador, contenedor ni script. No hay número de versión (`1.0.0.0` por defecto, no visible en ninguna pantalla) ni mecanismo de actualización.

**Por qué condiciona todo.** Determina el **costo marginal de cada cliente**, que es el número que decide si esto es un producto o un servicio:
- **Instalar** exige que el autor esté presente y recuerde de memoria el esquema real y los triggers (obstáculo 1). No es delegable.
- **Actualizar** exige compilar, copiar archivos a mano y aplicar cambios de base de datos a mano, cliente por cliente, sin saber qué versión tiene cada uno.
- **Cambiar cualquier cosa** se hace a ciegas: sin tests, tocar el cálculo de un total o la lógica de stock no tiene red. Y con lógica crítica en triggers, un error no se descubre hasta que un cliente reporta que el stock quedó mal.
- **Diagnosticar un problema remoto es imposible**: no hay log a archivo, no hay usuarios, no hay auditoría, no hay número de versión visible, y nueve de los buscadores fallan sin dejar rastro en pantalla (C3).

Y hay un multiplicador: **el 60% de los fallos posibles hoy son silenciosos**. El usuario final no es técnico; si el autocompletado devuelve un desplegable vacío porque el servidor tiró una excepción, el cliente concluye "no está el producto", carga otra cosa, y el problema nunca llega a reportarse.

**Consecuencia:** en el estado actual, el modelo comercial viable no es "vender un producto" sino "instalar y mantener personalmente cada copia". Escalar a más de dos o tres clientes exige, antes que cualquier feature nueva, **convertir el proyecto en algo instalable y verificable**: migraciones de EF Core que generen el esquema completo (triggers incluidos, una vez recuperados), un conjunto de tests sobre las 28 reglas de A3 que sirva de red para cualquier reescritura, un artefacto de despliegue reproducible, y logging persistente con versión visible.

---

## Cierre — Preguntas que quedaron sin responder

### Lo que no se puede determinar leyendo el repositorio

**Sobre la base de datos (requiere acceso a la base de producción)**

1. **¿Cuál es el esquema real?** `db/polleria.sql` está desactualizado y contradice al modelo de EF en al menos 11 puntos (B2). No se sabe si `compra.total` todavía existe, si las restricciones `CHECK` marcadas `NOT VALID` siguen activas, ni qué tipos tienen realmente las columnas.
   → *Cómo responderlo:* `pg_dump --schema-only --no-owner polleria > esquema-real.sql`.

2. **¿Qué dice exactamente el trigger `tg_actualizar_deuda_proveedor`?** Es una regla de negocio central (A3 regla 8) y no está escrita en ninguna parte.
   → *Cómo responderlo:* la misma extracción del punto anterior; el volcado de esquema incluye funciones y triggers.

3. **¿Existen otros triggers, funciones, vistas o índices no versionados?** El patrón observado sugiere que sí. En particular, ¿existe el trigger que gestiona `producto.costo_pendiente` / `stock_umbral` que menciona el comentario de `Models/Productos.cs:12`?

4. **¿Cómo se resolvió la contradicción entre `CHECK (precio_unitario >= 0.01)` en `detalle_venta` y el consumo interno, que se guarda con precio 0?** (A3 regla 7). O la restricción se eliminó, o el consumo nunca funcionó, o hay algo que no se ve desde el código.

5. **¿Cuál es el volumen real de datos?** El volcado de enero muestra las secuencias en: 88 productos, 184 ventas, 8 métodos de pago, 7 promociones, 4 proveedores. Con seis meses más de operación, hace falta el número actual para evaluar el impacto de la ausencia de índices y de las consultas que materializan todo en memoria (B7).

**Sobre el comportamiento en ejecución (requiere correr la aplicación)**

6. **¿La columna "Total" del listado de ventas muestra el total correcto, o sólo el redondeo?** (D1.1). Es el primer punto a verificar y afecta a la pantalla más usada del sistema.
   → *Cómo responderlo:* abrir `/Ventas` con datos y comparar contra la suma manual de una venta conocida.

7. **¿Funciona hoy la modificación de compras?** El parcial `_ModificarDetalleCompraItem.cshtml` emite `name="IdProducto"` sin índice de colección y depende enteramente del reindexado en JavaScript (C3, caso 15).

8. **¿A partir de cuántas ventas deja de persistir el filtro?** El `IndexVentasVM` completo, con la lista de resultados adentro, se serializa a una cookie (B4, punto 6). Hay que medir dónde está el corte real.

9. **¿Qué pasa exactamente cuando falla un trigger?** Qué mensaje ve el usuario, palabra por palabra, en cada uno de los nueve puntos donde se muestra `ex.Message` crudo (C4).

10. **¿Cuánto tarda el autocompletado con el volumen real?** Se ejecuta la consulta completa de productos, con subconsultas por producto, en cada pulsación de tecla (B7).

**Sobre el negocio (requiere hablar con el cliente original)**

11. **¿Los turnos 08:00–14:00 y 17:30–22:00 son los reales y son estables?** Están hardcodeados en dos lugares (A3 regla 18).

12. **¿Qué significa realmente el módulo de "Producción"?** El código registra sólo la salida de materia prima y nunca el ingreso del producto elaborado (A3 regla 5, A5 punto 1). ¿Es una decisión consciente —"lo que produzco lo vendo el mismo día y no lo quiero contar dos veces"— o una funcionalidad que quedó a medio hacer?

13. **¿Cómo se registra hoy la mercadería que se pierde o se descarta?** El sistema no tiene forma de hacerlo (A5 punto 2). ¿Se usa el consumo interno para eso? ¿Se ajusta el stock a mano? La respuesta cambia qué significan los números del dashboard.

14. **¿Qué unidad tiene cada producto?** El sistema no lo modela (A3 regla 1). Sólo el dueño sabe cuáles se venden por kilo y cuáles por unidad, y esa información no está en ninguna parte del sistema.

15. **¿Se cobra recargo por tarjeta?** Si la respuesta es sí, el sistema **lleva registrando ventas por debajo de lo cobrado** desde que existe el campo (A1 funcionalidad 1), y eso afecta a todos los números históricos del dashboard.

16. **¿Se detectó alguna vez que modificar una venta vieja le cambia los precios a los de hoy?** (A3 regla 16). Determina cuánta contaminación histórica hay en los datos.

17. **¿Qué se hace hoy para el respaldo de la base?** No hay nada en el sistema.

18. **¿Existe factura o ticket para el cliente, hecho por fuera del sistema?** No hay huella fiscal alguna (B9). Saber cómo se resuelve hoy define si la capa fiscal es un requisito de la versión 1 del producto o puede esperar.

### Método y limitaciones declarados

- Se leyó el **100% del código propio** del repositorio en la rama `TP9`. No se leyeron las librerías de terceros de `wwwroot/lib/`.
- **No se ejecutó la aplicación, no se compiló y no se abrió ninguna base de datos.** Las afirmaciones sobre compilación en D1.1 se basan en artefactos de compilación versionados y en su fecha, no en una compilación propia.
- **No se modificó ningún archivo del repositorio** salvo la creación de este informe. No se hizo commit.
- Las afirmaciones sobre el contenido de la base de datos en producción se basan **exclusivamente** en `db/polleria.sql` (desactualizado y en formato *custom* de `pg_dump`) y en `db/2026-05-10-compra-pagada-trigger.sql`. Donde el estado real no es deducible, está dicho explícitamente.
- Los porcentajes de B5 son una estimación razonada sobre las 28 reglas identificadas, no una medición.

