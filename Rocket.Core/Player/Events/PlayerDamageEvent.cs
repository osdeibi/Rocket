﻿using Rocket.API.Commands;
using Rocket.API.Eventing;
using Rocket.API.Player;
using Rocket.API.User;

namespace Rocket.Core.Player.Events
{
    public class PlayerDamageEvent : PlayerEvent, ICancellableEvent
    {
        public PlayerDamageEvent(IPlayer player, double damage, IUser damageDealer) : base(player)
        {
            Damage = damage;
            DamageDealer = damageDealer;
        }

        public PlayerDamageEvent(IPlayer player, double damage, IUser damageDealer, bool global = true) :
            base(player, global)
        {
            Damage = damage;

            DamageDealer = damageDealer;
        }

        public PlayerDamageEvent(IPlayer player, double damage, IUser damageDealer,
                                 EventExecutionTargetContext executionTarget = EventExecutionTargetContext.Sync,
                                 bool global = true) : base(player, executionTarget, global)
        {
            Damage = damage;
            DamageDealer = damageDealer;
        }

        public PlayerDamageEvent(IPlayer player, double damage, IUser damageDealer, string name = null,
                                 EventExecutionTargetContext executionTarget = EventExecutionTargetContext.Sync,
                                 bool global = true) : base(player, name, executionTarget, global)
        {
            Damage = damage;
            DamageDealer = damageDealer;
        }

        public IUser DamageDealer { get; }
        public double Damage { get; set; }
        public bool IsCancelled { get; set; }
    }
}