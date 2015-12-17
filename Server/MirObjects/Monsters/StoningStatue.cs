using System;
using System.Drawing;
using Server.MirDatabase;
using Server.MirEnvir;
using S = ServerPackets;

namespace Server.MirObjects.Monsters
{
    public class StoningStatue : MonsterObject
    {
        private const byte AttackRange = 6;

        protected internal StoningStatue(MonsterInfo info)
            : base(info)
        {
        }

        protected override bool InAttackRange()
        {
            if (Target.CurrentMap != CurrentMap) return false;
            if (Target.CurrentLocation == CurrentLocation) return false;

            int x = Math.Abs(Target.CurrentLocation.X - CurrentLocation.X);
            int y = Math.Abs(Target.CurrentLocation.Y - CurrentLocation.Y);

            if (x > 5 || y > 5) return false;


            return (x <= 1 && y <= 1) || (x == y || x % 5 == y % 5);
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
                if (Envir.Random.Next(6) > 0)
                {
                    int damage = GetAttackPower(MinDC, MaxDC);             
                    if (damage == 0) return;

                    Target.Attacked(this, damage, DefenceType.ACAgility);

                    ShockTime = 0;
                    ActionTime = Envir.Time + 300;
                    AttackTime = Envir.Time + AttackSpeed;

                    Direction = Functions.DirectionFromPoint(CurrentLocation, Target.CurrentLocation);
                    Broadcast(new S.ObjectAttack { ObjectID = ObjectID, Direction = Direction, Location = CurrentLocation, Type = 1 });

                }
                else
                {
                    Broadcast(new S.ObjectAttack { ObjectID = ObjectID, Direction = Direction, Location = CurrentLocation});
                    Attack2();
                }

            }
            else
            {
                if (Envir.Random.Next(2) == 0)
                {
                    Broadcast(new S.ObjectRangeAttack { ObjectID = ObjectID, Direction = Direction, Location = CurrentLocation, TargetID = Target.ObjectID });

                    ActionTime = Envir.Time + 300;
                    AttackTime = Envir.Time + AttackSpeed;

                    int damage = GetAttackPower(MinMC, MaxMC);
                    if (damage == 0) return;
                    Target.Attacked(this, damage, DefenceType.MACAgility);
                    int delay = Functions.MaxDistance(CurrentLocation, Target.CurrentLocation) * 50 + 500; //50 MS per Step

                    DelayedAction action = new DelayedAction(DelayedType.Damage, Envir.Time + delay, Target, damage, DefenceType.MACAgility);
                    ActionList.Add(action);
                }
                else
                {
                    MoveTo(Target.CurrentLocation);
                }
            }


            if (Target.Dead)
                FindTarget();

        }
       
        private void Attack2()
        {
            int damage = GetAttackPower(MinDC, MaxDC);
            if (damage == 0) return;

            Target.Attacked(this, damage, DefenceType.ACAgility);

            ShockTime = 0;
            ActionTime = Envir.Time + 300;
            AttackTime = Envir.Time + AttackSpeed;

            Target.ApplyPoison(new Poison { Owner = this, Duration = 5, PType = PoisonType.Stun, TickSpeed = 1000, }, this);
        }

        protected override void ProcessTarget()
        {
            if (Target == null) return;

            if (InAttackRange() && CanAttack)
            {
                Attack();
                return;
            }

            if (Envir.Time < ShockTime)
            {
                Target = null;
                return;
            }

            MoveTo(Target.CurrentLocation);

        }
    }
}
