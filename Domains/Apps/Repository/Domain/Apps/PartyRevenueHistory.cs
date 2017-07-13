namespace Allors.Repository
{
    using System;

    using Allors.Repository.Attributes;

    #region Allors
    [Id("16b7143f-d69e-402a-88af-405b5b88b1c9")]
    #endregion
    public partial class PartyRevenueHistory : AccessControlledObject 
    {
        #region inherited properties
        public Permission[] DeniedPermissions { get; set; }

        public SecurityToken[] SecurityTokens { get; set; }

        #endregion

        #region Allors
        [Id("00cc3ce5-d0d8-4082-a1af-8ee850bc76b3")]
        [AssociationId("dc4ec595-5121-4b69-bb2b-203d13ffdf75")]
        [RoleId("b2bff84f-9e85-4b2d-bcb5-3860378aa6a0")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]

        public Currency Currency { get; set; }
        #region Allors
        [Id("415b03ac-658e-4a1d-b137-ca45ddb89943")]
        [AssociationId("2c225add-0f0a-4269-9ea2-cb89b808050a")]
        [RoleId("e3030338-fd1f-4db2-8a94-39fc9b0597b8")]
        #endregion
        [Required]
        [Precision(19)]
        [Scale(2)]
        public decimal Revenue { get; set; }
        #region Allors
        [Id("63cba080-c950-4956-99ee-e380d482a272")]
        [AssociationId("fe3a4195-64ac-4ee7-9c27-07cb125489a5")]
        [RoleId("3dbf35cc-5e84-40b0-bac5-c388a8e9b241")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]

        public Party Party { get; set; }
        #region Allors
        [Id("c4610b89-4e21-4e03-a1d5-7487dce9ad42")]
        [AssociationId("0ebd138c-0a0f-4a6e-b792-0cc8e88a614e")]
        [RoleId("cacac289-9986-49a5-a0da-764af0dd5a9c")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]

        public InternalOrganisation InternalOrganisation { get; set; }


        #region inherited methods


        public void OnBuild(){}

        public void OnPostBuild(){}

        public void OnPreDerive(){}

        public void OnDerive(){}

        public void OnPostDerive(){}

        #endregion

    }
}