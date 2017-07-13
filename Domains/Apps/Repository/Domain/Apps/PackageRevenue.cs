namespace Allors.Repository
{
    using System;

    using Allors.Repository.Attributes;

    #region Allors
    [Id("8bc2d0a0-a371-4292-9fd6-ecb1db838107")]
    #endregion
    public partial class PackageRevenue : Deletable, AccessControlledObject 
    {
        #region inherited properties
        public Permission[] DeniedPermissions { get; set; }

        public SecurityToken[] SecurityTokens { get; set; }

        #endregion

        #region Allors
        [Id("1b941728-0511-45e3-8460-f49c6868ebaa")]
        [AssociationId("1d3aa935-9e18-4cb0-a7ad-e24447743bf3")]
        [RoleId("8d3b1810-f982-4ebd-b3ea-ea7647410c5f")]
        #endregion
        [Required]
        [Precision(19)]
        [Scale(2)]
        public decimal Revenue { get; set; }
        #region Allors
        [Id("2763fe2b-9d15-49a7-92c4-89994699cf05")]
        [AssociationId("fabbf131-374d-4f5d-801d-a449378b3ba8")]
        [RoleId("a8a3d6c2-7f15-42c7-a523-b3737b843e34")]
        #endregion
        [Required]

        public int Year { get; set; }
        #region Allors
        [Id("7674a37a-173d-4958-b73e-258773ef4277")]
        [AssociationId("39ed2c02-672e-42ba-ab9e-0b9b7b73be39")]
        [RoleId("f89627f5-2e66-4aed-9de1-3447dc281b15")]
        #endregion
        [Required]

        public int Month { get; set; }
        #region Allors
        [Id("b9acfd44-5228-4fcf-88c6-625110cb394e")]
        [AssociationId("0f7f8a0b-20b5-4fb8-abb4-cc727cab1337")]
        [RoleId("e6aa2dbf-0d8a-41d2-94ef-2268c847e1b7")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]

        public Currency Currency { get; set; }
        #region Allors
        [Id("c61a3dce-880f-403c-ad1a-bffabe59a57e")]
        [AssociationId("c9336998-8145-4a7d-95de-9d278487d205")]
        [RoleId("24266040-04dd-4388-a9cf-130ecac89094")]
        #endregion
        [Size(256)]

        public string PackageName { get; set; }
        #region Allors
        [Id("e6df8560-e85f-4e7a-a527-f7fb8f94cbb6")]
        [AssociationId("07b77c6b-09d0-4e4f-8cb4-69cbebc14e54")]
        [RoleId("17b95dec-dc59-4abc-817e-26eb0e9d5ac0")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]

        public InternalOrganisation InternalOrganisation { get; set; }
        #region Allors
        [Id("e952d6d6-d388-4e80-ba6b-8a612973aca6")]
        [AssociationId("a1b71548-06f0-40cd-a619-8b9c080dfe99")]
        [RoleId("c61b90e7-ea4d-4c7f-bfdb-ef87ba911c9d")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]

        public Package Package { get; set; }


        #region inherited methods


        public void OnBuild(){}

        public void OnPostBuild(){}

        public void OnPreDerive(){}

        public void OnDerive(){}

        public void OnPostDerive(){}


        public void Delete(){}

        #endregion

    }
}