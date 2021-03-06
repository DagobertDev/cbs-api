using System;
using FirebaseAdmin;
using FirebaseAdmin.Auth;
using Google.Apis.Auth.OAuth2;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

namespace CBSWebAPI
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
	        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
	        {
		        var firebaseProject = Configuration.GetValue<string>("FirebaseProjectName");

		        options.Authority = $"https://securetoken.google.com/{firebaseProject}";
		        options.TokenValidationParameters = new TokenValidationParameters
		        {
			       ValidateIssuer = true,
			       ValidIssuer = $"https://securetoken.google.com/{firebaseProject}", 
			       ValidateAudience = true, 
			       ValidAudience = firebaseProject, 
			       ValidateLifetime = true
		        };
	        });

	        services.AddCors();
            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "CBSWebAPI", Version = "v1" });
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme {
	                In = ParameterLocation.Header, 
	                Description = "Please insert JWT with Bearer into field",
	                Name = "Authorization",
	                Type = SecuritySchemeType.ApiKey 
                });
                c.AddSecurityRequirement(new OpenApiSecurityRequirement {
	                { 
		                new OpenApiSecurityScheme 
		                { 
			                Reference = new OpenApiReference 
			                { 
				                Type = ReferenceType.SecurityScheme,
				                Id = "Bearer" 
			                } 
		                },
                        Array.Empty<string>()
                    } 
                });
            });

            services.AddDbContext<AppDbContext>(options => options.UseNpgsql(Configuration.GetConnectionString("Postgres")));
            ConfigureFirebase(services);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
	            app.UseCors(options => options.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "CBSWebAPI v1"));
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }

        private void ConfigureFirebase(IServiceCollection services)
        {
	        var credentialsFile = Configuration.GetValue<string>("GoogleApplicationCredentials");
	        var credentialsString = Configuration.GetValue<string>("GoogleApplicationCredentialsSTRING");

	        GoogleCredential? credential;

	        if (!string.IsNullOrEmpty(credentialsFile))
	        {
		        credential = GoogleCredential.FromFile(credentialsFile);
	        }

	        else if (!string.IsNullOrEmpty(credentialsString))
	        {
		        credential = GoogleCredential.FromJson(credentialsString);
	        }

	        else
	        {
		        throw new ApplicationException("No GoogleApplicationCredentials are defined");
	        }
	        
	        var firebase = FirebaseApp.Create(new AppOptions
	        {
		        Credential = credential
	        });

	        services.AddSingleton(FirebaseAuth.GetAuth(firebase));
        }
    }
}
