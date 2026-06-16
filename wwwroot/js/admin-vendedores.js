// // ========================================================
// // GESTIÓN DE VENDEDORES - JONEL AUTOS
// // ========================================================

// // Variables globales para la sección
// let vendedores = [];
// let modalVendedorBootstrap = null;
// let listaCiudadesVendedoresMemoria = []; // Almacena temporalmente las ciudades de la provincia elegida

// // Esperar a que el DOM esté listo para inicializar el modal si existe
// document.addEventListener("DOMContentLoaded", () => {
//     const modalEl = document.getElementById('modalVendedor');
//     if (modalEl) {
//         modalVendedorBootstrap = new bootstrap.Modal(modalEl);
//     }
// });

// /**
//  * Función principal invocada por el ruteador de secciones del Index
//  */
// async function listarVendedores() {
//     const tbody = document.getElementById('tabla-vendedores-body');
//     if (!tbody) return;

//     tbody.innerHTML = `<tr><td colspan="6" class="text-center"><div class="spinner-border text-danger" role="status"></div></td></tr>`;

//     try {
//         const response = await fetch('/Admin/GetVendedores', {
//             method: 'GET',
//             headers: {
//                 'Authorization': `Bearer ${token}`,
//                 'Content-Type': 'application/json'
//             }
//         });

//         if (!response.ok) throw new Error("No se pudo obtener la lista de vendedores.");

//         vendedores = await response.json();
//         renderizarTablaVendedores(vendedores);
        
//         // Carga el catálogo base de provincias al iniciar la sección
//         await cargarDatalistProvinciasVendedores();

//     } catch (error) {
//         console.error(error);
//         tbody.innerHTML = `<tr><td colspan="6" class="text-center text-danger">Error al cargar los datos: ${error.message}</td></tr>`;
//     }
// }

// /**
//  * Renderiza las filas de la tabla de vendedores extendida
//  */
// function renderizarTablaVendedores(lista) {
//     const tbody = document.getElementById('tabla-vendedores-body');
//     if (!tbody) return;

//     if (lista.length === 0) {
//         tbody.innerHTML = `<tr><td colspan="6" class="text-center text-muted">No hay vendedores registrados.</td></tr>`;
//         return;
//     }

//     tbody.innerHTML = '';
//     lista.forEach(v => {
//         // Limpieza básica del teléfono para el enlace de WhatsApp
//         const telefonoLimpio = v.telefono ? v.telefono.replace(/\D/g, '') : '';
        
//         tbody.innerHTML += `
//             <tr>
//                 <td class="align-middle text-white fw-bold">${v.documentoIdentidad || '---'}</td>
//                 <td class="align-middle fw-bold">${v.apellidos}, ${v.nombres}</td>
//                 <td class="align-middle">
//                     ${telefonoLimpio ? `
//                     <a href="https://wa.me/${telefonoLimpio}" target="_blank" class="text-success text-decoration-none fw-bold">
//                         <i class="bi bi-whatsapp"></i> ${v.telefono}
//                     </a>` : '<span class="text-white-50 small">Sin teléfono</span>'}
//                 </td>
//                 <td class="align-middle">${v.email || '<span class="text-white-50 small">No especificado</span>'}</td>
//                 <td class="align-middle text-info fw-bold">${v.porcentajeComision || 0}%</td>
//                 <td class="align-middle text-end pe-4">
//                     <button class="btn btn-sm btn-outline-warning me-1" onclick="editarVendedor(${v.id})">
//                         <i class="bi bi-pencil"></i>
//                     </button>
//                     <button class="btn btn-sm btn-outline-danger" onclick="eliminarVendedor(${v.id})">
//                         <i class="bi bi-trash"></i>
//                     </button>
//                 </td>
//             </tr>
//         `;
//     });
// }

// /**
//  * Prepara el formulario del modal para un nuevo registro (Limpia los campos)
//  */
// function nuevoVendedor() {
//     if (!modalVendedorBootstrap) return;

//     const modalTitleEl = document.getElementById('modalVendedorTitle');
//     if (modalTitleEl) {
//         const span = modalTitleEl.querySelector('span');
//         if (span) span.innerText = "Nuevo Vendedor";
//         else modalTitleEl.innerText = "Nuevo Vendedor";
//     }
    
//     document.getElementById('vendedor-id').value = "0";
//     document.getElementById('vendedor-persona-id').value = "0";
    
//     // 1. Personales Básicos
//     document.getElementById('vendedor-nombre').value = "";
//     document.getElementById('vendedor-apellido').value = "";
//     document.getElementById('vendedor-documento').value = "";
//     document.getElementById('vendedor-telefono').value = "";
//     document.getElementById('vendedor-email').value = "";
    
//     // Campos Personales a Limpiar
//     document.getElementById('vendedor-fecha-nacimiento').value = "";
//     document.getElementById('vendedor-genero').value = "";
//     document.getElementById('vendedor-estado-civil').value = "";
//     document.getElementById('vendedor-codigo-postal').value = "";
    
//     // Control Geográfico Dinámico
//     document.getElementById('vendedor-provincia').value = "";
//     const inputCiudad = document.getElementById('vendedor-ciudad');
//     inputCiudad.value = "";
//     inputCiudad.disabled = true;
//     inputCiudad.placeholder = "Seleccione provincia...";
//     document.getElementById('vendedor-ciudad-id').value = "1";
//     document.getElementById('datalistCiudadesVendedores').innerHTML = '';
//     listaCiudadesVendedoresMemoria = [];
    
//     // 2. Credenciales (Obligatoria para nuevos)
//     const inputUser = document.getElementById('vendedor-user');
//     const inputPass = document.getElementById('vendedor-password');
//     inputUser.value = "";
//     inputUser.disabled = false;
//     inputPass.value = "";
//     inputPass.required = true; 
    
//     // Ocultar el texto de ayuda de la contraseña en altas
//     const helpText = document.getElementById('vendedor-password-help');
//     if (helpText) {
//         helpText.style.setProperty('display', 'none', 'important');
//     }
    
//     // 3. Comerciales
//     document.getElementById('vendedor-comision').value = "";
//     document.getElementById('vendedor-observaciones').value = "";

//     modalVendedorBootstrap.show();
// }

// /**
//  * Carga los datos del vendedor en el formulario dinámico para su edición
//  */
// // async function editarVendedor(id) {
// //     const v = vendedores.find(vendedor => vendedor.id === id);
// //     if (!v || !modalVendedorBootstrap) return;

// //     const modalTitleEl = document.getElementById('modalVendedorTitle');
// //     if (modalTitleEl) {
// //         const span = modalTitleEl.querySelector('span');
// //         if (span) span.innerText = "Editar Vendedor";
// //         else modalTitleEl.innerText = "Editar Vendedor";
// //     }

// //     document.getElementById('vendedor-id').value = v.id;
// //     document.getElementById('vendedor-persona-id').value = v.idPersonaId || 0;
    
// //     // 1. Personales Básicos
// //     document.getElementById('vendedor-nombre').value = v.nombres || "";
// //     document.getElementById('vendedor-apellido').value = v.apellidos || "";
// //     document.getElementById('vendedor-documento').value = v.documentoIdentidad || "";
// //     document.getElementById('vendedor-telefono').value = v.telefono || "";
// //     document.getElementById('vendedor-email').value = v.email || "";
    
// //     // Carga de Campos Personales (Controlando nulos si vienen de la BD)
// //     if (v.fechaNacimiento) {
// //         document.getElementById('vendedor-fecha-nacimiento').value = v.fechaNacimiento.split('T')[0];
// //     } else {
// //         document.getElementById('vendedor-fecha-nacimiento').value = "";
// //     }
// //     document.getElementById('vendedor-genero').value = v.genero || "";
// //     document.getElementById('vendedor-estado-civil').value = v.estadoCivil || "";
// //     document.getElementById('vendedor-codigo-postal').value = v.codigoPostal || "";
    
// //     // Asignación de Ubicación Geográfica Predictiva
// //     document.getElementById('vendedor-provincia').value = v.estadoProvincia || v.provincia || "";
// //     document.getElementById('vendedor-ciudad-id').value = v.ciudadId || "1";
    
// //     const inputCiudad = document.getElementById('vendedor-ciudad');
// //     inputCiudad.value = v.nombreCiudad || v.ciudad || ""; 
    
// //     // Rehidratar asíncronamente el Datalist de Ciudades dependientes de la Provincia asignada
// //     const provActual = v.estadoProvincia || v.provincia;
// //     if (provActual) {
// //         await cargarCiudadesPorProvincia(provActual);
// //         inputCiudad.disabled = false;
// //     } else {
// //         inputCiudad.disabled = true;
// //     }
    
// //     // 2. Credenciales
// //     const inputUser = document.getElementById('vendedor-user');
// //     const inputPass = document.getElementById('vendedor-password');
// //     inputUser.value = v.nombreUsuario || v.userName || "";
// //     inputUser.disabled = true; 
// //     inputPass.value = ""; 
// //     inputPass.required = false; 
    
// //     // Mostrar el texto de ayuda de la contraseña al editar
// //     const helpText = document.getElementById('vendedor-password-help');
// //     if (helpText) {
// //         helpText.style.setProperty('display', 'block', 'important');
// //     }
    
// //     // 3. Comerciales
// //     document.getElementById('vendedor-comision').value = v.porcentajeComision !== undefined ? v.porcentajeComision : (v.comision || 0);
// //     document.getElementById('vendedor-observaciones').value = v.observaciones || "";

// //     modalVendedorBootstrap.show();
// // }
// async function editarVendedor(id) {
//     const v = vendedores.find(vendedor => vendedor.id === id);
//     if (!v || !modalVendedorBootstrap) return;

//     const modalTitleEl = document.getElementById('modalVendedorTitle');
//     if (modalTitleEl) {
//         const span = modalTitleEl.querySelector('span');
//         if (span) span.innerText = "Editar Vendedor";
//         else modalTitleEl.innerText = "Editar Vendedor";
//     }

//     document.getElementById('vendedor-id').value = v.id;
//     document.getElementById('vendedor-persona-id').value = v.idPersonaId || 0;
    
//     // 1. Personales Básicos
//     document.getElementById('vendedor-nombre').value = v.nombres || "";
//     document.getElementById('vendedor-apellido').value = v.apellidos || "";
//     document.getElementById('vendedor-documento').value = v.documentoIdentidad || "";
//     document.getElementById('vendedor-telefono').value = v.telefono || "";
//     document.getElementById('vendedor-email').value = v.email || "";
    
//     // Carga de Campos Personales 
//     if (v.fechaNacimiento) {
//         document.getElementById('vendedor-fecha-nacimiento').value = v.fechaNacimiento.split('T')[0];
//     } else {
//         document.getElementById('vendedor-fecha-nacimiento').value = "";
//     }
//     document.getElementById('vendedor-genero').value = v.genero || "";
//     document.getElementById('vendedor-estado-civil').value = v.estadoCivil || "";
//     document.getElementById('vendedor-codigo-postal').value = v.codigoPostal || "";
    
//     // 2. CONFIGURACIÓN GEOGRÁFICA (Orden corregido para evitar solapamientos)
//     const provActual = v.estadoProvincia || v.provincia || "";
//     document.getElementById('vendedor-provincia').value = provActual;
    
//     const inputCiudad = document.getElementById('vendedor-ciudad');
    
//     if (provActual) {
//         // Primero esperamos de forma garantizada que las ciudades existan en el datalist
//         await cargarCiudadesPorProvincia(provActual);
        
//         // RECIÉN ACÁ asignamos el texto de la ciudad y el ID correspondiente de la BD
//         inputCiudad.value = v.nombreCiudad || v.ciudad || ""; 
//         document.getElementById('vendedor-ciudad-id').value = v.ciudadId || "1";
//         inputCiudad.disabled = false;
//     } else {
//         inputCiudad.value = "";
//         document.getElementById('vendedor-ciudad-id').value = "1";
//         inputCiudad.disabled = true;
//     }
    
//     // 3. Credenciales
//     const inputUser = document.getElementById('vendedor-user');
//     const inputPass = document.getElementById('vendedor-password');
//     inputUser.value = v.nombreUsuario || v.userName || "";
//     inputUser.disabled = true; 
//     inputPass.value = ""; 
//     inputPass.required = false; 
    
//     const helpText = document.getElementById('vendedor-password-help');
//     if (helpText) {
//         helpText.style.setProperty('display', 'block', 'important');
//     }
    
//     // 4. Comerciales
//     document.getElementById('vendedor-comision').value = v.porcentajeComision !== undefined ? v.porcentajeComision : (v.comision || 0);
//     document.getElementById('vendedor-observaciones').value = v.observaciones || "";

//     modalVendedorBootstrap.show();
// }

// /**
//  * Envía la estructura completa unificada al Backend (VendedorDto)
//  */
// async function guardarVendedor(event) {
//     if (event) event.preventDefault();

//     const id = parseInt(document.getElementById('vendedor-id').value) || 0;
//     const fechaNacVal = document.getElementById('vendedor-fecha-nacimiento').value;
    
//     // Armamos el objeto con la estructura exacta que espera tu VendedorDto en el backend
//     const model = {
//         id: id,
//         idPersonaId: parseInt(document.getElementById('vendedor-persona-id').value) || 0,
//         documentoIdentidad: document.getElementById('vendedor-documento').value.trim(),
//         nombres: document.getElementById('vendedor-nombre').value.trim(),
//         apellidos: document.getElementById('vendedor-apellido').value.trim(),
//         email: document.getElementById('vendedor-email').value.trim(),
//         telefono: document.getElementById('vendedor-telefono').value.trim(),
//         fechaNacimiento: fechaNacVal ? fechaNacVal : null,
//         genero: document.getElementById('vendedor-genero').value,
//         estadoCivil: document.getElementById('vendedor-estado-civil').value,
//         estadoProvincia: document.getElementById('vendedor-provincia').value.trim(),
//         ciudadId: parseInt(document.getElementById('vendedor-ciudad-id').value) || 1,
//         codigoPostal: document.getElementById('vendedor-codigo-postal').value.trim(),
//         nombreUsuario: document.getElementById('vendedor-user').value.trim(),
//         password: document.getElementById('vendedor-password').value, 
//         porcentajeComision: parseFloat(document.getElementById('vendedor-comision').value) || 0,
//         observaciones: document.getElementById('vendedor-observaciones').value.trim()
//     };

//     try {
//         // CORRECCIÓN CRÍTICA: Se añade el body con la información serializada en JSON
//         const response = await fetch('/Admin/GuardarVendedor', {
//             method: 'POST',
//             headers: {
//                 'Authorization': `Bearer ${token}`,
//                 'Content-Type': 'application/json'
//             },
//             body: JSON.stringify(model)
//         });

//         const data = await response.json();

//         if (response.ok) {
//             if (modalVendedorBootstrap) modalVendedorBootstrap.hide();
//             listarVendedores(); // Refrescar la grilla principal
//         } else {
//             alert(data.message || "Ocurrió un error al procesar la solicitud.");
//         }

//     } catch (error) {
//         console.error(error);
//         alert("Error de comunicación con el servidor.");
//     }
// }

// /**
//  * Envía el ID para dar de baja el registro
//  */
// async function eliminarVendedor(id) {
//     if (!confirm("¿De verdad querés eliminar a este vendedor? Esta acción dará de baja su acceso al sistema.")) return;

//     try {
//         const response = await fetch(`/Admin/EliminarVendedor?id=${id}`, {
//             method: 'DELETE',
//             headers: {
//                 'Authorization': `Bearer ${token}`
//             }
//         });

//         if (response.ok) {
//             listarVendedores();
//         } else {
//             const data = await response.json();
//             alert(data.message || "No se pudo eliminar al vendedor.");
//         }

//     } catch (error) {
//         console.error(error);
//         alert("Error al intentar conectar con el servidor.");
//     }
// }

// // =====================================================================
// // FUNCIONES COMPLEMENTARIAS: LÓGICA DE UBICACIÓN (DATALISTS)
// // =====================================================================

// async function cargarDatalistProvinciasVendedores() {
//     try {
//         const response = await fetch('/api/Ubicacion/Provincias', {
//             method: 'GET',
//             headers: { 'Authorization': `Bearer ${token}` }
//         });
//         if (!response.ok) return;
        
//         const provincias = await response.json();
//         const datalist = document.getElementById('datalistProvinciasVendedores');
//         if (!datalist) return;
        
//         datalist.innerHTML = '';
//         provincias.forEach(p => {
//             const option = document.createElement('option');
//             option.value = p.nombre || p.Nombre;
//             datalist.appendChild(option);
//         });
//     } catch (e) {
//         console.error("Error cargando provincias en el script de vendedores", e);
//     }
// }

// async function onProvinciaVendedorChange() {
//     const provSel = document.getElementById('vendedor-provincia').value;
//     const inputCiudad = document.getElementById('vendedor-ciudad');
//     const datalistCiu = document.getElementById('datalistCiudadesVendedores');
//     const hiddenCiudadId = document.getElementById('vendedor-ciudad-id');
    
//     if (!inputCiudad || !datalistCiu) return;

//     inputCiudad.value = '';
//     if (hiddenCiudadId) hiddenCiudadId.value = "1";
//     datalistCiu.innerHTML = '';
//     listaCiudadesVendedoresMemoria = [];

//     if (!provSel.trim()) {
//         inputCiudad.disabled = true;
//         inputCiudad.placeholder = "Seleccione provincia...";
//         return;
//     }

//     await cargarCiudadesPorProvincia(provSel);
// }

// async function cargarCiudadesPorProvincia(provincia) {
//     const inputCiudad = document.getElementById('vendedor-ciudad');
//     const datalistCiu = document.getElementById('datalistCiudadesVendedores');
    
//     try {
//         const response = await fetch(`/api/Ubicacion/Ciudades?provincia=${encodeURIComponent(provincia)}`, {
//             method: 'GET',
//             headers: { 'Authorization': `Bearer ${token}` }
//         });
//         if (!response.ok) throw new Error();

//         const ciudades = await response.json();
//         listaCiudadesVendedoresMemoria = ciudades;

//         if (ciudades.length > 0) {
//             datalistCiu.innerHTML = '';
//             ciudades.forEach(c => {
//                 const option = document.createElement('option');
//                 option.value = c.nombre || c.Nombre;
//                 datalistCiu.appendChild(option);
//             });
//             inputCiudad.disabled = false;
//             inputCiudad.placeholder = "Escriba para buscar ciudad...";
//         } else {
//             inputCiudad.disabled = true;
//             inputCiudad.placeholder = "Sin ciudades disponibles.";
//         }
//     } catch (e) {
//         if (inputCiudad) {
//             inputCiudad.disabled = true;
//             inputCiudad.placeholder = "Error al cargar ciudades.";
//         }
//     }
// }

// function onCiudadVendedorSeleccionada() {
//     const ciudadEscrita = document.getElementById('vendedor-ciudad').value;
//     const hiddenId = document.getElementById('vendedor-ciudad-id');
//     if (!hiddenId) return;

//     const coincidencia = listaCiudadesVendedoresMemoria.find(c => {
//         const nom = c.nombre || c.Nombre;
//         return nom.toLowerCase() === ciudadEscrita.toLowerCase().trim();
//     });

//     if (coincidencia) {
//         hiddenId.value = coincidencia.id || coincidencia.Id;
//     } else {
//         hiddenId.value = "1"; 
//     }
// }






// ========================================================
// GESTIÓN DE VENDEDORES - JONEL AUTOS
// ========================================================

let vendedores = [];
let modalVendedorBootstrap = null;
let listaCiudadesVendedoresMemoria = []; // Almacena temporalmente las ciudades de la provincia elegida

// Función segura para obtener el token de autenticación global
const getVendedoresAuthToken = () => {
    if (typeof window.token !== 'undefined' && window.token) return window.token;
    return localStorage.getItem('jonel_token') || '';
};

// Esperar a que el DOM esté listo
document.addEventListener("DOMContentLoaded", () => {
    // 🛡️ CONTROL DE ENTORNO: Si no existe la tabla de vendedores, mitigamos la ejecución
    if (!document.getElementById('tabla-vendedores-body')) {
        return; 
    }

    const modalEl = document.getElementById('modalVendedor');
    if (modalEl) {
        modalVendedorBootstrap = new bootstrap.Modal(modalEl);
    }
});

/**
 * Función principal invocada por el ruteador de secciones del Index
 */
window.listarVendedores = async function() {
    const tbody = document.getElementById('tabla-vendedores-body');
    if (!tbody) return;

    tbody.innerHTML = `<tr><td colspan="6" class="text-center"><div class="spinner-border text-danger" role="status"></div></td></tr>`;

    try {
        const tokenVal = getVendedoresAuthToken();
        const response = await fetch('/Admin/GetVendedores', {
            method: 'GET',
            headers: {
                'Authorization': `Bearer ${tokenVal}`,
                'Content-Type': 'application/json'
            }
        });

        if (!response.ok) throw new Error("No se pudo obtener la lista de vendedores.");

        vendedores = await response.json();
        renderizarTablaVendedores(vendedores);
        
        // Carga el catálogo base de provincias al iniciar la sección
        await cargarDatalistProvinciasVendedores();

    } catch (error) {
        console.error(error);
        tbody.innerHTML = `<tr><td colspan="6" class="text-center text-danger">Error al cargar los datos: ${error.message}</td></tr>`;
    }
}

/**
 * Renderiza las filas de la tabla de vendedores extendida
 */
function renderizarTablaVendedores(lista) {
    const tbody = document.getElementById('tabla-vendedores-body');
    if (!tbody) return;

    if (lista.length === 0) {
        tbody.innerHTML = `<tr><td colspan="6" class="text-center text-muted">No hay vendedores registrados.</td></tr>`;
        return;
    }

    tbody.innerHTML = '';
    lista.forEach(v => {
        const telefonoLimpio = v.telefono ? v.telefono.replace(/\D/g, '') : '';
        
        tbody.innerHTML += `
            <tr>
                <td class="align-middle text-white fw-bold">${v.documentoIdentidad || '---'}</td>
                <td class="align-middle fw-bold">${v.apellidos}, ${v.nombres}</td>
                <td class="align-middle">
                    ${telefonoLimpio ? `
                    <a href="https://wa.me/${telefonoLimpio}" target="_blank" class="text-success text-decoration-none fw-bold">
                        <i class="bi bi-whatsapp"></i> ${v.telefono}
                    </a>` : '<span class="text-white-50 small">Sin teléfono</span>'}
                </td>
                <td class="align-middle">${v.email || '<span class="text-white-50 small">No especificado</span>'}</td>
                <td class="align-middle text-info fw-bold">${v.porcentajeComision || 0}%</td>
                <td class="align-middle text-end pe-4">
                    <button class="btn btn-sm btn-outline-warning me-1" onclick="window.editarVendedor(${v.id})">
                        <i class="bi bi-pencil"></i>
                    </button>
                    <button class="btn btn-sm btn-outline-danger" onclick="window.eliminarVendedor(${v.id})">
                        <i class="bi bi-trash"></i>
                    </button>
                </td>
            </tr>
        `;
    });
}

/**
 * Prepara el formulario del modal para un nuevo registro
 */
window.nuevoVendedor = function() {
    if (!modalVendedorBootstrap) return;

    const modalTitleEl = document.getElementById('modalVendedorTitle');
    if (modalTitleEl) {
        const span = modalTitleEl.querySelector('span');
        if (span) span.innerText = "Nuevo Vendedor";
        else modalTitleEl.innerText = "Nuevo Vendedor";
    }
    
    if (document.getElementById('vendedor-id')) document.getElementById('vendedor-id').value = "0";
    if (document.getElementById('vendedor-persona-id')) document.getElementById('vendedor-persona-id').value = "0";
    
    // 1. Personales Básicos
    if (document.getElementById('vendedor-nombre')) document.getElementById('vendedor-nombre').value = "";
    if (document.getElementById('vendedor-apellido')) document.getElementById('vendedor-apellido').value = "";
    if (document.getElementById('vendedor-documento')) document.getElementById('vendedor-documento').value = "";
    if (document.getElementById('vendedor-telefono')) document.getElementById('vendedor-telefono').value = "";
    if (document.getElementById('vendedor-email')) document.getElementById('vendedor-email').value = "";
    
    // Campos Personales a Limpiar
    if (document.getElementById('vendedor-fecha-nacimiento')) document.getElementById('vendedor-fecha-nacimiento').value = "";
    if (document.getElementById('vendedor-genero')) document.getElementById('vendedor-genero').value = "";
    if (document.getElementById('vendedor-estado-civil')) document.getElementById('vendedor-estado-civil').value = "";
    if (document.getElementById('vendedor-codigo-postal')) document.getElementById('vendedor-codigo-postal').value = "";
    
    // Control Geográfico Dinámico
    if (document.getElementById('vendedor-provincia')) document.getElementById('vendedor-provincia').value = "";
    const inputCiudad = document.getElementById('vendedor-ciudad');
    if (inputCiudad) {
        inputCiudad.value = "";
        inputCiudad.disabled = true;
        inputCiudad.placeholder = "Seleccione provincia...";
    }
    if (document.getElementById('vendedor-ciudad-id')) document.getElementById('vendedor-ciudad-id').value = "1";
    if (document.getElementById('datalistCiudadesVendedores')) document.getElementById('datalistCiudadesVendedores').innerHTML = '';
    listaCiudadesVendedoresMemoria = [];
    
    // 2. Credenciales
    const inputUser = document.getElementById('vendedor-user');
    const inputPass = document.getElementById('vendedor-password');
    if (inputUser) { inputUser.value = ""; inputUser.disabled = false; }
    if (inputPass) { inputPass.value = ""; inputPass.required = true; }
    
    const helpText = document.getElementById('vendedor-password-help');
    if (helpText) {
        helpText.style.setProperty('display', 'none', 'important');
    }
    
    // 3. Comerciales
    if (document.getElementById('vendedor-comision')) document.getElementById('vendedor-comision').value = "";
    if (document.getElementById('vendedor-observaciones')) document.getElementById('vendedor-observaciones').value = "";

    modalVendedorBootstrap.show();
}

/**
 * Carga los datos del vendedor en el formulario dinámico para su edición
 */
window.editarVendedor = async function(id) {
    const v = vendedores.find(vendedor => vendedor.id === id);
    if (!v || !modalVendedorBootstrap) return;

    const modalTitleEl = document.getElementById('modalVendedorTitle');
    if (modalTitleEl) {
        const span = modalTitleEl.querySelector('span');
        if (span) span.innerText = "Editar Vendedor";
        else modalTitleEl.innerText = "Editar Vendedor";
    }

    if (document.getElementById('vendedor-id')) document.getElementById('vendedor-id').value = v.id;
    if (document.getElementById('vendedor-persona-id')) document.getElementById('vendedor-persona-id').value = v.idPersonaId || 0;
    
    if (document.getElementById('vendedor-nombre')) document.getElementById('vendedor-nombre').value = v.nombres || "";
    if (document.getElementById('vendedor-apellido')) document.getElementById('vendedor-apellido').value = v.apellidos || "";
    if (document.getElementById('vendedor-documento')) document.getElementById('vendedor-documento').value = v.documentoIdentidad || "";
    if (document.getElementById('vendedor-telefono')) document.getElementById('vendedor-telefono').value = v.telefono || "";
    if (document.getElementById('vendedor-email')) document.getElementById('vendedor-email').value = v.email || "";
    
    if (v.fechaNacimiento) {
        if (document.getElementById('vendedor-fecha-nacimiento')) document.getElementById('vendedor-fecha-nacimiento').value = v.fechaNacimiento.split('T')[0];
    } else {
        if (document.getElementById('vendedor-fecha-nacimiento')) document.getElementById('vendedor-fecha-nacimiento').value = "";
    }
    if (document.getElementById('vendedor-genero')) document.getElementById('vendedor-genero').value = v.genero || "";
    if (document.getElementById('vendedor-estado-civil')) document.getElementById('vendedor-estado-civil').value = v.estadoCivil || "";
    if (document.getElementById('vendedor-codigo-postal')) document.getElementById('vendedor-codigo-postal').value = v.codigoPostal || "";
    
    // 2. CONFIGURACIÓN GEOGRÁFICA
    const provActual = v.estadoProvincia || v.provincia || "";
    if (document.getElementById('vendedor-provincia')) document.getElementById('vendedor-provincia').value = provActual;
    
    const inputCiudad = document.getElementById('vendedor-ciudad');
    if (inputCiudad) {
        if (provActual) {
            await cargarCiudadesPorProvincia(provActual);
            inputCiudad.value = v.nombreCiudad || v.ciudad || ""; 
            if (document.getElementById('vendedor-ciudad-id')) document.getElementById('vendedor-ciudad-id').value = v.ciudadId || "1";
            inputCiudad.disabled = false;
        } else {
            inputCiudad.value = "";
            if (document.getElementById('vendedor-ciudad-id')) document.getElementById('vendedor-ciudad-id').value = "1";
            inputCiudad.disabled = true;
        }
    }
    
    // 3. Credenciales
    const inputUser = document.getElementById('vendedor-user');
    const inputPass = document.getElementById('vendedor-password');
    if (inputUser) { inputUser.value = v.nombreUsuario || v.userName || ""; inputUser.disabled = true; }
    if (inputPass) { inputPass.value = ""; inputPass.required = false; }
    
    const helpText = document.getElementById('vendedor-password-help');
    if (helpText) {
        helpText.style.setProperty('display', 'block', 'important');
    }
    
    // 4. Comerciales
    if (document.getElementById('vendedor-comision')) document.getElementById('vendedor-comision').value = v.porcentajeComision !== undefined ? v.porcentajeComision : (v.comision || 0);
    if (document.getElementById('vendedor-observaciones')) document.getElementById('vendedor-observaciones').value = v.observaciones || "";

    modalVendedorBootstrap.show();
}

/**
 * Envía la estructura completa unificada al Backend
 */
window.guardarVendedor = async function(event) {
    if (event) event.preventDefault();

    const id = parseInt(document.getElementById('vendedor-id')?.value) || 0;
    const fechaNacVal = document.getElementById('vendedor-fecha-nacimiento')?.value;
    
    const model = {
        id: id,
        idPersonaId: parseInt(document.getElementById('vendedor-persona-id')?.value) || 0,
        documentoIdentidad: document.getElementById('vendedor-documento')?.value.trim() || "",
        nombres: document.getElementById('vendedor-nombre')?.value.trim() || "",
        apellidos: document.getElementById('vendedor-apellido')?.value.trim() || "",
        email: document.getElementById('vendedor-email')?.value.trim() || "",
        telefono: document.getElementById('vendedor-telefono')?.value.trim() || "",
        fechaNacimiento: fechaNacVal ? fechaNacVal : null,
        genero: document.getElementById('vendedor-genero')?.value || "",
        estadoCivil: document.getElementById('vendedor-estado-civil')?.value || "",
        estadoProvincia: document.getElementById('vendedor-provincia')?.value.trim() || "",
        ciudadId: parseInt(document.getElementById('vendedor-ciudad-id')?.value) || 1,
        codigoPostal: document.getElementById('vendedor-codigo-postal')?.value.trim() || "",
        nombreUsuario: document.getElementById('vendedor-user')?.value.trim() || "",
        password: document.getElementById('vendedor-password')?.value || "", 
        porcentajeComision: parseFloat(document.getElementById('vendedor-comision')?.value) || 0,
        observaciones: document.getElementById('vendedor-observaciones')?.value.trim() || ""
    };

    try {
        const tokenVal = getVendedoresAuthToken();
        const response = await fetch('/Admin/GuardarVendedor', {
            method: 'POST',
            headers: {
                'Authorization': `Bearer ${tokenVal}`,
                'Content-Type': 'application/json'
            },
            body: JSON.stringify(model)
        });

        const data = await response.json();

        if (response.ok) {
            if (modalVendedorBootstrap) modalVendedorBootstrap.hide();
            window.listarVendedores(); 
        } else {
            alert(data.message || "Ocurrió un error al procesar la solicitud.");
        }

    } catch (error) {
        console.error(error);
        alert("Error de comunicación con el servidor.");
    }
}

/**
 * Envía el ID para dar de baja el registro
 */
window.eliminarVendedor = async function(id) {
    if (!confirm("¿De verdad querés eliminar a este vendedor? Esta acción dará de baja su acceso al sistema.")) return;

    try {
        const tokenVal = getVendedoresAuthToken();
        const response = await fetch(`/Admin/EliminarVendedor?id=${id}`, {
            method: 'DELETE',
            headers: {
                'Authorization': `Bearer ${tokenVal}`
            }
        });

        if (response.ok) {
            window.listarVendedores();
        } else {
            const data = await response.json();
            alert(data.message || "No se pudo eliminar al vendedor.");
        }

    } catch (error) {
        console.error(error);
        alert("Error al intentar conectar con el servidor.");
    }
}

// =====================================================================
// LÓGICA DE UBICACIÓN (DATALISTS)
// =====================================================================

async function cargarDatalistProvinciasVendedores() {
    try {
        const tokenVal = getVendedoresAuthToken();
        const response = await fetch('/api/Ubicacion/Provincias', {
            method: 'GET',
            headers: { 'Authorization': `Bearer ${tokenVal}` }
        });
        if (!response.ok) return;
        
        const provincias = await response.json();
        const datalist = document.getElementById('datalistProvinciasVendedores');
        if (!datalist) return;
        
        datalist.innerHTML = '';
        provincias.forEach(p => {
            const option = document.createElement('option');
            option.value = p.nombre || p.Nombre;
            datalist.appendChild(option);
        });
    } catch (e) {
        console.error("Error cargando provincias en el script de vendedores", e);
    }
}

window.onProvinciaVendedorChange = async function() {
    const provSel = document.getElementById('vendedor-provincia')?.value || '';
    const inputCiudad = document.getElementById('vendedor-ciudad');
    const datalistCiu = document.getElementById('datalistCiudadesVendedores');
    const hiddenCiudadId = document.getElementById('vendedor-ciudad-id');
    
    if (!inputCiudad || !datalistCiu) return;

    inputCiudad.value = '';
    if (hiddenCiudadId) hiddenCiudadId.value = "1";
    datalistCiu.innerHTML = '';
    listaCiudadesVendedoresMemoria = [];

    if (!provSel.trim()) {
        inputCiudad.disabled = true;
        inputCiudad.placeholder = "Seleccione provincia...";
        return;
    }

    await cargarCiudadesPorProvincia(provSel);
}

async function cargarCiudadesPorProvincia(provincia) {
    const inputCiudad = document.getElementById('vendedor-ciudad');
    const datalistCiu = document.getElementById('datalistCiudadesVendedores');
    
    try {
        const tokenVal = getVendedoresAuthToken();
        const response = await fetch(`/api/Ubicacion/Ciudades?provincia=${encodeURIComponent(provincia)}`, {
            method: 'GET',
            headers: { 'Authorization': `Bearer ${tokenVal}` }
        });
        if (!response.ok) throw new Error();

        const ciudades = await response.json();
        listaCiudadesVendedoresMemoria = ciudades;

        if (ciudades.length > 0) {
            if (datalistCiu) {
                datalistCiu.innerHTML = '';
                ciudades.forEach(c => {
                    const option = document.createElement('option');
                    option.value = c.nombre || c.Nombre;
                    datalistCiu.appendChild(option);
                });
            }
            if (inputCiudad) {
                inputCiudad.disabled = false;
                inputCiudad.placeholder = "Escriba para buscar ciudad...";
            }
        } else {
            if (inputCiudad) {
                inputCiudad.disabled = true;
                inputCiudad.placeholder = "Sin ciudades disponibles.";
            }
        }
    } catch (e) {
        if (inputCiudad) {
            inputCiudad.disabled = true;
            inputCiudad.placeholder = "Error al cargar ciudades.";
        }
    }
}

window.onCiudadVendedorSeleccionada = function() {
    const ciudadEscrita = document.getElementById('vendedor-ciudad')?.value || '';
    const hiddenId = document.getElementById('vendedor-ciudad-id');
    if (!hiddenId) return;

    const coincidencia = listaCiudadesVendedoresMemoria.find(c => {
        const nom = c.nombre || c.Nombre;
        return nom.toLowerCase() === ciudadEscrita.toLowerCase().trim();
    });

    if (coincidencia) {
        hiddenId.value = coincidencia.id || coincidencia.Id;
    } else {
        hiddenId.value = "1"; 
    }
}