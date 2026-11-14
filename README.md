# WARPED

**Warped** és un joc de combat per torns sobre una quadrícula, fet amb Unity. Tries un heroi i t'enfrontes a un enemic gegant. Has de moure't per la quadrícula, gestionar el mana i esquivar els atacs de l'enemic fins a derrotar-lo.

![Menú principal](Docs/screenshots/main_menu.png)

---

## Com jugar

1. Descomprimeix el fitxer `.zip` del joc.
2. Obre la carpeta i executa **`Warped.exe`**.

**Requisits:** Windows 10/11 de 64 bits. No cal tenir Unity instal·lat.

> `Warped.exe` ha de quedar a la mateixa carpeta que `Warped_Data/`, `UnityPlayer.dll` i `MonoBleedingEdge/`. Si el mous sol, no s'obrirà.

---

## Versió de Unity i com obrir el projecte

- **Versió de Unity:** **Unity 6.3 LTS (6000.3.24f1)**
- **Render pipeline:** Universal Render Pipeline (URP)
- **Paquets:** Input System, TextMeshPro, Unity UI (s'instal·len sols amb el projecte)

Per obrir el projecte a l'editor:
1. Instal·la **Unity 6000.3.24f1** des de Unity Hub.
2. A Unity Hub, prem **Add → Add project from disk** i selecciona la carpeta `ProjecteWarpped`.
3. Obre l'escena **`Assets/Scenes/MainMenu.unity`** i prem **Play**.

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

Totes les tecles es poden canviar a **Options → Controls → Reassign Keys**. En aquest menú també es pot ajustar la **velocitat de la càmera** i **invertir els eixos**. La configuració es guarda entre partides.

![Opcions de controls](Docs/screenshots/controls_menu.png)

---

## Captures

| | |
|---|---|
| ![Menú principal](Docs/screenshots/main_menu.png) | ![Selecció d'heroi](Docs/screenshots/hero_selection.png) |
| ![Combat](Docs/screenshots/combat.png) | ![Atac de l'enemic](Docs/screenshots/enemy_attack.png) |
| ![Menú de pausa](Docs/screenshots/pause_menu.png) | ![Victòria](Docs/screenshots/victory.png) |

---

## Crèdits

- Models i animacions dels personatges: **KayKit** (Kay Lousberg).
- Escenaris: **KayKit Medieval Hexagon Pack** i altres packs gratuïts.
- Enemics: diversos packs gratuïts de l'Asset Store.
