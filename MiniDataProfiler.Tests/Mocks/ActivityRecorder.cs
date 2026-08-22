namespace MiniDataProfiler.Mocks;

using System.Diagnostics;

internal sealed class ActivityRecorder : IDisposable
{
    private readonly ActivityListener listener;

    public List<Activity> Activities { get; } = [];

    public ActivityRecorder(string sourceName)
    {
        listener = new ActivityListener
        {
            ShouldListenTo = source => source.Name == sourceName,
            Sample = (ref _) => ActivitySamplingResult.AllDataAndRecorded,
            ActivityStopped = activity => Activities.Add(activity)
        };

        ActivitySource.AddActivityListener(listener);
    }

    public void Dispose() => listener.Dispose();
}
