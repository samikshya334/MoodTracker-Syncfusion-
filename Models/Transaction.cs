using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Mental_Health_Traker.Models
{
    public class Transaction
    {
        [Key]
        public int TransactionId { get; set; }
        [Range(1,int.MaxValue,ErrorMessage ="Please select a category")]
        public int CategoryId { get; set; }
        public Category? Category { get; set; } 
        public float Percentage { get; set; }//it is navigation property for introducting the foreign key
        [Column(TypeName ="nvarchar(75)")]
        public string? Note { get; set; }   
        public DateTime Date{ get; set; } = DateTime.Now;
        [NotMapped ]
        public string? CategoryTitleWithIcon 
        {
            get
            {
                return Category==null ? " " : Category.Icon + " " + Category.Title;//yedi category null cha vanye empty string pathaucha else category ko icon ra title lei concatenate garera pathaucha..
            } 
        }
    }
}
