using FCG.Domain.Core.Notifications;
using FCG.Domain.Handlers;
using FCG.Domain.Interfaces.Commons;
using FCG.Domain.Repositories;
using FCG.Infrastructure.Repositories;
using FCG.Infrastructure;
using FCG.Service.Interfaces;
using FCG.Service;
using MediatR;

namespace FCG.Presentation.Configuration
{
    public static class ServicesConfiguration
    {
        public static IServiceCollection AddAppServices(this IServiceCollection services)
        {
            services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IClientService, ClientService>();
            services.AddScoped<IGameService, GameService>();
            services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<Program>());
            services.AddScoped<IMediatorHandler, MediatorHandler>();
            services.AddScoped<INotificationHandler<DomainNotification>, DomainNotificationHandler>();
            services.AddAutoMapper(typeof(Service.AutoMapper.AutoMapperProfile).Assembly);
            return services;
        }
    }
}
