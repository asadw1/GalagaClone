# Galaga Clone (C# WinForms)

A faithful recreation of the classic arcade game **Galaga**, built from scratch using raw **C#** and **WinForms** on **.NET 10**. No game engines—just pure OOP, graphics, input handling, and game loops.

## Features (Milestone Plan)

| Stage | What you'll get | Key Systems |
|-------|-----------------|-------------|
| **1** | Player ship movement | Game loop, input, rendering |
| **2** | Player shooting | Bullets, pooling, input cooldowns |
| **3** | Enemy grid + sweep | Enemy waves, formation movement |
| **4** | Collisions + scoring | AABB collision, score tracking |
| **5** | Enemy shooting + lives | Enemy AI, player health |
| **6** | Galaga dive attacks | Pathfinding, tractor beam |
| **7** | Polish (sounds, sprites) | Audio, pixel art sprites |

## Tech Stack

```
.NET 10 SDK
├── WinForms (raw Graphics2D)
├── System.Drawing (rectangles → sprites)
├── System.Windows.Forms.Timer (~60 FPS)
└── VS Code + C# Dev Kit
```

**Current status:** Player movement ✅. Next: bullets.

## Quick Start

1. **Prerequisites**
   ```bash
   dotnet --version  # Should show 10.x
   ```

2. **Clone and Run**
   ```bash
   git clone <this-repo>
   cd GalagaClone
   dotnet run
   ```

## Controls

- **← →** or **A/D**: Move ship
- **Space**: Shoot (coming soon)

## Architecture

```
MainForm → Timer (16ms) → Game.Update(delta) → Game.Draw(g)
                           ↑
                       Input (Keys)
```

**Core Classes to Add Next:**

- `Bullet.cs` (player/enemy shots)
- `Enemy.cs` (grid, dive patterns)
- `CollisionDetector.cs` (AABB checks)
- `Score.cs` (lives, high score)

## Development Milestones

### ✅ Milestone 1: Core Loop
- 800x600 window, 60 FPS
- Cyan player rectangle moves with arrow keys
- Black starfield background

### ⏳ Milestone 2: Bullets (Next)

```csharp
// Add to Game.cs
private readonly List<Bullet> _playerBullets = new();
private float _shootCooldown;

// In Update()
if (keys.Contains(Keys.Space) && _shootCooldown <= 0)
{
    _playerBullets.Add(new Bullet(player.Bounds.CenterX, player.Bounds.Bottom));
    _shootCooldown = 0.15f;
}
```

### Milestone 3: Enemy Formation
- 5x8 grid of colored enemies
- Horizontal sweep left→right→left
- Vertical step-down every pass

## Key Learnings

- Delta time for smooth 30-144 FPS
- Double buffering eliminates flicker
- Object pooling for bullets (performance)
- AABB collision (`rect.IntersectsWith`)
- State machines for enemy behaviors

## Common Commands

```bash
# Test current build
dotnet run

# Add bullets (when code lands)
dotnet build && dotnet run

# VS Code setup
code .  # Install C# Dev Kit extension
```

## Future Enhancements

- [ ] Pixel-perfect sprites (PNG → Bitmap)
- [ ] Sound effects (System.Media.SoundPlayer)
- [ ] Multiple waves + boss Galaga
- [ ] High score persistence (JSON)
- [ ] Windowed/fullscreen toggle

---

**Stack:** C# → Spring Boot → C# (full circle)  
**License:** MIT