using Marten.Services;
using Marten;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace MartenSubClassAggregateTest;

public class CommandLogger : IMartenLogger, IMartenSessionLogger
{
	private readonly ILogger<CommandLogger> _logger;
	public CommandLogger(ILogger<CommandLogger> logger)
	{
		_logger = logger;
	}

	public IMartenSessionLogger StartSession(IQuerySession session)
	{
		return this;
	}

	public void SchemaChange(string sql)
	{
		_logger.LogInformation("Executing DDL change: {sql}", sql);
	}

	public void LogSuccess(NpgsqlCommand command)
	{
		var parameters = ParametersAsString(command.Parameters);
		_logger.LogInformation("Executed db command \"{command}\" with parameters [{parameters}]", command.CommandText, parameters);
	}

	public void LogFailure(NpgsqlCommand command, Exception ex)
	{
		var parameters = ParametersAsString(command.Parameters);
		_logger.LogError(ex, "Failed to execute db command \"{command}\" with parameters [{parameters}]", command.CommandText, parameters);
	}

	public void RecordSavedChanges(IDocumentSession session, IChangeSet commit) { }

	public void OnBeforeExecute(NpgsqlCommand command) { }

	private static string ParametersAsString(NpgsqlParameterCollection parameters)
		=> string.Join(", ", parameters.OfType<NpgsqlParameter>().Select(p => $"{p.ParameterName}: {p.Value}"));
}