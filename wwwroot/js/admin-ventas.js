// wwwroot/js/admin-ventas.js

// Instancia global del modal de Bootstrap
const modalVentaElement = document.getElementById('modalVenta');
const modalVenta = modalVentaElement ? new bootstrap.Modal(modalVentaElement) : null;

// Arrays globales para almacenar lo que viene de la BD y poder buscar por texto
let listaClientesMemoria = [];
let listaVehiculosMemoria = [];

// ==========================================
// 1. LISTAR VENTAS EN LA TABLA PRINCIPAL
// ==========================================
async function listarVentas() {
    const tbody = document.getElementById('tbodyVentas');
    if (!tbody) return;

    try {
        const resp = await fetch('/Admin/GetVentas', {
            headers: { 'Authorization': `Bearer ${token}` }
        });

        if (!resp.ok) throw new Error("No se pudieron recuperar las ventas.");

        const ventas = await resp.json();
        tbody.innerHTML = '';

        if (ventas.length === 0) {
            tbody.innerHTML = `
                <tr>
                    <td colspan="7" class="text-center py-4 text-muted small">No hay registros de ventas.</td>
                </tr>`;
            return;
        }

        ventas.forEach(v => {
            const fecha = v.fechaVenta ? new Date(v.fechaVenta).toLocaleDateString('es-AR') : 'N/A';
            const monto = new Intl.NumberFormat('es-AR', { style: 'currency', currency: 'ARS' }).format(v.montoFinal);

            tbody.innerHTML += `
                <tr>
                    <td style="color: #ffffff !important; font-weight: 600;">#V-${v.id}</td>
                    <td style="color: #ffffff !important;">${v.nombreCliente || 'N/A'}</td>
                    <td style="color: #ced4da !important;">${fecha}</td>
                    <td style="color: #ffffff !important; font-size: 0.9rem;">${v.detalleVehiculo || 'Vehículo asignado'}</td>
                    <td style="color: #ced4da !important;"><i class="bi bi-credit-card me-1 text-info"></i> ${v.formaPago}</td>
                    <td style="color: #ffffff !important; font-weight: bold;">${monto}</td>
                    <td class="text-end pe-4">
                        <button class="btn btn-sm btn-outline-info" onclick="verDetalleVentaCompletado(${JSON.stringify(v).replace(/"/g, '&quot;')})" title="Ver Detalles">
                            <i class="bi bi-eye"></i>
                        </button>
                    </td>
                </tr>`;
        });

    } catch (error) {
        console.error(error);
        tbody.innerHTML = `
            <tr>
                <td colspan="7" class="text-center py-4 text-danger small">Error al cargar el historial de ventas.</td>
            </tr>`;
    }
}

// ==========================================
// 2. CARGAR COMPONENTES PARA LA NUEVA VENTA
// ==========================================
async function abrirModalVenta(id = 0) {
    limpiarFormularioVenta();

    // Seteamos fecha y hora actual en el formulario
    const ahora = new Date();
    ahora.setMinutes(ahora.getMinutes() - ahora.getTimezoneOffset());
    document.getElementById('vFecha').value = ahora.toISOString().slice(0, 16);

    if (id === 0) {
        document.getElementById('modalVentaTitulo').innerText = "Registrar Nueva Venta";
        
        // Habilitar controles que se bloquean al "Ver Detalle"
        opacidadCamposVenta(false);

        // Cargamos los selectores/datalists de Clientes y Vehículos en paralelo
        await Promise.all([cargarDatalistClientes(), cargarDatalistVehiculos()]);

        modalVenta?.show();
    }
}

// Carga los clientes en el datalist para buscar por Nombre/DNI
async function cargarDatalistClientes() {
    try {
        const resp = await fetch('/Admin/GetClientes', { headers: { 'Authorization': `Bearer ${token}` } });
        if (!resp.ok) return;
        
        listaClientesMemoria = await resp.json();
        const dl = document.getElementById('datalistClientesVenta');
        if (!dl) return;

        dl.innerHTML = '';
        listaClientesMemoria.forEach(c => {
            const nombreCompleto = `${c.persona?.nombres} ${c.persona?.apellidos}`;
            dl.innerHTML += `<option value="${nombreCompleto} (DNI: ${c.persona?.documentoIdentidad})" data-id="${c.id}"></option>`;
        });

        // Evento para capturar el ID real cuando seleccionan el texto
        document.getElementById('vClienteBusqueda').oninput = function() {
            const val = this.value;
            const option = Array.from(dl.options).find(o => o.value === val);
            document.getElementById('vIdCliente').value = option ? option.getAttribute('data-id') : "";
        };
    } catch (e) { console.error("Error cargando clientes para venta:", e); }
}

// Carga los vehículos disponibles en el datalist para vender
async function cargarDatalistVehiculos() {
    try {
        const resp = await fetch('/Admin/GetVehiculos', { headers: { 'Authorization': `Bearer ${token}` } });
        if (!resp.ok) return;

        const vehiculos = await resp.json();
        // Filtramos para ofrecer únicamente los que están "Disponibles"
        listaVehiculosMemoria = vehiculos.filter(v => v.estado?.toLowerCase() === "disponible" || v.estado?.toLowerCase() == "disponibles");
        
        const dl = document.getElementById('datalistProductosVenta');
        if (!dl) return;

        dl.innerHTML = '';
        listaVehiculosMemoria.forEach(v => {
            dl.innerHTML += `<option value="${v.marca} ${v.modelo} - Patente: ${v.patente}" data-id="${v.id}" data-precio="${v.precio}"></option>`;
        });

        // Evento para capturar ID y setear automáticamente el Precio Unitario
        document.getElementById('vProductoBusqueda').oninput = function() {
            const val = this.value;
            const option = Array.from(dl.options).find(o => o.value === val);
            if (option) {
                document.getElementById('vIdVehiculo').value = option.getAttribute('data-id');
                const precio = parseFloat(option.getAttribute('data-precio')) || 0;
                document.getElementById('vPrecioUnitario').value = precio;
                
                // Mapear al flujo de subtotal/totales de tu modal viejo
                asignarItemFilaUnica(val, precio);
            } else {
                document.getElementById('vIdVehiculo').value = "";
                document.getElementById('vPrecioUnitario').value = "";
            }
        };
    } catch (e) { console.error("Error cargando vehículos para venta:", e); }
}

// Inserta el auto seleccionado en la tabla de vista previa del modal
function asignarItemFilaUnica(descripcion, precio) {
    const tbody = document.getElementById('tbodyDetalleVenta');
    if (!tbody) return;

    const totalFormateado = new Intl.NumberFormat('es-AR', { style: 'currency', currency: 'ARS' }).format(precio);

    tbody.innerHTML = `
        <tr>
            <td class="text-white">${descripcion}</td>
            <td class="text-center text-white">1</td>
            <td class="text-end text-white">${totalFormateado}</td>
            <td class="text-end text-white fw-bold">${totalFormateado}</td>
            <td class="text-center">
                <button type="button" class="btn btn-sm btn-outline-danger" onclick="limpiarSeleccionVehiculo()"><i class="bi bi-x-lg"></i></button>
            </td>
        </tr>`;

    document.getElementById('lblSubtotal').innerText = totalFormateado;
    document.getElementById('lblTotalFinal').innerText = totalFormateado;
    document.getElementById('vMontoFinalCalculado').value = precio;
}

function limpiarSeleccionVehiculo() {
    document.getElementById('vProductoBusqueda').value = "";
    document.getElementById('vIdVehiculo').value = "";
    document.getElementById('vPrecioUnitario').value = "";
    
    const tbody = document.getElementById('tbodyDetalleVenta');
    if (tbody) {
        tbody.innerHTML = `
            <tr id="rowDetalleVacio">
                <td colspan="5" class="text-center py-4 text-muted small">No hay ítems cargados en la venta actual.</td>
            </tr>`;
    }
    document.getElementById('lblSubtotal').innerText = "$0,00";
    document.getElementById('lblTotalFinal').innerText = "$0,00";
    document.getElementById('vMontoFinalCalculado').value = "0";
}

// ==========================================
// 3. GUARDAR LA VENTA (POST)
// ==========================================
async function guardarVenta() {
    const idCliente = document.getElementById('vIdCliente').value;
    const idVehiculo = document.getElementById('vIdVehiculo').value;

    if (!idCliente || !idVehiculo) {
        alert("Por favor seleccione un Cliente y un Vehículo válidos de la lista desplegable.");
        return;
    }

    // Estructura exacta que espera tu Base de Datos e Inyección SQL en C#
    const ventaData = {
        id: parseInt(document.getElementById('vId').value) || 0,
        vehiculoId: parseInt(idVehiculo),
        clienteId: parseInt(idCliente),
        vendedorId: 0, // Se encarga el backend de leerlo del JWT por seguridad
        fechaVenta: document.getElementById('vFecha').value,
        montoFinal: parseFloat(document.getElementById('vMontoFinalCalculado').value) || 0,
        formaPago: document.getElementById('vMedioPago').value,
        observaciones: document.getElementById('vObservaciones')?.value || ""
    };

    try {
        const resp = await fetch('/Admin/GuardarVenta', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'Authorization': `Bearer ${token}`
            },
            body: JSON.stringify(ventaData)
        });

        const data = await resp.json();

        if (!resp.ok) {
            alert(data.mensaje || "Error al procesar la operación.");
            return;
        }

        modalVenta?.hide();
        limpiarFormularioVenta();
        
        // Recargamos el listado principal
        await listarVentas();
        
        // Si tenés el script de vehículos activo, refrescá el stock para que desaparezca el coche vendido
        if (typeof listar === 'function') listar();

        alert("¡Venta registrada de forma exitosa!");

    } catch (error) {
        console.error(error);
        alert("Ocurrió un error crítico de red al guardar.");
    }
}

// ==========================================
// 4. AUXILIARES Y LIMPIEZA
// ==========================================
function limpiarFormularioVenta() {
    const form = document.getElementById('formVenta');
    if (form) form.reset();

    document.getElementById('vId').value = "0";
    document.getElementById('vIdCliente').value = "";
    document.getElementById('vIdVehiculo').value = "";
    
    // Inputs ocultos auxiliares creados para guardar el control
    if (!document.getElementById('vIdVehiculo')) {
        const hiddenFields = `
            <input type="hidden" id="vIdVehiculo" value="">
            <input type="hidden" id="vMontoFinalCalculado" value="0">`;
        document.getElementById('formVenta').insertAdjacentHTML('beforeend', hiddenFields);
    } else {
        document.getElementById('vMontoFinalCalculado').value = "0";
    }

    limpiarSeleccionVehiculo();
}

function verDetalleVentaCompletado(ventaObj) {
    abrirModalVenta(-1); // Bloquea flujos por ID ficticio
    
    document.getElementById('modalVentaTitulo').innerText = `Consulta de Venta #V-${ventaObj.id}`;
    document.getElementById('vClienteBusqueda').value = ventaObj.nombreCliente || '';
    document.getElementById('vMedioPago').value = ventaObj.formaPago;
    document.getElementById('vFecha').value = ventaObj.fechaVenta ? ventaObj.fechaVenta.slice(0, 16) : '';
    document.getElementById('vObservaciones').value = ventaObj.observaciones || '';
    
    asignarItemFilaUnica(ventaObj.detalleVehiculo || 'Detalle del rodado', ventaObj.montoFinal);
    opacidadCamposVenta(true);
    
    modalVenta?.show();
}

function opacidadCamposVenta(bloquear) {
    document.getElementById('vClienteBusqueda').disabled = bloquear;
    document.getElementById('vMedioPago').disabled = bloquear;
    document.getElementById('vFecha').disabled = bloquear;
    document.getElementById('vProductoBusqueda').disabled = bloquear;
    document.getElementById('vObservaciones').disabled = bloquear;
    
    const btnSubmit = document.getElementById('btnGuardarVenta');
    if (btnSubmit) btnSubmit.style.display = bloquear ? 'none' : 'block';
}