namespace Cedar.Testing
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Text;
    using System.Threading.Tasks;
    using Cedar.ProcessManagers;
    using Cedar.ProcessManagers.Messages;
    using Cedar.ProcessManagers.Persistence;

    public static partial class Scenario
    {
        public static ProcessManager.IGiven ForProcess<TProcess>(
            IProcessManagerFactory factory = null,
            string processId = null,
            [CallerMemberName] string scenarioName = null)
            where TProcess : IProcessManager
        {
            return new ProcessManager.ScenarioBuilder<TProcess>(factory, processId, scenarioName);
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

            internal class ScenarioBuilder<TProcess> : IGiven
                where TProcess: IProcessManager
            {
                private readonly string _processId;
                private readonly string _correlationId;
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

                public ScenarioBuilder(IProcessManagerFactory factory, string correlationId, string name)
                {
                    _processId = typeof(TProcess) + "-" + correlationId;
                    _correlationId = correlationId;
                    _name = name;
                    _factory = factory ?? new DefaultProcessManagerFactory();
                    _given = new object[0];
                    _expectedCommands = new object[0];
                    _expectedEvents = new object[0];

                    _runGiven = process =>
                    {
                        foreach(var message in _given)
                        {
                            process.Inbox.OnNext(message);
                        }
                    };

                    _runWhen = process =>
                    {
                        process.Inbox.OnNext(new CheckpointReached());

                        process.Inbox.OnNext(_when);
                    };

                    _checkCommands = _ => { };
                    _checkEvents = _ => { };

                    _runThen = process =>
                    {
                        var events = new List<object>();
                        var commands = new List<object>();

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
                        var process = _factory.Build(typeof(TProcess), _processId, _correlationId);

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
                        /*if (false == process.GetUndispatchedCommands()
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
                        }*/
                    };

                    return this;
                }

                public IThen ThenNothingWasSent()
                {
                    _expectedCommands = new object[0];

                    _checkCommands = process =>
                    {
                        /*if (process.GetUndispatchedCommands().Any())
                        {
                            throw new ScenarioException("No commands were expected, yet some commands occurred.");
                        }*/
                    };
                    return this;
                }

                public IThen ThenCompletes()
                {
                    var events = _expectedEvents = new []{new ProcessCompleted{ProcessId = _processId}};

                    _checkEvents = process =>
                    {
                        /*if (false == process.GetUndispatchedCommands()
                           .SequenceEqual(events, MessageEqualityComparer.Instance))
                        {
                            throw new ScenarioException("The ocurred events did not equal the expected events.");
                        }*/
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