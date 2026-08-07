using Discord.Interactions;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Mauren.Discord.Infrastructure.Modules
{
    internal interface IModuleRegistry
    {
        Task<IServiceProvider> GetServiceProviderAsync(ModuleInfo moduleInfo, CancellationToken cancellationToken = default);
    }
}
