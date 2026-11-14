# WARPED

**Warped** és un joc de combat per torns sobre una quadrícula, fet amb Unity. Tries un heroi i t'enfrontes a un enemic gegant. Has de moure't per la quadrícula, gestionar el mana i esquivar els atacs de l'enemic fins a derrotar-lo.

![Menú principal](Docs/screenshots/main_menu.png)

---

## Jugar directament (executable)

No cal tenir Unity instal·lat per jugar.

1. Obre la carpeta **`Build/`** del repositori.
2. Executa **`Warped.exe`** (Windows 10/11, 64 bits).

> No moguis `Warped.exe` fora de la carpeta: necessita els fitxers que té al costat (`Warped_Data/`, `UnityPlayer.dll`, `MonoBleedingEdge/`...). Si el vols passar a algú, comprimeix tota la carpeta `Build/` en un `.zip`.

---

## Requisits per obrir el projecte

| Requisit | Versió |
|---|---|
| **Unity** | **Unity 6.3 LTS (6000.3.24f1)** |
| Render pipeline | Universal Render Pipeline (URP) |
| Paquets | Input System, TextMeshPro, Unity UI (s'instal·len sols amb el projecte) |
| Sistema operatiu | Windows, macOS o Linux |
| Mòdul per generar l'executable | Windows Build Support (Mono) |

> Es recomana fer servir exactament la versió **6000.3.24f1**. Amb una altra versió, Unity demanarà actualitzar el projecte i alguns elements podrien no funcionar bé.

---

## Com obrir el projecte

1. Instal·la **Unity Hub** i, des d'allà, la versió **Unity 6000.3.24f1**.
2. Descomprimeix la carpeta del projecte (`ProjecteWarpped`).
3. A Unity Hub, prem **Add → Add project from disk** i selecciona la carpeta `ProjecteWarpped`.
4. Obre el projecte. La primera vegada Unity importarà tots els recursos i trigarà uns minuts.
5. Obre l'escena **`Assets/Scenes/MainMenu.unity`** i prem **Play**.

### Arbres del terreny (opcional)
Si a la consola surt l'avís *"The tree X must use the Nature/Soft Occlusion shader"*, executa **Tools → Warped → Fix Terrain Trees** i després guarda l'escena.

---

## Objectiu del joc

Derrotar l'enemic de cada fase deixant-li la vida a 0 abans que ell et derroti a tu.

- A cada torn reps **mana**, que serveix per **moure't** i per **atacar**.
- Cada atac té un **cost de mana**, un **dany** i un **rang**. Els atacs cos a cos només funcionen a prop de l'enemic, i els atacs a distància només des de lluny.
- Quan acabes el torn, l'enemic **marca caselles en taronja**: és on caurà el seu atac. Si al final del seu torn hi ets a sobre, reps mal.
- Les caselles **vermelles** són perilloses i et treuen vida si les trepitges.
- Les caselles **verdes** et curen.
- Quan derrotes l'enemic passes a la **fase següent**, que és més difícil. Si et quedes sense vida, pots tornar-ho a provar (**Try Again**) o tornar al menú.

![Combat](Docs/screenshots/combat.png)

---

## Personatges

| Heroi | Rol | Armes | Característiques |
|---|---|---|---|
| **Knight** | Cos a cos | Espasa + escut | Resistent i equilibrat |
| **Barbarian** | Cos a cos | Dues destrals | Molta vida i el dany més alt cos a cos |
| **Ranger** | A distància | Arc + fletxes | Fràgil, però ataca des de lluny |
| **Mage** | A distància | Bastó + llibre | Molt mana i dany alt, però poca vida |
| **Rogue** | Híbrid | Daga + ballesta | Ràpid i versàtil, de prop i de lluny |

![Selecció d'heroi](Docs/screenshots/hero_selection.png)

---

## Controls

| Acció | Tecla per defecte |
|---|---|
| Moure's | **Clic** a una casella o **fletxes** del teclat |
| Atac 1 / 2 / 3 | **1**, **2**, **3** (o clic a les cartes d'atac) |
| Saltar el torn | **Enter** |
| Canviar el mode de camí (Normal / Segur) | **C** |
| Mostrar / amagar els controls | **Tab** |
| Girar la càmera | **A** / **D** (horitzontal), **W** / **S** (vertical) |
| Apropar / allunyar la càmera | **Rodeta del ratolí** |
| Pausa | **Esc** |

Totes les tecles es poden canviar a **Options → Controls → Reassign Keys**. En aquest menú també es pot ajustar la **velocitat de la càmera** i **invertir els eixos** horitzontal i vertical. La configuració es guarda entre partides.

![Opcions de controls](Docs/screenshots/controls_menu.png)

---

## Opcions

- **Sound**: volum general, de la música i dels efectes.
- **Controls**: velocitat de la càmera, invertir els eixos i reassignar les tecles.

---

## Captures

| | |
|---|---|
| ![Menú principal](Docs/screenshots/main_menu.png) | ![Selecció d'heroi](Docs/screenshots/hero_selection.png) |
| ![Combat](Docs/screenshots/combat.png) | ![Atac de l'enemic](Docs/screenshots/enemy_attack.png) |
| ![Menú de pausa](Docs/screenshots/pause_menu.png) | ![Victòria](Docs/screenshots/victory.png) |

---

## Estructura del projecte

```
ProjecteWarpped/
├── Build/               Executable del joc (Warped.exe)
├── Docs/screenshots/    Imatges del README
└── Assets/
    ├── Scenes/          Escenes del joc (MainMenu, LoadingScene, FirstLevel, SecondLevel)
    ├── Scripts/
    │   ├── Sripts/          Lògica del combat (PlayerMover, EnemyLogic, GridBehaviour...)
    │   ├── MenuScripts/     Menús (principal, pausa, victòria, derrota, selecció)
    │   ├── OptionsScript/   Opcions de so i controls
    │   ├── CamScripts/      Càmera orbital
    │   └── Audio/           Gestor d'efectes de so
    ├── Prefabs/         Personatges, enemics, HUD...
    ├── Music/           Música i efectes de so
    ├── Resources/       Llibreria de sons (GameSoundLibrary)
    └── Imports/         Recursos externs (models, animacions, terrenys)
```

---

## Crèdits

- Models i animacions dels personatges: **KayKit** (Kay Lousberg).
- Escenaris: **KayKit Medieval Hexagon Pack** i altres packs gratuïts.
- Enemics: diversos packs gratuïts de l'Asset Store.
