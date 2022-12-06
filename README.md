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
| CustomLoginState   | 根据微信服务器返回的会话密匙进行执行自定义登录态操作。   |
| SaveSessionToCache   | 是否要保存微信服务端所返回的OpenId和SessionKey到缓存中。   |
| CacheExpiration   | 缓存滑动过期的时间。【默认值为：30分钟】   |

**需要特别说明的是`WeChatJsCodeQueryString`和`CustomLoginState`。**

`WeChatJsCodeQueryString`一般与Options中的`CallbackPath`参数搭配使用，两个值指定了需要用户访问验证接口的URL地址：

比如`CallbackPath`为“/signin-wechat”，而`WeChatJsCodeQueryString`为"code"，那么当访问"https://your-host-address/signin-wechat?code=xxx"时，则将进入到微信小程序登陆验证过程中。

*注：上方的code是您通过小程序调用 wx.login()获取到的临时登录凭证code。*

默认情况下，验证登陆地址就是`“/signin-wechat?code=”`。开放该配置的缘由是为了避免和您现有的api冲突，当有冲突时，您可以通过更改这两个参数解决。

`CustomLoginState`是一个`Func`类型，它返回了微信服务器所返回的`openid`和`session_key`信息（假如您开启了`SaveSessionToCache`配置，那么该模型中的`SessionInfoKey`属性将包含缓存的Key值，可以通过使用该Key来获取到保存的OpenId等信息）。您可以通过建立自有逻辑对登陆进行处理，比如根据`openid`颁发`JWT TOKEN`等操作。

就像下方的代码一样（该代码可以在仓库中的Sample中看到）：

```csharp
services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            options.Audience = Configuration["JwtConfig:Audience"];
            options.ClaimsIssuer = Configuration["JwtConfig:Issuer"];
        })
        .AddWeChatMiniProgram(options =>
        {
            options.WeChatAppId = Configuration["WeChatMiniProgram:appid"];
            options.WeChatSecret = Configuration["WeChatMiniProgram:secret"];

            options.CustomLoginState += CreateToken;   //添加颁发JwtToken的步骤
        });

public async Task CreateToken(CustomLoginStateContext context)
{
    var associateUserService = context.HttpContext.RequestServices.GetService<AssociateWeChatUser>();

    if (context.ErrCode != null && !context.ErrCode.Equals("0"))
    {
        throw new Exception(context.ErrMsg);
    }

    var jwtToken = associateUserService.GetUserToken(context.OpenId);
    var response = context.HttpContext.Response;
    await response.WriteAsync(jwtToken);
}
```

上方代码结合`JwtBearer`验证方案，在微信服务器返回成功后，根据`OpenID`信息查询到了本地数据库中的用户信息，并且为该用户创建了`Token`进行返回。

#### 🍆 缓存OpenId和SessionKey

在某些时候，您可能需要将微信所返回的密匙信息（OpenId和SessionKey）保存在缓存中。那么您可以将配置项中的`SaveSessionToCache`设置为`true`。

此时您可以提供一个`IWeChatSessionInfoStore`的具体实现，并且将它注入到依赖注入容器中。在获取微信小程序所返回的密匙之后，就会自动保存到您所自定义的缓存中。

假如您没有指定`IWeChatSessionInfoStore`的服务，那么将使用默认的缓存实现方案：`DefaultSessionInfoStore`，该方案将数据保存在内存中，具体实现为`IDistributeCache`的`MemoryCache`。

下方的代码使用了缓存的方案来进行微信小程序登录验证:

```csharp

services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddWeChatMiniProgram(options =>
        {
            options.WeChatAppId = Configuration["WeChatMiniProgram:appid"];
            options.WeChatSecret = Configuration["WeChatMiniProgram:secret"];
            options.SaveSessionToCache = true;
            options.CustomLoginState += RedirectToGiveToken;   //添加通过重定向的方案来进行颁发Jwt Token
        });

public Task RedirectToGiveToken(CustomLoginStateContext context)
{
    var currentUrl = $"Login/CreateToken?key={context.SessionInfoKey}";
    context.HttpContext.Response.Redirect(currentUrl);

    return Task.CompletedTask;
}
```

当运行程序，访问 "https://your-host-address/signin-wechat?code=xxx" 时,将被重定向至 "https://your-host-address/Login/CreateToken?key=yourcachekey"。

而`Login/CreateToken`Action中根据所传入的cacheKey来得到微信的OpenId,然后执行颁发JWT Token的操作：

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
    public async Task<string> CreateToken(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentException($"key 不能为空");

        //可以添加各种验证和操作逻辑

        var weChatSession = await _weChatSessionStore.GetSessionInfo(key);
        return _associateWeChatUser.GetUserToken(weChatSession.OpenId);
    }
}
```

### 🍅 一些小问题

+ **如何在`CustomLoginState`里面获取到依赖注入的服务实例？**
  
  **answer** :`CustomLoginStateContext`里面包含了`HttpContext`，您可以根据`HttpContext.RequestServices`来进行获取。该`ServiceProvider`的范围和`Controller`的范围是一样的。

+ **如果微信服务器验证失败会怎么样**

  **answer** :当微信服务器验证失败的时候，`OpenId`等信息将为空。所以无法进行后面的验证步骤，最后将返回验证失败的错误信息。如果您在错误时进行处理，您可以使用`WeChatMiniProgramOptions.Events.OnWeChatServerCompleted`的`Func`委托注册一些自定义操作。

  ```csharp
    .AddWeChatMiniProgram(options =>
    {
        options.WeChatAppId = Configuration["WeChatMiniProgram:appid"];
        options.WeChatSecret = Configuration["WeChatMiniProgram:secret"];

        options.Events.OnWeChatServerCompleted += async context =>
        {
            var msg = context.ErrMsg;  //此处将获取到错误信息。
        };
    }
  ```
