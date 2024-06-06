namespace dotnet.nugit.Services.Workspace
{
    using System;
    using Microsoft.Build.Framework;
    using Microsoft.Extensions.Logging;
    using ILogger = Microsoft.Build.Framework.ILogger;

    internal sealed class ProjectWorkspaceLogger(
        ILogger<ProjectWorkspaceLogger> logger) : ILogger
    {
        private readonly ILogger<ProjectWorkspaceLogger> logger = logger ?? throw new ArgumentNullException(nameof(logger));
        private IEventSource events = null!;

        public void Initialize(IEventSource eventSource)
        {
            this.events = eventSource;
            this.events.ErrorRaised += this.HandleErrorRaised;
            this.events.AnyEventRaised += this.HandleAnyEvent;
        }

        public void Shutdown()
        {
            this.events.ErrorRaised -= this.HandleErrorRaised;
            this.events.AnyEventRaised -= this.HandleAnyEvent;
        }

        public LoggerVerbosity Verbosity { get; set; }
        public string? Parameters { get; set; }

        private void HandleAnyEvent(object sender, BuildEventArgs? e)
        {
            if (e != null) this.logger.LogInformation(e.Message);
        }

        private void HandleErrorRaised(object sender, BuildErrorEventArgs? e)
        {
            var message = e?.ToString();
            if (string.IsNullOrWhiteSpace(message)) return;
            this.logger.LogError(message);
        }
    }
}