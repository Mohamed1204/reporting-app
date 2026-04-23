using ReportingApi1.Data;
using ReportingApi1.Entities;

namespace ReportingApi1.Services
{
    //add missing interface
    public class SalesEntryService
    {
        private readonly VatReportingContext _vatReportingContext;
        public SalesEntryService(VatReportingContext vatReportingContext)
        {
            _vatReportingContext = vatReportingContext;
        }
        public async Task<SalesEntry?> DeleteByIdAsync(int id)
        {
            var entry = await _vatReportingContext.SalesEntries.FindAsync(id);
            if (entry == null) return null;
            _vatReportingContext.SalesEntries.Remove(entry);
            await _vatReportingContext.SaveChangesAsync();
            return entry;

        }
    }
}
