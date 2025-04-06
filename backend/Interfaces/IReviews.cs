using LibraryAPI.DTO;

public interface IReviews 
{
    Task<IEnumerable<ReviewDto>> GetAllReviewsAsync();
    Task<ReviewDto> GetReviewByIdAsync(int id);
    Task<ReviewDto> AddReviewAsync(ReviewDto reviewDto);
    Task<ReviewDto> UpdateReviewAsync(int id, ReviewDto reviewDto);
    Task DeleteReviewAsync(int id);
}