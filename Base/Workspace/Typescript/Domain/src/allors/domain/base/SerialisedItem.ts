import { domain } from '../domain';
import { SerialisedItem } from '../generated/SerialisedItem.g';
import { Meta } from '../../meta/generated/domain.g';

declare module '../generated/SerialisedItem.g' {
  interface SerialisedItem {
    displayName: string;
    age: number;
    yearsToGo: number;
    goingConcern: number;
    marketValue: number;
    grossBookValue: number;
    expectedPosa: number;
  }
}

domain.extend((workspace) => {

  const m = workspace.metaPopulation as Meta;
  const obj = workspace.constructorByObjectType.get(m.SerialisedItem).prototype as any;

  Object.defineProperty(obj, 'displayName', {
    configurable: true,
    get(this: SerialisedItem) {

      return this.ItemNumber + ' ' + this.Name + ' SN: ' + this.SerialNumber;
    },
  });

  Object.defineProperty(obj, 'age', {
    configurable: true,
    get(this: SerialisedItem) {
      if (this.CanReadPurchasePrice && this.ManufacturingYear) {
        return new Date().getFullYear() - this.ManufacturingYear;
      } else {
        return 0;
      }
    },
  });

  Object.defineProperty(obj, 'yearsToGo', {
    configurable: true,
    get(this: SerialisedItem) {
      if (this.CanReadPurchasePrice && this.ManufacturingYear) {
        return this.LifeTime - this.age < 0 ? 0 : this.LifeTime - this.age;
      } else {
        return 0;
      }
    },
  });

  Object.defineProperty(obj, 'goingConcern', {
    configurable: true,
    get(this: SerialisedItem) {
      if (this.CanReadPurchasePrice) {
        return Math.round((parseFloat(this.ReplacementValue) * this.yearsToGo) / this.LifeTime);
      } else {
        return 0;
      }
    },
  });

  Object.defineProperty(obj, 'marketValue', {
    configurable: true,
    get(this: SerialisedItem) {
      if (this.CanReadPurchasePrice && this.ManufacturingYear) {
        return Math.round(parseFloat(this.ReplacementValue) * Math.exp(-2.045 * this.age / this.LifeTime));
      } else {
        return 0;
      }
    },
  });

  Object.defineProperty(obj, 'grossBookValue', {
    configurable: true,
    get(this: SerialisedItem) {
      if (this.CanReadPurchasePrice) {
        return Math.round(parseFloat(this.PurchasePrice) + parseFloat(this.RefurbishCost) + parseFloat(this.TransportCost));
      } else {
        return 0;
      }
    },
  });

  Object.defineProperty(obj, 'expectedPosa', {
    configurable: true,
    get(this: SerialisedItem) {
      if (this.CanReadPurchasePrice) {
        return parseFloat(this.ExpectedSalesPrice) - this.grossBookValue;
      } else {
        return 0;
      }
    },
  });

});
