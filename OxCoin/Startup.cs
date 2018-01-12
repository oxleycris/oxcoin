using System;
using Autofac;
using Autofac.Builder;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OxCoin.Services;
using Autofac.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using OxCoin.Repository;

namespace OxCoin
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IContainer ApplicationContainer { get; private set; }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();

            // Register services to Autofac
            var builder = new ContainerBuilder();
            builder.RegisterType<GenerationService>().SingleInstance().As<IStartable>();
            builder.RegisterType<OxCoinDbContext>().InstancePerLifetimeScope();
            builder.Populate(services);

            // Build our application container.
            ApplicationContainer = builder.Build(ContainerBuildOptions.IgnoreStartableComponents);

            return new AutofacServiceProvider(ApplicationContainer);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, IApplicationLifetime appLifetime)
        {
            using (var serviceScope = app.ApplicationServices.GetService<IServiceScopeFactory>().CreateScope())
            {
                var context = serviceScope.ServiceProvider.GetRequiredService<OxCoinDbContext>();
                context.Database.Migrate();
            }

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            // Start anything decorated with IStartable
            foreach (var startable in app.ApplicationServices.GetServices<IStartable>())
            {
                startable.Start();
            }

            app.UseMvc();

            // If you want to dispose of resources that have been resolved in the
            // application container, register for the "ApplicationStopped" event.
            appLifetime.ApplicationStopped.Register(() => ApplicationContainer.Dispose());
        }
    }
}
