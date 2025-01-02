using clsCms.Interfaces;
using clsCms.Services;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace fncTickWorks.Config
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddFunctionServices(this IServiceCollection services)
        {
            services.AddScoped<ITickServices, TickServices>();
            return services;
        }
    }
}
