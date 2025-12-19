using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using GameComposition.Core.Services.DI;
using GameComposition.Core.Types;
using Xunit;

namespace GameComposition.Core.Tests;

public sealed class WorkflowOrchestrationScenarioTests
{
    private sealed class CurrentUserContext
    {
        public CurrentUserContext(UserId userId)
        {
            UserId = userId;
        }

        public UserId UserId { get; }
    }

    private interface IWorkflowStep
    {
        ValueTask ExecuteAsync(ServiceRegistry registry, CancellationToken cancellationToken);
    }

    private sealed class WorkflowRunner
    {
        private readonly TimeSpan _stepTimeout;

        public WorkflowRunner(TimeSpan stepTimeout)
        {
            _stepTimeout = stepTimeout;
        }

        public async ValueTask RunAsync(ServiceRegistry registry, IWorkflowStep[] steps, CancellationToken cancellationToken)
        {
            foreach (var step in steps)
            {
                using var stepCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                stepCts.CancelAfter(_stepTimeout);

                var executeTask = step.ExecuteAsync(registry, stepCts.Token).AsTask();

                try
                {
                    await executeTask.ConfigureAwait(false);
                }
                catch (OperationCanceledException) when (stepCts.IsCancellationRequested)
                {
                    throw new TimeoutException($"Workflow step exceeded timeout of {_stepTimeout}.");
                }
            }
        }
    }

    private interface IAuditTrail
    {
        void Record(string message);
        string[] Snapshot();
    }

    private sealed class InMemoryAuditTrail : IAuditTrail
    {
        private readonly object _lock = new();
        private readonly System.Collections.Generic.List<string> _messages = new();

        public void Record(string message)
        {
            lock (_lock)
            {
                _messages.Add(message);
            }
        }

        public string[] Snapshot()
        {
            lock (_lock)
            {
                return _messages.ToArray();
            }
        }
    }

    private sealed class PlaceBuildingService
    {
        private readonly IAuditTrail _audit;

        public PlaceBuildingService(IAuditTrail audit)
        {
            _audit = audit;
        }

        public void Place(UserId userId)
        {
            _audit.Record($"placed:{userId}");
        }
    }

    private sealed class GrantOwnershipService
    {
        private readonly IAuditTrail _audit;

        public GrantOwnershipService(IAuditTrail audit)
        {
            _audit = audit;
        }

        public void Grant(UserId userId)
        {
            _audit.Record($"ownership:{userId}");
        }
    }

    private sealed class WorkflowPlaceBuildingStep : IWorkflowStep
    {
        public ValueTask ExecuteAsync(ServiceRegistry registry, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var userId = registry.GetService<CurrentUserContext>().UserId;
            registry.GetService<PlaceBuildingService>().Place(userId);

            return ValueTask.CompletedTask;
        }
    }

    private sealed class WorkflowGrantOwnershipStep : IWorkflowStep
    {
        public ValueTask ExecuteAsync(ServiceRegistry registry, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var userId = registry.GetService<CurrentUserContext>().UserId;
            registry.GetService<GrantOwnershipService>().Grant(userId);

            return ValueTask.CompletedTask;
        }
    }

    private sealed class NeverCompletingStep : IWorkflowStep
    {
        public async ValueTask ExecuteAsync(ServiceRegistry registry, CancellationToken cancellationToken)
        {
            await Task.Delay(Timeout.InfiniteTimeSpan, cancellationToken).ConfigureAwait(false);
        }
    }

    [Fact]
    public void ServiceOnly_Usage_DoesNotRequire_WorkflowAbstractions()
    {
        var audit = new InMemoryAuditTrail();

        using var registry = new ServiceRegistry();
        registry.RegisterSingleton<IAuditTrail>(audit);
        registry.RegisterSingleton(new CurrentUserContext(new UserId(Guid.Parse("11111111-1111-1111-1111-111111111111"))));
        registry.RegisterFactory(() => new PlaceBuildingService(registry.GetService<IAuditTrail>()));

        registry.GetService<PlaceBuildingService>().Place(registry.GetService<CurrentUserContext>().UserId);

        var snapshot = audit.Snapshot();
        Assert.Single(snapshot);
        Assert.Equal("placed:11111111-1111-1111-1111-111111111111", snapshot[0]);
    }

    [Fact]
    public async Task WorkflowOrchestration_IsOptional_And_CanCoordinate_MultipleServices_AcrossBoundaries()
    {
        var audit = new InMemoryAuditTrail();

        using var registry = new ServiceRegistry();
        registry.RegisterSingleton<IAuditTrail>(audit);
        registry.RegisterSingleton(new CurrentUserContext(new UserId(Guid.Parse("22222222-2222-2222-2222-222222222222"))));
        registry.RegisterFactory(() => new PlaceBuildingService(registry.GetService<IAuditTrail>()));
        registry.RegisterFactory(() => new GrantOwnershipService(registry.GetService<IAuditTrail>()));

        var runner = new WorkflowRunner(stepTimeout: TimeSpan.FromSeconds(1));

        await runner.RunAsync(
            registry,
            steps: new IWorkflowStep[]
            {
                new WorkflowPlaceBuildingStep(),
                new WorkflowGrantOwnershipStep(),
            },
            cancellationToken: CancellationToken.None);

        Assert.Equal(
            new[]
            {
                "placed:22222222-2222-2222-2222-222222222222",
                "ownership:22222222-2222-2222-2222-222222222222",
            },
            audit.Snapshot());
    }

    [Fact]
    public async Task HangGuard_WorkflowRunner_ReturnsControl_InUnder10Seconds_WhenStepNeverCompletes()
    {
        using var registry = new ServiceRegistry();
        var runner = new WorkflowRunner(stepTimeout: TimeSpan.FromMilliseconds(250));

        var sw = Stopwatch.StartNew();

        await Assert.ThrowsAsync<TimeoutException>(async () =>
            await runner.RunAsync(
                registry,
                steps: new IWorkflowStep[] { new NeverCompletingStep() },
                cancellationToken: CancellationToken.None));

        sw.Stop();

        Assert.True(sw.Elapsed < TimeSpan.FromSeconds(10), $"Expected hang guard < 10s, was {sw.Elapsed}.");
    }
}
