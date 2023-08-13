using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using WeChatCrawler;

namespace WeChatCrawlerWebApi {
  public class Startup {
    public Startup(IConfiguration configuration) {
      Configuration = configuration;
    }

    public IConfiguration Configuration { get; }

    // This method gets called by the runtime. Use this method to add services to the container.
    public void ConfigureServices(IServiceCollection services) {

      services.AddControllers();
    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env) {
      if (env.IsDevelopment()) {
        app.UseDeveloperExceptionPage();
      }

      app.Use(async (context, next) => {
        //if ("user".Equals(context.Request.Headers["User"]))
        //  await next.Invoke();
        //else
        //  await context.Response.WriteAsync("Permission denied.");
        using var db = new Database();
        if (db.GetUser(context.Request.Headers["User"]) == null)
          await context.Response.WriteAsync("Permission denied.");
        else
          await next.Invoke();
      });

      app.UseRouting();

      //app.UseAuthorization();

      app.UseEndpoints(endpoints => {
        endpoints.MapControllers();
      });
    }
  }
}
