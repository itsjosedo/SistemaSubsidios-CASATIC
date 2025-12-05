
# Sistema de subsidios - .Net/MySQL

# Descripción del Proyecto

El Sistema de Gestión de Subsidios es una aplicación web desarrollada con ASP.NET Core MVC que permite administrar usuarios, beneficiarios, solicitudes, subsidios y otros módulos relacionados.
El sistema incluye autenticación, autorización, gestión de datos mediante Entity Framework Core y conexión con una base de datos MySQL/MariaDB.

#Tecnologias utilizadas
#SDK
Microsoft.NET.Sdk.Web
#Frameworks
ASP.NET Core 8.0
C#
Bootstrap 5
Visual Studio 2022 / VS Code

#Base de Datos
MySQL 8.0.33
Entity Framework Core 9.0.9
Proveedor MySQL: Pomelo.EntityFrameworkCore.MySql 9.0.0

#Autenticación
Cookie Authentication (ASP.NET Core Identity NO está presente, es un sistema personalizado)

#Servicios Internos
EmailService (envío de correos)
OtpService (generación y validación de códigos OTP)

#Paquetes NuGet
MailKit 4.14.1 (SMTP)
MimeKit 4.14.0 (mensajes MIME)
Microsoft.EntityFrameworkCore 9.0.9
Microsoft.EntityFrameworkCore.Design 9.0.9
Pomelo.EntityFrameworkCore.MySql 9.0.0

#Frontend
ASP.NET Core MVC (Views + Controllers)
Razor Pages

#JSON / API
System.Text.Json con:
CamelCase
Ignore Nulls
Ignore Cycles

#Instalar los nugets necesarios
dotnet add package MailKit --version 4.14.1
dotnet add package Microsoft.EntityFrameworkCore --version 9.0.9
dotnet add package Microsoft.EntityFrameworkCore.Design --version 9.0.9
dotnet add package MimeKit --version 4.14.0
dotnet add package Pomelo.EntityFrameworkCore.MySql --version 9.0.0

#Aplicar migracion si es primera ejecucación
dotnet ef database update

#Estructura del proyecto
/Controllers     -> Controladores MVC
/Models          -> Entidades de base de datos
/Views           -> Vistas (Razor)
/Data            -> DbContext y migraciones
/wwwroot         -> Archivos estáticos (CSS, JS, imágenes)
/Services        -> Servicios personalizados (email, validación, etc.)

#Autores
José Douglas Calles Gómez
Reynaldo David Henriquez Cornejo
José Ismael Ventura Martinez
Linda Marcela Guillen Abarca