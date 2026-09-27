using EduCore.Application.Common.Pagination;

namespace EduCore.UnitTests.Services;

public class PaginationParametersTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(int.MinValue)]
    public void PageNumber_LessThanOne_IsClampedToOne(int invalidPageNumber)
    {
        var parameters = new PaginationParameters
        {
            PageNumber = invalidPageNumber
        };

        Assert.Equal(1, parameters.PageNumber);
    }
}