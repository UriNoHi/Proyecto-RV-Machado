# Truco VR

Proyecto de Unity para jugar al **Truco argentino en realidad virtual**, pensado para Meta Quest. La partida se juega en una mesa virtual contra una IA local; las cartas se seleccionan e interactúan en 3D con XR Interaction Toolkit.

> **Estado del proyecto:** en desarrollo. La escena de la mesa y la lógica principal del juego están en el proyecto, pero la escena todavía no está configurada como escena de inicio para una build.

## Proyecto Unity

El repositorio contiene el proyecto de Unity dentro de la carpeta `Truco-VR/`. Para abrirlo, seleccioná **esa carpeta** desde Unity Hub, no la raíz del repositorio.

- **Versión de Unity:** 6000.6.1f1
- **XR:** XR Interaction Toolkit, XR Management y OpenXR
- **Entrada:** Input System
- **Plataforma prevista:** Meta Quest mediante Android/OpenXR

Unity puede tardar unos minutos en regenerar `Library/` y otros archivos de caché al abrir el proyecto por primera vez.

## Cómo probar la partida en el Editor

1. Abrí `Truco-VR/` con Unity 6000.6.1f1.
2. En el Project, abrí `Assets/_TrucoVR/Scenes/GameTable/GamaTable.unity`.
3. Presioná **Play** para iniciar la escena.

La escena de juego contiene la mesa, la mano del jugador, las cartas y elementos de la partida. También hay escenas de prueba de XR en `Assets/_TrucoVR/Scenes/TestXR/`.

## Funcionalidades implementadas

- Mazo español de 40 cartas y reparto de tres cartas por jugador.
- Comparación de cartas según la jerarquía del Truco.
- Rondas de hasta tres bazas, con resolución de empates.
- Cantos de **Truco**, **Retruco** y **Vale cuatro**, con respuestas de **Quiero** y **No quiero**.
- Cantos de **Envido**, **Real Envido** y **Falta Envido**, con cálculo de tantos.
- Marcador de partida con objetivo predeterminado de 30 puntos.
- IA local sencilla para jugar cartas y responder o proponer cantos. No es multijugador en línea.
- Interacción XR para seleccionar cartas y controles de interfaz en la escena.

## Estructura principal

```text
Truco-VR/
├── Assets/
│   ├── _TrucoVR/
│   │   ├── Art/                 # Cartas y recursos propios del juego
│   │   ├── Scenes/              # Mesa de juego y escenas de prueba XR
│   │   ├── Scripts/
│   │   │   ├── Application/     # Flujo de ronda y marcador
│   │   │   ├── Domain/          # Reglas, mazo, apuestas, puntuación e IA
│   │   │   ├── Presentation/    # Cartas, mano, oponente e interfaz
│   │   │   └── Editor/          # Herramientas del Editor de Unity
│   │   └── Tests/               # Pruebas de lógica de Envido
│   ├── Samples/                 # Muestras de XR Interaction Toolkit
│   └── XR-UNS-Casita/           # Recursos y escenas de entorno XR
├── Packages/                    # Dependencias de Unity
└── ProjectSettings/             # Configuración del proyecto
```

## Compilar para Meta Quest

La configuración actual de Build Settings incluye `Assets/Scenes/SampleScene.unity`, no la escena del juego. Para generar una build de la partida:

1. En Unity, abrí **File > Build Profiles** (o **Build Settings**, según la interfaz disponible) y seleccioná Android.
2. Agregá `Assets/_TrucoVR/Scenes/GameTable/GamaTable.unity` a las escenas de la build y dejala habilitada.
3. Revisá la configuración de XR/OpenXR para el dispositivo Meta Quest y los ajustes de Android.
4. Generá la build y probala en el visor.

La inclusión de OpenXR y de paquetes XR en el proyecto no garantiza por sí sola que todos los ajustes de dispositivo estén listos; verificá la configuración y probá la build en el hardware destino.

## Pruebas

Las pruebas disponibles están en `Truco-VR/Assets/_TrucoVR/Tests/` y usan Unity Test Framework. Se pueden ejecutar desde **Window > General > Test Runner** en el Editor.

## Créditos de las cartas

El arte de las cartas españolas está atribuido en `Truco-VR/Assets/_TrucoVR/Art/Textures/SpanishCards/README.md` a **Basquetteur**, vía Wikimedia Commons, bajo licencia **CC BY-SA 3.0**. Consultá ese archivo para la atribución y los términos completos de la licencia.
