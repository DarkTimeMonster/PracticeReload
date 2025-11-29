using DAL;
using DAL.Interfaces;
using DAL.Storage;
using DaviskibaYP.Validation.LoginAndRegistration;
using DaviskibaYP.Validators;
using Domain.Entities;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Services;
using System.Text.Encodings.Web;
using System.Text.Unicode;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.OAuth;
using Services.Account;

var builder = WebApplication.CreateBuilder(args);

// ===== SMTP (Gmail) =====
var smtpSection = builder.Configuration.GetSection("Smtp");
var smtpSettings = smtpSection.Get<SmtpSettings>()
    ?? throw new InvalidOperationException("Не найдена секция Smtp в appsettings.json");
builder.Services.AddSingleton(smtpSettings);
builder.Services.AddTransient<IEmailSender, GmailEmailSender>();
// ===== MVC + JSON + FluentValidation =====
builder.Services
    .AddControllersWithViews()
    .AddJsonOptions(o =>
    {
        // Чтобы в Json не было \u0418\u043C\u044F, а сразу русский текст
        o.JsonSerializerOptions.Encoder = JavaScriptEncoder.Create(UnicodeRanges.All);
    });

builder.Services
    .AddFluentValidationAutoValidation()      // интеграция с ModelState (серверная)
    .AddFluentValidationClientsideAdapters(); // для jQuery unobtrusive

// Достаточно ОДНОГО вызова: он найдёт все валидаторы в этой сборке

builder.Services.AddValidatorsFromAssemblyContaining<LoginViewModelValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<RegisterViewModelValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<FestivalEditViewModelValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<FestivalImageEditViewModelValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<ContactMessageValidator>();

// ===== АУТЕНТИФИКАЦИЯ (cookies) =====
builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    })
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";
    })
    .AddGoogle(GoogleDefaults.AuthenticationScheme, options =>
    {
        var googleAuthSection = builder.Configuration.GetSection("Authentication:Google");
        options.ClientId = googleAuthSection["ClientId"]!;
        options.ClientSecret = googleAuthSection["ClientSecret"]!;
        options.CallbackPath = "/signin-google";

        // чтобы Google каждый раз показывал выбор аккаунта
        options.Events ??= new OAuthEvents();
        options.Events.OnRedirectToAuthorizationEndpoint = context =>
        {
            var uri = context.RedirectUri;

            // если уже есть параметры, просто добавим &prompt=select_account
            if (!uri.Contains("prompt=", StringComparison.OrdinalIgnoreCase))
            {
                uri += uri.Contains("?") ? "&prompt=select_account" : "?prompt=select_account";
            }

            context.Response.Redirect(uri);
            return Task.CompletedTask;
        };
    });

// ===== DbContext =====
builder.Services.AddDbContext<GastroFestDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("GastroFestConnection"))
);

// ===== РЕПОЗИТОРИИ / ХРАНИЛИЩА =====
builder.Services.AddScoped<IBaseStorage<ContactMessage>, ContactStorage>();

builder.Services.AddScoped<UserStorage>();
builder.Services.AddScoped<IBaseStorage<User>, UserStorage>();

builder.Services.AddScoped<IBaseStorage<Festival>, FestivalStorage>();
builder.Services.AddScoped<IBaseStorage<FestivalImage>, FestivalImageStorage>();

// ===== СЕРВИСЫ =====
builder.Services.AddScoped<ContactService>();
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<FestivalService>();
builder.Services.AddScoped<FestivalImageService>();

builder.Services.AddAuthorization();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
