
using EduCore.Application.Features.Payments.DTOs;
using EduCore.Application.Common.Pagination;

namespace EduCore.Application.Features.Payments.Interfaces;

public interface IPaymentService
{
    Task<PaymentResponse> CreateAsync(
        CreatePaymentRequest request,
        CancellationToken cancellationToken = default);

    Task<PagedResult<PaymentResponse>> GetAllAsync(
        PaginationParameters parameters,
        CancellationToken cancellationToken = default);

    Task<PaymentResponse> GetByIdAsync(
        int id,
        CancellationToken cancellationToken = default);

    Task<IEnumerable<PaymentResponse>> GetByStudentFeeAsync(
        int studentFeeId,
        CancellationToken cancellationToken = default);

    Task<PaymentResponse> UpdateAsync(
    int id,
    UpdatePaymentRequest request,
    CancellationToken cancellationToken = default);

    Task CancelAsync(
    int id,
    CancellationToken cancellationToken = default);

    Task SoftDeleteAsync(
        int id,
        CancellationToken cancellationToken = default);

}