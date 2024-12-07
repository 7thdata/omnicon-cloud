using Azure.Data.Tables;
using Azure;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.Identity.Client;

namespace clsCms.Models
{
    /// <summary>
    /// Channel
    /// </summary>
    [Table("Channels")]
    public class ChannelModel
    {
        // Parameterless constructor required for Entity Framework
        public ChannelModel()
        {
        }

        public ChannelModel(string id, string permaName, string title, string createdBy)
        {
            Id = id;
            PermaName = permaName;
            Title = title;
            CreatedBy = createdBy;
        }

        [Key, MaxLength(36)]
        public string Id { get; set; }

        [Required]
        [RegularExpression(@"^[a-z0-9\-]+$", ErrorMessage = "PermaName must be lowercase, contain no spaces, and only include letters, numbers, and hyphens.")]
        public string PermaName { get; set; }

        [Required]
        public string Title { get; set; }

        public string? Description { get; set; }
        public bool IsArchived { get; set; }
        public DateTimeOffset Created { get; set; }
        public DateTimeOffset Updated { get; set; }
        public int ArticleCount { get; set; }
        public string CreatedBy { get; set; }

        public bool IsPublic { get; set; }
        public string? PublicCss { get; set; }
        [MaxLength(36)]
        public string OrganizationId { get; set; }
    }

    [Table("ChannelMemberships")]
    public class ChannelMembershipModel
    {
        public ChannelMembershipModel(string membershipId, string channelId, string userId, string invitedBy)
        {
            MembershipId = membershipId;
            ChannelId = channelId;
            UserId = userId;
            InvitedBy = invitedBy;
        }

        [Key, MaxLength(36)]
        public string MembershipId { get; set; } // Unique ID for the membership
        [Required, MaxLength(36)]
        public string ChannelId { get; set; } // The channel the user is a member of
        [Required, MaxLength(36)]
        public string UserId { get; set; } // The user ID
        public DateTimeOffset InvitedOn { get; set; } // Date when the user was invited
        [Required, MaxLength(36)]
        public string InvitedBy { get; set; }
        public DateTimeOffset? Accepted { get; set; }
        public bool IsAccepted { get; set; }
        public DateTimeOffset? Rejected { get; set; }
        public bool IsRejected { get; set; }
        public bool IsOwner { get; set; } // Indicates if the user is the owner of the channel
        public bool IsEditor { get; set; } // Indicates if the user is an editor of the channel
        public bool IsReviewer { get; set; } // Indicates if the user is a reviewer of the channel
        public bool IsArchived { get; set; } // Soft delete/archive flag
        public DateTimeOffset? Archived { get; set; } // Date when the membership was archived
    }

    public class ChannelViewModel
    {
        public ChannelViewModel(ChannelModel channel)
        {
            Channel = channel;
        }

        public ChannelModel Channel { get; set; } // The channel details
        public List<ChannelMembershipViewModel>? Members { get; set; }
        public List<AuthorModel>? Authors { get; set; }
    }

    public class ChannelMembershipViewModel
    {
        public ChannelMembershipViewModel(ChannelMembershipModel membership, ProfileViewModel user)
        {
            Membership = membership;
            User = user;
        }

        public ChannelMembershipModel Membership { get; set; }
        public ProfileViewModel User { get; set; }
    }
}
