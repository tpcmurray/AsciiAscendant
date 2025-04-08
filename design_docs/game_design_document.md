# Game Design Document: ASCII Ascendant Prototype

## 1. Overview

* **Game Title:** ASCII Ascendant
* **Genre:** ASCII Action Role-Playing Game (ARPG)
* **Target Platform:** PC (Steam initially)
* **Target Audience:** ARPG fans, players who appreciate classic ASCII games (e.g., Dwarf Fortress, Caves of Qud), and those seeking deep, complex gameplay without demanding graphical fidelity.
* **Core Concept:** An ASCII-based ARPG focused on deep combat, complex character progression through a skill tree, challenging enemies with intelligent AI, and a rewarding loot system (including unique handcrafted items), all presented within a visually rich ASCII environment.

## 2. Gameplay

### 2.1. Core Gameplay Loop

1.  **Exploration:** Navigate a tile-based ASCII world.
2.  **Combat:** Engage in real-time combat with enemies.
3.  **Progression:** Gain experience, level up, and invest in a complex skill tree.
4.  **Loot:** Acquire randomized gear and unique handcrafted items to enhance character power.
5.  **Repeat:** Explore new areas, face tougher challenges, and further develop the character.

### 2.2. Combat

* **Real-time Action:** Combat occurs in real-time.
* **Targeting:** Players select enemies by clicking on any of their ASCII cells with the mouse. A visual indicator ([ ]) will appear to the left and right of the selected enemy.
* **Movement:** Players move using the WASD keys.
* **Skills:** Skills are mapped to number keys for quick access. Cooldowns will be visually represented on the skill bar with a greyed-out state and a countdown timer.
* **Resources:** Players manage Health and Stamina. Health depletion leads to defeat. Stamina is consumed by all active abilities, excluding the basic weapon attack, and regenerates over time.
* **Enemy AI:** Enemies in the MVP will have basic AI: pathfinding towards the player and attacking at melee range.
* **Hit Feedback:** When a mob or the player is hit in combat (melee or ability), they will be visually shifted one cell away from the attacker and then immediately shifted back to their original position, providing clear feedback of a successful hit.
* **Death:** Enemy death will be indicated by visual ASCII effects and sound effects (to be implemented).
* **Complex Combat (Future Potential):** The foundation for future complex combat with strategic use of various skills and abilities is a key design pillar.

### 2.3. Character Progression

* **No Fixed Classes:** Players are not locked into specific classes. Progression is driven by choices made in the skill tree.
* **Experience and Levels:** Players gain experience points (XP) by defeating enemies. Accumulating enough XP leads to leveling up.
* **Skill Points:** Upon leveling up, players will receive skill points to invest in the tech tree (design details to be determined).
* **Base Attributes:** Core attributes (Strength, Intelligence, Dexterity) influence character capabilities:
    * **Strength:** Increases melee weapon damage.
    * **Intelligence:** Increases the potency of spells.
    * **Dexterity:** Increases the chance to dodge attacks.

### 2.4. Loot System

* **Randomized Loot:** Enemies will drop randomized gear, including weapons and armor with primary stats (e.g., Weapon Damage, Armor Value) and potentially random secondary stats (affixes) such as:
    * +Max Health
    * +Max Stamina
    * +Strength
    * +Intelligence
    * (More to be determined)
* **Unique Items:** In addition to randomized loot, the game will feature unique, handcrafted items with specific names and fixed properties. These items will likely be found in specific locations or dropped by particular boss enemies.
* **Loot Tiers:** Item power will be visually indicated by color-coded tiers (e.g., Grey -> Common, Green -> Uncommon, Blue -> Rare, Purple -> Epic, etc. - further tiers can be added).
* **Inventory:** Players will have an inventory panel (accessible via a hotkey) to manage their acquired items.

### 2.5. World Interaction

* **Tile-Based Movement:** The world is navigated on a grid of ASCII characters.
* **World Map (Future Potential):** A world map will eventually be implemented, revealing points of interest as they are discovered or learned about. No minimap will be present.

## 3. Visual and Audio Design

### 3.1. ASCII Aesthetics

* **Full Color Palette:** The game will utilize a full range of colors to differentiate elements.
* **Extended ASCII Characters:** Heavy use of the high/extended ASCII character set, including block characters (█, ▄, ░, ▒, ▓), box drawing characters (╗), and potentially custom-designed glyphs within the limitations of ASCII.
* **Entity Representation:** Player characters may be around 5 ASCII characters high, while enemies will range from 3 to 15 characters high, allowing for visual distinction and animation.
* **Environmental Representation:** Terrain, walls, buildings, and flora will be represented using a combination of ASCII characters and color.

### 3.2. Audio

* **Sound Effects:** Sound effects will be used to enhance gameplay feedback (e.g., hits, enemy deaths).
* **Music:** Background music, consistent with a high fantasy setting, will be implemented.

### 3.3. User Interface (UI)

* **Main Game Screen:** The primary focus will be on the ASCII game world.
* **Skill Bar:** Located on the left side towards the bottom, displaying active skills, their hotkeys (number keys), cooldown status (greyed out with countdown), and potentially short labels. Items with active abilities might also be displayed here.
* **Health/Stamina Bars:** Displayed above the player and enemy characters using block characters (e.g., ▄) to represent a percentage of their total. Exact numerical values will be shown in a pop-up tooltip upon mouse hover. Enemy level and combat-related debuffs (represented by single characters if possible) will also be displayed above enemies.
* **Inventory Panel:** Accessible via a hotkey, allowing players to view and manage their collected items.
* **Other Panels (Future):** Panels for the world map, tech tree, character information, and quests are planned for later development.

### 3.4. Controls

* **Movement:** WASD keys.
* **Targeting/Interaction:** Mouse clicks to select enemies and potentially interact with the environment.
* **Skills:** Number keys (1-?).
* **Healing Potion (Example):** Q key.
* **Panel Navigation (Future):** Hotkeys will be implemented for accessing different game panels.

## 4. MVP (Minimum Viable Product) Scope

The initial prototype will focus on the following core features:

* **Player:**
    * Basic movement (WASD).
    * 1-2 core attack skills (mapped to number keys).
    * 1 defensive/utility skill (mapped to a number key).
    * Health and Stamina resources.
    * Basic attribute system (Strength, Intelligence, Dexterity) influencing relevant skills/stats.
* **Enemies:**
    * 1-2 basic enemy types with simple AI (move towards player, attack at melee range).
    * Health bar displayed above their head.
* **Combat:**
    * Real-time combat.
    * Mouse click targeting of enemies (visual selection indicator [ ]).
    * Damage calculation.
    * Basic hit feedback (one cell shift + sound).
    * Enemy death (ASCII effect + sound).
* **Loot:**
    * Enemies drop basic randomized gear (weapons/armor affecting damage/defense).
    * 1-2 random stats (e.g., +Health, +Damage).
    * Basic color tiering (grey/green).
    * Simple inventory panel (view only).
* **Environment:**
    * One small, self-contained test area (e.g., a small dungeon room or forest clearing).
* **Core Loop:** Enter area -> Fight enemies -> Get loot -> Equip better loot (manual, within inventory) -> Fight slightly tougher enemies (potentially just more numerous or with slightly higher stats).

## 5. Technology

* **Game Engine:** None, built from scratch for terminal.

## 6. Future Development (Beyond MVP)

* Complex skill tree and character progression.
* More diverse enemy types with advanced AI and attack patterns.
* A larger, non-procedurally generated world to explore.
* A compelling story and quest system.
* More intricate loot mechanics (unique items, set bonuses, crafting, enchanting).
* Additional UI panels (world map, tech tree, character sheet, quest log).
* More sophisticated visual and audio effects.