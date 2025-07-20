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
});