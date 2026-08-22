namespace MiniDataProfiler.Mocks;

using System.Diagnostics;

internal sealed class ActivityRecorder : IDisposable
{
    private readonly ActivityListener listener;

    public List<Activity> Activities { get; } = [];

    public ActivityRecorder(params string[] sourceNames)
    {
        listener = new ActivityListener
        {
            ShouldListenTo = source => Array.IndexOf(sourceNames, source.Name) >= 0,
            Sample = (ref _) => ActivitySamplingResult.AllDataAndRecorded,
            ActivityStopped = activity => Activities.Add(activity)
        };

        ActivitySource.AddActivityListener(listener);
    }

    public void Dispose() => listener.Dispose();
}
