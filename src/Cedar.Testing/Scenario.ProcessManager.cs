namespace Cedar.Testing
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Text;
    using System.Threading.Tasks;
    using Cedar.Handlers;
    using Cedar.ProcessManagers;
    using Cedar.ProcessManagers.Persistence;
    using NEventStore;

    public static partial class Scenario
    {
        public static ProcessManager.IGiven ForProcess<TProcess>(IProcessManagerFactory factory = null, Func<object, ICommit> buildCommit = null,[CallerMemberName] string scenarioName = null)
            where TProcess : IProcessManager
        {
            return new ProcessManager.ScenarioBuilder<TProcess>(factory, buildCommit, scenarioName);
        }

        public static class ProcessManager
        {
            public interface IGiven : IWhen
            {
                IWhen Given(params object[] events);
            }

            public interface IWhen : IThen
            {
                IThen When(object @event);
            }

            public interface IThen : IScenario
            {
                IThen ThenCompletes();

                IThen ThenNothingWasSent();

                IThen Then(params object[] events);
            }

            internal class SimpleCommit : ICommit
            {
                public string BucketId { get; private set; }
                public string StreamId { get; private set; }
                public int StreamRevision { get; private set; }
                public Guid CommitId { get; private set; }
                public int CommitSequence { get; private set; }
                public DateTime CommitStamp { get; private set; }
                public IDictionary<string, object> Headers { get; private set; }
                public ICollection<EventMessage> Events { get; private set; }
                public string CheckpointToken { get; private set; }

                public SimpleCommit(string streamId, object @event)
                {
                    StreamId = streamId;
                    Events = new List<EventMessage>
                {
                    new EventMessage {Body = @event}
                };
                    Headers = new Dictionary<string, object>();
                }
            }

            internal class ScenarioBuilder<TProcess> : IGiven
                where TProcess: IProcessManager
            {
                private readonly string _processId;
                private readonly string _name;
                private readonly IProcessManagerFactory _factory;
                
                private readonly Action<IProcessManager> _runGiven;
                private readonly Action<IProcessManager> _runWhen;
                private readonly Action<IProcessManager> _runThen;
                
                private Action<IProcessManager> _checkCommands;
                private Action<IProcessManager> _checkEvents;

                private object[] _given;
                private object _when;
                private object[] _expectedCommands;
                private object[] _expectedEvents;
                private object _results;

                private IEnumerable<object> Expect
                {
                    get { return _expectedCommands.Union(_expectedEvents); }
                }

                private bool _passed;
                private readonly Stopwatch _timer;

                private static dynamic CreateDomainEvent(object @event, Func<object, ICommit> buildCommit)
                {
                    return Activator.CreateInstance(
                        typeof(DomainEventMessage<>).MakeGenericType(@event.GetType()),
                        buildCommit(@event),
                        1,
                        new Dictionary<string, object>(),
                        @event);
                }

                public ScenarioBuilder(IProcessManagerFactory factory, Func<object, ICommit> buildCommit, string name)
                {
                    _processId = typeof (TProcess).Name + "-" + Guid.NewGuid();
                    buildCommit = buildCommit ?? (e => new SimpleCommit("stream", e));
                    _name = name;
                    _factory = factory ?? new DefaultProcessManagerFactory();
                    _given = new object[0];
                    _expectedCommands = new object[0];
                    _expectedEvents = new object[0];

                    _runGiven = process =>
                    {
                        foreach(var message in _given.Select(e => CreateDomainEvent(e, buildCommit)))
                        {
                            process.ApplyEvent(message);
                        }
                    };

                    _runWhen = process =>
                    {
                        process.ClearUncommittedEvents();
                        process.ClearUndispatchedCommands();

                        process.ApplyEvent(CreateDomainEvent(_when, buildCommit));
                    };

                    _checkCommands = _ => { };
                    _checkEvents = _ => { };

                    _runThen = process =>
                    {
                        _results = process.GetUndispatchedCommands()
                            .Union(process.GetUncommittedEvents());

                        _checkCommands(process);
                        _checkEvents(process);
                    };

                    _timer = new Stopwatch();
                }

                string IScenario.Name
                {
                    get { return _name; }
                }

                public async Task<ScenarioResult> Run()
                {
                    _timer.Start();

                    try
                    {
                        var process = _factory.Build(typeof(TProcess), _processId);

                        _runGiven(process);

                        try
                        {
                            _runWhen(process);
                        }
                        catch (Exception ex)
                        {
                            _results = ex;

                            return this;
                        }

                        _runThen(process);

                        _passed = true;
                    }
                    catch (Exception ex)
                    {
                        _results = ex;

                        return this;
                    }

                    _timer.Stop();

                    return this;
                }

                public TaskAwaiter<ScenarioResult> GetAwaiter()
                {
                    IScenario scenario = this;

                    return scenario.Run().GetAwaiter();
                }

                public IWhen Given(params object[] events)
                {
                    _given = events;
                    
                    return this;
                }

                public IThen When(object @event)
                {
                    _when = @event;

                    return this;
                }

                public IThen Then(params object[] commands)
                {
                    _expectedCommands = commands;
                    
                    _checkCommands = process =>
                    {
                        if (false == process.GetUndispatchedCommands()
                            .SequenceEqual(commands, MessageEqualityComparer.Instance))
                        {
                            throw new ScenarioException(
                                string.Format(
                                    "The ocurred commands ({0}) did not equal the expected commands ({1}).",
                                    process.GetUndispatchedCommands()
                                        .Aggregate(new StringBuilder(), (builder, s) => builder.Append(s))
                                        .ToString(),
                                    _expectedCommands.Aggregate(new StringBuilder(), (builder, s) => builder.Append(s))
                                        .ToString()));
                        }
                    };

                    return this;
                }

                public IThen ThenNothingWasSent()
                {
                    _expectedCommands = new object[0];

                    _checkCommands = process =>
                    {
                        if (process.GetUndispatchedCommands().Any())
                        {
                            throw new ScenarioException("No commands were expected, yet some commands occurred.");
                        }
                    };
                    return this;
                }

                public IThen ThenCompletes()
                {
                    var events = _expectedEvents = new []{new ProcessCompleted{ProcessId = _processId}};

                    _checkEvents = process =>
                    {
                        if (false == process.GetUndispatchedCommands()
                           .SequenceEqual(events, MessageEqualityComparer.Instance))
                        {
                            throw new ScenarioException("The ocurred events did not equal the expected events.");
                        }
                    };

                    return this;
                }

                public static implicit operator ScenarioResult(ScenarioBuilder<TProcess> builder)
                {
                    return new ScenarioResult(builder._name, builder._passed, builder._given, builder._when, builder.Expect, builder._results, builder._timer.Elapsed);
                }
            }
        }
    }
}