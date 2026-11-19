using System;
using System.Collections.Generic;
using System.Text;

namespace InsuranceSystem.BLL.Service.AuthApi
{
    public class LoginResponseModel
    {
        public string Token { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
    }
}
