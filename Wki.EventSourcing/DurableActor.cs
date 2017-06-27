﻿using System;
using Akka.Actor;
using Wki.EventSourcing.Statistics;
using Wki.EventSourcing.Protocol.Retrieval;
using Wki.EventSourcing.Protocol.Persistence;
using Wki.EventSourcing.Protocol.Subscription;
using static Wki.EventSourcing.Util.Constant;

namespace Wki.EventSourcing.Actors
{
    /// <summary>
    /// Base class for a durable actor
    /// </summary>
    public abstract class DurableActor : UntypedActor, IWithUnboundedStash
    {
        public IActorRef EventStore;
        public string PersistenceId;

        // flag for deciding if snapshot wanted
        public bool HasState;

        // type of (snapshot) state
        public Type StateType;

        // count received events against requested events. Re-Request if all received
        private int NrEventsMissing;

        // latest events Id
        public int LastEventId;

        // actor sending the latest command. Will receive an answer after persisting
        public IActorRef LastCommandSender;

        // a customizable receive timeout for regular operation
        public TimeSpan DefaultReceiveTimeout;

        // statistics about current operation
        public DurableActorStatistics Statistics;

        public IStash Stash { get; set; }

        public DurableActor(IActorRef eventStore)
        {
            EventStore = eventStore;
            PersistenceId = GetType().Name;
            HasState = false;
            StateType = typeof(object);
            NrEventsMissing = 0;
            LastEventId = -1;
            LastCommandSender = null;
            DefaultReceiveTimeout = MaxActorIdleTimeSpan;
            Statistics = new DurableActorStatistics();
        }

        protected void Subscribe() =>
            Subscribe(BuildEventFilter());

        protected void Subscribe(EventFilter eventFilter) =>
            EventStore.Tell(new Subscribe(eventFilter));

        protected void UnSubscribe() =>
            EventStore.Tell(Unsubscribe.Instance);

        // must be overloaded in order to return events we are interested in
        protected abstract EventFilter BuildEventFilter();

        protected override void PreStart()
        {
            base.PreStart();
            SetReceiveTimeout(MaxRestoreIdleTimeSpan);
            if (HasState)
            {
                EventStore.Tell(new LoadSnapshot(PersistenceId, StateType));
                BecomeStacked(WaitingForSnapshot);
            }
            else
            {
                BecomeStacked(Loading);
                RequestNextEvents();
            }
            Statistics.StartRestoring();
        }

        // inherited from ReceiveActor - handle EventRecord
        protected override void OnReceive(object message)
        {
            if (message is EventRecord eventRecord)
                HandleEventRecord(eventRecord);
            else
                Handle(message);
        }

        /// <summary>
        /// universal handling of an eventRecord received from event Store
        /// </summary>
        /// <param name="eventRecord"></param>
        protected void HandleEventRecord(EventRecord eventRecord)
        {
            Statistics.EventReceived();
            LastEventId = eventRecord.Id;
            Apply(eventRecord.Event);
        }

        /// <summary>
        /// handle incoming messages
        /// </summary>
        /// <param name="message"></param>
        protected abstract void Handle(object message);

        /// <summary>
        /// Apply an event
        /// </summary>
        /// <param name="e"></param>
        protected abstract void Apply(IEvent e);

        /// <summary>
        /// Persist a given domain Event to the event Store
        /// </summary>
        /// <param name="domainEvent"></param>
        protected void Persist(IEvent domainEvent)
        {
            Statistics.EventPersisted();
            LastCommandSender = Sender;
            EventStore.Tell(new PersistEvent(PersistenceId, domainEvent));
            SetReceiveTimeout(MaxPersistIdleTimeSpan);
            BecomeStacked(Persisting);
        }

        /// <summary>
        /// Save the state obtained state. default behavior: do nothing
        /// </summary>
        /// <param name="state"></param>
        virtual protected void SaveSnapshot(Snapshot snapshot) {}

        // Wait for Snapshot behavior
        protected virtual void WaitingForSnapshot(object message)
        {
            switch (message)
            {
                case ReceiveTimeout _:
                case NoSnapshot _:
                    // switch to Loading
                    break;

                case Snapshot snapshot:
                    SaveSnapshot(snapshot);
                    break;
            }

            Become(Loading);
            RequestNextEvents();
        }

        private void RequestNextEvents()
        {
            var loadNextEvents = LoadNextEvents.After(LastEventId);
            NrEventsMissing += loadNextEvents.NrEvents;
            EventStore.Tell(loadNextEvents);
        }

        // Loading behavior
        public void Loading(object message)
        {
            switch (message)
            {
                case ReceiveTimeout _:
                    throw new PersistTimeoutException("Timeout reached during Load");

                case EventRecord r:
                    HandleEventRecord(r);
                    if (--NrEventsMissing <= 0)
                        RequestNextEvents();
                    break;

                case End _:
                    Statistics.FinishedRestoring();
                    SetReceiveTimeout(DefaultReceiveTimeout);
                    Stash.UnstashAll();
                    UnbecomeStacked();
                    break;

                default:
                    Stash.Stash();
                    break;
            }
        }

        // Persisting behavior
        public void Persisting(object message)
        {
            switch (message)
            {
                case ReceiveTimeout _:
                    var error = "Timeout during persisting";
                    LastCommandSender.Tell(Reply.Error(error));
                    throw new PersistTimeoutException(error);

                case EventRecord r:
                    HandleEventRecord(r);
                    Stash.UnstashAll();
                    UnbecomeStacked();
                    break;

                default:
                    Stash.Stash();
                    break;
            }
        }
    }

    /// <summary>
    /// Base class for a durable actor with index and state
    /// </summary>
    /// <typeparam name="TState"></typeparam>
    /// <typeparam name="TIndex"></typeparam>
    /// <example>
    /// // Xxx: Aggregate Root, Xxx.Event, Xxx.Command: Basis Klassen
    /// public class XxxClerk : DurableActor&lt;int, Xxx&gt;
    /// {
    ///     public XxxClerk(int id) : base(id)
    ///     {
    ///     // TODO: Subscribe auf alle Events für PersistenceId
    ///     }
    ///     
    ///     public DurableWithoutState(IActorRef eventStore): base(eventStore) {}
    ///
    ///     // overload if default implementation does not work for you
    ///     protected override void Apply(IEvent e) {}
    ///
    ///     // overload mostly always
    ///     protected override EventFilter BuildEventFilter() =>
    ///        WantEvents.AnyEvent();
    ///
    ///     // regular handling
    ///     protected override void Handle(object message)
    ///     {
    ///         switch (message)
    ///         {
    ///             // one example command handler
    ///             case DoSomething d:
    ///                 Persist(new Xxx.SomethingDone(42, "foo"));
    ///                 break;
    ///             
    ///             // handle all events
    ///             case Xxx.Event e:
    ///                 Apply(e);
    ///                 break;
    ///         }
    ///     }
    ///     
    ///     // overload to construct initial state
    ///     protected override Xxx BuildInitialState() =>
    ///         new Xxx(Id, "foo", "bar", 42);
    /// }
    /// </example>
    public abstract class DurableActor<TState> : DurableActor
        where TState : IState<TState>
    {
        public IState<TState> State;

        public DurableActor(IActorRef eventStore)
            : base(eventStore)
        {
            HasState = true;
            State = BuildInitialState();
            StateType = typeof(TState);
        }

        /// <summary>
        /// initially construct the state. default: construct new state
        /// </summary>
        /// <returns></returns>
        protected abstract TState BuildInitialState();

        /// <summary>
        /// apply the event to state. default: call state.Apply()
        /// </summary>
        /// <param name="event"></param>
        protected override void Apply(IEvent @event) =>
            State = State.Apply(@event);

        protected override void SaveSnapshot(Snapshot snapshot)
        {
            if (snapshot.State is TState state)
            {
                State = state;
                LastEventId = snapshot.LastEventId;
            }
        }
    }

    /// <summary>
    /// Base class for a durable actor with a State and an index
    /// </summary>
    /// <typeparam name="TIndex"></typeparam>
    public abstract class DurableActor<TState, TIndex> : DurableActor<TState>
        where TState : IState<TState>, new()
    {
        public TIndex Id;

        public DurableActor(IActorRef eventStore, TIndex id)
            : base(eventStore)
        {
            Id = id;
            PersistenceId = $"{this.GetType().Name}-{id}";
        }
    }
}
