using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Connect
{
    public class Projectile
    {
        public Vector2 Position;
        public Vector2 Velocity;
        public float rotation;
        public int lifeTime = 120;
        public Projectile(Vector2 Position, Vector2 Velocity)
        {
            this.Position = Position;
            this.Velocity = Velocity;
            LinkGame.gameProjectiles.Add(this);
        }
        public void Update()
        {
            lifeTime--;
            Position += Velocity;
            rotation += (float)Math.PI / 15;
            Position = LinkGame.LoopAroundCheck(Position);
            if(LinkGame.random.Next(2)==0)
            {
                new Particle(Position, LinkGame.PolarVector(1, (float)LinkGame.random.NextDouble() * (float)Math.PI * 2), 2f);
            }
            for(int i =0; i < LinkGame.gameRings.Count; i++)
            {
                if((LinkGame.gameRings[i].Position - this.Position).Length() < LinkGame.gameRings[i].radius)
                {
                    for(int p = 0; p < LinkGame.gameRings[i].radius; p++)
                    {
                        new Particle(LinkGame.gameRings[i].Position, LinkGame.PolarVector((float)LinkGame.random.NextDouble() * 4 + 1, (float)LinkGame.random.NextDouble() * 2 * (float)Math.PI), 2f + (float)LinkGame.random.NextDouble() * 3);
                    }
                    LinkGame.gameRings[i].linkedTo = null;
                    LinkGame.gameRings[i] = null;
                    LinkGame.gameRings.RemoveAt(i);
                    LinkGame.gameProjectiles.Remove(this);
                    LinkGame.sounds[2].Play(.5f, 1f, 1f);
                }
            }
            if(lifeTime<=0)
            {
                LinkGame.gameProjectiles.Remove(this);
                for (int p = 0; p < 5; p++)
                {
                    new Particle(Position, LinkGame.PolarVector(3, (float)LinkGame.random.NextDouble() * (float)Math.PI * 2), 2f);
                }

            }
        }
    }
}
