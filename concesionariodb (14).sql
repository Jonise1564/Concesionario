-- phpMyAdmin SQL Dump
-- version 5.2.2
-- https://www.phpmyadmin.net/
--
-- Host: localhost:3306
-- Generation Time: Jun 09, 2026 at 02:58 PM
-- Server version: 8.4.3
-- PHP Version: 8.3.26

SET SQL_MODE = "NO_AUTO_VALUE_ON_ZERO";
START TRANSACTION;
SET time_zone = "+00:00";


/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET @OLD_CHARACTER_SET_RESULTS=@@CHARACTER_SET_RESULTS */;
/*!40101 SET @OLD_COLLATION_CONNECTION=@@COLLATION_CONNECTION */;
/*!40101 SET NAMES utf8mb4 */;

--
-- Database: `concesionariodb`
--

-- --------------------------------------------------------

--
-- Table structure for table `categorias`
--

CREATE TABLE `categorias` (
  `Id` int NOT NULL,
  `Nombre` varchar(50) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

--
-- Dumping data for table `categorias`
--

INSERT INTO `categorias` (`Id`, `Nombre`) VALUES
(1, 'Hatchback'),
(2, 'Sedán'),
(3, 'SUV'),
(4, 'Pick-up'),
(5, 'Híbrido'),
(6, 'Deportivo'),
(7, 'Furgón / Utilitario'),
(8, 'Monovolumen'),
(9, 'Coupé'),
(10, 'Convertible');

-- --------------------------------------------------------

--
-- Table structure for table `ciudades`
--

CREATE TABLE `ciudades` (
  `Id` int NOT NULL,
  `ProvinciaId` int NOT NULL,
  `Nombre` varchar(100) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

--
-- Dumping data for table `ciudades`
--

INSERT INTO `ciudades` (`Id`, `ProvinciaId`, `Nombre`) VALUES
(9, 1, 'Bahía Blanca'),
(7, 1, 'La Plata'),
(8, 1, 'Mar del Plata'),
(11, 1, 'Olavarría'),
(10, 1, 'Tandil'),
(14, 2, 'Belgrano'),
(13, 2, 'Caballito'),
(15, 2, 'Flores'),
(12, 2, 'Palermo'),
(61, 3, 'San Fernando del Valle de Catamarca'),
(50, 4, 'Presidencia Roque Sáenz Peña'),
(49, 4, 'Resistencia'),
(66, 5, 'Comodoro Rivadavia'),
(67, 5, 'Puerto Madryn'),
(65, 5, 'Rawson'),
(19, 6, 'Carlos Paz'),
(16, 6, 'Córdoba Capital'),
(17, 6, 'Río Cuarto'),
(20, 6, 'San Francisco'),
(18, 6, 'Villa María'),
(51, 7, 'Corrientes'),
(52, 7, 'Goya'),
(44, 8, 'Concordia'),
(45, 8, 'Gualeguaychú'),
(43, 8, 'Paraná'),
(62, 9, 'Formosa'),
(56, 10, 'San Pedro'),
(55, 10, 'San Salvador de Jujuy'),
(58, 11, 'General Pico'),
(57, 11, 'Santa Rosa'),
(60, 12, 'Chilecito'),
(59, 12, 'La Rioja'),
(23, 13, 'Godoy Cruz'),
(25, 13, 'Luján de Cuyo'),
(24, 13, 'Maipú'),
(21, 13, 'Mendoza Capital'),
(22, 13, 'San Rafael'),
(47, 14, 'Oberá'),
(46, 14, 'Posadas'),
(48, 14, 'Puerto Iguazú'),
(63, 15, 'Neuquén Capital'),
(64, 15, 'San Martín de los Andes'),
(33, 16, 'Cipolletti'),
(32, 16, 'General Roca'),
(30, 16, 'San Carlos de Bariloche'),
(31, 16, 'Viedma'),
(37, 17, 'Salta Capital'),
(38, 17, 'San Ramón de la Nueva Orán'),
(39, 17, 'Tartagal'),
(35, 18, 'Caucete'),
(36, 18, 'Chimbas'),
(34, 18, 'San Juan Capital'),
(5, 19, 'Juana Koslay'),
(6, 19, 'Justo Daract'),
(1, 19, 'La Punta'),
(4, 19, 'Merlo'),
(2, 19, 'San Luis Capital'),
(3, 19, 'Villa Mercedes'),
(69, 20, 'Caleta Olivia'),
(68, 20, 'Río Gallegos'),
(28, 21, 'Rafaela'),
(27, 21, 'Rosario'),
(26, 21, 'Santa Fe Capital'),
(29, 21, 'Venado Tuerto'),
(54, 22, 'La Banda'),
(53, 22, 'Santiago del Estero'),
(71, 23, 'Río Grande'),
(70, 23, 'Ushuaia'),
(40, 24, 'San Miguel de Tucumán'),
(42, 24, 'Tafí Viejo'),
(41, 24, 'Yerba Buena');

-- --------------------------------------------------------

--
-- Table structure for table `clientes`
--

CREATE TABLE `clientes` (
  `Id` int NOT NULL,
  `PersonaId` int NOT NULL,
  `FechaAlta` date NOT NULL,
  `CalificacionCrediticia` varchar(50) DEFAULT 'Regular',
  `Observaciones` text
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

--
-- Dumping data for table `clientes`
--

INSERT INTO `clientes` (`Id`, `PersonaId`, `FechaAlta`, `CalificacionCrediticia`, `Observaciones`) VALUES
(1, 5, '2026-06-08', 'Buena', NULL);

-- --------------------------------------------------------

--
-- Table structure for table `consultas`
--

CREATE TABLE `consultas` (
  `Id` int NOT NULL,
  `Nombre` varchar(100) NOT NULL,
  `Email` varchar(150) NOT NULL,
  `Telefono` varchar(50) DEFAULT NULL,
  `Interes` varchar(100) DEFAULT NULL,
  `Modelo` varchar(100) DEFAULT NULL,
  `Mensaje` text NOT NULL,
  `Fecha` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `Estado` varchar(50) NOT NULL DEFAULT 'Pendiente',
  `RespuestaAdmin` text,
  `UsuarioId` int DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

--
-- Dumping data for table `consultas`
--

INSERT INTO `consultas` (`Id`, `Nombre`, `Email`, `Telefono`, `Interes`, `Modelo`, `Mensaje`, `Fecha`, `Estado`, `RespuestaAdmin`, `UsuarioId`) VALUES
(1, 'Jonathan', 'Jona.garay@gmail.com', '2664584546', 'Venta / Permuta', 'Corolla', 'sasasasasasasas', '2026-05-21 22:11:57', 'Respondido', 'Gracias por cntatctarnos', NULL),
(2, 'Malvina Soledad', 'Malvina.Soledad@nmail.com', '2555562726', 'Compra de Vehículo', 'Duster', 'Pueden llamarme', '2026-05-22 14:14:04', 'Respondido', 'Gracias por contactarnos', NULL),
(3, 'Malvina Soledad', 'Malvina.Soledad@nmail.com', '2555562726', 'Compra de Vehículo', 'Duster', 'Testetsttst', '2026-05-28 10:07:26', 'Respondido', 'Gracias por contactarnos', NULL),
(4, 'Miguel Prueba1', 'correoprueba1@gmail.com', '2664645501', 'Compra de Vehículo', 'Vento', 'Hola, no tengo usuario', '2026-06-04 01:04:18', 'Respondido', 'No podemos asignarle un usuario por el momento.', NULL),
(5, 'Miguel Prueba2', 'correoprueba2@gmail.com', '2664645502', 'Financiación', 'Necesito saber más sobre una financiación.', 'Queria preguntar cuanto es lo maximo de cuotas que podria tener para el Vento 2019', '2026-06-04 01:11:40', 'Respondido', 'Test', NULL),
(6, 'Miguel Prueba3', 'roquerobertomiguellucero@gmail.com', '2664645503', 'Compra de Vehículo', 'Gol 2016', 'Quería preguntar si tendrán mas adelante en stock algún gol mod2016?', '2026-06-04 01:16:55', 'Respondido', 'Hola, gracias por ponerse en contacto con Jonel Autos.\r\nSobre su consulta, en 2 semanas entrarán 3 Gol Modelo 2016, en 2-3 semanas puede chequear la página para revisarlos o se puede dar una vuelta por nuestra sucursal :) Que tenga lindo día.', NULL),
(7, 'Jonathan', 'Jona.garay@gmail.com', '2664584546', 'Compra de Vehículo', 'Corolla', 'TESTTESTTEST', '2026-06-04 09:27:11', 'Pendiente', NULL, NULL),
(8, 'Jonathan', 'Jona.garay@gmail.com', '2664584546', 'Vehículo en Stock', 'Toyota Hilux (2026)', 'Hola, estoy interesado en el vehículo Toyota Hilux año 2026 visto en su sitio web. Me gustaría recibir más información.', '2026-06-04 13:56:40', 'Pendiente', NULL, NULL);

-- --------------------------------------------------------

--
-- Table structure for table `imagenes`
--

CREATE TABLE `imagenes` (
  `Id` int NOT NULL,
  `Url` varchar(255) NOT NULL,
  `EsPrincipal` tinyint(1) DEFAULT '0',
  `VehiculoId` int DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- --------------------------------------------------------

--
-- Table structure for table `personas`
--

CREATE TABLE `personas` (
  `Id` int NOT NULL,
  `DocumentoIdentidad` varchar(20) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
  `Nombres` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
  `Apellidos` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
  `Email` varchar(150) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `Telefono` varchar(20) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `TelefonoAlternativo` varchar(20) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `FechaNacimiento` date NOT NULL,
  `Genero` varchar(20) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `EstadoCivil` varchar(30) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `Direccion` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `CodigoPostal` varchar(15) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `Pais` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci DEFAULT 'Argentina',
  `CreadoEl` timestamp NULL DEFAULT CURRENT_TIMESTAMP,
  `ActualizadoEl` timestamp NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  `Activo` tinyint(1) DEFAULT '1',
  `CiudadId` int NOT NULL DEFAULT '1'
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

--
-- Dumping data for table `personas`
--

INSERT INTO `personas` (`Id`, `DocumentoIdentidad`, `Nombres`, `Apellidos`, `Email`, `Telefono`, `TelefonoAlternativo`, `FechaNacimiento`, `Genero`, `EstadoCivil`, `Direccion`, `CodigoPostal`, `Pais`, `CreadoEl`, `ActualizadoEl`, `Activo`, `CiudadId`) VALUES
(3, '41526378', 'Javier', 'Lozano', 'Javier.Lozano@jonel.com', '2666588775', NULL, '1988-09-25', 'Masculino', 'Casado/a', NULL, '5700', NULL, '2026-06-04 12:13:06', '2026-06-09 13:39:15', 1, 1),
(4, '33535588', 'Jonathan', 'Garay', 'Jona.garay@gmail.com', '02665034044', NULL, '1988-09-25', 'Masculino', 'Casado/a', NULL, '5700', NULL, '2026-06-04 12:22:12', '2026-06-06 11:54:55', 1, 1),
(5, '33677800', 'Raul', 'Rosendo', 'raul.rosendo@gmail.com', '254627282732', '2323232323', '1989-05-25', 'Masculino', 'Soltero/a', '900 m18 c30', '5700', 'Argentina', '2026-06-08 18:37:32', '2026-06-09 14:30:07', 1, 1),
(6, '910192039', 'Gabriel', 'Garay', 'gab.garay@gmail.com', '026650340433', NULL, '2002-05-02', 'Masculino', 'Soltero/a', NULL, '5700', NULL, '2026-06-09 13:40:36', NULL, 1, 1);

-- --------------------------------------------------------

--
-- Table structure for table `provincias`
--

CREATE TABLE `provincias` (
  `Id` int NOT NULL,
  `Nombre` varchar(100) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

--
-- Dumping data for table `provincias`
--

INSERT INTO `provincias` (`Id`, `Nombre`) VALUES
(1, 'Buenos Aires'),
(3, 'Catamarca'),
(4, 'Chaco'),
(5, 'Chubut'),
(2, 'Ciudad Autónoma de Buenos Aires'),
(6, 'Córdoba'),
(7, 'Corrientes'),
(8, 'Entre Ríos'),
(9, 'Formosa'),
(10, 'Jujuy'),
(11, 'La Pampa'),
(12, 'La Rioja'),
(13, 'Mendoza'),
(14, 'Misiones'),
(15, 'Neuquén'),
(16, 'Río Negro'),
(17, 'Salta'),
(18, 'San Juan'),
(19, 'San Luis'),
(20, 'Santa Cruz'),
(21, 'Santa Fe'),
(22, 'Santiago del Estero'),
(23, 'Tierra del Fuego'),
(24, 'Tucumán');

-- --------------------------------------------------------

--
-- Table structure for table `usuarios`
--

CREATE TABLE `usuarios` (
  `Id` int NOT NULL,
  `NombreUsuario` varchar(100) NOT NULL,
  `Password` varchar(255) NOT NULL,
  `Rol` varchar(50) NOT NULL,
  `Activo` tinyint(1) NOT NULL DEFAULT '1',
  `Email` varchar(255) DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

--
-- Dumping data for table `usuarios`
--

INSERT INTO `usuarios` (`Id`, `NombreUsuario`, `Password`, `Rol`, `Activo`, `Email`) VALUES
(1, 'admin', '$2a$11$fCw7OEXBrSVpYAQi8C1W/uwoUSFyNItdsDdcU/LGZm5SoFLCb1uBK', 'Admin', 1, 'roquerobertomiguellucero@gmail.com'),
(2, 'vendedor2', '$2a$11$LxOzYBZrDCzB2VP6z1qwFuqxrR4ou5qAzZSa/udsSG1Fpz4JEQJI6', 'Vendedor', 1, 'vendedor2@gmail.com'),
(3, 'admin2', '$2a$11$6nWWMw8HiE.pxxO8fCWHpehjUBVWzhbro1x6aV3UvKOK4JsGfcPBy', 'Admin', 1, 'admin2@gmail.com'),
(4, 'jlozano', '$2a$11$Uh9jOR.YQAYUZLPT2gYCpeBA6oYGcJnS/cyFJmOSFBIVzjVHVUpte', 'Vendedor', 1, NULL),
(5, 'jgaray', '$2a$11$rDgnK4ntcRFpeE.FWkWllu50w9q7ILaF68uLcVqjc0PGMrc.CSqCq', 'Vendedor', 1, NULL),
(6, 'ggaray', '$2a$11$bqXVEwFxqnp84x2lTqh.M.HTaLyc1iGn46XymXugryR6YEjulbvIK', 'Vendedor', 1, NULL);

-- --------------------------------------------------------

--
-- Table structure for table `vehiculos`
--

CREATE TABLE `vehiculos` (
  `Id` int NOT NULL,
  `Marca` varchar(50) NOT NULL,
  `Modelo` varchar(100) NOT NULL,
  `Vin` varchar(17) DEFAULT NULL,
  `Patente` varchar(10) DEFAULT NULL,
  `Version` varchar(100) DEFAULT NULL,
  `Anio` int NOT NULL,
  `Condicion` varchar(10) NOT NULL DEFAULT 'Usado',
  `Kilometros` int DEFAULT '0',
  `Precio` decimal(15,2) NOT NULL,
  `Combustible` varchar(30) DEFAULT NULL,
  `Transmision` varchar(30) DEFAULT NULL,
  `CategoriaId` int NOT NULL,
  `ImagenUrl` varchar(255) DEFAULT NULL,
  `Activo` tinyint(1) DEFAULT '1',
  `Estado` varchar(20) NOT NULL DEFAULT 'Disponible',
  `Tipo` varchar(50) DEFAULT NULL,
  `UsuarioId` int NOT NULL DEFAULT '1'
) ;

--
-- Dumping data for table `vehiculos`
--

INSERT INTO `vehiculos` (`Id`, `Marca`, `Modelo`, `Vin`, `Patente`, `Version`, `Anio`, `Condicion`, `Kilometros`, `Precio`, `Combustible`, `Transmision`, `CategoriaId`, `ImagenUrl`, `Activo`, `Estado`, `Tipo`, `UsuarioId`) VALUES
(1, 'Toyota', 'Corolla', '9BWZZZ00000000001', 'AA001AA', 'SEG', 2026, '0KM', 0, 32500000.00, 'Híbrido', 'Manual', 2, '46f065e2-ee99-4719-9024-63773f260d08.jpg', 1, 'Disponible', NULL, 1),
(2, 'Toyota', 'Corolla Cross', '9BWZZZ00000000002', 'AA002BB', 'XEI Hybrid', 2026, '0KM', 0, 41200000.00, 'Híbrido', 'Automática', 5, 'f3564e8b-ae1a-4447-ad67-6e218db8a5f1.jpg', 1, 'Disponible', NULL, 1),
(3, 'Toyota', 'Hilux', '9BWZZZ00000000003', 'AA003CC', 'SRX 4x4', 2026, '0KM', 0, 58900000.00, 'Diesel', 'Automática', 4, '6607eaaa-b79f-47ad-93f2-2c873e757a98.jpg', 1, 'Disponible', 'Camioneta', 1),
(4, 'Toyota', 'Yaris', '9BWZZZ00000000004', 'AA004DD', 'XLS', 2025, '0KM', 0, 21400000.00, 'Nafta', 'Manual', 1, '355c314f-98b7-4fb4-9fb4-9e8d17c2c774.JPG', 1, 'Disponible', 'Auto', 1),
(5, 'Toyota', 'SW4', '9BWZZZ00000000005', 'AA005EE', 'Diamond', 2026, '0KM', 0, 65000000.00, 'Diesel', 'Automática', 3, '655916f1-6e5c-486d-9ec6-a450e28b2815.jpg', 1, 'Disponible', 'SUV', 1),
(6, 'Toyota', 'GR Corolla', '9BWZZZ00000000006', 'AA006FF', 'Circuit Edition', 2026, '0KM', 0, 85000000.00, 'Nafta', 'Manual', 6, 'f54fe4fb-6486-44b1-8420-e0dcdee13851.jpg', 1, 'Disponible', 'Auto', 1),
(7, 'Toyota', 'Rav4', '9BWZZZ00000000007', 'AA007GG', 'Limited Hybrid', 2026, '0KM', 0, 52000000.00, 'Híbrido', 'e-CVT', 5, 'RAV.jpg', 1, 'Disponible', 'SUV', 1),
(8, 'Fiat', 'Palio', '9BWZZZ00000000008', 'AA008HH', '1.4 Atractive', 2017, 'Usado', 85000, 8500000.00, 'Nafta', 'Manual', 1, 'Palio.jpg', 1, 'Disponible', 'Auto', 1),
(9, 'Fiat', 'Cronos', '9BWZZZ00000000009', 'AA009II', 'Precision 1.8', 2023, 'Usado', 25000, 18200000.00, 'Nafta', 'Automática', 2, 'cronos.jpg', 1, 'Disponible', 'Auto', 1),
(10, 'Fiat', 'Toro', '9BWZZZ00000000010', 'AA010JJ', 'Freedom 4x4', 2022, 'Usado', 48000, 24500000.00, 'Diesel', 'Automática', 4, 'fiattoro.jpg', 1, 'Disponible', 'SUV', 1),
(11, 'Volkswagen', 'Gol Trend', '9BWZZZ00000000011', 'AA011KK', 'Trendline 1.6', 2018, 'Usado', 110000, 9200000.00, 'Nafta', 'Manual', 1, 'goltrend.jpg', 1, 'Disponible', 'Auto', 1),
(12, 'Volkswagen', 'Amarok', '9BWZZZ00000000012', 'AA012LL', 'V6 Extreme', 2024, 'Usado', 15000, 52000000.00, 'Diesel', 'Automática', 4, 'amarok.jpg', 1, 'Disponible', 'Camioneta', 1),
(13, 'Volkswagen', 'Taos', '9BWZZZ00000000013', 'AA013MM', 'Highline', 2023, 'Usado', 32000, 38500000.00, 'Nafta', 'Automática', 3, 'a696b4c3-d586-4202-a227-3ef38ea7c76f.jpg', 1, 'Disponible', 'SUV', 1),
(14, 'Ford', 'Ranger', '9BWZZZ00000000014', 'AA014NN', 'Limited 4x4', 2024, 'Usado', 9500, 55000000.00, 'Diesel', 'Automática', 4, '3dc7e9ab-9c8e-4d53-a076-c1d34be167fe.jpg', 1, 'Disponible', 'Camioneta', 1),
(15, 'Ford', 'EcoSport', '9BWZZZ00000000015', 'AA015OO', 'Titanium 2.0', 2019, 'Usado', 75000, 14800000.00, 'Nafta', 'Manual', 3, '781d98fa-c0fd-4c5e-a0e5-49c47c923b7b.jpg', 1, 'Disponible', 'SUV', 1),
(16, 'Ford', 'Focus', '9BWZZZ00000000016', 'AA016PP', 'Titanium', 2018, 'Usado', 88000, 13500000.00, 'Nafta', 'Automática', 1, 'focus.jpg', 1, 'Disponible', 'Auto', 1),
(17, 'Chevrolet', 'Onix', '9BWZZZ00000000017', 'AA017QQ', 'LTZ', 2021, 'Usado', 42000, 12400000.00, 'Nafta', 'Manual', 1, 'ONIX.jpg', 1, 'Disponible', 'Auto', 1),
(18, 'Chevrolet', 'Cruze', '9BWZZZ00000000018', 'AA018RR', 'Premier Hatchback', 2022, 'Usado', 35000, 21000000.00, 'Nafta', 'Automática', 1, 'cruze.jpg', 1, 'Disponible', 'Auto', 1),
(19, 'Chevrolet', 'S10', '9BWZZZ00000000019', 'AA019SS', 'High Country', 2023, 'Usado', 28000, 48000000.00, 'Diesel', 'Automática', 4, 's10.jpg', 1, 'Disponible', NULL, 1),
(20, 'Renault', 'Sandero', '9BWZZZ00000000020', 'AA020TT', 'Stepway', 2020, 'Usado', 62000, 11200000.00, 'Nafta', 'Manual', 1, 'stepway.jpg', 1, 'Disponible', 'SUV', 1),
(21, 'Renault', 'Alaskan', '9BWZZZ00000000021', 'AA021UU', 'Iconic 4x4', 2023, 'Usado', 18000, 42000000.00, 'Diesel', 'Automática', 4, 'alaskan.jpg', 1, 'Disponible', 'Camioneta', 1),
(22, 'Renault', 'Duster', '9BWZZZ00000000022', 'AA022VV', 'Iconic Turbo', 2022, 'Usado', 22003, 12950000.00, 'Nafta', 'Manual', 3, 'duster.jpg', 1, 'Disponible', NULL, 1),
(24, 'Renault', 'Duster', '9BWZZZ00000000024', 'AA024XX', 'Iconic Turbos', 2022, 'Usado', 22003, 11950000.00, 'Nafta', 'Manual', 1, '3ebe3845-091c-48fc-a37a-99c53bddb750.jpg', 1, 'Disponible', NULL, 1),
(25, 'BYD', 'DJY5000', '3232321321321', '7898710564', 'High Country', 2026, '0KM', 0, 50000000.00, 'Eléctrico', 'Automática', 3, '197d9eea-5fe5-4909-bf8e-c2c1b668abc5.jpg', 1, 'Disponible', NULL, 1),
(26, 'Ford', 'Falcon', '9BWZZZ00000000033', 'DJI889', 'SPRINT', 1980, 'Usado', 270000, 250000.00, 'Nafta', 'Manual', 2, '4904cfbc-9dbe-4ae0-b7c8-74ebcee922fb.jpg', 1, 'Disponible', NULL, 1);

-- --------------------------------------------------------

--
-- Table structure for table `vendedores`
--

CREATE TABLE `vendedores` (
  `Id` int NOT NULL,
  `PersonaId` int NOT NULL,
  `UsuarioId` int NOT NULL,
  `FechaContratacion` date NOT NULL,
  `PorcentajeComision` decimal(4,2) DEFAULT '0.00',
  `Observaciones` text
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

--
-- Dumping data for table `vendedores`
--

INSERT INTO `vendedores` (`Id`, `PersonaId`, `UsuarioId`, `FechaContratacion`, `PorcentajeComision`, `Observaciones`) VALUES
(1, 3, 4, '2026-06-04', 20.00, 'Atención corporativa y flotas.'),
(2, 4, 5, '2026-06-04', 20.00, 'Atención corporativa y flotas.'),
(3, 6, 6, '2026-06-09', 20.00, 'Atención corporativa y flotas.');

-- --------------------------------------------------------

--
-- Table structure for table `ventas`
--

CREATE TABLE `ventas` (
  `Id` int NOT NULL,
  `VehiculoId` int NOT NULL,
  `ClienteId` int NOT NULL,
  `VendedorId` int NOT NULL,
  `FechaVenta` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `MontoFinal` decimal(15,2) NOT NULL,
  `FormaPago` varchar(50) NOT NULL,
  `Observaciones` text
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- --------------------------------------------------------

--
-- Table structure for table `__efmigrationshistory`
--

CREATE TABLE `__efmigrationshistory` (
  `MigrationId` varchar(150) NOT NULL,
  `ProductVersion` varchar(32) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

--
-- Dumping data for table `__efmigrationshistory`
--

INSERT INTO `__efmigrationshistory` (`MigrationId`, `ProductVersion`) VALUES
('20260604035712_SincronizarDB', '10.0.4');

--
-- Indexes for dumped tables
--

--
-- Indexes for table `categorias`
--
ALTER TABLE `categorias`
  ADD PRIMARY KEY (`Id`);

--
-- Indexes for table `ciudades`
--
ALTER TABLE `ciudades`
  ADD PRIMARY KEY (`Id`),
  ADD UNIQUE KEY `UQ_Ciudad_Provincia` (`ProvinciaId`,`Nombre`);

--
-- Indexes for table `clientes`
--
ALTER TABLE `clientes`
  ADD PRIMARY KEY (`Id`),
  ADD UNIQUE KEY `UQ_Cliente_Persona` (`PersonaId`);

--
-- Indexes for table `consultas`
--
ALTER TABLE `consultas`
  ADD PRIMARY KEY (`Id`),
  ADD KEY `UsuarioId` (`UsuarioId`);

--
-- Indexes for table `imagenes`
--
ALTER TABLE `imagenes`
  ADD PRIMARY KEY (`Id`),
  ADD KEY `VehiculoId` (`VehiculoId`);

--
-- Indexes for table `personas`
--
ALTER TABLE `personas`
  ADD PRIMARY KEY (`Id`),
  ADD UNIQUE KEY `DocumentoIdentidad` (`DocumentoIdentidad`),
  ADD UNIQUE KEY `Email` (`Email`),
  ADD KEY `idx_apellido_nombre` (`Apellidos`,`Nombres`),
  ADD KEY `idx_documento` (`DocumentoIdentidad`),
  ADD KEY `FK_Personas_Ciudades` (`CiudadId`);

--
-- Indexes for table `provincias`
--
ALTER TABLE `provincias`
  ADD PRIMARY KEY (`Id`),
  ADD UNIQUE KEY `Nombre` (`Nombre`);

--
-- Indexes for table `usuarios`
--
ALTER TABLE `usuarios`
  ADD PRIMARY KEY (`Id`);

--
-- Indexes for table `vehiculos`
--
ALTER TABLE `vehiculos`
  ADD PRIMARY KEY (`Id`),
  ADD UNIQUE KEY `UQ_Vehiculo_Vin` (`Vin`),
  ADD KEY `CategoriaId` (`CategoriaId`),
  ADD KEY `fk_vehiculos_usuarios` (`UsuarioId`);

--
-- Indexes for table `vendedores`
--
ALTER TABLE `vendedores`
  ADD PRIMARY KEY (`Id`),
  ADD UNIQUE KEY `UQ_Vendedor_Persona` (`PersonaId`),
  ADD UNIQUE KEY `UQ_Vendedor_Usuario` (`UsuarioId`);

--
-- Indexes for table `ventas`
--
ALTER TABLE `ventas`
  ADD PRIMARY KEY (`Id`),
  ADD UNIQUE KEY `UQ_Venta_Vehiculo` (`VehiculoId`),
  ADD KEY `IX_Ventas_Cliente` (`ClienteId`),
  ADD KEY `IX_Ventas_Vendedor` (`VendedorId`);

--
-- Indexes for table `__efmigrationshistory`
--
ALTER TABLE `__efmigrationshistory`
  ADD PRIMARY KEY (`MigrationId`);

--
-- AUTO_INCREMENT for dumped tables
--

--
-- AUTO_INCREMENT for table `categorias`
--
ALTER TABLE `categorias`
  MODIFY `Id` int NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=11;

--
-- AUTO_INCREMENT for table `ciudades`
--
ALTER TABLE `ciudades`
  MODIFY `Id` int NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=72;

--
-- AUTO_INCREMENT for table `clientes`
--
ALTER TABLE `clientes`
  MODIFY `Id` int NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=2;

--
-- AUTO_INCREMENT for table `consultas`
--
ALTER TABLE `consultas`
  MODIFY `Id` int NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=9;

--
-- AUTO_INCREMENT for table `imagenes`
--
ALTER TABLE `imagenes`
  MODIFY `Id` int NOT NULL AUTO_INCREMENT;

--
-- AUTO_INCREMENT for table `personas`
--
ALTER TABLE `personas`
  MODIFY `Id` int NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=7;

--
-- AUTO_INCREMENT for table `provincias`
--
ALTER TABLE `provincias`
  MODIFY `Id` int NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=25;

--
-- AUTO_INCREMENT for table `usuarios`
--
ALTER TABLE `usuarios`
  MODIFY `Id` int NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=7;

--
-- AUTO_INCREMENT for table `vehiculos`
--
ALTER TABLE `vehiculos`
  MODIFY `Id` int NOT NULL AUTO_INCREMENT;

--
-- AUTO_INCREMENT for table `vendedores`
--
ALTER TABLE `vendedores`
  MODIFY `Id` int NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=4;

--
-- AUTO_INCREMENT for table `ventas`
--
ALTER TABLE `ventas`
  MODIFY `Id` int NOT NULL AUTO_INCREMENT;

--
-- Constraints for dumped tables
--

--
-- Constraints for table `ciudades`
--
ALTER TABLE `ciudades`
  ADD CONSTRAINT `FK_Ciudades_Provincias` FOREIGN KEY (`ProvinciaId`) REFERENCES `provincias` (`Id`) ON DELETE CASCADE;

--
-- Constraints for table `clientes`
--
ALTER TABLE `clientes`
  ADD CONSTRAINT `FK_Clientes_Personas` FOREIGN KEY (`PersonaId`) REFERENCES `personas` (`Id`) ON DELETE RESTRICT ON UPDATE CASCADE;

--
-- Constraints for table `consultas`
--
ALTER TABLE `consultas`
  ADD CONSTRAINT `consultas_ibfk_1` FOREIGN KEY (`UsuarioId`) REFERENCES `usuarios` (`Id`);

--
-- Constraints for table `imagenes`
--
ALTER TABLE `imagenes`
  ADD CONSTRAINT `imagenes_ibfk_1` FOREIGN KEY (`VehiculoId`) REFERENCES `vehiculos` (`Id`);

--
-- Constraints for table `personas`
--
ALTER TABLE `personas`
  ADD CONSTRAINT `FK_Personas_Ciudades` FOREIGN KEY (`CiudadId`) REFERENCES `ciudades` (`Id`) ON DELETE RESTRICT;

--
-- Constraints for table `vehiculos`
--
ALTER TABLE `vehiculos`
  ADD CONSTRAINT `fk_vehiculos_usuarios` FOREIGN KEY (`UsuarioId`) REFERENCES `usuarios` (`Id`) ON DELETE RESTRICT ON UPDATE CASCADE,
  ADD CONSTRAINT `vehiculos_ibfk_1` FOREIGN KEY (`CategoriaId`) REFERENCES `categorias` (`Id`);

--
-- Constraints for table `vendedores`
--
ALTER TABLE `vendedores`
  ADD CONSTRAINT `FK_Vendedores_Personas` FOREIGN KEY (`PersonaId`) REFERENCES `personas` (`Id`) ON DELETE RESTRICT ON UPDATE CASCADE,
  ADD CONSTRAINT `FK_Vendedores_Usuarios` FOREIGN KEY (`UsuarioId`) REFERENCES `usuarios` (`Id`) ON DELETE RESTRICT ON UPDATE CASCADE;

--
-- Constraints for table `ventas`
--
ALTER TABLE `ventas`
  ADD CONSTRAINT `FK_Ventas_Clientes` FOREIGN KEY (`ClienteId`) REFERENCES `clientes` (`Id`) ON DELETE RESTRICT ON UPDATE CASCADE,
  ADD CONSTRAINT `FK_Ventas_Vehiculos` FOREIGN KEY (`VehiculoId`) REFERENCES `vehiculos` (`Id`) ON DELETE RESTRICT ON UPDATE CASCADE,
  ADD CONSTRAINT `FK_Ventas_Vendedores` FOREIGN KEY (`VendedorId`) REFERENCES `vendedores` (`Id`) ON DELETE RESTRICT ON UPDATE CASCADE;
COMMIT;

/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
