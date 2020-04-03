using BotSharp.Core.Modules;
using DotNetToolkit.JwtHelper;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerUI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json.Serialization;


namespace BotSharp.WebHost
{
    public partial class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
            this.modulesStartup = new ModulesStartup(configuration);
        }

        private readonly ModulesStartup modulesStartup;

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllersWithViews();

            services.AddCors(options =>
            {
                options.AddPolicy("AllowAllHeaders", builder =>
                {
                    builder.WithOrigins("http://localhost:3110")
                        .AllowCredentials()
                        .AllowAnyHeader()
                        .AllowAnyMethod();
                });
            });
            services.AddJwtAuth(Configuration);

            var mvcBuilder = services.AddMvc(options =>
            {
                options.RespectBrowserAcceptHeader = true;
            }).AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
            });

            //PlatformModuleAssembyLoader.LoadAssemblies(Configuration, assembly => mvcBuilder.AddApplicationPart(assembly));

            this.modulesStartup.ConfigureServices(services);

            services.AddSwaggerGen(c =>
            {

                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description =
                      "JWT Authorization header using the Bearer scheme. \r\n\r\n Enter 'Bearer' [space] and then your token in the text input below.\r\n\r\nExample: \"Bearer 12345abcdef\"",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer"
                });

                c.AddSecurityRequirement(new OpenApiSecurityRequirement()
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            },
                            Scheme = "oauth2",
                            Name = "Bearer",
                            In = ParameterLocation.Header,

                        },
                        new List<string>()
                    }
                });

                var info = Configuration.GetSection("Swagger").Get<Swagger>();
                c.SwaggerDoc(info.Version, new OpenApiInfo
                {
                    Version = info.Version,
                    Description = info.Description,
                    Title = info.Title,
                    TermsOfService = new System.Uri(info.TermsOfService),
                    License = new OpenApiLicense { Name = info.License.Name, Url = new System.Uri(info.License.Url) },
                    Contact = new OpenApiContact { Name = info.Contact.Name, Url = new System.Uri(info.Contact.Url), Email = info.Contact.Email },
                });

            });

            // register platform dependency
            /*services.AddTransient<IBotEngine>((provider) =>
            {
                return instance;
            });*/
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            app.UseStaticFiles();

            app.UseRouting();
            app.UseCors("AllowAllHeaders");

            app.UseDefaultFiles();

            // Enable middleware to serve generated Swagger as a JSON endpoint.
            app.UseSwagger();

            app.UseSwaggerUI(c =>
            {
                var info = Configuration.GetSection("Swagger").Get<Swagger>();

                c.SupportedSubmitMethods(SubmitMethod.Get, SubmitMethod.Post, SubmitMethod.Put, SubmitMethod.Patch, SubmitMethod.Delete);
                c.ShowExtensions();
                c.SwaggerEndpoint(Configuration.GetValue<String>("Swagger:Endpoint"), info.Title);
                c.RoutePrefix = String.Empty;
                c.DocumentTitle = info.Title;
                c.InjectStylesheet(Configuration.GetValue<String>("Swagger:Stylesheet"));

                Console.WriteLine();
                Console.WriteLine($"{info.Title} [{info.Version}] {info.License.Name}");
                Console.WriteLine($"{info.Description}");
                Console.WriteLine($"{info.Contact.Name}, {DateTime.UtcNow.ToString()}");
                Console.WriteLine();
            });

            app.Use(async (context, next) =>
            {
                string token = context.Request.Headers["Authorization"];
                if (!string.IsNullOrWhiteSpace(token) && (token = token.Split(' ').Last()).Length == 32)
                {
                    var config = (IConfiguration)AppDomain.CurrentDomain.GetData("Configuration");
                    context.Request.Headers["Authorization"] = token;

                    context.Request.Headers["Authorization"] = "Bearer " + JwtToken.GenerateToken(config, token);
                }

                await next.Invoke();
            });

            app.UseAuthentication();
            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });


            this.modulesStartup.Configure(app, env);

            AppDomain.CurrentDomain.SetData("DataPath", Path.Combine(env.ContentRootPath, "App_Data"));
            AppDomain.CurrentDomain.SetData("Configuration", Configuration);
            AppDomain.CurrentDomain.SetData("ContentRootPath", env.ContentRootPath);
            AppDomain.CurrentDomain.SetData("Assemblies", Configuration.GetValue<String>("Assemblies").Split(','));

            /*InitializationLoader loader = new InitializationLoader();
            loader.Env = env;
            loader.Config = Configuration;
            loader.Load();*/
        }
    }


    public class SwaggerInfo
    {
        public Swagger Swagger { get; set; }
    }

    public class Swagger
    {
        public Contact Contact { get; set; }
        public string Description { get; set; }
        public string Endpoint { get; set; }
        public License License { get; set; }
        public string TermsOfService { get; set; }
        public string Title { get; set; }
        public string Version { get; set; }
        public string Stylesheet { get; set; }
    }

    public class Contact
    {
        public string Email { get; set; }
        public string Name { get; set; }
        public string Url { get; set; }
    }

    public class License
    {
        public string Name { get; set; }
        public string Url { get; set; }
    }

}