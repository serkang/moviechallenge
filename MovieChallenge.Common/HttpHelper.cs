using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.ComponentModel;

namespace MovieChallenge.Common
{
    public static class HttpHelper
    {
        private static IHttpContextAccessor _accessor;
        private static IConfiguration _configuration;
        public static string ApiKey { get; set; }
        public static void Configure(IHttpContextAccessor accessor, IConfiguration configuration)
        {
            _accessor = accessor;
            _configuration = configuration;
        }

        public static HttpContext HttpContext => _accessor != null ? _accessor.HttpContext : null;
        public static T GetService<T>()
        {
            return _accessor.HttpContext.RequestServices.GetService<T>();
        }

        public static string GetConfig(string key)
        {
            return GetConfig<string>(key);
        }

        public static T GetConfig<T>(string key)
        {
            var appSetting = _configuration[key.Trim()];
            if (string.IsNullOrWhiteSpace(appSetting)) throw new Exception(key);

            var converter = TypeDescriptor.GetConverter(typeof(T));
            return (T)(converter.ConvertFromInvariantString(appSetting));
        }
    }
}
