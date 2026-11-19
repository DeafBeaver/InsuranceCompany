using System;
using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace InsuranceSystem.BLL.Services
{
    public class LoginRequest
    { 
        public string Username { get; set; } = ""; 
        public string Password { get; set; } = ""; 
    }
    public class LoginResponse 
    { 
        public string Token { get; set; } = ""; 
        public string Role { get; set; } = ""; 
    }
    public class RegisterRequest
    {
        public string Username { get; set; } = ""; 
        public string Password { get; set; } = ""; 
        public string Role { get; set; } = ""; 
    }
    public class ChangePasswordRequest 
    { 
        public string OldPassword { get; set; } = ""; 
        public string NewPassword { get; set; } = ""; 
    }
    public class SetRoleRequest 
    { 
        public string Username { get; set; } = ""; 
        public string Role { get; set; } = ""; 
    }
    public class UsersResponse 
    { 
        public string UserName { get; set; } = ""; 
        public string Role { get; set; } = ""; 
    }

    public class AuthApiClient
    {
        private readonly HttpClient _httpClient;

        public AuthApiClient(string baseUrl = "https://localhost:5001/")
        {
            _httpClient = new HttpClient { BaseAddress = new Uri(baseUrl) };
        }

        public async Task<LoginResponse?> LoginAsync(string username, string password)
        {
            var response = await _httpClient.PostAsJsonAsync("api/Users/login", new LoginRequest { Username = username, Password = password });
            if (!response.IsSuccessStatusCode) return null;
            return await response.Content.ReadFromJsonAsync<LoginResponse>();
        }

        public async Task<bool> RegisterAsync(string adminToken, string username, string password, string role)
        {
            SetBearerToken(adminToken);
            var response = await _httpClient.PostAsJsonAsync("api/Users/register", new RegisterRequest { Username = username, Password = password, Role = role });
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> ChangePasswordAsync(string userToken, string oldPassword, string newPassword)
        {
            SetBearerToken(userToken);
            var response = await _httpClient.PutAsJsonAsync("api/Users/password/change", new ChangePasswordRequest { OldPassword = oldPassword, NewPassword = newPassword });
            return response.IsSuccessStatusCode;
        }
        public async Task<bool> SetUserRoleAsync(string adminToken, string username, string newRole)
        {
            SetBearerToken(adminToken);
            var response = await _httpClient.PutAsJsonAsync("api/Users/role/set", new SetRoleRequest { Username = username, Role = newRole });
            return response.IsSuccessStatusCode;
        }

        public async Task<IEnumerable<UsersResponse>?> GetUsersAsync(string adminToken)
        {
            SetBearerToken(adminToken);
            return await _httpClient.GetFromJsonAsync<IEnumerable<UsersResponse>>("api/Users");
        }

        private void SetBearerToken(string token)
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }
    }
}