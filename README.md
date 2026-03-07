# Inicio IPTV Player (Windows MVP)

Aplicación de escritorio **Windows-first** para reproducir listas IPTV grandes con interfaz ligera, rápida y mantenible.

## Stack

- .NET 8
- C#
- WPF (MVVM)
- LibVLCSharp (reproductor embebido)
- SQLite

## Objetivos cubiertos en este MVP

- Importación de listas `M3U/M3U8`.
- Importación de guía `XMLTV/XMLTV.GZ`.
- Persistencia de metadatos en SQLite (canales, grupos, EPG).
- Carga diferida: **no se abre ningún stream al arrancar**.
- Reproducción bajo demanda al seleccionar canal.
- Consulta de EPG `actual/siguiente` por canal.
- Cambio de pista de audio (cuando existe).
- UI WPF limpia con virtualización para listas grandes.

## Estructura

```text
src/
  Inicio.IptvPlayer.Wpf/
    Domain/          # Modelos de dominio
    Data/            # SQLite + repositorios
    Services/        # Importadores M3U/XMLTV + player LibVLC
    ViewModels/      # MVVM
    Views/           # Vistas WPF
```

## Notas de rendimiento

- Se parsea M3U en streaming (`StreamReader`) y se inserta por lotes.
- Se parsea XMLTV con `XmlReader` + inserción incremental para evitar retener EPG completa en RAM.
- La UI usa virtualización (`VirtualizingStackPanel`, recycling).
- Los streams se reproducen solo al seleccionar canal.

## Requisitos de ejecución (Windows)

1. .NET 8 SDK/runtime.
2. LibVLC instalado o librerías VLC accesibles para LibVLCSharp.
3. Compilar y ejecutar:

```powershell
dotnet build Inicio.sln
dotnet run --project .\src\Inicio.IptvPlayer.Wpf\Inicio.IptvPlayer.Wpf.csproj
```

