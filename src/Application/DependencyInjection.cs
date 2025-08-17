using System.Reflection;
using CleanArchitectureBase.Application.Common.Behaviours;
using Microsoft.Extensions.DependencyInjection;

namespace CleanArchitectureBase.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddAutoMapper(Assembly.GetExecutingAssembly());

        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
        
        
        services.AddMediatR(cfg => {
            //Quét assembly hiện tại để tìm tất cả các handler và đăng ký
            cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
            //Đăng ký pipeline behaviors, để xử lý một cái gì đấy trước hoặc sau khi handler được gọi
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(UnhandledExceptionBehaviour<,>));//1
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(AuthorizationBehaviour<,>));//2
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(CleanDataPropertiesBehaviour<,>));//4
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehaviour<,>));//3
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(PerformanceBehaviour<,>)); //5
        });

        return services;
    }
}
