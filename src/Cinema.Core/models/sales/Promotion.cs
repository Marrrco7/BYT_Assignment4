namespace Cinema.Core.models.sales
{

    public class Promotion
    {
        private DateTime ValidFrom { get; set; }
        private DateTime ValidTo { get; set; }
        private string Description { get; set; }
        private decimal DiscountAmount { get; set; }
        
        public Promotion(DateTime validFrom, DateTime validTo, string description, decimal discountAmount)
        {
            if (validFrom > validTo)
            {
                throw new ArgumentException("ValidFrom date cannot be after ValidTo date.");
            }

            ValidFrom = validFrom;
            ValidTo = validTo;
            Description = description;
            DiscountAmount = discountAmount;
        }
        
        public bool IsActive()
        {
            var today = DateTime.Today;
            return today >= ValidFrom.Date && today <= ValidTo.Date;
        }
    }
}