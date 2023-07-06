using Marten.Events.Aggregation;
using Marten.Events.Projections;
using Marten.Internal.Sessions;
using Marten.Linq.SoftDeletes;

namespace MartenSubClassAggregateTest;

public record FooCreated(Guid Id);
public record Bar1Created(Guid Id);
public record Bar2Created(Guid Id, string Name);
public record Bar2NameChanged(string Name);

public class Foo
{
	public Guid Id { get; set; }

	public void Apply(FooCreated @event)
	{
		Id = @event.Id;
	}
}

public class Bar1 : Foo
{
	public void Apply(Bar1Created @event)
	{
		Id = @event.Id;
	}
}

public class Bar2 : Foo
{
	public string Name { get; set; }

	public void Apply(Bar2Created @event)
	{
		Id = @event.Id;
		Name = @event.Name;
	}

	public void Apply(Bar2NameChanged @event)
	{
		Name = @event.Name;
	}
}


public class FooAggregate : CustomProjection<Foo, Guid>
{
	public FooAggregate()
	{
		AggregateByStream();
	}

	public override ValueTask ApplyChangesAsync(DocumentSessionBase session, EventSlice<Foo, Guid> slice, CancellationToken cancellation, ProjectionLifecycle lifecycle = ProjectionLifecycle.Inline)
	{
		var aggregate = slice.Aggregate;

		foreach (var data in slice.AllData())
		{
			switch (data)
			{
				case FooCreated fooCreated:
					aggregate = new Foo();
					aggregate.Apply(fooCreated);
					break;
				case Bar1Created bar1Created:
					var bar1 = new Bar1();
					bar1.Apply(bar1Created);
					aggregate = bar1;
					break;
				case Bar2Created bar2Created:
					var bar2 = new Bar2();
					bar2.Apply(bar2Created);
					aggregate = bar2;
					break;
				case Bar2NameChanged bar2NameChanged when aggregate is Bar2 bar2Aggregate:
					bar2Aggregate.Apply(bar2NameChanged);
					break;
			}
		}

		// Apply any updates!
		if (aggregate != null)
		{
			session.Store(aggregate);
		}

		// We didn't do anything that required an asynchronous call
		return new ValueTask();
	}
}
