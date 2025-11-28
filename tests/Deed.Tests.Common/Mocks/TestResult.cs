
namespace Deed.Tests.Common;

public class TestResult : Result
{
    protected TestResult(bool isSuccess, IList<Error> errors)
        : base(isSuccess, errors)
    {
    }
}

public class TestResult<TValue> : Result<TValue>
{

}
