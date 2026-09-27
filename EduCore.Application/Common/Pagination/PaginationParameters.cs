
namespace EduCore.Application.Common.Pagination
{
    public class PaginationParameters
    {
        private const int MaxPageSize = 100;

        private int _pageNumber = 1;

        public int PageNumber
        {
            get => _pageNumber;
            set => _pageNumber = value < 1 ? 1 : value;
        }

        private int _pageSize = 20;

        public int PageSize
        {
            get => _pageSize;
            set => _pageSize = value > MaxPageSize
                ? MaxPageSize
                : value < 1
                    ? 1
                    : value;
        }
    }

}
