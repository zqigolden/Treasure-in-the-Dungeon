# Treasure in the Dungeon

Author: Qi Zhu, QMUL student ID:210238697

This game is developed for the module *ECS7016P Interactive Agents and Procedural Generation*.

Developed in Unity 2020.3.26f1

## Introduction

In this game, players will adventure in randomly generated dungeons, traverse different terrains, escape from enemies, and finally collect treasures. It is hard, Try it!

## PCG-Based Dungeon Generation Design

First of all, the dungeon is generated using the Wave Functuion Colleps(WFC) method from the external package [unity-wave-function-collapse](https://github.com/selfsame/unity-wave-function-collapse). The tiles used for the input of WFC can be seen at Unity Editor. After generated, the structure of the dungeon is stored in `WorldControl.map` at `Generator/WorldControl.cs`.

Then the rooms and connections of dungeon are generated with Stochastic Agent method implemented at `Agent/StochasticAgent.cs`. It started at the bottom-left room, then randomly generated paths to other rooms and labeling each room at the same time. The rooms are stored in `WorldControl.roomMap` at `Generator/WorldControl.cs`. Also the connections between rooms are stored in `WorldControl.gateLoc` at `Generator/WorldControl.cs` for gates generation.

Next, with the PCG grammar generator implemented at `Generator/Grammer.cs`, the events for each of the rooms are decided. The events generate player, enemies, keys, gates and the treasure with the method `Grammer.DeployObjects`. Now, the Dungeon is ready to be played.

## Interactive agent Design

The Interactive agent for enemies is implemented at `Agent/EnemyControl.cs`. The agent applies Decision tree algorithm to decide the next action. The decision tree is built at `EnemyControl.CreateBehaviourTree` with the external package [NPBehave](https://github.com/meniku/NPBehave). For the path finding, I used the A* algorithm implemented at `EnemyControl.AStarSearch`. Also, the interactions with player and dungeon are added so that the agent can hear and see objects.

When an enemy is far from the player, it will patrol the dungeon to random rooms connected to the current room. If it hears the footsteps of the player(`Agent/EnemyControl.cs:L21`), it will get to the room the sound made to check what happened. If it sees the player(`EnemyControl.SeePlayer`), it will chase the player until some walls block the sight.

## Operations

1. Click the **WaveFunctuionColleps** button to generate the game map. It will take a few seconds to generate the map.

2. Click the **StochasticAgent** button to connect the rooms.

3. Click the **PCG Grammer** button to generate player, keys, gates, enemies and treasures.

4. Player can move around the dungeon by using the arrow keys. 

## User Interface

1. Keys: It shows the number of keys the player has. Keys can be used to open gates.

2. Footstep Sound: Terrains can make a footstep sound.
For example, the sound of a grass terrain will be more quite than water terrain when the player moves onto it. Enemies will notice the sound and be attracted to the room of player.

3. Speed: The speed of the player. It depends on the terrain player walked on.

## Game element
- **Red circle**: Player
- **Orange hexagon**: Keys
- **Purple rounded square**: Gates
- **Yellow circle**: Enemies
- **Blue diamond**: Treasure

## Terrains
- **Green**: Grass, make the least noise but also slow the player a little.
- **Blue**: Water, make the most noise and slow the player.
- **Dark brown**: Road, make a little noise but run very fast.
- **Light brown**: Sand, make a little noise with normal speed walking on it.

## External resources
- [unity-wave-function-collapse](https://github.com/selfsame/unity-wave-function-collapse)
- [NPBehave](https://github.com/meniku/NPBehave)
- [Input System](https://docs.unity3d.com/Packages/com.unity.inputsystem@1.3/manual/index.html)