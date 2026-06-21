---
name: unity-topdown
description: Comprehensive Unity 2D Top-Down game development patterns, scripting best practices, and workflows. Covers Rigidbody2D, Animator, Input System, Camera, Sprites, Editor tools, and common pitfalls.
---
## Unity 2D Top-Down — Guia Completa

### 1. Rigidbody2D y Fisicas

**Fuente:** [Unity Docs - Rigidbody2D Body Types](https://docs.unity3d.com/6000.0/Documentation/Manual/2d-physics/rigidbody/body-types/)

| Body Type | Gravedad | Colisiones | Uso tipico |
|---|---|---|---|
| Dynamic | Si | Con todos | Objetos con fisica real (cajas, enemigos empujables) |
| **Kinematic** | **No** | Solo con Dynamic | **Personaje controlado por codigo (RECOMENDADO)** |
| Static | No | Con Dynamic | Paredes, suelo, objetos inamovibles |

**Regla de oro para personajes:** Usar **Kinematic**. Segun la doc oficial: "is designed to move under simulation, but only under very explicit user control. While a Dynamic Rigidbody 2D is affected by gravity and forces, a Kinematic Rigidbody 2D is not."

```csharp
void Start()
{
    rb = GetComponent<Rigidbody2D>();
    rb.bodyType = RigidbodyType2D.Kinematic;
    // NO necesitas rb.gravityScale = 0; Kinematic ya ignora la gravedad.
}
```

**⚠️ Importante:** "A Kinematic Rigidbody 2D does not collide with other Kinematic Rigidbody 2Ds or with Static Rigidbody 2Ds." — solo colisiona con Dynamic. Si necesitas que dos personajes choquen entre si, uno debe ser Dynamic o usar triggers.

**Reposicionamiento:** `rb.MovePosition()` (recomendado) o `rb.position` (directo). La doc dice: "To reposition a Kinematic Rigidbody 2D, it must be repositioned explicitly via Rigidbody2D.MovePosition."

### 2. Movimiento: Update vs FixedUpdate

**Fuente:** [Unity Docs - Execution Order](https://docs.unity3d.com/6000.0/Documentation/Manual/execution-order.html)

Orden de ejecucion relevante:
```
Update()            ← input, logica de juego, ANIMATOR (por defecto)
  └─ Animator (Normal mode)
LateUpdate()        ← camara (despues de que todo se movio)
FixedUpdate()       ← fisica (frecuencia fija, puede desfasarse del render)
```

**Para movimiento top-down via codigo:** mover en **Update()** con `Time.deltaTime`.

```csharp
void Update()
{
    // Input y animaciones
    LeerInput();
    
    // Movimiento
    rb.position += moveInput * moveSpeed * Time.deltaTime;
}
```

**NO usar** `rb.linearVelocity` en FixedUpdate para personajes controlados por input. Por que:
- FixedUpdate corre a 50Hz por defecto, Update a la tasa de frames del monitor (60/120/144Hz)
- El desfase entre ambos causa **stuttering visual (jitter)**
- El Animator en modo "Normal" se actualiza en Update, no en FixedUpdate

### 3. Input System (New Input System 1.19+)

**Fuente:** [Unity Docs - Input System Workflows](https://docs.unity3d.com/Packages/com.unity.inputsystem@1.19/manual/Workflows.html)

Hay **3 workflows** oficiales:

| Workflow | Flexibilidad | Setup | Mejor para |
|---|---|---|---|
| **Actions (recomendado)** | Alta | Medio | Produccion, multi-plataforma |
| Actions + PlayerInput | Muy alta | Alto | Multiplayer, callbacks automaticos |
| **Direct device state** | Baja | Bajo | **Prototipado rapido (lo que usamos)** |

**Nuestro enfoque (Direct device state):** Segun la doc, "the simplest and most direct, but the least flexible... useful for quick implementation with one specific type of device."

```csharp
using UnityEngine.InputSystem;

var kb = Keyboard.current;
if (kb == null) return;

// Movimiento con ambas manos (WASD + flechas)
float h = 0f, v = 0f;
if (kb.aKey.isPressed || kb.leftArrowKey.isPressed)  h = -1f;
if (kb.dKey.isPressed || kb.rightArrowKey.isPressed) h =  1f;
if (kb.wKey.isPressed || kb.upArrowKey.isPressed)    v =  1f;
if (kb.sKey.isPressed || kb.downArrowKey.isPressed)  v = -1f;

// Mouse
var mouse = Mouse.current;
if (mouse == null) return;
mouse.leftButton.wasPressedThisFrame   // click unico (ataque)
mouse.rightButton.isPressed            // mantener presionado (defensa)
```

**Ventaja de Actions:** Si el proyecto crece, migrar a Actions permite rebinding de teclas, soporte multi-dispositivo, y mejor organizacion. Se hace con `InputAction` references en `Start()` y `action.ReadValue<Vector2>()` en `Update()`.

### 4. Animator y Animaciones 2D

**Fuente:** [Unity Docs - Animator Component](https://docs.unity3d.com/6000.0/Documentation/Manual/class-Animator.html)

**Configuracion del Animator:**
```
animator.applyRootMotion = false;  // OBLIGATORIO para personajes movidos por codigo
animator.updateMode = AnimatorUpdateMode.Normal;  // default, se actualiza en Update
```

**Update Modes:**
- `Normal` → se actualiza con Update (recomendado para personajes 2D)
- `Animate Physics` → se actualiza con FixedUpdate (solo si el movimiento es fisico)
- `Unscaled Time` → ignora timeScale (para UI)

**Parametros de Animator:**
| Tipo | Uso | Ejemplo |
|---|---|---|
| Float | Transicion suave Idle↔Run | `SetFloat("Speed", magnitude)` |
| Trigger | Accion instantanea (ataque) | `SetTrigger("Ataque1")` |
| Bool | Estado sostenido (guardia) | `SetBool("EnGuardia", true)` |
| Integer | Estados numericos | `SetInteger("ComboStep", 1)` |

**Transiciones en el Animator Controller:**
- `Has Exit Time = false` → para transiciones manejadas por parametros (Speed > 0.1, Trigger, Bool)
- `Has Exit Time = true` + `Exit Time = 1.0` → para que la animacion de ataque termine antes de volver a Idle
- `Transition Duration = 0` → para transiciones instantaneas (ataque)
- `Transition Duration = 0.1` → para blends suaves (Idle↔Run)

**Animation Clips:**
- **Loop Time = true** → obligatorio para Idle, Run, Guard
- **Loop Time = false** → para Ataque1, Ataque2 (animaciones que se reproducen una vez)
- Usar Editor script para bulk-fix (ver seccion 8)

**StateMachineBehaviour:** Para logica avanzada en estados de animacion:
```csharp
public class AttackStateBehaviour : StateMachineBehaviour
{
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) { }
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) { }
}
```
Usar `OnStateExit` para detectar cuando termino una animacion de ataque sin necesidad de timers.

### 5. Sprites y Renderizado

**SpriteRenderer:**
```csharp
spriteRenderer.flipX = (h < 0f);   // voltear al ir a la izquierda
spriteRenderer.flipX = false;      // solo si h > 0, no cambiar en idle (h == 0)
```

**Sorting Layers:** Crear layers en Project Settings > Tags and Layers:
- Default (orden 0)
- Player (orden 1)
- Enemies (orden 2)
- UI (orden 10)

Para top-down, si necesitas Y-sorting (objetos mas abajo se renderizan encima), usar `sortingOrder` o `Transparency Sort Mode` en Project Settings > Graphics.

**Pixel Art sprites (import settings):**
- Filter Mode: **Point (no filter)**
- Compression: **None**
- Pixels Per Unit: valor que calce con el tilemap o grid

### 6. Camara para Top-Down

**Camara Follow basica (LateUpdate + Lerp):**
```csharp
void LateUpdate()
{
    if (target == null) return;
    var desired = target.position + offset;  // offset tipico: (0, 0, -10)
    transform.position = Vector3.Lerp(transform.position, desired, smoothSpeed);
}
```

**Por que LateUpdate:** Para seguir al personaje DESPUES de que este ya se movio en Update. Asi evitas jitter.

**Configuracion de camara 2D:**
- Projection: **Orthographic**
- Size: 5 (default, ajustable segun necesites mas o menos zoom)
- Clear Flags: Solid Color (fondo negro/color solido)
- Z position: -10 (para que quede detras del UI pero delante de todo lo demas)

**Cinemachine (avanzado):** Para camaras mas complejas (confine, damping, multiple targets), usar el paquete Cinemachine. Ya viene incluido en Unity 6.

### 7. Colliders para Personajes

**Tipos de Collider2D disponibles:** Circle, Box, Polygon, Edge, Capsule, Composite, Custom.

**Para un personaje 2D top-down:**
```csharp
// BoxCollider2D o CircleCollider2D como trigger/hitbox
var col = GetComponent<BoxCollider2D>();
col.isTrigger = true;  // si no queres colisiones fisicas, solo deteccion
```

**CapsuleCollider2D** es ideal para personajes (forma redondeada), pero BoxCollider2D rectangular es mas simple para sprites cuadrados.

**Nota de la doc:** "You can't use 3D GameObjects with 2D colliders, or 2D GameObjects with 3D colliders."

### 8. Editor Scripts Utiles

**Reparar Loop Time en animaciones (bulk fix):**
```csharp
using UnityEditor;
using UnityEngine;

public static class AnimationFixer
{
    [MenuItem("DragonCeltas/Reparar Animaciones (Loop)")]
    public static void FixLoopTime()
    {
        var guids = AssetDatabase.FindAssets("t:AnimationClip", 
            new[] { "Assets/JulianWarrior" });
        foreach (var guid in guids)
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            var clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
            var settings = AnimationUtility.GetAnimationClipSettings(clip);
            settings.loopTime = true;
            AnimationUtility.SetAnimationClipSettings(clip, settings);
            EditorUtility.SetDirty(clip);
        }
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }
}
```

### 9. Patron de Scripting para Controladores

**Estructura recomendada para un controlador de personaje:**
```csharp
using UnityEngine;
using UnityEngine.InputSystem;

namespace DragonCeltas
{
    [RequireComponent(typeof(Animator))]
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(SpriteRenderer))]
    [RequireComponent(typeof(BoxCollider2D))]
    public class WarriorController : MonoBehaviour
    {
        [Header("Movimiento")]
        [SerializeField] private float moveSpeed = 5f;

        private Animator animator;
        private Rigidbody2D rb;
        private SpriteRenderer sr;
        
        void Start()
        {
            animator = GetComponent<Animator>();
            rb = GetComponent<Rigidbody2D>();
            sr = GetComponent<SpriteRenderer>();
            animator.applyRootMotion = false;
            rb.bodyType = RigidbodyType2D.Kinematic;
        }
    }
}
```

**Convenciones:**
- `[RequireComponent]` para dependencias obligatorias
- `[SerializeField]` para campos privados visibles en el Inspector
- `[Header]` para organizar el Inspector
- Namespace `DragonCeltas`
- Metodos en PascalCase
- Campos privados en camelCase
- `using UnityEngine.InputSystem;` para Input System

### 10. Sistema de Combo de Ataque

```csharp
[Header("Ataque")]
[SerializeField] private float comboWindow = 0.6f;
private bool enCombo, estaAtacando;
private float comboTimer;

void LeerAtaque()
{
    var mouse = Mouse.current;
    if (mouse == null || !mouse.leftButton.wasPressedThisFrame) return;

    if (enCombo)
    {
        animator.SetTrigger("Ataque2");
        enCombo = false;
        comboTimer = 0f;
    }
    else
    {
        animator.SetTrigger("Ataque1");
        enCombo = true;
        comboTimer = comboWindow;
    }
    estaAtacando = true;
}

void ActualizarCombo()
{
    if (!enCombo) return;
    comboTimer -= Time.deltaTime;
    if (comboTimer <= 0f)
    {
        enCombo = false;
        estaAtacando = false;
    }
}

void Mover()
{
    if (estaEnGuardia || estaAtacando) return;
    rb.position += moveInput * moveSpeed * Time.deltaTime;
}
```

### 11. Common Pitfalls / Errores Frecuentes

| Sintoma | Causa | Solucion |
|---|---|---|
| **Stuttering / jitter visual** | Movimiento en FixedUpdate, render en Update | Mover en Update con Time.deltaTime |
| **"Invalid Layer Index -1"** | Nombre de estado no existe en el controller | Verificar Animator.Play("StateName") |
| **Animacion no hace loop** | Loop Time = false en el .anim | Editor script o checkbox manual |
| **Root motion interfiere** | applyRootMotion = true | `animator.applyRootMotion = false` |
| **Personaje se "arrastra"** | Rigidbody Dynamic con gravedad | Usar Kinematic |
| **No colisiona con paredes** | Kinematic no colisiona con Static | Usar triggers + raycasts, o Dynamic para enemigos |
| **Input no responde** | Keyboard.current = null | Verificar que el Input System package este activo |
| **Sprites borrosos** | Filter Mode = Bilinear | Cambiar a Point (no filter) |
| **Animator no reproduce** | Controller no asignado | Asignar .controller al componente Animator |

### 12. Project Structure (DragonCeltas)

```
Assets/
├── JulianWarrior/                 ← Carpeta del personaje
│   ├── WarriorController.cs       ← Control principal (input + anim + movimiento)
│   ├── CameraFollow.cs            ← Seguimiento de camara
│   ├── Warrior Blue Animations/   ← .anim + .controller
│   │   ├── Warrior_Idle_Blue.anim
│   │   ├── Warrior_Run_Blue.anim
│   │   ├── Warrior_Attack1_Blue.anim
│   │   ├── Warrior_Attack2_Blue.anim
│   │   ├── Warrior_Guard_Blue.anim
│   │   └── Warrior_Blue.controller
│   ├── *.png                      ← Sprites originales
│   └── Editor/                    ← Scripts solo para editor
│       └── AnimationFixer.cs
├── InputSystem_Actions.inputactions ← Config de Input System
├── Scenes/
├── Settings/                      ← URP / Render Pipeline
└── Tiny Swords/                   ← Assets originales
```

### 13. Configuracion del Proyecto (DragonCeltas)

| Config | Valor |
|---|---|
| Unity | 6000.4.5f1 (Unity 6 LTS) |
| Render Pipeline | URP 17.4.0, 2D Renderer |
| Color Space | Linear |
| HDR | Enabled |
| Input | New Input System 1.19.0 |
| .NET | Standard 2.1, Mono |
| Namespace | `DragonCeltas` |
| Camara | Orthographic, Size=5 |
| Target | 1920x1080 |
