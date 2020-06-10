using MiCake.Authentication.MiniProgram.WeChat;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Threading.Tasks;
using WeChatAuthentication.Sample.Services;

namespace WeChatAuthentication.Sample
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();

            services.AddSingleton<IUserManager, UserManager>();
            services.AddScoped<AssociateWeChatUser>();

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

                    options.CustomerLoginState += CreateToken;   //添加颁发JwtToken的步骤
                });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }

        public async Task CreateToken(CustomerLoginStateContext context)
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
    }
}
