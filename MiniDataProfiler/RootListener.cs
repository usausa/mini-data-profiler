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

    private void ReportError(string method, Exception e) =>
        Trace.TraceError("Profile listener error. type=[{0}], method=[{1}], exception=[{2}]", listener.GetType().Name, method, e);

    public void NonQueryExecuting(in ProfilerExecutingContext context)
    {
#pragma warning disable CA1031
        try
        {
            listener.NonQueryExecuting(in context);
        }
        catch (Exception e)
        {
            ReportError(nameof(NonQueryExecuting), e);
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
        catch (Exception e)
        {
            ReportError(nameof(NonQueryExecuted), e);
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
        catch (Exception e)
        {
            ReportError(nameof(ScalarExecuting), e);
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
        catch (Exception e)
        {
            ReportError(nameof(ScalarExecuted), e);
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
        catch (Exception e)
        {
            ReportError(nameof(ReaderExecuting), e);
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
        catch (Exception e)
        {
            ReportError(nameof(ReaderExecuted), e);
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
        catch (Exception e)
        {
            ReportError(nameof(CommandFailed), e);
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
        catch (Exception e)
        {
            ReportError(nameof(CommandFinally), e);
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
        catch (Exception e)
        {
            ReportError(nameof(BatchNonQueryExecuting), e);
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
        catch (Exception e)
        {
            ReportError(nameof(BatchNonQueryExecuted), e);
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
        catch (Exception e)
        {
            ReportError(nameof(BatchReaderExecuting), e);
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
        catch (Exception e)
        {
            ReportError(nameof(BatchReaderExecuted), e);
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
        catch (Exception e)
        {
            ReportError(nameof(BatchFailed), e);
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
        catch (Exception e)
        {
            ReportError(nameof(BatchFinally), e);
        }
#pragma warning restore CA1031
    }
}
