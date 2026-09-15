using AutoMapper;
using EduCore.Application.Common;
using EduCore.Application.Common.Exceptions;
using EduCore.Application.Features.Payments.DTOs;
using EduCore.Application.Features.Payments.Services;
using EduCore.Domain.Entities;
using EduCore.Domain.Entities.Enums;
using EduCore.Domain.Interfaces.Repositories;
using Moq;
using System.Linq.Expressions;

namespace EduCore.UnitTests.Services
{
    public class PaymentServiceTests
    {
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<IMapper> _mapperMock;

        private readonly PaymentService _service;

        public PaymentServiceTests()
        {
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _mapperMock = new Mock<IMapper>();

            _service = new PaymentService(_unitOfWorkMock.Object, _mapperMock.Object);
        }

        [Fact]
        public async Task CreateAsync_ShouldThrowNotFound_WhenFeeDoesNotExist()
        {
            // Arrange
            var request = new CreatePaymentRequest
            {
                StudentFeeId = 10,
                Amount = 500
            };

            _unitOfWorkMock
                .Setup(x => x.StudentFees.FirstOrDefaultAsync(
                    It.IsAny<Expression<Func<StudentFee, bool>>>(),
                    true,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync((StudentFee?)null);

            // Act
            var act = () => _service.CreateAsync(
                request,
                CancellationToken.None);

            // Assert
            await Assert.ThrowsAsync<NotFoundException>(act);
        }

        [Fact]
        public async Task CreateAsync_ShouldThrowBadRequest_WhenAmountExceedsRemaining()
        {
            // Arrange
            var request = new CreatePaymentRequest
            {
                StudentFeeId = 10,
                Amount = 1500
            };

            var fee = new StudentFee
            {
                Id = 10,
                TotalAmount = 3000,
                PaidAmount = 2000
            };

            // Mock StudentFees 
            _unitOfWorkMock
                .Setup(x => x.StudentFees.FirstOrDefaultAsync(
                    It.IsAny<Expression<Func<StudentFee, bool>>>(),
                    true,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(fee);


            // Act
            var act = () => _service.CreateAsync(
                request,
                CancellationToken.None);

            // Assert
            await Assert.ThrowsAsync<BadRequestException>(act);
        }


        [Fact]
        public async Task CreateAsync_ShouldSetFeeAsPartiallyPaid_WhenPaymentIsLessThanRemaining()
        {
            // Arrange
            var request = new CreatePaymentRequest
            {
                StudentFeeId = 10,
                Amount = 500
            };

            var fee = new StudentFee
            {
                Id = 10,
                TotalAmount = 3000,
                PaidAmount = 0, 
                Status = StudentFeeStatus.Pending
            };

            _unitOfWorkMock
                .Setup(x => x.StudentFees.FirstOrDefaultAsync(
                    It.IsAny<Expression<Func<StudentFee, bool>>>(),
                    true,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(fee);

            _unitOfWorkMock.Setup(x => x.BeginTransactionAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
            _unitOfWorkMock.Setup(x => x.CommitTransactionAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
            _unitOfWorkMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).Returns(Task.FromResult(1));
            _unitOfWorkMock.Setup(x => x.Payments.AddAsync(It.IsAny<Payment>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

            var payment = new Payment
            {
                StudentFeeId = 10,
                Amount = 500
            };

            _mapperMock
                .Setup(x => x.Map<Payment>(request))
                .Returns(payment);

            _mapperMock
                .Setup(x => x.Map<PaymentResponse>(payment))
                .Returns(new PaymentResponse());

            // Act
            await _service.CreateAsync(request, CancellationToken.None);

            // Assert
            Assert.Equal(500, fee.PaidAmount);
            Assert.Equal(StudentFeeStatus.PartiallyPaid, fee.Status);
        }


        [Fact]
        public async Task CreateAsync_ShouldSetFeeAsPaid_WhenPaymentIsEqualToRemaining()
        {
            // Arrange
            var request = new CreatePaymentRequest
            {
                StudentFeeId = 10,
                Amount = 2000
            };

            var fee = new StudentFee
            {
                Id = 10,
                TotalAmount = 3000,
                PaidAmount = 1000, 
                Status = StudentFeeStatus.PartiallyPaid
            };

            _unitOfWorkMock
                .Setup(x => x.StudentFees.FirstOrDefaultAsync(
                    It.IsAny<Expression<Func<StudentFee, bool>>>(),
                    true,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(fee);

            _unitOfWorkMock.Setup(x => x.BeginTransactionAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
            _unitOfWorkMock.Setup(x => x.CommitTransactionAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
            _unitOfWorkMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).Returns(Task.FromResult(1));
            _unitOfWorkMock.Setup(x => x.Payments.AddAsync(It.IsAny<Payment>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
          
            var payment = new Payment
            {
                StudentFeeId = 10,
                Amount = 2000
            };

            _mapperMock
                .Setup(x => x.Map<Payment>(request))
                .Returns(payment);

            _mapperMock
                .Setup(x => x.Map<PaymentResponse>(payment))
                .Returns(new PaymentResponse());

            // Act
            await _service.CreateAsync(request, CancellationToken.None);

            // Assert
            Assert.Equal(3000, fee.PaidAmount);
            Assert.Equal(StudentFeeStatus.Paid, fee.Status);
        }

        [Fact]
        public async Task UpdateAsync_ShouldThrowNotFound_WhenPaymentDoesNotExist()
        {
            // Arrange
            var request = new UpdatePaymentRequest 
            {
                Amount = 1000
            };

            _unitOfWorkMock
                .Setup(x => x.Payments.FirstOrDefaultAsync(
                    It.IsAny<Expression<Func<Payment, bool>>>(),
                    true,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync((Payment?)null);

            // Act
            var act = () => _service.UpdateAsync(1, request, CancellationToken.None);

            // Assert
            await Assert.ThrowsAsync<NotFoundException>(act);
        }

        [Fact]
        public async Task UpdateAsync_ShouldThrowNotFound_WhenStudentFeeDoesNotExist()
        {
            // Arrange
            var request = new UpdatePaymentRequest 
            {
                Amount = 1000 
            };

            var existingPayment = new Payment 
            {
                Id = 1,
                StudentFeeId = 10,
                Amount = 500 
            };

            _unitOfWorkMock
                .Setup(x => x.Payments.FirstOrDefaultAsync(
                    It.IsAny<Expression<Func<Payment, bool>>>(),
                    true,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(existingPayment);

            _unitOfWorkMock
                .Setup(x => x.StudentFees.FirstOrDefaultAsync(
                    It.IsAny<Expression<Func<StudentFee, bool>>>(),
                    true,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync((StudentFee?)null);

            // Act
            var act = () => _service.UpdateAsync(1, request, CancellationToken.None);

            // Assert
            await Assert.ThrowsAsync<NotFoundException>(act);
        }

        [Fact]
        public async Task UpdateAsync_ShouldThrowBadRequest_WhenPaymentIsCancelled()
        {
            // Arrange
            var request = new UpdatePaymentRequest { Amount = 1000 };

            var existingPayment = new Payment
            {
                Id = 1,
                StudentFeeId = 10,
                Amount = 500,
                Status = PaymentStatus.Cancelled
            };

            var existingFee = new StudentFee { Id = 10, TotalAmount = 3000, PaidAmount = 500 };

            _unitOfWorkMock
                .Setup(x => x.Payments.FirstOrDefaultAsync(
                    It.IsAny<Expression<Func<Payment, bool>>>(),
                    true,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(existingPayment);

            _unitOfWorkMock
                .Setup(x => x.StudentFees.FirstOrDefaultAsync(
                    It.IsAny<Expression<Func<StudentFee, bool>>>(),
                    true,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(existingFee);

            // Act
            var act = () => _service.UpdateAsync(1, request, CancellationToken.None);

            // Assert
            await Assert.ThrowsAsync<BadRequestException>(act);
        }

        [Fact]
        public async Task UpdateAsync_ShouldThrowBadRequest_WhenAmountExceedsRemaining()
        {
            // Arrange
            var request = new UpdatePaymentRequest
            {
                Amount = 2000
            };

            var payment = new Payment
            {
                Id = 1,
                StudentFeeId = 10,
                Amount = 500
            };

            var fee = new StudentFee
            {
                Id = 10,
                TotalAmount = 3000,
                PaidAmount = 2000,
                Status = StudentFeeStatus.PartiallyPaid
            };

            _unitOfWorkMock
                .Setup(x => x.Payments.FirstOrDefaultAsync(
                    It.IsAny<Expression<Func<Payment, bool>>>(),
                    true,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(payment);

            _unitOfWorkMock
                .Setup(x => x.StudentFees.FirstOrDefaultAsync(
                    It.IsAny<Expression<Func<StudentFee, bool>>>(),
                    true,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(fee);

            // Act
            var act = () => _service.UpdateAsync(1, request, CancellationToken.None);
            
            // Assert
            await Assert.ThrowsAsync<BadRequestException>(act);
        }


        [Fact]
        public async Task UpdateAsync_ShouldUpdatePaymentAndFeeStatus_WhenRequestIsValid()
        {
            // Arrange
            var request = new UpdatePaymentRequest
            {
                Amount = 3000,
                PaymentMethod = PaymentMethod.Cash,
                Notes = "Updated payment"
            };

            var existingPayment = new Payment
            {
                Id = 1,
                StudentFeeId = 10,
                Amount = 1000
            };

            var existingFee = new StudentFee
            {
                Id = 10,
                TotalAmount = 3000,
                PaidAmount = 1000,
                Status = StudentFeeStatus.PartiallyPaid
            };

            _unitOfWorkMock
                .Setup(x => x.Payments.FirstOrDefaultAsync(
                    It.IsAny<Expression<Func<Payment, bool>>>(),
                    true,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(existingPayment);

            _unitOfWorkMock
                .Setup(x => x.StudentFees.FirstOrDefaultAsync(
                    It.IsAny<Expression<Func<StudentFee, bool>>>(),
                    true,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(existingFee);

            _unitOfWorkMock.Setup(x => x.BeginTransactionAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
            _unitOfWorkMock.Setup(x => x.CommitTransactionAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
            _unitOfWorkMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).Returns(Task.FromResult(1));

            _mapperMock
                .Setup(x => x.Map(request, existingPayment))
                .Callback<UpdatePaymentRequest, Payment>((src, dest) =>
                {
                    dest.Amount = src.Amount;
                    dest.PaymentMethod = src.PaymentMethod;
                    dest.Notes = src.Notes;
                });

            _mapperMock
                .Setup(x => x.Map<PaymentResponse>(existingPayment))
                .Returns(new PaymentResponse { Id = 1, Amount = 3000 });

            // Act
            var result = await _service.UpdateAsync(1, request, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(3000, existingPayment.Amount);
            Assert.Equal(3000, existingFee.PaidAmount); 
            Assert.Equal(StudentFeeStatus.Paid, existingFee.Status); 

            
            _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
            _unitOfWorkMock.Verify(x => x.CommitTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task UpdateAsync_ShouldRollbackAndRethrow_WhenDbUpdateFails()
        {
            // Arrange
            var request = new UpdatePaymentRequest { Amount = 2000 };
            var existingPayment = new Payment { Id = 1, StudentFeeId = 10, Amount = 1000 };
            var existingFee = new StudentFee { Id = 10, TotalAmount = 3000, PaidAmount = 1000 };

            _unitOfWorkMock
                .Setup(x => x.Payments.FirstOrDefaultAsync(
                    It.IsAny<Expression<Func<Payment, bool>>>(),
                    true,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(existingPayment);

            _unitOfWorkMock
                .Setup(x => x.StudentFees.FirstOrDefaultAsync(
                    It.IsAny<Expression<Func<StudentFee, bool>>>(),
                    true,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(existingFee);

            _unitOfWorkMock
                .Setup(x => x.BeginTransactionAsync(It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            
            _unitOfWorkMock
                .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("Database connection failed"));

            // Act
            var act = () => _service.UpdateAsync(1, request, CancellationToken.None);

            // Assert
            await Assert.ThrowsAsync<InvalidOperationException>(act);

                
            _unitOfWorkMock.Verify(
                x => x.RollbackTransactionAsync(It.IsAny<CancellationToken>()),
                Times.Once);

          
            _unitOfWorkMock.Verify(
                x => x.CommitTransactionAsync(It.IsAny<CancellationToken>()),
                Times.Never);
        }

    }
}
