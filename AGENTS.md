# DragonCeltas - Unity 6 2D Action/Platformer

## Tech Stack
- **Engine:** Unity 6000.4.5f1 (Unity 6 LTS)
- **Render Pipeline:** Universal Render Pipeline (URP) 17.4.0, 2D Renderer
- **Input:** New Input System 1.19.0
- **Scripting:** C#, .NET Standard 2.1, Mono runtime
- **2D Packages:** Animation 14.0.3, SpriteShape 14.0.1, Tilemap 1.0.0, Aseprite 4.0.1

## Project Structure
```
Assets/
├── Scenes/
│   ├── SampleScene.unity          # Gameplay principal
│   └── MainMenu.unity             # Menu principal
├── Scripts/
│   ├── Core/                      # GameManager, GameReferences, GameInput, GameEvents, SpriteUtils
│   ├── Player/                    # WarriorController, PlayerMovement, PlayerCombat, PlayerHealth, PlayerUpgrades
│   ├── Enemies/                   # AntiJulian, EnemyHealth, EnemyPool, Spawner
│   ├── UI/                        # MainMenu, HealthBar, HUDManager
│   ├── Camera/                   # CameraFollow
│   ├── Castle/                   # CastleHealth
│   └── Data/                     # ScriptableObjects, game configuration
├── Sprites/                       # Sprite assets
├── Settings/                      # URP and renderer assets
├── Tiny Swords/                   # Third-party asset pack (no tocar)
├── JulianWarrior/                 # Player animator, prefab, sprites
├── Arboles/                       # Arboles prefabs y animaciones
├── Portal/                        # Portal sprites y animaciones
├── Interfaz UI/                   # UI mockups
└── InputSystem_Actions.inputactions
```

## Coding Conventions

### General
- **Namespace:** Default namespace is `DragonCeltas` for all scripts
- **Language:** Use Spanish for variable names, method names, and comments in code. Use English for class names that match Unity concepts (e.g., `HealthBar`, `CameraFollow`)
- **Scripts folder:** ALL C# scripts go in `Assets/Scripts/`, organized by feature in subfolders (Player/, Enemies/, UI/, Core/, etc.)
- **One class per file**, file name matches class name exactly
- Use `MonoBehaviour` for Unity components
- Serialize private fields with `[SerializeField]` instead of making them public
- Follow Unity C# naming: PascalCase for classes/methods/properties, camelCase for private fields
- Use `[Header]` to organize inspectors: `[Header("Movimiento")]`, `[Header("Vida")]`, etc.

### Architecture & Design Patterns

#### Single Responsibility Principle (SRP)
- Each script should have ONE clear purpose. If a class does movement + attack + UI + upgrades, SPLIT IT.
- Current architecture follows SRP: `PlayerMovement`, `PlayerCombat`, `PlayerHealth`, `PlayerUpgrades` are separate
- `EnemyHealth` handles HP/damage separately from `AntiJulian` (AI/movement)

#### Singleton Pattern (use sparingly)
- Only for true global managers: `GameManager`, `AudioManager`, etc.
- Always add a null check: `if (GameManager.Instance != null)`
- Never use singletons as a shortcut to avoid proper references

#### Observer/Event Pattern
- Prefer events over direct coupling. Use `System.Action` or `UnityEvent`:
  ```csharp
  public static event Action OnEnemyDeath;
  public static event Action OnPlayerDeath;
  public static event Action<int> OnScoreGained;
  // Use GameEvents class for all global events
  ```
- This decouples enemies from the spawner and makes the system extensible

#### Component Pattern
- Favor composition over inheritance. Use `[RequireComponent]` and `GetComponent`
- If multiple enemies share behavior, create separate components (e.g., `ChaseTarget`, `DealDamageOnContact`) rather than a deep class hierarchy

#### State Pattern (for complex state machines)
- Use `Animator` parameters for animation states
- For game logic states (menu, playing, paused, game over), use an enum + switch or a `GameState` scriptable object
- Avoid booleans like `isDead`, `isAttacking`, `isGuarding` scattered everywhere — consolidate into a state enum when they grow

#### Object Pooling (for performance)
- Use object pooling for frequently spawned/destroyed objects (enemies, projectiles, UI elements, particles)
- Unity's `UnityEngine.Pool.ObjectPool<T>` is available in Unity 6

### Code Quality Rules

#### REGLA DE ORO — Antes de eliminar o modificar archivos
- **SIEMPRE preguntar antes de borrar** cualquier archivo, carpeta o .meta.
- **Verificar dependencias**: antes de eliminar, revisar si el archivo está referenciado por otros scripts, prefabs, escenas o assets.
- **NUNCA ejecutar Remove-Item -Recurse -Force en Assets sin confirmar** exactamente qué carpetas y archivos se van a borrar.
- Si un archivo .meta está huérfano, preguntar antes de eliminarlo.

#### REGLA DE ORO — Carpetas intocables
- **`Assets/AnimatorControllerGeneral/`** y **`Assets/Prefabs/`** son **ZONA PROHIBIDA**.
- Bajo ninguna circunstancia modificar, crear, o eliminar archivos en estas carpetas.
- Si cualquier cambio de codigo o tarea pudiera afectar estas carpetas, **ABORTAR y preguntar** al Sargento.

#### REGLA DE ORO — Carpeta Library
- **JAMAS eliminar la carpeta `Library/`** bajo ninguna circunstancia.
- Si cualquier solución implica borrar `Library`, borrar cachés, o regenerar archivos del sistema de Unity, **ABORTAR y preguntar** al Sargento.
- Advertir siempre que este tipo de operación puede corromper la escena y causar pérdida de progreso.

#### REGLA DE ORO — Distinguir pregunta de instrucción
- Antes de ejecutar cualquier comando, verificar si el usuario está **preguntando** o **ordenando**.
- Si es una pregunta, solo responder. No ejecutar nada.
- Si es una instrucción, ejecutar. Si hay duda, preguntar.

#### REGLA DE ORO — No limpiar warnings sin permiso
- **NUNCA corregir warnings del compilador sin preguntar.** Si están ahí, es por algo.
- Si un warning es relevante, primero **explicarle al Sargento** qué significa, por qué aparece y qué implicaciones tiene.
- Solo después de explicar y recibir la orden, proceder a corregirlo.

#### No usar Write en archivos existentes
- **SIEMPRE usar `edit`** para cambios puntuales. El `write` borra todo el contenido anterior.
- **JAMAS** usar `write` en archivos que el usuario ha modificado manualmente.
- Si hay que crear un archivo nuevo, usar `write`. Si hay que modificar un archivo existente, usar `edit`.

#### No God Classes
- If a script exceeds ~200 lines, it's doing too much. Refactor into smaller components.
- `WarriorController.cs` is now a slim facade (~16 lines) that delegates to PlayerMovement, PlayerCombat, PlayerHealth, PlayerUpgrades

#### No Hard-Coded Strings
- Use `GameReferences` singleton to access global references (Castle, Player) instead of `GameObject.Find`
- For local references, use `[SerializeField]` private fields assigned in Inspector

#### No Static State Except When Necessary
- `WarriorController.Score` and `WarriorController.IsDead` are instance accessors that delegate to their respective components
- Prefer instance references via `GameReferences` or component references
- `GameEvents` uses static events which is acceptable for global game events

#### Avoid `OnGUI` for Production UI
- `OnGUI` is for prototyping only. Migrate to:
  - Unity UI (Canvas + TextMeshPro) for menus, HUD, health bars
  - TextMeshPro for all text rendering
- Health bars should use UI Images or SpriteRenderers with children, not `OnGUI`
- All HUD elements (score, stats, waves, respawn, upgrades) use `HUDManager` with Canvas + TMP

#### Null Checks & Safety
- Always check if `GameManager.Instance != null` before calling it
- Use `TryGetComponent<T>` instead of `GetComponent<T>` when the component might not exist
- Validate `[SerializeField]` references in `Awake()` or `Start()` with `Debug.Assert`

#### Coroutines vs Update
- Prefer `Update()` with timers over `StartCoroutine` for simple timed events (respawn, cooldowns)
- Use coroutines only for sequential async operations (fade out → wait → load scene)

### Inspector Organization
```csharp
[Header("Movimiento")]
[SerializeField] private float moveSpeed = 3.2f;
[SerializeField] private float sprintSpeed = 6f;

[Header("Vida")]
[SerializeField] private float maxHp = 100f;

[Tooltip("Radio del circulo de ataque")]
[SerializeField] private float attackRadius = 1.2f;

[ Range(0.1f, 5f)]
[SerializeField] private float attackDuration = 0.1f;
```

### Folder Organization Rules
- `Scripts/Core/` — GameManager, SceneLoader, GameState, generic utilities
- `Scripts/Player/` — Anything specific to the player character
- `Scripts/Enemies/` — Enemy AI, spawner, enemy-specific components
- `Scripts/UI/` — Menu controllers, HUD, health bar UI, score display
- `Scripts/Castle/` — Castle health and castle-specific logic
- `Scripts/Camera/` — Camera follow, camera effects
- `Scripts/Data/` — ScriptableObjects, game configuration
- Prefabs go in their own folder alongside their related assets, NOT in Scripts/

### Refactoring Priority List (Current Issues)
1. ~~**Split WarriorController** (460 lines) into: PlayerMovement, PlayerCombat, PlayerHealth, PlayerUpgrades~~ ✅ DONE
2. ~~**Replace OnGUI** with Unity UI (Canvas + TextMeshPro) for all HUD elements~~ ✅ DONE (HUDManager)
3. ~~**Replace GameObject.Find** with Inspector references or a GameReferences component~~ ✅ DONE (GameReferences singleton)
4. ~~**Replace static Score/IsDead** with events or instance references~~ ✅ DONE (instance accessors + GameReferences)
5. ~~**Add Object Pooling** for enemy spawning (avoid Instantiate/Destroy every wave)~~ ✅ DONE (EnemyPool)
6. ~~**Migrate from direct Input polling** to Input System actions~~ ✅ DONE (GameInput class)
7. **Extract EnemyHealth** from AntiJulian ✅ DONE
8. **Fix memory leaks** in SpriteUtils and PlayerCombat ✅ DONE (caching + shared materials)

### Scene Setup Required
After code changes, configure the following in the Unity Editor:
1. **GameReferences**: Add component to a persistent GameObject, assign Castle and Player references
2. **GameInput**: Add component to a persistent GameObject, assign `InputSystem_Actions` asset to `actions` field
3. **HUDManager**: Add component to a persistent GameObject (auto-creates Canvas UI if references are null)
4. **EnemyPool**: Add component to Spawner or separate GameObject, assign enemy prefab to `enemyPrefab`
5. **Enemy prefab**: Add `EnemyHealth` component alongside `AntiJulian`, `HealthBar`, `Rigidbody2D`, etc.
6. **GameManager**: Assign `gameOverPanel`, `gameOverText` (TextMeshProUGUI), `reiniciarButton`, `menuButton` if not using auto-created HUD

## Input Action Maps
- **Player:** Move, Look, Attack, Jump, Sprint, Crouch, Interact, Previous, Next
- **Player (programmatic):** Guard (right click / gamepad), Restart (R), Menu (M/Esc)
- **UI:** Navigate, Submit, Cancel, Point, Click, ScrollWheel

## Key Settings
- 2D Orthographic camera (size=5)
- Linear color space
- HDR enabled
- Global Light 2D in sample scene
- Target resolution: 1920x1080

## WebGL Build Configuration
- **Target:** WebGL 2.0 (default in Unity 6)
- **Compression:** Gzip enabled
- **Memory:** 64 MB initial, max 2048 MB, geometric growth
- **Decompression fallback:** Enabled
- **Data caching:** Enabled
- **Hashed filenames:** Enabled (better caching)
- **Exception support:** Full (development); switch to None for release builds
- **Template:** Default
- **Power preference:** High performance
- **Submodule stripping:** Enabled

### WebGL Restrictions
- No `System.Threading` (single-threaded)
- No direct filesystem access; use `Application.persistentDataPath` and `PlayerPrefs`
- HTTP requests only via `UnityWebRequest` (no `System.Net.Http`)
- No native plugins (`.dll`/`.so`); only `.jslib` plugins allowed
- Audio: Use compressed formats (MP3/AAC/Vorbis) for smaller builds
- Shader variants: Minimize to reduce build time and size

### Para publicar en WebGL
1. Asegurar que el modulo **WebGL Build Support** esta instalado en Unity Hub
2. Build Settings > Platform > WebGL > Switch Platform
3. Player Settings > Resolution: 1280x720 (o adaptable)
4. Player Settings > Publishing Settings > Compression Format: Gzip
5. Para release: Exception Support > None, Strip Engine Code > enabled