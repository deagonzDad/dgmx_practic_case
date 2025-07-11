using System;
using System.Data.Common;
using api.Data;
using api.Exceptions;
using api.Models;
using api.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace api.Repository;

public class PaymentRepository(AppDbContext context) : IPaymentRepository
{
    private readonly AppDbContext _context = context;

    public async Task<(bool, Payment)> CreatePaymentAsync(Payment payment)
    {
        try
        {
            await _context.Payments.AddAsync(payment);
            // await _context.SaveChangesAsync();
            return (true, payment);
        }
        catch (DbUpdateException ex)
        {
            throw new UpdateException(ex);
        }
    }

    public async Task<Payment> UpdatePaymentStatusAsync(
        PaymentStatus paymentStatus,
        int paymentId,
        bool cancelledStatus
    )
    {
        try
        {
            Payment payment =
                await _context.Payments.FirstOrDefaultAsync(p => p.Id == paymentId)
                ?? throw new PaymentNotFoundException(null);
            bool isValidUpdate = ValidatePaymentStatus(
                payment.PaymentStatus,
                paymentStatus,
                cancelledStatus
            );

            if (isValidUpdate)
            {
                payment.PaymentStatus = paymentStatus;
                await _context.SaveChangesAsync();
            }
            return payment;
        }
        catch (DbException ex)
        {
            throw new UpdateException(ex);
        }
    }

    private static bool ValidatePaymentStatus(
        PaymentStatus paymentStatus,
        PaymentStatus newStatus,
        bool cancelledStatus
    )
    {
        if (!cancelledStatus)
        {
            return (int)newStatus > (int)paymentStatus
                && (int)paymentStatus <= (int)PaymentStatus.Failed;
        }
        else
        {
            return (int)newStatus > (int)PaymentStatus.Successful;
        }
    }
}
