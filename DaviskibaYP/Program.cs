using DAL;
using DAL.Interfaces;
using DAL.Storage;
// наши пространства имён с валидаторами
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

var builder = WebApplication.CreateBuilder(args);

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
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/";
        options.AccessDeniedPath = "/";
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
