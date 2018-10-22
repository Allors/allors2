// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Organisation.cs" company="Allors bvba">
//   Copyright 2002-2012 Allors bvba.
// Dual Licensed under
//   a) the General Public Licence v3 (GPL)
//   b) the Allors License
// The GPL License is included in the file gpl.txt.
// The Allors License is an addendum to your contract.
// Allors Applications is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// For more information visit http://www.allors.com/legal
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using System.Collections.Generic;

namespace Allors.Domain
{
    using System;

    public partial class Organisation
    {
        private bool IsDeletable => !this.ExistCurrentContacts;

        public void AppsOnBuild(ObjectOnBuild method)
        {
            if (!this.ExistLocale)
            {
                this.Locale = this.Strategy.Session.GetSingleton().DefaultLocale;
            }

            if  (!this.ExistInventoryStrategy)
            {
                this.InventoryStrategy = new InventoryStrategies(this.strategy.Session).Standard;
            }
        }

        public void AppsOnPreDerive(ObjectOnPreDerive method)
        {
            var derivation = method.Derivation;

            foreach (OrganisationContactRelationship contactRelationship in this.OrganisationContactRelationshipsWhereOrganisation)
            {
                derivation.AddDependency(this, contactRelationship);
            }
        }

        public void AppsOnDerive(ObjectOnDerive method)
        {
            var derivation = method.Derivation;
            var validation = derivation.Validation;

            if (this.IsInternalOrganisation)
            {
                if (!this.ExistRequestCounter)
                {
                    this.RequestCounter = new CounterBuilder(this.strategy.Session).Build();
                }

                if (!this.ExistQuoteCounter)
                {
                    this.QuoteCounter = new CounterBuilder(this.strategy.Session).Build();
                }

                if (!this.ExistRequestCounter)
                {
                    this.RequestCounter = new CounterBuilder(this.strategy.Session).Build();
                }

                if (!this.ExistPurchaseInvoiceCounter)
                {
                    this.PurchaseInvoiceCounter = new CounterBuilder(this.strategy.Session).Build();
                }

                if (!this.ExistPurchaseOrderCounter)
                {
                    this.PurchaseOrderCounter = new CounterBuilder(this.strategy.Session).Build();
                }

                if (!this.ExistSubAccountCounter)
                {
                    this.SubAccountCounter = new CounterBuilder(this.strategy.Session).Build();
                }

                if (!this.ExistIncomingShipmentCounter)
                {
                    this.IncomingShipmentCounter = new CounterBuilder(this.strategy.Session).Build();
                }

                if (!this.ExistWorkEffortCounter)
                {
                    this.WorkEffortCounter = new CounterBuilder(this.strategy.Session).Build();
                }

                if (!this.ExistInvoiceSequence)
                {
                    this.InvoiceSequence = new InvoiceSequenceBuilder(this.strategy.Session).Build();
                }

                if (this.DoAccounting && !this.ExistFiscalYearStartMonth)
                {
                    this.FiscalYearStartMonth = 1;
                }

                if (this.DoAccounting && !this.ExistFiscalYearStartDay)
                {
                    this.FiscalYearStartDay = 1;
                }
            }

            this.PartyName = this.Name;
            
            this.AppsOnDeriveCurrentContacts(derivation);
            this.AppsOnDeriveInactiveContacts(derivation);
            this.AppsOnDeriveCurrentOrganisationContactRelationships(derivation);
            this.AppsOnDeriveInactiveOrganisationContactRelationships(derivation);
            this.AppsOnDeriveCurrentPartyContactMechanisms(derivation);
            this.AppsOnDeriveInactivePartyContactMechanisms(derivation);
            this.AppsOnDeriverContactUserGroup(derivation);

            var deletePermission = new Permissions(this.strategy.Session).Get(this.Meta.ObjectType, this.Meta.Delete, Operations.Execute);
            if (this.IsDeletable)
            {
                this.RemoveDeniedPermission(deletePermission);
            }
            else
            {
                this.AddDeniedPermission(deletePermission);
            }
        }

        public List<string> Roles => new List<string>() { "Internal organisation" };

        public bool AppsIsActiveProfessionalServicesProvider(DateTime? date)
        {
            if (date == DateTime.MinValue)
            {
                return false;
            }

            var professionalServicesRelationships = this.ProfessionalServicesRelationshipsWhereProfessionalServicesProvider;
            foreach (ProfessionalServicesRelationship relationship in professionalServicesRelationships)
            {
                if (relationship.FromDate.Date <= date &&
                    (!relationship.ExistThroughDate || relationship.ThroughDate >= date))
                {
                    return true;
                }
            }

            return false;
        }

        public bool AppsIsActiveSubContractor(DateTime? date)
        {
            if (date == DateTime.MinValue)
            {
                return false;
            }

            var subContractorRelationships = this.SubContractorRelationshipsWhereSubContractor;
            foreach (SubContractorRelationship relationship in subContractorRelationships)
            {
                if (relationship.FromDate.Date <= date &&
                    (!relationship.ExistThroughDate || relationship.ThroughDate >= date))
                {
                    return true;
                }
            }

            return false;
        }

        public bool AppsIsActiveSupplier(DateTime? date)
        {
            if (date == DateTime.MinValue)
            {
                return false;
            }

            var supplierRelationships = this.SupplierRelationshipsWhereSupplier;
            foreach (SupplierRelationship relationship in supplierRelationships)
            {
                if (relationship.FromDate <= date &&
                    (!relationship.ExistThroughDate || relationship.ThroughDate >= date))
                {
                    return true;
                }
            }

            return false;
        }

        public void AppsOnDeriveCurrentContacts(IDerivation derivation)
        {
            this.RemoveCurrentContacts();

            var contactRelationships = this.OrganisationContactRelationshipsWhereOrganisation;
            foreach (OrganisationContactRelationship contactRelationship in contactRelationships)
            {
                if (contactRelationship.FromDate <= DateTime.UtcNow &&
                    (!contactRelationship.ExistThroughDate || contactRelationship.ThroughDate >= DateTime.UtcNow))
                {
                    this.AddCurrentContact(contactRelationship.Contact);
                }
            }
        }

        public void AppsOnDeriveInactiveContacts(IDerivation derivation)
        {
            this.RemoveInactiveContacts();

            var contactRelationships = this.OrganisationContactRelationshipsWhereOrganisation;
            foreach (OrganisationContactRelationship contactRelationship in contactRelationships)
            {
                if (contactRelationship.FromDate > DateTime.UtcNow ||
                    (contactRelationship.ExistThroughDate && contactRelationship.ThroughDate < DateTime.UtcNow))
                {
                    this.AddInactiveContact(contactRelationship.Contact);
                }
            }
        }

        public void AppsOnDeriveCurrentOrganisationContactRelationships(IDerivation derivation)
        {
            this.RemoveCurrentOrganisationContactRelationships();

            foreach (OrganisationContactRelationship organisationContactRelationship in this.OrganisationContactRelationshipsWhereOrganisation)
            {
                if (organisationContactRelationship.FromDate <= DateTime.UtcNow &&
                    (!organisationContactRelationship.ExistThroughDate || organisationContactRelationship.ThroughDate >= DateTime.UtcNow))
                {
                    this.AddCurrentOrganisationContactRelationship(organisationContactRelationship);
                }
            }
        }

        public void AppsOnDeriveInactiveOrganisationContactRelationships(IDerivation derivation)
        {
            this.RemoveInactiveOrganisationContactRelationships();

            foreach (OrganisationContactRelationship organisationContactRelationship in this.OrganisationContactRelationshipsWhereOrganisation)
            {
                if (organisationContactRelationship.FromDate > DateTime.UtcNow ||
                    (organisationContactRelationship.ExistThroughDate && organisationContactRelationship.ThroughDate < DateTime.UtcNow))
                {
                    this.AddInactiveOrganisationContactRelationship(organisationContactRelationship);
                }
            }
        }

        public void AppsOnDeriveInactivePartyContactMechanisms(IDerivation derivation)
        {
            this.RemoveInactivePartyContactMechanisms();

            foreach (PartyContactMechanism partyContactMechanism in this.PartyContactMechanisms)
            {
                if (partyContactMechanism.FromDate > DateTime.UtcNow ||
                    (partyContactMechanism.ExistThroughDate && partyContactMechanism.ThroughDate < DateTime.UtcNow))
                {
                    this.AddInactivePartyContactMechanism(partyContactMechanism);
                }
            }
        }

        public void AppsOnDeriverContactUserGroup(IDerivation derivation)
        {
            if (!this.ExistContactsUserGroup)
            {
                var customerContactGroupName = $"Customer contacts at {this.Name} ({this.UniqueId})";
                this.ContactsUserGroup = new UserGroupBuilder(this.strategy.Session).WithName(customerContactGroupName).Build();
            }

            this.ContactsUserGroup.Members = this.CurrentContacts.ToArray();
        }

        public void AppsDelete(DeletableDelete method)
        {
            if (this.IsDeletable)
            {
                foreach (PartyFinancialRelationship partyFinancialRelationship in this.PartyFinancialRelationshipsWhereParty)
                {
                    partyFinancialRelationship.Delete();
                }

                foreach (PartyContactMechanism partyContactMechanism in this.PartyContactMechanisms)
                {
                    partyContactMechanism.ContactMechanism.Delete();
                    partyContactMechanism.Delete();
                }

                foreach (OrganisationContactRelationship organisationContactRelationship in this.OrganisationContactRelationshipsWhereOrganisation)
                {
                    organisationContactRelationship.Contact.Delete();
                }
            }
        }
    }
}