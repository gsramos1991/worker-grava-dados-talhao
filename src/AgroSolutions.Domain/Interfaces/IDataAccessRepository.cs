using AgroSolutions.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgroSolutions.Domain.Interfaces
{
    public interface IDataAccessRepository
    {
        Task<bool> GravarDadosSensor(RegistroSensor registro);
    }
}
