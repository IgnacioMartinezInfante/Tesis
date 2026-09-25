# 🧠 Graphify: Tesis Project

## 🏗️ Estructura de Clases y Entidades

- **`UIManager`** (`UIManager.cs`)
  - Gestor de la Interfaz de Usuario (UI). Controla pantallas, botones y feedback visual.
- **`StoryManager`** (`StoryManager.cs`)
  - Controlador central de la narrativa. Carga e itera sobre los nodos de historia.
- **`StoryNode`** (`StoryNode.cs`)
  - `ScriptableObject` que representa un nodo de la historia (texto, diálogos, imágenes).
  - **`Opcion`** (Sub-clase) - Representa las diferentes opciones de respuesta/caminos en un nodo.
- **`DistanceTracker`** (`DistanceTracker.cs`)
  - Sistema que trackea la distancia (probablemente basado en geolocalización o movimiento in-game).
- **`SaveSystem`** (`SaveSystem.cs`)
  - Maneja la persistencia de datos (guardado/carga de progreso).
  - **`EstadoJuego`** (`enum`) - Posibles estados globales de la partida.
- **`SeleccionHistoriasManager`** (`SeleccionHistoriasManager.cs`)
  - Controlador del menú principal o selector de tramas/historias disponibles.
- **`SplashManager`** (`SplashManager.cs`)
  - Pantalla inicial o de carga de la aplicación (Splash Screen).

## 🔗 Posibles Dependencias (Grafo Inferido)
- `StoryManager` -> `StoryNode`, `UIManager`, `SaveSystem`
- `SeleccionHistoriasManager` -> `SaveSystem`, `StoryManager`
- `DistanceTracker` -> Eventualmente reportará a un Game/Story Manager.
