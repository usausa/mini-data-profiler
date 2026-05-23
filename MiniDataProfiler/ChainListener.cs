namespace MiniDataProfiler;

using System.Data.Common;

public sealed class ChainListener : IProfileListener
{
    private readonly IProfileListener[] listeners;

    public ChainListener(params IProfileListener[] listeners)
    {
        this.listeners = listeners;
    }

    public void NonQueryExecuting(in ProfilerExecutingContext context)
    {
        foreach (var listener in listeners)
        {
            // ReSharper disable once EmptyGeneralCatchClause
#pragma warning disable CA1031
            try
            {
                listener.NonQueryExecuting(in context);
            }
            catch (Exception)
            {
            }
#pragma warning restore CA1031
        }
    }

    public void NonQueryExecuted(in ProfilerExecutedContext<int> context)
    {
        foreach (var listener in listeners)
        {
            // ReSharper disable once EmptyGeneralCatchClause
#pragma warning disable CA1031
            try
            {
                listener.NonQueryExecuted(in context);
            }
            catch (Exception)
            {
            }
#pragma warning restore CA1031
        }
    }

    public void ScalarExecuting(in ProfilerExecutingContext context)
    {
        foreach (var listener in listeners)
        {
            // ReSharper disable once EmptyGeneralCatchClause
#pragma warning disable CA1031
            try
            {
                listener.ScalarExecuting(in context);
            }
            catch (Exception)
            {
            }
#pragma warning restore CA1031
        }
    }

    public void ScalarExecuted(in ProfilerExecutedContext<object?> context)
    {
        foreach (var listener in listeners)
        {
            // ReSharper disable once EmptyGeneralCatchClause
#pragma warning disable CA1031
            try
            {
                listener.ScalarExecuted(in context);
            }
            catch (Exception)
            {
            }
#pragma warning restore CA1031
        }
    }

    public void ReaderExecuting(in ProfilerExecutingContext context)
    {
        foreach (var listener in listeners)
        {
            // ReSharper disable once EmptyGeneralCatchClause
#pragma warning disable CA1031
            try
            {
                listener.ReaderExecuting(in context);
            }
            catch (Exception)
            {
            }
#pragma warning restore CA1031
        }
    }

    public void ReaderExecuted(in ProfilerExecutedContext<DbDataReader> context)
    {
        foreach (var listener in listeners)
        {
            // ReSharper disable once EmptyGeneralCatchClause
#pragma warning disable CA1031
            try
            {
                listener.ReaderExecuted(in context);
            }
            catch (Exception)
            {
            }
#pragma warning restore CA1031
        }
    }

    public void CommandFailed(in ProfilerFailedContext context)
    {
        foreach (var listener in listeners)
        {
            // ReSharper disable once EmptyGeneralCatchClause
#pragma warning disable CA1031
            try
            {
                listener.CommandFailed(in context);
            }
            catch (Exception)
            {
            }
#pragma warning restore CA1031
        }
    }

    public void CommandFinally(in ProfilerFinallyContext context)
    {
        foreach (var listener in listeners)
        {
            // ReSharper disable once EmptyGeneralCatchClause
#pragma warning disable CA1031
            try
            {
                listener.CommandFinally(in context);
            }
            catch (Exception)
            {
            }
#pragma warning restore CA1031
        }
    }

    public void BatchNonQueryExecuting(in BatchProfilerExecutingContext context)
    {
        foreach (var listener in listeners)
        {
            // ReSharper disable once EmptyGeneralCatchClause
#pragma warning disable CA1031
            try
            {
                listener.BatchNonQueryExecuting(in context);
            }
            catch (Exception)
            {
            }
#pragma warning restore CA1031
        }
    }

    public void BatchNonQueryExecuted(in BatchProfilerExecutedContext<int> context)
    {
        foreach (var listener in listeners)
        {
            // ReSharper disable once EmptyGeneralCatchClause
#pragma warning disable CA1031
            try
            {
                listener.BatchNonQueryExecuted(in context);
            }
            catch (Exception)
            {
            }
#pragma warning restore CA1031
        }
    }

    public void BatchReaderExecuting(in BatchProfilerExecutingContext context)
    {
        foreach (var listener in listeners)
        {
            // ReSharper disable once EmptyGeneralCatchClause
#pragma warning disable CA1031
            try
            {
                listener.BatchReaderExecuting(in context);
            }
            catch (Exception)
            {
            }
#pragma warning restore CA1031
        }
    }

    public void BatchReaderExecuted(in BatchProfilerExecutedContext<DbDataReader> context)
    {
        foreach (var listener in listeners)
        {
            // ReSharper disable once EmptyGeneralCatchClause
#pragma warning disable CA1031
            try
            {
                listener.BatchReaderExecuted(in context);
            }
            catch (Exception)
            {
            }
#pragma warning restore CA1031
        }
    }

    public void BatchFailed(in BatchProfilerFailedContext context)
    {
        foreach (var listener in listeners)
        {
            // ReSharper disable once EmptyGeneralCatchClause
#pragma warning disable CA1031
            try
            {
                listener.BatchFailed(in context);
            }
            catch (Exception)
            {
            }
#pragma warning restore CA1031
        }
    }

    public void BatchFinally(in BatchProfilerFinallyContext context)
    {
        foreach (var listener in listeners)
        {
            // ReSharper disable once EmptyGeneralCatchClause
#pragma warning disable CA1031
            try
            {
                listener.BatchFinally(in context);
            }
            catch (Exception)
            {
            }
#pragma warning restore CA1031
        }
    }
}
