using CleanArchitectureBase.Application.Common.Models;

namespace CleanArchitectureBase.Application.Common.Mappings;

public static class MappingExtensions
{

    public static Task<PaginatedList<TDestination>> PaginatedListAsync<TDestination>(
        this IQueryable<TDestination> queryable,
        int pageNumber,
        int pageSize)
        where TDestination : class
    {
        return PaginatedList<TDestination>.CreateAsync(queryable.AsNoTracking(), pageNumber, pageSize);
    }
    
    /// <summary>
    /// AutoMapper
    /// </summary>
    /// <param name="queryable"></param>
    /// <param name="configuration"></param>
    /// <typeparam name="TDestination"></typeparam>
    /// <returns></returns>
    public static Task<List<TDestination>> ProjectToListAsync<TDestination>(
        this IQueryable queryable,
        IConfigurationProvider configuration)
        where TDestination : class
    {
        return queryable.ProjectTo<TDestination>(configuration).AsNoTracking().ToListAsync();
    }
}
