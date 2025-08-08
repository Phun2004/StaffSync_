using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using iText.Kernel.Colors;
using iText.Layout.Borders;
using iText.Kernel.Font;
using iText.IO.Font.Constants;
using Demo.Models;

namespace Demo.Services
{
    public interface IPayslipPdfService
    {
        byte[] GeneratePayslipPdf(Payslip payslip);
    }

    public class PayslipPdfService : IPayslipPdfService
    {
        public byte[] GeneratePayslipPdf(Payslip payslip)
        {
            using var stream = new MemoryStream();
            var writer = new PdfWriter(stream);
            var pdf = new PdfDocument(writer);
            var document = new Document(pdf);

            // Company Header
            var logoTable = new Table(UnitValue.CreatePercentArray(new float[] { 1, 3 }))
                .SetWidth(UnitValue.CreatePercentValue(100))
                .SetMarginBottom(20);
            logoTable.AddCell(new Cell()
                .Add(new Paragraph("[COMPANY LOGO]"))
                .SetTextAlignment(TextAlignment.CENTER)
                .SetBorder(Border.NO_BORDER)
                .SetBackgroundColor(ColorConstants.LIGHT_GRAY)
                .SetHeight(60));
            var companyInfo = new Cell()
                .Add(new Paragraph("STAFFSYNC")
                    .SetFontSize(18)
                    .SetFont(PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD)))
                .Add(new Paragraph("Registration No: 123456-A"))
                .Add(new Paragraph("123 TARUMT Street, Kuantan"))
                .Add(new Paragraph("Tel: +60146233677 | Email: support@staffsync.com"))
                .SetBorder(Border.NO_BORDER)
                .SetPaddingLeft(20);
            logoTable.AddCell(companyInfo);
            document.Add(logoTable);

            // Payslip Title
            var titleTable = new Table(1)
                .SetWidth(UnitValue.CreatePercentValue(100))
                .SetMarginBottom(20);
            titleTable.AddCell(new Cell()
                .Add(new Paragraph("SALARY SLIP")
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetFontSize(16)
                    .SetFont(PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD)))
                .SetBackgroundColor(ColorConstants.LIGHT_GRAY)
                .SetPadding(10)
                .SetBorder(new SolidBorder(2)));
            document.Add(titleTable);

            // Employee Information
            var empInfoTable = new Table(UnitValue.CreatePercentArray(new float[] { 1, 2, 1, 2 }))
                .SetWidth(UnitValue.CreatePercentValue(100))
                .SetMarginBottom(20);
            empInfoTable.AddHeaderCell(new Cell(1, 4)
                .Add(new Paragraph("EMPLOYEE INFORMATION")
                    .SetFont(PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD)))
                .SetBackgroundColor(ColorConstants.GRAY)
                .SetFontColor(ColorConstants.WHITE)
                .SetTextAlignment(TextAlignment.CENTER)
                .SetPadding(8));
            AddTableRow(empInfoTable, "Employee ID:", payslip.EmployeeId, "Employee Name:", payslip.Employee?.Name ?? "N/A");
            AddTableRow(empInfoTable, "Department:", payslip.Employee?.Department?.Name ?? "N/A", "Position:", payslip.Employee?.Position?.Name ?? "N/A");
            AddTableRow(empInfoTable, "Period:", payslip.Period.ToString("MMMM yyyy"), "", "");
            document.Add(empInfoTable);

            // Earnings and Deductions
            var paymentTable = new Table(UnitValue.CreatePercentArray(new float[] { 3, 2, 3, 2 }))
                .SetWidth(UnitValue.CreatePercentValue(100))
                .SetMarginBottom(20);
            paymentTable.AddHeaderCell(new Cell(1, 2)
                .Add(new Paragraph("EARNINGS")
                    .SetFont(PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD)))
                .SetBackgroundColor(ColorConstants.GRAY)
                .SetFontColor(ColorConstants.WHITE)
                .SetTextAlignment(TextAlignment.CENTER)
                .SetPadding(8));
            paymentTable.AddHeaderCell(new Cell(1, 2)
                .Add(new Paragraph("DEDUCTIONS")
                    .SetFont(PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD)))
                .SetBackgroundColor(ColorConstants.GRAY)
                .SetFontColor(ColorConstants.WHITE)
                .SetTextAlignment(TextAlignment.CENTER)
                .SetPadding(8));
            AddPaymentRow(paymentTable, "Basic Salary", $"RM {payslip.BaseSalary:N2}", "EPF", $"RM {payslip.EPF:N2}");
            AddPaymentRow(paymentTable, "Overtime Pay", $"RM {payslip.OvertimePay:N2}", "SOCSO", $"RM {payslip.SOCSO:N2}");
            AddPaymentRow(paymentTable, "Bonus", $"RM {payslip.Bonus:N2}", "", "");
            AddPaymentRow(paymentTable, "Total Earnings", $"RM {(payslip.BaseSalary + payslip.OvertimePay + payslip.Bonus):N2}", "Total Deductions", $"RM {(payslip.EPF + payslip.SOCSO):N2}", true);
            document.Add(paymentTable);

            // Net Pay
            var netPayTable = new Table(UnitValue.CreatePercentArray(new float[] { 3, 2 }))
                .SetWidth(UnitValue.CreatePercentValue(100))
                .SetMarginBottom(20);
            netPayTable.AddCell(new Cell()
                .Add(new Paragraph("NET PAY")
                    .SetFont(PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD)))
                .SetTextAlignment(TextAlignment.LEFT)
                .SetPadding(8));
            netPayTable.AddCell(new Cell()
                .Add(new Paragraph($"RM {payslip.TotalPay:N2}")
                    .SetFont(PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD)))
                .SetTextAlignment(TextAlignment.RIGHT)
                .SetPadding(8));
            netPayTable.AddCell(new Cell()
                .Add(new Paragraph($"In Words: {ConvertToWords(payslip.TotalPay)} Ringgit Malaysia")
                    .SetFontSize(10))
                .SetTextAlignment(TextAlignment.LEFT)
                .SetPadding(8));
            netPayTable.AddCell(new Cell().SetBorder(Border.NO_BORDER));
            document.Add(netPayTable);

            // Footer Note
            document.Add(new Paragraph("This is a computer-generated payslip and does not require a signature.")
                .SetFontSize(10)
                .SetTextAlignment(TextAlignment.CENTER));

            document.Close();
            return stream.ToArray();
        }

        private void AddTableRow(Table table, string label1, string value1, string label2, string value2)
        {
            table.AddCell(new Cell()
                .Add(new Paragraph(label1)
                    .SetFont(PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD)))
                .SetBorder(Border.NO_BORDER)
                .SetPadding(5));
            table.AddCell(new Cell()
                .Add(new Paragraph(value1))
                .SetBorder(Border.NO_BORDER)
                .SetPadding(5));
            table.AddCell(new Cell()
                .Add(new Paragraph(label2)
                    .SetFont(PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD)))
                .SetBorder(Border.NO_BORDER)
                .SetPadding(5));
            table.AddCell(new Cell()
                .Add(new Paragraph(value2))
                .SetBorder(Border.NO_BORDER)
                .SetPadding(5));
        }

        private void AddPaymentRow(Table table, string earnings, string earningsAmount, string deductions, string deductionsAmount, bool isTotalRow = false)
        {
            var earningsCell = new Cell()
                .Add(new Paragraph(earnings))
                .SetPadding(5)
                .SetTextAlignment(TextAlignment.LEFT);
            var earningsAmountCell = new Cell()
                .Add(new Paragraph(earningsAmount))
                .SetPadding(5)
                .SetTextAlignment(TextAlignment.RIGHT);
            var deductionsCell = new Cell()
                .Add(new Paragraph(deductions))
                .SetPadding(5)
                .SetTextAlignment(TextAlignment.LEFT);
            var deductionsAmountCell = new Cell()
                .Add(new Paragraph(deductionsAmount))
                .SetPadding(5)
                .SetTextAlignment(TextAlignment.RIGHT);

            if (isTotalRow)
            {
                earningsCell.SetFont(PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD)).SetBorder(new SolidBorder(ColorConstants.BLACK, 2));
                earningsAmountCell.SetFont(PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD)).SetBorder(new SolidBorder(ColorConstants.BLACK, 2));
                deductionsCell.SetFont(PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD)).SetBorder(new SolidBorder(ColorConstants.BLACK, 2));
                deductionsAmountCell.SetFont(PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD)).SetBorder(new SolidBorder(ColorConstants.BLACK, 2));
            }

            table.AddCell(earningsCell);
            table.AddCell(earningsAmountCell);
            table.AddCell(deductionsCell);
            table.AddCell(deductionsAmountCell);
        }

        private string ConvertToWords(decimal amount)
        {
            var integerPart = (int)Math.Floor(amount);
            var decimalPart = (int)Math.Round((amount - integerPart) * 100);
            var words = ConvertIntegerToWords(integerPart);
            if (decimalPart > 0)
            {
                words += $" and {decimalPart:00}/100";
            }
            return words;
        }

        private string ConvertIntegerToWords(int number)
        {
            if (number == 0) return "Zero";

            string[] ones = { "", "One", "Two", "Three", "Four", "Five", "Six", "Seven", "Eight", "Nine" };
            string[] teens = { "Ten", "Eleven", "Twelve", "Thirteen", "Fourteen", "Fifteen", "Sixteen", "Seventeen", "Eighteen", "Nineteen" };
            string[] tens = { "", "", "Twenty", "Thirty", "Forty", "Fifty", "Sixty", "Seventy", "Eighty", "Ninety" };
            string[] thousands = { "", "Thousand", "Million", "Billion" };

            if (number < 0) return "Negative " + ConvertIntegerToWords(-number);

            string words = "";
            int groupIndex = 0;

            while (number > 0)
            {
                int group = number % 1000;
                if (group != 0)
                {
                    string groupWords = "";
                    int hundreds = group / 100;
                    if (hundreds > 0)
                    {
                        groupWords += ones[hundreds] + " Hundred ";
                    }
                    int remainder = group % 100;
                    if (remainder >= 20)
                    {
                        groupWords += tens[remainder / 10] + " ";
                        if (remainder % 10 > 0)
                        {
                            groupWords += ones[remainder % 10] + " ";
                        }
                    }
                    else if (remainder >= 10)
                    {
                        groupWords += teens[remainder - 10] + " ";
                    }
                    else if (remainder > 0)
                    {
                        groupWords += ones[remainder] + " ";
                    }
                    words = groupWords + thousands[groupIndex] + " " + words;
                }
                number /= 1000;
                groupIndex++;
            }
            return words.Trim();
        }
    }
}