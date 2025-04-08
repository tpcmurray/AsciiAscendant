# **Technical Design Document: ASCII Ascendant**

## **1. Introduction**

This document outlines the technical design for ASCII Ascendant, an ASCII-based action role-playing game (ARPG). It details the software architecture, core components, and technologies used in the game's development.

## **2. Project Overview**

* **Game Title:** ASCII Ascendant  
* **Genre:** ASCII Action Role-Playing Game (ARPG)  
* **Target Platform:** PC (Steam initially)  
* **Core Concept:** An ASCII-based ARPG focused on deep combat, complex character progression, challenging enemies, and a rewarding loot system.

## **3. System Architecture**

### **3.1. High-Level Architecture**

The game will be developed using a layered architecture to separate concerns and improve maintainability. The main layers are:

* **Engine Layer:** Handles game initialization, the game loop.  
* **Core Layer:** Contains the core game logic, including world representation, entity management, combat, AI, and the loot system. This layer is engine-agnostic.  
* **UI Layer:** Manages the user interface and rendering using custom code.  
* **Data Layer:** Handles data access, loading game data from JSON files (and potentially other sources in the future).

### **3.2. Layer Details**

#### **3.2.1. Engine Layer**

* **Purpose:** Initializes game, manages the game loop, and serves as the entry point for the application.  
* **Key Components:**  
  * Game: The main Game class, responsible for:  
    * Initializing the game.  
    * Creating and managing game screens.  
    * Handling the game loop.

#### **3.2.2. Core Layer**

* **Purpose:** Contains the core game logic, independent of any specific engine or rendering library.  
* **Key Components:**  
  * GameState: Manages the overall game state, including the current map, player character, and game mode.  
  * World:  
    * Tile: Represents a single cell in the game world.  
    * Map: Represents the game world as a grid of tiles. 
    * WorldData: Handles loading and saving map data.  
  * Entities:  
    * Entity (Abstract): Base class for all game entities.  
    * Creature (Abstract): Base class for living entities (player, enemies), inheriting from Entity. Contains health, stamina, attributes, skills, effects, and inventory. 
      * Ascii representation. Can be multiple lines. The player will be 5 lines high by default.
    * Player: Represents the player character, inheriting from Creature.  
    * Enemy (Abstract): Base class for enemies, inheriting from Creature.  
    * Item: Represents an item in the game world, inheriting from Entity.  
  * Combat:  
    * CombatEngine: Handles combat logic, calculating and applying damage.  
    * Damage: Represents a damage value.  
  * AI:  
    * AIAgent (Abstract): Base class for AI agents.  
    * BasicAI: A simple AI agent that moves towards the player and attacks.  
  * Loot:  
    * LootTable: Defines the possible loot drops.  
    * LootGenerator: Generates loot based on a loot table.  
    * ItemQuality: Represents the quality of an item.  
  * Attributes: Represents entity attributes (e.g., Strength, Intelligence, Dexterity).  
  * Events:  
    * GameEvent (Abstract): Base class for all game events.  
    * EntityDamagedEvent: Event raised when an entity takes damage.

#### **3.2.3. UI Layer**

* **Purpose:** Manages the user interface and rendering of the game.  
* **Technology:** custom code 
* Key Components:  
  * Panel: Base class for all UI panels, containing common properties like position and size.  
  * Screens:  
  * GameScreen: The main game screen. This is 90 x 50 characters.
  * InventoryScreen: Displays the player's inventory.  
  * Elements:  
  * StatusBar: Displays health and stamina bars.  
  * SkillBar: Displays the player's skill bar.  
  * InventoryPanel: Displays the player's inventory.  
  * UIRenderers:  
  * WorldRenderer: Renders the game world and entities.  
  * EntityRenderer: Renders individual entities.

#### **3.2.4. Data Layer**

* **Purpose:** Handles access to game data, such as item definitions, skill trees, and world data.  
* **Key Components:**  
  * DAL: Data Access Layer class, providing methods for loading game data from JSON files. This abstracts the data source, allowing for easier transition to a database in the future.

## **4. Class Diagrams**

### **4.1. Core Layer Class Diagram**

```
[Class diagram of Core Layer]
Core  
│  
├───GameState  
│  
├───World  
│   ├───Tile  
│   ├───Map  
│   └───WorldData  
│  
├───Entities  
│   ├───Entity \[abstract\]  
│   ├───Creature \[abstract\]  
│   │   ├───Player  
│   │   └───Enemy \[abstract\]  
│   └───Item  
│  
├───Combat  
│   ├───CombatEngine  
│   └───Damage  
│  
├───AI  
│   ├───AIAgent \[abstract\]  
│   └───BasicAI  
│  
├───Loot  
│   ├───LootTable  
│   ├───LootGenerator  
│   └───ItemQuality  
│  
├───Attributes  
│  
└───Events  
    ├───GameEvent \[abstract\]  
    └───EntityDamagedEvent
```
### **4.2. UI Layer Class Diagram**

```
[Class diagram of UI Layer]  
UI  
│  
├───Panel \[abstract\]  
│   ├───Screens  
│   │   ├───GameScreen  
│   │   └───InventoryScreen  
│   │  
│   └───Elements  
│       ├───StatusBar  
│       ├───SkillBar  
│       └───InventoryPanel  
│  
└───UIRenderers  
    ├───WorldRenderer  
    └───EntityRenderer
```

## **5. System Interactions**

### **5.1. Combat System Interaction**

1. **Input:** Player activates a skill.  
2. **Skill Activation:** Player checks skill availability (cooldown, stamina).  
3. **Targeting:** Player has a target Enemy.  
4. **Damage Calculation:** CombatEngine calls attacker.CalculateDamage() + skill.Damage  
5. **Damage Application:** CombatEngine calls defender.TakeDamage(damage).  
6. **Event Handling:**  
   * CombatEngine raises EntityDamagedEvent.  
   * UIRenderers (specifically WorldRenderer) listens for the event and updates the UI (e.g., health bars).

## **6. Data Flow**

### **6.1. Game Data Loading**

1. The Game class in the Engine layer initializes the DAL.  
2. The DAL loads game data (e.g., item definitions, skill trees) from JSON files.  
3. The loaded data is used to initialize game objects in the Core layer (e.g., creating Item instances, defining skills).

## **7. Technology Stack**

* Game Engine: home grown  
* ASCII Rendering: Custom code
* Data Storage: JSON files (for MVP), with potential for SQLite or other database solutions in the future.  
* Programming Language: C#

## **8. Development Considerations**

* **Modularity:** The layered architecture and use of interfaces promote modularity, making it easier to add new features and modify existing ones.  
* **Extensibility:** The design supports a wide variety of game mechanics and content, allowing for future expansion of the game.  
* **Testability:** The separation of concerns makes it easier to write unit tests for individual components.  
* **Performance:** For the MVP, performance is less critical, but the architecture allows for optimizations in the future if needed.