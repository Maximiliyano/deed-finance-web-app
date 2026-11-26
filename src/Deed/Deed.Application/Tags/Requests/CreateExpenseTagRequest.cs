namespace Deed.Application.Tags.Requests;

public sealed record CreateExpenseTagRequest(
    int ExpenseId,
    string Name
);
