using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System;
using JwtTok_RefTok.DbContexts;
using JwtTok_RefTok.Repository.IRepository;
using JwtTok_RefTok.Repository;
using System.IdentityModel.Tokens.Jwt;

namespace JwtTok_RefTok
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            ConfigurationManager configuration = builder.Configuration;


            builder.Services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("SqlServerDbCon")));

            builder.Services.AddIdentity<IdentityUser, IdentityRole>(options =>
                {
                    options.Password.RequireUppercase = true; // on production add more secured options
                    options.Password.RequireDigit = true;
                    options.SignIn.RequireConfirmedEmail = true;
                }).AddEntityFrameworkStores<AppDbContext>().AddDefaultTokenProviders();

            builder.Services.AddAuthentication(x =>
                {
                    x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                }).AddJwtBearer(o =>
                {
                    var Key = Encoding.UTF8.GetBytes(configuration["JWT:Key"]);
                    o.SaveToken = true;
                    o.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = false, // on production make it true
                        ValidateAudience = false, // on production make it true
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = configuration["JWT:Issuer"],
                        ValidAudience = configuration["JWT:Audience"],
                        IssuerSigningKey = new SymmetricSecurityKey(Key),
                        ClockSkew = TimeSpan.Zero
                    };
                    o.Events = new JwtBearerEvents
                    {
                        OnAuthenticationFailed = context =>
                        {
                            if (context.Exception.GetType() == typeof(SecurityTokenExpiredException))
                            {
                                context.Response.Headers.Add("IS-TOKEN-EXPIRED", "true");
                            }
                            return Task.CompletedTask;
                        }
                    };
                    o.SecurityTokenValidators.Clear();
                    o.SecurityTokenValidators.Add(new JwtSecurityTokenHandler
                    {
                        TokenLifetimeInMinutes = 60 * 3
                    }) ;
                });

            builder.Services.AddSingleton<IJWTManagerRepository, JWTManagerRepository>();
            builder.Services.AddScoped<IUserServiceRepository, UserServiceRepository>();
            // Add services to the container.

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();
            app.UseAuthentication();
            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}