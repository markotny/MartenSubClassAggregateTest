using Marten;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace MartenSubClassAggregateTest;

internal class Service : BackgroundService
{
	private readonly IDocumentSession session;
	private readonly ILogger<Service> _logger;

	public Service(IDocumentSession session, ILogger<Service> logger)
	{
		this.session = session;
		_logger = logger;
	}

	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
		var fooCreated = new FooCreated(Guid.NewGuid());
		session.Events.Append(fooCreated.Id, fooCreated);

		var bar1Created = new Bar1Created(Guid.NewGuid());
		session.Events.Append(bar1Created.Id, bar1Created);

		var bar2Created = new Bar2Created(Guid.NewGuid());
		session.Events.Append(bar2Created.Id, bar2Created);

		await session.SaveChangesAsync();

		var all = await session.Query<Foo>().ToListAsync();
		foreach (var item in all)
		{
			_logger.LogInformation("Loaded foo {FooType}, id {Id}", item.GetType(), item.Id);
		}
	}
}
