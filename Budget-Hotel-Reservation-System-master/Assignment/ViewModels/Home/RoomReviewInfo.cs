namespace Assignment.ViewModels.Home
{
    /// <summary>
    /// View model for displaying review statistics for a room type.
    /// Contains the total number of reviews and average rating.
    /// </summary>
    public class RoomReviewInfo
    {
        public int ReviewCount { get; set; }
        public double AverageRating { get; set; }
    }
}

