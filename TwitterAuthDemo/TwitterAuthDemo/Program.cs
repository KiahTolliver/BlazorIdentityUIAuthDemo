using Auth0.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using Tweetinvi;
using TwitterAuthDemo.Client.Pages;
using TwitterAuthDemo.Components;
using TwitterAuthDemo.Data;
using TwitterAuthDemo.Identity;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveWebAssemblyComponents();

builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<UserAccessor>();
builder.Services.AddScoped<IdentityRedirectManager>();
builder.Services.AddScoped<AuthenticationStateProvider, PersistingServerAuthenticationStateProvider>();
//Add Twitter Login
//builder.Services.AddAuthentication(IdentityConstants.ApplicationScheme)
//    .AddTwitter(options =>
//    {
//        options.ClientId = builder.Configuration["Twitter:ApiKey"];
//        options.ClientSecret = builder.Configuration["Twitter:ApiSecret"];
//    })
//    .AddIdentityCookies();

//Add Auth0 Login
//builder.Services.AddAuth0WebAppAuthentication(options =>
//{
//    options.Domain = builder.Configuration["Auth0:Domain"];
//    options.ClientId = builder.Configuration["Auth0:ClientId"];
//});



builder.Services.AddIdentityCore<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddSignInManager()
    .AddDefaultTokenProviders();


builder.Services.AddAuthorization();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));


builder.Services.AddDatabaseDeveloperPageExceptionFilter();
builder.Services.AddSingleton<IEmailSender, NoOpEmailSender>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(Counter).Assembly);

// Add additional endpoints required by the Identity /Account Razor components.
app.MapAdditionalIdentityEndpoints();

app.MapPost("/tweet", async context =>
{

    if (context.User.Identity?.IsAuthenticated == false)
    {
        context.Response.StatusCode = 401;
        return;
    }

    //post tweet
    var userClient = new TwitterClient((Tweetinvi.Models.IReadOnlyTwitterCredentials)context.User);
    var tweet = await userClient.Tweets.PublishTweetAsync("I wrote this tweet with C#");

});

app.Run();
