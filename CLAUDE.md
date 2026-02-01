# CLAUDE.md - AI Assistant Guide for Sky Jumper

This document provides guidance for AI assistants working on the Sky Jumper mobile game codebase.

## Project Overview

Sky Jumper is a browser-based platformer game optimized for mobile devices. Players control a rocket that jumps between platforms, collects stars, and aims for high scores. The game features double-jump mechanics, progressive difficulty, and local high score persistence.

**Tech Stack:** Vanilla JavaScript (ES6+), HTML5 Canvas API, CSS3 - zero external dependencies.

## File Structure

```
/
├── index.html     # Main HTML template with game container and UI overlays
├── style.css      # All styling, animations, and responsive design
├── game.js        # Complete game engine (~585 lines)
├── README.md      # User-facing documentation
└── CLAUDE.md      # This file - AI assistant guidelines
```

## Architecture

### Single-Class Game Engine (`game.js`)

The entire game is encapsulated in a single `Game` class with clear method organization:

| Category | Methods |
|----------|---------|
| **Initialization** | `constructor()`, `resize()`, `bindEvents()`, `createBackgroundStars()` |
| **Game Loop** | `gameLoop()`, `update()`, `draw()` |
| **Input Handling** | `handleInput()`, `handleMove()`, `handleInputEnd()`, `jump()` |
| **Game State** | `startGame()`, `togglePause()`, `gameOver()`, `loseLife()` |
| **Rendering** | `draw()`, `drawPlayer()`, `drawStar()`, `roundRect()` |
| **Utilities** | `checkCollision()`, `updateHUD()`, `spawnPlatform()` |

### Game Object Data Structures

```javascript
// Player object
this.player = {
    x, y,                    // Position
    width: 40, height: 40,   // Dimensions
    velocityY, velocityX,    // Movement
    isJumping, doubleJumpAvailable,
    color: '#667eea'
};

// Platform object
{ x, y, width, height, color, isGround }

// Star object
{ x, y, size, collected, rotation }

// Particle object
{ x, y, velocityX, velocityY, size, color, life }
```

### Game States

The game uses a simple state machine stored in `this.gameState`:
- `'start'` - Initial screen, waiting for player to begin
- `'playing'` - Active gameplay
- `'paused'` - Game paused via pause button
- `'gameover'` - Player lost all lives

## Key Code Conventions

### JavaScript Style

- **ES6 class syntax** for the main Game class
- **Arrow functions** for callbacks and event handlers
- **Descriptive method names** following `verbNoun` pattern (e.g., `createJumpParticles`, `drawPlayer`)
- **Inline object literals** for game entities (platforms, stars, particles)
- **No external dependencies** - pure vanilla JS

### Physics Constants

```javascript
this.gravity = 0.6;       // Downward acceleration per frame
this.jumpForce = -15;     // Initial upward velocity on jump
this.platformSpeed = 3;   // Base scrolling speed (increases with difficulty)
```

### Rendering Pattern

All drawing follows this sequence in `draw()`:
1. Clear canvas with solid color
2. Draw gradient background
3. Draw background stars (decorative)
4. Draw platforms with glow effects
5. Draw collectible stars
6. Draw particles
7. Draw player (rocket) last (on top)

### Event Handling

The game supports three input methods simultaneously:
- **Touch events** - Primary for mobile (`touchstart`, `touchmove`, `touchend`)
- **Mouse events** - Desktop fallback (`mousedown`, `mousemove`, `mouseup`)
- **Keyboard** - Spacebar for jumping

All touch/mouse handlers call `e.preventDefault()` to prevent scrolling.

### CSS Conventions

- **CSS variables not used** - colors defined inline
- **Flexbox** for centering screen overlays
- **Linear gradients** for backgrounds (purple/navy theme)
- **Keyframe animations** for UI pulsing effects
- **Media queries** for mobile breakpoints and landscape detection
- **Touch optimization**: `touch-action: manipulation`, `-webkit-tap-highlight-color: transparent`

### Color Scheme

```
Background: #0f0c29 → #302b63 → #24243e (gradient)
Player/UI accent: #667eea (purple)
Stars/Gold elements: #ffd700
Player window: #a8d8ff (light blue)
Rocket flame: #ff6b35 (orange), #ffd700 (gold core)
```

## Development Workflow

### Running the Game

Simply open `index.html` in a modern web browser. No build step required.

```bash
# Using Python's HTTP server
python -m http.server 8000
# Then open http://localhost:8000

# Or using Node.js
npx serve .
```

### Testing

No automated test framework is configured. Manual testing approach:
1. Test on mobile devices (touch input)
2. Test on desktop (mouse + keyboard)
3. Verify responsive behavior at different viewport sizes
4. Check high score persistence in localStorage

### Browser Compatibility

Requires:
- ES6 support (classes, arrow functions)
- HTML5 Canvas API
- localStorage API
- `requestAnimationFrame`

## Guidelines for AI Assistants

### When Making Changes

1. **Preserve the monolithic structure** - The single-class design is intentional for simplicity
2. **Maintain zero dependencies** - Don't add npm packages or external libraries
3. **Keep physics values balanced** - Changes to gravity, jumpForce, or platformSpeed affect gameplay feel
4. **Test touch AND mouse inputs** - Both must work for any input-related changes
5. **Preserve mobile optimization** - Don't remove viewport meta tags or touch CSS properties

### Common Modification Areas

| Task | Location |
|------|----------|
| Change player appearance | `drawPlayer()` method in `game.js:490-534` |
| Adjust difficulty scaling | `update()` method, lines ~266-267 |
| Modify platform behavior | `spawnPlatform()` and `createInitialPlatforms()` methods |
| Add new game objects | Follow the pattern: add to class properties, update in `update()`, render in `draw()` |
| Change UI styling | `style.css` - overlays use `.screen` class |
| Modify controls | `bindEvents()` and `handleInput()` methods |

### Potential Improvements (if requested)

- Add sound effects (Web Audio API)
- Add power-ups (follow star object pattern)
- Add different player skins (modify `drawPlayer()`)
- Add level system (extend game state)
- Add touch joystick (modify `handleMove()`)

### Things to Avoid

- Don't add build systems (webpack, vite) unless specifically requested
- Don't convert to TypeScript unless specifically requested
- Don't add external game engines (Phaser, PixiJS)
- Don't break the single-file simplicity without good reason
- Don't remove the double-jump mechanic (core gameplay feature)

## Important File Locations

| Purpose | File | Lines |
|---------|------|-------|
| Game initialization | `game.js` | 1-55 |
| Event binding | `game.js` | 80-106 |
| Physics/collision | `game.js` | 262-372 |
| Canvas rendering | `game.js` | 418-572 |
| Screen overlays HTML | `index.html` | 15-40 |
| Responsive styles | `style.css` | 159-193 |
| Button styles | `style.css` | 71-91 |

## Quick Reference

```javascript
// Start a new game programmatically
game.startGame();

// Check current game state
game.gameState; // 'start' | 'playing' | 'paused' | 'gameover'

// Access player position
game.player.x, game.player.y

// Get current score
game.score

// Get/set high score
game.highScore
localStorage.getItem('skyJumperHighScore')
```
