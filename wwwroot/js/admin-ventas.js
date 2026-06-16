// Referencia global de la instancia del modal de Bootstrap
let modalVenta = null;

// Temporizadores para el control de Debounce (Búsqueda dinámica)
let timeoutClientes = null;
let timeoutVehiculos = null;

// Variable segura para el token (Busca en localStorage igual que tu Layout)
const getAuthToken = () => {
    if (typeof window.token !== 'undefined' && window.token) return window.token;
    return localStorage.getItem('jonel_token') || '';
};

// 🚀 EXPOSICIÓN GLOBAL EXPLICÍTA DE LAS FUNCIONES CRÍTICAS
window.inicializarModalVentaDeFormaSegura = function() {
    try {
        const modalVentaElement = document.getElementById('modalVenta');
        if (modalVentaElement && typeof bootstrap !== 'undefined') {
            // Evitamos duplicar instancias si ya existía
            if (!modalVenta) {
                modalVenta = new bootstrap.Modal(modalVentaElement);
            }
        }
    } catch (e) {
        console.error("Error al inicializar el componente modal de ventas:", e);
    }
}

// ==========================================
// 1. LISTAR VENTAS EN LA TABLA PRINCIPAL
// ==========================================
window.listarVentas = async function() {
    console.log("🚀 Ejecutando listarVentas() de forma global.");
    const tbody = document.getElementById('tbodyVentas');
    
    if (!tbody) {
        console.error("ERROR CRÍTICO: No se encontró un elemento con el ID 'tbodyVentas' en el HTML.");
        return;
    }

    try {
        const tokenVal = getAuthToken();
        if (!tokenVal) {
            console.warn("Advertencia: El token JWT está vacío.");
        }

        const resp = await fetch('/Admin/GetVentas', {
            headers: { 'Authorization': `Bearer ${tokenVal}` }
        });

        if (!resp.ok) {
            throw new Error(`El servidor respondió con código de estado ${resp.status} (${resp.statusText})`);
        }

        const ventas = await resp.json();
        tbody.innerHTML = ''; 

        if (!ventas || ventas.length === 0) {
            tbody.innerHTML = `
                <tr>
                    <td colspan="7" class="text-center py-4 text-muted small">No hay registros de ventas.</td>
                </tr>`;
            return;
        }

        ventas.forEach(v => {
            const fecha = v.fechaVenta ? new Date(v.fechaVenta).toLocaleDateString('es-AR') : 'N/A';
            const monto = new Intl.NumberFormat('es-AR', { style: 'currency', currency: 'ARS' }).format(v.montoFinal || 0);
            
            // Corrección preventiva: Soporta tanto 'detalles' como 'detallesVenta'
            const listaDetalles = v.detalles || v.detallesVenta || [];
            let descripcionItems = 'Vehículo asignado';
            if (listaDetalles.length > 0) {
                descripcionItems = listaDetalles.map(d => d.descripcionItem || 'Unidad comercial').join(' | ');
            }

            const comprobanteText = v.tipoComprobante ? `<span class="badge bg-secondary me-1">${v.tipoComprobante}</span>` : '';
            const formaPagoText = v.formaPago || 'N/A';

            const objetoVentaEscapado = JSON.stringify(v).replace(/"/g, '&quot;');

            tbody.innerHTML += `
                <tr>
                    <td style="color: #ffffff !important; font-weight: 600;">#C-${v.nroComprobante || v.id}</td>
                    <td style="color: #ffffff !important;">${v.nombreCliente || 'N/A'}</td>
                    <td style="color: #ced4da !important;">${fecha}</td>
                    <td style="color: #ffffff !important; font-size: 0.9rem;">${descripcionItems}</td>
                    <td style="color: #ced4da !important;">
                        ${comprobanteText}
                        <i class="bi bi-credit-card me-1 text-info"></i> ${formaPagoText}
                    </td>
                    <td style="color: #ffffff !important; font-weight: bold;">${monto}</td>
                    <td class="text-end pe-4">
                        <button class="btn btn-sm btn-outline-info" onclick="verDetalleVentaCompletado(${objetoVentaEscapado})" title="Ver Detalles">
                            <i class="bi bi-eye"></i>
                        </button>
                    </td>
                </tr>`;
        });

    } catch (error) {
        console.error("Error capturado en listarVentas:", error);
        tbody.innerHTML = `
            <tr>
                <td colspan="7" class="text-center py-4 text-danger small">
                    <i class="bi bi-exclamation-triangle me-1"></i> <strong>Error al cargar:</strong> ${error.message}
                </td>
            </tr>`;
    }
}

// ==========================================
// 2. BUSCADORES DINÁMICOS BAJO DEMANDA
// ==========================================
window.abrirModalVenta = async function(id = 0) {
    try {
        limpiarFormularioVenta();

        const ahora = new Date();
        ahora.setMinutes(ahora.getMinutes() - ahora.getTimezoneOffset());
        const inputFecha = document.getElementById('vFecha');
        if (inputFecha) inputFecha.value = ahora.toISOString().slice(0, 16);

        // Si es una nueva venta (id === 0), inicializamos los escuchadores de los datalists
        if (id === 0) {
            const titulo = document.getElementById('modalVentaTitulo');
            if (titulo) titulo.innerText = "Registrar Nueva Venta";

            opacidadCamposVenta(false);

            // Configuramos los inputs para responder a la escritura de forma dinámica
            cargarDatalistClientes();
            cargarDatalistVehiculos();
        }

        // Mostrar el modal de forma segura sin importar si es nueva venta o consulta (id === -1)
        const modalVentaElement = document.getElementById('modalVenta');
        if (modalVentaElement) {
            if (typeof bootstrap !== 'undefined') {
                if (!modalVenta) modalVenta = new bootstrap.Modal(modalVentaElement);
                modalVenta.show();
            } else {
                modalVentaElement.classList.add('show');
                modalVentaElement.style.display = 'block';
                document.body.classList.add('modal-open');
            }
        } else {
            alert("Error: El elemento HTML '#modalVenta' no existe en esta página.");
        }
    } catch (err) {
        console.error("Error crítico al abrir modal de venta:", err);
    }
}

function cargarDatalistClientes() {
    const inputBusqueda = document.getElementById('vClienteBusqueda');
    const dl = document.getElementById('datalistClientesVenta');
    if (!inputBusqueda || !dl) return;

    // Reseteamos el evento oninput anterior si existía
    inputBusqueda.oninput = null;

    inputBusqueda.addEventListener('input', function () {
        const query = this.value.trim();
        const inputIdCliente = document.getElementById('vIdCliente');

        clearTimeout(timeoutClientes);

        // Si se vacía el input de búsqueda, limpiamos el ID del cliente seleccionado
        if (query.length === 0 && inputIdCliente) {
            inputIdCliente.value = "";
            return;
        }

        // 🛡️ Filtro de seguridad: Solo busca en BD si escribió 3 o más caracteres
        if (query.length < 3) return;

        // Si coincide exactamente con una opción existente, mapeamos su data-id directamente
        const optionExistente = Array.from(dl.options).find(o => o.value === query);
        if (optionExistente) {
            if (inputIdCliente) inputIdCliente.value = optionExistente.getAttribute('data-id');
            return;
        }

        // Debounce de 300ms antes de disparar el Fetch a tu Backend
        timeoutClientes = setTimeout(async () => {
            try {
                const resp = await fetch(`/Admin/GetClientes?search=${encodeURIComponent(query)}`, { 
                    headers: { 'Authorization': `Bearer ${getAuthToken()}` } 
                });
                
                if (!resp.ok) return;
                const clientesFiltrados = await resp.json();

                dl.innerHTML = '';
                clientesFiltrados.forEach(c => {
                    const nombreCompleto = `${c.persona?.nombres || ''} ${c.persona?.apellidos || ''}`.trim();
                    dl.innerHTML += `<option value="${nombreCompleto} (DNI: ${c.persona?.documentoIdentidad || 'S/D'})" data-id="${c.id}"></option>`;
                });

                // Si tras renderizar coincide el texto, asignamos el ID inmediatamente
                const opcionMatch = Array.from(dl.options).find(o => o.value === inputBusqueda.value);
                if (opcionMatch && inputIdCliente) {
                    inputIdCliente.value = opcionMatch.getAttribute('data-id');
                }
            } catch (e) {
                console.error("Error en búsqueda dinámica de clientes:", e);
            }
        }, 300);
    });
}

function cargarDatalistVehiculos() {
    const inputProdBusqueda = document.getElementById('vProductoBusqueda');
    const dl = document.getElementById('datalistProductosVenta');
    if (!inputProdBusqueda || !dl) return;

    inputProdBusqueda.oninput = null;

    inputProdBusqueda.addEventListener('input', function () {
        const query = this.value.trim();
        const inputIdVehiculo = document.getElementById('vIdVehiculo');
        const inputPrecioUnitario = document.getElementById('vPrecioUnitario');

        clearTimeout(timeoutVehiculos);

        if (query.length === 0) {
            if (inputIdVehiculo) inputIdVehiculo.value = "";
            if (inputPrecioUnitario) inputPrecioUnitario.value = "0";
            return;
        }

        // Para vehículos (marcas, patentes) abrimos la búsqueda desde los 2 caracteres
        if (query.length < 2) return;

        const optionExistente = Array.from(dl.options).find(o => o.value === query);
        if (optionExistente) {
            if (inputIdVehiculo) inputIdVehiculo.value = optionExistente.getAttribute('data-id');
            const precio = parseFloat(optionExistente.getAttribute('data-precio')) || 0;
            if (inputPrecioUnitario) inputPrecioUnitario.value = precio;
            asignarItemFilaUnica(query, precio);
            return;
        }

        timeoutVehiculos = setTimeout(async () => {
            try {
                const resp = await fetch(`/Admin/GetVehiculos?search=${encodeURIComponent(query)}`, { 
                    headers: { 'Authorization': `Bearer ${getAuthToken()}` } 
                });
                
                if (!resp.ok) return;
                const dataVehiculos = await resp.json();

                // Filtrado dinámico por estado en caliente antes de renderizar
                const vehiculosDisponibles = dataVehiculos.filter(v => 
                    v.estado?.toLowerCase() === "disponible" || v.estado?.toLowerCase() === "disponibles"
                );

                dl.innerHTML = '';
                vehiculosDisponibles.forEach(v => {
                    dl.innerHTML += `<option value="${v.marca} ${v.modelo} - Patente: ${v.patente || 'S/P'}" data-id="${v.id}" data-precio="${v.precio}"></option>`;
                });

                const opcionMatch = Array.from(dl.options).find(o => o.value === inputProdBusqueda.value);
                if (opcionMatch) {
                    if (inputIdVehiculo) inputIdVehiculo.value = opcionMatch.getAttribute('data-id');
                    const precio = parseFloat(opcionMatch.getAttribute('data-precio')) || 0;
                    if (inputPrecioUnitario) inputPrecioUnitario.value = precio;
                    asignarItemFilaUnica(inputProdBusqueda.value, precio);
                }
            } catch (e) {
                console.error("Error en búsqueda dinámica de vehículos:", e);
            }
        }, 350);
    });
}

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
            <td class="text-center visual-control">
                <button type="button" class="btn btn-sm btn-outline-danger" onclick="limpiarSeleccionVehiculo()"><i class="bi bi-x-lg"></i></button>
            </td>
        </tr>`;

    if (document.getElementById('lblSubtotal')) document.getElementById('lblSubtotal').innerText = totalFormateado;
    if (document.getElementById('lblTotalFinal')) document.getElementById('lblTotalFinal').innerText = totalFormateado;
    if (document.getElementById('vMontoFinalCalculado')) document.getElementById('vMontoFinalCalculado').value = precio;
}

window.limpiarSeleccionVehiculo = function() {
    if (document.getElementById('vProductoBusqueda')) document.getElementById('vProductoBusqueda').value = "";
    if (document.getElementById('vIdVehiculo')) document.getElementById('vIdVehiculo').value = "";
    if (document.getElementById('vPrecioUnitario')) document.getElementById('vPrecioUnitario').value = "0";

    const tbody = document.getElementById('tbodyDetalleVenta');
    if (tbody) {
        tbody.innerHTML = `
            <tr id="rowDetalleVacio">
                <td colspan="5" class="text-center py-4 text-muted small">No hay ítems cargados en la venta actual.</td>
            </tr>`;
    }
    if (document.getElementById('lblSubtotal')) document.getElementById('lblSubtotal').innerText = "$0,00";
    if (document.getElementById('lblTotalFinal')) document.getElementById('lblTotalFinal').innerText = "$0,00";
    if (document.getElementById('vMontoFinalCalculado')) document.getElementById('vMontoFinalCalculado').value = "0";
}

// ==========================================
// 3. GUARDAR LA VENTA (POST)
// ==========================================
window.guardarVenta = async function() {
    const idCliente = document.getElementById('vIdCliente')?.value;
    const idVehiculo = document.getElementById('vIdVehiculo')?.value;
    const precioUnitario = parseFloat(document.getElementById('vPrecioUnitario')?.value) || 0;

    if (!idCliente || !idVehiculo) {
        alert("Por favor seleccione un Cliente y un Vehículo válidos de la lista desplegable.");
        return;
    }

    const ventaData = {
        id: parseInt(document.getElementById('vId')?.value) || 0,
        clienteId: parseInt(idCliente),
        vendedorId: 0,
        tipoComprobanteId: parseInt(document.getElementById('vTipoComprobante')?.value) || 1,
        formaPagoId: parseInt(document.getElementById('vMedioPago')?.value) || 1,
        puntoVenta: parseInt(document.getElementById('vPuntoVenta')?.value) || 1,
        nroComprobante: parseInt(document.getElementById('vNroComprobante')?.value) || 0,
        fechaVenta: document.getElementById('vFecha')?.value,
        montoFinal: parseFloat(document.getElementById('vMontoFinalCalculado')?.value) || 0,
        observaciones: document.getElementById('vObservaciones')?.value || "",
        detallesVenta: [
            {
                vehiculoId: parseInt(idVehiculo),
                repuestoId: null,
                servicioId: null,
                cantidad: 1,
                precioUnitario: precioUnitario
            }
        ]
    };

    try {
        const resp = await fetch('/Admin/GuardarVenta', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'Authorization': `Bearer ${getAuthToken()}`
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

        await window.listarVentas();
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

    if (document.getElementById('vId')) document.getElementById('vId').value = "0";
    if (document.getElementById('vIdCliente')) document.getElementById('vIdCliente').value = "";
    if (document.getElementById('vIdVehiculo')) document.getElementById('vIdVehiculo').value = "";

    if (!document.getElementById('vMontoFinalCalculado')) {
        const formElem = document.getElementById('formVenta');
        if (formElem) {
            const hiddenFields = `<input type="hidden" id="vMontoFinalCalculado" value="0">`;
            formElem.insertAdjacentHTML('beforeend', hiddenFields);
        }
    } else {
        document.getElementById('vMontoFinalCalculado').value = "0";
    }

    window.limpiarSeleccionVehiculo();
}

window.verDetalleVentaCompletado = function(ventaObj) {
    window.abrirModalVenta(-1);

    if (document.getElementById('modalVentaTitulo')) {
        document.getElementById('modalVentaTitulo').innerText = `Consulta de Venta #C-${ventaObj.nroComprobante || ventaObj.id}`;
    }
    if (document.getElementById('vClienteBusqueda')) document.getElementById('vClienteBusqueda').value = ventaObj.nombreCliente || '';
    if (document.getElementById('vMedioPago')) document.getElementById('vMedioPago').value = ventaObj.formaPagoId || "1";
    if (document.getElementById('vTipoComprobante')) document.getElementById('vTipoComprobante').value = ventaObj.tipoComprobanteId || "1";
    if (document.getElementById('vFecha')) document.getElementById('vFecha').value = ventaObj.fechaVenta ? ventaObj.fechaVenta.slice(0, 16) : '';
    if (document.getElementById('vObservaciones')) document.getElementById('vObservaciones').value = ventaObj.observaciones || '';

    if (ventaObj.detalles && ventaObj.detalles.length > 0) {
        const primerItem = ventaObj.detalles[0];
        asignarItemFilaUnica(primerItem.descripcionItem || 'Unidad Vehicular', ventaObj.montoFinal);
    } else {
        asignarItemFilaUnica(ventaObj.detalleVehiculo || 'Detalle del Rodado', ventaObj.montoFinal);
    }

    if (!modalVenta) {
        const modalVentaElement = document.getElementById('modalVenta');
        if (modalVentaElement && typeof bootstrap !== 'undefined') {
            modalVenta = new bootstrap.Modal(modalVentaElement);
        }
    }

    opacidadCamposVenta(true);
    modalVenta?.show();
}

function opacidadCamposVenta(bloquear) {
    if (document.getElementById('vClienteBusqueda')) document.getElementById('vClienteBusqueda').disabled = bloquear;
    if (document.getElementById('vMedioPago')) document.getElementById('vMedioPago').disabled = bloquear;
    if (document.getElementById('vFecha')) document.getElementById('vFecha').disabled = bloquear;
    if (document.getElementById('vProductoBusqueda')) document.getElementById('vProductoBusqueda').disabled = bloquear;
    if (document.getElementById('vObservaciones')) document.getElementById('vObservaciones').disabled = bloquear;

    const btnSubmit = document.getElementById('btnGuardarVenta');
    if (btnSubmit) btnSubmit.style.setProperty('display', bloquear ? 'none' : 'block', 'important');
}

// Inicialización controlada para que no bloquee otros archivos JS en el Layout
document.addEventListener("DOMContentLoaded", () => {
    if (document.getElementById('tbodyVentas')) {
        window.inicializarModalVentaDeFormaSegura();
        window.listarVentas();
    }
});