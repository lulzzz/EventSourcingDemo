﻿using System;
using System.Collections.Generic;
using Wki.EventSourcing.Util;

namespace Wki.EventSourcing.Messages
{
    public class OfficeActorChildState
    {
        // start information
        public DateTime StartedAt { get; internal set; }

        // alive management
        public DateTime LastStatusQuerySentAt { get; internal set; }
        public DateTime LastStatusReceivedAt { get; internal set; }
        public DurableActorStatus Status { get; internal set; }

        // command forwarding
        public int NrCommandsForwarded { get; internal set; }
        public DateTime LastCommandForwardedAt { get; internal set; }

        public OfficeActorChildState()
        {
            var now = SystemTime.Now;
            StartedAt = now;
            LastStatusQuerySentAt = DateTime.MinValue;
            LastStatusReceivedAt = DateTime.MinValue;
            Status = DurableActorStatus.Initializing;
            NrCommandsForwarded = 0;
            LastCommandForwardedAt = DateTime.MinValue;
        }
    }

    public class OfficeActorState
    {
        // start information
        public DateTime StartedAt { get; internal set; }

        // durable actors
        public int NrActorsLoaded { get; internal set; }
        public DateTime LastActorLoadedAt { get; internal set; }
        public int NrActorsRemoved { get; internal set; }
        public DateTime LastActorRemovedAt { get; internal set; }
        public int NrActorChecks { get; internal set; }
        public DateTime LastActorCheckAt { get; internal set; }

        // currently loaded actors
        public Dictionary<string, OfficeActorChildState> ChildActorStates { get; internal set; }
        public int NrActorsMissed { get; internal set; }

        // message handling
        public int NrCommandsForwarded { get; internal set; }
        public int NrUnhandledMessages { get; internal set; }
        public DateTime LastCommandForwardedAt { get; internal set; }

        public OfficeActorState()
        {
            var now = SystemTime.Now;
            StartedAt = now;
            NrActorsLoaded = 0;
            LastActorLoadedAt = DateTime.MinValue;
            NrActorsRemoved = 0;
            LastActorRemovedAt = DateTime.MinValue;
            NrActorChecks = 0;
            LastActorCheckAt = DateTime.MinValue;
            ChildActorStates = new Dictionary<string, OfficeActorChildState>();
            NrActorsMissed = 0;
            NrCommandsForwarded = 0;
            NrUnhandledMessages = 0;
            LastCommandForwardedAt = DateTime.MinValue;
        }

        internal void AddChildActor(string name)
        {
            if (ChildActorStates.ContainsKey(name))
            {
                // already present. Just update if needed
            }
            else
            {
                NrActorsLoaded++;
                LastActorLoadedAt = SystemTime.Now;
                ChildActorStates[name] = new OfficeActorChildState();
            }
        }

        internal void RemoveChildActor(string name)
        {
            if (ChildActorStates.ContainsKey(name))
            {
                NrActorsRemoved++;
                LastActorRemovedAt = SystemTime.Now;
                ChildActorStates.Remove(name);
            }
        }
    }
}
