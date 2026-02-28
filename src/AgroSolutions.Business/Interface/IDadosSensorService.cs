using AgroSolutions.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgroSolutions.Business.Interface
{
    public interface IDadosSensorService
    {
        Task<bool> ProcessarDadosSensor(RegistroSensor registro);
    }
}
