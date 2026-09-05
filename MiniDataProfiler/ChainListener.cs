namespace MiniDataProfiler;

using System.Data.Common;
using System.Diagnostics;

public sealed class ChainListener : IProfileListener
{
    private readonly IProfileListener[] listeners;

    public ChainListener(params IProfileListener[] listeners)
    {
        this.listeners = listeners;
    }

    public ChainListener(IEnumerable<IProfileListener> listeners)
    {
        this.listeners = [.. listeners];
    }

    private static void ReportError(IProfileListener listener, string method, Exception ex) =>
        Trace.TraceError("Profile listener error. type=[{0}], method=[{1}], exception=[{2}]", listener.GetType().Name, method, ex);

    public void NonQueryExecuting(in ProfilerExecutingContext context)
    {
        foreach (var listener in listeners)
        {
#pragma warning disable CA1031
            try
            {
                listener.NonQueryExecuting(in context);
            }
            catch (Exception ex)
            {
                ReportError(listener, nameof(NonQueryExecuting), ex);
            }
#pragma warning restore CA1031
        }
    }

    public void NonQueryExecuted(in ProfilerExecutedContext<int> context)
    {
        foreach (var listener in listeners)
        {
#pragma warning disable CA1031
            try
            {
                listener.NonQueryExecuted(in context);
            }
            catch (Exception ex)
            {
                ReportError(listener, nameof(NonQueryExecuted), ex);
            }
#pragma warning restore CA1031
        }
    }

    public void ScalarExecuting(in ProfilerExecutingContext context)
    {
        foreach (var listener in listeners)
        {
#pragma warning disable CA1031
            try
            {
                listener.ScalarExecuting(in context);
            }
            catch (Exception ex)
            {
                ReportError(listener, nameof(ScalarExecuting), ex);
            }
#pragma warning restore CA1031
        }
    }

    public void ScalarExecuted(in ProfilerExecutedContext<object?> context)
    {
        foreach (var listener in listeners)
        {
#pragma warning disable CA1031
            try
            {
                listener.ScalarExecuted(in context);
            }
            catch (Exception ex)
            {
                ReportError(listener, nameof(ScalarExecuted), ex);
            }
#pragma warning restore CA1031
        }
    }

    public void ReaderExecuting(in ProfilerExecutingContext context)
    {
        foreach (var listener in listeners)
        {
#pragma warning disable CA1031
            try
            {
                listener.ReaderExecuting(in context);
            }
            catch (Exception ex)
            {
                ReportError(listener, nameof(ReaderExecuting), ex);
            }
#pragma warning restore CA1031
        }
    }

    public void ReaderExecuted(in ProfilerExecutedContext<DbDataReader> context)
    {
        foreach (var listener in listeners)
        {
#pragma warning disable CA1031
            try
            {
                listener.ReaderExecuted(in context);
            }
            catch (Exception ex)
            {
                ReportError(listener, nameof(ReaderExecuted), ex);
            }
#pragma warning restore CA1031
        }
    }

    public void CommandFailed(in ProfilerFailedContext context)
    {
        foreach (var listener in listeners)
        {
#pragma warning disable CA1031
            try
            {
                listener.CommandFailed(in context);
            }
            catch (Exception ex)
            {
                ReportError(listener, nameof(CommandFailed), ex);
            }
#pragma warning restore CA1031
        }
    }

    public void CommandFinally(in ProfilerFinallyContext context)
    {
        foreach (var listener in listeners)
        {
#pragma warning disable CA1031
            try
            {
                listener.CommandFinally(in context);
            }
            catch (Exception ex)
            {
                ReportError(listener, nameof(CommandFinally), ex);
            }
#pragma warning restore CA1031
        }
    }

    public void ReaderFinished(in ProfilerReaderFinishedContext context)
    {
        foreach (var listener in listeners)
        {
#pragma warning disable CA1031
            try
            {
                listener.ReaderFinished(in context);
            }
            catch (Exception ex)
            {
                ReportError(listener, nameof(ReaderFinished), ex);
            }
#pragma warning restore CA1031
        }
    }

    public void BatchNonQueryExecuting(in BatchProfilerExecutingContext context)
    {
        foreach (var listener in listeners)
        {
#pragma warning disable CA1031
            try
            {
                listener.BatchNonQueryExecuting(in context);
            }
            catch (Exception ex)
            {
                ReportError(listener, nameof(BatchNonQueryExecuting), ex);
            }
#pragma warning restore CA1031
        }
    }

    public void BatchNonQueryExecuted(in BatchProfilerExecutedContext<int> context)
    {
        foreach (var listener in listeners)
        {
#pragma warning disable CA1031
            try
            {
                listener.BatchNonQueryExecuted(in context);
            }
            catch (Exception ex)
            {
                ReportError(listener, nameof(BatchNonQueryExecuted), ex);
            }
#pragma warning restore CA1031
        }
    }

    public void BatchReaderExecuting(in BatchProfilerExecutingContext context)
    {
        foreach (var listener in listeners)
        {
#pragma warning disable CA1031
            try
            {
                listener.BatchReaderExecuting(in context);
            }
            catch (Exception ex)
            {
                ReportError(listener, nameof(BatchReaderExecuting), ex);
            }
#pragma warning restore CA1031
        }
    }

    public void BatchReaderExecuted(in BatchProfilerExecutedContext<DbDataReader> context)
    {
        foreach (var listener in listeners)
        {
#pragma warning disable CA1031
            try
            {
                listener.BatchReaderExecuted(in context);
            }
            catch (Exception ex)
            {
                ReportError(listener, nameof(BatchReaderExecuted), ex);
            }
#pragma warning restore CA1031
        }
    }

    public void BatchFailed(in BatchProfilerFailedContext context)
    {
        foreach (var listener in listeners)
        {
#pragma warning disable CA1031
            try
            {
                listener.BatchFailed(in context);
            }
            catch (Exception ex)
            {
                ReportError(listener, nameof(BatchFailed), ex);
            }
#pragma warning restore CA1031
        }
    }

    public void BatchFinally(in BatchProfilerFinallyContext context)
    {
        foreach (var listener in listeners)
        {
#pragma warning disable CA1031
            try
            {
                listener.BatchFinally(in context);
            }
            catch (Exception ex)
            {
                ReportError(listener, nameof(BatchFinally), ex);
            }
#pragma warning restore CA1031
        }
    }

    public void BatchReaderFinished(in BatchProfilerReaderFinishedContext context)
    {
        foreach (var listener in listeners)
        {
#pragma warning disable CA1031
            try
            {
                listener.BatchReaderFinished(in context);
            }
            catch (Exception ex)
            {
                ReportError(listener, nameof(BatchReaderFinished), ex);
            }
#pragma warning restore CA1031
        }
    }
}
