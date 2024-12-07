using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace clsCms.Models
{
    /// <summary>
    /// Represents a user in the system, extending the IdentityUser class with additional properties.
    /// </summary>
    public class UserModel : IdentityUser
    {
        /// <summary>
        /// Gets or sets the nickname of the user.
        /// </summary>
        [MaxLength(50)]
        public string NickName { get; set; }

        /// <summary>
        /// Gets or sets the URL or path to the user's icon image.
        /// </summary>
        [MaxLength(512)]
        public string IconImage { get; set; }

        /// <summary>
        /// Gets or sets an optional introduction or biography of the user.
        /// </summary>
        [MaxLength(512)]
        public string? Introduction { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the user's account is suspended.
        /// </summary>
        public bool IsSuspended { get; set; }

        /// <summary>
        /// Gets or sets the date and time when the user's account was suspended.
        /// </summary>
        public DateTimeOffset Suspended { get; set; }

        /// <summary>
        /// Gets or sets the date and time when the user's account was created.
        /// </summary>
        public DateTimeOffset CreatedOn { get; set; }

        /// <summary>
        /// Storage permaname for the user.
        /// </summary>
        [MaxLength(50)]
        public string? StorageSpace { get; set; }

        public string? OrganizationId { get; set; }
    }

    /// <summary>
    /// Represents an organization in the system.
    /// </summary>
    [Table("Organizations")]
    public class OrganizationModel
    {
        [Key, MaxLength(36)]
        public string? OrganizationId { get; set; }
        [MaxLength(50)]
        public string? OrganizationName { get; set; }
        [MaxLength(50)]
        public string? OrganizationType { get; set; } // Non-profit, for-profit, government, etc.
        [MaxLength(512)]
        public string? OrganizationDescription { get; set; }
        [MaxLength(512)]
        public string? OrganizationLogo { get; set; }
        [MaxLength(512)]
        public string? OrganizationWebsite { get; set; }
        [MaxLength(512)]
        public string? OrganizationEmail { get; set; }
        [MaxLength(512)]
        public string? OrganizationPhone { get; set; }
        [MaxLength(512)]
        public string? OrganizationAddress { get; set; }
        [MaxLength(50)]
        public string? OrganizationCity { get; set; }
        [MaxLength(50)]
        public string? OrganizationState { get; set; }
        [MaxLength(50)]
        public string? OrganizationCountry { get; set; }
        public DateTimeOffset Created { get; set; }
        public DateTimeOffset Updated { get; set; }
        public string CreatedBy { get; set; }
    }

    /// <summary>
    /// Membership mapping for the organizaiton.
    /// </summary>
    [Table("OrganizationMemberships")]
    public class OrganizationMemberModel
    {
        [Key, MaxLength(36)]
        public string MemberId { get; set; }
        [MaxLength(36)]
        public string OrganizationId { get; set; }
        [MaxLength(36)]
        public string UserId { get; set; }  
        [MaxLength(36)] 
        public string Role { get; set; }
        [MaxLength(36)] 
        public string Status { get; set; }  
        public DateTimeOffset Created { get; set; }
        public DateTimeOffset Updated { get; set; }
        public DateTimeOffset? Joined { get; set; }
    }

    public class OrganizationViewModel
    {
        public OrganizationModel Organization { get; set; }
        public List<OrganizationMemberModel> Members { get; set; }
    }

    /// <summary>
    /// ViewModel for managing users in the admin area.
    /// Includes the UserModel and the roles assigned to the user.
    /// </summary>
    public class UserViewModel
    {
        /// <summary>
        /// Gets or sets the UserModel representing the user.
        /// </summary>
        public UserModel User { get; set; }

        /// <summary>
        /// Gets or sets the list of roles associated with the user.
        /// </summary>
        public List<IdentityRole> Roles { get; set; }
    }

    /// <summary>
    /// Represents a simplified user profile view within the application, typically for public-facing user profiles.
    /// Includes basic user information like ID, username, email, nickname, profile image, and creation date.
    /// </summary>
    public class ProfileViewModel
    {
        /// <summary>
        /// Initializes a new instance of the ProfileViewModel class with the provided user details.
        /// </summary>
        /// <param name="id">The unique identifier for the user.</param>
        /// <param name="userName">The username of the user.</param>
        /// <param name="email">The email address of the user.</param>
        /// <param name="nickName">The nickname of the user.</param>
        /// <param name="iconImage">The URL or path to the user's profile image.</param>
        /// <param name="createdOn">The date and time when the user's profile was created.</param>
        public ProfileViewModel(string id, string userName, string email, string nickName, string iconImage, DateTimeOffset createdOn)
        {
            Id = id;
            UserName = userName;
            Email = email;
            NickName = nickName;
            IconImage = iconImage;
            CreatedOn = createdOn;
        }

        /// <summary>
        /// Gets or sets the unique identifier for the user.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the username of the user.
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// Gets or sets the email address of the user.
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// Gets or sets the nickname of the user.
        /// </summary>
        public string NickName { get; set; }

        /// <summary>
        /// Gets or sets the URL or path to the user's profile image.
        /// </summary>
        public string IconImage { get; set; }

        /// <summary>
        /// Gets or sets an optional introduction or biography of the user.
        /// </summary>
        public string? Introduction { get; set; }

        /// <summary>
        /// Gets or sets the date and time when the user's profile was created.
        /// </summary>
        public DateTimeOffset CreatedOn { get; set; }
    }

}
