﻿using System;

namespace Wki.EventSourcing.Messages
{
    /// <summary>
    /// Command to journal writer to persist an event
    /// </summary>
    public class PersistEvent
    {
        public Event Event { get; set; }

        public PersistEvent(Event @event)
        {
            Event = @event;
        }
    }
}
