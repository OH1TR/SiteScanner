using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace DataModel.AdvancedServiceProvider
{
    /// <summary>
    /// Service provider that allows for dynamic adding of new services
    /// </summary>
    public interface IAdvancedServiceProvider : IServiceProvider
    {
        /// <summary>
        /// Add services to this collection
        /// </summary>
        IServiceCollection ServiceCollection { get; }
    }
}
