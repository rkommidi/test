// Space Runner - Phaser 3 Mobile Game

const config = {
    type: Phaser.AUTO,
    width: 400,
    height: 600,
    parent: document.body,
    backgroundColor: '#1a1a2e',
    physics: {
        default: 'arcade',
        arcade: {
            gravity: { y: 800 },
            debug: false
        }
    },
    scale: {
        mode: Phaser.Scale.FIT,
        autoCenter: Phaser.Scale.CENTER_BOTH
    },
    scene: [BootScene, MenuScene, GameScene, GameOverScene]
};

// Boot Scene - Load assets
class BootScene extends Phaser.Scene {
    constructor() {
        super('BootScene');
    }

    create() {
        // Create game textures programmatically
        this.createTextures();
        this.scene.start('MenuScene');
    }

    createTextures() {
        // Player ship
        const playerGraphics = this.make.graphics({ x: 0, y: 0, add: false });
        playerGraphics.fillStyle(0x00ff88);
        playerGraphics.fillTriangle(20, 0, 0, 40, 40, 40);
        playerGraphics.fillStyle(0x00cc66);
        playerGraphics.fillRect(15, 40, 10, 8);
        playerGraphics.generateTexture('player', 40, 48);

        // Enemy
        const enemyGraphics = this.make.graphics({ x: 0, y: 0, add: false });
        enemyGraphics.fillStyle(0xff4444);
        enemyGraphics.fillTriangle(20, 40, 0, 0, 40, 0);
        enemyGraphics.generateTexture('enemy', 40, 40);

        // Bullet
        const bulletGraphics = this.make.graphics({ x: 0, y: 0, add: false });
        bulletGraphics.fillStyle(0xffff00);
        bulletGraphics.fillRect(0, 0, 6, 15);
        bulletGraphics.generateTexture('bullet', 6, 15);

        // Star
        const starGraphics = this.make.graphics({ x: 0, y: 0, add: false });
        starGraphics.fillStyle(0xffd700);
        starGraphics.fillStar(12, 12, 5, 12, 6);
        starGraphics.generateTexture('star', 24, 24);

        // Particle
        const particleGraphics = this.make.graphics({ x: 0, y: 0, add: false });
        particleGraphics.fillStyle(0xffffff);
        particleGraphics.fillCircle(4, 4, 4);
        particleGraphics.generateTexture('particle', 8, 8);
    }
}

// Menu Scene
class MenuScene extends Phaser.Scene {
    constructor() {
        super('MenuScene');
    }

    create() {
        const { width, height } = this.scale;

        // Title
        this.add.text(width / 2, height / 3, 'SPACE\nRUNNER', {
            fontSize: '48px',
            fontFamily: 'Arial',
            color: '#00ff88',
            align: 'center'
        }).setOrigin(0.5);

        // Instructions
        this.add.text(width / 2, height / 2, 'Tap left/right to move\nTap center to shoot', {
            fontSize: '18px',
            fontFamily: 'Arial',
            color: '#888888',
            align: 'center'
        }).setOrigin(0.5);

        // High score
        const highScore = localStorage.getItem('spaceRunnerHighScore') || 0;
        this.add.text(width / 2, height / 2 + 80, `High Score: ${highScore}`, {
            fontSize: '20px',
            fontFamily: 'Arial',
            color: '#ffd700'
        }).setOrigin(0.5);

        // Start button
        const startBtn = this.add.text(width / 2, height * 0.75, '[ TAP TO START ]', {
            fontSize: '24px',
            fontFamily: 'Arial',
            color: '#ffffff'
        }).setOrigin(0.5).setInteractive();

        this.tweens.add({
            targets: startBtn,
            alpha: 0.5,
            duration: 800,
            yoyo: true,
            repeat: -1
        });

        this.input.on('pointerdown', () => {
            this.scene.start('GameScene');
        });

        // Background stars
        for (let i = 0; i < 50; i++) {
            const star = this.add.circle(
                Phaser.Math.Between(0, width),
                Phaser.Math.Between(0, height),
                Phaser.Math.Between(1, 2),
                0xffffff,
                Phaser.Math.FloatBetween(0.3, 1)
            );
            this.tweens.add({
                targets: star,
                alpha: 0.2,
                duration: Phaser.Math.Between(500, 1500),
                yoyo: true,
                repeat: -1
            });
        }
    }
}

// Main Game Scene
class GameScene extends Phaser.Scene {
    constructor() {
        super('GameScene');
    }

    create() {
        const { width, height } = this.scale;

        this.score = 0;
        this.gameOver = false;

        // Background stars
        this.stars = this.add.group();
        for (let i = 0; i < 30; i++) {
            const star = this.add.circle(
                Phaser.Math.Between(0, width),
                Phaser.Math.Between(0, height),
                Phaser.Math.Between(1, 2),
                0xffffff,
                Phaser.Math.FloatBetween(0.3, 0.8)
            );
            this.stars.add(star);
        }

        // Player
        this.player = this.physics.add.sprite(width / 2, height - 80, 'player');
        this.player.setCollideWorldBounds(true);
        this.player.body.allowGravity = false;

        // Groups
        this.bullets = this.physics.add.group();
        this.enemies = this.physics.add.group();
        this.collectibles = this.physics.add.group();

        // Collisions
        this.physics.add.overlap(this.bullets, this.enemies, this.hitEnemy, null, this);
        this.physics.add.overlap(this.player, this.enemies, this.playerHit, null, this);
        this.physics.add.overlap(this.player, this.collectibles, this.collectStar, null, this);

        // Timers
        this.enemyTimer = this.time.addEvent({
            delay: 1500,
            callback: this.spawnEnemy,
            callbackScope: this,
            loop: true
        });

        this.starTimer = this.time.addEvent({
            delay: 3000,
            callback: this.spawnStar,
            callbackScope: this,
            loop: true
        });

        // Score text
        this.scoreText = this.add.text(16, 16, 'Score: 0', {
            fontSize: '20px',
            fontFamily: 'Arial',
            color: '#ffffff'
        });

        // Touch controls
        this.input.on('pointerdown', (pointer) => {
            if (this.gameOver) return;

            const third = width / 3;
            if (pointer.x < third) {
                this.moveLeft = true;
            } else if (pointer.x > third * 2) {
                this.moveRight = true;
            } else {
                this.shoot();
            }
        });

        this.input.on('pointerup', () => {
            this.moveLeft = false;
            this.moveRight = false;
        });

        // Keyboard controls
        this.cursors = this.input.keyboard.createCursorKeys();
        this.spaceKey = this.input.keyboard.addKey(Phaser.Input.Keyboard.KeyCodes.SPACE);

        // Particle emitter for explosions
        this.explosionEmitter = this.add.particles(0, 0, 'particle', {
            speed: { min: 50, max: 150 },
            scale: { start: 1, end: 0 },
            lifespan: 400,
            blendMode: 'ADD',
            emitting: false
        });
    }

    update() {
        if (this.gameOver) return;

        const { width, height } = this.scale;

        // Player movement
        const speed = 300;
        this.player.setVelocityX(0);

        if (this.moveLeft || this.cursors.left.isDown) {
            this.player.setVelocityX(-speed);
        } else if (this.moveRight || this.cursors.right.isDown) {
            this.player.setVelocityX(speed);
        }

        if (Phaser.Input.Keyboard.JustDown(this.spaceKey)) {
            this.shoot();
        }

        // Move background stars
        this.stars.children.iterate((star) => {
            star.y += 1;
            if (star.y > height) {
                star.y = 0;
                star.x = Phaser.Math.Between(0, width);
            }
        });

        // Clean up off-screen objects
        this.bullets.children.iterate((bullet) => {
            if (bullet && bullet.y < -20) bullet.destroy();
        });

        this.enemies.children.iterate((enemy) => {
            if (enemy && enemy.y > height + 50) {
                enemy.destroy();
                this.addScore(5); // Points for dodging
            }
        });

        this.collectibles.children.iterate((star) => {
            if (star && star.y > height + 50) star.destroy();
        });

        // Increase difficulty over time
        if (this.score > 0 && this.score % 500 === 0) {
            if (this.enemyTimer.delay > 500) {
                this.enemyTimer.delay -= 50;
            }
        }
    }

    shoot() {
        const bullet = this.bullets.create(this.player.x, this.player.y - 30, 'bullet');
        bullet.body.allowGravity = false;
        bullet.setVelocityY(-400);
    }

    spawnEnemy() {
        if (this.gameOver) return;

        const x = Phaser.Math.Between(40, this.scale.width - 40);
        const enemy = this.enemies.create(x, -40, 'enemy');
        enemy.body.allowGravity = false;
        enemy.setVelocityY(Phaser.Math.Between(100, 200));

        // Some enemies move sideways
        if (Math.random() > 0.5) {
            enemy.setVelocityX(Phaser.Math.Between(-50, 50));
        }
    }

    spawnStar() {
        if (this.gameOver) return;

        const x = Phaser.Math.Between(40, this.scale.width - 40);
        const star = this.collectibles.create(x, -30, 'star');
        star.body.allowGravity = false;
        star.setVelocityY(120);

        this.tweens.add({
            targets: star,
            angle: 360,
            duration: 2000,
            repeat: -1
        });
    }

    hitEnemy(bullet, enemy) {
        this.explosionEmitter.explode(10, enemy.x, enemy.y);
        bullet.destroy();
        enemy.destroy();
        this.addScore(25);
    }

    collectStar(player, star) {
        this.explosionEmitter.explode(8, star.x, star.y);
        star.destroy();
        this.addScore(100);
    }

    playerHit(player, enemy) {
        this.explosionEmitter.explode(20, player.x, player.y);
        enemy.destroy();
        this.endGame();
    }

    addScore(points) {
        this.score += points;
        this.scoreText.setText(`Score: ${this.score}`);
    }

    endGame() {
        this.gameOver = true;
        this.player.setVisible(false);
        this.physics.pause();

        // Save high score
        const highScore = localStorage.getItem('spaceRunnerHighScore') || 0;
        if (this.score > highScore) {
            localStorage.setItem('spaceRunnerHighScore', this.score);
        }

        this.time.delayedCall(1000, () => {
            this.scene.start('GameOverScene', { score: this.score });
        });
    }
}

// Game Over Scene
class GameOverScene extends Phaser.Scene {
    constructor() {
        super('GameOverScene');
    }

    init(data) {
        this.finalScore = data.score || 0;
    }

    create() {
        const { width, height } = this.scale;
        const highScore = localStorage.getItem('spaceRunnerHighScore') || 0;

        this.add.text(width / 2, height / 3, 'GAME OVER', {
            fontSize: '40px',
            fontFamily: 'Arial',
            color: '#ff4444'
        }).setOrigin(0.5);

        this.add.text(width / 2, height / 2 - 20, `Score: ${this.finalScore}`, {
            fontSize: '28px',
            fontFamily: 'Arial',
            color: '#ffffff'
        }).setOrigin(0.5);

        this.add.text(width / 2, height / 2 + 30, `Best: ${highScore}`, {
            fontSize: '22px',
            fontFamily: 'Arial',
            color: '#ffd700'
        }).setOrigin(0.5);

        const restartBtn = this.add.text(width / 2, height * 0.7, '[ TAP TO RESTART ]', {
            fontSize: '22px',
            fontFamily: 'Arial',
            color: '#00ff88'
        }).setOrigin(0.5).setInteractive();

        this.tweens.add({
            targets: restartBtn,
            alpha: 0.5,
            duration: 800,
            yoyo: true,
            repeat: -1
        });

        this.input.on('pointerdown', () => {
            this.scene.start('GameScene');
        });
    }
}

// Start the game
const game = new Phaser.Game(config);
