using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgroSolutions.Business.Model
{
    public class RegistroSensor
    {
        public Guid TalhaoId { get; set; }
        public DateTime DataHora { get; set; }
        public double TemperaturaValor { get; set; }
        public int UmidadeValor { get; set; }
        public double VelocidadeVento { get; set; }
        public int StatusProcessamento { get; set; }
        public string? EnumAlerta { get; set; }
        public DateTime? DataProcessamento { get; set; }
    }
}
