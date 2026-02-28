using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgroSolutions.Domain.Constants
{
    public static class QuerySql
    {
        public const string InsereDados = @"
            INSERT INTO RegistroSensor 
            (
                TalhaoId, 
                DataHora, 
                TemperaturaValor, 
                UmidadeValor, 
                VelocidadeVento, 
                StatusProcessamento, 
                EnumAlerta, 
                DataProcessamento
            )
            VALUES 
            (
                @TalhaoId, 
                @DataHora, 
                @TemperaturaValor, 
                @UmidadeValor, 
                @VelocidadeVento, 
                @StatusProcessamento, 
                @EnumAlerta, 
                @DataProcessamento
            )";

        
    }
}
