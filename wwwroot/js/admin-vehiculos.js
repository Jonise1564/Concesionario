// // Variables globales para interactuar con el inventario y modales
// let myModalVehiculo;
// let cacheVehiculos = []; // Guardará el stock original para filtrado local instantáneo
// let cacheCategorias = []; // <--- Guardamos las categorías en memoria para cruzarlas en la tabla

// document.addEventListener("DOMContentLoaded", async () => {
//     const modalEl = document.getElementById('modalVehiculo');
//     if (modalEl) {
//         myModalVehiculo = new bootstrap.Modal(modalEl);
//     }

//     // Extraemos el token del almacenamiento local
//     token = localStorage.getItem('jonel_token');

//     // Si hay un token válido, cargamos los datos estructurales iniciales y el listado de forma secuencial
//     if (token) {
//         await cargarCategorias();
//         await listar(); // <-- Forzamos el await para asegurar el orden de ejecución
//     } else {
//         window.location.href = '/Home/Acceso';
//     }
// });

// // --- CARGA DINÁMICA DE CATEGORÍAS (SELECTS) ---
// async function cargarCategorias() {
//     try {
//         console.log("Iniciando la petición a /Admin/GetCategorias...");
        
//         // Ejecutamos el fetch apuntando a la raíz
//         const resp = await fetch('/Admin/GetCategorias', { 
//             method: 'GET'
//             // Si volvés a activar la seguridad JWT, descomentá la línea de abajo:
//             // , headers: { 'Authorization': `Bearer ${token}` }
//         });

//         if (!resp.ok) {
//             console.error(`Error de red al traer categorías. Código de respuesta: ${resp.status}`);
//             return;
//         }

//         const categorias = await resp.json();
//         console.log("Categorías crudas devueltas por el servidor:", categorias);

//         // Guardamos en la caché global del sistema
//         cacheCategorias = categorias; 
        
//         // Capturamos el select usando el ID exacto de tu HTML
//         const selectModal = document.getElementById('vCategoriaId');

//         if (!selectModal) {
//             console.error("¡ERROR CRÍTICO! No se encontró el elemento HTML con ID 'vCategoriaId'.");
//             return;
//         }

//         // Limpiamos opciones previas e inyectamos la opción por defecto
//         selectModal.innerHTML = '<option value="" disabled selected>Seleccione una categoría...</option>';
        
//         // Recorremos los datos inyectando cada option de forma segura
//         categorias.forEach(cat => {
//             // Evaluamos ambas variantes (minúscula/mayúscula) para evitar romper el bucle
//             const catId = cat.id !== undefined ? cat.id : cat.Id;
//             const catNombre = cat.nombre || cat.Nombre;

//             if (catId !== undefined && catNombre) {
//                 const option = document.createElement('option');
//                 option.value = catId;
//                 option.textContent = catNombre;
//                 selectModal.appendChild(option);
//             } else {
//                 console.warn("Se detectó un elemento de categoría con estructura inválida:", cat);
//             }
//         });

//         console.log(`¡Éxito! Se inyectaron ${categorias.length} categorías en el selector modal.`);

//     } catch (err) {
//         console.error("Error crítico atrapado en cargarCategorias():", err);
//     }
// }




// // --- 📊 LÓGICA DE VEHÍCULOS ---
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

//         // Almacenamos los ítems en caché para poder usar búsquedas ultra rápidas
//         cacheVehiculos = data.items ? data.items : data;

//         // Inyectamos el set inicial en la tabla
//         inyectarTablaVehiculos(cacheVehiculos);

//     } catch (err) {
//         console.error("Error al listar:", err);
//         document.getElementById('tablaCuerpo').innerHTML = `<tr><td colspan="10" class="text-center text-danger">Error de conexión: ${err.message}</td></tr>`;
//     }
// }

// // 🖨️ FUNCIÓN AUXILIAR PARA RENDERIZAR LAS FILAS DE LA TABLA
// function inyectarTablaVehiculos(lista) {
//     const cuerpo = document.getElementById('tablaCuerpo');

//     if (!lista || lista.length === 0) {
//         cuerpo.innerHTML = '<tr><td colspan="10" class="text-center text-muted p-4">No hay vehículos coincidentes</td></tr>';
//         return;
//     }

//     cuerpo.innerHTML = lista.map(v => {
//         // Mapeo seguro con tolerancia a mayúsculas/minúsculas del Backend (C# PascalCase)
//         const idVehiculo = v.id !== undefined ? v.id : v.Id;
//         const txtMarca = v.marca || v.Marca || '-';
//         const txtModelo = v.modelo || v.Modelo || '-';
//         const txtVersion = v.version || v.Version || '';
//         const txtAnio = v.anio || v.Anio || '-';
//         const numPrecio = v.precio !== undefined ? v.precio : (v.Precio || 0);
//         const isActivo = v.activo !== undefined ? v.activo : v.Activo;

//         const txtVin = v.vin || v.Vin || '-';
//         const txtPatente = v.patente || v.Patente || '-';
//         const rawCondicion = v.condicion || v.Condicion || 'Usado';

//         // =========================================================================
//         // 🛠️ CRUCE DE DATOS LOCAL CON TOLERANCIA DE TIPOS (String vs Int)
//         // =========================================================================
//         const idCategoriaVehiculo = v.categoriaId !== undefined ? v.categoriaId : v.CategoriaId;
//         const categoriaEncontrada = cacheCategorias.find(cat => {
//             const catId = cat.id !== undefined ? cat.id : cat.Id;
//             return catId == idCategoriaVehiculo; // <-- Comparación flexible para romper discrepancias de tipo
//         });
//         const txtCategoria = categoriaEncontrada ? (categoriaEncontrada.nombre || categoriaEncontrada.Nombre) : 'Sin Categoría';
//         // =========================================================================

//         // Normalización visual para la insignia de la tabla
//         const txtCondicion = (rawCondicion === '0KM' || rawCondicion === 'Nuevo') ? 'Nuevo' : 'Usado';

//         let imgPath = 'https://placehold.co/60x40/00?text=S/F';
//         const urlBase = v.imagenUrl || v.ImagenUrl;
//         if (urlBase) {
//             imgPath = urlBase.startsWith('http') ? urlBase : `/img/cars/${urlBase}`;
//         }

//         return `
//             <tr class="align-middle">
//                 <td>
//                     <img src="${imgPath}" class="img-thumb-table" onerror="this.src='https://placehold.co/60x40/00?text=S/F'">
//                 </td>
//                 <td>
//                     <div class="form-check form-switch">
//                         <input class="form-check-input" type="checkbox" ${isActivo ? 'checked' : ''} onclick="toggleEstado(${idVehiculo})">
//                     </div>
//                 </td>
//                 <td class="fw-bold text-white">
//                     ${txtMarca}
//                     <br><small class="text-danger text-uppercase" style="font-size: 0.75rem;">${txtCategoria}</small>
//                 </td>
//                 <td class="text-white">${txtModelo} <br><small class="text-muted">${txtVersion}</small></td>
//                 <td>
//                     <span class="badge ${txtCondicion === 'Nuevo' ? 'bg-success' : 'bg-secondary'}">${txtCondicion === 'Nuevo' ? 'Nuevo (0Km)' : 'Usado'}</span>
//                 </td>
//                 <td class="text-white text-uppercase font-monospace">${txtPatente}</td>
//                 <td class="text-white">${txtAnio}</td>
//                 <td class="text-danger fw-bold">$ ${numPrecio.toLocaleString()}</td>
//                 <td class="text-white text-uppercase font-monospace small">${txtVin}</td>
//                 <td>
//                     <button class="btn btn-sm btn-outline-light me-2" onclick='editar(${JSON.stringify(v)})'>
//                         <i class="bi bi-pencil"></i>
//                     </button>
//                     <button class="btn btn-sm btn-outline-danger" onclick="eliminar(${idVehiculo})">
//                         <i class="bi bi-trash"></i>
//                     </button>
//                 </td>
//             </tr>
//         `;
//     }).join('');
// }

// // ⚡ FILTRADO COMBINADO EN TIEMPO REAL (TEXTO + ESTADO VISIBLE)
// function filtrarVehiculos() {
//     const busqueda = document.getElementById('buscarVehiculo').value.toLowerCase().trim();
//     const filtroEstado = document.getElementById('filtrarEstado').value;

//     const resultado = cacheVehiculos.filter(v => {
//         // Extracción segura de datos para el filtro local
//         const marca = (v.marca || v.Marca || '').toLowerCase();
//         const modelo = (v.modelo || v.Modelo || '').toLowerCase();
//         const version = (v.version || v.Version || '').toLowerCase();
//         const vin = (v.vin || v.Vin || '').toLowerCase();
//         const patente = (v.patente || v.Patente || '').toLowerCase();
//         const activo = v.activo !== undefined ? v.activo : v.Activo;

//         // 1. Match por texto extensible
//         const cumpleTexto =
//             marca.includes(busqueda) ||
//             modelo.includes(busqueda) ||
//             version.includes(busqueda) ||
//             vin.includes(busqueda) ||
//             patente.includes(busqueda);

//         // 2. Match por visibilidad web
//         let cumpleEstado = true;
//         if (filtroEstado === 'activos') cumpleEstado = activo === true;
//         if (filtroEstado === 'inactivos') cumpleEstado = false;

//         return cumpleTexto && cumpleEstado;
//     });

//     inyectarTablaVehiculos(resultado);
// }

// function seleccionarArchivo(input) {
//     if (input.files && input.files[0]) {
//         const reader = new FileReader();
//         reader.onload = function (e) {
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

//         const vehiculo = cacheVehiculos.find(v => (v.id === id || v.Id === id));
//         if (vehiculo) {
//             if (vehiculo.activo !== undefined) vehiculo.activo = !vehiculo.activo;
//             if (vehiculo.Activo !== undefined) vehiculo.Activo = !vehiculo.Activo;
//         }

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
//     document.getElementById('vVin').value = "";
//     document.getElementById('vPatente').value = "";
//     document.getElementById('vCondicion').value = "Usado";
//     document.getElementById('vCategoriaId').value = "";
//     document.getElementById('vActivo').checked = true;
//     actualizarPreview();
//     myModalVehiculo?.show();
// }

// function editar(v) {
//     document.getElementById('modalTitulo').innerText = "MODIFICAR VEHÍCULO";

//     // Asignación con fallback estricto para C# (Mayúsculas / Minúsculas)
//     document.getElementById('vId').value = v.id !== undefined ? v.id : (v.Id || 0);
//     document.getElementById('vMarca').value = v.marca || v.Marca || '';
//     document.getElementById('vModelo').value = v.modelo || v.Modelo || '';
//     document.getElementById('vVersion').value = v.version || v.Version || '';
//     document.getElementById('vAnio').value = v.anio || v.Anio || '';
//     document.getElementById('vKilometros').value = v.kilometros !== undefined ? v.kilometros : (v.Kilometros || 0);
//     document.getElementById('vPrecio').value = v.precio !== undefined ? v.precio : (v.Precio || 0);

//     document.getElementById('vVin').value = v.vin || v.Vin || '';
//     document.getElementById('vPatente').value = v.patente || v.Patente || '';

//     // Mapeo preciso del selector de condición ("0KM" de la base de datos se convierte en "Nuevo")
//     const rawCondicion = v.condicion || v.Condicion || 'Usado';
//     if (rawCondicion === '0KM' || rawCondicion === 'Nuevo') {
//         document.getElementById('vCondicion').value = "Nuevo";
//     } else {
//         document.getElementById('vCondicion').value = "Usado";
//     }

//     document.getElementById('vCombustible').value = v.combustible || v.Combustible || 'Nafta';
//     document.getElementById('vTransmision').value = v.transmision || v.Transmision || 'Manual';

//     // Seteo del combo de categorías de forma segura
//     const idCategoriaVehiculo = v.categoriaId !== undefined ? v.categoriaId : (v.CategoriaId || "");
//     document.getElementById('vCategoriaId').value = idCategoriaVehiculo;

//     document.getElementById('vImagenUrl').value = v.imagenUrl || v.ImagenUrl || '';
//     document.getElementById('vActivo').checked = v.activo !== undefined ? v.activo : v.Activo;
//     document.getElementById('vFotoFile').value = "";

//     actualizarPreview();
//     myModalVehiculo?.show();
// }

// async function guardar() {
//     const form = document.getElementById('formVehiculo');
//     if (!form.checkValidity()) {
//         form.reportValidity();
//         return;
//     }

//     const formData = new FormData(form);

//     // Homologación de condición al enviar al Backend para mantener limpia la BD
//     const condicionSeleccionada = document.getElementById('vCondicion').value;
//     if (condicionSeleccionada === 'Nuevo') {
//         formData.set('Condicion', '0KM');
//     } else {
//         formData.set('Condicion', 'Usado');
//     }

//     const isChecked = document.getElementById('vActivo').checked;
//     formData.set('Activo', isChecked ? "True" : "False");

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
//             if (resp.ok) listar();
//         } catch (err) {
//             alert("Error al eliminar.");
//         }
//     }
// }


// Variables globales para interactuar con el inventario y modales
let myModalVehiculo;
let cacheVehiculos = []; // Guardará el stock original para filtrado local instantáneo
let cacheCategorias = []; // <--- Guardamos las categorías en memoria para cruzarlas en la tabla

document.addEventListener("DOMContentLoaded", async () => {
    const modalEl = document.getElementById('modalVehiculo');
    if (modalEl) {
        myModalVehiculo = new bootstrap.Modal(modalEl);
    }

    // Extraemos el token del almacenamiento local
    token = localStorage.getItem('jonel_token');

    // Si hay un token válido, cargamos los datos estructurales iniciales y el listado de forma secuencial
    if (token) {
        await cargarCategorias();
        await listar(); // <-- Forzamos el await para asegurar el orden de ejecución
    } else {
        window.location.href = '/Home/Acceso';
    }
});

// --- CARGA DINÁMICA DE CATEGORÍAS (SELECTS) ---
async function cargarCategorias() {
    try {
        console.log("Iniciando la petición a /Admin/GetCategorias...");
        
        // Ejecutamos el fetch apuntando a la raíz
        const resp = await fetch('/Admin/GetCategorias', { 
            method: 'GET'
            // Si volvés a activar la seguridad JWT, descomentá la línea de abajo:
            // , headers: { 'Authorization': `Bearer ${token}` }
        });

        if (!resp.ok) {
            console.error(`Error de red al traer categorías. Código de respuesta: ${resp.status}`);
            return;
        }

        const categories = await resp.json();
        console.log("Categorías crudas devueltas por el servidor:", categories);

        // Guardamos en la caché global del sistema
        cacheCategorias = categories; 
        
        // Capturamos el select usando el ID exacto de tu HTML
        const selectModal = document.getElementById('vCategoriaId');

        if (!selectModal) {
            console.error("¡ERROR CRÍTICO! No se encontró el elemento HTML con ID 'vCategoriaId'.");
            return;
        }

        // Limpiamos opciones previas e inyectamos la opción por defecto
        selectModal.innerHTML = '<option value="" disabled selected>Seleccione una categoría...</option>';
        
        // Recorremos los datos inyectando cada option de forma segura
        categories.forEach(cat => {
            // Evaluamos ambas variantes (minúscula/mayúscula) para evitar romper el bucle
            const catId = cat.id !== undefined ? cat.id : cat.Id;
            const catNombre = cat.nombre || cat.Nombre;

            if (catId !== undefined && catNombre) {
                const option = document.createElement('option');
                option.value = catId;
                option.textContent = catNombre;
                selectModal.appendChild(option);
            } else {
                console.warn("Se detectó un elemento de categoría con estructura inválida:", cat);
            }
        });

        console.log(`¡Éxito! Se inyectaron ${categories.length} categorías en el selector modal.`);

    } catch (err) {
        console.error("Error crítico atrapado en cargarCategorias():", err);
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
        document.getElementById('tablaCuerpo').innerHTML = `<tr><td colspan="10" class="text-center text-danger">Error de conexión: ${err.message}</td></tr>`;
    }
}

// 🖨️ FUNCIÓN AUXILIAR PARA RENDERIZAR LAS FILAS DE LA TABLA
function inyectarTablaVehiculos(lista) {
    const cuerpo = document.getElementById('tablaCuerpo');

    if (!lista || lista.length === 0) {
        cuerpo.innerHTML = '<tr><td colspan="10" class="text-center text-muted p-4">No hay vehículos coincidentes</td></tr>';
        return;
    }

    cuerpo.innerHTML = lista.map(v => {
        // Mapeo seguro con tolerancia a mayúsculas/minúsculas del Backend (C# PascalCase)
        const idVehiculo = v.id !== undefined ? v.id : v.Id;
        const txtMarca = v.marca || v.Marca || '-';
        const txtModelo = v.modelo || v.Modelo || '-';
        const txtVersion = v.version || v.Version || '';
        const txtAnio = v.anio || v.Anio || '-';
        const numPrecio = v.precio !== undefined ? v.precio : (v.Precio || 0);
        const isActivo = v.activo !== undefined ? v.activo : v.Activo;

        const txtVin = v.vin || v.Vin || '-';
        const txtPatente = v.patente || v.Patente || '-';
        const rawCondicion = v.condicion || v.Condicion || 'Usado';

        // =========================================================================
        // 🛠️ CRUCE DE DATOS LOCAL CON TOLERANCIA DE TIPOS (String vs Int)
        // =========================================================================
        const idCategoriaVehiculo = v.categoriaId !== undefined ? v.categoriaId : v.CategoriaId;
        const categoriaEncontrada = cacheCategorias.find(cat => {
            const catId = cat.id !== undefined ? cat.id : cat.Id;
            return catId == idCategoriaVehiculo; // <-- Comparación flexible para romper discrepancies de tipo
        });
        const txtCategoria = categoriaEncontrada ? (categoriaEncontrada.nombre || categoriaEncontrada.Nombre) : 'Sin Categoría';
        // =========================================================================

        // Normalización visual para la insignia de la tabla
        const txtCondicion = (rawCondicion === '0KM' || rawCondicion === 'Nuevo') ? 'Nuevo' : 'Usado';

        let imgPath = 'https://placehold.co/60x40/00?text=S/F';
        const urlBase = v.imagenUrl || v.ImagenUrl;
        if (urlBase) {
            imgPath = urlBase.startsWith('http') ? urlBase : `/img/cars/${urlBase}`;
        }

        return `
            <tr class="align-middle">
                <td>
                    <img src="${imgPath}" class="img-thumb-table" onerror="this.src='https://placehold.co/60x40/00?text=S/F'">
                </td>
                <td>
                    <div class="form-check form-switch">
                        <input class="form-check-input" type="checkbox" ${isActivo ? 'checked' : ''} onclick="toggleEstado(${idVehiculo})">
                    </div>
                </td>
                <td class="fw-bold text-white">
                    ${txtMarca}
                    <br><small class="text-danger text-uppercase" style="font-size: 0.75rem;">${txtCategoria}</small>
                </td>
                <td class="text-white">${txtModelo} <br><small class="text-muted">${txtVersion}</small></td>
                <td>
                    <span class="badge ${txtCondicion === 'Nuevo' ? 'bg-success' : 'bg-secondary'}">${txtCondicion === 'Nuevo' ? 'Nuevo (0Km)' : 'Usado'}</span>
                </td>
                <td class="text-white text-uppercase font-monospace">${txtPatente}</td>
                <td>${txtAnio}</td>
                <td class="text-danger fw-bold">$ ${numPrecio.toLocaleString()}</td>
                <td class="text-white text-uppercase font-monospace small">${txtVin}</td>
                <td>
                    <button class="btn btn-sm btn-outline-light me-2" onclick='editar(${JSON.stringify(v)})'>
                        <i class="bi bi-pencil"></i>
                    </button>
                    <button class="btn btn-sm btn-outline-danger" onclick="eliminar(${idVehiculo})">
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
    const filterEstado = document.getElementById('filtrarEstado').value;

    const resultado = cacheVehiculos.filter(v => {
        // Extracción segura de datos para el filtro local
        const marca = (v.marca || v.Marca || '').toLowerCase();
        const modelo = (v.modelo || v.Modelo || '').toLowerCase();
        const version = (v.version || v.Version || '').toLowerCase();
        const vin = (v.vin || v.Vin || '').toLowerCase();
        const patente = (v.patente || v.Patente || '').toLowerCase();
        const activo = v.activo !== undefined ? v.activo : v.Activo;

        // 1. Match por texto extensible
        const cumpleTexto =
            marca.includes(busqueda) ||
            modelo.includes(busqueda) ||
            version.includes(busqueda) ||
            vin.includes(busqueda) ||
            patente.includes(busqueda);

        // 2. Match por visibilidad web
        let cumpleEstado = true;
        if (filterEstado === 'activos') cumpleEstado = activo === true;
        if (filterEstado === 'inactivos') cumpleEstado = false;

        return cumpleTexto && cumpleEstado;
    });

    inyectarTablaVehiculos(resultado);
}

function seleccionarArchivo(input) {
    if (input.files && input.files[0]) {
        const reader = new FileReader();
        reader.onload = function (e) {
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

        const vehiculo = cacheVehiculos.find(v => (v.id === id || v.Id === id));
        if (vehiculo) {
            if (vehiculo.activo !== undefined) vehiculo.activo = !vehiculo.activo;
            if (vehiculo.Activo !== undefined) vehiculo.Activo = !vehiculo.Activo;
        }

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
    document.getElementById('vVin').value = "";
    document.getElementById('vPatente').value = "";
    document.getElementById('vCondicion').value = "Usado";
    document.getElementById('vCategoriaId').value = "";
    document.getElementById('vActivo').checked = true;
    actualizarPreview();
    myModalVehiculo?.show();
}

function editar(v) {
    document.getElementById('modalTitulo').innerText = "MODIFICAR VEHÍCULO";

    // Asignación con fallback estricto para C# (Mayúsculas / Minúsculas)
    document.getElementById('vId').value = v.id !== undefined ? v.id : (v.Id || 0);
    document.getElementById('vMarca').value = v.marca || v.Marca || '';
    document.getElementById('vModelo').value = v.modelo || v.Modelo || '';
    document.getElementById('vVersion').value = v.version || v.Version || '';
    document.getElementById('vAnio').value = v.anio || v.Anio || '';
    document.getElementById('vKilometros').value = v.kilometros !== undefined ? v.kilometros : (v.Kilometros || 0);
    document.getElementById('vPrecio').value = v.precio !== undefined ? v.precio : (v.Precio || 0);

    document.getElementById('vVin').value = v.vin || v.Vin || '';
    document.getElementById('vPatente').value = v.patente || v.Patente || '';

    // Mapeo preciso del selector de condición ("0KM" de la base de datos se convierte en "Nuevo")
    const rawCondicion = v.condicion || v.Condicion || 'Usado';
    if (rawCondicion === '0KM' || rawCondicion === 'Nuevo') {
        document.getElementById('vCondicion').value = "Nuevo";
    } else {
        document.getElementById('vCondicion').value = "Usado";
    }

    document.getElementById('vCombustible').value = v.combustible || v.Combustible || 'Nafta';
    document.getElementById('vTransmision').value = v.transmision || v.Transmision || 'Manual';

    // Seteo del combo de categorías de forma segura
    const idCategoriaVehiculo = v.categoriaId !== undefined ? v.categoriaId : (v.CategoriaId || "");
    document.getElementById('vCategoriaId').value = idCategoriaVehiculo;

    document.getElementById('vImagenUrl').value = v.imagenUrl || v.ImagenUrl || '';
    document.getElementById('vActivo').checked = v.activo !== undefined ? v.activo : v.Activo;
    document.getElementById('vFotoFile').value = "";

    actualizarPreview();
    myModalVehiculo?.show();
}

// async function guardar() {
//     const form = document.getElementById('formVehiculo');
//     if (!form.checkValidity()) {
//         form.reportValidity();
//         return;
//     }

//     // Inicializamos el FormData basado en los controles del formulario
//     const formData = new FormData(form);

//     // =========================================================================
//     // 🛠️ SOBREESCRITURA TOLERANTE A LOWERCASE/PASCALCASE PARA .NET
//     // =========================================================================
    
//     // 1. Forzamos ID en ambas variantes de casing por seguridad
//     const idVehiculo = document.getElementById('vId').value;
//     formData.set('id', idVehiculo);
//     formData.set('Id', idVehiculo);

//     // 2. Homologación de condición limpia
//     const condicionSeleccionada = document.getElementById('vCondicion').value;
//     const valorCondicion = (condicionSeleccionada === 'Nuevo') ? '0KM' : 'Usado';
//     formData.set('condicion', valorCondicion);
//     formData.set('Condicion', valorCondicion);

//     // 3. Formateo correcto del booleano del Switch
//     const isChecked = document.getElementById('vActivo').checked;
//     formData.set('activo', isChecked ? "true" : "false");
//     formData.set('Activo', isChecked ? "True" : "False");

//     // 4. CAPTURA EXPLÍCITA DEL ARCHIVO BINARIO DE LA FOTO (Sin duplicar)
//     const fileInput = document.getElementById('vFotoFile');
//     if (fileInput && fileInput.files.length > 0) {
//         // Borramos cualquier residuo anterior y seteamos de forma limpia
//         formData.delete('FotoFile'); 
//         formData.delete('fotoFile');
//         formData.set('FotoFile', fileInput.files[0]);
//     } else {
//         // Si no subió foto nueva, eliminamos el campo para que .NET no intente parsear un string vacío como archivo
//         formData.delete('FotoFile');
//         formData.delete('fotoFile');
//     }
//     // =========================================================================

//     try {
//         const response = await fetch('/Admin/Guardar', {
//             method: 'POST',
//             headers: { 'Authorization': `Bearer ${token}` }, // Sin asignar Content-Type manual
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




async function guardar() {
    const form = document.getElementById('formVehiculo');
    if (!form.checkValidity()) {
        form.reportValidity();
        return;
    }

    // Inicializamos el FormData basado en los controles del formulario
    const formData = new FormData(form);

    // =========================================================================
    // 🛠️ SOBREESCRITURA TOLERANTE A LOWERCASE/PASCALCASE PARA .NET
    // =========================================================================
    
    // 1. Forzamos ID en ambas variantes de casing por seguridad
    const idVehiculo = document.getElementById('vId').value;
    formData.set('id', idVehiculo);
    formData.set('Id', idVehiculo);

    // 2. Homologación de condición limpia
    const condicionSeleccionada = document.getElementById('vCondicion').value;
    const valorCondicion = (condicionSeleccionada === 'Nuevo') ? '0KM' : 'Usado';
    formData.set('condicion', valorCondicion);
    formData.set('Condicion', valorCondicion);

    // 3. Formateo correcto del booleano del Switch
    const isChecked = document.getElementById('vActivo').checked;
    formData.set('activo', isChecked ? "true" : "false");
    formData.set('Activo', isChecked ? "True" : "False");

    // 4. CAPTURA EXPLÍCITA DEL ARCHIVO BINARIO DE LA FOTO (Mapeado exacto con C#)
    const fileInput = document.getElementById('vFotoFile');
    
    // Limpiamos cualquier residuo previo por las dudas
    formData.delete('FotoArchivo'); 
    formData.delete('fotoArchivo');

    if (fileInput && fileInput.files.length > 0) {
        // Seteamos la foto usando el nombre EXACTO del parámetro en el controlador: FotoArchivo
        formData.set('FotoArchivo', fileInput.files[0]);
    } else {
        // Si NO hay archivo nuevo y estamos EDITANDO (Id > 0), nos aseguramos de que 
        // el FormData conserve el string de la foto vieja (guardado en el input oculto o propiedad vImagenUrl)
        // para que C# no reciba model.ImagenUrl vacío y asuma que querés borrarla.
        const imagenUrlActual = document.getElementById('vImagenUrl')?.value || '';
        if (idVehiculo > 0 && imagenUrlActual) {
            formData.set('ImagenUrl', imagenUrlActual);
        }
    }
    // =========================================================================

    try {
        const response = await fetch('/Admin/Guardar', {
            method: 'POST',
            headers: { 'Authorization': `Bearer ${token}` }, // Sin asignar Content-Type manual
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
            if (resp.ok) listar();
        } catch (err) {
            alert("Error al eliminar.");
        }
    }
}