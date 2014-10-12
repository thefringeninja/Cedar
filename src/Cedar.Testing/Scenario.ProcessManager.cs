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
    using NEventStore;
    using NEventStore.Persistence;

    public static partial class Scenario
    {
        public static ProcessManager.IGiven ForProcess<TProcess>(Guid correlationId, ProcessManager.ProcessManagerFactory factory = null, [CallerMemberName] string scenarioName = null)
            where TProcess : IProcessManager
        {
            return new ProcessManager.ScenarioBuilder<TProcess>(correlationId, factory, scenarioName);
        }

        public static class ProcessManager
        {
            public delegate IProcessManager ProcessManagerFactory(string id);

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

            internal class ScenarioBuilder<TProcess> : IGiven
                where TProcess: IProcessManager
            {
                private readonly Guid _correlationId;
                private readonly string _processId;
                private readonly string _name;
                private readonly ProcessManagerFactory _factory;
                
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

                private static dynamic CreateDomainEvent(object @event)
                {
                    return Activator.CreateInstance(
                        typeof(DomainEventMessage<>).MakeGenericType(@event.GetType()),
                        new Commit("bucket", "stream", 1, Guid.NewGuid(), 0, DateTime.UtcNow,
                            null, new Dictionary<string, object>(), new[] {new EventMessage {Body = @event}}),
                        1,
                        new Dictionary<string, object>(),
                        @event);
                }

                public ScenarioBuilder(Guid correlationId, ProcessManagerFactory factory, string name)
                {
                    _correlationId = correlationId;
                    _processId = typeof (TProcess).Name + "-" + correlationId;
                    _name = name;
                    _factory = factory ?? (id => (TProcess) Activator.CreateInstance(typeof (TProcess), id));
                    _given = new object[0];
                    _expectedCommands = new object[0];
                    _expectedEvents = new object[0];

                    _runGiven = process =>
                    {
                        foreach(var message in _given.Select(CreateDomainEvent))
                        {
                            process.ApplyEvent(message);
                        }
                    };

                    _runWhen = process =>
                    {
                        process.ClearUncommittedEvents();
                        process.ClearUndispatchedCommands();

                        process.ApplyEvent(CreateDomainEvent(_when));
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
                        var process = _factory(_processId);

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