using Coinbase.AdvancedTrade.Models;

namespace EZATB07.Library.Exchanges.Coinbase.Models;

public class ResultDTO
{
    public Status Status { get; set; }
    public string? Message { get; set; }
    public bool? IsRetryable { get; set; } = null;
    public Account PayingAccount { get; set; } = new Account();
    public Account BuyingAccount { get; set; } = new Account();

    //Default: Success Constructor
    public ResultDTO()
    {
           
    }

    //Error Constructor
    public ResultDTO(string message, bool? isRetryable)
    {
        Status = Status.Error;
        Message = message;
        IsRetryable = isRetryable;
    }
}

public class ResultDTO<T>
{
    public Status Status { get; set; }
    public string? Message { get; set; }
    public bool? IsRetryable { get; set; } = null;
    public T? Data { get; set; }
    public Account? PayingAccount { get; set; }
    public Account? BuyingAccount { get; set; }
}

public enum Status
{
    Valid,
    Success,
    Error
}