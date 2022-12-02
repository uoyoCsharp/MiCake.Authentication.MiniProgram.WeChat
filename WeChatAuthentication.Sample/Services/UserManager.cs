using WeChatAuthentication.Sample.Models;

namespace WeChatAuthentication.Sample.Services
{
    public class UserManager : IUserManager
    {
        public User GetUser(int id)
        {
            return new User()
            {
                UserID = 1001,
                Name = "Bob"
            };
        }

        public User GetUserByWeChatOpenId(string openId)
        {
            return new User()
            {
                UserID = 1001,
                Name = "Bob"
            };
        }
    }
}
