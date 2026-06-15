# Cucumbro

<p align="center">
  <strong>English</strong> | <a href="README_ru.md">Русский</a>
</p>

<p align="center">
  <a href="docs/%D0%A0%D1%83%D0%BA%D0%BE%D0%B2%D0%BE%D0%B4%D1%81%D1%82%D0%B2%D0%BE%20%D0%BF%D0%BE%D0%BB%D1%8C%D0%B7%D0%BE%D0%B2%D0%B0%D1%82%D0%B5%D0%BB%D1%8F.docx">User Documentation</a>
  ·
  <a href="docs/%D0%A0%D1%83%D0%BA%D0%BE%D0%B2%D0%BE%D0%B4%D1%81%D1%82%D0%B2%D0%BE%20%D0%BF%D1%80%D0%BE%D0%B3%D1%80%D0%B0%D0%BC%D0%BC%D0%B8%D1%81%D1%82%D0%B0.docx">Developer Documentation</a>
</p>

<p align="center">
  <a href="https://ogurechnayateam.github.io/cucumbro/">
    <img src="https://avatars.githubusercontent.com/u/271751028?s=200&v=4" alt="Cucumbro icon" width="96" height="96">
  </a>
</p>

<h2 align="center">
  <a href="https://ogurechnayateam.github.io/cucumbro/">Play Cucumbro on GitHub Pages</a>
</h2>

Cucumbro is a Unity 2D action game prototype set in small procedurally generated dungeons. The player explores rooms, fights vegetable-themed enemies, collects pickups, and can start runs with different weapon classes.

## Features

- Procedural dungeon and room generation.
- Player movement with Unity Input System.
- Multiple player/weapon scripts, including katana, gun, shield, and projectile gameplay.
- Enemy spawning, activation, and basic combat behavior.
- Pickups such as food, batteries, and shields.
- Several Unity scenes for testing and development.

## Project Info

- Engine: Unity 6000.4.0f1
- Render pipeline: Universal Render Pipeline 2D
- Main assets and scripts: `Assets/`
- Project settings: `ProjectSettings/`
- Package manifest: `Packages/manifest.json`

## Getting Started

1. Open the project in Unity 6000.4.0f1 or a compatible Unity 6 version.
2. Let Unity restore packages from `Packages/manifest.json`.
3. Open a scene `Assets/Scenes/SampleScene.unity`.
4. Press Play to test the current gameplay prototype.

## Web Build and GitHub Pages

This repository includes GitHub Actions workflows for WebGL releases and GitHub Pages deployment.

1. Add Unity activation secrets to the GitHub repository: `UNITY_LICENSE`, `UNITY_EMAIL`, and `UNITY_PASSWORD`.
2. In repository settings, set GitHub Pages source to GitHub Actions.
3. Push a version tag, for example `v0.1.0`.
4. The `Build WebGL Release` workflow builds the Unity WebGL player and attaches `cucumbro-webgl.zip` to the GitHub Release.
5. The `Deploy Pages From Latest Release` workflow downloads `cucumbro-webgl.zip` from the latest release and publishes it to GitHub Pages.

## Repository Structure

- `Assets/Scripts/` - gameplay scripts for player, enemies, weapons, UI, and level flow.
- `Assets/_Scripts/` - procedural dungeon generation scripts and ScriptableObject data.
- `Assets/Prefabs/` - player, enemy, weapon, item, and UI prefabs.
- `Assets/Tiles/` - tile assets and palettes for dungeon floors and walls.
- `ProjectSettings/` - Unity project configuration.
