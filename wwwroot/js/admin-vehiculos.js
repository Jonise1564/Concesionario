// // Variable global para interactuar con el Modal de Bootstrap
// let myModalVehiculo;

// document.addEventListener("DOMContentLoaded", () => {
//     const modalEl = document.getElementById('modalVehiculo');
//     if (modalEl) {
//         myModalVehiculo = new bootstrap.Modal(modalEl);
//     }
// });

// // --- LÓGICA DE VEHÍCULOS ---
// async function listar() {
//     try {
//         const resp = await fetch('/Admin/GetVehiculos', {
//             method: 'GET',
//             headers: { 'Authorization': `Bearer ${token}` }
//         });

//         if (resp.status === 401) {
//             window.location.href = '/Home/Acceso';
//             return;
//         }

//         if (!resp.ok) throw new Error("Error en el servidor: " + resp.status);

//         const data = await resp.json();
//         const cuerpo = document.getElementById('tablaCuerpo');
//         const listaVehiculos = data.items ? data.items : data;

//         if (!listaVehiculos || listaVehiculos.length === 0) {
//             cuerpo.innerHTML = '<tr><td colspan="8" class="text-center text-muted p-4">No hay vehículos en el inventario</td></tr>';
//             return;
//         }

//         cuerpo.innerHTML = listaVehiculos.map(v => {
//             let imgPath = 'https://placehold.co/60x40/00?text=S/F';
//             if (v.imagenUrl) {
//                 imgPath = v.imagenUrl.startsWith('http') ? v.imagenUrl : `/img/cars/${v.imagenUrl}`;
//             }

//             return `
//                 <tr class="align-middle">
//                     <td>
//                         <img src="${imgPath}" class="img-thumb-table" onerror="this.src='https://placehold.co/60x40/00?text=S/F'">
//                     </td>
//                     <td>
//                         <div class="form-check form-switch">
//                             <input class="form-check-input" type="checkbox" ${v.activo ? 'checked' : ''} onclick="toggleEstado(${v.id})">
//                         </div>
//                     </td>
//                     <td class="fw-bold text-white">${v.marca}</td>
//                     <td class="text-white">${v.modelo} <br><small class="text-muted">${v.version || ''}</small></td>
//                     <td class="text-white">${v.anio}</td>
//                     <td class="text-danger fw-bold">$ ${v.precio.toLocaleString()}</td>
//                     <td class="text-white">${v.stock}</td>
//                     <td>
//                         <button class="btn btn-sm btn-outline-light me-2" onclick='editar(${JSON.stringify(v)})'>
//                             <i class="bi bi-pencil"></i>
//                         </button>
//                         <button class="btn btn-sm btn-outline-danger" onclick="eliminar(${v.id})">
//                             <i class="bi bi-trash"></i>
//                         </button>
//                     </td>
//                 </tr>
//             `;
//         }).join('');
//     } catch (err) { 
//         console.error("Error al listar:", err);
//         document.getElementById('tablaCuerpo').innerHTML = `<tr><td colspan="8" class="text-center text-danger">Error de conexión: ${err.message}</td></tr>`;
//     }
// }

// function seleccionarArchivo(input) {
//     if (input.files && input.files[0]) {
//         const reader = new FileReader();
//         reader.onload = function(e) {
//             document.getElementById('imgPreview').src = e.target.result;
//         }
//         reader.readAsDataURL(input.files[0]);
//     }
// }

// function actualizarPreview() {
//     const nombreArchivo = document.getElementById('vImagenUrl').value;
//     const imgElement = document.getElementById('imgPreview');
    
//     if (nombreArchivo) {
//         imgElement.src = nombreArchivo.startsWith('http') ? nombreArchivo : `/img/cars/${nombreArchivo}`;
//     } else {
//         imgElement.src = 'https://placehold.co/400x300/000000/FFFFFF?text=Sin+Imagen';
//     }
// }

// async function toggleEstado(id) {
//     try {
//         const resp = await fetch(`/Admin/CambiarEstado?id=${id}`, {
//             method: 'POST',
//             headers: { 'Authorization': `Bearer ${token}` }
//         });
//         if (!resp.ok) listar(); 
//     } catch (err) { 
//         console.error(err);
//         listar();
//     }
// }

// function abrirModal() {
//     document.getElementById('modalTitulo').innerText = "NUEVO INGRESO DE VEHÍCULO";
//     document.getElementById('formVehiculo').reset();
//     document.getElementById('vId').value = "0";
//     document.getElementById('vImagenUrl').value = "";
//     document.getElementById('vFotoFile').value = "";
//     document.getElementById('vActivo').checked = true;
//     actualizarPreview();
//     myModalVehiculo?.show();
// }

// function editar(v) {
//     document.getElementById('modalTitulo').innerText = "MODIFICAR VEHÍCULO";
//     document.getElementById('vId').value = v.id;
//     document.getElementById('vMarca').value = v.marca;
//     document.getElementById('vModelo').value = v.modelo;
//     document.getElementById('vVersion').value = v.version || '';
//     document.getElementById('vAnio').value = v.anio;
//     document.getElementById('vKilometros').value = v.kilometros;
//     document.getElementById('vPrecio').value = v.precio;
//     document.getElementById('vStock').value = v.stock;
//     document.getElementById('vCombustible').value = v.combustible;
//     document.getElementById('vTransmision').value = v.transmision;
//     document.getElementById('vCategoriaId').value = v.categoriaId;
//     document.getElementById('vImagenUrl').value = v.imagenUrl || '';
//     document.getElementById('vActivo').checked = v.activo;
//     document.getElementById('vFotoFile').value = ""; 
//     actualizarPreview();
//     myModalVehiculo?.show();
// }

// async function guardar() {
//     const form = document.getElementById('formVehiculo');
//     if(!form.checkValidity()) {
//         form.reportValidity();
//         return;
//     }

//     const formData = new FormData(form);
//     formData.set('Activo', document.getElementById('vActivo').checked);

//     try {
//         const response = await fetch('/Admin/Guardar', {
//             method: 'POST',
//             headers: { 'Authorization': `Bearer ${token}` },
//             body: formData
//         });

//         if (response.ok) {
//             myModalVehiculo?.hide();
//             listar();
//         } else {
//             const error = await response.json();
//             alert("Error: " + (error.message || "No se pudo guardar."));
//         }
//     } catch (err) {
//         alert("Error de conexión al guardar.");
//     }
// }

// async function eliminar(id) {
//     if (confirm("¿Eliminar definitivamente este vehículo?")) {
//         try {
//             const resp = await fetch(`/Admin/Eliminar?id=${id}`, {
//                 method: 'DELETE',
//                 headers: { 'Authorization': `Bearer ${token}` }
//             });
//             if(resp.ok) listar();
//         } catch (err) {
//             alert("Error al eliminar.");
//         }
//     }
// }

// function filtrarVehiculos() {
//     // Implementación opcional para búsquedas del lado del cliente
//     console.log("Filtrando...");
// }


// 💾 Variables globales para interactuar con el inventario y modales
let myModalVehiculo;
let cacheVehiculos = []; // Guardará el stock original para filtrado local instantáneo

document.addEventListener("DOMContentLoaded", () => {
    const modalEl = document.getElementById('modalVehiculo');
    if (modalEl) {
        myModalVehiculo = new bootstrap.Modal(modalEl);
    }
    
    // Si hay un token válido, cargamos los datos estructurales iniciales
    if (localStorage.getItem('jonel_token')) {
        cargarCategorias();
    }
});

// --- 🏷️ CARGA DINÁMICA DE CATEGORÍAS (SELECTS) ---
async function cargarCategorias() {
    try {
        const resp = await fetch('/Admin/GetCategorias', { // Reemplaza por tu endpoint real si difiere
            method: 'GET',
            headers: { 'Authorization': `Bearer ${token}` }
        });

        if (resp.ok) {
            const categorias = await resp.json();
            const selectModal = document.getElementById('vCategoriaId');

            if (selectModal) {
                // Mantenemos la opción deshabilitada por defecto
                selectModal.innerHTML = '<option value="" disabled selected>Seleccione una categoría...</option>';
                categorias.forEach(cat => {
                    selectModal.innerHTML += `<option value="${cat.id}">${cat.nombre}</option>`;
                });
            }
        }
    } catch (err) {
        console.error("Error al cargar el listado de categorías:", err);
    }
}

// --- 📊 LÓGICA DE VEHÍCULOS ---
async function listar() {
    try {
        const resp = await fetch('/Admin/GetVehiculos', {
            method: 'GET',
            headers: { 'Authorization': `Bearer ${token}` }
        });

        if (resp.status === 401) {
            window.location.href = '/Home/Acceso';
            return;
        }

        if (!resp.ok) throw new Error("Error en el servidor: " + resp.status);

        const data = await resp.json();
        
        // Almacenamos los ítems en caché para poder usar búsquedas ultra rápidas
        cacheVehiculos = data.items ? data.items : data;

        // Inyectamos el set inicial en la tabla
        inyectarTablaVehiculos(cacheVehiculos);

    } catch (err) { 
        console.error("Error al listar:", err);
        document.getElementById('tablaCuerpo').innerHTML = `<tr><td colspan="8" class="text-center text-danger">Error de conexión: ${err.message}</td></tr>`;
    }
}

// 🖨️ FUNCIÓN AUXILIAR PARA RENDERIZAR LAS FILAS DE LA TABLA
function inyectarTablaVehiculos(lista) {
    const cuerpo = document.getElementById('tablaCuerpo');

    if (!lista || lista.length === 0) {
        cuerpo.innerHTML = '<tr><td colspan="8" class="text-center text-muted p-4">No hay vehículos coincidentes</td></tr>';
        return;
    }

    cuerpo.innerHTML = lista.map(v => {
        let imgPath = 'https://placehold.co/60x40/00?text=S/F';
        if (v.imagenUrl) {
            imgPath = v.imagenUrl.startsWith('http') ? v.imagenUrl : `/img/cars/${v.imagenUrl}`;
        }

        return `
            <tr class="align-middle">
                <td>
                    <img src="${imgPath}" class="img-thumb-table" onerror="this.src='https://placehold.co/60x40/00?text=S/F'">
                </td>
                <td>
                    <div class="form-check form-switch">
                        <input class="form-check-input" type="checkbox" ${v.activo ? 'checked' : ''} onclick="toggleEstado(${v.id})">
                    </div>
                </td>
                <td class="fw-bold text-white">${v.marca}</td>
                <td class="text-white">${v.modelo} <br><small class="text-muted">${v.version || ''}</small></td>
                <td class="text-white">${v.anio}</td>
                <td class="text-danger fw-bold">$ ${v.precio.toLocaleString()}</td>
                <td class="text-white">${v.stock}</td>
                <td>
                    <button class="btn btn-sm btn-outline-light me-2" onclick='editar(${JSON.stringify(v)})'>
                        <i class="bi bi-pencil"></i>
                    </button>
                    <button class="btn btn-sm btn-outline-danger" onclick="eliminar(${v.id})">
                        <i class="bi bi-trash"></i>
                    </button>
                </td>
            </tr>
        `;
    }).join('');
}

// ⚡ FILTRADO COMBINADO EN TIEMPO REAL (TEXTO + ESTADO VISIBLE)
function filtrarVehiculos() {
    const busqueda = document.getElementById('buscarVehiculo').value.toLowerCase().trim();
    const filtroEstado = document.getElementById('filtrarEstado').value;

    const resultado = cacheVehiculos.filter(v => {
        // 1. Macheo por texto (Marca, Modelo o Versión)
        const cumpleTexto = 
            v.marca.toLowerCase().includes(busqueda) || 
            v.modelo.toLowerCase().includes(busqueda) || 
            (v.version && v.version.toLowerCase().includes(busqueda));

        // 2. Macheo por visibilidad web
        let cumpleEstado = true;
        if (filtroEstado === 'activos') cumpleEstado = v.activo === true;
        if (filtroEstado === 'inactivos') cumpleEstado = v.activo === false;

        return cumpleTexto && cumpleEstado;
    });

    inyectarTablaVehiculos(resultado);
}

function seleccionarArchivo(input) {
    if (input.files && input.files[0]) {
        const reader = new FileReader();
        reader.onload = function(e) {
            document.getElementById('imgPreview').src = e.target.result;
        }
        reader.readAsDataURL(input.files[0]);
    }
}

function actualizarPreview() {
    const nombreArchivo = document.getElementById('vImagenUrl').value;
    const imgElement = document.getElementById('imgPreview');
    
    if (nombreArchivo) {
        imgElement.src = nombreArchivo.startsWith('http') ? nombreArchivo : `/img/cars/${nombreArchivo}`;
    } else {
        imgElement.src = 'https://placehold.co/400x300/000000/FFFFFF?text=Sin+Imagen';
    }
}

async function toggleEstado(id) {
    try {
        const resp = await fetch(`/Admin/CambiarEstado?id=${id}`, {
            method: 'POST',
            headers: { 'Authorization': `Bearer ${token}` }
        });
        
        // Actualizamos de forma inmediata el objeto en la memoria local para no perder el estado al escribir filtros
        const vehiculo = cacheVehiculos.find(v => v.id === id);
        if (vehiculo) vehiculo.activo = !vehiculo.activo;

        if (!resp.ok) listar(); 
    } catch (err) { 
        console.error(err);
        listar();
    }
}

function abrirModal() {
    document.getElementById('modalTitulo').innerText = "NUEVO INGRESO DE VEHÍCULO";
    document.getElementById('formVehiculo').reset();
    document.getElementById('vId').value = "0";
    document.getElementById('vImagenUrl').value = "";
    document.getElementById('vFotoFile').value = "";
    document.getElementById('vCategoriaId').value = ""; // Resetea el dropdown a la opción por defecto
    document.getElementById('vActivo').checked = true;
    actualizarPreview();
    myModalVehiculo?.show();
}

function editar(v) {
    document.getElementById('modalTitulo').innerText = "MODIFICAR VEHÍCULO";
    document.getElementById('vId').value = v.id;
    document.getElementById('vMarca').value = v.marca;
    document.getElementById('vModelo').value = v.modelo;
    document.getElementById('vVersion').value = v.version || '';
    document.getElementById('vAnio').value = v.anio;
    document.getElementById('vKilometros').value = v.kilometros;
    document.getElementById('vPrecio').value = v.precio;
    document.getElementById('vStock').value = v.stock;
    document.getElementById('vCombustible').value = v.combustible;
    document.getElementById('vTransmision').value = v.transmision;
    
    // ✅ Autoselección limpia de la categoría en el dropdown usando el ID relacional
    document.getElementById('vCategoriaId').value = v.categoriaId || "";
    
    document.getElementById('vImagenUrl').value = v.imagenUrl || '';
    document.getElementById('vActivo').checked = v.activo;
    document.getElementById('vFotoFile').value = ""; 
    actualizarPreview();
    myModalVehiculo?.show();
}

async function guardar() {
    const form = document.getElementById('formVehiculo');
    if(!form.checkValidity()) {
        form.reportValidity();
        return;
    }

    const formData = new FormData(form);
    formData.set('Activo', document.getElementById('vActivo').checked);

    try {
        const response = await fetch('/Admin/Guardar', {
            method: 'POST',
            headers: { 'Authorization': `Bearer ${token}` },
            body: formData
        });

        if (response.ok) {
            myModalVehiculo?.hide();
            listar();
        } else {
            const error = await response.json();
            alert("Error: " + (error.message || "No se pudo guardar."));
        }
    } catch (err) {
        alert("Error de conexión al guardar.");
    }
}

async function eliminar(id) {
    if (confirm("¿Eliminar definitivamente este vehículo?")) {
        try {
            const resp = await fetch(`/Admin/Eliminar?id=${id}`, {
                method: 'DELETE',
                headers: { 'Authorization': `Bearer ${token}` }
            });
            if(resp.ok) listar();
        } catch (err) {
            alert("Error al eliminar.");
        }
    }
}