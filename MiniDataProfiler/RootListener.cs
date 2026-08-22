namespace MiniDataProfiler;

using System.Data.Common;
using System.Diagnostics;

internal sealed class RootListener : IProfileListener
{
    private readonly IProfileListener listener;

    private RootListener(IProfileListener listener)
    {
        this.listener = listener;
    }

    public static IProfileListener Wrap(IProfileListener listener) =>
        listener is RootListener ? listener : new RootListener(listener);

    private void ReportError(string method, Exception ex) =>
        Trace.TraceError("Profile listener error. type=[{0}], method=[{1}], exception=[{2}]", listener.GetType().Name, method, ex);

    public void NonQueryExecuting(in ProfilerExecutingContext context)
    {
#pragma warning disable CA1031
        try
        {
            listener.NonQueryExecuting(in context);
        }
        catch (Exception ex)
        {
            ReportError(nameof(NonQueryExecuting), ex);
        }
#pragma warning restore CA1031
    }

    public void NonQueryExecuted(in ProfilerExecutedContext<int> context)
    {
#pragma warning disable CA1031
        try
        {
            listener.NonQueryExecuted(in context);
        }
        catch (Exception ex)
        {
            ReportError(nameof(NonQueryExecuted), ex);
        }
#pragma warning restore CA1031
    }

    public void ScalarExecuting(in ProfilerExecutingContext context)
    {
#pragma warning disable CA1031
        try
        {
            listener.ScalarExecuting(in context);
        }
        catch (Exception ex)
        {
            ReportError(nameof(ScalarExecuting), ex);
        }
#pragma warning restore CA1031
    }

    public void ScalarExecuted(in ProfilerExecutedContext<object?> context)
    {
#pragma warning disable CA1031
        try
        {
            listener.ScalarExecuted(in context);
        }
        catch (Exception ex)
        {
            ReportError(nameof(ScalarExecuted), ex);
        }
#pragma warning restore CA1031
    }

    public void ReaderExecuting(in ProfilerExecutingContext context)
    {
#pragma warning disable CA1031
        try
        {
            listener.ReaderExecuting(in context);
        }
        catch (Exception ex)
        {
            ReportError(nameof(ReaderExecuting), ex);
        }
#pragma warning restore CA1031
    }

    public void ReaderExecuted(in ProfilerExecutedContext<DbDataReader> context)
    {
#pragma warning disable CA1031
        try
        {
            listener.ReaderExecuted(in context);
        }
        catch (Exception ex)
        {
            ReportError(nameof(ReaderExecuted), ex);
        }
#pragma warning restore CA1031
    }

    public void CommandFailed(in ProfilerFailedContext context)
    {
#pragma warning disable CA1031
        try
        {
            listener.CommandFailed(in context);
        }
        catch (Exception ex)
        {
            ReportError(nameof(CommandFailed), ex);
        }
#pragma warning restore CA1031
    }

    public void CommandFinally(in ProfilerFinallyContext context)
    {
#pragma warning disable CA1031
        try
        {
            listener.CommandFinally(in context);
        }
        catch (Exception ex)
        {
            ReportError(nameof(CommandFinally), ex);
        }
#pragma warning restore CA1031
    }

    public void ReaderFinished(in ProfilerReaderFinishedContext context)
    {
#pragma warning disable CA1031
        try
        {
            listener.ReaderFinished(in context);
        }
        catch (Exception ex)
        {
            ReportError(nameof(ReaderFinished), ex);
        }
#pragma warning restore CA1031
    }

    public void BatchNonQueryExecuting(in BatchProfilerExecutingContext context)
    {
#pragma warning disable CA1031
        try
        {
            listener.BatchNonQueryExecuting(in context);
        }
        catch (Exception ex)
        {
            ReportError(nameof(BatchNonQueryExecuting), ex);
        }
#pragma warning restore CA1031
    }

    public void BatchNonQueryExecuted(in BatchProfilerExecutedContext<int> context)
    {
#pragma warning disable CA1031
        try
        {
            listener.BatchNonQueryExecuted(in context);
        }
        catch (Exception ex)
        {
            ReportError(nameof(BatchNonQueryExecuted), ex);
        }
#pragma warning restore CA1031
    }

    public void BatchReaderExecuting(in BatchProfilerExecutingContext context)
    {
#pragma warning disable CA1031
        try
        {
            listener.BatchReaderExecuting(in context);
        }
        catch (Exception ex)
        {
            ReportError(nameof(BatchReaderExecuting), ex);
        }
#pragma warning restore CA1031
    }

    public void BatchReaderExecuted(in BatchProfilerExecutedContext<DbDataReader> context)
    {
#pragma warning disable CA1031
        try
        {
            listener.BatchReaderExecuted(in context);
        }
        catch (Exception ex)
        {
            ReportError(nameof(BatchReaderExecuted), ex);
        }
#pragma warning restore CA1031
    }

    public void BatchFailed(in BatchProfilerFailedContext context)
    {
#pragma warning disable CA1031
        try
        {
            listener.BatchFailed(in context);
        }
        catch (Exception ex)
        {
            ReportError(nameof(BatchFailed), ex);
        }
#pragma warning restore CA1031
    }

    public void BatchFinally(in BatchProfilerFinallyContext context)
    {
#pragma warning disable CA1031
        try
        {
            listener.BatchFinally(in context);
        }
        catch (Exception ex)
        {
            ReportError(nameof(BatchFinally), ex);
        }
#pragma warning restore CA1031
    }

    public void BatchReaderFinished(in BatchProfilerReaderFinishedContext context)
    {
#pragma warning disable CA1031
        try
        {
            listener.BatchReaderFinished(in context);
        }
        catch (Exception ex)
        {
            ReportError(nameof(BatchReaderFinished), ex);
        }
#pragma warning restore CA1031
    }
}
