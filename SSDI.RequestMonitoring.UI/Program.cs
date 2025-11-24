global using SSDI.RequestMonitoring.UI.Helpers;
global using SSDI.RequestMonitoring.UI.Models.Enums;
global using SSDI.RequestMonitoring.UI.Models.Enums.Components;
using Blazored.LocalStorage;
using Blazored.SessionStorage;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.FluentUI.AspNetCore.Components;
using SSDI.RequestMonitoring.UI;
using SSDI.RequestMonitoring.UI.Contracts;
using SSDI.RequestMonitoring.UI.Contracts.MasterData;
using SSDI.RequestMonitoring.UI.Contracts.Requests;
using SSDI.RequestMonitoring.UI.Contracts.Users;
using SSDI.RequestMonitoring.UI.Handlers;
using SSDI.RequestMonitoring.UI.Helpers.States;
using SSDI.RequestMonitoring.UI.Providers;
using SSDI.RequestMonitoring.UI.Services;
using SSDI.RequestMonitoring.UI.Services.Base;
using SSDI.RequestMonitoring.UI.Services.MasterData;
using SSDI.RequestMonitoring.UI.Services.Requests;
using SSDI.RequestMonitoring.UI.Services.Users;
using System.Reflection;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
builder.Services.AddFluentUIComponents();

builder.Services.AddTransient<JwtAuthorizationMessageHandler>();
builder.Services.AddHttpClient<IClient, Client>(client => client.BaseAddress = new Uri("https://localhost:7042"))
    .AddHttpMessageHandler<JwtAuthorizationMessageHandler>();

builder.Services.AddBlazoredLocalStorage();
builder.Services.AddBlazoredSessionStorage();
builder.Services.AddAuthorizationCore();
builder.Services.AddScoped<ApiAuthenticationStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider, ApiAuthenticationStateProvider>();

builder.Services.AddScoped<IAuthenticationSvc, AuthenticationSvc>();

builder.Services.AddScoped<IUIStateService, UIStateService>();
builder.Services.AddScoped<CurrentUser>();
builder.Services.AddScoped<Utils>();

builder.Services.AddScoped<IPurchaseRequestSvc, PurchaseRequestSvc>();
builder.Services.AddScoped<IPRAttachSvc, PRAttachSvc>();
builder.Services.AddScoped<IPRRequisitionSvc, PRRequisitionSvc>();

builder.Services.AddScoped<IJobOrderSvc, JobOrderSvc>();

builder.Services.AddScoped<IDivisionSvc, DivisionSvc>();
builder.Services.AddScoped<IDepartmentSvc, DepartmentSvc>();

builder.Services.AddScoped<IUserSvc, UserSvc>();

builder.Services.AddAutoMapper(Assembly.GetExecutingAssembly());

await builder.Build().RunAsync();