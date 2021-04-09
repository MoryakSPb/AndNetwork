using System.Globalization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace AndNetwork.Server
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CultureInfo culture = CultureInfo.CreateSpecificCulture("ru");
            culture.NumberFormat.CurrencySymbol = "SC";
            CultureInfo.CurrentCulture = CultureInfo.CurrentUICulture = CultureInfo.DefaultThreadCurrentCulture = CultureInfo.DefaultThreadCurrentUICulture = culture;

            CreateHostBuilder(args)
#if DEBUG
                .UseEnvironment("Development")
#endif
                .Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args).ConfigureWebHostDefaults(webBuilder => { webBuilder.UseStartup<Startup>(); });
    }
}
