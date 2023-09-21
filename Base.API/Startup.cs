using Base.API.Filter;
using Base.API.Mapper.ModelToResource;
using Base.API.Mapper.ResourceToModel;
using Base.API.Middleware;
using Base.API.Permission;
using Base.API.Services;
using Base.Core.Application;
using Base.Infrastructure;
using Hangfire;
using Hangfire.SqlServer;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using static Base.API.Middleware.GlobalExceptionMiddleware;

namespace Base.API
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }
        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            var emailConfig = Configuration.GetSection("EmailConfig").Get<EmailConfig>();
            services.AddSingleton(emailConfig);
            services.AddTransient<IMailService,MailService>();

            services.Configure<FormOptions>(o =>
            {
                o.ValueLengthLimit = int.MaxValue;
                o.MultipartBodyLengthLimit = int.MaxValue;
                o.MemoryBufferThreshold = int.MaxValue;
            });

            services.AddAutoMapper(typeof(ItemMapper1), typeof(ItemMapper2));
            services.AddInfrastructure(Configuration);
            services.AddScoped<IBackgroundTaskService, BackgroundTaskService>();
            services.AddScoped<IJWTTokenService, JWTTokenService>();
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddSingleton<ICurrentUserService,CurrentUserService>();
            services.AddSingleton<IAuthorizationPolicyProvider, AuthorizationPolicyProvider>();
            services.AddScoped<IAuthorizationHandler, HasScopeHandler>();

            services.AddControllers(options =>
            {
                // Add Global Exception Filter here
                //options.Filters.Add<HttpResponseExceptionFilter>();
            })
                .AddJsonOptions(o =>
                {
                    o.JsonSerializerOptions.PropertyNamingPolicy = null;
                    o.JsonSerializerOptions.DictionaryKeyPolicy = null;
                })
                .AddNewtonsoftJson(option =>
            option.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore);

            services.AddRazorPages();

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Voucher.API", Version = "v1" });
                c.AddSecurityDefinition("Bearer",
                    new OpenApiSecurityScheme
                    {
                        Description = "JWT Authorization header using the Bearer scheme",
                        Name = "Authorization",
                        In = ParameterLocation.Header,
                        Type = SecuritySchemeType.Http,
                        BearerFormat = "JWT",
                        Scheme = "Bearer"
                    });
                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
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

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(x =>
            {
                x.RequireHttpsMetadata = false;
                x.SaveToken = true;
                x.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = Configuration["Jwt:Issuer"],
                    ValidateAudience = true,
                    ValidAudience = Configuration["Jwt:Issuer"],
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(Configuration["Jwt:SecretKey"]!)),
                };
            });

            services.AddAuthorization(options =>
            {
                options.AddPolicy("Read", policy =>
                {
                    policy.Requirements.Add(new HasScopeRequirement("read", Configuration["Jwt:Issuer"]!));
                });

                options.AddPolicy("Write", policy =>
                {
                    policy.Requirements.Add(new HasScopeRequirement("write", Configuration["Jwt:Issuer"]!));
                });

                options.AddPolicy("Delete", policy =>
                {
                    policy.Requirements.Add(new HasScopeRequirement("delete", Configuration["Jwt:Issuer"]!));
                });

                options.AddPolicy("Update", policy =>
                {
                    policy.Requirements.Add(new HasScopeRequirement("update", Configuration["Jwt:Issuer"]!));
                });

                options.AddPolicy("All", policy =>
                {
                    policy.Requirements.Add(new HasScopeRequirement("all", Configuration["Jwt:Issuer"]!));
                });

                options.AddPolicy("Restore", policy =>
                {
                    policy.Requirements.Add(new HasScopeRequirement("restore", Configuration["Jwt:Issuer"]!));
                });

                options.AddPolicy("User", policy =>
                {
                    policy.Requirements.Add(new HasScopeRequirement("User", Configuration["Jwt:Issuer"]!));
                });

                options.AddPolicy("Customer", policy =>
                {
                    policy.Requirements.Add(new HasScopeRequirement("Customer", Configuration["Jwt:Issuer"]!));
                });

                options.AddPolicy("Booking", policy =>
                {
                    policy.Requirements.Add(new HasScopeRequirement("Booking", Configuration["Jwt:Issuer"]!));
                });

                options.AddPolicy("VoucherType", policy =>
                {
                    policy.Requirements.Add(new HasScopeRequirement("VoucherType", Configuration["Jwt:Issuer"]!));
                });

                options.AddPolicy("Voucher", policy =>
                {
                    policy.Requirements.Add(new HasScopeRequirement("Voucher", Configuration["Jwt:Issuer"]!));
                });

                options.AddPolicy("Service", policy =>
                {
                    policy.Requirements.Add(new HasScopeRequirement("Service", Configuration["Jwt:Issuer"]!));
                });

                options.AddPolicy("ServicePackage", policy =>
                {
                    policy.Requirements.Add(new HasScopeRequirement("ServicePackage", Configuration["Jwt:Issuer"]!));
                });

                options.AddPolicy("VoucherExtension", policy =>
                {
                    policy.Requirements.Add(new HasScopeRequirement("VoucherExtension", Configuration["Jwt:Issuer"]!));
                });

                options.AddPolicy("Role", policy =>
                {
                    policy.Requirements.Add(new HasScopeRequirement("Role", Configuration["Jwt:Issuer"]!));
                });

                options.AddPolicy("Log", policy =>
                {
                    policy.Requirements.Add(new HasScopeRequirement("Log", Configuration["Jwt:Issuer"]!));
                }); 

                options.AddPolicy("Statistic", policy =>
                {
                    policy.Requirements.Add(new HasScopeRequirement("Statistic", Configuration["Jwt:Issuer"]!));
                });
            });

            services.AddCors(options =>
            {
                options.AddPolicy("ClientPermission", policy =>
                {
                    policy
                        .WithOrigins("http://vm.e-biz.com.vn",
                                     "http://fevm.e-biz.com.vn",
                                     "http://localhost:3000")
                        .AllowAnyHeader()
                        .AllowAnyMethod();
                });
            });

            services.AddHangfire(configuration =>
            {
                configuration
                .SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
                .UseSimpleAssemblyNameTypeSerializer()
                .UseRecommendedSerializerSettings()
                .UseSqlServerStorage(Configuration.GetConnectionString("MsSQLConnection"), new SqlServerStorageOptions
                {
                    CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
                    SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
                    QueuePollInterval = TimeSpan.Zero,
                    UseRecommendedIsolationLevel = true,
                    DisableGlobalLocks = true,
                });
            });
                

            services.AddHangfireServer();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (!env.IsDevelopment())
            {
            }

            app.UseStatusCodePages(async context =>
            {
                if(context.HttpContext.Response.StatusCode == 401)
                {
                    // Customize the response for 401 Unauthorized status code
                    context.HttpContext.Response.ContentType = "application/json";
                    var error = new ErrorDetails()
                    {
                        StatusCode = context.HttpContext.Response.StatusCode,
                        Message = "Unauthorize: You do not have permission to access this resource."
                    };
                    await context.HttpContext.Response.WriteAsync(error.ToString());
                }

                if(context.HttpContext.Response.StatusCode == 403)
                {
                    // Customize the response for 403 Forbidden status code
                    context.HttpContext.Response.ContentType = "application/json";
                    var error = new ErrorDetails()
                    {
                        StatusCode = context.HttpContext.Response.StatusCode,
                        Message = "Forbidden: You do not have sufficient privileges to access this resource."
                    };
                    await context.HttpContext.Response.WriteAsync(error.ToString());
                }
            });

            app.UseMiddleware<GlobalExceptionMiddleware>();

            app.UseSwagger();
            app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Voucher.API v1"));

            app.UseHttpsRedirection();

            app.UseHangfireDashboard("/hangfire", new DashboardOptions
            {
                Authorization = new[] {new AuthorizationFilter()}
            });

            app.UseStaticFiles();
            // app.UseCookiePolicy();

            app.UseRouting();
            // app.UseRateLimiter();
            // app.UseRequestLocalization();

            app.UseCors("ClientPermission");

            app.UseAuthentication();

            app.UseAuthorization();
            // app.UseSession();
            // app.UseResponseCompression();
            // app.UseResponseCaching();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapRazorPages();
                endpoints.MapControllers();
            });
        }
    }
}
