namespace Allors.Repository
{
    using System;

    using Allors.Repository.Attributes;

    #region Allors
    [Id("6b56e13b-d075-40f1-8e33-a9a4c6cadb96")]
    #endregion
    public partial class AccountingPeriod : Budget 
    {
        #region inherited properties
        public string Description { get; set; }

        public BudgetRevision[] BudgetRevisions { get; set; }

        public BudgetStatus[] BudgetStatuses { get; set; }

        public string BudgetNumber { get; set; }

        public BudgetObjectState CurrentObjectState { get; set; }

        public BudgetReview[] BudgetReviews { get; set; }

        public BudgetStatus CurrentBudgetStatus { get; set; }

        public BudgetItem[] BudgetItems { get; set; }

        public DateTime FromDate { get; set; }

        public DateTime ThroughDate { get; set; }

        public string Comment { get; set; }

        public Guid UniqueId { get; set; }

        public ObjectState PreviousObjectState { get; set; }

        public ObjectState LastObjectState { get; set; }

        public Permission[] DeniedPermissions { get; set; }

        public SecurityToken[] SecurityTokens { get; set; }

        #endregion

        #region Allors
        [Id("0fd97106-1e39-4629-a7bd-ad263bc2d296")]
        [AssociationId("816f8a0b-3c3a-4dd2-a50e-5c3cd197c592")]
        [RoleId("2d803ef5-2e9a-46fa-8690-5d5ef00f6785")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        public AccountingPeriod Parent { get; set; }

        #region Allors
        [Id("93b16073-8196-40c2-8777-5719fe1e6360")]
        [AssociationId("50a13e06-7df7-4d56-b498-5eea8415bb48")]
        [RoleId("e6b86e57-d8d2-41aa-b238-2fe027d74813")]
        #endregion
        [Required]
        public bool Active { get; set; }
        
        #region Allors
        [Id("babffef0-47ad-44ad-9a55-ffefb0fec783")]
        [AssociationId("b490215a-8185-40c8-bb31-087906d10911")]
        [RoleId("9fdaab7a-5e4a-4ec1-85bb-0610c0d0493b")]
        #endregion
        [Required]
        public int PeriodNumber { get; set; }
        
        #region Allors
        [Id("d776c4f4-9408-4083-8eb4-a4f940f6066f")]
        [AssociationId("8789a4bf-fd21-48d1-ae0b-26ebd100c0ea")]
        [RoleId("98fec7aa-6357-4b8e-baf6-0a8ef3d221dc")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Required]
        public TimeFrequency TimeFrequency { get; set; }
        
        #region inherited methods


        public void OnBuild(){}

        public void OnPostBuild(){}

        public void OnPreDerive(){}

        public void OnDerive(){}

        public void OnPostDerive(){}


        public void Close(){}

        public void Reopen(){}





        #endregion
    }
}