# 🎮 Tesis - Directivas del Repositorio

> [!NOTE]
> Este archivo `AGENTS.md` configura las reglas y skills activas específicamente para el proyecto **Tesis**. Aislando este repositorio del resto dentro de la carpeta GitHub.

## 📌 Contexto del Proyecto
- **Tipo:** Proyecto Unity (Móvil / Narrativo / Location-based).
- **Aislamiento:** Este es un repositorio independiente. **ESTRICTAMENTE PROHIBIDO** mezclar dependencias, código o assets con otros proyectos en `c:\Users\bigna\OneDrive\Desktop\Github`.

## ⚡ Skills Activadas para este Proyecto
Al trabajar en este proyecto, se aplicarán proactivamente los siguientes conocimientos desde nuestro core (`Antigravity/.agents/skills`):

1. **`unity-6-mobile-master`**: Optimización Zero-GC, arquitecturas de UI eficientes (pooling, UI Toolkit o Canvas optimizado), y best practices para build en Android/móvil.
2. **`game-feel-and-juiciness`**: Feedback en la UI (squash & stretch, haptics) para que las transiciones de historia, selecciones de opciones y tracking de distancia se sientan responsivas.
3. **`csharp-pro`**: Uso de C# moderno (async/await, structs, pattern matching) para refactorización y escritura de código limpio y desacoplado (especialmente entre `StoryManager`, `SaveSystem` y `UIManager`).
4. **`android-qa-playtesting`**: Asegurar soporte multipantalla y safe areas, clave para los `StoryNodes` y los menús de selección.
5. **`systematic-debugging`**: Diagnóstico metódico de causa raíz antes de realizar cambios de código.
6. **`antislop`**: Código limpio, libre de patrones deprecados y sin comentarios redundantes.

## 🛠️ Graphify Local
- La estructura de este proyecto se encuentra mapeada en `graphify-out/graph.md`. Actualizar el grafo cada vez que se agreguen nuevos sistemas base (como nuevos ScriptableObjects o Managers).
