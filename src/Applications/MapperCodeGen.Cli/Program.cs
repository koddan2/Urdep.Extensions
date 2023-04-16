// See https://aka.ms/new-console-template for more information
using MapperCodeGen.Cli;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

//Example.Run();

var host = Host.CreateApplicationBuilder();

host.Services.AddScoped<Example>();

var app = host.Build();

var example = app.Services.CreateScope().ServiceProvider.GetRequiredService<Example>();
example.Run();
//app.Run();
