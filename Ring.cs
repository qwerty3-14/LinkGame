using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Connect
{
    public class Ring
    {
        public Vector2 Position;
        public int radius;
        public Ring linkedTo;
        public int LinkedToPlayer = -1;
        public int time = 1;
        public int energy;
        public int maxEnergy = 10;
        public Ring(Vector2 Position, int radius = 24)
        {
            
            this.Position = Position;
            this.radius = radius;
            LinkGame.gameRings.Add(this);
        }
        public void Update()
        {
            this.energy++;
            if (this.energy > this.maxEnergy)
            {
                this.energy = this.maxEnergy;
            }
            this.time++;
            if (this.LinkedToPlayer != -1)
            {
                this.Position = LinkGame.gamePlayers[this.LinkedToPlayer].Position;
                LinkGame.gamePlayers[this.LinkedToPlayer].energy += this.energy;
                this.energy = 0;
            }
            if (this.linkedTo != null && !LinkGame.gameRings.Contains(this.linkedTo))
            {
                this.linkedTo = null;
            }
            foreach (Ring otherRing in LinkGame.gameRings)
            {

                if (this.LinkedToPlayer == -1 && this != otherRing)
                {
                    while ((this.Position - LinkGame.screenLoopAdjust(Position, otherRing.Position) ).Length() < this.radius + otherRing.radius)
                    {
                        float direction = LinkGame.ToRotation(this.Position - LinkGame.screenLoopAdjust(Position, otherRing.Position));
                        this.Position += LinkGame.PolarVector(1, direction);
                        if (otherRing.LinkedToPlayer == -1)
                        {
                            otherRing.Position -= LinkGame.PolarVector(1, direction);
                        }

                    }

                    if (this.linkedTo == null && (this.Position - LinkGame.screenLoopAdjust(this.Position, otherRing.Position)).Length() < 100 + this.radius + otherRing.radius && otherRing.linkedTo != this)
                    {
                        this.linkedTo = otherRing;
                        LinkGame.playeLinkSound = true;
                    }

                }
            }
            if (this.linkedTo != null)
            {
                this.linkedTo.energy += this.energy;
                this.energy = 0;
                if ((this.Position - LinkGame.screenLoopAdjust(this.Position, this.linkedTo.Position)).Length() > 100 + this.radius + this.linkedTo.radius)
                {
                    float direction = LinkGame.ToRotation(LinkGame.screenLoopAdjust(this.Position, this.linkedTo.Position) - this.Position);
                    this.Position += LinkGame.PolarVector(3, direction);
                }


            }
            this.Position = LinkGame.LoopAroundCheck(this.Position);
        }
    }
}
