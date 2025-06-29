using System.Diagnostics;

namespace BankingProject.Application.Extensions;

/// <summary>
/// Extension methods for working with Activity and distributed tracing
/// </summary>
public static class ActivityExtensions
{
    /// <summary>
    /// Adds an exception to the activity with proper tags
    /// </summary>
    /// <param name="activity">The activity to add the exception to</param>
    /// <param name="exception">The exception to add</param>
    public static void AddException(this Activity? activity, Exception exception)
    {
        if (activity == null) return;
        
        activity.SetTag("exception.type", exception.GetType().Name);
        activity.SetTag("exception.message", exception.Message);
        activity.SetTag("exception.stacktrace", exception.StackTrace);
        
        if (exception.InnerException != null)
        {
            activity.SetTag("exception.inner.type", exception.InnerException.GetType().Name);
            activity.SetTag("exception.inner.message", exception.InnerException.Message);
        }
    }

    /// <summary>
    /// Sets the activity status to Ok with a success message
    /// </summary>
    /// <param name="activity">The activity to update</param>
    /// <param name="message">The success message</param>
    public static void SetSuccess(this Activity? activity, string message)
    {
        activity?.SetStatus(ActivityStatusCode.Ok, message);
    }

    /// <summary>
    /// Sets the activity status to Error with the exception details
    /// </summary>
    /// <param name="activity">The activity to update</param>
    /// <param name="exception">The exception that occurred</param>
    public static void SetError(this Activity? activity, Exception exception)
    {
        if (activity == null) return;
        
        activity.SetStatus(ActivityStatusCode.Error, exception.Message);
        activity.AddException(exception);
    }

    /// <summary>
    /// Sets common tags for customer operations
    /// </summary>
    /// <param name="activity">The activity to update</param>
    /// <param name="customerId">The customer ID</param>
    /// <param name="operation">The operation name</param>
    public static void SetCustomerTags(this Activity? activity, Guid customerId, string operation)
    {
        if (activity == null) return;
        
        activity.SetTag("customer.id", customerId.ToString());
        activity.SetTag("operation", operation);
    }

    /// <summary>
    /// Sets common tags for balance operation operations
    /// </summary>
    /// <param name="activity">The activity to update</param>
    /// <param name="operationId">The balance operation ID</param>
    /// <param name="operation">The operation name</param>
    public static void SetBalanceOperationTags(this Activity? activity, Guid operationId, string operation)
    {
        if (activity == null) return;
        
        activity.SetTag("balance_operation.id", operationId.ToString());
        activity.SetTag("operation", operation);
    }
}
