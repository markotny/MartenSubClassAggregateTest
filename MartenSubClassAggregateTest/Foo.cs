using Marten.Events;

namespace MartenSubClassAggregateTest;

public record FooCreated(Guid Id);
public record Bar1Created(Guid Id);
public record Bar2Created(Guid Id);

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
	public void Apply(Bar2Created @event)
	{
		Id = @event.Id;
	}
}