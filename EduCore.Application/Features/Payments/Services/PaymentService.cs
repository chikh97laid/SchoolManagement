using AutoMapper;
using EduCore.Application.Common.Exceptions;
using EduCore.Application.Features.Payments.DTOs;
using EduCore.Application.Features.Payments.Interfaces;
using EduCore.Domain.Entities;
using EduCore.Domain.Entities.Enums;
using EduCore.Domain.Interfaces.Repositories;
using EduCore.Application.Common;


namespace EduCore.Application.Features.Payments.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public PaymentService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<PaymentResponse> CreateAsync(CreatePaymentRequest request,
                         CancellationToken cancellationToken = default)
        {

            var fee = await _unitOfWork.StudentFees
            .FirstOrDefaultAsync(
                x => (x.Id == request.StudentFeeId && !x.IsDeleted),
                true,
                cancellationToken);

            if (fee is null)
                throw new NotFoundException(ErrorCodes.StudentFee.NotFound);

            var remainingAmount = fee.TotalAmount - fee.PaidAmount;

            if (request.Amount > remainingAmount)
            {
                throw new BadRequestException(ErrorCodes.Payment.InvalidAmount);
            }

            await _unitOfWork.BeginTransactionAsync(cancellationToken);

            try
            {

                var payment = _mapper.Map<Payment>(request);

                await _unitOfWork.Payments.AddAsync(
                    payment,
                    cancellationToken);

                fee.PaidAmount += request.Amount;

                UpdateFeeStatus(fee);

                await _unitOfWork.SaveChangesAsync(
                    cancellationToken);

                await _unitOfWork.CommitTransactionAsync(
                    cancellationToken);

                return _mapper.Map<PaymentResponse>(payment);
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync(
                    cancellationToken);

                throw;
            }
        }

        public async Task<PaymentResponse> GetByIdAsync(
        int id,
        CancellationToken cancellationToken = default)
        {
            var payment = await _unitOfWork.Payments
                .FirstOrDefaultAsync(
                    x => x.Id == id && !x.IsDeleted,
                    trackChanges: false,
                    cancellationToken: cancellationToken);

            if (payment is null)
                throw new NotFoundException(ErrorCodes.Payment.NotFound);

            return _mapper.Map<PaymentResponse>(payment);
        }

        public async Task<IEnumerable<PaymentResponse>> GetByStudentFeeAsync(
            int studentFeeId,
            CancellationToken cancellationToken = default)
        {
            var fee = await _unitOfWork.StudentFees
                .FirstOrDefaultAsync(
                    x => x.Id == studentFeeId && !x.IsDeleted,
                    trackChanges: false,
                    cancellationToken: cancellationToken);

            if (fee is null)
                throw new NotFoundException(ErrorCodes.StudentFee.NotFound);

            var payments = await _unitOfWork.Payments
                .FindAsync(
                    x => x.StudentFeeId == studentFeeId
                         && !x.IsDeleted,
                    trackChanges: false,
                    cancellationToken: cancellationToken);

            return _mapper.Map<IEnumerable<PaymentResponse>>(
                payments);
        }

        private static void UpdateFeeStatus(StudentFee fee)
        {
            if (fee.PaidAmount == 0)
            {
                fee.Status = StudentFeeStatus.Pending;
            }
            else if (fee.PaidAmount < fee.TotalAmount)
            {
                fee.Status = StudentFeeStatus.PartiallyPaid;
            }
            else
            {
                fee.Status = StudentFeeStatus.Paid;
            }
        }


        public async Task<PaymentResponse> UpdateAsync(
        int id,
        UpdatePaymentRequest request,
        CancellationToken cancellationToken = default)
        {

            var payment = await _unitOfWork.Payments
            .FirstOrDefaultAsync(
            x => x.Id == id && !x.IsDeleted,
            trackChanges: true,
            cancellationToken: cancellationToken);

            if (payment is null)
                throw new NotFoundException(ErrorCodes.Payment.NotFound);

            var fee = await _unitOfWork.StudentFees
                .FirstOrDefaultAsync(
                    x => x.Id == payment.StudentFeeId
                         && !x.IsDeleted,
                    trackChanges: true,
                    cancellationToken: cancellationToken);

            if (fee is null)
                throw new NotFoundException(ErrorCodes.StudentFee.NotFound);

            if (payment.Status == PaymentStatus.Cancelled)
                throw new BadRequestException(ErrorCodes.Payment.CannotUpdateCancelled);

            // PaidAmount currently includes the old payment.
            // Remove it first.
            var paidWithoutCurrentPayment = fee.PaidAmount - payment.Amount;

            var newPaidAmount = paidWithoutCurrentPayment + request.Amount;

            if (newPaidAmount > fee.TotalAmount)
            {
                throw new BadRequestException(ErrorCodes.Payment.InvalidAmount);
            }


            await _unitOfWork.BeginTransactionAsync(cancellationToken);

            try
            {
                _mapper.Map(request, payment);

                fee.PaidAmount = newPaidAmount;

                UpdateFeeStatus(fee);

                await _unitOfWork.SaveChangesAsync(cancellationToken);

                await _unitOfWork.CommitTransactionAsync(cancellationToken);

                return _mapper.Map<PaymentResponse>(payment);
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                throw;
            }
        }


        public async Task CancelAsync(
        int id,
        CancellationToken cancellationToken = default)
        {

            var payment = await _unitOfWork.Payments
            .FirstOrDefaultAsync(
                x => x.Id == id && !x.IsDeleted,
                trackChanges: true,
                cancellationToken: cancellationToken);

            if (payment is null)
                throw new NotFoundException(ErrorCodes.Payment.NotFound);

            if (payment.Status == PaymentStatus.Cancelled)
                throw new BadRequestException(ErrorCodes.Payment.CannotUpdateCancelled);

            var fee = await _unitOfWork.StudentFees
                .FirstOrDefaultAsync(
                    x => x.Id == payment.StudentFeeId
                         && !x.IsDeleted,
                    trackChanges: true,
                    cancellationToken: cancellationToken);

            if (fee is null)
                throw new NotFoundException(ErrorCodes.StudentFee.NotFound);

            if (payment.Amount > fee.PaidAmount)
            {
                throw new BadRequestException(ErrorCodes.Payment.InvalidAmount);
            }

            await _unitOfWork.BeginTransactionAsync(cancellationToken);

            try
            {

                // Cancel the payment
                payment.Status = PaymentStatus.Cancelled;

                // Remove its amount from the fee
                fee.PaidAmount -= payment.Amount;

                // Recalculate fee status
                UpdateFeeStatus(fee);

                await _unitOfWork.SaveChangesAsync(
                    cancellationToken);

                await _unitOfWork.CommitTransactionAsync(
                    cancellationToken);
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync(
                    cancellationToken);

                throw;
            }
        }

    }
}
