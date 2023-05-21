using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Twitter.Data;
using Twitter.Services;
using Twitter;
using SendGrid.Extensions.DependencyInjection;

const string connectionString = "server=localhost;database=twitter;user=root;password=root";
const string sendGridApiKey = "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";

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).
    AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, (options) =>
    {
        options.Cookie.SameSite = SameSiteMode.None;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        options.Cookie.IsEssential = true;
        options.Cookie.Name = "Auth";
        options.Cookie.HttpOnly = true;
        options.ExpireTimeSpan = TimeSpan.FromDays(7);
        options.SlidingExpiration = true;

        options.Events.OnRedirectToLogin = ctx =>
        {
            ctx.Response.StatusCode = 401;
            return Task.CompletedTask;
        };
    });

builder.Services.AddAuthorization(options =>
{
});
builder.Services.AddDbContext<UserContext>(options => options.UseMySQL(connectionString));
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ITweetService, TweetService>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddSendGrid(options =>
{
    options.ApiKey = sendGridApiKey;
});


builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins("https://localhost:5173").AllowCredentials().AllowAnyHeader().AllowAnyMethod();
    });
});



builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "Twitter Api", Version = "v1" });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "v1"));
}

app.UseCors();
app.UseAuthorization();
app.UseTwitterApi();

app.Run();
