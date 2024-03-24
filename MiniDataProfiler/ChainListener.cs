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
}
