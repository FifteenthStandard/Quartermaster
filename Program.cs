global using System.CommandLine;

var app = new QuartermasterCommand();

return await app.InvokeAsync(args);