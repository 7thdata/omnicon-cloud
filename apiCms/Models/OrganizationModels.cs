using clsCms.Models;

namespace apiCms.Models
{
    public class OrganizationModels
    {
    }

    public class GetOrganizationRequestModel
    {
        public string OrganizationId { get; set; }
    }

    public class GetOrganizationResponseModel
    {
        public OrganizationViewModel Organization { get; set; }
    }

    public class GetOrganizationsRequestModel
    {
        public string Keyword { get; set; }
        public string Sort { get; set; }
    }

    public class GetOrganizationsResponseModel
    {
        public List<OrganizationViewModel> Organizations { get; set; }
     
    }
}
