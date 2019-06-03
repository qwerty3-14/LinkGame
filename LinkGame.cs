using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using SpriteFontPlus; //Loading fonts in XNA/Monogame is a pain so I used this SpriteFontPlus tool
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace Connect
{
   
    public class LinkGame : Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch; //XNA uses spritebatch is used to draw sprites ingame
        MouseState mouse; //An object for our mouse
        public static Random random = new Random();
        public static SpriteFont font; //font using SpritefontPlus to load this more easily
       
        public static Vector2 screenSize = new Vector2(1000, 800);
        Vector2 mousePos;

        public static List<Player> gamePlayers = new List<Player>();
        public static List<Projectile> gameProjectiles = new List<Projectile>();
        public static LinkGame instance;
        public static List<Ring> gameRings = new List<Ring>();
        public static List<Particle> gameParticles = new List<Particle>();

        static bool paused = false;
        static bool sandBox = false;
        static bool MainMenu = true;
        public static bool playeLinkSound = false;
        static int SandboxCircleSize = 24;

        //textures that will be drawn, you can easily load Textur2D's from png files but I decided to make all my texture2D's algorithmicly
        Texture2D X;
        Texture2D[] CircleSizes = new Texture2D[100];
        Texture2D playerArrow;
        Texture2D linkTriangle;
        static Texture2D pixel;
        static Texture2D shot;

        Song song;
        public static SoundEffect[] sounds = new SoundEffect[3];

        public LinkGame()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            //adjust window size
            this.graphics.PreferredBackBufferWidth = (int)screenSize.X;
            this.graphics.PreferredBackBufferHeight = (int)screenSize.Y;
            Window.Title = ("Link");
        }
        public static Texture2D drawTriangle(int height, float widthMultiplier)
        {


            int width = (int)(height * widthMultiplier);
            Texture2D triangle = new Texture2D(instance.GraphicsDevice, width, height);
            var dataColors = new Color[width * height];
            int mid = height / 2;
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    int dist = Math.Abs(mid - y);
                    if (dist < ((width - x) / widthMultiplier) / 2)
                    {
                        dataColors[x + y * width] = Color.White;
                    }
                }
            }
            triangle.SetData(0, null, dataColors, 0, width * height);
            return triangle;
        }

        public static Texture2D drawRing(int radius) //creates a ring based on the radius givin
        {
            
            int diameter = (radius * 2) + 1;
            Texture2D circle = new Texture2D(instance.GraphicsDevice, diameter, diameter); // create Texture2D
            var dataColors = new Color[diameter * diameter]; //Color array

            for (int x = 0; x < diameter; x++)
            {
                for (int y = 0; y < diameter; y++)
                {
                    Point p = new Point(x - radius, y - radius); //make a point shifted in a way to make it the plane centered at 0,0 as origin
                    float distFromOrigin = p.ToVector2().Length(); 
                    if ((int)distFromOrigin == radius) 
                    {
                        dataColors[x + y * diameter] = Color.White;
                    }
                }
            }
            circle.SetData(0, null, dataColors, 0, diameter * diameter); //input our data to the Texture2D
            return circle;
        }
        public static Texture2D drawSquare(int length)//creates a square based on the length givin
        {
            
            Texture2D square = new Texture2D(instance.GraphicsDevice, length, length); //create texture2D
            var dataColors = new Color[length * length]; //Color array

            for (int x = 0; x < length; x++)
            {
                for (int y = 0; y < length; y++)
                {
                    
                        dataColors[x + y * length] = Color.White; // color every pixel since its alread square
                    
                }
            }
            square.SetData(0, null, dataColors, 0, length * length); //input our data into Texture2D
            return square;
        }
        public static void drawLine(SpriteBatch spritebatch, Vector2 start, Vector2 end) //draws a ling between 2 points
        {

            int distance = (int)(start - screenLoopAdjust(start, end)).Length();

            if (distance > 0)
            {
                float rotation = ToRotation(screenLoopAdjust(start, end) - start);

                
                spritebatch.Draw(pixel, start, null, Color.White, rotation, Vector2.Zero, new Vector2(distance, 1f), SpriteEffects.None, 0); //the line is a pixel with its x scale stretched by the needed distance
                if (screenLoopAdjust(start, end) != end)
                {
                    spritebatch.Draw(pixel, end, null, Color.White, rotation + (float)Math.PI, Vector2.Zero, new Vector2(distance, 1f), SpriteEffects.None, 0); // draw a line in the opposite direction if you need to draw around the screen loop
                }
            }

        }
        public static Vector2 PolarVector(float radius, float theta) // converts a rotation and amount into a x,y vector
        {
            return (new Vector2((float)Math.Cos(theta), (float)Math.Sin(theta)) * radius);
        }
        public static float ToRotation(Vector2 v)
        {
            return (float)Math.Atan2((double)v.Y, (double)v.X);
        }
        public static Vector2 LoopAroundCheck(Vector2 Position) // this is how thing loop around when offscreen
        {
            if (Position.X < 0)
            {
                Position.X += instance.Window.ClientBounds.Width;
            }
            if (Position.X > instance.Window.ClientBounds.Width)
            {
                Position.X -= instance.Window.ClientBounds.Width;
            }
            if (Position.Y < 0)
            {
                Position.Y += instance.Window.ClientBounds.Height;
            }
            if (Position.Y > instance.Window.ClientBounds.Height)
            {
                Position.Y -= instance.Window.ClientBounds.Height;
            }
            return Position;
        }
        public static Vector2 screenLoopAdjust(Vector2 myPosition, Vector2 targetPosition) // allows thins to know if a shorter distance can be attained by looping around the screen
        {
            float arenaWidth = instance.Window.ClientBounds.Width;
            if (myPosition.X - targetPosition.X > arenaWidth / 2)
            {
                targetPosition.X = arenaWidth + targetPosition.X;
            }
            else if (myPosition.X - targetPosition.X < -arenaWidth / 2)
            {
                targetPosition.X = -(arenaWidth - targetPosition.X);
            }
            float arenaHeight = instance.Window.ClientBounds.Height;
            if (myPosition.Y - targetPosition.Y > arenaHeight / 2)
            {
                targetPosition.Y = arenaHeight + targetPosition.Y;
            }
            else if (myPosition.Y - targetPosition.Y < -arenaHeight / 2)
            {
                targetPosition.Y = -(arenaHeight - targetPosition.Y);
            }
            return targetPosition;
        }
        protected override void Initialize()
        {
            base.Initialize();
        }
        
        public static void Reset() //this is what places everything when vs. mod starts
        {
            MainMenu = false;
            gamePlayers.Clear();
            gameParticles.Clear();
            gameProjectiles.Clear();
            gameRings.Clear();
            Vector2 playerPos = screenSize * .25f;
            new Player(playerPos);
            new Ring(playerPos + Vector2.UnitX * 100);
            new Ring(playerPos + Vector2.UnitY * 100);
            playerPos = screenSize * .75f;
            new Player(playerPos);
            new Ring(playerPos + Vector2.UnitX * -100);
            new Ring(playerPos + Vector2.UnitY * -100);
            gamePlayers[1].rotation += (float)Math.PI;
            for (int i = 0; i < 8; i++)
            {
                float ratio = i / 8f;
                new Ring(new Vector2(ratio * screenSize.X, screenSize.Y - (ratio * screenSize.Y)), random.Next(10, 36));
            }
        }
       
        protected override void LoadContent() // runs once when the game starts
        {
            instance = this;
            font = TtfFontBaker.Bake(File.ReadAllBytes(@"C:\\Windows\\Fonts\arial.ttf"), 25, 1024, 1024, new[] { CharacterRange.BasicLatin, CharacterRange.Latin1Supplement, CharacterRange.LatinExtendedA, CharacterRange.Cyrillic }).CreateSpriteFont(GraphicsDevice); //Idk how this works but it make fonts a lot easier
            spriteBatch = new SpriteBatch(GraphicsDevice);

            //Music and Sound effects by Eric Matyas
            //www.soundimage.org
            song = Content.Load<Song>("Retro-Frantic_V001_Looping");
            MediaPlayer.Volume = .3f;
            MediaPlayer.IsRepeating = true;
            MediaPlayer.Play(song);
            sounds[0] = Content.Load<SoundEffect>("UI_Quirky26"); //linking sound
            sounds[1] = Content.Load<SoundEffect>("Laser-Ricochet2"); //shooting
            sounds[2] = Content.Load<SoundEffect>("Explosion1"); // ring eliminated
            
            //define all our textures
            playerArrow = drawTriangle(12, 1.4f);
            shot = drawSquare(10);
            for (int c =1; c < CircleSizes.Length; c++)
            {
                CircleSizes[c] = drawRing(c);
            }
            
            linkTriangle = drawTriangle(12, 1f);
            pixel = new Texture2D(instance.GraphicsDevice, 1, 1);
            Color[] dataColors = { Color.White };
            pixel.SetData(0, null, dataColors, 0, 1);

            int Xsize = 20;
            X = new Texture2D(instance.GraphicsDevice, Xsize, Xsize);
            dataColors = new Color[Xsize * Xsize];
            for(int x =0; x < Xsize;  x++)
            {
                for(int y =0; y < Xsize; y++)
                {
                    if(x == y || x == Xsize-y)
                    {
                        dataColors[x + y * Xsize] = Color.White;
                    }
                }
            }
            X.SetData(0, null, dataColors, 0, Xsize*Xsize);


            // L i n k
            int width = 880;
           
            Vector2 startTextAt = new Vector2(screenSize.X/2 - width/2, 200);
            new Ring(startTextAt);
            new Ring(startTextAt + new Vector2(0, 100));
            new Ring(startTextAt + new Vector2(0, 200));
            new Ring(startTextAt + new Vector2(120, 200));

            new Ring(startTextAt + new Vector2(280, 200));
            new Ring(startTextAt + new Vector2(280, 100));
            new Ring(startTextAt + new Vector2(280, 0));



            new Ring(startTextAt + new Vector2(440, 100));
            new Ring(startTextAt + new Vector2(440, 200));
            new Ring(startTextAt + new Vector2(440, 0));
            new Ring(startTextAt + new Vector2(520, 0));
            new Ring(startTextAt + new Vector2(600, 100));
            new Ring(startTextAt + new Vector2(600, 200));

            Ring r = new Ring(startTextAt + new Vector2(880, 200));
            Ring r2 = new Ring(startTextAt + new Vector2(880, 0));
            Ring r3 = new Ring(startTextAt + new Vector2(760, 0));
            Ring r4 = new Ring(startTextAt + new Vector2(760, 200));
            r.linkedTo = r2.linkedTo = r3.linkedTo = r4.linkedTo= new Ring(startTextAt + new Vector2(760, 100));


        }

        protected override void UnloadContent()
        {
            
        }

        bool pausePressed = false;
        bool RPressed = false;
        bool TPressed = false;
        bool leftClicked = false;
        bool rightClicked = false;
        int scrollWheelOld = 0;
        protected override void Update(GameTime gameTime)
        {
            if (Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();
            #region pause toggle
            if (Keyboard.GetState().IsKeyDown(Keys.P))
            {
                if (!pausePressed)
                {
                    paused = !paused;
                    pausePressed = true;
                }


            }
            else
            {
                pausePressed = false;
            }
            #endregion
            mouse = Mouse.GetState();
            mousePos = mouse.Position.ToVector2();
            playeLinkSound = false;
            if (sandBox)
            {
                #region sandbox
                mouse = Mouse.GetState();
                mousePos = mouse.Position.ToVector2();
                int scrollWheelChange = mouse.ScrollWheelValue - scrollWheelOld;
                scrollWheelOld = mouse.ScrollWheelValue;
                SandboxCircleSize += scrollWheelChange / 60;
                if (SandboxCircleSize <1)
                {
                    SandboxCircleSize = 1;
                }
                if(SandboxCircleSize>99)
                {
                    SandboxCircleSize = 99;
                }
                if (mouse.LeftButton == ButtonState.Pressed)
                {
                    if (!leftClicked)
                    {
                        new Ring(mousePos, SandboxCircleSize);
                        leftClicked = true;
                    }


                }
                else
                {
                    leftClicked = false;
                }
                if (mouse.RightButton == ButtonState.Pressed)
                {
                    if (!rightClicked)
                    {
                        for(int r = 0; r < gameRings.Count; r++)
                        {
                            if((gameRings[r].Position-mousePos).Length() < gameRings[r].radius)
                            {
                                gameRings[r].linkedTo = null;
                                gameRings[r] = null;
                                gameRings.RemoveAt(r);
                            }
                        }
                        rightClicked = true;
                    }


                }
                else
                {
                    rightClicked = false;
                }
                #endregion
            }
            if (!paused)
            {
                #region controls
                if (Keyboard.GetState().IsKeyDown(Keys.R))
                {
                    if (!RPressed)
                    {
                        Reset();
                        RPressed = true;
                    }


                }
                else
                {
                    RPressed = false;
                }
                if (Keyboard.GetState().IsKeyDown(Keys.T))
                {
                    if (!TPressed)
                    {
                        sandBox = !sandBox;
                        TPressed = true;
                    }


                }
                else
                {
                    TPressed = false;
                }
                if (gamePlayers.Count > 0)
                {
                    if (!gamePlayers[0].dead)
                    {
                        if (Keyboard.GetState().IsKeyDown(Keys.A))
                        {
                            gamePlayers[0].rotation -= (float)Math.PI / 30;
                        }
                        if (Keyboard.GetState().IsKeyDown(Keys.D))
                        {
                            gamePlayers[0].rotation += (float)Math.PI / 30;
                        }
                        if (Keyboard.GetState().IsKeyDown(Keys.W))
                        {
                            gamePlayers[0].Position += PolarVector(3f, gamePlayers[0].rotation);
                        }
                        if (Keyboard.GetState().IsKeyDown(Keys.Space) && gamePlayers[0].energy > (int)(Player.energyMax / 3f) && gamePlayers[0].cooldown == 0)
                        {
                            gamePlayers[0].energy -= (int)(Player.energyMax / 3f);
                            gamePlayers[0].cooldown = 10;
                            new Projectile(gamePlayers[0].Position + PolarVector(30, gamePlayers[0].rotation), PolarVector(7, gamePlayers[0].rotation));
                            sounds[1].Play(.5f, 1f, 1f); ;
                        }
                        gamePlayers[0].Position = LoopAroundCheck(gamePlayers[0].Position);
                    }
                    
                    if (gamePlayers.Count > 1 && !gamePlayers[1].dead)
                    {
                        GamePadState gamePad = GamePad.GetState(0);
                        if (Keyboard.GetState().IsKeyDown(Keys.Left) || gamePad.DPad.Left == ButtonState.Pressed)
                        {
                            gamePlayers[1].rotation -= (float)Math.PI / 30;
                        }
                        if (Keyboard.GetState().IsKeyDown(Keys.Right) || gamePad.DPad.Right == ButtonState.Pressed)
                        {
                            gamePlayers[1].rotation += (float)Math.PI / 30;
                        }
                        if (Keyboard.GetState().IsKeyDown(Keys.Up) || gamePad.DPad.Up == ButtonState.Pressed)
                        {
                            gamePlayers[1].Position += PolarVector(3f, gamePlayers[1].rotation);
                        }
                        if ((Keyboard.GetState().IsKeyDown(Keys.RightAlt) || gamePad.Buttons.A == ButtonState.Pressed || gamePad.Buttons.B== ButtonState.Pressed || gamePad.Buttons.X == ButtonState.Pressed || gamePad.Buttons.Y == ButtonState.Pressed) && gamePlayers[1].energy > (int)(Player.energyMax / 3f) && gamePlayers[1].cooldown == 0)
                        {
                            gamePlayers[1].energy -= (int)(Player.energyMax/3f);
                            gamePlayers[1].cooldown = 10;
                            new Projectile(gamePlayers[1].Position + PolarVector(30, gamePlayers[1].rotation), PolarVector(7, gamePlayers[1].rotation));
                            sounds[1].Play(.5f, 1f, 1f); ;
                        }
                        gamePlayers[1].Position = LoopAroundCheck(gamePlayers[1].Position);
                    }

                    

                }
                #endregion

                //gives players 'death' state, If I just remove them from the list it can cause an error
                for(int p =0; p < gamePlayers.Count; p++)
                {
                    if(!gamePlayers[p].dead)
                    {
                        gamePlayers[p].Update();
                        bool hasRing = false;
                        foreach (Ring ring in LinkGame.gameRings)
                        {
                            if (ring.LinkedToPlayer == p)
                            {
                                hasRing = true;
                                break;
                            }
                        }
                        if (!hasRing)
                        {
                            gamePlayers[p].dead = true;
                        }
                        
                    }
                    
                }

                for (int p = 0; p < gameProjectiles.Count; p++)
                {
                    gameProjectiles[p].Update(); //call once per frame per object projectile logic
                }
                foreach (Ring ring in gameRings)
                {
                    ring.Update();//call once per frame per object ring logic
                }
                if (gameRings.Count < 12 && gamePlayers.Count>0 && !sandBox)
                {
                    new Ring(new Vector2(random.Next((int)screenSize.X), random.Next((int)screenSize.Y)), random.Next(10, 36)); //spawn a random ring if a ring got shot in vs. mode
                }
                for(int k =0; k < gameParticles.Count; k++)
                {
                    gameParticles[k].Update();//call once per frame per object particle logic
                    if (gameParticles[k].scale <=0)
                    {
                        gameParticles.RemoveAt(k);//remove particles when they shrink into nothingness
                    }
                }
            }

            if(playeLinkSound)
            {
                sounds[0].Play();
            }
            base.Update(gameTime);
        }
        float orbitCounter;
        protected override void Draw(GameTime gameTime) // handles drawing
        {
            orbitCounter += (float)Math.PI / 30;
            GraphicsDevice.Clear(Color.Black);
            spriteBatch.Begin();

            //player drawing code
            foreach (Player player in gamePlayers)
            {
                if (!player.dead)
                {
                    spriteBatch.Draw(playerArrow, player.Position, null, Color.White, player.rotation, new Vector2(playerArrow.Width, playerArrow.Height) * .5f, 1f, SpriteEffects.None, 0); //the triangle you control

                    //the orbiting squares
                    if (player.energy >= Player.energyMax)
                    {
                        for (int r = 0; r < 3; r++)
                        {
                            float rot = r * 2 * (float)(Math.PI / 3) + orbitCounter;
                            spriteBatch.Draw(shot, player.Position + PolarVector(20, rot), null, Color.White, rot, new Vector2(shot.Width, shot.Height) * .5f, 1f, SpriteEffects.None, 0);
                        }
                    }
                    else if (player.energy > 2f * Player.energyMax/3f)
                    {
                        for (int r = 0; r < 2; r++)
                        {
                            float rot = r * (float)Math.PI + orbitCounter;
                            spriteBatch.Draw(shot, player.Position + PolarVector(20, rot), null, Color.White, rot, new Vector2(shot.Width, shot.Height) * .5f, 1f, SpriteEffects.None, 0);
                        }

                    }
                    else if (player.energy > Player.energyMax / 3f)
                    {
                        spriteBatch.Draw(shot, player.Position + PolarVector(20, orbitCounter), null, Color.White, orbitCounter, new Vector2(shot.Width, shot.Height) * .5f, 1f, SpriteEffects.None, 0);
                    }

                }
            }
            
            foreach(Ring ring in gameRings) //ring drawing
            {
               
                int radiusUsed = ring.radius;
                if (ring.time< ring.radius)
                {
                        radiusUsed = ring.time;
                }
                spriteBatch.Draw(CircleSizes[radiusUsed], ring.Position - new Vector2(CircleSizes[radiusUsed].Width, CircleSizes[radiusUsed].Height)*.5f);
                if (ring.Position.Y < radiusUsed)
                {
                    spriteBatch.Draw(CircleSizes[radiusUsed], ring.Position - new Vector2(CircleSizes[radiusUsed].Width, CircleSizes[radiusUsed].Height) * .5f + Vector2.UnitY *screenSize.Y);
                }
                if (ring.Position.Y > screenSize.Y - radiusUsed)
                {
                    spriteBatch.Draw(CircleSizes[radiusUsed], ring.Position - new Vector2(CircleSizes[radiusUsed].Width, CircleSizes[radiusUsed].Height) * .5f - Vector2.UnitY * screenSize.Y);
                }
                if (ring.Position.X < radiusUsed)
                {
                    spriteBatch.Draw(CircleSizes[radiusUsed], ring.Position - new Vector2(CircleSizes[radiusUsed].Width, CircleSizes[radiusUsed].Height) * .5f + Vector2.UnitX * screenSize.X);
                }
                if (ring.Position.Y > screenSize.X - radiusUsed)
                {
                    spriteBatch.Draw(CircleSizes[radiusUsed], ring.Position - new Vector2(CircleSizes[radiusUsed].Width, CircleSizes[radiusUsed].Height) * .5f - Vector2.UnitX * screenSize.X);
                }
                if (ring.linkedTo != null)
                {
                    float direction = ToRotation(screenLoopAdjust(ring.Position, ring.linkedTo.Position) - ring.Position);
                    int linkedToRadius = ring.linkedTo.radius;
                    if (ring.linkedTo.time < ring.linkedTo.radius)
                    {
                        linkedToRadius = ring.linkedTo.time;
                    }
                    drawLine(spriteBatch, ring.Position + PolarVector(radiusUsed, direction), ring.linkedTo.Position + PolarVector(linkedToRadius, direction + (float)Math.PI));
                   
                    if (radiusUsed > 9)
                    {
                        spriteBatch.Draw(linkTriangle, ring.Position, null, Color.White, direction, new Vector2(linkTriangle.Width, linkTriangle.Height) * .5f, 1f, SpriteEffects.None, 0);
                    }

                }
            }
            foreach(Projectile projectile in gameProjectiles) //projectile drawing
            {
                spriteBatch.Draw(shot, projectile.Position, null, Color.White, projectile.rotation, new Vector2(shot.Width, shot.Height) * .5f, 1f, SpriteEffects.None, 0);
            }
            foreach(Particle particle in gameParticles)
            {
                spriteBatch.Draw(pixel, particle.Position, null, Color.White, 0f, Vector2.Zero, particle.scale, SpriteEffects.None, 0);
            }
            if(paused)
            {
                string pauseText = "Paused";
                Vector2 pauseSize = font.MeasureString(pauseText);
                spriteBatch.DrawString(font, pauseText, new Vector2(screenSize.X / 2 - pauseSize.X / 2, 450), Color.White);
            }
            string howToStart = "";
            if (MainMenu)
            {
                 howToStart = "Press 'R' to begin vs. mode!";
                
            }
            if(gamePlayers.Count ==2 && gamePlayers[0].dead)
            {
                 howToStart = "Player 2 Wins! Press 'R' to reset!";
               
            }
            else if(gamePlayers.Count == 2 && gamePlayers[1].dead)
            {
                 howToStart = "Player 1 Wins! Press 'R' to reset!";
              
            }
            if(howToStart != "" && !sandBox)
            {
                Vector2 TextSize = font.MeasureString(howToStart);
                spriteBatch.DrawString(font, howToStart, new Vector2(screenSize.X / 2 - TextSize.X / 2, 500), Color.White);
                howToStart = "Press 'T' to toggle sandbox mode!";
                 TextSize = font.MeasureString(howToStart);
                spriteBatch.DrawString(font, howToStart, new Vector2(screenSize.X / 2 - TextSize.X / 2, 500+ TextSize.Y *2), Color.White);
            }
            if(sandBox)
            {
                spriteBatch.Draw(X, mousePos, null, Color.White, 0f, new Vector2(X.Width, X.Height)*.5f, 1f, SpriteEffects.None, 0);
                spriteBatch.Draw(CircleSizes[SandboxCircleSize], mousePos- new Vector2(CircleSizes[SandboxCircleSize].Width, CircleSizes[SandboxCircleSize].Height) * .5f, new Color(Color.White, .5f));
            }
            spriteBatch.End();
            
    
            base.Draw(gameTime);
        }
    }
}
