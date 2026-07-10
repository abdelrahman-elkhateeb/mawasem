namespace Mawasem.Domain.Catalog
{
    public class ProductGrade
    {
        public int ProductId { get; set; }

        public Product Product { get; set; } = null!;

        public int GradeId { get; set; }

        public Grade Grade { get; set; } = null!;
    }
}
