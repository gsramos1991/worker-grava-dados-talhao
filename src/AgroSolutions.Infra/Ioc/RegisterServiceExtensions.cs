using AgroSolutions.Business.Interface;
using AgroSolutions.Business.Services;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgroSolutions.Infra.Ioc
{
    public static class RegisterServiceExtensions
    {
        public static IServiceCollection AddRegisterServices(this IServiceCollection services)
        {
            services.AddScoped<IDadosSensorService, DadosSensorService>();
            return services;
        }
    }
}
