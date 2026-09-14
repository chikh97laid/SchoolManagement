
using EduCore.Application.Features.Payments.DTOs;

namespace EduCore.Application.Features.Payments.Interfaces;

public interface IPaymentService
{
    Task<PaymentResponse> CreateAsync(
        CreatePaymentRequest request,
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

}