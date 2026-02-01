// Sky Jumper - Mobile Game
// A fun platformer game optimized for mobile devices

class Game {
    constructor() {
        this.canvas = document.getElementById('gameCanvas');
        this.ctx = this.canvas.getContext('2d');

        // Screen elements
        this.startScreen = document.getElementById('start-screen');
        this.gameOverScreen = document.getElementById('game-over-screen');
        this.pauseScreen = document.getElementById('pause-screen');
        this.hudScore = document.getElementById('score');
        this.hudLives = document.getElementById('lives');
        this.highScoreDisplay = document.getElementById('high-score-display');
        this.finalScoreDisplay = document.getElementById('final-score');
        this.bestScoreDisplay = document.getElementById('best-score');

        // Game state
        this.gameState = 'start'; // start, playing, paused, gameover
        this.score = 0;
        this.lives = 3;
        this.highScore = parseInt(localStorage.getItem('skyJumperHighScore')) || 0;
        this.difficulty = 1;

        // Player
        this.player = {
            x: 0,
            y: 0,
            width: 40,
            height: 40,
            velocityY: 0,
            velocityX: 0,
            isJumping: false,
            color: '#667eea'
        };

        // Game objects
        this.platforms = [];
        this.stars = [];
        this.particles = [];
        this.backgroundStars = [];

        // Physics
        this.gravity = 0.6;
        this.jumpForce = -15;
        this.platformSpeed = 3;

        // Initialize
        this.resize();
        this.createBackgroundStars();
        this.updateHighScoreDisplay();
        this.bindEvents();
        this.gameLoop();
    }

    resize() {
        const container = document.getElementById('game-container');
        this.canvas.width = container.clientWidth;
        this.canvas.height = container.clientHeight;

        // Reset player position on resize
        this.player.x = this.canvas.width / 2 - this.player.width / 2;
        this.player.y = this.canvas.height - 150;
    }

    createBackgroundStars() {
        this.backgroundStars = [];
        for (let i = 0; i < 100; i++) {
            this.backgroundStars.push({
                x: Math.random() * this.canvas.width,
                y: Math.random() * this.canvas.height,
                size: Math.random() * 2 + 1,
                opacity: Math.random(),
                twinkleSpeed: Math.random() * 0.02 + 0.01
            });
        }
    }

    bindEvents() {
        // Touch events
        this.canvas.addEventListener('touchstart', (e) => this.handleInput(e), { passive: false });
        this.canvas.addEventListener('touchmove', (e) => this.handleMove(e), { passive: false });
        this.canvas.addEventListener('touchend', (e) => this.handleInputEnd(e), { passive: false });

        // Mouse events for testing on desktop
        this.canvas.addEventListener('mousedown', (e) => this.handleInput(e));
        this.canvas.addEventListener('mousemove', (e) => this.handleMove(e));
        this.canvas.addEventListener('mouseup', (e) => this.handleInputEnd(e));

        // Keyboard events
        document.addEventListener('keydown', (e) => {
            if (e.code === 'Space' && this.gameState === 'playing') {
                this.jump();
            }
        });

        // Button events
        document.getElementById('start-btn').addEventListener('click', () => this.startGame());
        document.getElementById('restart-btn').addEventListener('click', () => this.startGame());
        document.getElementById('pause-btn').addEventListener('click', () => this.togglePause());
        document.getElementById('resume-btn').addEventListener('click', () => this.togglePause());

        // Resize
        window.addEventListener('resize', () => this.resize());
    }

    handleInput(e) {
        e.preventDefault();
        if (this.gameState !== 'playing') return;

        const touch = e.touches ? e.touches[0] : e;
        const rect = this.canvas.getBoundingClientRect();
        const x = touch.clientX - rect.left;

        // Store touch position for movement
        this.touchX = x;
        this.isTouching = true;

        // Jump on tap
        this.jump();
    }

    handleMove(e) {
        e.preventDefault();
        if (this.gameState !== 'playing' || !this.isTouching) return;

        const touch = e.touches ? e.touches[0] : e;
        const rect = this.canvas.getBoundingClientRect();
        this.touchX = touch.clientX - rect.left;
    }

    handleInputEnd(e) {
        this.isTouching = false;
    }

    jump() {
        if (!this.player.isJumping || this.player.doubleJumpAvailable) {
            if (this.player.isJumping) {
                this.player.doubleJumpAvailable = false;
                this.player.velocityY = this.jumpForce * 0.8;
                this.createJumpParticles();
            } else {
                this.player.velocityY = this.jumpForce;
                this.player.isJumping = true;
                this.player.doubleJumpAvailable = true;
                this.createJumpParticles();
            }
        }
    }

    createJumpParticles() {
        for (let i = 0; i < 10; i++) {
            this.particles.push({
                x: this.player.x + this.player.width / 2,
                y: this.player.y + this.player.height,
                velocityX: (Math.random() - 0.5) * 6,
                velocityY: Math.random() * 3 + 1,
                size: Math.random() * 6 + 2,
                color: `hsl(${250 + Math.random() * 30}, 70%, 60%)`,
                life: 1
            });
        }
    }

    createStarParticles(x, y) {
        for (let i = 0; i < 15; i++) {
            this.particles.push({
                x: x,
                y: y,
                velocityX: (Math.random() - 0.5) * 8,
                velocityY: (Math.random() - 0.5) * 8,
                size: Math.random() * 5 + 2,
                color: '#ffd700',
                life: 1
            });
        }
    }

    startGame() {
        this.gameState = 'playing';
        this.score = 0;
        this.lives = 3;
        this.difficulty = 1;
        this.platformSpeed = 3;

        // Reset player
        this.player.x = this.canvas.width / 2 - this.player.width / 2;
        this.player.y = this.canvas.height - 150;
        this.player.velocityY = 0;
        this.player.isJumping = false;

        // Clear and create platforms
        this.platforms = [];
        this.stars = [];
        this.particles = [];

        // Create initial platforms
        this.createInitialPlatforms();

        // Hide screens
        this.startScreen.classList.add('hidden');
        this.gameOverScreen.classList.add('hidden');
        this.pauseScreen.classList.add('hidden');

        this.updateHUD();
    }

    createInitialPlatforms() {
        // Ground platform
        this.platforms.push({
            x: 0,
            y: this.canvas.height - 50,
            width: this.canvas.width,
            height: 50,
            isGround: true
        });

        // Initial platforms
        for (let i = 0; i < 5; i++) {
            this.spawnPlatform(this.canvas.height - 150 - (i * 150));
        }
    }

    spawnPlatform(y = -50) {
        const minWidth = 80 - this.difficulty * 5;
        const maxWidth = 150 - this.difficulty * 5;
        const width = Math.max(60, Math.random() * (maxWidth - minWidth) + minWidth);

        const platform = {
            x: Math.random() * (this.canvas.width - width),
            y: y,
            width: width,
            height: 15,
            color: `hsl(${250 + Math.random() * 30}, 60%, 50%)`
        };

        this.platforms.push(platform);

        // Spawn star on some platforms
        if (Math.random() > 0.5) {
            this.stars.push({
                x: platform.x + platform.width / 2,
                y: platform.y - 25,
                size: 15,
                collected: false,
                rotation: 0
            });
        }
    }

    togglePause() {
        if (this.gameState === 'playing') {
            this.gameState = 'paused';
            this.pauseScreen.classList.remove('hidden');
        } else if (this.gameState === 'paused') {
            this.gameState = 'playing';
            this.pauseScreen.classList.add('hidden');
        }
    }

    update() {
        if (this.gameState !== 'playing') return;

        // Update difficulty
        this.difficulty = 1 + Math.floor(this.score / 500) * 0.2;
        this.platformSpeed = 3 + this.difficulty * 0.5;

        // Horizontal movement based on touch
        if (this.isTouching) {
            const targetX = this.touchX - this.player.width / 2;
            this.player.velocityX = (targetX - this.player.x) * 0.15;
        } else {
            this.player.velocityX *= 0.9;
        }

        // Apply physics
        this.player.velocityY += this.gravity;
        this.player.x += this.player.velocityX;
        this.player.y += this.player.velocityY;

        // Boundary check
        if (this.player.x < 0) this.player.x = 0;
        if (this.player.x + this.player.width > this.canvas.width) {
            this.player.x = this.canvas.width - this.player.width;
        }

        // Move platforms down (scroll effect)
        if (this.player.y < this.canvas.height / 2) {
            const diff = this.canvas.height / 2 - this.player.y;
            this.player.y = this.canvas.height / 2;

            this.platforms.forEach(p => {
                if (!p.isGround) p.y += diff;
            });

            this.stars.forEach(s => {
                s.y += diff;
            });

            this.score += Math.floor(diff);
        }

        // Platform collision
        this.platforms.forEach(platform => {
            if (this.checkCollision(this.player, platform) && this.player.velocityY > 0) {
                if (this.player.y + this.player.height - this.player.velocityY <= platform.y + 5) {
                    this.player.y = platform.y - this.player.height;
                    this.player.velocityY = 0;
                    this.player.isJumping = false;
                    this.player.doubleJumpAvailable = false;
                }
            }
        });

        // Star collection
        this.stars.forEach(star => {
            if (!star.collected) {
                const dx = (this.player.x + this.player.width / 2) - star.x;
                const dy = (this.player.y + this.player.height / 2) - star.y;
                const distance = Math.sqrt(dx * dx + dy * dy);

                if (distance < 30) {
                    star.collected = true;
                    this.score += 100;
                    this.createStarParticles(star.x, star.y);
                }
            }
            star.rotation += 0.05;
        });

        // Remove off-screen platforms and spawn new ones
        this.platforms = this.platforms.filter(p => p.y < this.canvas.height + 50);
        this.stars = this.stars.filter(s => s.y < this.canvas.height + 50);

        // Spawn new platforms
        const topPlatform = this.platforms.reduce((min, p) => p.y < min ? p.y : min, this.canvas.height);
        if (topPlatform > 0) {
            this.spawnPlatform(topPlatform - (100 + Math.random() * 50));
        }

        // Check if player falls
        if (this.player.y > this.canvas.height) {
            this.loseLife();
        }

        // Update particles
        this.particles.forEach(p => {
            p.x += p.velocityX;
            p.y += p.velocityY;
            p.life -= 0.02;
            p.size *= 0.98;
        });
        this.particles = this.particles.filter(p => p.life > 0);

        // Update background stars
        this.backgroundStars.forEach(star => {
            star.opacity += star.twinkleSpeed;
            if (star.opacity > 1 || star.opacity < 0.3) {
                star.twinkleSpeed *= -1;
            }
        });

        this.updateHUD();
    }

    checkCollision(a, b) {
        return a.x < b.x + b.width &&
               a.x + a.width > b.x &&
               a.y < b.y + b.height &&
               a.y + a.height > b.y;
    }

    loseLife() {
        this.lives--;

        if (this.lives <= 0) {
            this.gameOver();
        } else {
            // Reset player position
            this.player.y = this.canvas.height / 2;
            this.player.velocityY = 0;

            // Flash effect
            this.player.color = '#ff4444';
            setTimeout(() => {
                this.player.color = '#667eea';
            }, 200);
        }

        this.updateHUD();
    }

    gameOver() {
        this.gameState = 'gameover';

        // Update high score
        if (this.score > this.highScore) {
            this.highScore = this.score;
            localStorage.setItem('skyJumperHighScore', this.highScore);
        }

        this.finalScoreDisplay.textContent = this.score;
        this.bestScoreDisplay.textContent = this.highScore;
        this.gameOverScreen.classList.remove('hidden');
        this.updateHighScoreDisplay();
    }

    updateHUD() {
        this.hudScore.textContent = `Score: ${this.score}`;
        this.hudLives.textContent = '❤️'.repeat(Math.max(0, this.lives));
    }

    updateHighScoreDisplay() {
        this.highScoreDisplay.textContent = this.highScore;
    }

    draw() {
        // Clear canvas
        this.ctx.fillStyle = '#0f0c29';
        this.ctx.fillRect(0, 0, this.canvas.width, this.canvas.height);

        // Draw gradient background
        const gradient = this.ctx.createLinearGradient(0, 0, 0, this.canvas.height);
        gradient.addColorStop(0, '#0f0c29');
        gradient.addColorStop(0.5, '#302b63');
        gradient.addColorStop(1, '#24243e');
        this.ctx.fillStyle = gradient;
        this.ctx.fillRect(0, 0, this.canvas.width, this.canvas.height);

        // Draw background stars
        this.backgroundStars.forEach(star => {
            this.ctx.beginPath();
            this.ctx.arc(star.x, star.y, star.size, 0, Math.PI * 2);
            this.ctx.fillStyle = `rgba(255, 255, 255, ${star.opacity})`;
            this.ctx.fill();
        });

        // Draw platforms
        this.platforms.forEach(platform => {
            if (platform.isGround) {
                // Ground platform gradient
                const groundGradient = this.ctx.createLinearGradient(0, platform.y, 0, platform.y + platform.height);
                groundGradient.addColorStop(0, '#4a4a7a');
                groundGradient.addColorStop(1, '#2a2a4a');
                this.ctx.fillStyle = groundGradient;
            } else {
                this.ctx.fillStyle = platform.color || '#667eea';
            }

            // Rounded rectangle for platforms
            this.roundRect(platform.x, platform.y, platform.width, platform.height, 8);

            // Platform glow
            if (!platform.isGround) {
                this.ctx.shadowColor = platform.color || '#667eea';
                this.ctx.shadowBlur = 10;
                this.ctx.fill();
                this.ctx.shadowBlur = 0;
            } else {
                this.ctx.fill();
            }
        });

        // Draw stars
        this.stars.forEach(star => {
            if (!star.collected) {
                this.ctx.save();
                this.ctx.translate(star.x, star.y);
                this.ctx.rotate(star.rotation);
                this.drawStar(0, 0, star.size);
                this.ctx.restore();
            }
        });

        // Draw particles
        this.particles.forEach(p => {
            this.ctx.beginPath();
            this.ctx.arc(p.x, p.y, p.size, 0, Math.PI * 2);
            this.ctx.fillStyle = p.color;
            this.ctx.globalAlpha = p.life;
            this.ctx.fill();
            this.ctx.globalAlpha = 1;
        });

        // Draw player
        this.drawPlayer();
    }

    drawPlayer() {
        const p = this.player;
        const centerX = p.x + p.width / 2;
        const centerY = p.y + p.height / 2;

        // Player glow
        this.ctx.shadowColor = p.color;
        this.ctx.shadowBlur = 20;

        // Main body (rocket shape)
        this.ctx.fillStyle = p.color;
        this.ctx.beginPath();
        this.ctx.moveTo(centerX, p.y);
        this.ctx.lineTo(p.x + p.width, p.y + p.height * 0.7);
        this.ctx.lineTo(p.x + p.width * 0.7, p.y + p.height);
        this.ctx.lineTo(p.x + p.width * 0.3, p.y + p.height);
        this.ctx.lineTo(p.x, p.y + p.height * 0.7);
        this.ctx.closePath();
        this.ctx.fill();

        // Window
        this.ctx.shadowBlur = 0;
        this.ctx.fillStyle = '#a8d8ff';
        this.ctx.beginPath();
        this.ctx.arc(centerX, p.y + p.height * 0.4, 8, 0, Math.PI * 2);
        this.ctx.fill();

        // Flame when jumping/falling
        if (this.player.velocityY < 0 || this.player.isJumping) {
            this.ctx.fillStyle = '#ff6b35';
            this.ctx.beginPath();
            this.ctx.moveTo(p.x + p.width * 0.3, p.y + p.height);
            this.ctx.lineTo(centerX, p.y + p.height + 15 + Math.random() * 10);
            this.ctx.lineTo(p.x + p.width * 0.7, p.y + p.height);
            this.ctx.closePath();
            this.ctx.fill();

            this.ctx.fillStyle = '#ffd700';
            this.ctx.beginPath();
            this.ctx.moveTo(p.x + p.width * 0.4, p.y + p.height);
            this.ctx.lineTo(centerX, p.y + p.height + 8 + Math.random() * 5);
            this.ctx.lineTo(p.x + p.width * 0.6, p.y + p.height);
            this.ctx.closePath();
            this.ctx.fill();
        }
    }

    drawStar(x, y, size) {
        this.ctx.fillStyle = '#ffd700';
        this.ctx.shadowColor = '#ffd700';
        this.ctx.shadowBlur = 15;
        this.ctx.beginPath();

        for (let i = 0; i < 5; i++) {
            const angle = (i * 4 * Math.PI) / 5 - Math.PI / 2;
            const px = x + Math.cos(angle) * size;
            const py = y + Math.sin(angle) * size;

            if (i === 0) {
                this.ctx.moveTo(px, py);
            } else {
                this.ctx.lineTo(px, py);
            }
        }

        this.ctx.closePath();
        this.ctx.fill();
        this.ctx.shadowBlur = 0;
    }

    roundRect(x, y, width, height, radius) {
        this.ctx.beginPath();
        this.ctx.moveTo(x + radius, y);
        this.ctx.lineTo(x + width - radius, y);
        this.ctx.quadraticCurveTo(x + width, y, x + width, y + radius);
        this.ctx.lineTo(x + width, y + height - radius);
        this.ctx.quadraticCurveTo(x + width, y + height, x + width - radius, y + height);
        this.ctx.lineTo(x + radius, y + height);
        this.ctx.quadraticCurveTo(x, y + height, x, y + height - radius);
        this.ctx.lineTo(x, y + radius);
        this.ctx.quadraticCurveTo(x, y, x + radius, y);
        this.ctx.closePath();
    }

    gameLoop() {
        this.update();
        this.draw();
        requestAnimationFrame(() => this.gameLoop());
    }
}

// Initialize game when DOM is loaded
document.addEventListener('DOMContentLoaded', () => {
    new Game();
});
