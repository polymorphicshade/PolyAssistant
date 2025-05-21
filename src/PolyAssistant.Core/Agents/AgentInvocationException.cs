namespace PolyAssistant.Core.Agents;

public class AgentInvocationException : Exception
{
    public AgentInvocationException()
    {
    }

    public AgentInvocationException(string? message) : base(message)
    {
    }

    public AgentInvocationException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}