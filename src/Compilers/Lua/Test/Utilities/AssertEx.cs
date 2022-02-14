using Xunit.Sdk;

namespace Loretta.CodeAnalysis.Lua.Test.Utilities
{
    public static class AssertEx
    {
        public static void RunsWithin(int millisecondsTimeout, Action action)
        {
            var token = new CancellationTokenSource(millisecondsTimeout);
            try
            {
                var task = Task.Run(action, token.Token);
                if (!task.Wait(millisecondsTimeout))
                    throw new XunitException("Test timed out.");
                task.GetAwaiter().GetResult();
            }
            finally
            {
                token.Dispose();
            }
        }

        public static T RunsWithin<T>(int millisecondsTimeout, Func<T> func)
        {
            var token = new CancellationTokenSource(millisecondsTimeout);
            try
            {
                var task = Task.Run(func, token.Token);
                if (!task.Wait(millisecondsTimeout))
                    throw new XunitException("Test timed out.");
                return task.GetAwaiter().GetResult();
            }
            finally
            {
                token.Dispose();
            }
        }
    }
}
