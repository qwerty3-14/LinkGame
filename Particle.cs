using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Connect
{
    public class Particle
    {
        public float scale = 1f;
        public Vector2 Position;
        Vector2 Velocity;
        public Particle(Vector2 Position, Vector2 Velocity, float scale)
        {
            this.Position = Position;
            this.Velocity = Velocity;
            this.scale = scale;
            LinkGame.gameParticles.Add(this);
        }
        public void Update()
        {
            this.Position += Velocity;
            this.scale -= 1f/30f;
            
        }
    }
}
