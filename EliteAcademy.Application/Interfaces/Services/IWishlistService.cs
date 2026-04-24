using EliteAcademy.Application.DTOs.Wishlist;
using EliteAcademy.Application.Wrappers;

namespace EliteAcademy.Application.Interfaces.Services
{
    public interface IWishlistService
    {
        Task<Result<List<WishlistDto>>> GetMyWishlistAsync();
        Task<Result<HashSet<int>>>      GetMyWishlistedClassIdsAsync();
        Task<Result<bool>>              AddAsync(int classId);
        Task<Result<bool>>              RemoveAsync(int wishlistId);
    }
}
