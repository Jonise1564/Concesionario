
// // ========================================================
// // GESTIÓN DE VENDEDORES - JONEL AUTOS
// // ========================================================

// // Variables globales para la sección
// let vendedores = [];
// let modalVendedorBootstrap = null;

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

//         if (!response.ok) {
//             if (response.status === 401 || response.status === 403) {
//                 alert("Sesión expirada o no tenés permisos de administrador.");
//                 window.location.href = '/Home/Acceso';
//                 return;
//             }
//             throw new Error("No se pudieron cargar los vendedores.");
//         }

//         vendedores = await response.json();
//         renderizarTablaVendedores(vendedores);

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
//         tbody.innerHTML += `
//             <tr>
//                 <td class="align-middle text-muted small">${v.documentoIdentidad}</td>
//                 <td class="align-middle fw-bold">${v.apellidos}, ${v.nombres}</td>
//                 <td class="align-middle">
//                     <a href="https://wa.me/${v.telefono.replace(/\D/g, '')}" target="_blank" class="text-success text-decoration-none fw-bold">
//                         <i class="bi bi-whatsapp"></i> ${v.telefono}
//                     </a>
//                 </td>
//                 <td class="align-middle">${v.email || '<span class="text-muted small">No especificado</span>'}</td>
//                 <td class="align-middle text-info fw-bold">${v.porcentajeComision}%</td>
//                 <td class="align-middle text-end">
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
//  * Prepara el formulario del modal para un nuevo registro (Limpia los 14 campos)
//  */
// function nuevoVendedor() {
//     if (!modalVendedorBootstrap) return;

//     document.getElementById('modalVendedorTitle').innerText = "Nuevo Vendedor";
//     document.getElementById('vendedor-id').value = "0";
    
//     // 1. Personales Básicos
//     document.getElementById('vendedor-nombre').value = "";
//     document.getElementById('vendedor-apellido').value = "";
//     document.getElementById('vendedor-documento').value = "";
//     document.getElementById('vendedor-telefono').value = "";
//     document.getElementById('vendedor-email').value = "";
    
//     // Nuevos Campos Personales a Limpiar
//     document.getElementById('vendedor-fecha-nacimiento').value = "";
//     document.getElementById('vendedor-genero').value = "";
//     document.getElementById('vendedor-estado-civil').value = "";
//     document.getElementById('vendedor-provincia').value = "";
//     document.getElementById('vendedor-codigo-postal').value = "";
    
//     // 2. Credenciales (Obligatoria para nuevos)
//     const inputUser = document.getElementById('vendedor-user');
//     const inputPass = document.getElementById('vendedor-password');
//     inputUser.value = "";
//     inputUser.disabled = false;
//     inputPass.value = "";
//     inputPass.required = true; 
    
//     // 3. Comerciales
//     document.getElementById('vendedor-comision').value = "";
//     document.getElementById('vendedor-observaciones').value = "";

//     modalVendedorBootstrap.show();
// }

// /**
//  * Carga los datos del vendedor en el formulario dinámico para su edición
//  */
// function editarVendedor(id) {
//     const v = vendedores.find(vendedor => vendedor.id === id);
//     if (!v || !modalVendedorBootstrap) return;

//     document.getElementById('modalVendedorTitle').innerText = "Editar Vendedor";
//     document.getElementById('vendedor-id').value = v.id;
    
//     // 1. Personales Básicos
//     document.getElementById('vendedor-nombre').value = v.nombres;
//     document.getElementById('vendedor-apellido').value = v.apellidos;
//     document.getElementById('vendedor-documento').value = v.documentoIdentidad;
//     document.getElementById('vendedor-telefono').value = v.telefono;
//     document.getElementById('vendedor-email').value = v.email || "";
    
//     // Carga de Nuevos Campos Personales (Controlando nulos si vienen de la BD)
//     if (v.fechaNacimiento) {
//         // Corta el formato ISO (YYYY-MM-DDTHH:mm:ss) a YYYY-MM-DD para el input de tipo date
//         document.getElementById('vendedor-fecha-nacimiento').value = v.fechaNacimiento.split('T')[0];
//     } else {
//         document.getElementById('vendedor-fecha-nacimiento').value = "";
//     }
//     document.getElementById('vendedor-genero').value = v.genero || "";
//     document.getElementById('vendedor-estado-civil').value = v.estadoCivil || "";
//     document.getElementById('vendedor-provincia').value = v.provincia || "";
//     document.getElementById('vendedor-codigo-postal').value = v.codigoPostal || "";
    
//     // 2. Credenciales (El usuario no se debería cambiar para mantener integridad)
//     const inputUser = document.getElementById('vendedor-user');
//     const inputPass = document.getElementById('vendedor-password');
//     inputUser.value = v.nombreUsuario;
//     inputUser.disabled = true; 
//     inputPass.value = ""; 
//     inputPass.required = false; // Opcional al editar si no se quiere cambiar la contraseña
    
//     // 3. Comerciales
//     document.getElementById('vendedor-comision').value = v.porcentajeComision;
//     document.getElementById('vendedor-observaciones').value = v.observaciones || "";

//     modalVendedorBootstrap.show();
// }

// /**
//  * Envía la estructura completa unificada al Backend (VendedorRegistroDto)
//  */
// async function guardarVendedor(event) {
//     if (event) event.preventDefault();

//     const id = parseInt(document.getElementById('vendedor-id').value) || 0;
    
//     // Captura el valor de la fecha; si está vacío, enviamos null
//     const fechaNacVal = document.getElementById('vendedor-fecha-nacimiento').value;
    
//     // Armamos el objeto con la estructura exacta que espera el DTO de C#
//     const model = {
//         id: id,
//         documentoIdentidad: document.getElementById('vendedor-documento').value.trim(),
//         nombres: document.getElementById('vendedor-nombre').value.trim(),
//         apellidos: document.getElementById('vendedor-apellido').value.trim(),
//         email: document.getElementById('vendedor-email').value.trim(),
//         telefono: document.getElementById('vendedor-telefono').value.trim(),
        
//         // Mapeo de nuevas propiedades al modelo JSON
//         fechaNacimiento: fechaNacVal ? fechaNacVal : null,
//         genero: document.getElementById('vendedor-genero').value,
//         estadoCivil: document.getElementById('vendedor-estado-civil').value,
//         provincia: document.getElementById('vendedor-provincia').value,
//         codigoPostal: document.getElementById('vendedor-codigo-postal').value.trim(),
        
//         nombreUsuario: document.getElementById('vendedor-user').value.trim(),
//         password: document.getElementById('vendedor-password').value, // Puede ir vacío al editar
//         porcentajeComision: parseFloat(document.getElementById('vendedor-comision').value) || 0,
//         observaciones: document.getElementById('vendedor-observaciones').value.trim()
//     };

//     try {
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
//             modalVendedorBootstrap.hide();
//             listarVendedores(); // Refrescar la grilla principal
//         } else {
//             alert(data.message || "Ocurrió un error al procesar el alta.");
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






// ========================================================
// GESTIÓN DE VENDEDORES - JONEL AUTOS
// ========================================================

// Variables globales para la sección
let vendedores = [];
let modalVendedorBootstrap = null;

// Esperar a que el DOM esté listo para inicializar el modal si existe
document.addEventListener("DOMContentLoaded", () => {
    const modalEl = document.getElementById('modalVendedor');
    if (modalEl) {
        modalVendedorBootstrap = new bootstrap.Modal(modalEl);
    }
});

/**
 * Función principal invocada por el ruteador de secciones del Index
 */
async function listarVendedores() {
    const tbody = document.getElementById('tabla-vendedores-body');
    if (!tbody) return;

    tbody.innerHTML = `<tr><td colspan="6" class="text-center"><div class="spinner-border text-danger" role="status"></div></td></tr>`;

    try {
        const response = await fetch('/Admin/GetVendedores', {
            method: 'GET',
            headers: {
                'Authorization': `Bearer ${token}`,
                'Content-Type': 'application/json'
            }
        });

        if (!response.ok) {
            if (response.status === 401 || response.status === 403) {
                alert("Sesión expirada o no tenés permisos de administrador.");
                window.location.href = '/Home/Acceso';
                return;
            }
            throw new Error("No se pudieron cargar los vendedores.");
        }

        vendedores = await response.json();
        renderizarTablaVendedores(vendedores);

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
        tbody.innerHTML += `
            <tr>
                <td class="align-middle text-white fw-bold">${v.documentoIdentidad || '---'}</td>
                <td class="align-middle fw-bold">${v.apellidos}, ${v.nombres}</td>
                <td class="align-middle">
                    <a href="https://wa.me/${v.telefono.replace(/\D/g, '')}" target="_blank" class="text-success text-decoration-none fw-bold">
                        <i class="bi bi-whatsapp"></i> ${v.telefono}
                    </a>
                </td>
                <td class="align-middle">${v.email || '<span class="text-white-50 small">No especificado</span>'}</td>
                <td class="align-middle text-info fw-bold">${v.porcentajeComision}%</td>
                <td class="align-middle text-end">
                    <button class="btn btn-sm btn-outline-warning me-1" onclick="editarVendedor(${v.id})">
                        <i class="bi bi-pencil"></i>
                    </button>
                    <button class="btn btn-sm btn-outline-danger" onclick="eliminarVendedor(${v.id})">
                        <i class="bi bi-trash"></i>
                    </button>
                </td>
            </tr>
        `;
    });
}

/**
 * Prepara el formulario del modal para un nuevo registro (Limpia los 14 campos)
 */
function nuevoVendedor() {
    if (!modalVendedorBootstrap) return;

    document.getElementById('modalVendedorTitle').innerText = "Nuevo Vendedor";
    document.getElementById('vendedor-id').value = "0";
    
    // 1. Personales Básicos
    document.getElementById('vendedor-nombre').value = "";
    document.getElementById('vendedor-apellido').value = "";
    document.getElementById('vendedor-documento').value = "";
    document.getElementById('vendedor-telefono').value = "";
    document.getElementById('vendedor-email').value = "";
    
    // Nuevos Campos Personales a Limpiar
    document.getElementById('vendedor-fecha-nacimiento').value = "";
    document.getElementById('vendedor-genero').value = "";
    document.getElementById('vendedor-estado-civil').value = "";
    document.getElementById('vendedor-provincia').value = "";
    document.getElementById('vendedor-codigo-postal').value = "";
    
    // 2. Credenciales (Obligatoria para nuevos)
    const inputUser = document.getElementById('vendedor-user');
    const inputPass = document.getElementById('vendedor-password');
    inputUser.value = "";
    inputUser.disabled = false;
    inputPass.value = "";
    inputPass.required = true; 
    
    // CORRECCIÓN: Ocultar el texto de ayuda de la contraseña en altas
    const helpText = document.getElementById('vendedor-password-help');
    if (helpText) {
        helpText.style.setProperty('display', 'none', 'important');
    }
    
    // 3. Comerciales
    document.getElementById('vendedor-comision').value = "";
    document.getElementById('vendedor-observaciones').value = "";

    modalVendedorBootstrap.show();
}

/**
 * Carga los datos del vendedor en el formulario dinámico para su edición
 */
function editarVendedor(id) {
    const v = vendedores.find(vendedor => vendedor.id === id);
    if (!v || !modalVendedorBootstrap) return;

    document.getElementById('modalVendedorTitle').innerText = "Editar Vendedor";
    document.getElementById('vendedor-id').value = v.id;
    
    // 1. Personales Básicos
    document.getElementById('vendedor-nombre').value = v.nombres;
    document.getElementById('vendedor-apellido').value = v.apellidos;
    document.getElementById('vendedor-documento').value = v.documentoIdentidad;
    document.getElementById('vendedor-telefono').value = v.telefono;
    document.getElementById('vendedor-email').value = v.email || "";
    
    // Carga de Nuevos Campos Personales (Controlando nulos si vienen de la BD)
    if (v.fechaNacimiento) {
        // Corta el formato ISO (YYYY-MM-DDTHH:mm:ss) a YYYY-MM-DD para el input de tipo date
        document.getElementById('vendedor-fecha-nacimiento').value = v.fechaNacimiento.split('T')[0];
    } else {
        document.getElementById('vendedor-fecha-nacimiento').value = "";
    }
    document.getElementById('vendedor-genero').value = v.genero || "";
    document.getElementById('vendedor-estado-civil').value = v.estadoCivil || "";
    document.getElementById('vendedor-provincia').value = v.provincia || "";
    document.getElementById('vendedor-codigo-postal').value = v.codigoPostal || "";
    
    // 2. Credenciales (El usuario no se debería cambiar para mantener integridad)
    const inputUser = document.getElementById('vendedor-user');
    const inputPass = document.getElementById('vendedor-password');
    inputUser.value = v.nombreUsuario;
    inputUser.disabled = true; 
    inputPass.value = ""; 
    inputPass.required = false; // Opcional al editar si no se quiere cambiar la contraseña
    
    // CORRECCIÓN: Mostrar el texto de ayuda de la contraseña al editar
    const helpText = document.getElementById('vendedor-password-help');
    if (helpText) {
        helpText.style.setProperty('display', 'block', 'important');
    }
    
    // 3. Comerciales
    document.getElementById('vendedor-comision').value = v.porcentajeComision;
    document.getElementById('vendedor-observaciones').value = v.observaciones || "";

    modalVendedorBootstrap.show();
}

/**
 * Envía la estructura completa unificada al Backend (VendedorRegistroDto)
 */
async function guardarVendedor(event) {
    if (event) event.preventDefault();

    const id = parseInt(document.getElementById('vendedor-id').value) || 0;
    
    // Captura el valor de la fecha; si está vacío, enviamos null
    const fechaNacVal = document.getElementById('vendedor-fecha-nacimiento').value;
    
    // Armamos el objeto con la estructura exacta que espera el DTO de C#
    const model = {
        id: id,
        documentoIdentidad: document.getElementById('vendedor-documento').value.trim(),
        nombres: document.getElementById('vendedor-nombre').value.trim(),
        apellidos: document.getElementById('vendedor-apellido').value.trim(),
        email: document.getElementById('vendedor-email').value.trim(),
        telefono: document.getElementById('vendedor-telefono').value.trim(),
        
        // Mapeo de nuevas propiedades al modelo JSON
        fechaNacimiento: fechaNacVal ? fechaNacVal : null,
        genero: document.getElementById('vendedor-genero').value,
        estadoCivil: document.getElementById('vendedor-estado-civil').value,
        provincia: document.getElementById('vendedor-provincia').value,
        codigoPostal: document.getElementById('vendedor-codigo-postal').value.trim(),
        
        nombreUsuario: document.getElementById('vendedor-user').value.trim(),
        password: document.getElementById('vendedor-password').value, // Puede ir vacío al editar
        porcentajeComision: parseFloat(document.getElementById('vendedor-comision').value) || 0,
        observaciones: document.getElementById('vendedor-observaciones').value.trim()
    };

    try {
        const response = await fetch('/Admin/GuardarVendedor', {
            method: 'POST',
            headers: {
                'Authorization': `Bearer ${token}`,
                'Content-Type': 'application/json'
            },
            body: JSON.stringify(model)
        });

        const data = await response.json();

        if (response.ok) {
            modalVendedorBootstrap.hide();
            listarVendedores(); // Refrescar la grilla principal
        } else {
            alert(data.message || "Ocurrió un error al procesar el alta.");
        }

    } catch (error) {
        console.error(error);
        alert("Error de comunicación con el servidor.");
    }
}

/**
 * Envía el ID para dar de baja el registro
 */
async function eliminarVendedor(id) {
    if (!confirm("¿De verdad querés eliminar a este vendedor? Esta acción dará de baja su acceso al sistema.")) return;

    try {
        const response = await fetch(`/Admin/EliminarVendedor?id=${id}`, {
            method: 'DELETE',
            headers: {
                'Authorization': `Bearer ${token}`
            }
        });

        if (response.ok) {
            listarVendedores();
        } else {
            const data = await response.json();
            alert(data.message || "No se pudo eliminar al vendedor.");
        }

    } catch (error) {
        console.error(error);
        alert("Error al intentar conectar con el servidor.");
    }
}