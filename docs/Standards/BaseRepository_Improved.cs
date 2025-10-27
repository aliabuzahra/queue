using Microsoft.EntityFrameworkCore;
using VirtualQueue.Application.Common.Interfaces;
using VirtualQueue.Domain.Common;
using VirtualQueue.Infrastructure.Data;

namespace VirtualQueue.Infrastructure.Repositories;

/// <summary>
/// Base repository implementation providing common data access operations for entities.
/// </summary>
/// <typeparam name="T">The entity type that inherits from BaseEntity.</typeparam>
public abstract class BaseRepository<T> : IRepository<T> where T : BaseEntity
{
    #region Fields
    /// <summary>
    /// The database context for data access operations.
    /// </summary>
    protected readonly VirtualQueueDbContext _context;
    
    /// <summary>
    /// The database set for the entity type.
    /// </summary>
    protected readonly DbSet<T> _dbSet;
    #endregion

    #region Constructors
    /// <summary>
    /// Initializes a new instance of the <see cref="BaseRepository{T}"/> class.
    /// </summary>
    /// <param name="context">The database context.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when the context is null.
    /// </exception>
    protected BaseRepository(VirtualQueueDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _dbSet = context.Set<T>();
    }
    #endregion

    #region Public Methods
    /// <summary>
    /// Retrieves an entity by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the entity.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>
    /// The entity if found; otherwise, null.
    /// </returns>
    /// <exception cref="ArgumentException">
    /// Thrown when the id is empty.
    /// </exception>
    public virtual async Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("ID cannot be empty", nameof(id));
            
        return await _dbSet.FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
    }

    /// <summary>
    /// Retrieves all entities of the specified type.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>
    /// A collection of all entities.
    /// </returns>
    public virtual async Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet.ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Adds a new entity to the database.
    /// </summary>
    /// <param name="entity">The entity to add.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>
    /// The added entity.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when the entity is null.
    /// </exception>
    public virtual async Task<T> AddAsync(T entity, CancellationToken cancellationToken = default)
    {
        if (entity == null)
            throw new ArgumentNullException(nameof(entity));
            
        await _dbSet.AddAsync(entity, cancellationToken);
        return entity;
    }

    /// <summary>
    /// Updates an existing entity in the database.
    /// </summary>
    /// <param name="entity">The entity to update.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>
    /// A task representing the asynchronous operation.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when the entity is null.
    /// </exception>
    public virtual Task UpdateAsync(T entity, CancellationToken cancellationToken = default)
    {
        if (entity == null)
            throw new ArgumentNullException(nameof(entity));
            
        _dbSet.Update(entity);
        return Task.CompletedTask;
    }

    /// <summary>
    /// Deletes an entity from the database.
    /// </summary>
    /// <param name="entity">The entity to delete.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>
    /// A task representing the asynchronous operation.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when the entity is null.
    /// </exception>
    /// <remarks>
    /// This method performs a hard delete. For soft delete functionality,
    /// use the domain entity's soft delete methods instead.
    /// </remarks>
    public virtual Task DeleteAsync(T entity, CancellationToken cancellationToken = default)
    {
        if (entity == null)
            throw new ArgumentNullException(nameof(entity));
            
        _dbSet.Remove(entity);
        return Task.CompletedTask;
    }

    /// <summary>
    /// Saves all changes made to the database context.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>
    /// A task representing the asynchronous operation.
    /// </returns>
    /// <exception cref="DbUpdateException">
    /// Thrown when an error occurs while saving changes to the database.
    /// </exception>
    public virtual async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex)
        {
            throw new InvalidOperationException("An error occurred while saving changes to the database", ex);
        }
    }
    #endregion

    #region Protected Methods
    /// <summary>
    /// Validates that an entity is not null.
    /// </summary>
    /// <param name="entity">The entity to validate.</param>
    /// <param name="paramName">The name of the parameter.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when the entity is null.
    /// </exception>
    protected static void ValidateEntity(T entity, string paramName)
    {
        if (entity == null)
            throw new ArgumentNullException(paramName);
    }

    /// <summary>
    /// Validates that an ID is not empty.
    /// </summary>
    /// <param name="id">The ID to validate.</param>
    /// <param name="paramName">The name of the parameter.</param>
    /// <exception cref="ArgumentException">
    /// Thrown when the ID is empty.
    /// </exception>
    protected static void ValidateId(Guid id, string paramName)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("ID cannot be empty", paramName);
    }
    #endregion
}
