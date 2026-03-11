using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using PasswordManager.Contracts;
using PasswordManager.Core.Models;
using PasswordManager.Services;

namespace PasswordManager.Tests.Api;

public class ApiClientTests
{
    [Fact]
    public async Task OpenVaultAsync_ReturnsTrue_OnSuccess()
    {
        var handler = new FakeHandler((request, _) =>
        {
            Assert.Equal(HttpMethod.Post, request.Method);
            Assert.Equal("/api/vault/open", request.RequestUri!.AbsolutePath);

            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = JsonContent.Create(new OpenVaultResponse { Success = true })
            };
        });

        var client = new HttpClient(handler) { BaseAddress = new Uri("https://localhost") };
        var api = new ApiClient(client);

        var result = await api.OpenVaultAsync("secret");

        Assert.True(result);
    }

    [Fact]
    public async Task GetCredentialsAsync_ParsesResponse()
    {
        var credentials = new[]
        {
            new Credential { Id = 1, Title = "Test", Username = "user" }
        };

        var handler = new FakeHandler((request, _) =>
        {
            Assert.Equal(HttpMethod.Get, request.Method);
            Assert.Equal("/api/credentials", request.RequestUri!.AbsolutePath);

            var json = JsonSerializer.Serialize(credentials);
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(json, System.Text.Encoding.UTF8, "application/json")
            };
        });

        var client = new HttpClient(handler) { BaseAddress = new Uri("https://localhost") };
        var api = new ApiClient(client);

        var result = await api.GetCredentialsAsync();

        Assert.Single(result);
        Assert.Equal("Test", result[0].Title);
        Assert.Equal("user", result[0].Username);
    }

    private sealed class FakeHandler : HttpMessageHandler
    {
        private readonly Func<HttpRequestMessage, CancellationToken, HttpResponseMessage> _handler;

        public FakeHandler(Func<HttpRequestMessage, CancellationToken, HttpResponseMessage> handler)
        {
            _handler = handler;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return Task.FromResult(_handler(request, cancellationToken));
        }
    }
}

