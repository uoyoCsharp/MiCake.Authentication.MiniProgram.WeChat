using WeChatAuthentication.Sample.Models;

namespace WeChatAuthentication.Sample.Services
{
    public interface IUserManager
    {
        public User GetUser(int id);

        public User GetUserByWeChatOpenId(string openId);
    }
}
