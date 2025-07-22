// Este código se ejecutará cuando el documento HTML esté completamente cargado.
$(document).ready(function () {

    // Función para actualizar la tabla de proveedores
    function actualizarTablaProveedores() {
        var busqueda = $('#buscador-proveedor').val();
        var filtroDeuda = $('#filtro-deuda').val();

        // Hacemos una petición AJAX al controlador
        $.ajax({
            url: '/Proveedores/_BuscarProveedores', // La URL de nuestra nueva acción
            type: 'GET',
            data: {
                busqueda: busqueda,
                filtroDeuda: filtroDeuda
            },
            success: function (result) {
                // Si la petición es exitosa, reemplazamos el contenido del <tbody>
                $('#tabla-proveedores-body').html(result);
            },
            error: function (err) {
                // Opcional: manejar errores, por ejemplo, mostrando un mensaje
                console.error("Error al buscar proveedores:", err);
            }
        });
    }

    // Vinculamos la función al evento 'keyup' del buscador
    // Esto significa que se ejecutará cada vez que el usuario suelte una tecla.
    $('#buscador-proveedor').on('keyup', function () {
        actualizarTablaProveedores();
    });

    // También vinculamos la función al evento 'change' del filtro de deuda
    $('#filtro-deuda').on('change', function () {
        actualizarTablaProveedores();
    });
    // --- LÓGICA NUEVA PARA PRODUCTOS ---

    function actualizarTablaProductos() {
        var busqueda = $('#buscador-producto').val();
        var ordenarPor = $('#ordenar-por').val();
        var inactivos = $('#mostrar-inactivos').is(':checked');
        $.ajax({
            url: '/Productos/_BuscarProductos', // URL de la nueva acción
            type: 'GET',
            data: {
                busqueda: busqueda,
                ordenarPor: ordenarPor,
                inactivos: inactivos
            },
            success: function (result) {
                $('#tabla-productos-body').html(result);
            },
            error: function (err) {
                console.error("Error al buscar productos:", err);
                // Opcional: Mostrar un error al usuario
            }
        });
    }

    // Eventos que disparan la actualización
    $('#buscador-producto').on('keyup', actualizarTablaProductos);
    $('#ordenar-por').on('change', actualizarTablaProductos);
    $('#mostrar-inactivos').on('change', actualizarTablaProductos);
});

