using System.Net.Http.Headers;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace APIProject.IntegrationTests
{
    public abstract class IntegrationTestBase : IClassFixture<TestFixture>, IDisposable
    {
        protected readonly TestFixture _fixture;
        protected readonly HttpClient _client;
        private bool _disposed;

        protected IntegrationTestBase(TestFixture fixture)
        {
            _fixture = fixture;
            _client = fixture.Client;
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                // Specific cleanup for test if needed
                _disposed = true;
            }
        }

        protected void SetAuthenticationToken(string token)
        {
            _fixture.SetAuthenticationToken(token);
        }
    }
}