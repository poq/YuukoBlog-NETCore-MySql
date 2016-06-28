﻿using System;
using System.IO;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using YuukoBlog.Models;

namespace YuukoBlog
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            IConfiguration Configuration;
            services.AddConfiguration(out Configuration);
            var connStr = $"Data source={Configuration["DBFile"]};";
            if (connStr.IndexOf('\\') >= 0)
                connStr = connStr.Replace("/", "\\");
            services.AddDbContext<BlogContext>(x => x.UseSqlite(connStr));

            services.AddSmartCookies();

            services.AddMemoryCache();
            services.AddDistributedMemoryCache();
            services.AddSession(x => x.IdleTimeout = TimeSpan.FromMinutes(20));

            services.AddBlobStorage()
                .AddEntityFrameworkStorage<BlogContext>()
                .AddSessionUploadAuthorization();

            services.AddJsonLocalization()
                .AddCookieCulture();

            services.AddMvc()
                .AddMultiTemplateEngine()
                .AddCookieTemplateProvider();
        }

        public async void Configure(IApplicationBuilder app, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(LogLevel.Warning, true);

            app.UseStaticFiles();
            app.UseSession();
            app.UseBlobStorage("/assets/shared/scripts/jquery.codecomb.fileupload.js");
            app.UseDeveloperExceptionPage();
            app.UseMvcWithDefaultRoute();

            await SampleData.InitializeYuukoBlog(app.ApplicationServices);
        }

        public static void Main(string[] args)
        {
            var host = new WebHostBuilder()
                .UseKestrel()
                .UseUrls("http://*:80")
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseStartup<Startup>()
                .Build();

            host.Run();
        }
    }
}
