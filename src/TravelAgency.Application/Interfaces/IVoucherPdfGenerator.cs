using System;
using System.Collections.Generic;
using System.Text;
using TravelAgency.Domain.Entities;

public interface IVoucherPdfGenerator
{
    byte[] GenerateVoucherPdf(Booking booking, Tour tour);
}
