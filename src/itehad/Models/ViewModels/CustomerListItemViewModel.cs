namespace itehad.Models.ViewModels
{
    public class CustomerListItemViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Phone { get; set; }
        public decimal OutstandingILS { get; set; }
        public decimal OutstandingUSD { get; set; }
        public bool IsOverLimit { get; set; }
    }
}
