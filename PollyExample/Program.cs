// See https://aka.ms/new-console-template for more information

using Polly;
using Polly.CircuitBreaker;

namespace PollyExample
{
public static class OpenExchangeRatesClient
{
    public static void Main(string[] args)
    {
        var circuitBreakerPolicy = Policy<int>
            .Handle<Exception>()
            .CircuitBreaker(2, TimeSpan.FromSeconds(3), (ex, duration) =>
            {
                Console.WriteLine($"Circuit breaker opened for {duration.TotalSeconds} seconds.");
            }, () =>
            {
                Console.WriteLine("Circuit breaker closed.");
            });

        var fallBackPolicy = Policy<int>.Handle<BrokenCircuitException>() // The BrokenCircuitException is thrown by Polly if the policy is used while the circuit is open.
            .Fallback(fallbackAction: (context) => 0); // A fallback will return the value 0.

        var policy = Policy.Wrap(fallBackPolicy, circuitBreakerPolicy); // The two policies are wrapped together in order to make them work together.
        
        while (true)
        {
            try // We don't want the exceptions to bubble up, but just be handled by the policies.
            {
                var result = policy.Execute(() =>
                {
                    // We're emulating an API call here - it will fail in 1/3 of all cases.
                    
                    var r = new Random();
                    var n = r.Next(1, 3);
                    if (n != 1)
                    {
                        Console.WriteLine("Calling API - FAILED");
                        throw new AggregateException(); // The exception thrown when executed must match the exception handled by the circuit breaker.
                    }

                    return 1; // A successful API call returns 1
                });
                Console.WriteLine("Calling API - Response: " + result);
            }
            catch
            {
                // ignored
            }

            Thread.Sleep(500);
        }
    }
}
    
}