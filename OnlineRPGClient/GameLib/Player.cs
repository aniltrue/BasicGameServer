using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace OnlineRPGClient.GameLib
{
    public class Player
    {
        private String name;
        private int id;

        private double x, y, mouseAngle, dirX, dirY, targetX, targetY, range, attackAngle, attackSpeed, moveSpeed, attackDamage, health;
        private bool attacking, isAlive;

        public Player(String json)
        {
            Update(json);
        }

        public void Update(String json)
        {
            dynamic player = JObject.Parse(json);

            name = player.name;
            id = player.id;
            x = player.x;
            y = player.y;
            dirX = player.dirx;
            dirY = player.diry;
            range = player.range;
            attackAngle = player.attackangle;
            attackSpeed = player.attackspeed;
            moveSpeed = player.movespeed;
            attackDamage = player.attackdamage;
            health = player.health;
            isAlive = player.isalive;
        }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this).ToLower();
        }

        public int ID => id;
        public String Name => name;

        public double X => x;

        public double Y => y;

        public double MouseAngle => mouseAngle;

        public double DirX => dirX;

        public double DirY => dirY;

        public double TargetX => targetX;

        public double TargetY => targetY;

        public double Range => range;

        public double AttackAngle => attackAngle;

        public double AttackSpeed => attackSpeed;

        public double MoveSpeed => moveSpeed;

        public double AttackDamage => attackDamage;

        public double Health => health;

        public bool Attacking
        {
            get => attacking;
            set => attacking = value;
        }

        public bool IsAlive => isAlive;

        public void SetMouse(double mouseX, double mouseY)
        {
            targetX = mouseX;
            targetY = mouseY;

            double angle = Math.Atan2(mouseY - y, mouseX - x);

            mouseAngle = angle;

            if (Math.Abs(mouseX - x) > moveSpeed)
                dirX = Math.Cos(angle);
            else
                dirX = 0;

            if (Math.Abs(mouseY - y) > moveSpeed)
                dirY = Math.Sin(angle);
            else
                dirY = 0;
        }
    }
}
