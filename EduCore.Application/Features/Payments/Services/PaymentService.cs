using AutoMapper;
using EduCore.Application.Common;
using EduCore.Application.Common.Exceptions;
using EduCore.Application.Common.Interfaces;
using EduCore.Application.Features.Payments.DTOs;
using EduCore.Application.Features.Payments.Interfaces;
using EduCore.Domain.Entities;
using EduCore.Domain.Entities.Enums;
using EduCore.Domain.Interfaces.Repositories;


namespace EduCore.Application.Features.Payments.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ICurrentUserService _currentUserService;

        public PaymentService(IUnitOfWork unitOfWork, IMapper mapper, ICurrentUserService currentUserService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _currentUserService = currentUserService;
        }

        public async Task<PaymentResponse> CreateAsync(CreatePaymentRequest request,
                         CancellationToken cancellationToken = default)
        {
            await _unitOfWork.BeginTransactionAsync(cancellationToken);

            try
            {
                var fee = await _unitOfWork.StudentFees
                    .GetForUpdateAsync(
                        request.StudentFeeId,
                        cancellationToken);

                if (fee is null)
                    throw new NotFoundException(ErrorCodes.StudentFee.NotFound);

                var remainingAmount = fee.TotalAmount - fee.PaidAmount;

                if (request.Amount > remainingAmount)
                {
                    throw new BadRequestException(ErrorCodes.Payment.InvalidAmount);
                }

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

        public async Task<IEnumerable<PaymentResponse>> GetAllAsync(
        CancellationToken cancellationToken = default)
        {
            if (_currentUserService.IsAdmin)
            {
                var payments = await _unitOfWork.Payments
                    .FindAsync(
                        x => !x.IsDeleted,
                        trackChanges: false,
                        cancellationToken: cancellationToken);

                return _mapper.Map<IEnumerable<PaymentResponse>>(payments);
            }

            if (_currentUserService.IsStudent)
            {
                var paymentsForStudent = await _unitOfWork.Payments
                .FindAsync(
                    x => !x.IsDeleted
                        && x.StudentFee.Enrollment.Student.UserId == _currentUserService.UserId,
                    trackChanges: false,
                    cancellationToken: cancellationToken);

                return _mapper.Map<IEnumerable<PaymentResponse>>(paymentsForStudent);
            }

            throw new ForbiddenException(ErrorCodes.User.Forbidden);
        }

        public async Task<PaymentResponse> GetByIdAsync(
        int id,
        CancellationToken cancellationToken = default)
        {

            if (_currentUserService.IsAdmin)
            {
                var payment = await _unitOfWork.Payments
                    .FirstOrDefaultAsync(
                        x => x.Id == id && !x.IsDeleted,
                        false,
                        cancellationToken);

                if (payment is null)
                    throw new NotFoundException(ErrorCodes.Payment.NotFound);

                return _mapper.Map<PaymentResponse>(payment);
            }

            if (_currentUserService.IsStudent)
            {
                var studentPayment = await _unitOfWork.Payments
                .FirstOrDefaultAsync(
                    x => x.Id == id
                        && !x.IsDeleted
                        && x.StudentFee.Enrollment.Student.UserId == _currentUserService.UserId,
                    false,
                    cancellationToken);

                if (studentPayment is null)
                    throw new NotFoundException(ErrorCodes.Payment.NotFound);

                return _mapper.Map<PaymentResponse>(studentPayment);
            }

            throw new ForbiddenException(ErrorCodes.User.Forbidden);
        }

        public async Task<IEnumerable<PaymentResponse>> GetByStudentFeeAsync(
            int studentFeeId,
            CancellationToken cancellationToken = default)
        {
            var fee = await _unitOfWork.StudentFees
                .ExistsAsync(
                    x => x.Id == studentFeeId && !x.IsDeleted,
                    cancellationToken: cancellationToken);

            if (!fee)
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
            await _unitOfWork.BeginTransactionAsync(cancellationToken);

            try
            {
                var paymentReference = await _unitOfWork.Payments
                    .FirstOrDefaultAsync(
                        x => x.Id == id && !x.IsDeleted,
                        trackChanges: false,
                        cancellationToken: cancellationToken);

                if (paymentReference is null)
                    throw new NotFoundException(ErrorCodes.Payment.NotFound);

                var fee = await _unitOfWork.StudentFees
                    .GetForUpdateAsync(
                        paymentReference.StudentFeeId,
                        cancellationToken);

                if (fee is null)
                    throw new NotFoundException(ErrorCodes.StudentFee.NotFound);

                var payment = await _unitOfWork.Payments
                    .FirstOrDefaultAsync(
                        x => x.Id == id && !x.IsDeleted,
                        trackChanges: true,
                        cancellationToken: cancellationToken);

                if (payment is null)
                    throw new NotFoundException(ErrorCodes.Payment.NotFound);

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
            await _unitOfWork.BeginTransactionAsync(cancellationToken);

            try
            {
                var paymentReference = await _unitOfWork.Payments
                    .FirstOrDefaultAsync(
                        x => x.Id == id && !x.IsDeleted,
                        trackChanges: false,
                        cancellationToken: cancellationToken);

                if (paymentReference is null)
                    throw new NotFoundException(ErrorCodes.Payment.NotFound);

                var fee = await _unitOfWork.StudentFees
                    .GetForUpdateAsync(
                        paymentReference.StudentFeeId,
                        cancellationToken);

                if (fee is null)
                    throw new NotFoundException(ErrorCodes.StudentFee.NotFound);

                var payment = await _unitOfWork.Payments
                    .FirstOrDefaultAsync(
                        x => x.Id == id && !x.IsDeleted,
                        trackChanges: true,
                        cancellationToken: cancellationToken);

                if (payment is null)
                    throw new NotFoundException(ErrorCodes.Payment.NotFound);

                if (payment.Status == PaymentStatus.Cancelled)
                    throw new BadRequestException(ErrorCodes.Payment.CannotUpdateCancelled);

                if (payment.Amount > fee.PaidAmount)
                {
                    throw new BadRequestException(ErrorCodes.Payment.InvalidAmount);
                }

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
