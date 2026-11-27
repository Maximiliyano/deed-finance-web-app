namespace Deed.Application.BankConnections.Statements;

public enum StatementMailOutcome
{
    Imported = 1,
    Rejected = 2,
    Unrouted = 3,
    Unreadable = 4
}
