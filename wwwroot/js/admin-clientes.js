// =========================================================================
// GESTIÓN DE CLIENTES 
// =========================================================================

let myModalCliente = null;
let listaClientesMemoria = []; // Memoria caché local

document.addEventListener("DOMContentLoaded", () => {
    // Inicializamos el modal de Bootstrap 5 de forma segura
    const modalElement = document.getElementById('modalCliente');
    if (modalElement) {
        myModalCliente = new bootstrap.Modal(modalElement);
    }

    // Inicializar el datalist de provincias cargándolo desde la Base de Datos
    cargarDatalistProvincias();

    // Ejecutamos la carga inicial del listado
    listarClientes();
});

// ---------------------------------------------------------------------
// OBTENER PROVINCIAS Y CIUDADES DESDE LA BASE DE DATOS (DATALISTS)
// ---------------------------------------------------------------------
async function cargarDatalistProvincias() {
    try {
        // Reemplazar este endpoint por la ruta real de tu Controlador de C#
        const response = await fetch('/api/Ubicacion/Provincias', {
            method: 'GET',
            headers: { 'Authorization': `Bearer ${token}` }
        });
        
        if (!response.ok) throw new Error("Error al obtener provincias");
        const provincias = await response.json();
        
        const datalist = document.getElementById('datalistProvincias');
        if (!datalist) return;
        
        datalist.innerHTML = '';
        provincias.forEach(prov => {
            const option = document.createElement('option');
            // Se asume que tu objeto C# expone el nombre (ej: prov.nombre o prov.Nombre)
            option.value = prov.nombre || prov.Nombre; 
            datalist.appendChild(option);
        });
    } catch (error) {
        console.error("Error al cargar el catálogo de provincias:", error);
    }
}

async function onProvinciaChange() {
    const provinciaSeleccionada = document.getElementById('cEstadoProvincia').value;
    const inputCiudad = document.getElementById('cCiudad');
    const datalistCiudades = document.getElementById('datalistCiudades');
    
    if (!inputCiudad || !datalistCiudades) return;

    // Reiniciamos por completo el campo de ciudad al cambiar o borrar la provincia
    inputCiudad.value = '';
    datalistCiudades.innerHTML = '';
    
    if (!provinciaSeleccionada.trim()) {
        inputCiudad.disabled = true;
        return;
    }

    try {
        // Enviamos la provincia elegida como parámetro para consultar las ciudades correspondientes
        const url = `/api/Ubicacion/Ciudades?provincia=${encodeURIComponent(provinciaSeleccionada)}`;
        const response = await fetch(url, {
            method: 'GET',
            headers: { 'Authorization': `Bearer ${token}` }
        });
        
        if (!response.ok) throw new Error("Error al obtener ciudades");
        const ciudades = await response.json();
        
        if (ciudades.length > 0) {
            ciudades.forEach(ciu => {
                const option = document.createElement('option');
                option.value = ciu.nombre || ciu.Nombre;
                datalistCiudades.appendChild(option);
            });
            inputCiudad.disabled = false;
        } else {
            inputCiudad.disabled = true;
        }
    } catch (error) {
        console.error("Error al cargar el catálogo de ciudades de la BD:", error);
        inputCiudad.disabled = true;
    }
}

// ---------------------------------------------------------------------
// OBTENER Y RENDERIZAR LISTADO DE CLIENTES
// ---------------------------------------------------------------------
async function listarClientes() {
    const tbody = document.getElementById('tbodyClientes');
    if (!tbody) return;

    try {
        const response = await fetch('/api/Clientes/Listar', {
            method: 'GET',
            headers: {
                'Authorization': `Bearer ${token}`,
                'Accept': 'application/json'
            }
        });

        if (!response.ok) throw new Error("Error al responder");

        const clientes = await response.json();
        listaClientesMemoria = clientes; 
        tbody.innerHTML = ""; 

        if (clientes.length === 0) {
            tbody.innerHTML = `
                <tr>
                    <td colspan="7" class="text-center py-4 text-muted">
                        <i class="bi bi-info-circle me-1"></i> No hay clientes registrados en el sistema.
                    </td>
                </tr>`;
            return;
        }

        clientes.forEach((c, index) => {
            const p = c.persona || c.Persona; 
            if (!p) return;

            const apellidos = p.apellidos || p.Apellidos || '';
            const nombres = p.nombres || p.Nombres || '';
            const nombreCompleto = `${apellidos}, ${nombres}`;
            
            const documento = p.documentoIdentidad || p.DocumentoIdentidad || 'N/D';
            const email = p.email || p.Email || 'N/D';
            const telefono = p.telefono || p.Telefono || 'N/D';
            const ciudad = p.ciudad || p.Ciudad || '';
            const provincia = p.estadoProvincia || p.EstadoProvincia || 'N/D';
            const calificacion = c.calificacionCrediticia || c.CalificacionCrediticia || 'Buena';

            const fechaAltaRaw = c.idFechaAlta || c.IdFechaAlta || c.fechaAlta || c.FechaAlta;
            const fechaAltaFormateada = fechaAltaRaw ? new Date(fechaAltaRaw).toLocaleDateString('es-AR') : 'N/D';

            let badgeColor = "bg-secondary";
            if (calificacion === "Excelente") badgeColor = "bg-success";
            if (calificacion === "Buena") badgeColor = "bg-info text-dark";
            if (calificacion === "Regular") badgeColor = "bg-warning text-dark";
            if (calificacion === "Riesgosa") badgeColor = "bg-danger";

            const tr = document.createElement('tr');
            tr.innerHTML = `
                <td><span class="badge bg-light text-dark border">${documento}</span></td>
                <td class="fw-semibold text-dark">${nombreCompleto}</td>
                <td>
                    <div class="small"><i class="bi bi-envelope text-muted me-1"></i>${email}</div>
                    <div class="small text-muted"><i class="bi bi-telephone me-1"></i>${telefono}</div>
                </td>
                <td class="small text-secondary">${ciudad} (${provincia})</td>
                <td><span class="badge ${badgeColor}">${calificacion}</span></td>
                <td class="text-muted small">${fechaAltaFormateada}</td>
                <td class="text-end pe-4">
                    <button class="btn btn-sm btn-outline-primary me-1" onclick="editarClientePorIndex(${index})" title="Editar Cliente">
                        <i class="bi bi-pencil-square"></i>
                    </button>
                </td>
            `;
            tbody.appendChild(tr);
        });

    } catch (err) {
        tbody.innerHTML = `
            <tr>
                <td colspan="7" class="text-center py-4 text-danger fw-semibold">
                    <i class="bi bi-exclamation-triangle-fill me-1"></i> Error de conexión al cargar el listado.
                </td>
            </tr>`;
    }
}

// ---------------------------------------------------------------------
// PREPARACIÓN Y APERTURA DEL MODAL (CREAR / EDITAR)
// ---------------------------------------------------------------------
function abrirModalCliente(id = 0) {
    limpiarFormularioCliente();

    if (id === 0) {
        document.getElementById('modalClienteTitulo').innerText = "Nuevo Cliente";
        document.getElementById('cId').value = "0";
        document.getElementById('cIdPersonaId').value = "0";
        myModalCliente?.show();
    }
}

function editarClientePorIndex(index) {
    const clienteSeleccionado = listaClientesMemoria[index];
    if (clienteSeleccionado) {
        buscarClientePorId(clienteSeleccionado);
    }
}

// Función encargada de recibir el objeto seleccionado e inyectarlo en el formulario
async function buscarClientePorId(cliente) {
    limpiarFormularioCliente();

    document.getElementById('modalClienteTitulo').innerText = "Modificar Perfil de Cliente";

    const idCliente = cliente.id !== undefined ? cliente.id : cliente.Id;
    const idPersonaId = cliente.idPersonaId !== undefined ? cliente.idPersonaId : cliente.IdPersonaId;

    document.getElementById('cId').value = idCliente;
    document.getElementById('cIdPersonaId').value = idPersonaId;

    const p = cliente.persona || cliente.Persona;
    if (!p) return;

    document.getElementById('cDocumentoIdentidad').value = p.documentoIdentidad || p.DocumentoIdentidad || '';
    document.getElementById('cNombres').value = p.nombres || p.Nombres || '';
    document.getElementById('cApellidos').value = p.apellidos || p.Apellidos || '';
    document.getElementById('cEmail').value = p.email || p.Email || '';
    document.getElementById('cTelefono').value = p.telefono || p.Telefono || '';
    document.getElementById('cTelefonoAlternativo').value = p.telefonoAlternativo || p.TelefonoAlternativo || '';
    document.getElementById('cGenero').value = p.genero || p.Genero || '';
    document.getElementById('cEstadoCivil').value = p.estadoCivil || p.EstadoCivil || '';
    document.getElementById('cDireccion').value = p.direccion || p.Direccion || '';
    
    // Inyección asíncrona controlada del Datalist de Ubicación
    const provActual = p.estadoProvincia || p.EstadoProvincia || '';
    const ciuActual = p.ciudad || p.Ciudad || '';
    
    document.getElementById('cEstadoProvincia').value = provActual;
    
    // Esperamos que cargue las ciudades asociadas de la BD antes de setear el input de Ciudad
    await onProvinciaChange(); 
    document.getElementById('cCiudad').value = ciuActual;

    document.getElementById('cCodigoPostal').value = p.codigoPostal || p.PostalCode || p.CodigoPostal || '';
    document.getElementById('cPais').value = p.pais || p.Pais || 'Argentina';

    const fechaNac = p.fechaNacimiento || p.FechaNacimiento;
    if (fechaNac) {
        const fechaStr = fechaNac.split('T')[0];
        document.getElementById('cFechaNacimiento').value = fechaStr;
    }

    document.getElementById('cCalificacionCrediticia').value = cliente.calificacionCrediticia || cliente.CalificacionCrediticia || 'Buena';
    document.getElementById('cObservaciones').value = cliente.observaciones || cliente.Observaciones || '';

    myModalCliente?.show();
}

// ---------------------------------------------------------------------
// ENVIAR DATOS AL BACKEND
// ---------------------------------------------------------------------
async function guardarCliente() {
    const form = document.getElementById('formCliente');
    if (!form.checkValidity()) {
        form.reportValidity();
        return;
    }

    const formData = new FormData(form);
    const idCliente = document.getElementById('cId').value;
    const idPersona = document.getElementById('cIdPersonaId').value;

    formData.set('id', idCliente);
    formData.set('idPersonaId', idPersona);

    try {
        const response = await fetch('/api/Clientes/Guardar', {
            method: 'POST',
            headers: { 'Authorization': `Bearer ${token}` },
            body: formData
        });

        if (response.ok) {
            myModalCliente?.hide();
            limpiarFormularioCliente();
            listarClientes(); 
        } else {
            const error = await response.json();
            alert("Error del sistema: " + (error.message || "No se pudo procesar la transacción."));
        }
    } catch (err) {
        alert("Error de red: No se pudo establecer contacto con el servidor de la concesionaria.");
    }
}

// ---------------------------------------------------------------------
// 🧼 LIMPIEZA TOTAL DEL FORMULARIO Y RESIDUOS
// ---------------------------------------------------------------------
function limpiarFormularioCliente() {
    const form = document.getElementById('formCliente');
    if (form) form.reset();

    const hiddenId = document.getElementById('cId');
    const hiddenPersonaId = document.getElementById('cIdPersonaId');
    if (hiddenId) hiddenId.value = "0";
    if (hiddenPersonaId) hiddenPersonaId.value = "0";

    // Reiniciar inputs y datalists del autocompletado
    const inputCiudad = document.getElementById("cCiudad");
    const datalistCiudades = document.getElementById("datalistCiudades");
    
    if (inputCiudad) inputCiudad.disabled = true;
    if (datalistCiudades) datalistCiudades.innerHTML = '';
}