using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Text;

namespace WeChatAuthentication.Sample.Services
{
    /// <summary>
    /// 该类用于根据微信所返回的OpenID来关联系统中的用户，并且创建Token。
    /// </summary>
    public class AssociateWeChatUser
    {
        private readonly IUserManager _userManager;
        private readonly IConfiguration _configuration;
        public AssociateWeChatUser(IUserManager userManager, IConfiguration configuration)
        {
            _userManager = userManager;
            _configuration = configuration;
        }

        public string GetUserToken(string openId)
        {
            var user = _userManager.GetUserByWeChatOpenId(openId);

            if (user == null)
            {
                throw new Exception("Can not find any user.");
                //或者根据Open来创建一个User，并且将该user保持至数据库，然后返回Token。
            }

            //生成JWT Token
            var signKey = Encoding.Default.GetBytes(_configuration["JwtConfig:SecurityKey"]);
            var userClaims = new Dictionary<string, object> {
                { "userId", user.UserID }
            };
            var tokenDescriptor = new SecurityTokenDescriptor()
            {
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(signKey), SecurityAlgorithms.HmacSha256Signature),
                Audience = _configuration["JwtConfig:Audience"],
                Issuer = _configuration["JwtConfig:Issuer"],
                Expires = DateTime.Now.AddDays(Convert.ToInt32(_configuration["JwtConfig:ExpireDay"])),
                Claims = userClaims
            };

            var jwtHandler = new JwtSecurityTokenHandler();
            return jwtHandler.WriteToken(jwtHandler.CreateJwtSecurityToken(tokenDescriptor));
        }
    }
}
