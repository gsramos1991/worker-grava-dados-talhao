using AgroSolutions.Domain.Constants;
using AgroSolutions.Domain.Diagnostics;
using AgroSolutions.Domain.Interfaces;
using AgroSolutions.Domain.Models;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Data;
using System.Diagnostics;

namespace AgroSolutions.Infra.Data.Repository
{
    public class DataAccessRepository : IDataAccessRepository
    {
        private readonly IConfiguration _configuration;
        private readonly string _connectionString;

        public DataAccessRepository(IConfiguration configuration)
        {
            _configuration = configuration;
            _connectionString = _configuration["ConnectionStrings:AgroSolutionsDb"]
                ?? throw new InvalidOperationException("Connection string 'ConnectionStrings--AgroSolutionsDb' não encontrada.");
        }

        public async Task<bool> GravarDadosSensor(RegistroSensor registro)
        {
            using var activity = AgroSolutionsDiagnostics.Source.StartActivity(
                "db.gravar_sensor", ActivityKind.Client);

            activity?.SetTag("db.system", "sqlserver");
            activity?.SetTag("db.operation", "INSERT");
            activity?.SetTag("db.sql.table", "RegistroSensor");
            activity?.SetTag("talhao.id", registro.TalhaoId);
            activity?.SetTag("sensor.data_hora", registro.DataHora.ToString("o"));

            try
            {
                using IDbConnection dbConnection = new SqlConnection(_connectionString);

                var parameters = new
                {
                    TalhaoId = registro.TalhaoId,
                    DataHora = registro.DataHora,
                    TemperaturaValor = registro.TemperaturaValor,
                    UmidadeValor = registro.UmidadeValor,
                    VelocidadeVento = registro.VelocidadeVento,
                    StatusProcessamento = registro.StatusProcessamento,
                    EnumAlerta = registro.EnumAlerta,
                    DataProcessamento = registro.DataProcessamento
                };

                var rowsAffected = await dbConnection.ExecuteAsync(
                    QuerySql.InsereDados,
                    parameters,
                    commandType: CommandType.Text);

                var sucesso = rowsAffected > 0;
                activity?.SetTag("db.rows_affected", rowsAffected);
                activity?.SetStatus(sucesso ? ActivityStatusCode.Ok : ActivityStatusCode.Error, "Nenhuma linha afetada");

                return sucesso;
            }
            catch (SqlException ex)
            {
                activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
                activity?.SetTag("exception.type", nameof(SqlException));
                activity?.SetTag("exception.message", ex.Message);
                throw new InvalidOperationException($"Erro ao gravar dados do sensor no banco de dados: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
                activity?.SetTag("exception.type", ex.GetType().Name);
                activity?.SetTag("exception.message", ex.Message);
                throw new InvalidOperationException($"Erro inesperado ao gravar dados do sensor: {ex.Message}", ex);
            }
        }
    }
}
