-- ============================================================================
-- SkillHub Database Complete Export Script (Schema + Seed Data)
-- Target Database: SkillHubDb on localhost\TEW_SQLEXPRESS
-- Generated: 2026-10-27 16:15:56 UTC
-- ============================================================================

IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = N'SkillHubDb')
BEGIN
    CREATE DATABASE [SkillHubDb];
END;
GO

USE [SkillHubDb];
GO

-- ============================================================================
-- SECTION 1: DATABASE SCHEMA (TABLES, CONSTRAINTS, INDEXES)
-- ============================================================================
IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
GO

CREATE TABLE [AspNetRoles] (
    [Id] nvarchar(450) NOT NULL,
    [Name] nvarchar(256) NULL,
    [NormalizedName] nvarchar(256) NULL,
    [ConcurrencyStamp] nvarchar(max) NULL,
    CONSTRAINT [PK_AspNetRoles] PRIMARY KEY ([Id])
);
GO

CREATE TABLE [AspNetUsers] (
    [Id] nvarchar(450) NOT NULL,
    [FullName] nvarchar(200) NOT NULL,
    [Specialty] nvarchar(200) NULL,
    [UserName] nvarchar(256) NULL,
    [NormalizedUserName] nvarchar(256) NULL,
    [Email] nvarchar(256) NULL,
    [NormalizedEmail] nvarchar(256) NULL,
    [EmailConfirmed] bit NOT NULL,
    [PasswordHash] nvarchar(max) NULL,
    [SecurityStamp] nvarchar(max) NULL,
    [ConcurrencyStamp] nvarchar(max) NULL,
    [PhoneNumber] nvarchar(max) NULL,
    [PhoneNumberConfirmed] bit NOT NULL,
    [TwoFactorEnabled] bit NOT NULL,
    [LockoutEnd] datetimeoffset NULL,
    [LockoutEnabled] bit NOT NULL,
    [AccessFailedCount] int NOT NULL,
    CONSTRAINT [PK_AspNetUsers] PRIMARY KEY ([Id])
);
GO

CREATE TABLE [Instructors] (
    [Id] uniqueidentifier NOT NULL,
    [FullName] nvarchar(200) NOT NULL,
    [Email] nvarchar(256) NOT NULL,
    [Specialty] nvarchar(200) NOT NULL,
    [UserId] nvarchar(450) NULL,
    CONSTRAINT [PK_Instructors] PRIMARY KEY ([Id])
);
GO

CREATE TABLE [Participants] (
    [Id] uniqueidentifier NOT NULL,
    [FullName] nvarchar(200) NOT NULL,
    [Email] nvarchar(256) NOT NULL,
    [UserId] nvarchar(450) NULL,
    [RegisteredAt] datetime2 NOT NULL,
    CONSTRAINT [PK_Participants] PRIMARY KEY ([Id])
);
GO

CREATE TABLE [AspNetRoleClaims] (
    [Id] int NOT NULL IDENTITY,
    [RoleId] nvarchar(450) NOT NULL,
    [ClaimType] nvarchar(max) NULL,
    [ClaimValue] nvarchar(max) NULL,
    CONSTRAINT [PK_AspNetRoleClaims] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_AspNetRoleClaims_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [AspNetRoles] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [AspNetUserClaims] (
    [Id] int NOT NULL IDENTITY,
    [UserId] nvarchar(450) NOT NULL,
    [ClaimType] nvarchar(max) NULL,
    [ClaimValue] nvarchar(max) NULL,
    CONSTRAINT [PK_AspNetUserClaims] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_AspNetUserClaims_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [AspNetUserLogins] (
    [LoginProvider] nvarchar(450) NOT NULL,
    [ProviderKey] nvarchar(450) NOT NULL,
    [ProviderDisplayName] nvarchar(max) NULL,
    [UserId] nvarchar(450) NOT NULL,
    CONSTRAINT [PK_AspNetUserLogins] PRIMARY KEY ([LoginProvider], [ProviderKey]),
    CONSTRAINT [FK_AspNetUserLogins_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [AspNetUserRoles] (
    [UserId] nvarchar(450) NOT NULL,
    [RoleId] nvarchar(450) NOT NULL,
    CONSTRAINT [PK_AspNetUserRoles] PRIMARY KEY ([UserId], [RoleId]),
    CONSTRAINT [FK_AspNetUserRoles_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [AspNetRoles] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_AspNetUserRoles_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [AspNetUserTokens] (
    [UserId] nvarchar(450) NOT NULL,
    [LoginProvider] nvarchar(450) NOT NULL,
    [Name] nvarchar(450) NOT NULL,
    [Value] nvarchar(max) NULL,
    CONSTRAINT [PK_AspNetUserTokens] PRIMARY KEY ([UserId], [LoginProvider], [Name]),
    CONSTRAINT [FK_AspNetUserTokens_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [Workshops] (
    [Id] uniqueidentifier NOT NULL,
    [Code] nvarchar(50) NOT NULL,
    [Title] nvarchar(200) NOT NULL,
    [Description] nvarchar(2000) NOT NULL,
    [InstructorId] uniqueidentifier NOT NULL,
    [StartDateTime] datetime2 NOT NULL,
    [EndDateTime] datetime2 NOT NULL,
    [Capacity] int NOT NULL,
    CONSTRAINT [PK_Workshops] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Workshops_Instructors_InstructorId] FOREIGN KEY ([InstructorId]) REFERENCES [Instructors] ([Id]) ON DELETE NO ACTION
);
GO

CREATE TABLE [Enrollments] (
    [Id] uniqueidentifier NOT NULL,
    [ParticipantId] uniqueidentifier NOT NULL,
    [WorkshopId] uniqueidentifier NOT NULL,
    [Status] int NOT NULL,
    [EnrolledAt] datetime2 NOT NULL,
    [CancelledAt] datetime2 NULL,
    [CancelledBy] nvarchar(256) NULL,
    CONSTRAINT [PK_Enrollments] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Enrollments_Participants_ParticipantId] FOREIGN KEY ([ParticipantId]) REFERENCES [Participants] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_Enrollments_Workshops_WorkshopId] FOREIGN KEY ([WorkshopId]) REFERENCES [Workshops] ([Id]) ON DELETE CASCADE
);
GO

CREATE INDEX [IX_AspNetRoleClaims_RoleId] ON [AspNetRoleClaims] ([RoleId]);
GO

CREATE UNIQUE INDEX [RoleNameIndex] ON [AspNetRoles] ([NormalizedName]) WHERE [NormalizedName] IS NOT NULL;
GO

CREATE INDEX [IX_AspNetUserClaims_UserId] ON [AspNetUserClaims] ([UserId]);
GO

CREATE INDEX [IX_AspNetUserLogins_UserId] ON [AspNetUserLogins] ([UserId]);
GO

CREATE INDEX [IX_AspNetUserRoles_RoleId] ON [AspNetUserRoles] ([RoleId]);
GO

CREATE INDEX [EmailIndex] ON [AspNetUsers] ([NormalizedEmail]);
GO

CREATE UNIQUE INDEX [UserNameIndex] ON [AspNetUsers] ([NormalizedUserName]) WHERE [NormalizedUserName] IS NOT NULL;
GO

CREATE INDEX [IX_Enrollments_ParticipantId_WorkshopId] ON [Enrollments] ([ParticipantId], [WorkshopId]);
GO

CREATE INDEX [IX_Enrollments_WorkshopId] ON [Enrollments] ([WorkshopId]);
GO

CREATE INDEX [IX_Instructors_Email] ON [Instructors] ([Email]);
GO

CREATE INDEX [IX_Participants_Email] ON [Participants] ([Email]);
GO

CREATE UNIQUE INDEX [IX_Workshops_Code] ON [Workshops] ([Code]);
GO

CREATE INDEX [IX_Workshops_InstructorId] ON [Workshops] ([InstructorId]);
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20261027193346_InitialCreate', N'8.0.33');
GO

COMMIT;
GO



-- ============================================================================
-- DATA: AspNetRoles
-- ============================================================================
INSERT INTO [AspNetRoles] ([Id], [Name], [NormalizedName], [ConcurrencyStamp]) VALUES (N'05331ea0-c8ac-4176-964b-a231b0ec5781', N'Participant', N'PARTICIPANT', NULL);
INSERT INTO [AspNetRoles] ([Id], [Name], [NormalizedName], [ConcurrencyStamp]) VALUES (N'2cc3e93e-eb2f-40a8-a175-6b0ca19f3808', N'Employee', N'EMPLOYEE', NULL);
GO
-- 2 rows exported for AspNetRoles

-- ============================================================================
-- DATA: AspNetUsers
-- ============================================================================
INSERT INTO [AspNetUsers] ([Id], [FullName], [Specialty], [UserName], [NormalizedUserName], [Email], [NormalizedEmail], [EmailConfirmed], [PasswordHash], [SecurityStamp], [ConcurrencyStamp], [PhoneNumber], [PhoneNumberConfirmed], [TwoFactorEnabled], [LockoutEnd], [LockoutEnabled], [AccessFailedCount]) VALUES (N'1070822c-0401-4f75-8a6a-37b3923bdac9', N'Óscar Iván Guerrero Lozano', NULL, N'oscar.guerrero@correo.com', N'OSCAR.GUERRERO@CORREO.COM', N'oscar.guerrero@correo.com', N'OSCAR.GUERRERO@CORREO.COM', 1, N'AQAAAAIAAYagAAAAEGxuDXzhMi04pcNw0aJOmVDj2P+1ZDYnPUwJQckqorqkunYWNW3KTkjEDWsnitZy2A==', N'36FNDN7KSH5GBZXY7FX2SKRU77AXNODH', N'2ba8bcc8-3899-49dc-a2ca-2291d16966d1', NULL, 0, 0, NULL, 1, 0);
INSERT INTO [AspNetUsers] ([Id], [FullName], [Specialty], [UserName], [NormalizedUserName], [Email], [NormalizedEmail], [EmailConfirmed], [PasswordHash], [SecurityStamp], [ConcurrencyStamp], [PhoneNumber], [PhoneNumberConfirmed], [TwoFactorEnabled], [LockoutEnd], [LockoutEnabled], [AccessFailedCount]) VALUES (N'1b6ee890-c1a4-48ee-bcad-7e0977d47aba', N'Juliana Ortega Sánchez', NULL, N'juliana.ortega@correo.com', N'JULIANA.ORTEGA@CORREO.COM', N'juliana.ortega@correo.com', N'JULIANA.ORTEGA@CORREO.COM', 1, N'AQAAAAIAAYagAAAAEBn3RJruAVYzQpZlFgcuPMwjfumL+J6gncrRLacl4/d5UR6LE2CLqWXfHwccfIg7JQ==', N'MBDTXMJLMZ7BOZVWTG5OO4MRV627PRWB', N'daf58076-a7e5-4ca1-beba-ea38b1ab5ad8', NULL, 0, 0, NULL, 1, 0);
INSERT INTO [AspNetUsers] ([Id], [FullName], [Specialty], [UserName], [NormalizedUserName], [Email], [NormalizedEmail], [EmailConfirmed], [PasswordHash], [SecurityStamp], [ConcurrencyStamp], [PhoneNumber], [PhoneNumberConfirmed], [TwoFactorEnabled], [LockoutEnd], [LockoutEnabled], [AccessFailedCount]) VALUES (N'219ade49-1747-4bfb-8490-5770136e8fef', N'Hernán Darío Quintero Gil', N'Música', N'hernan.quintero@correo.com', N'HERNAN.QUINTERO@CORREO.COM', N'hernan.quintero@correo.com', N'HERNAN.QUINTERO@CORREO.COM', 1, N'AQAAAAIAAYagAAAAEHlXGvYZZGPEizQABjMFCNLd0Z4P9jFF7gKhZB4ih/nGPQhrlhDfpUixYaIxq0NxwQ==', N'DRZJPC5IQQDQMFC7ABGVZVYGU2YFPZMO', N'680211a1-b047-41f9-a3bb-a8cdfb3d3aa9', NULL, 0, 0, NULL, 1, 0);
INSERT INTO [AspNetUsers] ([Id], [FullName], [Specialty], [UserName], [NormalizedUserName], [Email], [NormalizedEmail], [EmailConfirmed], [PasswordHash], [SecurityStamp], [ConcurrencyStamp], [PhoneNumber], [PhoneNumberConfirmed], [TwoFactorEnabled], [LockoutEnd], [LockoutEnabled], [AccessFailedCount]) VALUES (N'28cd4165-b35d-4332-b899-dab7899a09d2', N'Carlos Eduardo Rincón Díaz', NULL, N'carlos.rincon@correo.com', N'CARLOS.RINCON@CORREO.COM', N'carlos.rincon@correo.com', N'CARLOS.RINCON@CORREO.COM', 1, N'AQAAAAIAAYagAAAAEHmrvcij8H93qY4qI10lbFo/Xme6T2zIC2bmuwCP+s0kfcdtzx53W2a/tLVvy3tlcg==', N'3BT5PS5OK3ALDGAE7LBG7SMHWZWYP4AV', N'ee2f083d-f152-4f0b-8451-64a3d29f6a11', NULL, 0, 0, NULL, 1, 0);
INSERT INTO [AspNetUsers] ([Id], [FullName], [Specialty], [UserName], [NormalizedUserName], [Email], [NormalizedEmail], [EmailConfirmed], [PasswordHash], [SecurityStamp], [ConcurrencyStamp], [PhoneNumber], [PhoneNumberConfirmed], [TwoFactorEnabled], [LockoutEnd], [LockoutEnabled], [AccessFailedCount]) VALUES (N'35f15c0a-d09c-416d-abdb-65fd3471cd21', N'Sofía Vargas', N'Fotografía', N'sofia.vargas@correo.com', N'SOFIA.VARGAS@CORREO.COM', N'sofia.vargas@correo.com', N'SOFIA.VARGAS@CORREO.COM', 1, N'AQAAAAIAAYagAAAAEJ5B3gJa3sBatr/5yp834o6CMU56HbKfz7DaoHvmr5WTVpWmrUGftGSW3o5yR+ehRw==', N'UETTLVGO42TMIKTMCAUXXXYYPI2G4AZB', N'4096aa88-3c55-4f4d-8218-120244c963b5', NULL, 0, 0, NULL, 1, 0);
INSERT INTO [AspNetUsers] ([Id], [FullName], [Specialty], [UserName], [NormalizedUserName], [Email], [NormalizedEmail], [EmailConfirmed], [PasswordHash], [SecurityStamp], [ConcurrencyStamp], [PhoneNumber], [PhoneNumberConfirmed], [TwoFactorEnabled], [LockoutEnd], [LockoutEnabled], [AccessFailedCount]) VALUES (N'3757a43d-9d2e-4047-bd29-7e46235c142d', N'Lucía Gómez', N'Oratoria', N'lucia.gomez@correo.com', N'LUCIA.GOMEZ@CORREO.COM', N'lucia.gomez@correo.com', N'LUCIA.GOMEZ@CORREO.COM', 1, N'AQAAAAIAAYagAAAAEPNpgPpRMF6vktDZaRQ5rWmu7vwrGMwjTJT1vxEVKuTEo2Wd+1vhnxSXh6zjFvI6dw==', N'64GIRHIRVYUSN5YAYKNFFFFVIFYHWO6G', N'35fdc2c3-1af5-4637-b464-53835156ba9d', NULL, 0, 0, NULL, 1, 0);
INSERT INTO [AspNetUsers] ([Id], [FullName], [Specialty], [UserName], [NormalizedUserName], [Email], [NormalizedEmail], [EmailConfirmed], [PasswordHash], [SecurityStamp], [ConcurrencyStamp], [PhoneNumber], [PhoneNumberConfirmed], [TwoFactorEnabled], [LockoutEnd], [LockoutEnabled], [AccessFailedCount]) VALUES (N'3e9b0b30-8f96-45fe-8fa7-4c5107619df1', N'Cristian David Becerra Mejía', NULL, N'cristian.becerra@correo.com', N'CRISTIAN.BECERRA@CORREO.COM', N'cristian.becerra@correo.com', N'CRISTIAN.BECERRA@CORREO.COM', 1, N'AQAAAAIAAYagAAAAEEitALGvp0asuFRC5PgJvl4bd7YtxwUL2OhJknqwIYOiwyeFuOxhI7cSJ/v8iYxktQ==', N'62T52AQTY3ONAQN5F2QY4HHEOJ7GUULD', N'20e0cb09-6598-4576-ae01-1730c9ce0e01', NULL, 0, 0, NULL, 1, 0);
INSERT INTO [AspNetUsers] ([Id], [FullName], [Specialty], [UserName], [NormalizedUserName], [Email], [NormalizedEmail], [EmailConfirmed], [PasswordHash], [SecurityStamp], [ConcurrencyStamp], [PhoneNumber], [PhoneNumberConfirmed], [TwoFactorEnabled], [LockoutEnd], [LockoutEnabled], [AccessFailedCount]) VALUES (N'443fae6e-502a-4f09-b4f7-41f3834aeee5', N'Jorge Andrés Pérez Rojas', NULL, N'jorge.perez@correo.com', N'JORGE.PEREZ@CORREO.COM', N'jorge.perez@correo.com', N'JORGE.PEREZ@CORREO.COM', 1, N'AQAAAAIAAYagAAAAEDp1B1mmp9abIPkKYyLGn5I3bzWtRIpO2LG4X7qtvwt5/I7VTKAyaviaCVFlb/Ukwg==', N'3B3MV6IVHSGNLZGABEWYCWCNKAAQKBPN', N'713ce2aa-0932-40d2-b576-5e6d1b2228cc', NULL, 0, 0, NULL, 1, 0);
INSERT INTO [AspNetUsers] ([Id], [FullName], [Specialty], [UserName], [NormalizedUserName], [Email], [NormalizedEmail], [EmailConfirmed], [PasswordHash], [SecurityStamp], [ConcurrencyStamp], [PhoneNumber], [PhoneNumberConfirmed], [TwoFactorEnabled], [LockoutEnd], [LockoutEnabled], [AccessFailedCount]) VALUES (N'47d0118c-2a21-4f6b-a636-992067d0f3ee', N'Alejandro Botero', NULL, N'alejandro.botero@skillhub.com', N'ALEJANDRO.BOTERO@SKILLHUB.COM', N'alejandro.botero@skillhub.com', N'ALEJANDRO.BOTERO@SKILLHUB.COM', 0, N'AQAAAAIAAYagAAAAELbTfLckmqs+8hUOo7dGouTLKuR8BSm3qhs741QCaqz65GPimZuOs78qRF2GXou25w==', N'QZATNEW4WKXWRFFSYFR62VEUVSBPZ7UG', N'cac00b45-c737-4174-b4a6-e1231155a367', NULL, 0, 0, NULL, 1, 0);
INSERT INTO [AspNetUsers] ([Id], [FullName], [Specialty], [UserName], [NormalizedUserName], [Email], [NormalizedEmail], [EmailConfirmed], [PasswordHash], [SecurityStamp], [ConcurrencyStamp], [PhoneNumber], [PhoneNumberConfirmed], [TwoFactorEnabled], [LockoutEnd], [LockoutEnabled], [AccessFailedCount]) VALUES (N'53fcada6-0a1a-4fa7-93d3-d525ab965ad1', N'Juan Camilo Restrepo Vélez', NULL, N'juan.restrepo@correo.com', N'JUAN.RESTREPO@CORREO.COM', N'juan.restrepo@correo.com', N'JUAN.RESTREPO@CORREO.COM', 1, N'AQAAAAIAAYagAAAAECO1xa4pZg+1R5a38KyYp2z8v4fbbAHifjythBFNAFNaR4L2DrGIvtF4Jhjc7n/Aqw==', N'4PBIXOBY774WMJ4NKSNZ7VD3JASKSVRJ', N'2243bbb2-9760-48f1-981c-288ffb9b2bab', NULL, 0, 0, NULL, 1, 0);
INSERT INTO [AspNetUsers] ([Id], [FullName], [Specialty], [UserName], [NormalizedUserName], [Email], [NormalizedEmail], [EmailConfirmed], [PasswordHash], [SecurityStamp], [ConcurrencyStamp], [PhoneNumber], [PhoneNumberConfirmed], [TwoFactorEnabled], [LockoutEnd], [LockoutEnabled], [AccessFailedCount]) VALUES (N'55c0fb1e-717e-4bd3-a13e-ed3db03a5ac2', N'Esteban Rodríguez Molina', NULL, N'esteban.rodriguez@correo.com', N'ESTEBAN.RODRIGUEZ@CORREO.COM', N'esteban.rodriguez@correo.com', N'ESTEBAN.RODRIGUEZ@CORREO.COM', 1, N'AQAAAAIAAYagAAAAEJ4Ua+OZTIET+zOlYWMXkpT2GGkQ+8j6QQPu+HchFomQzENCpIB0TS1V+vzWqwItLw==', N'ZASNMHM2ZP4TO623DM3VRE2TQP4LEMK6', N'd0380757-0c5a-4ffa-a44d-d5581b1961a9', NULL, 0, 0, NULL, 1, 0);
INSERT INTO [AspNetUsers] ([Id], [FullName], [Specialty], [UserName], [NormalizedUserName], [Email], [NormalizedEmail], [EmailConfirmed], [PasswordHash], [SecurityStamp], [ConcurrencyStamp], [PhoneNumber], [PhoneNumberConfirmed], [TwoFactorEnabled], [LockoutEnd], [LockoutEnabled], [AccessFailedCount]) VALUES (N'5638a9bc-b80b-464a-b795-a1f0474ede27', N'Kevin Stiven Mosquera Palacios', NULL, N'kevin.mosquera@correo.com', N'KEVIN.MOSQUERA@CORREO.COM', N'kevin.mosquera@correo.com', N'KEVIN.MOSQUERA@CORREO.COM', 1, N'AQAAAAIAAYagAAAAEOaxqKs0qbR8INPR5Te2hMzvRFkfFQ27ifaqpJQVMja0xbit4ERsz0xYH+bGv0IcMw==', N'RC7CYLAMX6432CCMAJ4KC2D3TDMESFKB', N'120a38e4-bc6a-43d7-8ebc-aec47e26aa6b', NULL, 0, 0, NULL, 1, 0);
INSERT INTO [AspNetUsers] ([Id], [FullName], [Specialty], [UserName], [NormalizedUserName], [Email], [NormalizedEmail], [EmailConfirmed], [PasswordHash], [SecurityStamp], [ConcurrencyStamp], [PhoneNumber], [PhoneNumberConfirmed], [TwoFactorEnabled], [LockoutEnd], [LockoutEnabled], [AccessFailedCount]) VALUES (N'566d1607-80e7-49ae-97e0-c852ddd779c0', N'Mateo Morales', N'Diseño', N'mateo.morales@correo.com', N'MATEO.MORALES@CORREO.COM', N'mateo.morales@correo.com', N'MATEO.MORALES@CORREO.COM', 1, N'AQAAAAIAAYagAAAAEGn9mxtyuTmYUND/xtr4x1XZUEezp/idniUvk0BKt52GCTJtKnkaDj2bb7GUzmrepA==', N'LM43WY2LOHUSMFEGIHNEXO66I2L45IBR', N'8a99e8f1-02e8-450d-a9f1-ad000bc3f74a', NULL, 0, 0, NULL, 1, 0);
INSERT INTO [AspNetUsers] ([Id], [FullName], [Specialty], [UserName], [NormalizedUserName], [Email], [NormalizedEmail], [EmailConfirmed], [PasswordHash], [SecurityStamp], [ConcurrencyStamp], [PhoneNumber], [PhoneNumberConfirmed], [TwoFactorEnabled], [LockoutEnd], [LockoutEnabled], [AccessFailedCount]) VALUES (N'56f149ab-fc54-4226-a6ec-e755f5e5a01a', N'Andrés Felipe Mejía Arango', N'Fotografía', N'andres.mejia@correo.com', N'ANDRES.MEJIA@CORREO.COM', N'andres.mejia@correo.com', N'ANDRES.MEJIA@CORREO.COM', 1, N'AQAAAAIAAYagAAAAEHQiSP3pyv7OT0/vRfTLOOQeqBSg0T0BjIprdcOXDA7w8v5L4TWqLYimvIuP7Eaj4w==', N'GDC4IBOPSR6QTFXZXX5AY6ERYASMJME4', N'9fd8670d-069d-4aef-90ae-61727e5cba5d', NULL, 0, 0, NULL, 1, 0);
INSERT INTO [AspNetUsers] ([Id], [FullName], [Specialty], [UserName], [NormalizedUserName], [Email], [NormalizedEmail], [EmailConfirmed], [PasswordHash], [SecurityStamp], [ConcurrencyStamp], [PhoneNumber], [PhoneNumberConfirmed], [TwoFactorEnabled], [LockoutEnd], [LockoutEnabled], [AccessFailedCount]) VALUES (N'5f2215fb-d9bd-402a-9386-b662a7cba9f7', N'Camila Torres', N'Cocina', N'camila.torres@correo.com', N'CAMILA.TORRES@CORREO.COM', N'camila.torres@correo.com', N'CAMILA.TORRES@CORREO.COM', 1, N'AQAAAAIAAYagAAAAEJUBebkt6Dgk4211eWclM+KyBeg6gGyhkBBEBEjV6Z6VNk8Ubg5DqDIPG+lcT/nLzQ==', N'NC7QZY2H3ZFGGNLMZSZOE2BRHCLHJQ3Z', N'9cf580f8-64aa-4b2c-a5e1-6ce74b87771f', NULL, 0, 0, NULL, 1, 0);
INSERT INTO [AspNetUsers] ([Id], [FullName], [Specialty], [UserName], [NormalizedUserName], [Email], [NormalizedEmail], [EmailConfirmed], [PasswordHash], [SecurityStamp], [ConcurrencyStamp], [PhoneNumber], [PhoneNumberConfirmed], [TwoFactorEnabled], [LockoutEnd], [LockoutEnabled], [AccessFailedCount]) VALUES (N'5fc7197e-312f-4756-ba8e-89ca8e300d19', N'Javier Alonso Cuéllar Pardo', NULL, N'javier.cuellar@correo.com', N'JAVIER.CUELLAR@CORREO.COM', N'javier.cuellar@correo.com', N'JAVIER.CUELLAR@CORREO.COM', 1, N'AQAAAAIAAYagAAAAEECgOfdB6bBfXhiV9y/rsbrQ6eRMPv+ZnqPri2NSxdBLCZ7PLyMUK4zDsbL3nidt3Q==', N'624HPIUP42JWT7GU2ARMV4FH7ZZMSWT2', N'4cdbb383-e3a9-49fd-a9ac-ddc3dcf5144b', NULL, 0, 0, NULL, 1, 0);
INSERT INTO [AspNetUsers] ([Id], [FullName], [Specialty], [UserName], [NormalizedUserName], [Email], [NormalizedEmail], [EmailConfirmed], [PasswordHash], [SecurityStamp], [ConcurrencyStamp], [PhoneNumber], [PhoneNumberConfirmed], [TwoFactorEnabled], [LockoutEnd], [LockoutEnabled], [AccessFailedCount]) VALUES (N'6ca61adc-c88b-4cef-b248-d746c52287b9', N'Diego Alejandro Muñoz Peña', NULL, N'diego.munoz@correo.com', N'DIEGO.MUNOZ@CORREO.COM', N'diego.munoz@correo.com', N'DIEGO.MUNOZ@CORREO.COM', 1, N'AQAAAAIAAYagAAAAEBayCtzAmZng3v0m+wUZJnN4oJciTxuPS4qVVnXYhVzTNFc5jszigdEUT48VKbPZyg==', N'2ETVT2KRWSUBO4TRCLZO2XCN2V44A4BS', N'aa965877-189b-45df-babf-af12bcce5be7', NULL, 0, 0, NULL, 1, 0);
INSERT INTO [AspNetUsers] ([Id], [FullName], [Specialty], [UserName], [NormalizedUserName], [Email], [NormalizedEmail], [EmailConfirmed], [PasswordHash], [SecurityStamp], [ConcurrencyStamp], [PhoneNumber], [PhoneNumberConfirmed], [TwoFactorEnabled], [LockoutEnd], [LockoutEnabled], [AccessFailedCount]) VALUES (N'7be19053-3bfb-4646-bd82-1f773433881c', N'Yuliana Patiño Osorio', NULL, N'yuliana.patino@correo.com', N'YULIANA.PATINO@CORREO.COM', N'yuliana.patino@correo.com', N'YULIANA.PATINO@CORREO.COM', 1, N'AQAAAAIAAYagAAAAED8C8Rh+zZxgrlFLK15Au8PgIYlLjwRCkErgopeTytAz+OLK2G+rphYraKdTdYJfng==', N'BBL2NKEUJICDOGN52Y6WRFW3G667ALEI', N'3f908fc6-4933-4a00-aef5-fd49151f6fca', NULL, 0, 0, NULL, 1, 0);
INSERT INTO [AspNetUsers] ([Id], [FullName], [Specialty], [UserName], [NormalizedUserName], [Email], [NormalizedEmail], [EmailConfirmed], [PasswordHash], [SecurityStamp], [ConcurrencyStamp], [PhoneNumber], [PhoneNumberConfirmed], [TwoFactorEnabled], [LockoutEnd], [LockoutEnabled], [AccessFailedCount]) VALUES (N'7cb0401b-8021-417f-8a26-4cb666ab34c8', N'Wilmer Alfonso Benítez Cruz', NULL, N'wilmer.benitez@correo.com', N'WILMER.BENITEZ@CORREO.COM', N'wilmer.benitez@correo.com', N'WILMER.BENITEZ@CORREO.COM', 1, N'AQAAAAIAAYagAAAAEEIW2tRgXO4yC8qmCGLdy9Xzys0eeLA6FFiJk/TfL/vzQuXJkDCtgrYNWsfYt8W16Q==', N'S4WBOWMMFSL5MPVGOWH4UPL35IV5UCZR', N'f58ad0b5-479f-4706-a104-8fd3e20dd630', NULL, 0, 0, NULL, 1, 0);
INSERT INTO [AspNetUsers] ([Id], [FullName], [Specialty], [UserName], [NormalizedUserName], [Email], [NormalizedEmail], [EmailConfirmed], [PasswordHash], [SecurityStamp], [ConcurrencyStamp], [PhoneNumber], [PhoneNumberConfirmed], [TwoFactorEnabled], [LockoutEnd], [LockoutEnabled], [AccessFailedCount]) VALUES (N'853398db-2721-4200-8346-38a0c0141f38', N'Sebastián Castro Duque', NULL, N'sebastian.castro@correo.com', N'SEBASTIAN.CASTRO@CORREO.COM', N'sebastian.castro@correo.com', N'SEBASTIAN.CASTRO@CORREO.COM', 1, N'AQAAAAIAAYagAAAAEGbOZELyRXEKVwUl5EZOvUSsjxEO6hPKakpc06JLD5SbhCtBkSJHxhga1vJ3ipHXfA==', N'HVOURZICSEOC65KOQGNZWK2WXM6K3Q2T', N'48d667bf-22d6-420f-b622-f203ebf6e870', NULL, 0, 0, NULL, 1, 0);
INSERT INTO [AspNetUsers] ([Id], [FullName], [Specialty], [UserName], [NormalizedUserName], [Email], [NormalizedEmail], [EmailConfirmed], [PasswordHash], [SecurityStamp], [ConcurrencyStamp], [PhoneNumber], [PhoneNumberConfirmed], [TwoFactorEnabled], [LockoutEnd], [LockoutEnabled], [AccessFailedCount]) VALUES (N'87c84754-dbc4-41de-8740-885d3e197cf9', N'Santiago Ramírez Henao', NULL, N'santiago.ramirez@correo.com', N'SANTIAGO.RAMIREZ@CORREO.COM', N'santiago.ramirez@correo.com', N'SANTIAGO.RAMIREZ@CORREO.COM', 1, N'AQAAAAIAAYagAAAAEDDXkC395vIFn51ESn0kUBdaEaZ25A6Quf2BUYjPIERGgUoNtuVHJq4vYu6sDliS5g==', N'SFUY52OR3L6P72YIAMHJMXEKLKATQYN6', N'dae068ca-eb57-498d-b1d7-a967574dcc99', NULL, 0, 0, NULL, 1, 0);
INSERT INTO [AspNetUsers] ([Id], [FullName], [Specialty], [UserName], [NormalizedUserName], [Email], [NormalizedEmail], [EmailConfirmed], [PasswordHash], [SecurityStamp], [ConcurrencyStamp], [PhoneNumber], [PhoneNumberConfirmed], [TwoFactorEnabled], [LockoutEnd], [LockoutEnabled], [AccessFailedCount]) VALUES (N'88a54ae8-b553-4de0-846d-f3673d708b04', N'Luis Fernando Arias Cano', NULL, N'luis.arias@correo.com', N'LUIS.ARIAS@CORREO.COM', N'luis.arias@correo.com', N'LUIS.ARIAS@CORREO.COM', 1, N'AQAAAAIAAYagAAAAEJN2Z5UvAm3AeCaFNCJS6kGn8QdjAXqnozkVNbw8gyw1U9kTCZNcod4H7sg3XUuVkA==', N'5HVK5NDHXKRAJUCX3N5KKX5G7TS4G5FP', N'772e75f8-f935-4f2d-9c39-bd8f92be1f72', NULL, 0, 0, NULL, 1, 0);
INSERT INTO [AspNetUsers] ([Id], [FullName], [Specialty], [UserName], [NormalizedUserName], [Email], [NormalizedEmail], [EmailConfirmed], [PasswordHash], [SecurityStamp], [ConcurrencyStamp], [PhoneNumber], [PhoneNumberConfirmed], [TwoFactorEnabled], [LockoutEnd], [LockoutEnabled], [AccessFailedCount]) VALUES (N'8c015a8e-9342-4b34-abaa-4b9c82771e87', N'Natalia Herrera Castaño', N'Diseño gráfico', N'natalia.herrera@correo.com', N'NATALIA.HERRERA@CORREO.COM', N'natalia.herrera@correo.com', N'NATALIA.HERRERA@CORREO.COM', 1, N'AQAAAAIAAYagAAAAEBn91yg9NcX6pJd/5h3Tn8PsTngRL7povgXl5yrO2AZQ5V3gk6MS5bAbFtWqekxmjA==', N'JERBLXZXZPLHAZ6HLC4RJD2J3ETZRX2Y', N'f3fef501-014b-49b1-bbd4-eff84c8d9f9c', NULL, 0, 0, NULL, 1, 0);
INSERT INTO [AspNetUsers] ([Id], [FullName], [Specialty], [UserName], [NormalizedUserName], [Email], [NormalizedEmail], [EmailConfirmed], [PasswordHash], [SecurityStamp], [ConcurrencyStamp], [PhoneNumber], [PhoneNumberConfirmed], [TwoFactorEnabled], [LockoutEnd], [LockoutEnabled], [AccessFailedCount]) VALUES (N'8c149f02-b1e0-4dda-90ae-189de5277e10', N'María Fernanda Torres León', NULL, N'maria.torres@correo.com', N'MARIA.TORRES@CORREO.COM', N'maria.torres@correo.com', N'MARIA.TORRES@CORREO.COM', 1, N'AQAAAAIAAYagAAAAEBSXtwmv45FPse/BZYdO5a+of9fHl7Vf5RAIBZAr0sb9HslmthB5mgn90nvl5/w0nA==', N'BB36BIRBDGGVYUZ5AYHART3LYA4LP7VG', N'ca740894-b148-4f0d-8fb9-f78845b9f401', NULL, 0, 0, NULL, 1, 0);
INSERT INTO [AspNetUsers] ([Id], [FullName], [Specialty], [UserName], [NormalizedUserName], [Email], [NormalizedEmail], [EmailConfirmed], [PasswordHash], [SecurityStamp], [ConcurrencyStamp], [PhoneNumber], [PhoneNumberConfirmed], [TwoFactorEnabled], [LockoutEnd], [LockoutEnabled], [AccessFailedCount]) VALUES (N'97da5cc2-8349-482b-8ebd-aed92e0db314', N'Sara Valencia Bermúdez', NULL, N'sara.valencia@correo.com', N'SARA.VALENCIA@CORREO.COM', N'sara.valencia@correo.com', N'SARA.VALENCIA@CORREO.COM', 1, N'AQAAAAIAAYagAAAAEAJHkNWdC9KA6NDec9aNTpf0NmCYdLIDAhkOk/3vicSJtbjuxK3TCOqUsQj6Un+QLA==', N'6LUIQ7JXFRUSDA5UL2B4JHWOE37RY3QH', N'f81720c2-51f8-40f3-9d8b-33259d224f2e', NULL, 0, 0, NULL, 1, 0);
INSERT INTO [AspNetUsers] ([Id], [FullName], [Specialty], [UserName], [NormalizedUserName], [Email], [NormalizedEmail], [EmailConfirmed], [PasswordHash], [SecurityStamp], [ConcurrencyStamp], [PhoneNumber], [PhoneNumberConfirmed], [TwoFactorEnabled], [LockoutEnd], [LockoutEnabled], [AccessFailedCount]) VALUES (N'9a05d623-d9a7-4288-8d68-afaed70774ff', N'Manuela Escobar Toro', NULL, N'manuela.escobar@correo.com', N'MANUELA.ESCOBAR@CORREO.COM', N'manuela.escobar@correo.com', N'MANUELA.ESCOBAR@CORREO.COM', 1, N'AQAAAAIAAYagAAAAEIpJhI7w3sNM3F86p5vwYucDhykxatPotRbq+huzLJkLNW5LcMGPW7Hz/XvJyzg1IA==', N'YO4SNAC7VOVGECHD6J6GOA6GO7TDQKHT', N'3aa4e66e-5598-44da-902d-62660862df81', NULL, 0, 0, NULL, 1, 0);
INSERT INTO [AspNetUsers] ([Id], [FullName], [Specialty], [UserName], [NormalizedUserName], [Email], [NormalizedEmail], [EmailConfirmed], [PasswordHash], [SecurityStamp], [ConcurrencyStamp], [PhoneNumber], [PhoneNumberConfirmed], [TwoFactorEnabled], [LockoutEnd], [LockoutEnabled], [AccessFailedCount]) VALUES (N'a280b2e2-3d0d-477b-bfae-4563bcbeb3c3', N'Isabella Navarro Fuentes', NULL, N'isabella.navarro@correo.com', N'ISABELLA.NAVARRO@CORREO.COM', N'isabella.navarro@correo.com', N'ISABELLA.NAVARRO@CORREO.COM', 1, N'AQAAAAIAAYagAAAAEFZk27geG7PPDXUKgJF/YGNmqg1yY3q8UN5do3lbWWWCQCBbUTjccU6uG5B2XYU0pQ==', N'I3PM3WSLOKBX34YAAEJOFWDPPZ7RLXO3', N'7ce10a78-3313-426a-b463-1f2e0aa506d1', NULL, 0, 0, NULL, 1, 0);
INSERT INTO [AspNetUsers] ([Id], [FullName], [Specialty], [UserName], [NormalizedUserName], [Email], [NormalizedEmail], [EmailConfirmed], [PasswordHash], [SecurityStamp], [ConcurrencyStamp], [PhoneNumber], [PhoneNumberConfirmed], [TwoFactorEnabled], [LockoutEnd], [LockoutEnabled], [AccessFailedCount]) VALUES (N'a3282c39-cb97-44bb-bd1e-f962097f1649', N'Valentina Ríos', N'Cocina', N'valentina.rios@correo.com', N'VALENTINA.RIOS@CORREO.COM', N'valentina.rios@correo.com', N'VALENTINA.RIOS@CORREO.COM', 1, N'AQAAAAIAAYagAAAAECLYSnPXutktVcF9M98+5JlQUW2MHIblDwdzfl3W+D7H9IfL+0+6m3RJXTDYPosF6Q==', N'EOER3GLSCRP6SVGCVW3N2APHASJ7QR3I', N'ff82bc90-7722-4822-94ae-9af16211e890', NULL, 0, 0, NULL, 1, 0);
INSERT INTO [AspNetUsers] ([Id], [FullName], [Specialty], [UserName], [NormalizedUserName], [Email], [NormalizedEmail], [EmailConfirmed], [PasswordHash], [SecurityStamp], [ConcurrencyStamp], [PhoneNumber], [PhoneNumberConfirmed], [TwoFactorEnabled], [LockoutEnd], [LockoutEnabled], [AccessFailedCount]) VALUES (N'a4627874-e2ef-42f4-84a2-00adfdc17b40', N'Mariana Londoño Álvarez', NULL, N'mariana.londono@correo.com', N'MARIANA.LONDONO@CORREO.COM', N'mariana.londono@correo.com', N'MARIANA.LONDONO@CORREO.COM', 1, N'AQAAAAIAAYagAAAAEBwy4wAB4gMJH/LVgCAPVOKFVcgCGr1DX8kuxdA3f4GAfVHuxVjklS8c9VFNA0dw4Q==', N'BEIC5VZXGJIQW6U6D7DKPXPQQC5X3EGY', N'ebec03a8-6f7c-4035-ae25-904efaa6902f', NULL, 0, 0, NULL, 1, 0);
INSERT INTO [AspNetUsers] ([Id], [FullName], [Specialty], [UserName], [NormalizedUserName], [Email], [NormalizedEmail], [EmailConfirmed], [PasswordHash], [SecurityStamp], [ConcurrencyStamp], [PhoneNumber], [PhoneNumberConfirmed], [TwoFactorEnabled], [LockoutEnd], [LockoutEnabled], [AccessFailedCount]) VALUES (N'a5e86c21-9621-4730-81be-962633d69426', N'Andrés Mauricio Salazar Ortiz', NULL, N'andres.salazar@correo.com', N'ANDRES.SALAZAR@CORREO.COM', N'andres.salazar@correo.com', N'ANDRES.SALAZAR@CORREO.COM', 1, N'AQAAAAIAAYagAAAAEC5zmS9FOXgCjxgALjYJOLoKopcutbaSzel7wA6n6SWFfXMbEkJjM8ETm3OZG7g2kg==', N'OVUCXNWFDUCGF63AJIGUQC4CVJ3GZPME', N'2487d03d-992b-4c0e-bf9a-44b50ec64383', NULL, 0, 0, NULL, 1, 0);
INSERT INTO [AspNetUsers] ([Id], [FullName], [Specialty], [UserName], [NormalizedUserName], [Email], [NormalizedEmail], [EmailConfirmed], [PasswordHash], [SecurityStamp], [ConcurrencyStamp], [PhoneNumber], [PhoneNumberConfirmed], [TwoFactorEnabled], [LockoutEnd], [LockoutEnabled], [AccessFailedCount]) VALUES (N'a6ebc84c-6c34-4e0a-a7c9-41f0fff16732', N'Ana María Jaramillo Gaviria', NULL, N'ana.jaramillo@correo.com', N'ANA.JARAMILLO@CORREO.COM', N'ana.jaramillo@correo.com', N'ANA.JARAMILLO@CORREO.COM', 1, N'AQAAAAIAAYagAAAAEDlcDQ+cHU5Av4Kw4CrZz6rCjqCSuCdS3hWkcecHh3crwOWZqGAb+cnrrRpHMyo/DQ==', N'F77IW2ZICPPOBIZCBUVOKG7P52LGZH5P', N'dbe49776-583a-48ea-abd5-8c8961de0cc4', NULL, 0, 0, NULL, 1, 0);
INSERT INTO [AspNetUsers] ([Id], [FullName], [Specialty], [UserName], [NormalizedUserName], [Email], [NormalizedEmail], [EmailConfirmed], [PasswordHash], [SecurityStamp], [ConcurrencyStamp], [PhoneNumber], [PhoneNumberConfirmed], [TwoFactorEnabled], [LockoutEnd], [LockoutEnabled], [AccessFailedCount]) VALUES (N'ad6937ab-ab40-4546-91d6-9af657372c06', N'Felipe Agudelo Marín', NULL, N'felipe.agudelo@correo.com', N'FELIPE.AGUDELO@CORREO.COM', N'felipe.agudelo@correo.com', N'FELIPE.AGUDELO@CORREO.COM', 1, N'AQAAAAIAAYagAAAAEKke6ALonbgcionC0t9O8oqbPGjh0rY1L697KhP22nFQrVNBa+3cNtycjo1/biU0lA==', N'3MQ2IGN4JVQGMYKYUGAYEJZEE7IPDP5T', N'f0265c94-79e6-473d-b0bb-f1b361690e8f', NULL, 0, 0, NULL, 1, 0);
INSERT INTO [AspNetUsers] ([Id], [FullName], [Specialty], [UserName], [NormalizedUserName], [Email], [NormalizedEmail], [EmailConfirmed], [PasswordHash], [SecurityStamp], [ConcurrencyStamp], [PhoneNumber], [PhoneNumberConfirmed], [TwoFactorEnabled], [LockoutEnd], [LockoutEnabled], [AccessFailedCount]) VALUES (N'b815f79a-4db6-4b12-a827-7f0eeae1d445', N'Valentina Gómez Ruiz', NULL, N'valentina.gomez@correo.com', N'VALENTINA.GOMEZ@CORREO.COM', N'valentina.gomez@correo.com', N'VALENTINA.GOMEZ@CORREO.COM', 1, N'AQAAAAIAAYagAAAAEIaQBop5+4/SRVjYPIL/eF2kdP6QfmuX82nuxILJQB0kO9xXLjxprGeD7LM3rqe+Sw==', N'ZGNEQVX745COMPXP4PKPQJ53HH7L53KF', N'c2168aae-cecb-46f7-bcca-09a9dbd67808', NULL, 0, 0, NULL, 1, 0);
INSERT INTO [AspNetUsers] ([Id], [FullName], [Specialty], [UserName], [NormalizedUserName], [Email], [NormalizedEmail], [EmailConfirmed], [PasswordHash], [SecurityStamp], [ConcurrencyStamp], [PhoneNumber], [PhoneNumberConfirmed], [TwoFactorEnabled], [LockoutEnd], [LockoutEnabled], [AccessFailedCount]) VALUES (N'bca2ae13-2fc3-4e64-b0cc-4bbcf99b096d', N'Laura Sofía Martínez Suárez', NULL, N'laura.martinez@correo.com', N'LAURA.MARTINEZ@CORREO.COM', N'laura.martinez@correo.com', N'LAURA.MARTINEZ@CORREO.COM', 1, N'AQAAAAIAAYagAAAAEPCwSDcWF734mHUEloHK6TjiUqb+gYS4okQz9Fj7vKbj4hC3bKmIX1fRU0rDSxhW7w==', N'OFVMNLFVQC7ZO6VVM4DU37G3GQK77HBK', N'87457bc1-42f0-4fe9-a347-d63380650e7d', NULL, 0, 0, NULL, 1, 0);
INSERT INTO [AspNetUsers] ([Id], [FullName], [Specialty], [UserName], [NormalizedUserName], [Email], [NormalizedEmail], [EmailConfirmed], [PasswordHash], [SecurityStamp], [ConcurrencyStamp], [PhoneNumber], [PhoneNumberConfirmed], [TwoFactorEnabled], [LockoutEnd], [LockoutEnabled], [AccessFailedCount]) VALUES (N'c315bc46-db61-4dbe-a245-b327b983273b', N'Luz Marina Ospina Ríos', NULL, N'luz.ospina@correo.com', N'LUZ.OSPINA@CORREO.COM', N'luz.ospina@correo.com', N'LUZ.OSPINA@CORREO.COM', 1, N'AQAAAAIAAYagAAAAEGqCuCbTTfUnWPIX5xbKkcmBX1IDd75PbvkHLSSrOaVKfk13JQB2vNEYndKlRksCZw==', N'OXQCJF2STL2S57TJIFV3C7PWPIFJ4KCQ', N'ceb2aaa5-49b4-47b9-a0cc-db24f8b4e42d', NULL, 0, 0, NULL, 1, 0);
INSERT INTO [AspNetUsers] ([Id], [FullName], [Specialty], [UserName], [NormalizedUserName], [Email], [NormalizedEmail], [EmailConfirmed], [PasswordHash], [SecurityStamp], [ConcurrencyStamp], [PhoneNumber], [PhoneNumberConfirmed], [TwoFactorEnabled], [LockoutEnd], [LockoutEnabled], [AccessFailedCount]) VALUES (N'c9c1acab-08dd-4cd6-b1d9-b10a61b20eee', N'Rosa Elena Barrios Polo', N'Cocina', N'rosa.barrios@correo.com', N'ROSA.BARRIOS@CORREO.COM', N'rosa.barrios@correo.com', N'ROSA.BARRIOS@CORREO.COM', 1, N'AQAAAAIAAYagAAAAEFV3/k9N0fP2RPbLTGaYzMyhD+DTsDIKPf0PgppmZl1F1UhKbp+fw88NbbMOX6nqwA==', N'JPLZCCNTNRW6JSCKRRWU4MWEMR3EDDIP', N'b6cdd260-ac06-4ac1-8ef6-34c3fff8bcc4', NULL, 0, 0, NULL, 1, 0);
INSERT INTO [AspNetUsers] ([Id], [FullName], [Specialty], [UserName], [NormalizedUserName], [Email], [NormalizedEmail], [EmailConfirmed], [PasswordHash], [SecurityStamp], [ConcurrencyStamp], [PhoneNumber], [PhoneNumberConfirmed], [TwoFactorEnabled], [LockoutEnd], [LockoutEnabled], [AccessFailedCount]) VALUES (N'ce0ab835-d640-4bfd-99fa-244386ac3537', N'Natalia Montoya Correa', NULL, N'natalia.montoya@correo.com', N'NATALIA.MONTOYA@CORREO.COM', N'natalia.montoya@correo.com', N'NATALIA.MONTOYA@CORREO.COM', 1, N'AQAAAAIAAYagAAAAEPO6bKhP4xGW0wL+MHnOCMY0Wq7YDQ9nyV9OnFgIN2yGD6aUcgA5lj4jdnMwUa3NIw==', N'XIQE35HHZYOX6GJBYZ7ED5FSRRYJ2KWF', N'05f136b2-65e7-40b0-8c34-5cd514ae1e7d', NULL, 0, 0, NULL, 1, 0);
INSERT INTO [AspNetUsers] ([Id], [FullName], [Specialty], [UserName], [NormalizedUserName], [Email], [NormalizedEmail], [EmailConfirmed], [PasswordHash], [SecurityStamp], [ConcurrencyStamp], [PhoneNumber], [PhoneNumberConfirmed], [TwoFactorEnabled], [LockoutEnd], [LockoutEnabled], [AccessFailedCount]) VALUES (N'd49a5514-df89-4ac4-a090-31c33a51bab4', N'Paola Andrea Vargas Mora', N'Marketing digital', N'paola.vargas@correo.com', N'PAOLA.VARGAS@CORREO.COM', N'paola.vargas@correo.com', N'PAOLA.VARGAS@CORREO.COM', 1, N'AQAAAAIAAYagAAAAEEzjyNkDMGlOkQU9BFufazCAY9IySdrR6hQoivo+LLa7EW380pcwvaK4Iejfu3h4bQ==', N'DKAMQJ3KA2GLSKFFFWFA7MKD4FOKXXON', N'4f40da20-bb1f-4ffc-8e15-983f04b0efcd', NULL, 0, 0, NULL, 1, 0);
INSERT INTO [AspNetUsers] ([Id], [FullName], [Specialty], [UserName], [NormalizedUserName], [Email], [NormalizedEmail], [EmailConfirmed], [PasswordHash], [SecurityStamp], [ConcurrencyStamp], [PhoneNumber], [PhoneNumberConfirmed], [TwoFactorEnabled], [LockoutEnd], [LockoutEnabled], [AccessFailedCount]) VALUES (N'ddd6e4ae-2401-4192-b618-72f1534f1a77', N'Camila Andrea Cárdenas Ruiz', NULL, N'camila.cardenas@correo.com', N'CAMILA.CARDENAS@CORREO.COM', N'camila.cardenas@correo.com', N'CAMILA.CARDENAS@CORREO.COM', 1, N'AQAAAAIAAYagAAAAEI1uBqopIPS9NZx17ef+9i6nhCPfCibz79QrouYI3TPWb8955ocu9kvh53t8KPrW8Q==', N'3XZP7CFMMERXFETYB4RN5TGWCBAYWTVU', N'5c95528f-5b4a-47aa-be2c-aa8e5f993e5d', NULL, 0, 0, NULL, 1, 0);
INSERT INTO [AspNetUsers] ([Id], [FullName], [Specialty], [UserName], [NormalizedUserName], [Email], [NormalizedEmail], [EmailConfirmed], [PasswordHash], [SecurityStamp], [ConcurrencyStamp], [PhoneNumber], [PhoneNumberConfirmed], [TwoFactorEnabled], [LockoutEnd], [LockoutEnabled], [AccessFailedCount]) VALUES (N'e5315f50-3ecb-498a-8a70-7e3936e39604', N'Paula Andrea Zapata Ríos', NULL, N'paula.zapata@correo.com', N'PAULA.ZAPATA@CORREO.COM', N'paula.zapata@correo.com', N'PAULA.ZAPATA@CORREO.COM', 1, N'AQAAAAIAAYagAAAAEBtUeV+FHo72rxZ1cG1+SwFMdnA0jc5p5D0UUz3gnziGnUUrNbDwY4Tn3SYI6uEcHw==', N'DZU3MG3KXIZKCX26D7NL3PYQ2VZE4OZG', N'0ae89ce0-3322-4597-971e-af0280f2183d', NULL, 0, 0, NULL, 1, 0);
INSERT INTO [AspNetUsers] ([Id], [FullName], [Specialty], [UserName], [NormalizedUserName], [Email], [NormalizedEmail], [EmailConfirmed], [PasswordHash], [SecurityStamp], [ConcurrencyStamp], [PhoneNumber], [PhoneNumberConfirmed], [TwoFactorEnabled], [LockoutEnd], [LockoutEnabled], [AccessFailedCount]) VALUES (N'f420c081-794f-4c1e-9cc8-c9550b9a7580', N'Daniela Morales Pineda', NULL, N'daniela.morales@correo.com', N'DANIELA.MORALES@CORREO.COM', N'daniela.morales@correo.com', N'DANIELA.MORALES@CORREO.COM', 1, N'AQAAAAIAAYagAAAAEEj78aw6/larKxMJDhw0TO5GZ2HvM+3QdcXHqJi/xrn/8EytjzyEtwfa+4W29C81SA==', N'D4SSAUZG3DOANBQVDY3D5AXLO7SKKLMQ', N'3db4754a-f3a0-4834-bad5-84b7c65fec38', NULL, 0, 0, NULL, 1, 0);
INSERT INTO [AspNetUsers] ([Id], [FullName], [Specialty], [UserName], [NormalizedUserName], [Email], [NormalizedEmail], [EmailConfirmed], [PasswordHash], [SecurityStamp], [ConcurrencyStamp], [PhoneNumber], [PhoneNumberConfirmed], [TwoFactorEnabled], [LockoutEnd], [LockoutEnabled], [AccessFailedCount]) VALUES (N'ff66263e-e5a6-461f-8916-5368d6904195', N'Mauricio Jiménez Cárdenas', N'Oratoria', N'mauricio.jimenez@correo.com', N'MAURICIO.JIMENEZ@CORREO.COM', N'mauricio.jimenez@correo.com', N'MAURICIO.JIMENEZ@CORREO.COM', 1, N'AQAAAAIAAYagAAAAEMqFtAitjcJPd2mBFVYnk/28ZlMS3PQc/lQn6EMI5Yhp98H8QYJw0NrrHrNPXdnvkQ==', N'FIPSLXQUUUXPX7FECJCIQSUYEHQ527JA', N'da2bd855-6f92-4387-bc5d-4738a8d7cc3f', NULL, 0, 0, NULL, 1, 0);
GO
-- 42 rows exported for AspNetUsers

-- ============================================================================
-- DATA: AspNetUserRoles
-- ============================================================================
INSERT INTO [AspNetUserRoles] ([UserId], [RoleId]) VALUES (N'1070822c-0401-4f75-8a6a-37b3923bdac9', N'05331ea0-c8ac-4176-964b-a231b0ec5781');
INSERT INTO [AspNetUserRoles] ([UserId], [RoleId]) VALUES (N'1b6ee890-c1a4-48ee-bcad-7e0977d47aba', N'05331ea0-c8ac-4176-964b-a231b0ec5781');
INSERT INTO [AspNetUserRoles] ([UserId], [RoleId]) VALUES (N'28cd4165-b35d-4332-b899-dab7899a09d2', N'05331ea0-c8ac-4176-964b-a231b0ec5781');
INSERT INTO [AspNetUserRoles] ([UserId], [RoleId]) VALUES (N'3e9b0b30-8f96-45fe-8fa7-4c5107619df1', N'05331ea0-c8ac-4176-964b-a231b0ec5781');
INSERT INTO [AspNetUserRoles] ([UserId], [RoleId]) VALUES (N'443fae6e-502a-4f09-b4f7-41f3834aeee5', N'05331ea0-c8ac-4176-964b-a231b0ec5781');
INSERT INTO [AspNetUserRoles] ([UserId], [RoleId]) VALUES (N'47d0118c-2a21-4f6b-a636-992067d0f3ee', N'05331ea0-c8ac-4176-964b-a231b0ec5781');
INSERT INTO [AspNetUserRoles] ([UserId], [RoleId]) VALUES (N'53fcada6-0a1a-4fa7-93d3-d525ab965ad1', N'05331ea0-c8ac-4176-964b-a231b0ec5781');
INSERT INTO [AspNetUserRoles] ([UserId], [RoleId]) VALUES (N'55c0fb1e-717e-4bd3-a13e-ed3db03a5ac2', N'05331ea0-c8ac-4176-964b-a231b0ec5781');
INSERT INTO [AspNetUserRoles] ([UserId], [RoleId]) VALUES (N'5638a9bc-b80b-464a-b795-a1f0474ede27', N'05331ea0-c8ac-4176-964b-a231b0ec5781');
INSERT INTO [AspNetUserRoles] ([UserId], [RoleId]) VALUES (N'5fc7197e-312f-4756-ba8e-89ca8e300d19', N'05331ea0-c8ac-4176-964b-a231b0ec5781');
INSERT INTO [AspNetUserRoles] ([UserId], [RoleId]) VALUES (N'6ca61adc-c88b-4cef-b248-d746c52287b9', N'05331ea0-c8ac-4176-964b-a231b0ec5781');
INSERT INTO [AspNetUserRoles] ([UserId], [RoleId]) VALUES (N'7be19053-3bfb-4646-bd82-1f773433881c', N'05331ea0-c8ac-4176-964b-a231b0ec5781');
INSERT INTO [AspNetUserRoles] ([UserId], [RoleId]) VALUES (N'7cb0401b-8021-417f-8a26-4cb666ab34c8', N'05331ea0-c8ac-4176-964b-a231b0ec5781');
INSERT INTO [AspNetUserRoles] ([UserId], [RoleId]) VALUES (N'853398db-2721-4200-8346-38a0c0141f38', N'05331ea0-c8ac-4176-964b-a231b0ec5781');
INSERT INTO [AspNetUserRoles] ([UserId], [RoleId]) VALUES (N'87c84754-dbc4-41de-8740-885d3e197cf9', N'05331ea0-c8ac-4176-964b-a231b0ec5781');
INSERT INTO [AspNetUserRoles] ([UserId], [RoleId]) VALUES (N'88a54ae8-b553-4de0-846d-f3673d708b04', N'05331ea0-c8ac-4176-964b-a231b0ec5781');
INSERT INTO [AspNetUserRoles] ([UserId], [RoleId]) VALUES (N'8c149f02-b1e0-4dda-90ae-189de5277e10', N'05331ea0-c8ac-4176-964b-a231b0ec5781');
INSERT INTO [AspNetUserRoles] ([UserId], [RoleId]) VALUES (N'97da5cc2-8349-482b-8ebd-aed92e0db314', N'05331ea0-c8ac-4176-964b-a231b0ec5781');
INSERT INTO [AspNetUserRoles] ([UserId], [RoleId]) VALUES (N'9a05d623-d9a7-4288-8d68-afaed70774ff', N'05331ea0-c8ac-4176-964b-a231b0ec5781');
INSERT INTO [AspNetUserRoles] ([UserId], [RoleId]) VALUES (N'a280b2e2-3d0d-477b-bfae-4563bcbeb3c3', N'05331ea0-c8ac-4176-964b-a231b0ec5781');
INSERT INTO [AspNetUserRoles] ([UserId], [RoleId]) VALUES (N'a4627874-e2ef-42f4-84a2-00adfdc17b40', N'05331ea0-c8ac-4176-964b-a231b0ec5781');
INSERT INTO [AspNetUserRoles] ([UserId], [RoleId]) VALUES (N'a5e86c21-9621-4730-81be-962633d69426', N'05331ea0-c8ac-4176-964b-a231b0ec5781');
INSERT INTO [AspNetUserRoles] ([UserId], [RoleId]) VALUES (N'a6ebc84c-6c34-4e0a-a7c9-41f0fff16732', N'05331ea0-c8ac-4176-964b-a231b0ec5781');
INSERT INTO [AspNetUserRoles] ([UserId], [RoleId]) VALUES (N'ad6937ab-ab40-4546-91d6-9af657372c06', N'05331ea0-c8ac-4176-964b-a231b0ec5781');
INSERT INTO [AspNetUserRoles] ([UserId], [RoleId]) VALUES (N'b815f79a-4db6-4b12-a827-7f0eeae1d445', N'05331ea0-c8ac-4176-964b-a231b0ec5781');
INSERT INTO [AspNetUserRoles] ([UserId], [RoleId]) VALUES (N'bca2ae13-2fc3-4e64-b0cc-4bbcf99b096d', N'05331ea0-c8ac-4176-964b-a231b0ec5781');
INSERT INTO [AspNetUserRoles] ([UserId], [RoleId]) VALUES (N'c315bc46-db61-4dbe-a245-b327b983273b', N'05331ea0-c8ac-4176-964b-a231b0ec5781');
INSERT INTO [AspNetUserRoles] ([UserId], [RoleId]) VALUES (N'ce0ab835-d640-4bfd-99fa-244386ac3537', N'05331ea0-c8ac-4176-964b-a231b0ec5781');
INSERT INTO [AspNetUserRoles] ([UserId], [RoleId]) VALUES (N'ddd6e4ae-2401-4192-b618-72f1534f1a77', N'05331ea0-c8ac-4176-964b-a231b0ec5781');
INSERT INTO [AspNetUserRoles] ([UserId], [RoleId]) VALUES (N'e5315f50-3ecb-498a-8a70-7e3936e39604', N'05331ea0-c8ac-4176-964b-a231b0ec5781');
INSERT INTO [AspNetUserRoles] ([UserId], [RoleId]) VALUES (N'f420c081-794f-4c1e-9cc8-c9550b9a7580', N'05331ea0-c8ac-4176-964b-a231b0ec5781');
INSERT INTO [AspNetUserRoles] ([UserId], [RoleId]) VALUES (N'219ade49-1747-4bfb-8490-5770136e8fef', N'2cc3e93e-eb2f-40a8-a175-6b0ca19f3808');
INSERT INTO [AspNetUserRoles] ([UserId], [RoleId]) VALUES (N'35f15c0a-d09c-416d-abdb-65fd3471cd21', N'2cc3e93e-eb2f-40a8-a175-6b0ca19f3808');
INSERT INTO [AspNetUserRoles] ([UserId], [RoleId]) VALUES (N'3757a43d-9d2e-4047-bd29-7e46235c142d', N'2cc3e93e-eb2f-40a8-a175-6b0ca19f3808');
INSERT INTO [AspNetUserRoles] ([UserId], [RoleId]) VALUES (N'566d1607-80e7-49ae-97e0-c852ddd779c0', N'2cc3e93e-eb2f-40a8-a175-6b0ca19f3808');
INSERT INTO [AspNetUserRoles] ([UserId], [RoleId]) VALUES (N'56f149ab-fc54-4226-a6ec-e755f5e5a01a', N'2cc3e93e-eb2f-40a8-a175-6b0ca19f3808');
INSERT INTO [AspNetUserRoles] ([UserId], [RoleId]) VALUES (N'5f2215fb-d9bd-402a-9386-b662a7cba9f7', N'2cc3e93e-eb2f-40a8-a175-6b0ca19f3808');
INSERT INTO [AspNetUserRoles] ([UserId], [RoleId]) VALUES (N'8c015a8e-9342-4b34-abaa-4b9c82771e87', N'2cc3e93e-eb2f-40a8-a175-6b0ca19f3808');
INSERT INTO [AspNetUserRoles] ([UserId], [RoleId]) VALUES (N'a3282c39-cb97-44bb-bd1e-f962097f1649', N'2cc3e93e-eb2f-40a8-a175-6b0ca19f3808');
INSERT INTO [AspNetUserRoles] ([UserId], [RoleId]) VALUES (N'c9c1acab-08dd-4cd6-b1d9-b10a61b20eee', N'2cc3e93e-eb2f-40a8-a175-6b0ca19f3808');
INSERT INTO [AspNetUserRoles] ([UserId], [RoleId]) VALUES (N'd49a5514-df89-4ac4-a090-31c33a51bab4', N'2cc3e93e-eb2f-40a8-a175-6b0ca19f3808');
INSERT INTO [AspNetUserRoles] ([UserId], [RoleId]) VALUES (N'ff66263e-e5a6-461f-8916-5368d6904195', N'2cc3e93e-eb2f-40a8-a175-6b0ca19f3808');
GO
-- 42 rows exported for AspNetUserRoles

-- ============================================================================
-- DATA: Instructors
-- ============================================================================
INSERT INTO [Instructors] ([Id], [FullName], [Email], [Specialty], [UserId]) VALUES ('9331335b-b902-4018-82f7-1cbb9be6228f', N'Mateo Morales', N'mateo.morales@correo.com', N'Diseño', N'566d1607-80e7-49ae-97e0-c852ddd779c0');
INSERT INTO [Instructors] ([Id], [FullName], [Email], [Specialty], [UserId]) VALUES ('18344c5b-bac7-4b12-b2c1-28a77fac942b', N'Mauricio Jiménez Cárdenas', N'mauricio.jimenez@correo.com', N'Oratoria', N'ff66263e-e5a6-461f-8916-5368d6904195');
INSERT INTO [Instructors] ([Id], [FullName], [Email], [Specialty], [UserId]) VALUES ('82822c0d-4c81-4972-a988-29f8ab906134', N'Lucía Gómez', N'lucia.gomez@correo.com', N'Oratoria', N'3757a43d-9d2e-4047-bd29-7e46235c142d');
INSERT INTO [Instructors] ([Id], [FullName], [Email], [Specialty], [UserId]) VALUES ('2a628ce5-ebdb-4ce2-a246-45c5350a9327', N'Rosa Elena Barrios Polo', N'rosa.barrios@correo.com', N'Cocina', N'c9c1acab-08dd-4cd6-b1d9-b10a61b20eee');
INSERT INTO [Instructors] ([Id], [FullName], [Email], [Specialty], [UserId]) VALUES ('a661164e-821b-4191-a921-5dd725337ee9', N'Sofía Vargas', N'sofia.vargas@correo.com', N'Fotografía', N'35f15c0a-d09c-416d-abdb-65fd3471cd21');
INSERT INTO [Instructors] ([Id], [FullName], [Email], [Specialty], [UserId]) VALUES ('cc0f26da-6b90-49d8-a04a-69a55331c47b', N'Natalia Herrera Castaño', N'natalia.herrera@correo.com', N'Diseño gráfico', N'8c015a8e-9342-4b34-abaa-4b9c82771e87');
INSERT INTO [Instructors] ([Id], [FullName], [Email], [Specialty], [UserId]) VALUES ('70738b5e-1920-482c-a520-7ea77dcc80f0', N'Hernán Darío Quintero Gil', N'hernan.quintero@correo.com', N'Música', N'219ade49-1747-4bfb-8490-5770136e8fef');
INSERT INTO [Instructors] ([Id], [FullName], [Email], [Specialty], [UserId]) VALUES ('2fcb7d66-10cb-4e4f-86ce-96dcf3d5ebb7', N'Valentina Ríos', N'valentina.rios@correo.com', N'Cocina', N'a3282c39-cb97-44bb-bd1e-f962097f1649');
INSERT INTO [Instructors] ([Id], [FullName], [Email], [Specialty], [UserId]) VALUES ('829ed046-376a-4073-a011-d826c3a42242', N'Camila Torres', N'camila.torres@correo.com', N'Cocina', N'5f2215fb-d9bd-402a-9386-b662a7cba9f7');
INSERT INTO [Instructors] ([Id], [FullName], [Email], [Specialty], [UserId]) VALUES ('eec6d12d-6a94-4071-8e3e-e18a76abe66f', N'Andrés Felipe Mejía Arango', N'andres.mejia@correo.com', N'Fotografía', N'56f149ab-fc54-4226-a6ec-e755f5e5a01a');
INSERT INTO [Instructors] ([Id], [FullName], [Email], [Specialty], [UserId]) VALUES ('306a7da4-484c-45c8-bff4-e51cfcfa3c13', N'Paola Andrea Vargas Mora', N'paola.vargas@correo.com', N'Marketing digital', N'd49a5514-df89-4ac4-a090-31c33a51bab4');
GO
-- 11 rows exported for Instructors

-- ============================================================================
-- DATA: Participants
-- ============================================================================
INSERT INTO [Participants] ([Id], [FullName], [Email], [UserId], [RegisteredAt]) VALUES ('6e8f9d41-12f6-4f97-90b2-080084dec910', N'Wilmer Alfonso Benítez Cruz', N'wilmer.benitez@correo.com', N'7cb0401b-8021-417f-8a26-4cb666ab34c8', '2026-10-27 20:02:33.752820');
INSERT INTO [Participants] ([Id], [FullName], [Email], [UserId], [RegisteredAt]) VALUES ('beee8187-3711-4a29-bf44-0de98e7d8ba1', N'Santiago Ramírez Henao', N'santiago.ramirez@correo.com', N'87c84754-dbc4-41de-8740-885d3e197cf9', '2026-10-27 20:02:31.081864');
INSERT INTO [Participants] ([Id], [FullName], [Email], [UserId], [RegisteredAt]) VALUES ('53a6da0b-0f7c-4d52-af15-229b8d395d30', N'Luis Fernando Arias Cano', N'luis.arias@correo.com', N'88a54ae8-b553-4de0-846d-f3673d708b04', '2026-10-27 20:02:33.304293');
INSERT INTO [Participants] ([Id], [FullName], [Email], [UserId], [RegisteredAt]) VALUES ('6cefa4c3-fcf2-4bda-9a53-25c15b33c5e1', N'Paula Andrea Zapata Ríos', N'paula.zapata@correo.com', N'e5315f50-3ecb-498a-8a70-7e3936e39604', '2026-10-27 20:02:32.340677');
INSERT INTO [Participants] ([Id], [FullName], [Email], [UserId], [RegisteredAt]) VALUES ('288c0ebc-3e50-4a22-8435-296eeb18ab0f', N'María Fernanda Torres León', N'maria.torres@correo.com', N'8c149f02-b1e0-4dda-90ae-189de5277e10', '2026-10-27 20:02:30.958880');
INSERT INTO [Participants] ([Id], [FullName], [Email], [UserId], [RegisteredAt]) VALUES ('c01e07b3-1789-41fe-8e16-2e14b4b36925', N'Camila Andrea Cárdenas Ruiz', N'camila.cardenas@correo.com', N'ddd6e4ae-2401-4192-b618-72f1534f1a77', '2026-10-27 20:02:31.649993');
INSERT INTO [Participants] ([Id], [FullName], [Email], [UserId], [RegisteredAt]) VALUES ('95fbf0f7-a4a5-4ecc-9082-2ea1701ff044', N'Felipe Agudelo Marín', N'felipe.agudelo@correo.com', N'ad6937ab-ab40-4546-91d6-9af657372c06', '2026-10-27 20:02:32.001590');
INSERT INTO [Participants] ([Id], [FullName], [Email], [UserId], [RegisteredAt]) VALUES ('7f9a54cd-46de-40cd-9f69-39140d9b671c', N'Manuela Escobar Toro', N'manuela.escobar@correo.com', N'9a05d623-d9a7-4288-8d68-afaed70774ff', '2026-10-27 20:02:33.634276');
INSERT INTO [Participants] ([Id], [FullName], [Email], [UserId], [RegisteredAt]) VALUES ('486d6688-1e7c-4e6a-b36e-42cc76c1d16d', N'Isabella Navarro Fuentes', N'isabella.navarro@correo.com', N'a280b2e2-3d0d-477b-bfae-4563bcbeb3c3', '2026-10-27 20:02:32.787389');
INSERT INTO [Participants] ([Id], [FullName], [Email], [UserId], [RegisteredAt]) VALUES ('542b4506-4f29-4a58-a2d3-5500e3c5433c', N'Jorge Andrés Pérez Rojas', N'jorge.perez@correo.com', N'443fae6e-502a-4f09-b4f7-41f3834aeee5', '2026-10-27 20:02:30.323747');
INSERT INTO [Participants] ([Id], [FullName], [Email], [UserId], [RegisteredAt]) VALUES ('331c37bb-5a17-40ee-ac3f-560b0db4c6d4', N'Alejandro Botero', N'alejandro.botero@skillhub.com', N'47d0118c-2a21-4f6b-a636-992067d0f3ee', '2026-10-27 20:04:01.743494');
INSERT INTO [Participants] ([Id], [FullName], [Email], [UserId], [RegisteredAt]) VALUES ('c69ffb99-f9ba-4673-8aa3-632a8ba24d32', N'Juan Camilo Restrepo Vélez', N'juan.restrepo@correo.com', N'53fcada6-0a1a-4fa7-93d3-d525ab965ad1', '2026-10-27 20:02:30.838652');
INSERT INTO [Participants] ([Id], [FullName], [Email], [UserId], [RegisteredAt]) VALUES ('0a9d3a7d-5df0-4403-a877-6e5de16fd73c', N'Óscar Iván Guerrero Lozano', N'oscar.guerrero@correo.com', N'1070822c-0401-4f75-8a6a-37b3923bdac9', '2026-10-27 20:02:32.216362');
INSERT INTO [Participants] ([Id], [FullName], [Email], [UserId], [RegisteredAt]) VALUES ('7c0efc21-19f9-46d0-b48d-6f1e40edda2d', N'Valentina Gómez Ruiz', N'valentina.gomez@correo.com', N'b815f79a-4db6-4b12-a827-7f0eeae1d445', '2026-10-27 20:02:30.702989');
INSERT INTO [Participants] ([Id], [FullName], [Email], [UserId], [RegisteredAt]) VALUES ('14e52f94-fd24-4967-b2e4-6fba45c45035', N'Cristian David Becerra Mejía', N'cristian.becerra@correo.com', N'3e9b0b30-8f96-45fe-8fa7-4c5107619df1', '2026-10-27 20:02:32.458061');
INSERT INTO [Participants] ([Id], [FullName], [Email], [UserId], [RegisteredAt]) VALUES ('3dba0cd2-d6cf-4d6b-b09c-756bc85d9b11', N'Daniela Morales Pineda', N'daniela.morales@correo.com', N'f420c081-794f-4c1e-9cc8-c9550b9a7580', '2026-10-27 20:02:31.185855');
INSERT INTO [Participants] ([Id], [FullName], [Email], [UserId], [RegisteredAt]) VALUES ('7909a796-c5f4-4b7a-84ad-7969ee15d7da', N'Yuliana Patiño Osorio', N'yuliana.patino@correo.com', N'7be19053-3bfb-4646-bd82-1f773433881c', '2026-10-27 20:02:33.865133');
INSERT INTO [Participants] ([Id], [FullName], [Email], [UserId], [RegisteredAt]) VALUES ('399dec69-e7fd-41c9-a472-90af8a15cae2', N'Javier Alonso Cuéllar Pardo', N'javier.cuellar@correo.com', N'5fc7197e-312f-4756-ba8e-89ca8e300d19', '2026-10-27 20:02:32.683651');
INSERT INTO [Participants] ([Id], [FullName], [Email], [UserId], [RegisteredAt]) VALUES ('0122e1ae-9a65-4b77-b420-925576b0566c', N'Sebastián Castro Duque', N'sebastian.castro@correo.com', N'853398db-2721-4200-8346-38a0c0141f38', '2026-10-27 20:02:31.300053');
INSERT INTO [Participants] ([Id], [FullName], [Email], [UserId], [RegisteredAt]) VALUES ('c2d18343-6865-4c39-9806-a1e0e34ddec0', N'Esteban Rodríguez Molina', N'esteban.rodriguez@correo.com', N'55c0fb1e-717e-4bd3-a13e-ed3db03a5ac2', '2026-10-27 20:02:33.524005');
INSERT INTO [Participants] ([Id], [FullName], [Email], [UserId], [RegisteredAt]) VALUES ('0cc05177-8156-442e-8245-a770d2b48e69', N'Ana María Jaramillo Gaviria', N'ana.jaramillo@correo.com', N'a6ebc84c-6c34-4e0a-a7c9-41f0fff16732', '2026-10-27 20:02:33.191312');
INSERT INTO [Participants] ([Id], [FullName], [Email], [UserId], [RegisteredAt]) VALUES ('a402e95b-6e79-43c9-be21-ab193f66ac92', N'Sara Valencia Bermúdez', N'sara.valencia@correo.com', N'97da5cc2-8349-482b-8ebd-aed92e0db314', '2026-10-27 20:02:33.417988');
INSERT INTO [Participants] ([Id], [FullName], [Email], [UserId], [RegisteredAt]) VALUES ('9fbbf751-8d7d-42b2-a115-b17da0f1a079', N'Luz Marina Ospina Ríos', N'luz.ospina@correo.com', N'c315bc46-db61-4dbe-a245-b327b983273b', '2026-10-27 20:02:30.494236');
INSERT INTO [Participants] ([Id], [FullName], [Email], [UserId], [RegisteredAt]) VALUES ('c9ad9dab-d844-44b1-aa8c-c4c6b008c02a', N'Diego Alejandro Muñoz Peña', N'diego.munoz@correo.com', N'6ca61adc-c88b-4cef-b248-d746c52287b9', '2026-10-27 20:02:31.760593');
INSERT INTO [Participants] ([Id], [FullName], [Email], [UserId], [RegisteredAt]) VALUES ('bfcadc54-4c9f-4d4d-adff-ccaaabb269b2', N'Juliana Ortega Sánchez', N'juliana.ortega@correo.com', N'1b6ee890-c1a4-48ee-bcad-7e0977d47aba', '2026-10-27 20:02:31.868013');
INSERT INTO [Participants] ([Id], [FullName], [Email], [UserId], [RegisteredAt]) VALUES ('c522ff8a-2aec-49d2-9205-cd8436cb9941', N'Natalia Montoya Correa', N'natalia.montoya@correo.com', N'ce0ab835-d640-4bfd-99fa-244386ac3537', '2026-10-27 20:02:32.107634');
INSERT INTO [Participants] ([Id], [FullName], [Email], [UserId], [RegisteredAt]) VALUES ('fc39b88b-56fa-4b2f-83c0-cf9b68c5c984', N'Kevin Stiven Mosquera Palacios', N'kevin.mosquera@correo.com', N'5638a9bc-b80b-464a-b795-a1f0474ede27', '2026-10-27 20:02:33.036290');
INSERT INTO [Participants] ([Id], [FullName], [Email], [UserId], [RegisteredAt]) VALUES ('cc346d1c-a32f-4de8-a7e7-e0afc07912a7', N'Laura Sofía Martínez Suárez', N'laura.martinez@correo.com', N'bca2ae13-2fc3-4e64-b0cc-4bbcf99b096d', '2026-10-27 20:02:31.415904');
INSERT INTO [Participants] ([Id], [FullName], [Email], [UserId], [RegisteredAt]) VALUES ('9068b0b8-2308-40a8-b89c-e281b716acdd', N'Carlos Eduardo Rincón Díaz', N'carlos.rincon@correo.com', N'28cd4165-b35d-4332-b899-dab7899a09d2', '2026-10-27 20:02:30.602285');
INSERT INTO [Participants] ([Id], [FullName], [Email], [UserId], [RegisteredAt]) VALUES ('dacdd316-d5a2-4b3a-9c03-e4382b1aa28f', N'Mariana Londoño Álvarez', N'mariana.londono@correo.com', N'a4627874-e2ef-42f4-84a2-00adfdc17b40', '2026-10-27 20:02:32.574662');
INSERT INTO [Participants] ([Id], [FullName], [Email], [UserId], [RegisteredAt]) VALUES ('1b87dac4-336d-4fca-a01e-ebe9b08560ed', N'Andrés Mauricio Salazar Ortiz', N'andres.salazar@correo.com', N'a5e86c21-9621-4730-81be-962633d69426', '2026-10-27 20:02:31.525167');
GO
-- 31 rows exported for Participants

-- ============================================================================
-- DATA: Workshops
-- ============================================================================
INSERT INTO [Workshops] ([Id], [Code], [Title], [Description], [InstructorId], [StartDateTime], [EndDateTime], [Capacity]) VALUES ('f4c57f5d-3206-4383-8688-0f20b8b40a54', N'MKT-003', N'Emprendimiento digital', N'Validar y lanzar una idea de negocio en línea', '306a7da4-484c-45c8-bff4-e51cfcfa3c13', '2027-02-27 09:00:00.000000', '2027-02-27 12:00:00.000000', 20);
INSERT INTO [Workshops] ([Id], [Code], [Title], [Description], [InstructorId], [StartDateTime], [EndDateTime], [Capacity]) VALUES ('769f3c89-9d1c-4f37-8726-1029fef7b97f', N'FOT-003', N'Edición en Lightroom', N'Flujo de revelado y ajustes de color', 'eec6d12d-6a94-4071-8e3e-e18a76abe66f', '2027-02-06 14:00:00.000000', '2027-02-06 18:00:00.000000', 12);
INSERT INTO [Workshops] ([Id], [Code], [Title], [Description], [InstructorId], [StartDateTime], [EndDateTime], [Capacity]) VALUES ('ebdbad4c-67fc-42c2-9e97-196782ec3bf2', N'NEW-101', N'Masterclass Fotografia Nocturna', N'Tecnicas avanzadas de larga exposicion', 'eec6d12d-6a94-4071-8e3e-e18a76abe66f', '2026-12-20 18:00:00.000000', '2026-12-20 22:00:00.000000', 10);
INSERT INTO [Workshops] ([Id], [Code], [Title], [Description], [InstructorId], [StartDateTime], [EndDateTime], [Capacity]) VALUES ('3eefa31e-5fad-4e91-bef7-1c5e3572a3fc', N'COC-003', N'Cocina saludable', N'Platos balanceados para la semana', '2a628ce5-ebdb-4ce2-a246-45c5350a9327', '2027-02-13 09:00:00.000000', '2027-02-13 12:00:00.000000', 14);
INSERT INTO [Workshops] ([Id], [Code], [Title], [Description], [InstructorId], [StartDateTime], [EndDateTime], [Capacity]) VALUES ('5388c057-ab4f-4e51-972e-1ecaff90f903', N'ORA-002', N'Presentaciones efectivas', N'Cómo estructurar y presentar una idea en 10 minutos', '18344c5b-bac7-4b12-b2c1-28a77fac942b', '2027-01-23 09:00:00.000000', '2027-01-23 12:00:00.000000', 18);
INSERT INTO [Workshops] ([Id], [Code], [Title], [Description], [InstructorId], [StartDateTime], [EndDateTime], [Capacity]) VALUES ('62bdb7ff-543a-4a6f-b1f4-1ff8f6678393', N'FOT-001', N'Fotografía con celular', N'Composición, luz y edición básica desde el celular', 'eec6d12d-6a94-4071-8e3e-e18a76abe66f', '2026-12-05 08:00:00.000000', '2026-12-05 12:00:00.000000', 15);
INSERT INTO [Workshops] ([Id], [Code], [Title], [Description], [InstructorId], [StartDateTime], [EndDateTime], [Capacity]) VALUES ('c60ef68a-f642-42d4-86f0-310460204acb', N'MUS-002', N'Producción musical en casa', N'Grabación y mezcla con herramientas gratuitas', '70738b5e-1920-482c-a520-7ea77dcc80f0', '2027-02-20 14:00:00.000000', '2027-02-20 18:00:00.000000', 12);
INSERT INTO [Workshops] ([Id], [Code], [Title], [Description], [InstructorId], [StartDateTime], [EndDateTime], [Capacity]) VALUES ('c2e1f486-9407-4154-abec-34588e8f862d', N'MKT-001', N'Marketing en redes sociales', N'Estrategia de contenido para pequeños negocios', '306a7da4-484c-45c8-bff4-e51cfcfa3c13', '2026-12-19 08:00:00.000000', '2026-12-19 12:00:00.000000', 20);
INSERT INTO [Workshops] ([Id], [Code], [Title], [Description], [InstructorId], [StartDateTime], [EndDateTime], [Capacity]) VALUES ('93de42df-3469-47a1-a44b-4df67e09b4a1', N'FOT-002', N'Retrato en exteriores', N'Manejo de luz natural para retratos al aire libre', 'eec6d12d-6a94-4071-8e3e-e18a76abe66f', '2027-01-16 08:00:00.000000', '2027-01-16 12:00:00.000000', 10);
INSERT INTO [Workshops] ([Id], [Code], [Title], [Description], [InstructorId], [StartDateTime], [EndDateTime], [Capacity]) VALUES ('5e2b37c6-9773-4109-b582-7c35f842858f', N'COC-001', N'Cocina costeña', N'Arroz con coco, patacones y mojarra frita', '2a628ce5-ebdb-4ce2-a246-45c5350a9327', '2026-12-05 14:00:00.000000', '2026-12-05 18:00:00.000000', 12);
INSERT INTO [Workshops] ([Id], [Code], [Title], [Description], [InstructorId], [StartDateTime], [EndDateTime], [Capacity]) VALUES ('9fd71e3a-665f-461c-b4cb-915e24434976', N'COC-002', N'Repostería básica', N'Tortas, galletas y postres de nevera', '2a628ce5-ebdb-4ce2-a246-45c5350a9327', '2027-01-09 09:00:00.000000', '2027-01-09 13:00:00.000000', 10);
INSERT INTO [Workshops] ([Id], [Code], [Title], [Description], [InstructorId], [StartDateTime], [EndDateTime], [Capacity]) VALUES ('00c9ecb4-0789-4dd5-a871-940917d9d492', N'ORA-001', N'Oratoria para principiantes', N'Técnicas para hablar en público con seguridad', '18344c5b-bac7-4b12-b2c1-28a77fac942b', '2026-12-12 09:00:00.000000', '2026-12-12 12:00:00.000000', 20);
INSERT INTO [Workshops] ([Id], [Code], [Title], [Description], [InstructorId], [StartDateTime], [EndDateTime], [Capacity]) VALUES ('19173baf-e8c5-4cf9-9b97-a32e818c6d86', N'MUS-001', N'Guitarra desde cero', N'Acordes básicos y primeras canciones', '70738b5e-1920-482c-a520-7ea77dcc80f0', '2026-12-12 14:00:00.000000', '2026-12-12 17:00:00.000000', 10);
INSERT INTO [Workshops] ([Id], [Code], [Title], [Description], [InstructorId], [StartDateTime], [EndDateTime], [Capacity]) VALUES ('712b8af2-80c7-41a7-829d-bbf4eb20bfa6', N'DIS-002', N'Fundamentos de color y tipografía', N'Principios visuales para piezas profesionales', 'cc0f26da-6b90-49d8-a04a-69a55331c47b', '2027-01-30 14:00:00.000000', '2027-01-30 17:00:00.000000', 15);
INSERT INTO [Workshops] ([Id], [Code], [Title], [Description], [InstructorId], [StartDateTime], [EndDateTime], [Capacity]) VALUES ('34ecb87f-821a-4bcd-8f04-e0fa735073fb', N'MKT-002', N'Publicidad en Meta e Instagram', N'Configuración y medición de campañas', '306a7da4-484c-45c8-bff4-e51cfcfa3c13', '2027-01-16 14:00:00.000000', '2027-01-16 18:00:00.000000', 16);
INSERT INTO [Workshops] ([Id], [Code], [Title], [Description], [InstructorId], [StartDateTime], [EndDateTime], [Capacity]) VALUES ('50e0dc40-141a-45f2-bf61-f999352dddfc', N'DIS-001', N'Diseño en Canva', N'Piezas gráficas para redes sociales', 'cc0f26da-6b90-49d8-a04a-69a55331c47b', '2026-12-19 14:00:00.000000', '2026-12-19 17:00:00.000000', 18);
GO
-- 16 rows exported for Workshops

-- ============================================================================
-- DATA: Enrollments
-- ============================================================================
INSERT INTO [Enrollments] ([Id], [ParticipantId], [WorkshopId], [Status], [EnrolledAt], [CancelledAt], [CancelledBy]) VALUES ('141ba5e3-499f-435d-a5ef-00c04459ab02', '0cc05177-8156-442e-8245-a770d2b48e69', '19173baf-e8c5-4cf9-9b97-a32e818c6d86', 1, '2026-10-27 20:02:34.328309', NULL, NULL);
INSERT INTO [Enrollments] ([Id], [ParticipantId], [WorkshopId], [Status], [EnrolledAt], [CancelledAt], [CancelledBy]) VALUES ('d8f3c8da-01fd-494a-8a9b-02dd2a9b56ee', '9fbbf751-8d7d-42b2-a115-b17da0f1a079', 'c2e1f486-9407-4154-abec-34588e8f862d', 1, '2026-10-27 20:02:34.272327', NULL, NULL);
INSERT INTO [Enrollments] ([Id], [ParticipantId], [WorkshopId], [Status], [EnrolledAt], [CancelledAt], [CancelledBy]) VALUES ('da37aa09-03c2-4b9e-b53b-0abdc9eff1eb', '542b4506-4f29-4a58-a2d3-5500e3c5433c', '34ecb87f-821a-4bcd-8f04-e0fa735073fb', 1, '2026-10-27 20:02:34.293472', NULL, NULL);
INSERT INTO [Enrollments] ([Id], [ParticipantId], [WorkshopId], [Status], [EnrolledAt], [CancelledAt], [CancelledBy]) VALUES ('dd320f7b-5fba-42f3-85d2-0c0343a01a11', '486d6688-1e7c-4e6a-b36e-42cc76c1d16d', '712b8af2-80c7-41a7-829d-bbf4eb20bfa6', 1, '2026-10-27 20:02:34.190605', NULL, NULL);
INSERT INTO [Enrollments] ([Id], [ParticipantId], [WorkshopId], [Status], [EnrolledAt], [CancelledAt], [CancelledBy]) VALUES ('55b5f1cb-7398-4995-84c8-0fdc5028e659', '95fbf0f7-a4a5-4ecc-9082-2ea1701ff044', 'c2e1f486-9407-4154-abec-34588e8f862d', 1, '2026-10-27 20:02:34.259731', NULL, NULL);
INSERT INTO [Enrollments] ([Id], [ParticipantId], [WorkshopId], [Status], [EnrolledAt], [CancelledAt], [CancelledBy]) VALUES ('e0bddcd7-e194-40a4-b760-0ffb7ec0aa5f', 'c522ff8a-2aec-49d2-9205-cd8436cb9941', '712b8af2-80c7-41a7-829d-bbf4eb20bfa6', 1, '2026-10-27 20:02:34.200422', NULL, NULL);
INSERT INTO [Enrollments] ([Id], [ParticipantId], [WorkshopId], [Status], [EnrolledAt], [CancelledAt], [CancelledBy]) VALUES ('d16f9cfc-5ec6-4304-9641-1158dd450288', '3dba0cd2-d6cf-4d6b-b09c-756bc85d9b11', 'c2e1f486-9407-4154-abec-34588e8f862d', 1, '2026-10-27 20:02:34.254997', NULL, NULL);
INSERT INTO [Enrollments] ([Id], [ParticipantId], [WorkshopId], [Status], [EnrolledAt], [CancelledAt], [CancelledBy]) VALUES ('22c2d6dc-e679-447a-b2c8-1b2d1bd93076', 'fc39b88b-56fa-4b2f-83c0-cf9b68c5c984', '9fd71e3a-665f-461c-b4cb-915e24434976', 1, '2026-10-27 20:02:34.138558', NULL, NULL);
INSERT INTO [Enrollments] ([Id], [ParticipantId], [WorkshopId], [Status], [EnrolledAt], [CancelledAt], [CancelledBy]) VALUES ('0e7acd19-10c8-441a-8f6c-1da791e638ff', 'a402e95b-6e79-43c9-be21-ab193f66ac92', '5388c057-ab4f-4e51-972e-1ecaff90f903', 1, '2026-10-27 20:02:34.415661', NULL, NULL);
INSERT INTO [Enrollments] ([Id], [ParticipantId], [WorkshopId], [Status], [EnrolledAt], [CancelledAt], [CancelledBy]) VALUES ('e4e77b0c-d05d-47b1-a65b-1dafa0103ee9', '542b4506-4f29-4a58-a2d3-5500e3c5433c', 'ebdbad4c-67fc-42c2-9e97-196782ec3bf2', 2, '2026-10-27 20:24:50.789052', '2026-10-27 20:27:46.815511', N'jorge.perez@correo.com');
INSERT INTO [Enrollments] ([Id], [ParticipantId], [WorkshopId], [Status], [EnrolledAt], [CancelledAt], [CancelledBy]) VALUES ('ce1de633-2c12-4fcf-8480-1ee59cd013bb', '9fbbf751-8d7d-42b2-a115-b17da0f1a079', '19173baf-e8c5-4cf9-9b97-a32e818c6d86', 1, '2026-10-27 20:02:34.352487', NULL, NULL);
INSERT INTO [Enrollments] ([Id], [ParticipantId], [WorkshopId], [Status], [EnrolledAt], [CancelledAt], [CancelledBy]) VALUES ('09c74069-d886-48de-ba63-20a5d505e71c', '6cefa4c3-fcf2-4bda-9a53-25c15b33c5e1', '34ecb87f-821a-4bcd-8f04-e0fa735073fb', 1, '2026-10-27 20:02:34.297787', NULL, NULL);
INSERT INTO [Enrollments] ([Id], [ParticipantId], [WorkshopId], [Status], [EnrolledAt], [CancelledAt], [CancelledBy]) VALUES ('f06952d7-b49a-4730-94e9-22b67e7780e6', 'c9ad9dab-d844-44b1-aa8c-c4c6b008c02a', '19173baf-e8c5-4cf9-9b97-a32e818c6d86', 1, '2026-10-27 20:02:34.332582', NULL, NULL);
INSERT INTO [Enrollments] ([Id], [ParticipantId], [WorkshopId], [Status], [EnrolledAt], [CancelledAt], [CancelledBy]) VALUES ('e09a8467-a6f2-4325-96bc-2b2784aa8cd7', '3dba0cd2-d6cf-4d6b-b09c-756bc85d9b11', '5388c057-ab4f-4e51-972e-1ecaff90f903', 1, '2026-10-27 20:02:34.387836', NULL, NULL);
INSERT INTO [Enrollments] ([Id], [ParticipantId], [WorkshopId], [Status], [EnrolledAt], [CancelledAt], [CancelledBy]) VALUES ('a6cc5bdf-83a0-4f51-ab25-2fa94c31976b', '14e52f94-fd24-4967-b2e4-6fba45c45035', '34ecb87f-821a-4bcd-8f04-e0fa735073fb', 1, '2026-10-27 20:02:34.289636', NULL, NULL);
INSERT INTO [Enrollments] ([Id], [ParticipantId], [WorkshopId], [Status], [EnrolledAt], [CancelledAt], [CancelledBy]) VALUES ('6c95b184-2362-4701-aeaa-307a3feb5136', 'c69ffb99-f9ba-4673-8aa3-632a8ba24d32', '9fd71e3a-665f-461c-b4cb-915e24434976', 1, '2026-10-27 20:02:34.130214', NULL, NULL);
INSERT INTO [Enrollments] ([Id], [ParticipantId], [WorkshopId], [Status], [EnrolledAt], [CancelledAt], [CancelledBy]) VALUES ('270819dc-69e4-4fe7-9399-35043f7cc9ee', 'bfcadc54-4c9f-4d4d-adff-ccaaabb269b2', '3eefa31e-5fad-4e91-bef7-1c5e3572a3fc', 1, '2026-10-27 20:02:34.152091', NULL, NULL);
INSERT INTO [Enrollments] ([Id], [ParticipantId], [WorkshopId], [Status], [EnrolledAt], [CancelledAt], [CancelledBy]) VALUES ('11bb7b56-1cbc-4bc2-ab46-3942476e09fc', '542b4506-4f29-4a58-a2d3-5500e3c5433c', 'f4c57f5d-3206-4383-8688-0f20b8b40a54', 1, '2026-10-27 20:02:34.315446', NULL, NULL);
INSERT INTO [Enrollments] ([Id], [ParticipantId], [WorkshopId], [Status], [EnrolledAt], [CancelledAt], [CancelledBy]) VALUES ('588834d8-76d8-435b-81b8-39a05302cc9c', '6cefa4c3-fcf2-4bda-9a53-25c15b33c5e1', '712b8af2-80c7-41a7-829d-bbf4eb20bfa6', 1, '2026-10-27 20:02:34.204404', NULL, NULL);
INSERT INTO [Enrollments] ([Id], [ParticipantId], [WorkshopId], [Status], [EnrolledAt], [CancelledAt], [CancelledBy]) VALUES ('d85fe508-0c3f-47fb-99f2-3a4774f76885', 'fc39b88b-56fa-4b2f-83c0-cf9b68c5c984', '19173baf-e8c5-4cf9-9b97-a32e818c6d86', 1, '2026-10-27 20:02:34.336503', NULL, NULL);
INSERT INTO [Enrollments] ([Id], [ParticipantId], [WorkshopId], [Status], [EnrolledAt], [CancelledAt], [CancelledBy]) VALUES ('95f1db8c-4f4d-4f90-8f4e-407cffe8974a', 'beee8187-3711-4a29-bf44-0de98e7d8ba1', 'c2e1f486-9407-4154-abec-34588e8f862d', 1, '2026-10-27 20:02:34.276783', NULL, NULL);
INSERT INTO [Enrollments] ([Id], [ParticipantId], [WorkshopId], [Status], [EnrolledAt], [CancelledAt], [CancelledBy]) VALUES ('9db3b1c3-1da3-4da6-bb22-4204137461ae', '0122e1ae-9a65-4b77-b420-925576b0566c', '34ecb87f-821a-4bcd-8f04-e0fa735073fb', 1, '2026-10-27 20:02:34.306657', NULL, NULL);
INSERT INTO [Enrollments] ([Id], [ParticipantId], [WorkshopId], [Status], [EnrolledAt], [CancelledAt], [CancelledBy]) VALUES ('8fdee1ee-ec51-49bf-bc83-46aca2085956', '6e8f9d41-12f6-4f97-90b2-080084dec910', '769f3c89-9d1c-4f37-8726-1029fef7b97f', 1, '2026-10-27 20:02:34.250539', NULL, NULL);
INSERT INTO [Enrollments] ([Id], [ParticipantId], [WorkshopId], [Status], [EnrolledAt], [CancelledAt], [CancelledBy]) VALUES ('829634af-749a-48f1-bcd4-48d35f31ff00', '53a6da0b-0f7c-4d52-af15-229b8d395d30', '19173baf-e8c5-4cf9-9b97-a32e818c6d86', 1, '2026-10-27 20:02:34.341818', NULL, NULL);
INSERT INTO [Enrollments] ([Id], [ParticipantId], [WorkshopId], [Status], [EnrolledAt], [CancelledAt], [CancelledBy]) VALUES ('ffe1bddd-a448-4b43-ad7b-49c075455cc5', '542b4506-4f29-4a58-a2d3-5500e3c5433c', 'ebdbad4c-67fc-42c2-9e97-196782ec3bf2', 1, '2026-10-27 20:27:55.759575', NULL, NULL);
INSERT INTO [Enrollments] ([Id], [ParticipantId], [WorkshopId], [Status], [EnrolledAt], [CancelledAt], [CancelledBy]) VALUES ('8f92c888-707c-40f0-96ea-55821235f06e', '53a6da0b-0f7c-4d52-af15-229b8d395d30', 'f4c57f5d-3206-4383-8688-0f20b8b40a54', 1, '2026-10-27 20:02:34.319566', NULL, NULL);
INSERT INTO [Enrollments] ([Id], [ParticipantId], [WorkshopId], [Status], [EnrolledAt], [CancelledAt], [CancelledBy]) VALUES ('6460c31f-cc4b-4c81-add4-5887a05a9ee7', 'c69ffb99-f9ba-4673-8aa3-632a8ba24d32', '769f3c89-9d1c-4f37-8726-1029fef7b97f', 1, '2026-10-27 20:02:34.236963', NULL, NULL);
INSERT INTO [Enrollments] ([Id], [ParticipantId], [WorkshopId], [Status], [EnrolledAt], [CancelledAt], [CancelledBy]) VALUES ('e75a589a-5dd9-45c7-9a3a-594b339b5bba', '0122e1ae-9a65-4b77-b420-925576b0566c', 'c60ef68a-f642-42d4-86f0-310460204acb', 1, '2026-10-27 20:02:34.369459', NULL, NULL);
INSERT INTO [Enrollments] ([Id], [ParticipantId], [WorkshopId], [Status], [EnrolledAt], [CancelledAt], [CancelledBy]) VALUES ('9ee979ed-bcb8-4960-a785-5d1d08b83b91', '53a6da0b-0f7c-4d52-af15-229b8d395d30', '3eefa31e-5fad-4e91-bef7-1c5e3572a3fc', 1, '2026-10-27 20:02:34.157577', NULL, NULL);
INSERT INTO [Enrollments] ([Id], [ParticipantId], [WorkshopId], [Status], [EnrolledAt], [CancelledAt], [CancelledBy]) VALUES ('c0135c5f-f4a5-4e4f-8073-5da2c4fb5df6', '399dec69-e7fd-41c9-a472-90af8a15cae2', '9fd71e3a-665f-461c-b4cb-915e24434976', 1, '2026-10-27 20:02:34.126340', NULL, NULL);
INSERT INTO [Enrollments] ([Id], [ParticipantId], [WorkshopId], [Status], [EnrolledAt], [CancelledAt], [CancelledBy]) VALUES ('2e555843-687e-452a-952b-5e99321fcd2c', 'cc346d1c-a32f-4de8-a7e7-e0afc07912a7', '5388c057-ab4f-4e51-972e-1ecaff90f903', 1, '2026-10-27 20:02:34.401134', NULL, NULL);
INSERT INTO [Enrollments] ([Id], [ParticipantId], [WorkshopId], [Status], [EnrolledAt], [CancelledAt], [CancelledBy]) VALUES ('69147c2c-2e74-4329-a020-5f30fe1f501a', 'beee8187-3711-4a29-bf44-0de98e7d8ba1', '34ecb87f-821a-4bcd-8f04-e0fa735073fb', 1, '2026-10-27 20:02:34.302338', NULL, NULL);
INSERT INTO [Enrollments] ([Id], [ParticipantId], [WorkshopId], [Status], [EnrolledAt], [CancelledAt], [CancelledBy]) VALUES ('1dcc06f8-d3ef-440b-8a35-646a84366213', '288c0ebc-3e50-4a22-8435-296eeb18ab0f', '5388c057-ab4f-4e51-972e-1ecaff90f903', 1, '2026-10-27 20:02:34.411012', NULL, NULL);
INSERT INTO [Enrollments] ([Id], [ParticipantId], [WorkshopId], [Status], [EnrolledAt], [CancelledAt], [CancelledBy]) VALUES ('27a7fcbb-e971-4ab9-aa5f-664edb5fa9f5', '3dba0cd2-d6cf-4d6b-b09c-756bc85d9b11', '5e2b37c6-9773-4109-b582-7c35f842858f', 1, '2026-10-27 20:02:34.070336', NULL, NULL);
INSERT INTO [Enrollments] ([Id], [ParticipantId], [WorkshopId], [Status], [EnrolledAt], [CancelledAt], [CancelledBy]) VALUES ('0a912880-a7fa-4f34-ac09-6a34e3f6e560', '399dec69-e7fd-41c9-a472-90af8a15cae2', 'c2e1f486-9407-4154-abec-34588e8f862d', 1, '2026-10-27 20:02:34.268277', NULL, NULL);
INSERT INTO [Enrollments] ([Id], [ParticipantId], [WorkshopId], [Status], [EnrolledAt], [CancelledAt], [CancelledBy]) VALUES ('e0d68089-0d8d-43f6-82a5-6b94f2c8aa58', '6cefa4c3-fcf2-4bda-9a53-25c15b33c5e1', '769f3c89-9d1c-4f37-8726-1029fef7b97f', 1, '2026-10-27 20:02:34.241257', NULL, NULL);
INSERT INTO [Enrollments] ([Id], [ParticipantId], [WorkshopId], [Status], [EnrolledAt], [CancelledAt], [CancelledBy]) VALUES ('7b356f74-54b1-4603-adbe-6f8a99c24347', '6e8f9d41-12f6-4f97-90b2-080084dec910', '3eefa31e-5fad-4e91-bef7-1c5e3572a3fc', 1, '2026-10-27 20:02:34.164591', NULL, NULL);
INSERT INTO [Enrollments] ([Id], [ParticipantId], [WorkshopId], [Status], [EnrolledAt], [CancelledAt], [CancelledBy]) VALUES ('b55cef8d-1ab1-46b0-87e3-6fc3cf48aac9', 'c2d18343-6865-4c39-9806-a1e0e34ddec0', '50e0dc40-141a-45f2-bf61-f999352dddfc', 1, '2026-10-27 20:02:34.168601', NULL, NULL);
INSERT INTO [Enrollments] ([Id], [ParticipantId], [WorkshopId], [Status], [EnrolledAt], [CancelledAt], [CancelledBy]) VALUES ('f4e92295-7eac-4a5c-8be7-76c82893caba', '9068b0b8-2308-40a8-b89c-e281b716acdd', '00c9ecb4-0789-4dd5-a871-940917d9d492', 1, '2026-10-27 20:02:34.374152', NULL, NULL);
INSERT INTO [Enrollments] ([Id], [ParticipantId], [WorkshopId], [Status], [EnrolledAt], [CancelledAt], [CancelledBy]) VALUES ('23538f5c-a3d1-4d6f-ba06-7700cb4da199', '95fbf0f7-a4a5-4ecc-9082-2ea1701ff044', '9fd71e3a-665f-461c-b4cb-915e24434976', 1, '2026-10-27 20:02:34.121637', NULL, NULL);
INSERT INTO [Enrollments] ([Id], [ParticipantId], [WorkshopId], [Status], [EnrolledAt], [CancelledAt], [CancelledBy]) VALUES ('14bad2fd-0a39-47cc-92d8-782a1da214ca', '0a9d3a7d-5df0-4403-a877-6e5de16fd73c', '50e0dc40-141a-45f2-bf61-f999352dddfc', 1, '2026-10-27 20:02:34.177647', NULL, NULL);
INSERT INTO [Enrollments] ([Id], [ParticipantId], [WorkshopId], [Status], [EnrolledAt], [CancelledAt], [CancelledBy]) VALUES ('abac3833-5c21-4db5-9a83-78b5d78afa91', '0a9d3a7d-5df0-4403-a877-6e5de16fd73c', '00c9ecb4-0789-4dd5-a871-940917d9d492', 1, '2026-10-27 20:02:34.383725', NULL, NULL);
INSERT INTO [Enrollments] ([Id], [ParticipantId], [WorkshopId], [Status], [EnrolledAt], [CancelledAt], [CancelledBy]) VALUES ('ab1bd439-a708-4e4e-99eb-7f23714641ec', 'c2d18343-6865-4c39-9806-a1e0e34ddec0', '62bdb7ff-543a-4a6f-b1f4-1ff8f6678393', 1, '2026-10-27 20:02:34.210774', NULL, NULL);
INSERT INTO [Enrollments] ([Id], [ParticipantId], [WorkshopId], [Status], [EnrolledAt], [CancelledAt], [CancelledBy]) VALUES ('6adcd9d8-faf7-42e5-a58f-7fa0720ad545', 'bfcadc54-4c9f-4d4d-adff-ccaaabb269b2', '5388c057-ab4f-4e51-972e-1ecaff90f903', 1, '2026-10-27 20:02:34.392394', NULL, NULL);
INSERT INTO [Enrollments] ([Id], [ParticipantId], [WorkshopId], [Status], [EnrolledAt], [CancelledAt], [CancelledBy]) VALUES ('091a6c24-c07d-4c03-ba74-8117c9927ddc', 'a402e95b-6e79-43c9-be21-ab193f66ac92', 'c60ef68a-f642-42d4-86f0-310460204acb', 1, '2026-10-27 20:02:34.365056', NULL, NULL);
INSERT INTO [Enrollments] ([Id], [ParticipantId], [WorkshopId], [Status], [EnrolledAt], [CancelledAt], [CancelledBy]) VALUES ('ea305e98-cb0f-46e7-9a42-8222ad6427b5', 'beee8187-3711-4a29-bf44-0de98e7d8ba1', '3eefa31e-5fad-4e91-bef7-1c5e3572a3fc', 1, '2026-10-27 20:02:34.161216', NULL, NULL);
INSERT INTO [Enrollments] ([Id], [ParticipantId], [WorkshopId], [Status], [EnrolledAt], [CancelledAt], [CancelledBy]) VALUES ('3886ba94-6b38-47fd-9c2a-84e833e30b7c', '0a9d3a7d-5df0-4403-a877-6e5de16fd73c', '19173baf-e8c5-4cf9-9b97-a32e818c6d86', 1, '2026-10-27 20:02:34.357487', NULL, NULL);
INSERT INTO [Enrollments] ([Id], [ParticipantId], [WorkshopId], [Status], [EnrolledAt], [CancelledAt], [CancelledBy]) VALUES ('11bf3647-f8f1-431c-8e43-8afa6df4115b', 'c522ff8a-2aec-49d2-9205-cd8436cb9941', '00c9ecb4-0789-4dd5-a871-940917d9d492', 1, '2026-10-27 20:02:34.379303', NULL, NULL);
INSERT INTO [Enrollments] ([Id], [ParticipantId], [WorkshopId], [Status], [EnrolledAt], [CancelledAt], [CancelledBy]) VALUES ('6202d8d0-657a-4e7d-9bc7-8c505bbbc421', 'cc346d1c-a32f-4de8-a7e7-e0afc07912a7', '5e2b37c6-9773-4109-b582-7c35f842858f', 1, '2026-10-27 20:02:34.109727', NULL, NULL);
INSERT INTO [Enrollments] ([Id], [ParticipantId], [WorkshopId], [Status], [EnrolledAt], [CancelledAt], [CancelledBy]) VALUES ('084b52ce-9821-4b07-96bc-924f5aeffcbd', '9fbbf751-8d7d-42b2-a115-b17da0f1a079', '93de42df-3469-47a1-a44b-4df67e09b4a1', 1, '2026-10-27 20:02:34.232462', NULL, NULL);
INSERT INTO [Enrollments] ([Id], [ParticipantId], [WorkshopId], [Status], [EnrolledAt], [CancelledAt], [CancelledBy]) VALUES ('32f48ce7-2c98-4ab2-b8c3-958cb9e7b0c6', '1b87dac4-336d-4fca-a01e-ebe9b08560ed', '34ecb87f-821a-4bcd-8f04-e0fa735073fb', 1, '2026-10-27 20:02:34.285664', NULL, NULL);
INSERT INTO [Enrollments] ([Id], [ParticipantId], [WorkshopId], [Status], [EnrolledAt], [CancelledAt], [CancelledBy]) VALUES ('4a75a5f8-145d-4722-9120-95d945ad17f7', '486d6688-1e7c-4e6a-b36e-42cc76c1d16d', '3eefa31e-5fad-4e91-bef7-1c5e3572a3fc', 1, '2026-10-27 20:02:34.147936', NULL, NULL);
INSERT INTO [Enrollments] ([Id], [ParticipantId], [WorkshopId], [Status], [EnrolledAt], [CancelledAt], [CancelledBy]) VALUES ('db3b82ef-00f9-4390-85bd-9a465650d7cb', '0a9d3a7d-5df0-4403-a877-6e5de16fd73c', '5e2b37c6-9773-4109-b582-7c35f842858f', 1, '2026-10-27 20:02:34.117835', NULL, NULL);
INSERT INTO [Enrollments] ([Id], [ParticipantId], [WorkshopId], [Status], [EnrolledAt], [CancelledAt], [CancelledBy]) VALUES ('0d05ce2b-bf87-435c-8667-9f64211893f5', '486d6688-1e7c-4e6a-b36e-42cc76c1d16d', 'c2e1f486-9407-4154-abec-34588e8f862d', 1, '2026-10-27 20:02:34.264057', NULL, NULL);
INSERT INTO [Enrollments] ([Id], [ParticipantId], [WorkshopId], [Status], [EnrolledAt], [CancelledAt], [CancelledBy]) VALUES ('addf890c-8819-4e10-bcfe-9fbf9b051ecb', 'c01e07b3-1789-41fe-8e16-2e14b4b36925', '93de42df-3469-47a1-a44b-4df67e09b4a1', 1, '2026-10-27 20:02:34.223871', NULL, NULL);
INSERT INTO [Enrollments] ([Id], [ParticipantId], [WorkshopId], [Status], [EnrolledAt], [CancelledAt], [CancelledBy]) VALUES ('43cd51d7-5472-4139-83d0-a08619504932', '3dba0cd2-d6cf-4d6b-b09c-756bc85d9b11', '93de42df-3469-47a1-a44b-4df67e09b4a1', 1, '2026-10-27 20:02:34.228413', NULL, NULL);
INSERT INTO [Enrollments] ([Id], [ParticipantId], [WorkshopId], [Status], [EnrolledAt], [CancelledAt], [CancelledBy]) VALUES ('b548f60d-8531-4ccb-a59f-a4be0e3048b2', '0cc05177-8156-442e-8245-a770d2b48e69', 'f4c57f5d-3206-4383-8688-0f20b8b40a54', 1, '2026-10-27 20:02:34.310965', NULL, NULL);
INSERT INTO [Enrollments] ([Id], [ParticipantId], [WorkshopId], [Status], [EnrolledAt], [CancelledAt], [CancelledBy]) VALUES ('7af7a089-478c-4666-8f04-a7a831049d7b', 'a402e95b-6e79-43c9-be21-ab193f66ac92', '769f3c89-9d1c-4f37-8726-1029fef7b97f', 1, '2026-10-27 20:02:34.246629', NULL, NULL);
INSERT INTO [Enrollments] ([Id], [ParticipantId], [WorkshopId], [Status], [EnrolledAt], [CancelledAt], [CancelledBy]) VALUES ('a43ceac8-c1ea-4ea0-871f-a8c2d6eed384', 'fc39b88b-56fa-4b2f-83c0-cf9b68c5c984', '5388c057-ab4f-4e51-972e-1ecaff90f903', 1, '2026-10-27 20:02:34.396777', NULL, NULL);
INSERT INTO [Enrollments] ([Id], [ParticipantId], [WorkshopId], [Status], [EnrolledAt], [CancelledAt], [CancelledBy]) VALUES ('9e0cfeb8-d32d-4603-82a9-b3135c0163ca', '542b4506-4f29-4a58-a2d3-5500e3c5433c', '5e2b37c6-9773-4109-b582-7c35f842858f', 1, '2026-10-27 20:02:34.104725', NULL, NULL);
INSERT INTO [Enrollments] ([Id], [ParticipantId], [WorkshopId], [Status], [EnrolledAt], [CancelledAt], [CancelledBy]) VALUES ('40f5d5ca-2dff-49cf-89a1-ba0082923296', '7c0efc21-19f9-46d0-b48d-6f1e40edda2d', '62bdb7ff-543a-4a6f-b1f4-1ff8f6678393', 1, '2026-10-27 20:02:34.219653', NULL, NULL);
INSERT INTO [Enrollments] ([Id], [ParticipantId], [WorkshopId], [Status], [EnrolledAt], [CancelledAt], [CancelledBy]) VALUES ('4b5f78a7-4cf6-4c26-ad1f-c8cfa758ac1c', '0cc05177-8156-442e-8245-a770d2b48e69', '34ecb87f-821a-4bcd-8f04-e0fa735073fb', 1, '2026-10-27 20:02:34.281006', NULL, NULL);
INSERT INTO [Enrollments] ([Id], [ParticipantId], [WorkshopId], [Status], [EnrolledAt], [CancelledAt], [CancelledBy]) VALUES ('9e22d005-ed3f-46f8-b63c-d1ab604b2c7a', 'bfcadc54-4c9f-4d4d-adff-ccaaabb269b2', '50e0dc40-141a-45f2-bf61-f999352dddfc', 1, '2026-10-27 20:02:34.172924', NULL, NULL);
INSERT INTO [Enrollments] ([Id], [ParticipantId], [WorkshopId], [Status], [EnrolledAt], [CancelledAt], [CancelledBy]) VALUES ('bcb28c07-ffc5-4075-a3f8-d493c83e8529', '7f9a54cd-46de-40cd-9f69-39140d9b671c', '5388c057-ab4f-4e51-972e-1ecaff90f903', 1, '2026-10-27 20:02:34.406266', NULL, NULL);
INSERT INTO [Enrollments] ([Id], [ParticipantId], [WorkshopId], [Status], [EnrolledAt], [CancelledAt], [CancelledBy]) VALUES ('9caed474-9f62-42b9-ad0a-d909c32d1fd6', 'a402e95b-6e79-43c9-be21-ab193f66ac92', '50e0dc40-141a-45f2-bf61-f999352dddfc', 1, '2026-10-27 20:02:34.181435', NULL, NULL);
INSERT INTO [Enrollments] ([Id], [ParticipantId], [WorkshopId], [Status], [EnrolledAt], [CancelledAt], [CancelledBy]) VALUES ('646b6d38-63fe-4a24-b6f1-e65944a74506', 'c522ff8a-2aec-49d2-9205-cd8436cb9941', '62bdb7ff-543a-4a6f-b1f4-1ff8f6678393', 1, '2026-10-27 20:02:34.215561', NULL, NULL);
INSERT INTO [Enrollments] ([Id], [ParticipantId], [WorkshopId], [Status], [EnrolledAt], [CancelledAt], [CancelledBy]) VALUES ('72b42071-8260-4365-a8dd-e7226e166894', 'c522ff8a-2aec-49d2-9205-cd8436cb9941', 'f4c57f5d-3206-4383-8688-0f20b8b40a54', 1, '2026-10-27 20:02:34.323757', NULL, NULL);
INSERT INTO [Enrollments] ([Id], [ParticipantId], [WorkshopId], [Status], [EnrolledAt], [CancelledAt], [CancelledBy]) VALUES ('2c014df6-db99-4cb5-8464-eb6a23fba494', 'bfcadc54-4c9f-4d4d-adff-ccaaabb269b2', '9fd71e3a-665f-461c-b4cb-915e24434976', 1, '2026-10-27 20:02:34.134034', NULL, NULL);
INSERT INTO [Enrollments] ([Id], [ParticipantId], [WorkshopId], [Status], [EnrolledAt], [CancelledAt], [CancelledBy]) VALUES ('993f3e57-1534-437b-a49e-ef19adac8019', '9068b0b8-2308-40a8-b89c-e281b716acdd', '712b8af2-80c7-41a7-829d-bbf4eb20bfa6', 1, '2026-10-27 20:02:34.185770', NULL, NULL);
INSERT INTO [Enrollments] ([Id], [ParticipantId], [WorkshopId], [Status], [EnrolledAt], [CancelledAt], [CancelledBy]) VALUES ('ccce08e3-24e7-4e24-b45f-f190b09faa5b', '1b87dac4-336d-4fca-a01e-ebe9b08560ed', '3eefa31e-5fad-4e91-bef7-1c5e3572a3fc', 1, '2026-10-27 20:02:34.143765', NULL, NULL);
INSERT INTO [Enrollments] ([Id], [ParticipantId], [WorkshopId], [Status], [EnrolledAt], [CancelledAt], [CancelledBy]) VALUES ('57070e1f-868f-4146-b359-f2e6bc5fdacd', '53a6da0b-0f7c-4d52-af15-229b8d395d30', '712b8af2-80c7-41a7-829d-bbf4eb20bfa6', 1, '2026-10-27 20:02:34.195895', NULL, NULL);
INSERT INTO [Enrollments] ([Id], [ParticipantId], [WorkshopId], [Status], [EnrolledAt], [CancelledAt], [CancelledBy]) VALUES ('eeb65bf1-edc1-4ea0-b721-fb1ed6cb40c1', '288c0ebc-3e50-4a22-8435-296eeb18ab0f', '5e2b37c6-9773-4109-b582-7c35f842858f', 1, '2026-10-27 20:02:34.113843', NULL, NULL);
GO
-- 72 rows exported for Enrollments

-- ============================================================================
-- END OF SCRIPT
-- ============================================================================
