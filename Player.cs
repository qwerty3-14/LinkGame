using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Connect
{
    public class Player
    {
        public float rotation = 0f;
        public Vector2 Position;
        public int energy = 0;
        public const int energyMax = 720;
        public int cooldown = 0;
        public bool dead = false;
        
        public Player(Vector2 Position)
        {
            this.Position = Position;
            Ring r = new Ring(Position);
            LinkGame.gamePlayers.Add(this);
            r.LinkedToPlayer = LinkGame.gamePlayers.IndexOf(this);
        }
        public void Update()
        {
            if(energy>energyMax)
            {
                energy = energyMax;
            }
            if(cooldown > 0)
            {
                cooldown--;
            }
            



        }
    }
}
