using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NSE.WebApp.MVC.Extensions;
using NSE.WebApp.MVC.Services;
using NSE.WebApp.MVC.Services.Handlers;
using Polly;
using System;

namespace NSE.WebApp.MVC.Configuration
{
    public static class DependencyInjectionConfig
    {
        public static void RegisterServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddTransient<HttpClientAuthorizationDelegateHandler>();
            services.AddHttpClient<IAutenticacaoService, AutenticacaoService>();

            services.AddHttpClient<ICatalogoService, CatalogoService>()
                .AddHttpMessageHandler<HttpClientAuthorizationDelegateHandler>()
                .AddTransientHttpErrorPolicy(p => p.WaitAndRetryAsync(3, _ => TimeSpan.FromMilliseconds(600)));

            //services.AddHttpClient("Refit", options =>
            //{
            //    options.BaseAddress = new Uri(configuration.GetSection("CatalogoUrl").Value);
            //})
            // .AddHttpMessageHandler<HttpClientAuthorizationDelegateHandler>()
            //.AddTypedClient(Refit.RestService.For<ICatalogoServiceRefit>);


            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddScoped<IUser, AspNetUser>();
        }
    }
}
