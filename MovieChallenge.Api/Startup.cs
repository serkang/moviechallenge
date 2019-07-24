using Hangfire;
using Hangfire.Common;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Cors.Internal;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MovieChallenge.Common;
using MovieChallenge.Data;
using MovieChallenge.Data.Domain;
using MovieChallenge.Logic.Interface;
using MovieChallenge.Logic.Service;
using Serilog;
using Serilog.Events;

namespace MovieChallenge.Api
{
    public class Startup
    {
        public IConfiguration Configuration { get; }
        public IHostingEnvironment Environment { get; }
        public Startup(IConfiguration configuration, IHostingEnvironment environment)
        {
            Configuration = configuration;
            Environment = environment;
        }


        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            HttpHelper.ApiKey = Configuration["ApiKey"];

            var assemblyDbName = typeof(AppDbContext).Namespace;
            services.AddDbContext<AppDbContext>(o =>
            {
                o.UseSqlServer(Configuration.GetConnectionString(Configuration["AppConfig:ActiveBranch"]), serverOptions =>
                {
                    serverOptions.CommandTimeout(10200);
                    serverOptions.MigrationsAssembly(assemblyDbName);
                });
            });

            services.AddTransient<ILogger>(l =>
            {
                return new LoggerConfiguration().Enrich.FromLogContext().MinimumLevel.Override("Microsoft", LogEventLevel.Error)
                .MinimumLevel.Override("System", LogEventLevel.Error).WriteTo.File(Configuration["Serilog:FileName"], rollOnFileSizeLimit: true)
                .CreateLogger();
            });

            services.AddIdentity<ApplicationUser, ApplicationRole>(opt =>
            {
                opt.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
                opt.User.RequireUniqueEmail = false;
            }).AddEntityFrameworkStores<AppDbContext>();

            services.AddMemoryCache();
            services.AddCors(o => o.AddPolicy("MyPolicy", builder =>
            {
                builder.AllowAnyOrigin()
                    .AllowAnyMethod()
                    .AllowAnyHeader();
            }));
            services.AddSingleton(Environment);
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            services.AddScoped<IMovieService, MovieService>();

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

            services.Configure<MvcOptions>(o => o.Filters.Add(new CorsAuthorizationFilterFactory("MyPolicy")));
            services.AddSwaggerGen(s =>
            {
                s.SwaggerDoc("v1", new Swashbuckle.AspNetCore.Swagger.Info { Title = "Movie Challenge API", Version = "v1" });
            });

            services.AddDistributedRedisCache(option =>
            {
                option.Configuration = Configuration["Redis"];
                option.InstanceName = "movieChallenge";
            });

            services.AddHangfire(x=>x.UseSqlServerStorage(Configuration.GetConnectionString(Configuration["AppConfig:ActiveBranch"])));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, IHttpContextAccessor accessor, IRecurringJobManager recurringJobs)
        {
            if (env.IsDevelopment()) app.UseDeveloperExceptionPage();
            HttpHelper.Configure(accessor, Configuration);

            app.UseCors("MyPolicy");
            app.UseStaticFiles();
            app.UseSwagger();
            app.UseSwaggerUI(s => { s.SwaggerEndpoint("/swagger/v1/swagger.json", "Movie Challenge API v1"); });

            app.UseHangfireDashboard();
            app.UseHangfireServer();

            //app.UseHttpsRedirection();
            app.UseMvc();

            recurringJobs.AddOrUpdate("UpdateMovies", Job.FromExpression<IMovieService>(x=>x.UpdateMovies()), "*/10 * * * *");
        }
    }
}
