using System;
using Wki.EventSourcing.Util;

namespace Wki.EventSourcing.Protocol
{
    /// <summary>
    /// Abstract base class for an event
    /// </summary>
    public abstract class Event 
    {
        /// <summary>
        /// The time the event occured.
        /// </summary>
        /// <value>The occured on.</value>
        /// 
        public DateTime OccuredOn { get; private set; }

        public Event() : this(SystemTime.Now) {}
        public Event(DateTime occuredOn)
        {
            OccuredOn = occuredOn;
        }

        /// <summary>
        /// Stringified Id (needed for serialisation into envelope)
        /// </summary>
        /// <returns>The identifier.</returns>
        public virtual string GetId() => null;
    }

    /// <summary>
    /// abstract base class for an event targeting an aggregate root with an id type
    /// </summary>
    public abstract class Event<TIndex> : Event
    {

        /// <summary>
        /// Identifier for the aggregate root involved. May be null
        /// </summary>
        /// <value>The persistence identifier.</value>
        public TIndex Id { get; protected set; }

        public Event() : this(SystemTime.Now) { }
        public Event(TIndex id) : this(SystemTime.Now, id) { }
        public Event(DateTime occuredOn) : this(occuredOn, default(TIndex)) { }
        public Event(DateTime occuredOn, TIndex id) : base(occuredOn)
        {
            Id = id;
        }

        public override string GetId() => Id.ToString();
    }
}
