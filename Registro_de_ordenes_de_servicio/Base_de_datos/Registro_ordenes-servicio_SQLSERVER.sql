-- CREAR DB

USE master;
GO

IF EXISTS (SELECT * FROM sys.databases WHERE name = 'dbTallerMecanico')
BEGIN
    DROP DATABASE dbTallerMecanico;
END
GO

CREATE DATABASE dbTallerMecanico;
GO

USE dbTallerMecanico;
GO


-- CREACIÓN DE TABLAS

CREATE TABLE clientes (
    idCliente INT IDENTITY(1,1) PRIMARY KEY,
    rfc VARCHAR(13) NOT NULL UNIQUE,
    nombre VARCHAR(50) NOT NULL,
    apellidoPaterno VARCHAR(50) NOT NULL,
    apellidoMaterno VARCHAR(50),
    direccionCalle VARCHAR(100),
    direccionNumero VARCHAR(20),
    direccionColonia VARCHAR(100),
    codigoPostal VARCHAR(10),
    ciudad VARCHAR(100),
    correoElectronico VARCHAR(100) UNIQUE,
    fechaRegistro DATE DEFAULT CAST(GETDATE() AS DATE)
);

CREATE TABLE telefonosClientes (
    idTelefono INT IDENTITY(1,1) PRIMARY KEY,
    idCliente INT NOT NULL,
    numeroTelefono VARCHAR(20) NOT NULL,
    tipo VARCHAR(20) DEFAULT 'Móvil',
    CONSTRAINT fkTelCliente FOREIGN KEY (idCliente) 
        REFERENCES clientes(idCliente) ON DELETE CASCADE
);

CREATE TABLE mecanicos (
    idMecanico INT IDENTITY(1,1) PRIMARY KEY,
    rfc VARCHAR(13) NOT NULL UNIQUE,
    nombreCompleto VARCHAR(150) NOT NULL,
    especialidades VARCHAR(100) NOT NULL,
    telefono VARCHAR(20),
    salario DECIMAL(10, 2) NOT NULL,
    aniosExperiencia INT DEFAULT 0
);

CREATE TABLE servicios (
    claveServicio VARCHAR(20) PRIMARY KEY,
    nombreServicio VARCHAR(100) NOT NULL,
    descripcion TEXT,
    costoBase DECIMAL(10, 2) NOT NULL,
    tiempoEstimadoHrs DECIMAL(4, 1) NOT NULL
);

CREATE TABLE refacciones (
    codigoRefaccion VARCHAR(20) PRIMARY KEY,
    nombre VARCHAR(100) NOT NULL,
    marca VARCHAR(50),
    precioUnitario DECIMAL(10, 2) NOT NULL,
    stockActual INT NOT NULL DEFAULT 0,
    stockMinimo INT NOT NULL DEFAULT 5,
    proveedor VARCHAR(100)
);

CREATE TABLE vehiculos (
    idVehiculo INT IDENTITY(1,1) PRIMARY KEY,
    idCliente INT NOT NULL,
    numeroSerie VARCHAR(17) NOT NULL UNIQUE,
    placas VARCHAR(10) NOT NULL UNIQUE,
    marca VARCHAR(50) NOT NULL,
    modelo VARCHAR(50) NOT NULL,
    anio INT NOT NULL,
    color VARCHAR(30),
    kilometrajeActual INT NOT NULL,
    tipo VARCHAR(30),
    antiguedad AS (YEAR(GETDATE()) - anio), -- Columna calculada directa
    CONSTRAINT fkVehiculoCliente FOREIGN KEY (idCliente) 
        REFERENCES clientes(idCliente)
);

CREATE TABLE ordenesServicio (
    folioOrden INT IDENTITY(1,1) PRIMARY KEY,
    idVehiculo INT NOT NULL,
    fechaIngreso DATETIME DEFAULT GETDATE(),
    fechaEstimadaEntrega DATETIME,
    fechaRealEntrega DATETIME,
    estado VARCHAR(20) DEFAULT 'Abierta',
    costoTotal DECIMAL(12, 2) DEFAULT 0.00,
    CONSTRAINT fkOrdenVehiculo FOREIGN KEY (idVehiculo) 
        REFERENCES vehiculos(idVehiculo),
    CONSTRAINT chkEstado CHECK (estado IN ('Abierta', 'En Proceso', 'Finalizada', 'Cancelada'))
);

-- Tablas Intermedias (N:M)

CREATE TABLE ordenAsignacionMecanicos (
    folioOrden INT,
    idMecanico INT,
    PRIMARY KEY (folioOrden, idMecanico),
    CONSTRAINT fkAsmOrden FOREIGN KEY (folioOrden) REFERENCES ordenesServicio(folioOrden),
    CONSTRAINT fkAsmMecanico FOREIGN KEY (idMecanico) REFERENCES mecanicos(idMecanico)
);

CREATE TABLE ordenDetallesServicios (
    folioOrden INT,
    claveServicio VARCHAR(20),
    precioAlMomento DECIMAL(10, 2) NOT NULL,
    PRIMARY KEY (folioOrden, claveServicio),
    CONSTRAINT fkOdsOrden FOREIGN KEY (folioOrden) REFERENCES ordenesServicio(folioOrden),
    CONSTRAINT fkOdsServicio FOREIGN KEY (claveServicio) REFERENCES servicios(claveServicio)
);

CREATE TABLE ordenDetallesRefacciones (
    idDetalleRef INT IDENTITY(1,1) PRIMARY KEY,
    folioOrden INT,
    codigoRefaccion VARCHAR(20),
    cantidad INT NOT NULL DEFAULT 1,
    precioUnitarioMomento DECIMAL(10, 2) NOT NULL,
    CONSTRAINT fkOdrOrden FOREIGN KEY (folioOrden) REFERENCES ordenesServicio(folioOrden),
    CONSTRAINT fkOdrRefaccion FOREIGN KEY (codigoRefaccion) REFERENCES refacciones(codigoRefaccion)
);
GO


-- TRIGGERS Y SP

CREATE TRIGGER trgValidarMaxTelefonos
ON telefonosClientes
AFTER INSERT
AS
BEGIN
    SET NOCOUNT ON;
    
    IF EXISTS (
        SELECT idCliente 
        FROM telefonosClientes
        WHERE idCliente IN (SELECT idCliente FROM inserted)
        GROUP BY idCliente
        HAVING COUNT(*) > 3
    )
    BEGIN
        RAISERROR ('ERROR: El cliente ya tiene el máximo permitido de 3 teléfonos.', 16, 1);
        ROLLBACK TRANSACTION;
    END
END
GO

-- SP: Registrar Cliente con 1 Teléfono Obligatorio
CREATE PROCEDURE spRegistrarCliente
    @pRfc VARCHAR(13),
    @pNombre VARCHAR(50),
    @pPaterno VARCHAR(50),
    @pMaterno VARCHAR(50),
    @pCalle VARCHAR(100),
    @pNum VARCHAR(20),
    @pCol VARCHAR(100),
    @pCp VARCHAR(10),
    @pCiudad VARCHAR(100),
    @pEmail VARCHAR(100),
    @pTelefonoObligatorio VARCHAR(20),
    @pTipoTel VARCHAR(20)
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        BEGIN TRANSACTION
            
            INSERT INTO clientes (rfc, nombre, apellidoPaterno, apellidoMaterno, direccionCalle, direccionNumero, direccionColonia, codigoPostal, ciudad, correoElectronico)
            VALUES (@pRfc, @pNombre, @pPaterno, @pMaterno, @pCalle, @pNum, @pCol, @pCp, @pCiudad, @pEmail);
            
            DECLARE @nuevoId INT = SCOPE_IDENTITY();

            INSERT INTO telefonosClientes (idCliente, numeroTelefono, tipo)
            VALUES (@nuevoId, @pTelefonoObligatorio, @pTipoTel);

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END
GO


-- DATOS DE PRUEBA
EXEC spRegistrarCliente 'GOME800101H20', 'Juan', 'Gómez', 'Pérez', 'Av. Hidalgo', '123', 'Centro', '38980', 'Uriangato', 'juan.g@email.com', '4451234567', 'Móvil';
EXEC spRegistrarCliente 'HERM900202M30', 'María', 'Hernández', 'López', 'Calle Morelos', '45', 'La Joya', '38800', 'Moroleón', 'maria.h@email.com', '4451112233', 'Móvil';
EXEC spRegistrarCliente 'RODR850303H15', 'Carlos', 'Rodríguez', 'Sánchez', 'Blvd. Lopez Mateos', '500', 'Industrial', '38980', 'Uriangato', 'carlos.r@email.com', '4459998877', 'Trabajo';
EXEC spRegistrarCliente 'LOPE950404M10', 'Ana', 'López', 'García', '16 de Septiembre', '89', 'El Llanito', '38800', 'Moroleón', 'ana.l@email.com', '4450001122', 'Móvil';
EXEC spRegistrarCliente 'MART880505H40', 'Luis', 'Martínez', 'Torres', 'Calle Pípila', '230', 'Centro', '38980', 'Uriangato', 'luis.m@email.com', '4456667788', 'Móvil';

-- Teléfonos Adicionales
INSERT INTO telefonosClientes (idCliente, numeroTelefono, tipo) VALUES 
(1, '4457654321', 'Casa'),
(4, '4453334455', 'Casa');

INSERT INTO mecanicos (rfc, nombreCompleto, especialidades, telefono, salario, aniosExperiencia) VALUES
('MEC010101ABC', 'Pedro Ramírez', 'Motor y Transmisión', '4451002000', 12000.00, 10),
('MEC020202DEF', 'Javier Solís', 'Electricidad Automotriz', '4452003000', 11500.00, 8),
('MEC030303GHI', 'Roberto Díaz', 'Suspensión y Frenos', '4453004000', 10000.00, 5),
('MEC040404JKL', 'Miguel Ángel', 'Hojalatería y Pintura', '4454005000', 9500.00, 12),
('MEC050505MNO', 'Fernando Ruiz', 'Diagnóstico General', '4455006000', 8000.00, 3);

INSERT INTO servicios VALUES
('SERV-001', 'Afinación Mayor', 'Bujías, filtros, aceite, inyectores', 2500.00, 4.0),
('SERV-002', 'Cambio de Aceite', 'Sintético y filtro', 800.00, 1.0),
('SERV-003', 'Frenos Delanteros', 'Balatas y rectificado', 1200.00, 2.5),
('SERV-004', 'Alineación y Balanceo', '4 ruedas', 600.00, 1.5),
('SERV-005', 'Escaneo de Motor', 'OBD2 Diagnóstico', 300.00, 0.5);

INSERT INTO refacciones (codigoRefaccion, nombre, marca, precioUnitario, stockActual, proveedor) VALUES
('REF-001', 'Bujía Iridio', 'NGK', 150.00, 50, 'AutoZone'),
('REF-002', 'Filtro de Aceite', 'Gonher', 120.00, 30, 'Refaccionaria California'),
('REF-003', 'Filtro de Aire', 'Fram', 180.00, 25, 'Refaccionaria California'),
('REF-004', 'Aceite Sintético 5W-30', 'Castrol', 250.00, 100, 'Lubricantes Bajío'),
('REF-005', 'Balatas Delanteras', 'Brembo', 850.00, 15, 'Frenos y Más'),
('REF-006', 'Disco de Freno', 'Ruville', 600.00, 10, 'Frenos y Más'),
('REF-007', 'Amortiguador Delantero', 'Monroe', 1200.00, 8, 'Suspensiones Garcia'),
('REF-008', 'Batería 12V', 'LTH', 2200.00, 12, 'Acumuladores Centro'),
('REF-009', 'Bomba de Agua', 'Gates', 750.00, 6, 'Refaccionaria California'),
('REF-010', 'Correa Distribución', 'Gates', 500.00, 10, 'Refaccionaria California'),
('REF-011', 'Alternador', 'Bosch', 3500.00, 4, 'Auto Eléctrica Sur'),
('REF-012', 'Radiador', 'Valeo', 2800.00, 3, 'Radiadores Moroleón'),
('REF-013', 'Sensor de Oxígeno', 'Denso', 900.00, 8, 'AutoZone'),
('REF-014', 'Líquido de Frenos', 'Bardahl', 90.00, 40, 'Lubricantes Bajío'),
('REF-015', 'Junta de Cabeza', 'TF Victor', 450.00, 5, 'Refaccionaria California');

INSERT INTO vehiculos (idCliente, numeroSerie, placas, marca, modelo, anio, color, kilometrajeActual, tipo) VALUES
(1, '3N1CN7AP4CL00123', 'GTO-123-A', 'Nissan', 'Versa', 2018, 'Plata', 65000, 'Sedán'),
(1, '8G1CN7AP4CY90124', 'GTO-456-F', 'Nissan', 'Tsuru', 2002, 'Negro', 500000, 'Sedán'),
(2, '2T1BR32X5AC00456', 'GTO-456-B', 'Toyota', 'RAV4', 2020, 'Blanco', 45000, 'SUV'),
(3, '1G1JC54447700789', 'GTO-789-C', 'Chevrolet', 'Chevy', 2010, 'Rojo', 180000, 'Hatchback'),
(4, '3FA6P0H50HR00321', 'MICH-987', 'Ford', 'Lobo', 2015, 'Negro', 120000, 'Pickup'),
(4, '123HN7AP4XZ90123', 'MICH-989', 'Nissan', 'Versa', 2020, 'Blanco', 49000, 'Sedán'),
(4, '456IN7AP4XZ90123', 'MICH-159', 'Lanborgini', 'Uracan', 2024, 'Azul Marino', 9000, 'Deportivo'),
(5, 'VW34567890123456', 'CDMX-555', 'Volkswagen', 'Jetta', 2023, 'Azul', 15000, 'Sedán');

INSERT INTO ordenesServicio (idVehiculo, fechaIngreso, estado, costoTotal) VALUES
(1, '2025-11-01 09:00:00', 'Finalizada', 3500.00),
(2, '2025-11-02 10:00:00', 'Finalizada', 2900.00),
(5, GETDATE(), 'En Proceso', 1300.00),
(3, '2025-12-01 08:00:00', 'Abierta', 300.00),
(4, '2025-10-15 00:00:00', 'Cancelada', 0.00);

-- Detalles
INSERT INTO ordenAsignacionMecanicos VALUES (1, 1);
INSERT INTO ordenDetallesServicios VALUES (1, 'SERV-001', 2500.00);
INSERT INTO ordenDetallesRefacciones (folioOrden, codigoRefaccion, cantidad, precioUnitarioMomento) VALUES 
(1, 'REF-001', 4, 150.00), (1, 'REF-002', 1, 120.00);

INSERT INTO ordenAsignacionMecanicos VALUES (2, 3);
INSERT INTO ordenDetallesServicios VALUES (2, 'SERV-003', 1200.00);
INSERT INTO ordenDetallesRefacciones (folioOrden, codigoRefaccion, cantidad, precioUnitarioMomento) VALUES 
(2, 'REF-005', 1, 850.00), (2, 'REF-006', 1, 600.00);

INSERT INTO ordenAsignacionMecanicos VALUES (3, 5);
INSERT INTO ordenDetallesServicios VALUES (3, 'SERV-002', 800.00);
INSERT INTO ordenDetallesRefacciones (folioOrden, codigoRefaccion, cantidad, precioUnitarioMomento) VALUES 
(3, 'REF-004', 5, 250.00);

INSERT INTO ordenAsignacionMecanicos VALUES (4, 2);
INSERT INTO ordenDetallesServicios VALUES (4, 'SERV-005', 300.00);


-- Consultas

SELECT * FROM clientes;
SELECT * FROM telefonosClientes;

-- Catálogos
SELECT * FROM mecanicos;
SELECT * FROM servicios;
SELECT * FROM refacciones;

SELECT * FROM vehiculos;

SELECT * FROM ordenesServicio;

-- Detalles de las Órdenes
SELECT * FROM ordenAsignacionMecanicos;
SELECT * FROM ordenDetallesServicios; 
SELECT * FROM ordenDetallesRefacciones;