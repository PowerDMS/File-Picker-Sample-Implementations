using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Runtime.Caching;
using System.Text.Json;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;

namespace FilePickerSample
{
    [ApiController]
    public class Controller : ControllerBase
    {
        private readonly string ClientID = "your-client-id";
        private readonly string ClientSecret = "your-client-secret";
        private readonly string AuthServerHost = "https://accounts.powerdms.com";
        private readonly string FilePickerHost = "https://filepicker.powerdms.com";

        [HttpGet, Route("callback")]
        public async Task<ActionResult> Callback(string code, string state, string error, string error_description)
        {
            if (!string.IsNullOrEmpty(error))
            {
                return new JsonResult(new {
                    error,
                    error_description
                });
            }

            var url = $"{AuthServerHost}/oauth/token";
            var content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["grant_type"] = "authorization_code",
                ["client_id"] = ClientID,
                ["client_secret"] = ClientSecret,
                ["code"] = code,
                ["redirect_uri"] = $"http://localhost:8008/callback"
            });

            var httpClient = new HttpClient();
            var response = await httpClient.PostAsync(url, content);

            if (response.IsSuccessStatusCode)
            {
                var responseJson = await response.Content.ReadAsStringAsync();
                var tokenResponse = JsonSerializer.Deserialize<TokenResponse>(responseJson);

                // save refresh token
                MemoryCache.Default["refreshToken"] = tokenResponse.refresh_token;

                var responseUrl = $"{FilePickerHost}/auth-finalize?" +
                    $"access_token={Uri.EscapeDataString(tokenResponse.access_token)}&" +
                    $"client_id={Uri.EscapeDataString(ClientID)}&" +
                    $"id_token={Uri.EscapeDataString(tokenResponse.id_token)}&" +
                    $"redirect_url={Uri.EscapeDataString("http://localhost:8008/callback")}";

                return new RedirectResult(responseUrl);
            }
            return null;
        }

        [HttpPost, Route("refresh")]
        public async Task<ActionResult> Refresh([FromQuery(Name = "id_token")] string idToken, string username)
        {
            // get last saved refresh token
            var refreshToken = MemoryCache.Default["refreshToken"].ToString();

            var url = $"{AuthServerHost}/oauth/token";
            var content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["grant_type"] = "refresh_token",
                ["client_id"] = ClientID,
                ["client_secret"] = ClientSecret,
                ["refresh_token"] = refreshToken
            });

            var httpClient = new HttpClient();
            var response = await httpClient.PostAsync(url, content);

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Failed to get tokens ({response.ReasonPhrase})");
            }

            var json = await response.Content.ReadAsStringAsync();

            // see structure of token response below
            var tokenResponse = JsonSerializer.Deserialize<TokenResponse>(json);

            return new OkObjectResult(new
            {
                AccessToken = tokenResponse.access_token,
                IdToken = tokenResponse.id_token
            });
        }
    }

    public class TokenResponse
    {
        public string access_token { get; set; }

        public string refresh_token { get; set; }

        public string id_token { get; set; }

        public string token_type { get; set; }
    }
}
