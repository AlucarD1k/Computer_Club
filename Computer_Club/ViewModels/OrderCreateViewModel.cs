using System.ComponentModel.DataAnnotations;

namespace Computer_Club.ViewModels;


    public class ProductOrderViewModel
    {
        public int    ProductId   { get; set; }
        public string Name        { get; set; }
        public decimal Price      { get; set; }
        
        [Range(0, int.MaxValue, ErrorMessage = "Quantity must be non‑negative")]
        public int    Quantity    { get; set; } = 0;
    }

    public class OrderCreateViewModel
    {
        [Required]
        [Display(Name = "User")]
        public int UserId { get; set; }

        public List<ProductOrderViewModel> Products { get; set; } = new List<ProductOrderViewModel>();
    }

    public class OrderEditViewModel
    {
        public int OrderId { get; set; }

        public List<ProductOrderViewModel> Products { get; set; }
    }



