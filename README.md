# Inicio IPTV Player (Windows MVP)

Aplicación de escritorio **Windows-first** para listas IPTV grandes, con reproducción embebida por LibVLCSharp, EPG en SQLite y UI WPF ligera.

## Stack

- .NET 8
- C#
- WPF (MVVM)
- LibVLCSharp + LibVLC nativo para Windows
- SQLite

## Qué hace esta versión

- Importa listas `M3U/M3U8`
- Importa guía `XMLTV/XMLTV.GZ`
- Guarda canales y EPG en SQLite
- Muestra canales por grupo
- Búsqueda rápida por nombre
- Reproduce **solo al seleccionar canal**
- Muestra EPG actual/siguiente
- Permite cambiar pista de audio
- UI virtualizada para listas muy grandes

## Publicar versión ejecutable Windows (doble clic)

> Estas instrucciones son para un usuario no programador en **Windows 10/11 x64**.

### Paso 1: instalar .NET 8 SDK (una sola vez)

1. Abre este enlace: `https://dotnet.microsoft.com/download/dotnet/8.0`
2. Descarga **.NET SDK 8 (x64)** para Windows.
3. Instálalo con siguiente > siguiente > finalizar.

### Paso 2: generar la carpeta lista para usar

1. Abre esta carpeta del proyecto.
2. Entra en `scripts`.
3. Haz doble clic en: `publish-windows.cmd`.
4. Espera a que termine (puede tardar varios minutos la primera vez).

Al final tendrás esta carpeta:

- `publish\win-x64\`

Dentro estará todo lo necesario para ejecutar:

- `Inicio.IptvPlayer.Wpf.exe`
- DLLs de la app
- librerías nativas de LibVLC (`libvlc` y plugins)
- lanzador `INICIAR_IPTV_PLAYER.cmd`

### Paso 3: ejecutar la app

En `publish\win-x64\`:

- doble clic en `INICIAR_IPTV_PLAYER.cmd`
  o
- doble clic en `Inicio.IptvPlayer.Wpf.exe`

## Uso básico

1. Botón **Importar M3U** → selecciona tu lista.
2. Botón **Importar XMLTV** → selecciona tu guía.
3. Filtra por grupo o usa búsqueda.
4. Haz clic en un canal para reproducir.
5. Si hay varias pistas de audio, cámbiala desde el combo de audio.

## Rendimiento

- No se abre ningún stream al iniciar.
- Metadatos en SQLite con índices.
- EPG parseada e insertada de forma incremental.
- Lista de canales virtualizada + paginada.
