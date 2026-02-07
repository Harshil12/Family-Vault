using AutoMapper;
using FamilyVault.Application.Interfaces.Repositories;
using FamilyVault.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace FamilyVault.Application.Services;

public abstract class GenericService<TDto, TEntity>
    where TEntity : BaseEntity
{
    protected readonly IGenericRepository<TEntity> _repository;
    protected readonly IMapper _mapper;
    protected readonly ILogger _logger;

    protected GenericService(IGenericRepository<TEntity> repository, IMapper mapper, ILogger logger)
    {
        _repository = repository;
        _mapper = mapper;
        _logger = logger;
    }

    protected virtual async Task<IReadOnlyList<TDto>> GetAllAsync(CancellationToken cancellationToken)
    {
        var entities = await _repository.GetAllAsync(cancellationToken);
        return _mapper.Map<IReadOnlyList<TDto>>(entities);
    }

    protected virtual async Task<TDto> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var entity = await _repository.GetByIdAsync(id, cancellationToken);
        return _mapper.Map<TDto>(entity);
    }

    protected virtual async Task<TDto> CreateAsync(TEntity entity, Guid userId, CancellationToken cancellationToken)
    {
        entity.CreatedAt = DateTimeOffset.Now;
        entity.CreatedBy = userId.ToString();

        var result = await _repository.AddAsync(entity, cancellationToken);
        return _mapper.Map<TDto>(result);
    }

    protected virtual async Task<TDto> UpdateAsync(TEntity entity, Guid userId, CancellationToken cancellationToken)
    {
        entity.UpdatedAt = DateTimeOffset.Now;
        entity.UpdatedBy = userId.ToString();

        var result = await _repository.UpdateAsync(entity, cancellationToken);
        return _mapper.Map<TDto>(result);
    }

    protected virtual async Task DeleteAsync(Guid id, Guid userId, CancellationToken cancellationToken)
    {
        await _repository.DeleteByIdAsync(id, userId.ToString(), cancellationToken);
    }
}
