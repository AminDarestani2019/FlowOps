using FlowOps.BuildingBlocks.Domain.Results;

namespace FlowOps.Modules.Procurement.Domain.PurchaseRequests.ValueObjects;

public sealed record Currency
{
    public static readonly Currency EUR = new("EUR");
    public static readonly Currency USD = new("USD");
    public static readonly Currency IRR = new("IRR");

    private static readonly HashSet<string> SupportedCodes =
    [
        EUR.Code,
        USD.Code,
        IRR.Code
    ];

    private Currency(string code)
    {
        Code = code;
    }

    public string Code { get; }

    public static Result<Currency> Create(string code)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            return Result<Currency>.Failure(
                new Error(
                    "Currency.CodeRequired",
                    "Currency code is required."));
        }

        var normalizedCode = code.Trim().ToUpperInvariant();

        if (!SupportedCodes.Contains(normalizedCode))
        {
            return Result<Currency>.Failure(
                new Error(
                    "Currency.Unsupported",
                    $"Currency '{normalizedCode}' is not supported."));
        }

        return Result<Currency>.Success(new Currency(normalizedCode));
    }

    public override string ToString() => Code;
}