using System;

namespace TGC.Group.Model.World.Weapons
{
    public class Weapon
    {
        public int Id { get; private set; }
        public string Name { get; private set; }
        public int Ammo { get; set; }

        public Weapon(int id, string name, int initialAmmo)
        {
            Id = id;
            Name = name;
            Ammo = initialAmmo;
        }

        public virtual void Fire()
        {
            Ammo--;
        }

        //public void Dispose()
        //{

        //}

    }
}
