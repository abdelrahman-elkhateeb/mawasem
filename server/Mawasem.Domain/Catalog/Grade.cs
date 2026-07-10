using Mawasem.Domain.Common;
using Mawasem.Domain.Common.ValueObjects;

namespace Mawasem.Domain.Catalog;

public class Grade : BaseAuditableEntity
{
    public LocalizedText Name { get; set; } = new("" , "");

    public ICollection<ProductGrade> ProductGrades { get; set; } = new List<ProductGrade>();
}