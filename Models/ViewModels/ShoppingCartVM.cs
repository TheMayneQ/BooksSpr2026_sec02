namespace BooksSpr2026_sec02.Models.ViewModels
{
    public class ShoppingCartVM
    {
        public IEnumerable<CartItem> CartItems { get; set; }

        public double OrderTotal { get; set; }



    }
}
