﻿using System;
using Rocket.API.Commands;
using Rocket.API.Eventing;
using Rocket.API.Player;

namespace Rocket.Core.Player.Events
{
    public class PlayerBanEvent : Event, ICancellableEvent
    {
        public TimeSpan? Duration { get; set; }
        public ICommandCaller Caller { get; }
        public string Reason { get; set; }

        public PlayerBanEvent(IPlayer player, ICommandCaller caller = null, string reason = null, TimeSpan? duration = null) : base(true)
        {
            Caller = caller;
            Reason = reason;
            Duration = duration;
        }
        public PlayerBanEvent(IPlayer player, ICommandCaller caller = null, string reason = null, TimeSpan? duration = null, bool global = true) : base(global)
        {
            Caller = caller;
            Reason = reason;
            Duration = duration;
        }
        public PlayerBanEvent(IPlayer player, ICommandCaller caller = null, string reason = null, TimeSpan? duration = null, EventExecutionTargetContext executionTarget = EventExecutionTargetContext.Sync, bool global = true) : base(executionTarget, global)
        {
            Caller = caller;
            Reason = reason;
            Duration = duration;
        }
        public PlayerBanEvent(IPlayer player, ICommandCaller caller = null, string reason = null, TimeSpan? duration = null, string name = null, EventExecutionTargetContext executionTarget = EventExecutionTargetContext.Sync, bool global = true) : base(name, executionTarget, global)
        {
            Caller = caller;
            Reason = reason;
            Duration = duration;
        }

        public bool IsCancelled { get; set; }
    }
}