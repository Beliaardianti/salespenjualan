namespace BelajarAspNetMvc.Models;

public abstract record Transaction(decimal Amount);
public record Deposit(decimal Amount) : Transaction(Amount);
public record Withdrawal(decimal Amount) : Transaction(Amount);

public record TransactionRow(Transaction Transaction, decimal RunningBalance);
