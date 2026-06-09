

// =========================================================================
//GESTIÓN DE CLIENTES 
// =========================================================================

let myModalCliente = null;
// Asumimos que la variable 'token' ya está declarada globalmente en tu dashboard principal.

document.addEventListener("DOMContentLoaded", () => {
    // Inicializamos el modal de Bootstrap 5 de forma segura
    const modalElement = document.getElementById('modalCliente');
    if (modalElement) {
        myModalCliente = new bootstrap.Modal(modalElement);
    }
    
    // Ejecutamos la carga inicial del listado
    listarClientes();
});

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
        tbody.innerHTML = ""; // Limpiamos el spinner de carga

        if (clientes.length === 0) {
            tbody.innerHTML = `
                <tr>
                    <td colspan="8" class="text-center py-4 text-muted">
                        <i class="bi bi-info-circle me-1"></i> No hay clientes registrados en el sistema.
                    </td>
                </tr>`;
            return;
        }

        // Iteramos el JSON compuesto (Cliente + Persona incluida por Entity Framework)
        clientes.forEach(c => {
            const p = c.persona; // Objeto de navegación de persona
            const nombreCompleto = `${p.apellidos}, ${p.nombres}`;
            
            // 🛠️ AJUSTE AQUÍ: EF Core serializa las propiedades respetando el nombre de C# (o camelCase si está configurado).
            // Usamos 'c.idFechaAlta' tal como figura en tu modelo de C#.
            const fechaAltaRaw = c.idFechaAlta || c.fechaAlta; 
            const fechaAltaFormateada = fechaAltaRaw ? new Date(fechaAltaRaw).toLocaleDateString('es-AR') : 'N/D';
            
            // Evaluamos color del Badge de calificación crediticia
            let badgeColor = "bg-secondary";
            if (c.calificacionCrediticia === "Excelente") badgeColor = "bg-success";
            if (c.calificacionCrediticia === "Buena") badgeColor = "bg-info text-dark";
            if (c.calificacionCrediticia === "Regular") badgeColor = "bg-warning text-dark";
            if (c.calificacionCrediticia === "Riesgosa") badgeColor = "bg-danger";

            const tr = document.createElement('tr');
            tr.innerHTML = `
               
                <td><span class="badge bg-light text-dark border">${p.documentoIdentidad}</span></td>
                <td class="fw-semibold text-dark">${nombreCompleto}</td>
                <td>
                    <div class="small"><i class="bi bi-envelope text-muted me-1"></i>${p.email}</div>
                    <div class="small text-muted"><i class="bi bi-telephone me-1"></i>${p.telefono || 'N/D'}</div>
                </td>
                <td class="small text-secondary">${p.ciudad || ''} (${p.estadoProvincia || 'N/D'})</td>
                <td><span class="badge ${badgeColor}">${c.calificacionCrediticia || 'Buena'}</span></td>
                <td class="text-muted small">${fechaAltaFormateada}</td>
                <td class="text-end pe-4">
                    <button class="btn btn-sm btn-outline-primary me-1" onclick='buscarClientePorId(${JSON.stringify(c)})' title="Editar Cliente">
                        <i class="bi bi-pencil-square"></i>
                    </button>
                </td>
            `;
            tbody.appendChild(tr);
        });

    } catch (err) {
        tbody.innerHTML = `
            <tr>
                <td colspan="8" class="text-center py-4 text-danger fw-semibold">
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

// Función encargada de recibir el objeto seleccionado e inyectarlo en el formulario
function buscarClientePorId(cliente) {
    limpiarFormularioCliente();
    
    document.getElementById('modalClienteTitulo').innerText = "Modificar Perfil de Cliente";
    
    // IDs de control relacional
    document.getElementById('cId').value = cliente.id;
    document.getElementById('cIdPersonaId').value = cliente.idPersonaId;

    // Datos extraídos del objeto embebido de la Persona
    const p = cliente.persona;
    document.getElementById('cDocumentoIdentidad').value = p.documentoIdentidad;
    document.getElementById('cNombres').value = p.nombres;
    document.getElementById('cApellidos').value = p.apellidos;
    document.getElementById('cEmail').value = p.email;
    document.getElementById('cTelefono').value = p.telefono || '';
    document.getElementById('cTelefonoAlternativo').value = p.telefonoAlternativo || '';
    document.getElementById('cGenero').value = p.genero || '';
    document.getElementById('cEstadoCivil').value = p.estadoCivil || '';
    document.getElementById('cDireccion').value = p.direccion || '';
    document.getElementById('cCiudad').value = p.ciudad || '';
    document.getElementById('cEstadoProvincia').value = p.estadoProvincia || '';
    document.getElementById('cCodigoPostal').value = p.codigoPostal || '';
    document.getElementById('cPais').value = p.pais || 'Argentina';

    // Formateo de fecha de nacimiento si existe para que el input HTML tipo date lo interprete
    if (p.fechaNacimiento) {
        const fechaStr = p.fechaNacimiento.split('T')[0];
        document.getElementById('cFechaNacimiento').value = fechaStr;
    }

    // Datos directos de la Ficha Comercial del Cliente
    document.getElementById('cCalificacionCrediticia').value = cliente.calificacionCrediticia || 'Buena';
    document.getElementById('cObservaciones').value = cliente.observaciones || '';

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

    // Inicializamos el FormData con las claves basadas en los "name" del HTML
    const formData = new FormData(form);

    // Forzamos los parámetros clave con sus casings exactos para C# .NET
    const idCliente = document.getElementById('cId').value;
    const idPersona = document.getElementById('cIdPersonaId').value;
    
    formData.set('id', idCliente);
    formData.set('idPersonaId', idPersona);

    try {
        const response = await fetch('/api/Clientes/Guardar', {
            method: 'POST',
            headers: { 'Authorization': `Bearer ${token}` }, // Envío directo del binario mutipart sin definir Content-Type
            body: formData
        });

        if (response.ok) {
            myModalCliente?.hide();
            limpiarFormularioCliente();
            listarClientes(); // Refrescamos la tabla en tiempo real
        } else {
            const error = await response.json();
            alert("Error del sistema: " + (error.message || "No se pudo procesar la transacción."));
        }
    } catch (err) {
        alert("Error de red: No se pudo establecer contacto con el servidor de la concesionaria.");
    }
}

// ---------------------------------------------------------------------
// 🧼 4. LIMPIEZA TOTAL DEL FORMULARIO Y RESIDUOS
// ---------------------------------------------------------------------
function limpiarFormularioCliente() {
    const form = document.getElementById('formCliente');
    if (form) form.reset();

    // Limpieza explícita de los ID ocultos
    const hiddenId = document.getElementById('cId');
    const hiddenPersonaId = document.getElementById('cIdPersonaId');
    if (hiddenId) hiddenId.value = "0";
    if (hiddenPersonaId) hiddenPersonaId.value = "0";
}



// Mapa de las principales ciudades por cada provincia argentina
const ciudadesPorProvincia = {
    "Buenos Aires": ["La Plata", "Mar del Plata", "Bahía Blanca", "Tandil", "Pilar", "San Isidro", "Lanús", "Quilmes", "Olavarría", "Pergamino"],
    "Ciudad Autónoma de Buenos Aires": ["Palermo", "Caballito", "Flores", "Belgrano", "Retiro", "Recoleta", "San Telmo"],
    "Catamarca": ["San Fernando del Valle de Catamarca", "Andalgalá", "Tinogasta", "Belén", "Santa María"],
    "Chaco": ["Resistencia", "Presidencia Roque Sáenz Peña", "Villa Ángela", "Charata", "General José de San Martín"],
    "Chubut": ["Rawson", "Comodoro Rivadavia", "Trelew", "Puerto Madryn", "Esquel"],
    "Córdoba": ["Córdoba Capital", "Río Cuarto", "Villa María", "Carlos Paz", "San Francisco", "Alta Gracia", "Río Tercero"],
    "Corrientes": ["Corrientes", "Goya", "Paso de los Libres", "Curuzú Cuatiá", "Mercedes", "Santo Tomé"],
    "Entre Ríos": ["Paraná", "Concordia", "Gualeguaychú", "Concepción del Uruguay", "Federación", "Gualeguay"],
    "Formosa": ["Formosa", "Clorinda", "Pirané", "El Colorado"],
    "Jujuy": ["San Salvador de Jujuy", "San Pedro de Jujuy", "Palpalá", "Libertador General San Martín"],
    "La Pampa": ["Santa Rosa", "General Pico", "Eduardo Castex", "Toay"],
    "La Rioja": ["La Rioja", "Chilecito", "Aimogasta", "Chepes"],
    "Mendoza": ["Mendoza Capital", "San Rafael", "Godoy Cruz", "Las Heras", "Maipú", "Luján de Cuyo", "San Martín"],
    "Misiones": ["Posadas", "Oberá", "Eldorado", "Puerto Iguazú", "San Vicente"],
    "Neuquén": ["Neuquén Capital", "San Martín de los Andes", "Cutral Có", "Plottier", "Centenario", "Zapala"],
    "Río Negro": ["Viedma", "San Carlos de Bariloche", "General Roca", "Cipolletti", "San Antonio Oeste"],
    "Salta": ["Salta Capital", "San Ramón de la Nueva Orán", "Tartagal", "General Güemes", "Cafayate"],
    "San Juan": ["San Juan Capital", "Caucete", "Chimbas", "Rivadavia", "Santa Lucía"],
    "San Luis": ["La Punta", "San Luis Capital", "Villa Mercedes", "Merlo", "Juana Koslay", "Justo Daract"],
    "Santa Cruz": ["Río Gallegos", "Caleta Olivia", "El Calafate", "Puerto Deseado", "Las Heras"],
    "Santa Fe": ["Santa Fe Capital", "Rosario", "Rafaela", "Venado Tuerto", "Reconquista", "Santo Tomé"],
    "Santiago del Estero": ["Santiago del Estero", "La Banda", "Termas de Río Hondo", "Frías"],
    "Tierra del Fuego": ["Ushuaia", "Río Grande", "Tolhuin"],
    "Tucumán": ["San Miguel de Tucumán", "Yerba Buena", "Tafí Viejo", "Concepción", "Aguilares"]
};

function cargarCiudadesPorProvincia() {
    const provinciaSelect = document.getElementById("cEstadoProvincia");
    const ciudadSelect = document.getElementById("cCiudad");
    const provinciaSeleccionada = provinciaSelect.value;

    // Limpiar select de ciudades
    ciudadSelect.innerHTML = '<option value="">Seleccionar...</option>';

    if (provinciaSeleccionada && ciudadesPorProvincia[provinciaSeleccionada]) {
        // Rellenar con las ciudades de la provincia elegida
        ciudadesPorProvincia[provinciaSeleccionada].forEach(ciudad => {
            const option = document.createElement("option");
            option.value = ciudad;
            option.textContent = ciudad;
            ciudadSelect.appendChild(option);
        });
    } else {
        ciudadSelect.innerHTML = '<option value="">Seleccione una provincia...</option>';
    }
}