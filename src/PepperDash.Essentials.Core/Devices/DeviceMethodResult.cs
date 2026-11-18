// This file is also compiled into PepperDash.Essentials.Core.Tests, which enables nullable
// reference types. Core does not, so the annotations here would be read inconsistently between the
// two. Disabling explicitly keeps the behaviour identical in both, and is a no-op in Core.
#nullable disable

using Newtonsoft.Json;
using System;
using System.Globalization;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;

namespace PepperDash.Essentials.Core;

// No Crestron references by design, so the waiting and the result formatting for devjson and
// deviceCommands can be covered by PepperDash.Essentials.Core.Tests.

/// <summary>
/// What a device method called through devjson or deviceCommands returned: its value, the exception it threw,
/// or that it was still running when the caller stopped waiting.
/// </summary>
public class DeviceMethodResult
{
    /// <summary>
    /// How long devjson and deviceCommands wait for a method that returns a value
    /// </summary>
    public static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(2);

    /// <summary>
    /// True when the method finished, with a value or an error, before the timeout
    /// </summary>
    public bool Completed { get; private set; }

    /// <summary>
    /// The value the method returned, or a returned Task's result. Null for a plain Task.
    /// </summary>
    public object Value { get; private set; }

    /// <summary>
    /// The exception the method threw, if any
    /// </summary>
    public Exception Error { get; private set; }

    /// <summary>
    /// How long the caller waited for the method
    /// </summary>
    public TimeSpan Timeout { get; private set; }

    private DeviceMethodResult()
    {
    }

    /// <summary>
    /// Invokes a method and, when it returns a Task, awaits it and returns the Task's result (null for a plain Task).
    /// An exception the method throws is rethrown as itself, not wrapped in a TargetInvocationException.
    /// </summary>
    /// <param name="invoke">Calls the method, e.g. () => method.Invoke(obj, args)</param>
    /// <param name="returnType">The method's declared return type</param>
    /// <returns>The method's value</returns>
    public static async Task<object> InvokeAsync(Func<object> invoke, Type returnType)
    {
        object result;

        try
        {
            result = invoke();
        }
        catch (TargetInvocationException e) when (e.InnerException != null)
        {
            ExceptionDispatchInfo.Capture(e.InnerException).Throw();
            throw;
        }

        if (!(result is Task task))
        {
            return result;
        }

        await task.ConfigureAwait(false);

        // An async method declared as Task runs as a Task<VoidTaskResult>; only report a result when one was declared
        if (returnType == typeof(Task))
        {
            return null;
        }

        var genericTask = task.GetType();
        while (genericTask != null && !(genericTask.IsGenericType && genericTask.GetGenericTypeDefinition() == typeof(Task<>)))
        {
            genericTask = genericTask.BaseType;
        }

        return genericTask?.GetProperty("Result").GetValue(task);
    }

    /// <summary>
    /// Waits for a method started with <see cref="InvokeAsync"/>, for no longer than the timeout
    /// </summary>
    /// <param name="work">The running method</param>
    /// <param name="timeout">How long to wait</param>
    /// <returns>The method's value or error, or a result that isn't Completed if it's still running</returns>
    public static DeviceMethodResult Wait(Task<object> work, TimeSpan timeout)
    {
        try
        {
            if (!work.Wait(timeout))
            {
                return new DeviceMethodResult { Timeout = timeout };
            }

            return new DeviceMethodResult { Completed = true, Value = work.Result, Timeout = timeout };
        }
        catch (AggregateException e)
        {
            var error = e.InnerExceptions.Count == 1 ? e.InnerException : e;
            return new DeviceMethodResult { Completed = true, Error = error, Timeout = timeout };
        }
    }

    /// <summary>
    /// The line devjson prints after its success line:
    /// "Result: &lt;json&gt;", "Result: (still running after 2 s)" or "Result error: &lt;message&gt;"
    /// </summary>
    public string ToConsoleLine()
    {
        if (!Completed)
        {
            return String.Format("Result: ({0})", StillRunningMessage());
        }

        string error;
        var json = SerializeValue(out error);

        return error == null ? "Result: " + json : "Result error: " + error;
    }

    /// <summary>
    /// The body deviceCommands returns: { "result": &lt;json&gt; } or { "error": "&lt;message&gt;" }
    /// </summary>
    public string ToJson()
    {
        if (!Completed)
        {
            return JsonConvert.SerializeObject(new { error = StillRunningMessage() });
        }

        string error;
        var json = SerializeValue(out error);

        return error == null
            ? "{\"result\":" + json + "}"
            : JsonConvert.SerializeObject(new { error });
    }

    private string StillRunningMessage()
    {
        return String.Format("still running after {0} s", Timeout.TotalSeconds.ToString(CultureInfo.InvariantCulture));
    }

    private string SerializeValue(out string error)
    {
        if (Error != null)
        {
            error = Error.Message;
            return null;
        }

        try
        {
            error = null;
            return JsonConvert.SerializeObject(Value, Formatting.None);
        }
        catch (Exception e)
        {
            // e.g. a self-referencing object
            error = "Unable to serialize the result: " + e.Message;
            return null;
        }
    }
}
