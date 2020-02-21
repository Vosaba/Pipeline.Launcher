using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PipelineLauncher.Abstractions.PipelineRunner;

namespace PipelineLauncher.Addon.Web
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
            services.AddSignalR();
            //services.AddControllers();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            //app.M("/",ApplicationBuilderExtensions.GetTestPipeline());
            //app.UseWebSockets();
            //app.M2("/testHub");
            app.UseRouting();

            app.UseWebSockets(new WebSocketOptions
            {
                KeepAliveInterval = TimeSpan.FromSeconds(120),
            });

            // Than register your hubs here with a url.
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapHub<TestHub>("/testHub");
            });
        }
    }
    
    public class Test
    {
        public string Value { get; set; }
    }

    public class Test2
    {
        public string Value2 { get; set; }
    }

    public static class ApplicationBuilderExtensions
    {
        public static void M <TInput, TOutput>(this IApplicationBuilder applicationBuilder, string pattern, IAwaitablePipelineRunner<TInput, TOutput> pipelineRunner) 
        {
            applicationBuilder.UseRouting();

            applicationBuilder.UseEndpoints(endpoints =>
            {
                // Configuration of app endpoints.
                endpoints.MapPost(pattern, context =>
                {
                    using (StreamReader sr = new StreamReader(context.Request.Body))
                    {
                        var obj = JsonSerializer.Deserialize<TInput[]>(sr.ReadToEndAsync().Result);

                        var result = pipelineRunner.Process(obj);

                        return context.Response.WriteAsync(JsonSerializer.Serialize(result));
                    }
                });
                //endpoints.MapHealthChecks("/healthz");
            });
        }

        public static void M2(this IApplicationBuilder applicationBuilder, string pattern)
        {
            applicationBuilder.UseRouting();

            applicationBuilder.UseEndpoints(endpoints =>
            {
                endpoints.MapHub<TestHub>(pattern);
            });
        }

        public static IAwaitablePipelineRunner<Test, Test2> GetTestPipeline()
        {
            var g = new PipelineCreator();
            return g.Prepare<Test>().Stage(e=>new Test2(){Value2 = e.Value + "_)"}).CreateAwaitable();

        }

        
    }

    //[Route("/testHub")]

    public class TestHub : Hub
    {
        private readonly IPipelineRunner<Test, Test2> PipelineRunner;

        public TestHub()
        {
            var g = new PipelineCreator();
            PipelineRunner = g.Prepare<Test>().Stage(e => new Test2() { Value2 = e.Value + "_)" }).Create();

            PipelineRunner.ItemReceivedEvent += PipelineRunner_ItemReceivedEvent;
        }

        private void PipelineRunner_ItemReceivedEvent(Test2 item)
        {
            Clients.All.SendAsync("ReceiveMessage", JsonSerializer.Serialize(item));
        }

        public override Task OnConnectedAsync()
        {
            return base.OnConnectedAsync();
        }

        public async Task SendMessage(string message)
        {
            var s = JsonSerializer.Deserialize<Test[]>(message);

            PipelineRunner.Post(s);
        }
    }
}
