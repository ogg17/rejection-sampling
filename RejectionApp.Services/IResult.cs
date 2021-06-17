using RejectionApp.Models;

namespace RejectionApp.Services
{
    public interface IResult
    {
        private static Result _result;

        public Result GetResult()
        {
            return new();
        }

        public void SetResult(Result result)
        {
            _result = result;
        }
    }
}