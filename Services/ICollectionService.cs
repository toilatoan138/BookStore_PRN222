using BookStore.Models.Entities;

namespace BookStore.Services
{
    public interface ICollectionService
    {
        Task<List<Collection>> GetUserCollectionsAsync(string userId);
        Task<Collection?> GetCollectionByIdAsync(int collectionId, string? userId = null);
        Task<Collection> CreateCollectionAsync(string userId, string name, string? description, bool isPublic, string? coverColor);
        Task<bool> UpdateCollectionAsync(int collectionId, string userId, string name, string? description, bool isPublic, string? coverColor);
        Task<bool> DeleteCollectionAsync(int collectionId, string userId);
        Task<bool> AddBookToCollectionAsync(int collectionId, int bookId, string userId);
        Task<bool> RemoveBookFromCollectionAsync(int collectionId, int bookId, string userId);
    }
}
