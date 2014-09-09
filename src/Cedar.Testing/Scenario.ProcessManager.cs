namespace Cedar.Testing
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Reactive.Subjects;
    using System.Runtime.CompilerServices;
    using System.Threading.Tasks;
    using Cedar.ProcessManagers;

    public static partial class Scenario
    {
        public static ProcessManager.Given ForProcess<TProcess>(Guid correlationId, ProcessManager.ProcessManagerFactory factory = null, [CallerMemberName] string scenarioName = null)
            where TProcess : IProcessManager
        {
            return new ProcessManager.ScenarioBuilder<TProcess>(correlationId, factory, scenarioName);
        }

        public static class ProcessManager
        {
            public delegate IProcessManager ProcessManagerFactory(string id, Guid correlationId);


            public interface Given : When
            {
                When Given(params object[] events);
            }

            public interface When : Then
            {
                Then When(object @event);
            }

            public interface Then : IScenario
            {
                Then ThenCompletes();
                Then ThenNothingWasSent();
                Then Then(params object[] events);
            }

            internal class ScenarioBuilder<TProcess> : Given
                where TProcess: IProcessManager
            {
                private readonly Guid _correlationId;
                private readonly string _processId;
                private readonly string _name;
                private readonly ProcessManagerFactory _factory;
                
                private readonly Action<IProcessManager> _runGiven;
                private readonly Action<IProcessManager> _runWhen;
                
                private Action<IProcessManager> _checkCommands;
                private Action<IProcessManager> _checkEvents;
                private Action<IProcessManager> _runThen;

                private object[] _given;
                private object _when;
                private object[] _expectedCommands;
                private object[] _expectedEvents;

                private IEnumerable<object> expect
                {
                    get { return _expectedCommands.Union(_expectedEvents); }
                }

                private Exception _occurredException;
                private bool _passed;
                private readonly Stopwatch _timer;

                public ScenarioBuilder(Guid correlationId, ProcessManagerFactory factory, string name)
                {
                    _correlationId = correlationId;
                    _processId = typeof (TProcess).Name + "-" + correlationId;
                    _name = name;
                    _factory = factory ?? ((id, cid) => (TProcess) Activator.CreateInstance(typeof (TProcess), id, cid));
                    _given = new object[0];
                    _expectedCommands = new object[0];
                    _expectedEvents = new object[0];

                    _runGiven = process => _given.ForEach(process.ApplyEvent);

                    _runWhen = process =>
                    {
                        process.ClearUncommittedEvents();
                        process.ClearUndispatchedCommands();

                        process.ApplyEvent(_when);
                    };

                    _checkCommands = _ => { };
                    _checkEvents = _ => { };

                    _runThen = process =>
                    {
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

                    var process = _factory(
                        _processId,
                        _correlationId);

                    _runGiven(process);
                    
                    try
                    {
                        _runWhen(process);
                    }
                    catch (Exception ex)
                    {
                        _occurredException = ex;
                    }

                    _checkCommands(process);

                    _passed = true;

                    _timer.Stop();

                    return this;
                }

                public TaskAwaiter<ScenarioResult> GetAwaiter()
                {
                    IScenario scenario = this;

                    return scenario.Run().GetAwaiter();
                }

                public When Given(params object[] events)
                {
                    _given = events;
                    
                    return this;
                }

                public Then When(object @event)
                {
                    _when = @event;

                    return this;
                }

                public Then Then(params object[] commands)
                {
                    _expectedCommands = commands;
                    
                    _checkCommands = process =>
                    {
                        if (false == process.GetUndispatchedCommands()
                            .SequenceEqual(commands, MessageEqualityComparer.Instance))
                        {
                            throw new ScenarioException(this, "The ocurred commands did not equal the expected commands.");
                        }
                    };

                    return this;
                }

                public Then ThenNothingWasSent()
                {
                    _expectedCommands = new object[0];

                    _checkCommands = process =>
                    {
                        if (process.GetUndispatchedCommands().Any())
                        {
                            throw new ScenarioException(this, "No commands were expected, yet some commands occurred.");
                        }
                    };
                    return this;
                }

                public Then ThenCompletes()
                {
                    var events = _expectedEvents = new []{new ProcessCompleted{CorrelationId = _correlationId, ProcessId = _processId}};

                    _checkEvents = process =>
                    {
                        if (false == process.GetUndispatchedCommands()
                           .SequenceEqual(events, MessageEqualityComparer.Instance))
                        {
                            throw new ScenarioException(this, "The ocurred events did not equal the expected events.");
                        }
                    };

                    return this;
                }

                public static implicit operator ScenarioResult(ScenarioBuilder<TProcess> builder)
                {
                    return new ScenarioResult(builder._name, builder._passed, builder._given, builder._when, builder.expect, builder._timer.Elapsed, builder._occurredException);
                }
            }
        }
    }
}