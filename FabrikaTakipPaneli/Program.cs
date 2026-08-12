using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using FabrikaTakipPaneli.Authorization;
using FabrikaTakipPaneli.Data;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = false)
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddAuthorizationBuilder()
    .AddPolicy(AppPolicies.AdminOnly, policy => policy.RequireRole(AppRoles.Admin))
    .AddPolicy(AppPolicies.ViewProducts, policy => policy.RequireRole(AppRoles.Admin, AppRoles.Personel))
    .AddPolicy(AppPolicies.ManageProducts, policy => policy.RequireRole(AppRoles.Admin))
    .AddPolicy(AppPolicies.EnterStock, policy => policy.RequireRole(AppRoles.Admin, AppRoles.Personel));

// Add services to the container.
builder.Services.AddRazorPages(options =>
{
    options.Conventions.AuthorizeFolder("/Products", AppPolicies.ViewProducts);
    options.Conventions.AuthorizePage("/Products/Create", AppPolicies.ManageProducts);
    options.Conventions.AuthorizePage("/Products/Edit", AppPolicies.ManageProducts);
    options.Conventions.AuthorizePage("/Products/Delete", AppPolicies.ManageProducts);

    options.Conventions.AuthorizeFolder("/StockEntries", AppPolicies.EnterStock);

    options.Conventions.AuthorizeFolder("/Dashboard", AppPolicies.ViewProducts);

    options.Conventions.AuthorizeFolder("/Admin", AppPolicies.AdminOnly);
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    await RoleSeeder.SeedAsync(scope.ServiceProvider);
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();
app.MapRazorPages()
   .WithStaticAssets();

app.Run();
