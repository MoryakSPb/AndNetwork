using System;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using AndNetwork.Server.Discord;
using AndNetwork.Server.Discord.Utility;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace AndNetwork.Server
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration) => Configuration = configuration;

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<ExceptionLogger>();
            //services.AddLettuceEncrypt();

            services.Configure<BrotliCompressionProviderOptions>(options => options.Level = CompressionLevel.Optimal);
            services.Configure<GzipCompressionProviderOptions>(options => options.Level = CompressionLevel.Optimal);
            services.AddResponseCompression(options =>
            {
                options.Providers.Add<BrotliCompressionProvider>();
                options.Providers.Add<GzipCompressionProvider>();
                options.EnableForHttps = true;
            });

            services.AddDbContext<ClanContext>(x => x.UseLazyLoadingProxies().UseNpgsql(Configuration["ConnectionStrings:Postgres"]));
            //services.AddTransient<DiscordRoleAssigner>();
            services.AddSingleton<DiscordConfiguration>();
            services.AddSingleton<ElectionsService>();

            services.AddSingleton<DiscordBot>();
            services.AddHostedService(provider => (DiscordBot)provider.GetService(typeof(DiscordBot)));

            services.AddSingleton<DiscordCommandService>();
            services.AddHostedService(provider => (DiscordCommandService)provider.GetService(typeof(DiscordCommandService)));


            services.AddControllersWithViews();
            services.AddRazorPages();
            services.AddLogging();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            
            ((ExceptionLogger)app.ApplicationServices.GetService(typeof(ExceptionLogger)))?.SetEvent();

            app.UseResponseCompression();
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseWebAssemblyDebugging();
            }
            else
            {
                app.UseExceptionHandler("/Error");

                app.UseForwardedHeaders(new ForwardedHeadersOptions
                                        {
                                            ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
                                        });
            }


            app.UseBlazorFrameworkFiles();
            app.UseStaticFiles();

            app.UseRouting();
            app.UseResponseCaching();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapRazorPages();
                endpoints.MapControllers();
                endpoints.MapFallbackToFile("index.html");
            });

            using IServiceScope scope = ((IServiceProvider)app.ApplicationServices.GetService(typeof(IServiceProvider))).CreateScope();
            ApplyMigrations((ClanContext)scope.ServiceProvider.GetService(typeof(ClanContext)));
        }

        public void ApplyMigrations(ClanContext context)
        {
            int tries = 8;
            while (true)
                try
                {
                    if (context.Database.GetPendingMigrations().Any()) context.Database.Migrate();
                    return;
                }
                catch
                {
                    if (tries > 0)
                    {
                        Task.Delay(2500);
                        tries--;
                    }
                    else throw;
                }
        }
    }
}
