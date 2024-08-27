using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;


namespace CourseApply3.Models
{
    public class CourseProperty
    {
        public class UserProperty
        {
            [Key]
            public int UsersId { get; set; }

            [Required(ErrorMessage = "Adı gerekli")]
            public string FirstName { get; set; } = string.Empty;

            [Required(ErrorMessage = "Soyadı gerekli")]
            public string LastName { get; set; } = string.Empty;

            [Required(ErrorMessage = "E-posta gerekli")]
            [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi girin")]
            public string Email { get; set; } = string.Empty;

            [Required(ErrorMessage = "Yaşı gerekli")]
            [Range(1, 120, ErrorMessage = "Geçerli bir yaş girin")]
            public int Age { get; set; } = 0;

            public DateTime AccessDate { get; set; } = DateTime.Now;

            public string? Password { get; set; }

            public byte[]? ProfilePictureData { get; set; }
            public string? ProfilePictureMimeType { get; set; }
            public virtual UserInformation? UserInformation { get; set; }
        }

        public class QuestionProperty
        {
            [Key]
            public int QuestionId { get; set; }

            public string Question { get; set; } = string.Empty;

            public string CorrectAnswer { get; set; } = string.Empty;
            public string AnswerA {  get; set; } = string.Empty;
            public string AnswerB { get; set; } = string.Empty;
            public string AnswerC { get; set; } = string.Empty;
            public string AnswerD { get; set; } = string.Empty;

            [ForeignKey("ExamId")]
            public int ExamId { get; set; }

            public virtual ExamProperty? Exam { get; set; }
        }

        public class ExamProperty
        {
            [Key]
            public int ExamId { get; set; }

            public string ExamName { get; set; } = string.Empty;

            public virtual ICollection<QuestionProperty> Questions { get; set; } = new List<QuestionProperty>();
            public virtual UserInformation? UserInformation { get; set; }
        }
        public class UserInformation
        {
            [Key]
            public int UserInformtionId { get; set; }
            [ForeignKey(nameof(UserId))]
            public int UserId { get; set; }
            [ForeignKey(nameof(ExamId))]
            public int ExamId { get; set; }
            public int Score { get; set; }

            public virtual UserProperty? User { get; set; } 
            public virtual ExamProperty? Exam { get; set; } 
        }
        public class AdminProperty
        {
            [Key]
            public int AdminId { get; set; }
            public string AdminEmail { get; set; } = string.Empty;
            public string AdminPassword { get; set; } = string.Empty;
        }
        public class AnswerProperty
        {
            [Key]
            public int AnswerPropertyId { get; set; }
            public int? QuestionId { get; set; }
            public string? Answer { get; set; } = string.Empty;



        }
        

    }
}