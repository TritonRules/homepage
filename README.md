# 🚀 homepage

Página de inicio personal publicada en **GitHub Pages**, usada como homepage en todos los navegadores.

🌐 **URL activa:** [`https://tritonrules.github.io/homepage/src/homepage/index.html`](https://tritonrules.github.io/homepage/src/homepage/index.html)

---

## ¿Qué es esto?

Una página HTML autocontenida con:
- Links de acceso rápido organizados por categoría (Día, Redes, Drive, Bancos, Utilidades)
- Buscador integrado (Google, Maps, ChatGPT)
- Conversor de monedas en tiempo real
- Calendario mensual
- Pomodoro timer
- Relojes mundiales
- Widget de clima (Open-Meteo, sin API key)
- Fondo aleatorio (Picsum Photos)

---

## 📁 Estructura

```
homepage/
├── README.md                  ← este archivo
├── .nojekyll                  ← desactiva Jekyll en GitHub Pages
└── src/
    └── homepage/
        └── index.html         ← la página, edítala aquí
```

---

## ✏️ Cómo editar la homepage

1. Ve a [`src/homepage/index.html`](./src/homepage/index.html)
2. Haz clic en el icono ✏️ (Edit)
3. Edita lo que necesites
4. Haz commit directamente en `main`
5. GitHub Pages despliega automáticamente en ~30 segundos

---

## 🔗 Assets y dependencias

Todo es **externo** — no hay assets locales que romper:

| Recurso | URL |
|---|---|
| Fuente Montserrat | `fonts.googleapis.com` |
| Iconos Font Awesome 6.4 | `cdnjs.cloudflare.com` |
| API Tipo de cambio | `api.exchangerate-api.com` (sin key) |
| API Clima | `api.open-meteo.com` (sin key) |
| Fondos aleatorios | `picsum.photos` |
| Audio Pomodoro | `actions.google.com/sounds` |

---

## ⚠️ Reglas para no romper GitHub Pages

| Regla | Por qué |
|---|---|
| No mover `index.html` de `src/homepage/` | Cambiaría la URL activa |
| No renombrar el repo `homepage` | Cambiaría la URL base |
| No cambiar la rama de Pages de `main` | Dejaría de publicarse |
| No añadir Jekyll, Hugo ni generadores | El HTML es estático puro |
| Pages source: raíz `/` de `main` | Configurado en Settings → Pages |

---

## 🗂️ Otros repositorios

| Repo | Descripción |
|---|---|
| [TritonRules](https://github.com/TritonRules/TritonRules) | Perfil GitHub |
| *(próximos proyectos aquí)* | — |
