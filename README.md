# MiCake.Authentication.MiniProgram.WeChat

## 🍧 介绍

`AspNet Core`的微信小程序登录验证支持包。

根据微信小程序官方的登陆文档所实现的`AspNet Core`身份验证方案。该包主要完成了开发者服务器同微信接口服务器进行凭证交换的过程，用户可以根据该扩展包所提供的生命周期方法进行自定义登陆态的处理。

![pic](https://res.wx.qq.com/wxdoc/dist/assets/img/api-login.2fcc9f35.jpg)

## 🍒 用法

### 🍓 所需环境版本

+ `AspNet Core` 7.0及以上版本

### 🍑 安装包

```powershell
Install-Package MiCake.Authentication.MiniProgram.WeChat
```

### 🍍 工作原理

该扩展包主要帮助 `同微信服务器交换数据（openId & session_key)` 和 `将交换的数据保存到缓存中` 两个操作，方便用户能够直接通过 `小程序客户端调用login的code` 就获取到对应的数据。

由于微信服务器所返回的`session_key`等数据为敏感数据，你不应该直接暴露给外界，所以你可以将缓存的key返回给外界。  

外界通过传递缓存key到服务器，服务器再查找缓存来进行后续操作。

### 🍈使用

```csharp
services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddWeChatMiniProgram(options =>
        {
            options.WeChatAppId = Configuration["WeChatMiniProgram:appid"];  //微信appid
            options.WeChatSecret = Configuration["WeChatMiniProgram:secret"]; //微信secret_key
        });
```

通过`AddWeChatMiniProgram`扩展方法就可以添加对微信小程序验证的支持。`WeChatAppId`和`WeChatSecret`是必须要配置的参数，因为这是和微信服务器进行交换的必要信息之一。

#### 🍍 参数说明

下方解释了`AddWeChatMiniProgram`中使用的类型为`WeChatMiniProgramOptions`的配置信息：

| 参数 | 说明 |
| ---- | ---- |
| WeChatAppId   | 小程序 appId。从微信开放平台申请。   |
| WeChatSecret     | 小程序 appSecret key。从微信开放平台申请。   |
| WeChatGrantTtype   | 授权类型，该值为:authorization_code。无须更改。   |
| WeChatJsCodeQueryString   | 登录url中,携带小程序客户端获取到code的参数名。默认为:"code"。   |
| SaveSessionToCache   | 是否要保存微信服务端所返回的OpenId和SessionKey到缓存中。   |
| CacheSlidingExpiration   | 缓存滑动过期的时间。【默认值为：30分钟】   |
| CacheKeyGenerationRule | 缓存的key生成规则。 |
| Events   | 程序进行过程中注册的事件集。 你可以注册属于自己的事件来实现一些自定义逻辑。   |

*对`Events`需要特别说明的是： 你可以特别关注其中的 `OnWeChatSessionObtained`、`OnRemoteFailure`和`OnWeChatServerCompleted`这三个事件。*

*你可以查询下方的示例，来了解他们的用途。*

**需要特别说明的是`WeChatJsCodeQueryString`和`CustomLoginState`。**

`WeChatJsCodeQueryString`一般与Options中的`CallbackPath`参数搭配使用，两个值指定了需要用户访问验证接口的URL地址：

比如`CallbackPath`为“/signin-wechat”，而`WeChatJsCodeQueryString`为"code"，那么当访问"https://your-host-address/signin-wechat?code=xxx"时，则将进入到微信小程序登陆验证过程中。

*注：上方的code是您通过小程序调用 wx.login()获取到的临时登录凭证code。*

默认情况下，验证登陆地址就是`“/signin-wechat?code=”`。开放该配置的缘由是为了避免和您现有的api冲突，当有冲突时，您可以通过更改这两个参数解决。

#### 🍆 缓存OpenId和SessionKey

在某些时候，您可能需要将微信所返回的密匙信息（OpenId和SessionKey）保存在缓存中。那么您可以将配置项中的`SaveSessionToCache`设置为`true`。

此时您可以提供一个`IWeChatSessionInfoStore`的具体实现，并且将它注入到依赖注入容器中。在获取微信小程序所返回的密匙之后，就会自动保存到您所自定义的缓存中。

假如您没有指定`IWeChatSessionInfoStore`的服务，那么将使用默认的缓存实现方案：`DefaultSessionInfoStore`，该方案将数据保存在内存中，具体实现为`IDistributeCache`的`MemoryCache`。

#### 🍆 示例

```csharp

services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddWeChatMiniProgram(options =>
        {
            options.WeChatAppId = Configuration["WeChatMiniProgram:appid"];
            options.WeChatSecret = Configuration["WeChatMiniProgram:secret"];
            options.SaveSessionToCache = true;
            options.Events.OnWeChatSessionObtained += RedirectToGiveToken;   //添加颁发JwtToken的步骤
            options.Events.OnRemoteFailure += HandleFailure;  //添加错误处理，将异常信息包装为格式化的对象
        });

        public Task RedirectToGiveToken(WeChatSessionObtainedContext context)
        {
            // 将缓存的key返回给客户端 便于后期客户端传递回来进行操作
            context.HttpContext.Response.WriteAsJsonAsync(new { data = context.SessionCacheKey });
            return Task.CompletedTask;
        }

        public Task HandleFailure(RemoteFailureContext context)
        {
            context.HttpContext.Response.StatusCode = 500;
            context.HttpContext.Response.WriteAsJsonAsync(new { errorMsg = context.Failure.Message });

            context.HandleResponse();   // 当Response已经Write了数据时，必须调用这句话
            return Task.CompletedTask;
        }
```

当运行程序，访问 "https://your-host-address/signin-wechat?code=xxx" 时,如果数据正确，将会得到一个对应的cacheKey值。

在后期的逻辑中根据所传入的cacheKey来得到微信的OpenId,然后执行颁发JWT Token的操作：

```csharp
[ApiController]
[Route("[controller]/[action]")]
public class LoginController : ControllerBase
{
    private readonly IWeChatSessionInfoStore _weChatSessionStore;
    private readonly AssociateWeChatUser _associateWeChatUser;

    public LoginController(IWeChatSessionInfoStore weChatSessionStore, AssociateWeChatUser associateWeChatUser)
    {
        _weChatSessionStore = weChatSessionStore;
        _associateWeChatUser = associateWeChatUser;
    }

    [HttpGet]
    public async Task<string> CreateToken(string cacheKey)
    {
        if (string.IsNullOrWhiteSpace(cacheKey))
            throw new ArgumentException($"key 不能为空");

        //可以添加各种验证和操作逻辑

        var weChatSession = await _weChatSessionStore.GetSessionInfo(cacheKey);
        return _associateWeChatUser.GetUserToken(weChatSession.OpenId);
    }
}
```

### 🍅 一些小问题

+ **如何在`CustomLoginState`里面获取到依赖注入的服务实例？**
  
  **answer** :`CustomLoginStateContext`里面包含了`HttpContext`，您可以根据`HttpContext.RequestServices`来进行获取。该`ServiceProvider`的范围和`Controller`的范围是一样的。
