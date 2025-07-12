using System;
using api.DTO.Interfaces;
using api.Models;

namespace api.DTO.PaymentDTO;

public class BasePaymentDTO : IResponseData { }

public class CreatedPaymentDTO : BasePaymentDTO
{
    public decimal AmountNight { get; set; }
    public decimal Total { get; set; }
    public DateTime? PaymentDate { get; set; }
    public PaymentStatus Status { get; set; }
    public PaymentMethod Method { get; set; }
}
