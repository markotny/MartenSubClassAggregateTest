using Marten;
using Marten.Events.Projections;
using MartenSubClassAggregateTest;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var builder = Host.CreateApplicationBuilder();

builder.Services.AddMarten((options) =>
{
	using (var sp = builder.Services.BuildServiceProvider())
	{
		options.Logger(new CommandLogger(sp.GetRequiredService<ILogger<CommandLogger>>()));
	}
	options.Connection("Host=localhost;Port=5444;Username=postgres;Password=admin;Database=test");

	options.Schema.For<Foo>()
		.AddSubClass<Bar1>()
		.AddSubClass<Bar2>();

	options.Projections.Snapshot<Bar1>(SnapshotLifecycle.Inline);
	options.Projections.Snapshot<Bar2>(SnapshotLifecycle.Inline);
	options.Projections.Snapshot<Foo>(SnapshotLifecycle.Inline);
}).UseLightweightSessions().ApplyAllDatabaseChangesOnStartup();

builder.Services.AddHostedService<Service>();

var host = builder.Build();

await host.RunAsync();