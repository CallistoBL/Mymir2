using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using Server.MirDatabase;
using Server.MirEnvir;
using S = ServerPackets;

namespace Server.MirObjects.Monsters
{
    public class WingedTigerLord : MonsterObject
    {
        protected internal WingedTigerLord(MonsterInfo info)
            : base(info)
        {
        }

        protected override bool InAttackRange()
        {
            return CurrentMap == Target.CurrentMap && Functions.InRange(CurrentLocation, Target.CurrentLocation, 1);
        }
        
        protected override void Attack()
        {
            if (!Target.IsAttackTarget(this))
            {
                Target = null;
                return;
            }

            ShockTime = 0;


            Direction = Functions.DirectionFromPoint(CurrentLocation, Target.CurrentLocation);
            bool ranged = CurrentLocation == Target.CurrentLocation || !Functions.InRange(CurrentLocation, Target.CurrentLocation, 1);

            if (!ranged)
            {
                if (Envir.Random.Next(2) > 0)
                {
                    base.Attack();
                }
                else
                {
                    if (Envir.Random.Next(2) > 0)
                    {
                        Broadcast(new S.ObjectAttack { ObjectID = ObjectID, Direction = Direction, Location = CurrentLocation, Type = 1});
                        Attack1();
                    }
                    else
                    {
                        if (Envir.Random.Next(2) > 0)
                        {
                            Broadcast(new S.ObjectAttack { ObjectID = ObjectID, Direction = Direction, Location = CurrentLocation, Type = 2 });
                            Attack2();
                        }
                        else
                        {
                            Broadcast(new S.ObjectAttack { ObjectID = ObjectID, Direction = Direction, Location = CurrentLocation, Type = 3 });
                            Attack3();
                        }
                    }
                }

                int damage = GetAttackPower(MinDC, MaxDC);
                Broadcast(new S.ObjectAttack { ObjectID = ObjectID, Direction = Direction, Location = CurrentLocation});
                if (damage == 0) return;

                Target.Attacked(this, damage, DefenceType.ACAgility);

                ShockTime = 0;
                ActionTime = Envir.Time + 300;
                AttackTime = Envir.Time + AttackSpeed;
                
            }
            else
            {
                MoveTo(Target.CurrentLocation);
            }
        }
        private void Attack1()
        {
            int damage = GetAttackPower(MinDC, MaxDC);
            if (damage == 0) return;

            Target.Attacked(this, damage, DefenceType.ACAgility);
        }
        private void Attack2()
        {
            int damage = GetAttackPower(MinDC, MaxDC);
            if (damage == 0) return;

            Target.Attacked(this, damage, DefenceType.ACAgility);
            
            if (Envir.Random.Next(2) == 0)
            {
                List<MapObject> targets = FindAllTargets(1, CurrentLocation);
                if (targets.Count == 0) return;

                for (int i = 0; i < targets.Count; i++)
                targets[i].ApplyPoison(new Poison { Owner = this, Duration = 3, PType = PoisonType.Stun, TickSpeed = 1000 }, this);
            }
        }
        private void Attack3()
        {
            int damage = GetAttackPower(MinDC, MaxDC);
            if (damage == 0) return;

            Target.Attacked(this, damage, DefenceType.MACAgility);
            if (Envir.Random.Next(Settings.MagicResistWeight) >= Target.MagicResist)
            {
                Target.Pushed(Owner = this, Direction = Direction, distance: 3);
            }
        }

    }
}