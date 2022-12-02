namespace MiCake.Authentication.MiniProgram.WeChat
{
    public class WeChatMiniProgramAuthConstants
    {
        public const string AuthenticationScheme = "MiniProgam.WeChat";

        public const string DisplayName = "WeChatMiniProgram";

        /// <summary>
        /// 微信小程序服务端验证的url
        /// https://developers.weixin.qq.com/miniprogram/dev/api-backend/open-api/login/auth.code2Session.html
        /// </summary>
        public const string AuthorizationEndpoint = "https://api.weixin.qq.com/sns/jscode2session";
    }
}
