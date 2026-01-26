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
using SSDI.RequestMonitoring.UI.Contracts.Requests.Common;
using SSDI.RequestMonitoring.UI.Contracts.Requests.JobOrder;
using SSDI.RequestMonitoring.UI.Contracts.Requests.Purchase;
using SSDI.RequestMonitoring.UI.Contracts.Users;
using SSDI.RequestMonitoring.UI.Handlers;
using SSDI.RequestMonitoring.UI.Helpers.Export;
using SSDI.RequestMonitoring.UI.Helpers.States;
using SSDI.RequestMonitoring.UI.Providers;
using SSDI.RequestMonitoring.UI.Services;
using SSDI.RequestMonitoring.UI.Services.Base;
using SSDI.RequestMonitoring.UI.Services.MasterData;
using SSDI.RequestMonitoring.UI.Services.Requests;
using SSDI.RequestMonitoring.UI.Services.Requests.JobOrder;
using SSDI.RequestMonitoring.UI.Services.Requests.Purchase;
using SSDI.RequestMonitoring.UI.Services.Users;
using System.Reflection;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
builder.Services.AddFluentUIComponents();

builder.Services.AddTransient<JwtAuthorizationMessageHandler>();
//builder.Services.AddHttpClient<IClient, Client>(client => client.BaseAddress = new Uri("https://localhost:7042"))
//builder.Services.AddHttpClient<IClient, Client>(client => client.BaseAddress = new Uri("http://192.168.1.96:4116/"))
builder.Services.AddHttpClient<IClient, Client>(client => client.BaseAddress = new Uri("http://sonicsales.net:4116/"))
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

builder.Services.AddTransient<ExportRequest>();

builder.Services.AddScoped<IPurchaseRequestSvc, PurchaseRequestSvc>();

builder.Services.AddScoped<IRSSlipSvc, RSSlipSvc>();
builder.Services.AddScoped<IPOSlipSvc, POSlipSvc>();
builder.Services.AddScoped<IAttachSvc, AttachSvc>();

builder.Services.AddScoped<IJobOrderSvc, JobOrderSvc>();

builder.Services.AddScoped<ISystemConfigSvc, SystemConfigSvc>();
builder.Services.AddScoped<IDivisionSvc, DivisionSvc>();
builder.Services.AddScoped<IDepartmentSvc, DepartmentSvc>();

builder.Services.AddScoped<IUserSvc, UserSvc>();

builder.Services.AddAutoMapper(Assembly.GetExecutingAssembly());

//builder.Logging.SetMinimumLevel(LogLevel.Warning);

//builder.Logging.AddFilter("System.Net.Http.HttpClient", LogLevel.None);
//builder.Logging.AddFilter("System.Net.Http", LogLevel.None);

await builder.Build().RunAsync();