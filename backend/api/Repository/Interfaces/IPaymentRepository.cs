using System;
using api.Models;

namespace api.Repository.Interfaces;

public interface IPaymentRepository
{
    Task<(bool, Payment)> CreatePaymentAsync(Payment payment);
    Task<Payment> UpdatePaymentStatusAsync(
        PaymentStatus paymentStatus,
        int paymentId,
        bool cancelledStatus
    );
}
