// =========================================================================
// GESTIÓN DE CLIENTES 
// =========================================================================

let myModalCliente = null;
let listaClientesMemoria = []; // Memoria caché local
let ciudadesCargadasMemoria = []; // ✨ Guarda las ciudades con sus IDs reales de la BD

// Variable segura para el token (Busca en window o local storage de forma idéntica a ventas)
const getClientesAuthToken = () => {
    if (typeof window.token !== 'undefined' && window.token) return window.token;
    return localStorage.getItem('jonel_token') || '';
};

document.addEventListener("DOMContentLoaded", () => {
    // 🛡️ CONTROL DE ENTORNO: Si no estamos en la vista de clientes, frenamos acá de forma segura.
    if (!document.getElementById('tbodyClientes')) {
        return; 
    }

    // Inicializamos el modal de Bootstrap 5 de forma segura
    const modalElement = document.getElementById('modalCliente');
    if (modalElement) {
        myModalCliente = new bootstrap.Modal(modalElement);
    }

    // Inicializar el datalist de provincias cargándolo desde la Base de Datos
    cargarDatalistProvincias();

    // Ejecutamos la carga inicial del listado
    window.listarClientes();
});

// ---------------------------------------------------------------------
// OBTENER PROVINCIAS Y CIUDADES DESDE LA BASE DE DATOS (DATALISTS)
// ---------------------------------------------------------------------
async function cargarDatalistProvincias() {
    try {
        const tokenVal = getClientesAuthToken();
        const response = await fetch('/api/Ubicacion/Provincias', {
            method: 'GET',
            headers: { 'Authorization': `Bearer ${tokenVal}` }
        });
        
        if (!response.ok) throw new Error("Error al obtener provincias");
        const provincias = await response.json();
        
        const datalist = document.getElementById('datalistProvincias');
        if (!datalist) return;
        
        datalist.innerHTML = '';
        provincias.forEach(prov => {
            const option = document.createElement('option');
            option.value = prov.nombre || prov.Nombre; 
            datalist.appendChild(option);
        });
    } catch (error) {
        console.error("Error al cargar el catálogo de provincias:", error);
    }
}

// ✨ Modificado: recibe un parámetro para saber si viene desde una edición
window.onProvinciaChange = async function(esEdicion = false) {
    const provElem = document.getElementById('cEstadoProvincia');
    if (!provElem) return;
    
    const provinciaSeleccionada = provElem.value;
    const inputCiudad = document.getElementById('cCiudad');
    const datalistCiudades = document.getElementById('datalistCiudades');
    
    if (!inputCiudad || !datalistCiudades) return;

    // Si no es edición, limpiamos la ciudad porque el usuario cambió de provincia manualmente
    if (!esEdicion) {
        inputCiudad.value = '';
    }
    datalistCiudades.innerHTML = '';
    ciudadesCargadasMemoria = []; 
    
    if (!provinciaSeleccionada.trim()) {
        inputCiudad.disabled = true;
        return;
    }

    try {
        const tokenVal = getClientesAuthToken();
        const url = `/api/Ubicacion/Ciudades?provincia=${encodeURIComponent(provinciaSeleccionada)}`;
        const response = await fetch(url, {
            method: 'GET',
            headers: { 'Authorization': `Bearer ${tokenVal}` }
        });
        
        if (!response.ok) throw new Error("Error al obtener ciudades");
        const ciudades = await response.json();
        
        // Guardamos las ciudades en caché para extraer sus IDs numéricos al guardar
        ciudadesCargadasMemoria = ciudades;

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
window.listarClientes = async function() {
    const tbody = document.getElementById('tbodyClientes');
    if (!tbody) return;

    try {
        const tokenVal = getClientesAuthToken();
        const response = await fetch('/api/Clientes/Listar', {
            method: 'GET',
            headers: {
                'Authorization': `Bearer ${tokenVal}`,
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
            
            const ciudadObj = p.ciudad || p.Ciudad;
            const ciudad = (ciudadObj && typeof ciudadObj === 'object') ? (ciudadObj.nombre || ciudadObj.Nombre) : (ciudadObj || 'N/D');
            
            const provObj = ciudadObj ? (ciudadObj.provincia || ciudadObj.Provincia) : null;
            const provincia = provObj ? (provObj.nombre || provObj.Nombre || 'N/D') : (p.estadoProvincia || p.EstadoProvincia || 'N/D');
            
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
                <td class="fw-semibold text-white">${nombreCompleto}</td>
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
window.abrirModalCliente = function(id = 0) {
    limpiarFormularioCliente();

    if (id === 0) {
        if (document.getElementById('modalClienteTitulo')) document.getElementById('modalClienteTitulo').innerText = "Nuevo Cliente";
        if (document.getElementById('cId')) document.getElementById('cId').value = "0";
        if (document.getElementById('cIdPersonaId')) document.getElementById('cIdPersonaId').value = "0";
        myModalCliente?.show();
    }
}

window.editarClientePorIndex = function(index) {
    const clienteSeleccionado = listaClientesMemoria[index];
    if (clienteSeleccionado) {
        buscarClientePorId(clienteSeleccionado);
    }
}

async function buscarClientePorId(cliente) {
    limpiarFormularioCliente();

    if (document.getElementById('modalClienteTitulo')) {
        document.getElementById('modalClienteTitulo').innerText = "Modificar Perfil de Cliente";
    }

    const idCliente = cliente.id !== undefined ? cliente.id : cliente.Id;
    const idPersonaId = cliente.idPersonaId !== undefined ? cliente.idPersonaId : cliente.IdPersonaId;

    if (document.getElementById('cId')) document.getElementById('cId').value = idCliente;
    if (document.getElementById('cIdPersonaId')) document.getElementById('cIdPersonaId').value = idPersonaId;

    const p = cliente.persona || cliente.Persona;
    if (!p) return;

    if (document.getElementById('cDocumentoIdentidad')) document.getElementById('cDocumentoIdentidad').value = p.documentoIdentidad || p.DocumentoIdentidad || '';
    if (document.getElementById('cNombres')) document.getElementById('cNombres').value = p.nombres || p.Nombres || '';
    if (document.getElementById('cApellidos')) document.getElementById('cApellidos').value = p.apellidos || p.Apellidos || '';
    if (document.getElementById('cEmail')) document.getElementById('cEmail').value = p.email || p.Email || '';
    if (document.getElementById('cTelefono')) document.getElementById('cTelefono').value = p.telefono || p.Telefono || '';
    if (document.getElementById('cTelefonoAlternativo')) document.getElementById('cTelefonoAlternativo').value = p.telefonoAlternativo || p.TelefonoAlternativo || '';
    if (document.getElementById('cGenero')) document.getElementById('cGenero').value = p.genero || p.Genero || '';
    if (document.getElementById('cEstadoCivil')) document.getElementById('cEstadoCivil').value = p.estadoCivil || p.EstadoCivil || '';
    if (document.getElementById('cDireccion')) document.getElementById('cDireccion').value = p.direccion || p.Direccion || '';
    if (document.getElementById('cCodigoPostal')) document.getElementById('cCodigoPostal').value = p.codigoPostal || p.PostalCode || p.CodigoPostal || '';
    if (document.getElementById('cPais')) document.getElementById('cPais').value = p.pais || p.Pais || 'Argentina';

    const fechaNac = p.fechaNacimiento || p.FechaNacimiento;
    if (fechaNac && document.getElementById('cFechaNacimiento')) {
        const fechaStr = fechaNac.split('T')[0];
        document.getElementById('cFechaNacimiento').value = fechaStr;
    }

    if (document.getElementById('cCalificacionCrediticia')) document.getElementById('cCalificacionCrediticia').value = cliente.calificacionCrediticia || cliente.CalificacionCrediticia || 'Buena';
    if (document.getElementById('cObservaciones')) document.getElementById('cObservaciones').value = cliente.observaciones || cliente.Observaciones || '';

    const cityObj = p.ciudad || p.Ciudad;
    let ciuActual = '';
    let provActual = '';

    if (cityObj && typeof cityObj === 'object') {
        ciuActual = cityObj.nombre || cityObj.Nombre || '';
        
        const provObj = cityObj.provincia || cityObj.Provincia;
        if (provObj && typeof provObj === 'object') {
            provActual = provObj.nombre || provObj.Nombre || '';
        } else {
            provActual = p.estadoProvincia || p.EstadoProvincia || '';
        }
    }
    
    if (document.getElementById('cEstadoProvincia')) {
        document.getElementById('cEstadoProvincia').value = provActual;
    }
    
    await window.onProvinciaChange(true); 
    
    setTimeout(() => {
        if (document.getElementById('cCiudad')) {
            document.getElementById('cCiudad').value = ciuActual;
        }
    }, 120);

    myModalCliente?.show();
}

// ---------------------------------------------------------------------
// ENVIAR DATOS AL BACKEND
// ---------------------------------------------------------------------
window.guardarCliente = async function() {
    const form = document.getElementById('formCliente');
    if (!form) return;
    
    if (!form.checkValidity()) {
        form.reportValidity();
        return;
    }

    const formData = new FormData(form);
    const idCliente = document.getElementById('cId')?.value || "0";
    const idPersona = document.getElementById('cIdPersonaId')?.value || "0";

    formData.set('id', idCliente);
    formData.set('idPersonaId', idPersona);

    const inputCiudad = document.getElementById('cCiudad');
    const textoCiudadEscrita = inputCiudad ? inputCiudad.value.trim().toLowerCase() : '';
    let ciudadIdResuelto = null;

    if (textoCiudadEscrita) {
        const match = ciudadesCargadasMemoria.find(c => {
            const n = c.nombre || c.Nombre;
            return n.toLowerCase() === textoCiudadEscrita;
        });
        if (match) {
            ciudadIdResuelto = match.id !== undefined ? match.id : match.Id;
        }
    }

    if (ciudadIdResuelto) {
        formData.set('ciudadId', ciudadIdResuelto);
    } else {
        formData.set('ciudadId', "1"); 
    }

    try {
        const tokenVal = getClientesAuthToken();
        const response = await fetch('/api/Clientes/Guardar', {
            method: 'POST',
            headers: { 'Authorization': `Bearer ${tokenVal}` },
            body: formData
        });

        if (response.ok) {
            myModalCliente?.hide();
            limpiarFormularioCliente();
            window.listarClientes(); 
        } else {
            const error = await response.json();
            alert("Error del sistema: " + (error.message || "No se pudo procesar la transacción."));
        }
    } catch (err) {
        alert("Error de red: No se pudo establecer contacto con el servidor de la concesionaria.");
    }
}

// ---------------------------------------------------------------------
// LIMPIEZA TOTAL DEL FORMULARIO Y RESIDUOS
// ---------------------------------------------------------------------
function limpiarFormularioCliente() {
    const form = document.getElementById('formCliente');
    if (form) form.reset();

    const hiddenId = document.getElementById('cId');
    const hiddenPersonaId = document.getElementById('cIdPersonaId');
    if (hiddenId) hiddenId.value = "0";
    if (hiddenPersonaId) hiddenPersonaId.value = "0";

    const inputCiudad = document.getElementById("cCiudad");
    const datalistCiudades = document.getElementById("datalistCiudades");
    
    if (inputCiudad) inputCiudad.disabled = true;
    if (datalistCiudades) datalistCiudades.innerHTML = '';
    ciudadesCargadasMemoria = []; 
}